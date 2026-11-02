module Medhavi.Client.Promise.OrderBook.OrderBookActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Promise
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client

let ingestOrder: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Promise.UnderstandOrders.OrderBook
            let lineId = sprintf "LINE-IN-%d" (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 10000L)
            let snapId = sprintf "SNAP-ORD-%s" (DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss"))
            let orderRef = sprintf "ORD-PORTAL-%d" (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 10000L)
            let req: IngestOrderRequest =
                { SnapshotId = snapId
                  PlanningScopeId = "PS-GLOBAL-SUPPLY-CHAIN"
                  CustomerOrderIdentifier = orderRef
                  CustomerId = "CUST-TITAN-CLOUD"
                  OrderChannel = "WebPortal"
                  OrderPriorityClass = "High"
                  ObservationTimestamp = DateTimeOffset.UtcNow
                  Lines =
                    [ { OrderLineId = lineId
                        Item = "SKU-DRIVE-1TB"
                        RequestedQuantity = 100.0m
                        UnitOfMeasure = "EA"
                        RequestedDeliveryDate = DateTimeOffset.UtcNow.AddDays(7.0)
                        EarliestAcceptableDate = Some(DateTimeOffset.UtcNow.AddDays(5.0))
                        LatestAcceptableDate = Some(DateTimeOffset.UtcNow.AddDays(10.0))
                        DestinationLocation = "LOC-EAST-HUB"
                        ExternalLineReference = "PO-LINE-AUTO" } ] }

            match! api.Ingest req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Order intake snapshot '{dto.SnapshotId}' ingested successfully with {dto.Lines.Length} lines.",
                            [ OrderBookGrid.storeId
                              DataStoreId.literal "order-commitment-store" ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let validateLine: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some snapId ->
                let api = ctx.Domain.Promise.UnderstandOrders.OrderBook
                let req: ValidateOrderLineReq =
                    { SnapshotId = snapId
                      OrderLineId = "LINE-TITAN-7001-01"
                      EvaluationTime = DateTimeOffset.UtcNow }

                match! api.Validate req with
                | Ok dto ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Order snapshot '{dto.SnapshotId}' lines validated. State: {dto.LifecycleState}",
                                [ OrderBookGrid.storeId ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No order snapshot selected for validation."
        }

let evaluateAtp: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some snapId ->
                return Ok(ActionExecutionOutcome.ExplainRequested(snapId, "PromiseATP"))
            | None -> return Error "No order snapshot selected for ATP evaluation."
        }

let cancelOrder: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some snapId ->
                let api = ctx.Domain.Promise.UnderstandOrders.OrderBook
                let req: CancelOrderLineReq =
                    { SnapshotId = snapId
                      OrderLineId = "LINE-TITAN-7001-01"
                      CancellationReason = "Customer requested cancellation via portal"
                      CancellationTime = DateTimeOffset.UtcNow }

                match! api.Cancel req with
                | Ok dto ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Order snapshot '{dto.SnapshotId}' cancelled successfully.",
                                [ OrderBookGrid.storeId ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No order snapshot selected for cancellation."
        }

let all: (ActionDefId * ActionHandler) list =
    [ OrderBookGrid.OrderBookActions.ingestOrder.Id, ingestOrder
      OrderBookGrid.OrderBookActions.validateLine.Id, validateLine
      OrderBookGrid.OrderBookActions.evaluateAtp.Id, evaluateAtp
      OrderBookGrid.OrderBookActions.cancelOrder.Id, cancelOrder ]

