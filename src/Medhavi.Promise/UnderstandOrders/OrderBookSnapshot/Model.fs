/// CA-P-001 Capture and Validate Order Book
/// Traces to: SE-P-001 Order Book Snapshot, SE-P-010 Order Line
module Medhavi.Promise.UnderstandOrders.OrderBookSnapshot.Model

open System
open Medhavi.SemanticModel
open Medhavi.Promise

type SnapshotLifecycleState =
    | Received
    | Validated
    | ValidationFailed
    | CommittedToEvaluation
    | Archived

/// Granular line entity within Order Book Snapshot (SE-P-010)
type OrderLine =
    { OrderLineId: OrderLineId
      Item: ItemId
      RequestedQuantity: Quantity
      UnitOfMeasure: UnitOfMeasureId
      RequestedDeliveryDate: Timestamp
      EarliestAcceptableDate: Timestamp option
      LatestAcceptableDate: Timestamp option
      DestinationLocation: LocationId
      ExternalLineReference: string
      ValidationStatus: LineValidationStatus
      RejectionReason: string option }

/// SE-P-001 Order Book Snapshot Aggregate Root
type OrderBookSnapshot =
    { SnapshotId: OrderBookSnapshotId
      PlanningScopeId: PlanningScopeId
      CustomerOrderIdentifier: string
      CustomerId: CustomerId
      OrderChannel: string
      OrderPriorityClass: OrderPriorityClass
      ObservationTimestamp: Timestamp
      Lines: OrderLine list
      LifecycleState: SnapshotLifecycleState
      DecisionTraceability: string option }

/// AB-P-001 Ingest Order Request Command payload
type IngestOrderRequestCmd =
    { SnapshotId: OrderBookSnapshotId
      PlanningScopeId: PlanningScopeId
      CustomerOrderIdentifier: string
      CustomerId: CustomerId
      OrderChannel: string
      OrderPriorityClass: OrderPriorityClass
      ObservationTimestamp: Timestamp
      Lines: OrderLine list }

/// AB-P-002 Validate Order Terms Command payload
type ValidateOrderTermsCmd =
    { SnapshotId: OrderBookSnapshotId
      OrderLineId: OrderLineId
      EvaluationTime: Timestamp }

/// AB-P-003 Cancel Order Line Command payload
type CancelOrderLineCmd =
    { SnapshotId: OrderBookSnapshotId
      OrderLineId: OrderLineId
      CancellationReason: string
      CancellationTime: Timestamp }

type OrderBookCmd =
    | Ingest of IngestOrderRequestCmd
    | Validate of ValidateOrderTermsCmd
    | Cancel of CancelOrderLineCmd

    static member GetId(cmd: OrderBookCmd) =
        match cmd with
        | Ingest c -> c.SnapshotId
        | Validate c -> c.SnapshotId
        | Cancel c -> c.SnapshotId

/// Domain Events emitted by Order Book Snapshot
type OrderBookEvent =
    | OrderBookIngested of OrderBookSnapshot
    | OrderValidationPassed of SnapshotId: OrderBookSnapshotId * LineId: OrderLineId * EvaluationTime: Timestamp * TraceId: string
    | OrderValidationFailed of SnapshotId: OrderBookSnapshotId * LineId: OrderLineId * EvaluationTime: Timestamp * Reasons: string list * TraceId: string
    | OrderLineCancelled of SnapshotId: OrderBookSnapshotId * LineId: OrderLineId * Reason: string * CancelTime: Timestamp

/// Pure state evolution (catamorphism)
let evolve: Medhavi.Foundation.Contracts.Evolve<OrderBookSnapshot, OrderBookEvent> =
    fun (state: OrderBookSnapshot option) (event: OrderBookEvent) ->
        match event with
        | OrderBookIngested snapshot -> Some snapshot

        | OrderValidationPassed(snapId, lineId, _, traceId) ->
            state
            |> Option.map(fun snapshot ->
                if snapshot.SnapshotId = snapId then
                    let updatedLines =
                        snapshot.Lines
                        |> List.map(fun line ->
                            if line.OrderLineId = lineId then
                                { line with
                                    ValidationStatus = LineValidationStatus.Validated
                                    RejectionReason = None }
                            else
                                line)

                    let allPassed =
                        updatedLines
                        |> List.forall(fun l -> l.ValidationStatus = LineValidationStatus.Validated)

                    { snapshot with
                        Lines = updatedLines
                        LifecycleState = if allPassed then Validated else snapshot.LifecycleState
                        DecisionTraceability = Some traceId }
                else
                    snapshot)

        | OrderValidationFailed(snapId, lineId, _, reasons, traceId) ->
            state
            |> Option.map(fun snapshot ->
                if snapshot.SnapshotId = snapId then
                    let reasonMsg = String.concat "; " reasons

                    let updatedLines =
                        snapshot.Lines
                        |> List.map(fun line ->
                            if line.OrderLineId = lineId then
                                { line with
                                    ValidationStatus = LineValidationStatus.Quarantined reasonMsg
                                    RejectionReason = Some reasonMsg }
                            else
                                line)

                    { snapshot with
                        Lines = updatedLines
                        LifecycleState = ValidationFailed
                        DecisionTraceability = Some traceId }
                else
                    snapshot)

        | OrderLineCancelled(snapId, lineId, reason, _) ->
            state
            |> Option.map(fun snapshot ->
                if snapshot.SnapshotId = snapId then
                    let updatedLines =
                        snapshot.Lines
                        |> List.map(fun line ->
                            if line.OrderLineId = lineId then
                                { line with
                                    ValidationStatus = LineValidationStatus.Quarantined $"Cancelled: {reason}"
                                    RejectionReason = Some $"Cancelled: {reason}" }
                            else
                                line)

                    { snapshot with Lines = updatedLines }
                else
                    snapshot)

