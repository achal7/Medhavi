namespace Medhavi.Supply.Application

open System
open System.Threading.Tasks
open Medhavi.Contracts
open Medhavi.Contracts.Integration
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.Supply

module SupplyIntegrationHandler =

    let handleInventoryPositions
        (caps: Supply)
        (payload: InventoryPositionPayload list)
        (logger: IngestionLogger)
        : Task<unit> =
        task {
            logger.LogInfo "\n  [Supply BC] Processing Inventory Positions..."
            for item in payload do
                let req : InventoryDefineReq = {
                    Id = $"INV-{item.ProductId}-{item.StockingPointId}"
                    SkuId = item.ProductId
                    StockingPointId = item.StockingPointId
                    Quantity = item.Quantity
                    UnitOfMeasure = "UOM-PCS"
                }
                let! res = caps.Inventory.Define req
                match res with
                | Ok _ -> logger.LogSuccess(sprintf "    - Inventory Position Updated: Product=%s, SP=%s, Qty=%M [ OK ]" item.ProductId item.StockingPointId item.Quantity)
                | Error err -> logger.LogError(sprintf "    - Inventory Ingestion Error: Product=%s, SP=%s [ ERR: %A ]" item.ProductId item.StockingPointId err)
        }

    let handleSupplyOrders
        (caps: Supply)
        (payload: SupplyOrderStatusPayload list)
        (logger: IngestionLogger)
        : Task<unit> =
        task {
            logger.LogInfo "\n  [Supply BC] Processing Supply Orders..."
            for item in payload do
                // 1. Check if the order already exists
                let! existingOpt = caps.Queries.SupplyOrder.GetById item.SupplyOrderId
                
                let mutable isCreated = false
                match existingOpt with
                | None ->
                    // Create new order
                    let orderType =
                        if item.SupplyOrderId.StartsWith("WO", StringComparison.OrdinalIgnoreCase) then "WorkOrder"
                        elif item.SupplyOrderId.StartsWith("TO", StringComparison.OrdinalIgnoreCase) then "TransportOrder"
                        else "PurchaseOrder"
                    
                    let createReq : SupplyOrderCreateReq = {
                        Id = item.SupplyOrderId
                        OrderType = orderType
                        SkuId = item.ProductId
                        StockingPointId = item.StockingPointId
                        Quantity = item.Quantity
                        UnitOfMeasure = "UOM-PCS"
                        RoutingId = None
                        SupplierId = None
                        IsFirm = (item.Status.Equals("Firm", StringComparison.OrdinalIgnoreCase) || item.Status.Equals("InTransit", StringComparison.OrdinalIgnoreCase))
                        IsExpedited = false
                        IsLocked = false
                        UsesLeadTimeQuantity = false
                        RequiredDeliveryDate = Some item.ExpectedDeliveryUtc
                        CreatedDate = DateTimeOffset.UtcNow
                    }
                    let! createRes = caps.SupplyOrder.Create createReq
                    match createRes with
                    | Ok _ -> 
                        logger.LogSuccess(sprintf "    - Supply Order Created: %s (Type=%s, Qty=%M) [ OK ]" item.SupplyOrderId orderType item.Quantity)
                        isCreated <- true
                    | Error err -> 
                        logger.LogError(sprintf "    - Supply Order Creation Failed: %s [ ERR: %A ]" item.SupplyOrderId err)
                | Some _ -> ()

                // 2. Perform State Transitions
                let! currentOpt = caps.Queries.SupplyOrder.GetById item.SupplyOrderId
                match currentOpt with
                | Some current ->
                    let normalizedStatus = item.Status.Trim().ToLowerInvariant()
                    let currentStatus = current.State.Trim().ToLowerInvariant()

                    if normalizedStatus = "inprogress" || normalizedStatus = "intransit" then
                        if currentStatus <> "inprogress" then
                            let! transitionRes = caps.SupplyOrder.Start { Id = item.SupplyOrderId; StartedDate = DateTimeOffset.UtcNow }
                            match transitionRes with
                            | Ok _ -> logger.LogSuccess(sprintf "      -> Order %s transitioned to InProgress" item.SupplyOrderId)
                            | Error err -> logger.LogError(sprintf "      -> Transition failed for %s to InProgress: %A" item.SupplyOrderId err)
                    elif normalizedStatus = "completed" || normalizedStatus = "received" then
                        if currentStatus <> "completed" then
                            let! transitionRes = caps.SupplyOrder.Complete { Id = item.SupplyOrderId; CompletedDate = DateTimeOffset.UtcNow }
                            match transitionRes with
                            | Ok _ -> logger.LogSuccess(sprintf "      -> Order %s transitioned to Completed" item.SupplyOrderId)
                            | Error err -> logger.LogError(sprintf "      -> Transition failed for %s to Completed: %A" item.SupplyOrderId err)
                    elif normalizedStatus = "cancelled" then
                        if currentStatus <> "cancelled" then
                            let! transitionRes = caps.SupplyOrder.Cancel { Id = item.SupplyOrderId; CancelledDate = DateTimeOffset.UtcNow }
                            match transitionRes with
                            | Ok _ -> logger.LogSuccess(sprintf "      -> Order %s transitioned to Cancelled" item.SupplyOrderId)
                            | Error err -> logger.LogError(sprintf "      -> Transition failed for %s to Cancelled: %A" item.SupplyOrderId err)
                    elif normalizedStatus = "firm" || normalizedStatus = "confirmed" then
                        if currentStatus <> "confirmed" then
                            let! transitionRes = caps.SupplyOrder.Confirm { Id = item.SupplyOrderId; ConfirmedDate = DateTimeOffset.UtcNow }
                            match transitionRes with
                            | Ok _ -> logger.LogSuccess(sprintf "      -> Order %s transitioned to Confirmed" item.SupplyOrderId)
                            | Error err -> logger.LogError(sprintf "      -> Transition failed for %s to Confirmed: %A" item.SupplyOrderId err)
                    elif normalizedStatus = "planned" then
                        if currentStatus <> "planned" then
                            let! transitionRes = caps.SupplyOrder.Plan { Id = item.SupplyOrderId; PlannedDeliveryDate = DateTimeOffset.UtcNow }
                            match transitionRes with
                            | Ok _ -> logger.LogSuccess(sprintf "      -> Order %s transitioned to Planned" item.SupplyOrderId)
                            | Error err -> logger.LogError(sprintf "      -> Transition failed for %s to Planned: %A" item.SupplyOrderId err)
                | None -> ()
        }
