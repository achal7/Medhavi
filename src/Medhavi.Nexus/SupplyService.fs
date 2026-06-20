module Medhavi.Nexus.SupplyService

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common.Patterns
open Medhavi.Contracts
open Medhavi.Contracts.Supply
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Integration
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.MasterData
open Medhavi.Supply
open Medhavi.SharedKernel

type Service =
    { Context: SupplyContext
      IntegrationHandler: SubscriptionHandle }

let handleWorkOrdersCompleted
    (workOrdersCompleted: list<WorkOrderCompletedPayload>)
    (supplyContext: SupplyContext)
    (masterDataContext: MasterDataContext)
    (logger: IngestionLogger)
    : Task<unit> =
    task {
        for item in workOrdersCompleted do
            let! existingOpt = supplyContext.Queries.SupplyOrder.GetById item.WorkOrderId

            match existingOpt with
            | None ->
                logger.LogError $"    - Work Order Ingestion Error: Work Order %s{item.WorkOrderId} not found"
            | Some order ->
                if order.State.Equals("Completed", StringComparison.OrdinalIgnoreCase) then
                    logger.LogInfo $"    - Work Order %s{item.WorkOrderId} already Completed. Skipping (Idempotent)."
                else
                    // 1. Complete the work order (passing the reported scrap)
                    let completeReq: SupplyOrderCompleteReq =
                        { Id = item.WorkOrderId
                          ScrapQuantity = item.QuantityScrapped
                          CompletedDate = item.CompletedAtUtc
                          FeedbackId = None }

                    let! completeRes = supplyContext.Commands.SupplyOrder.Complete completeReq

                    match completeRes with
                    | Error err ->
                        logger.LogError $"    - Failed to complete Work Order %s{item.WorkOrderId}: %A{err}"
                    | Ok _ ->
                        logger.LogSuccess $"    - Work Order %s{item.WorkOrderId} Completed successfully [ OK ]"

                        // 2. Increase Finished Goods inventory by QuantityCompleted
                        let! allInvs = supplyContext.Queries.Inventory.GetAll()

                        let fgInvOpt =
                            allInvs
                            |> List.tryFind(fun inv ->
                                inv.SkuId.Equals(order.SkuId, StringComparison.OrdinalIgnoreCase)
                                && inv.StockingPointId.Equals(
                                    order.StockingPointId,
                                    StringComparison.OrdinalIgnoreCase
                                ))

                        match fgInvOpt with
                        | Some fgInv ->
                            let newQty = fgInv.Quantity + item.QuantityCompleted

                            let! fgRes =
                                supplyContext.Commands.Inventory.Define
                                    { Id = fgInv.Id
                                      SkuId = fgInv.SkuId
                                      StockingPointId = fgInv.StockingPointId
                                      Quantity = newQty
                                      UnitOfMeasure = fgInv.UnitOfMeasure }

                            match fgRes with
                            | Ok _ ->
                                logger.LogSuccess(
                                    sprintf
                                        "      -> Finished Goods Inventory increased for %s to %M"
                                        order.SkuId
                                        newQty
                                )
                            | Error e -> logger.LogError(sprintf "      -> FG Inv update failed: %A" e)
                        | None ->
                            let fgId = $"INV-{order.SkuId}-{order.StockingPointId}"

                            let! fgRes =
                                supplyContext.Commands.Inventory.Define
                                    { Id = fgId
                                      SkuId = order.SkuId
                                      StockingPointId = order.StockingPointId
                                      Quantity = item.QuantityCompleted
                                      UnitOfMeasure = "UOM-PCS" }

                            match fgRes with
                            | Ok _ ->
                                logger.LogSuccess(
                                    sprintf
                                        "      -> Finished Goods Inventory created for %s with %M"
                                        order.SkuId
                                        item.QuantityCompleted
                                )
                            | Error e -> logger.LogError(sprintf "      -> FG Inv create failed: %A" e)

                        // 3. Deduct BOM components inventory (Backflushing)
                        let! boms = masterDataContext.Queries.Bom.GetAll()

                        let skuBomOpt =
                            boms
                            |> List.tryFind(fun b ->
                                b.SkuId.Equals(order.SkuId, StringComparison.OrdinalIgnoreCase) && b.Status)

                        match skuBomOpt with
                        | None -> ()
                        | Some bom ->
                            for bomItem in bom.Items do
                                let consumedQty = bomItem.Quantity * (item.QuantityCompleted + item.QuantityScrapped)

                                let compInvOpt =
                                    allInvs
                                    |> List.tryFind(fun inv ->
                                        inv.SkuId.Equals(bomItem.ComponentSkuId, StringComparison.OrdinalIgnoreCase)
                                        && inv.StockingPointId.Equals(
                                            order.StockingPointId,
                                            StringComparison.OrdinalIgnoreCase
                                        ))

                                match compInvOpt with
                                | Some compInv ->
                                    let newCompQty = compInv.Quantity - consumedQty

                                    let! compRes =
                                        supplyContext.Commands.Inventory.Define
                                            { Id = compInv.Id
                                              SkuId = compInv.SkuId
                                              StockingPointId = compInv.StockingPointId
                                              Quantity = newCompQty
                                              UnitOfMeasure = compInv.UnitOfMeasure }

                                    match compRes with
                                    | Ok _ ->
                                        logger.LogSuccess $"      -> Component Stock reduced: Component=%s{bomItem.ComponentSkuId}, consumed=%M{consumedQty}, newBalance=%M{newCompQty}"
                                    | Error e ->
                                        logger.LogError $"      -> Component Stock reduction failed: %A{e}"
                                | None ->
                                    let compId = $"INV-{bomItem.ComponentSkuId}-{order.StockingPointId}"

                                    let! compRes =
                                        supplyContext.Commands.Inventory.Define
                                            { Id = compId
                                              SkuId = bomItem.ComponentSkuId
                                              StockingPointId = order.StockingPointId
                                              Quantity = -consumedQty
                                              UnitOfMeasure = "UOM-PCS" }

                                    match compRes with
                                    | Ok _ ->
                                        logger.LogSuccess
                                            $"      -> Component Stock created with negative balance: Component=%s{bomItem.ComponentSkuId}, consumed=%M{consumedQty}"
                                    | Error e -> logger.LogError $"      -> Component Stock create failed: %A{e}"
    }

