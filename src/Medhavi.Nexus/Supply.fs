namespace Medhavi.Nexus

open Medhavi.Integration
open Medhavi.Supply
open Medhavi.Contracts.Integration
open Medhavi.SharedKernel.BoundedContexts
open System
open Medhavi.MasterData
open Medhavi.MasterData.Application
open Medhavi.Common.Patterns
open Medhavi.Supply.Application
open Medhavi.SharedKernel

module Supply =

    let handleWorkOrdersCompleted
        (workOrdersCompleted: list<Medhavi.Contracts.WorkOrderCompletedPayload>)
        (supplyContext: SupplyContext)
        (masterDataContext: MasterData)
        (logger: IngestionLogger)
        =
        task {
            for item in workOrdersCompleted do
                let! existingOpt = supplyContext.Queries.SupplyOrder.GetById item.WorkOrderId

                match existingOpt with
                | None ->
                    logger.LogError(sprintf "    - Work Order Ingestion Error: Work Order %s not found" item.WorkOrderId)
                | Some order ->
                    if order.State.Equals("Completed", StringComparison.OrdinalIgnoreCase) then
                        logger.LogInfo(
                            sprintf "    - Work Order %s already Completed. Skipping (Idempotent)." item.WorkOrderId
                        )
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
                            logger.LogError(sprintf "    - Failed to complete Work Order %s: %A" item.WorkOrderId err)
                        | Ok _ ->
                            logger.LogSuccess(sprintf "    - Work Order %s Completed successfully [ OK ]" item.WorkOrderId)

                            // 2. Increase Finished Goods inventory by QuantityCompleted
                            let! allInvs = supplyContext.Queries.Inventory.GetAll()

                            let fgInvOpt =
                                allInvs
                                |> List.tryFind (fun inv ->
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
                                |> List.tryFind (fun b ->
                                    b.SkuId.Equals(order.SkuId, StringComparison.OrdinalIgnoreCase)
                                    && b.Status)

                            match skuBomOpt with
                            | None -> ()
                            | Some bom ->
                                for bomItem in bom.Items do
                                    let consumedQty =
                                        bomItem.Quantity
                                        * (item.QuantityCompleted + item.QuantityScrapped)

                                    let compInvOpt =
                                        allInvs
                                        |> List.tryFind (fun inv ->
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
                                            logger.LogSuccess(
                                                sprintf
                                                    "      -> Component Stock reduced: Component=%s, consumed=%M, newBalance=%M"
                                                    bomItem.ComponentSkuId
                                                    consumedQty
                                                    newCompQty
                                            )
                                        | Error e ->
                                            logger.LogError(sprintf "      -> Component Stock reduction failed: %A" e)
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
                                            logger.LogSuccess(
                                                sprintf
                                                    "      -> Component Stock created with negative balance: Component=%s, consumed=%M"
                                                    bomItem.ComponentSkuId
                                                    consumedQty
                                            )
                                        | Error e -> logger.LogError(sprintf "      -> Component Stock create failed: %A" e)
        }

    let handleRequest
        (supplyContext: SupplyContext)
        (masterDataContext: MasterData)
        (logger: IngestionLogger)
        (req: Medhavi.Integration.IntegrationEvent)
        =
        task {
            match req with
            | SupplyOffersImported supplyOffers ->
                let! _ = supplyContext.Commands.SupplierOffer.DefineBulk(supplyOffers)
                ()
            | InventoryPositionsImported inventoryPositions ->
                let! res = supplyContext.Commands.Inventory.DefineBulk(inventoryPositions)

                match res with
                | Ok items ->
                    for item in items do
                        logger.LogSuccess(
                            sprintf
                                "    - Inventory Position Updated: Product=%s, SP=%s, Qty=%M [ OK ]"
                                item.SkuId
                                item.StockingPointId
                                item.Quantity
                        )
                | Error err -> logger.LogError(sprintf "    - Inventory Ingestion Error: %A" err)

                ()
            | InventoryTargetsImported inventoryTargets ->
                let! _ = supplyContext.Commands.InventoryTarget.DefineBulk(inventoryTargets)
                ()
            | SupplyOrdersImported supplyOrders ->
                let! _ = supplyContext.Commands.SupplyOrder.ProcessStatusUpdates(supplyOrders)
                ()
            | MaterialReservationsImported reservations ->
                let! res =
                    reservations
                    |> List.map supplyContext.Commands.MaterialReservation.CreateTentative
                    |> TaskResult.sequence

                match res with
                | Ok items ->
                    for item in items do
                        logger.LogSuccess(
                            sprintf
                                "    - Material Reservation Created: Id=%s, Sku=%s, Qty=%M [ OK ]"
                                item.Id
                                item.SkuId
                                item.Quantity
                        )
                | Error err -> logger.LogError(sprintf "    - Reservation Ingestion Error: %A" err)

                ()
            | ResourceCalendarsImported resourceCalendars ->
                ()
            | WorkOrdersCompleted workOrdersCompleted ->
                handleWorkOrdersCompleted workOrdersCompleted supplyContext masterDataContext logger
                |> ignore
            | _ -> ()
        }
