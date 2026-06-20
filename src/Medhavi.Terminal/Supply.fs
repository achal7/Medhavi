module Medhavi.Terminal.Supply

open Medhavi.Terminal
open Medhavi.Integration
open Medhavi.Supply
open Medhavi.Contracts.Integration
open Medhavi.SharedKernel.BoundedContexts
open System
open Medhavi.MasterData
open Medhavi.MasterData.Application
open Medhavi.Common.Patterns
open Medhavi.Supply.Application

let handleWorkOrdersCompleted
    (workOrdersCompleted: list<Medhavi.Contracts.WorkOrderCompletedPayload>)
    (supplyContext: SupplyContext)
    (masterDataContext: MasterData)
    (logger: IngestionLogger)
    (printer: Printer)
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
    (printer: Printer)
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
            // do! supplyContext.ResourceCalendar.DefineBulk(resourceCalendars)
            ()
        | WorkOrdersCompleted workOrdersCompleted ->
            handleWorkOrdersCompleted workOrdersCompleted supplyContext masterDataContext logger printer
            |> ignore
        | _ -> () // Ignore other event types
    }

let showData (supplyContext: SupplyContext) printer =
    let inventories = supplyContext.Queries.Inventory.GetAll().Result

    let invRows =
        inventories
        |> List.map (fun i ->
            [| i.Id
               i.SkuId
               i.StockingPointId
               i.Quantity.ToString()
               i.InTransitInbound.ToString()
               i.AvailableToPromise.ToString() |])
        |> List.toArray

    Printer.printTable
        printer
        "INVENTORIES IN DATABASE"
        [| "INVENTORY ID"
           "SKU ID"
           "STOCKING POINT ID"
           "ON-HAND QTY"
           "IN-TRANSIT"
           "ATP QTY" |]
        invRows

    // 9. Inventory Targets Table
    let targets =
        supplyContext.Queries.InventoryTarget
            .GetAll()
            .Result

    let targetRows =
        targets
        |> List.map (fun t ->
            let safetyStr =
                t.SafetyStockQty
                |> Option.map (fun q -> q.ToString())
                |> Option.defaultValue "0"

            let minStr =
                t.MinQty
                |> Option.map (fun q -> q.ToString())
                |> Option.defaultValue "None"

            let maxStr =
                t.MaxQty
                |> Option.map (fun q -> q.ToString())
                |> Option.defaultValue "None"

            [| t.Id; t.SkuId; t.StockingPointId; safetyStr; minStr; maxStr |])
        |> List.toArray

    Printer.printTable
        printer
        "INVENTORY TARGETS IN DATABASE"
        [| "TARGET ID"
           "SKU ID"
           "STOCKING POINT ID"
           "SAFETY STOCK"
           "MIN QTY"
           "MAX QTY" |]
        targetRows

    let offers =
        supplyContext.Queries.SupplierOffer
            .GetAll()
            .Result

    let offerRows =
        offers
        |> List.map (fun o ->
            let moqStr =
                o.Moq
                |> Option.map (fun q -> q.ToString())
                |> Option.defaultValue "None"

            let lotStr =
                o.LotSize
                |> Option.map (fun q -> q.ToString())
                |> Option.defaultValue "None"

            [| o.Id
               o.SupplierId
               o.SkuId
               moqStr
               lotStr
               (if o.IsActive then "Active" else "Inactive") |])
        |> List.toArray

    Printer.printTable
        printer
        "SUPPLIER OFFERS IN DATABASE"
        [| "OFFER ID"; "SUPPLIER ID"; "SKU ID"; "MOQ"; "LOT SIZE"; "STATUS" |]
        offerRows

    // 11. Supply Orders Table
    let orders = supplyContext.Queries.SupplyOrder.GetAll().Result

    let orderRows =
        orders
        |> List.map (fun o ->
            [| o.Id
               o.OrderType
               o.SkuId
               o.StockingPointId
               o.Quantity.ToString()
               o.State |])
        |> List.toArray

    Printer.printTable
        printer
        "SUPPLY ORDERS IN DATABASE"
        [| "ORDER ID"; "TYPE"; "SKU ID"; "STOCKING POINT ID"; "QTY"; "STATE" |]
        orderRows

    let reservationsList =
        supplyContext.Queries.MaterialReservation
            .GetAll()
            .Result

    let resvRows =
        reservationsList
        |> List.map (fun r ->
            [| r.Id
               r.SkuId
               r.StockingPointId
               r.Quantity.ToString()
               r.State
               r.RequiredDate.ToString("yyyy-MM-dd")
               r.ExpiryTime.ToString("yyyy-MM-dd HH:mm:ss") |])
        |> List.toArray

    Printer.printTable
        printer
        "MATERIAL RESERVATIONS IN DATABASE"
        [| "RESERVATION ID"
           "SKU ID"
           "STOCKING POINT ID"
           "QTY"
           "STATE"
           "REQUIRED DATE"
           "EXPIRY TIME" |]
        resvRows

    // 12. Live Material Availability ATP Snapshots & Projections
    printer.PrintLine Bold "\n================================================================================"
    printer.PrintLine Bold "                     LIVE MATERIAL AVAILABILITY SNAPSHOTS                       "
    printer.PrintLine Bold "================================================================================"

    let sampleProducts = [ "SKU-BIKE"; "SKU-FRAME"; "SKU-WHEEL" ]
    let stockingPoints = [ "SP-WAREHOUSE"; "SP-FACTORY" ]
    let now = DateTimeOffset.UtcNow

    let atpRows =
        [ for p in sampleProducts do
              for sp in stockingPoints do
                  let snapRes =
                      MaterialProvider.getSnapshot supplyContext p sp now
                      |> Async.RunSynchronously

                  match snapRes with
                  | Ok snap ->
                      let net = MaterialProvider.calculateNetAvailable snap
                      let totalInbound = snap.Inbound |> List.sumBy snd
                      let totalReservations = snap.Reservations |> List.sumBy snd

                      yield
                          [| p
                             sp
                             snap.OnHand.ToString()
                             totalInbound.ToString()
                             snap.Safety.ToString()
                             totalReservations.ToString()
                             net.ToString() |]
                  | Error _ -> () ]
        |> List.toArray

    Printer.printTable
        printer
        "MATERIAL AVAILABILITY (ATP) SUMMARY"
        [| "SKU ID"
           "STOCKING POINT"
           "ON-HAND QTY"
           "INBOUND QTY"
           "SAFETY QTY"
           "RESERVATIONS"
           "NET AVAILABLE (ATP)" |]
        atpRows

    // 13. Time-Phased Availability Projections
    printer.PrintLine Bold "\n================================================================================"
    printer.PrintLine Bold "             TIME-PHASED AVAILABILITY PROJECTIONS (90-DAY HORIZON)               "
    printer.PrintLine Bold "================================================================================"

    let timePhasedRows =
        [ for p in [ "SKU-BIKE"; "SKU-FRAME" ] do
              let sp =
                  if p = "SKU-BIKE" then
                      "SP-WAREHOUSE"
                  else
                      "SP-FACTORY"

              let tpRes =
                  MaterialProvider.getTimePhasedAvailability supplyContext p sp now 10 90
                  |> Async.RunSynchronously

              match tpRes with
              | Ok list ->
                  for (date, qty) in list do
                      yield [| p; sp; date.ToString("yyyy-MM-dd"); qty.ToString() |]
              | Error _ -> () ]
        |> List.toArray

    Printer.printTable
        printer
        "TIME-PHASED NET AVAILABILITY BUCKETS (10-DAY BUCKETS)"
        [| "SKU ID"; "STOCKING POINT"; "BUCKET START DATE"; "NET AVAILABLE" |]
        timePhasedRows

    // 13.5 Daily Date-Wise Step-Curve Projections
    printer.PrintLine Bold "\n================================================================================"
    printer.PrintLine Bold "             DAILY DATE-WISE STEP-CURVE AVAILABILITY PROJECTIONS                "
    printer.PrintLine Bold "================================================================================"

    let dailyRows =
        [ for p in [ "SKU-BIKE"; "SKU-FRAME" ] do
              let sp =
                  if p = "SKU-BIKE" then
                      "SP-WAREHOUSE"
                  else
                      "SP-FACTORY"

              let dailyRes =
                  MaterialProvider.getDateWiseAvailability supplyContext p sp now 90
                  |> Async.RunSynchronously

              match dailyRes with
              | Ok list ->
                  for (date, qty) in list do
                      yield [| p; sp; date.ToString("yyyy-MM-dd"); qty.ToString() |]
              | Error _ -> () ]
        |> List.toArray

    Printer.printTable
        printer
        "DAILY STEP-CURVE NET AVAILABILITY"
        [| "SKU ID"; "STOCKING POINT"; "DATE"; "NET AVAILABLE" |]
        dailyRows