let mapApi (tr: TaskResult<'T, ApiError>) : TaskResult<'T, ApplicationError> =
    tr |> TaskResult.mapError(fun err -> ApplicationError.External(err.Code, err.Message, Map.empty))

let handler
    (supplyContext: SupplyContext)
    (masterDataContext: MasterDataContext)
    (event: IntegrationEvent)
    : TaskResult<unit, ApplicationError> =
    taskResult {
        let logger =
            { LogInfo = fun m -> printfn $"[Supply] %s{m}"
              LogSuccess = fun m -> printfn $"[Supply OK] %s{m}"
              LogError = fun m -> printfn $"[Supply ERR] %s{m}" }

        match event with
        | SupplyOffersImported supplyOffers ->
            let! (_: SupplierOffer list) = mapApi (supplyContext.Commands.SupplierOffer.DefineBulk(supplyOffers))
            return ()
        | InventoryPositionsImported inventoryPositions ->
            let! (items: Inventory list) = mapApi (supplyContext.Commands.Inventory.DefineBulk(inventoryPositions))
            let () =
                items
                |> List.iter (fun item ->
                    logger.LogSuccess $"    - Inventory Position Updated: Product=%s{item.SkuId}, SP=%s{item.StockingPointId}, Qty=%M{item.Quantity} [ OK ]")
            return ()
        | InventoryTargetsImported inventoryTargets ->
            let! (_: InventoryTarget list) = mapApi (supplyContext.Commands.InventoryTarget.DefineBulk(inventoryTargets))
            return ()
        | SupplyOrdersImported supplyOrders ->
            let! (_: SupplyOrder list) = mapApi (supplyContext.Commands.SupplyOrder.ProcessStatusUpdates(supplyOrders))
            return ()
        | MaterialReservationsImported reservations ->
            let! (items: MaterialReservation list) =
                reservations
                |> List.map supplyContext.Commands.MaterialReservation.CreateTentative
                |> TaskResult.sequence
                |> mapApi

            let () =
                items
                |> List.iter (fun item ->
                    logger.LogSuccess $"    - Material Reservation Created: Id=%s{item.Id}, Sku=%s{item.SkuId}, Qty=%M{item.Quantity} [ OK ]")
            return ()
        | ResourceCalendarsImported _ ->
            return ()
        | WorkOrdersCompleted workOrdersCompleted ->
            let! () = handleWorkOrdersCompleted workOrdersCompleted supplyContext masterDataContext logger
            return ()
        | _ -> return ()
    }

let create
    (integrationStore: EnvelopeStoreOps)
    (extractEnvelope: IntegrationService.ExtractEnvelope)
    (masterDataContext: MasterDataContext)
    : TaskResult<Service, ApplicationError> =
    taskResult {
        let context = BoundedContext.create()

        let handleEvents (envelopedEvent: EnvelopedEvent) : Task<unit> =
            task {
                match extractEnvelope envelopedEvent with
                | Error e -> printfn $"[ Supply ] Error while deserializing envelope: {e.ToString()}"
                | Ok event ->
                    let! res = handler context masterDataContext event

                    match res with
                    | Ok() -> ()
                    | Error err -> printfn $"[ Supply ] Error while processing event: Code={err.Code}, Message={err.Message} ({err.ToString()})"
            }

        let! integrationHandler =
            integrationStore.Subscribe SubscriptionMode.All None handleEvents CancellationToken.None
            |> TaskResult.mapError(fun e -> ApplicationError.Unknown $"{e.ToString()}")

        return
            { Context = context
              IntegrationHandler = integrationHandler }
    }
