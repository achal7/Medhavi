/// CA-P-001 Ingest and Understand Orders Projections
module Medhavi.Promise.UnderstandOrders.OrderBookSnapshot.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let mapLineToDto (line: OrderLine) : OrderLineDto =
    { OrderLineId = OrderLineId.value line.OrderLineId
      Item = ItemId.value line.Item
      RequestedQuantity = Quantity.value line.RequestedQuantity
      UnitOfMeasure = UnitOfMeasureId.value line.UnitOfMeasure
      RequestedDeliveryDate = Timestamp.value line.RequestedDeliveryDate
      EarliestAcceptableDate = line.EarliestAcceptableDate |> Option.map Timestamp.value
      LatestAcceptableDate = line.LatestAcceptableDate |> Option.map Timestamp.value
      DestinationLocation = LocationId.value line.DestinationLocation
      ExternalLineReference = line.ExternalLineReference
      ValidationStatus =
        match line.ValidationStatus with
        | LineValidationStatus.PendingValidation -> "PendingValidation"
        | LineValidationStatus.Validated -> "Validated"
        | LineValidationStatus.InactiveSku -> "InactiveSku"
        | LineValidationStatus.InvalidDestination -> "InvalidDestination"
        | LineValidationStatus.CreditHold -> "CreditHold"
        | LineValidationStatus.ZeroQuantity -> "ZeroQuantity"
        | LineValidationStatus.PastDueDeliveryDate -> "PastDueDeliveryDate"
        | LineValidationStatus.Quarantined r -> $"Quarantined: {r}"
      RejectionReason = line.RejectionReason }

let mapToDto (snapshot: OrderBookSnapshot) : OrderBookSnapshotDto =
    { SnapshotId = OrderBookSnapshotId.value snapshot.SnapshotId
      PlanningScopeId = PlanningScopeId.value snapshot.PlanningScopeId
      CustomerOrderIdentifier = snapshot.CustomerOrderIdentifier
      CustomerId = CustomerId.value snapshot.CustomerId
      OrderChannel = snapshot.OrderChannel
      OrderPriorityClass = sprintf "%A" snapshot.OrderPriorityClass
      ObservationTimestamp = Timestamp.value snapshot.ObservationTimestamp
      Lines = snapshot.Lines |> List.map mapLineToDto
      LifecycleState =
        match snapshot.LifecycleState with
        | Received -> "Received"
        | Validated -> "Validated"
        | ValidationFailed -> "ValidationFailed"
        | CommittedToEvaluation -> "CommittedToEvaluation"
        | Archived -> "Archived"
      DecisionTraceability = snapshot.DecisionTraceability }

type State = Map<OrderBookSnapshotId, OrderBookSnapshotDto>

let initial: State = Map.empty

let apply (state: State) (event: OrderBookEvent) : State =
    match event with
    | OrderBookIngested snapshot ->
        state |> Map.add snapshot.SnapshotId (mapToDto snapshot)

    | OrderValidationPassed(snapId, lineId, _, traceId) ->
        state
        |> Map.change snapId (Option.map (fun dto ->
            let updatedLines =
                dto.Lines
                |> List.map (fun l ->
                    if l.OrderLineId = OrderLineId.value lineId then
                        { l with ValidationStatus = "Validated"; RejectionReason = None }
                    else l)
            let allPassed = updatedLines |> List.forall (fun l -> l.ValidationStatus = "Validated")
            { dto with
                Lines = updatedLines
                LifecycleState = if allPassed then "Validated" else dto.LifecycleState
                DecisionTraceability = Some traceId }))

    | OrderValidationFailed(snapId, lineId, _, reasons, traceId) ->
        state
        |> Map.change snapId (Option.map (fun dto ->
            let reasonStr = String.concat "; " reasons
            let updatedLines =
                dto.Lines
                |> List.map (fun l ->
                    if l.OrderLineId = OrderLineId.value lineId then
                        { l with ValidationStatus = $"Quarantined: {reasonStr}"; RejectionReason = Some reasonStr }
                    else l)
            { dto with
                Lines = updatedLines
                LifecycleState = "ValidationFailed"
                DecisionTraceability = Some traceId }))

    | OrderLineCancelled(snapId, lineId, reason, _) ->
        state
        |> Map.change snapId (Option.map (fun dto ->
            let updatedLines =
                dto.Lines
                |> List.map (fun l ->
                    if l.OrderLineId = OrderLineId.value lineId then
                        { l with ValidationStatus = $"Cancelled: {reason}"; RejectionReason = Some $"Cancelled: {reason}" }
                    else l)
            { dto with Lines = updatedLines }))

let seedFromAggregates (aggregates: OrderBookSnapshot list) : State =
    aggregates
    |> List.fold (fun state agg -> Map.add agg.SnapshotId (mapToDto agg) state) initial

