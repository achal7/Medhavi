/// CA-P-006 Establish Commitments Projections
module Medhavi.Promise.EstablishCommitments.OrderCommitment.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let mapRevisionToDto (r: CommitmentRevision) : CommitmentRevisionDto =
    { RevisionIndex = r.RevisionIndex
      CommittedDeliveryDate = Timestamp.value r.CommittedDeliveryDate
      PromisedQuantity = Quantity.value r.PromisedQuantity
      ProjectedLandedCost = r.ProjectedLandedCost
      GrossMarginPct = r.GrossMarginPct
      DecisionConfidence = r.DecisionConfidence
      SourcingLegs = r.SourcingLegs |> List.map EvaluateATP.Projections.mapLegToDto
      RevisionReason = r.RevisionReason
      RevisionTimestamp = Timestamp.value r.RevisionTimestamp }

let mapToDto (c: OrderCommitment) : OrderCommitmentDto =
    { CommitmentId = OrderCommitmentId.value c.CommitmentId
      CustomerOrderIdentifier = c.CustomerOrderIdentifier
      OrderLineId = OrderLineId.value c.OrderLineId
      CustomerId = CustomerId.value c.CustomerId
      Item = ItemId.value c.Item
      DestinationLocation = LocationId.value c.DestinationLocation
      CurrentRevisionIndex = c.CurrentRevisionIndex
      Revisions = c.Revisions |> List.map mapRevisionToDto
      LifecycleState =
        match c.LifecycleState with
        | Draft -> "Draft"
        | Committed -> "Committed"
        | Revised -> "Revised"
        | Fulfilled -> "Fulfilled"
        | Cancelled -> "Cancelled"
      DecisionTraceability = c.DecisionTraceability }

type State = Map<OrderCommitmentId, OrderCommitmentDto>

let initial: State = Map.empty

let apply (state: State) (event: OrderCommitmentEvent) : State =
    match event with
    | CommitmentEstablished commitment ->
        state |> Map.add commitment.CommitmentId (mapToDto commitment)

    | CommitmentRevised updated ->
        state |> Map.add updated.CommitmentId (mapToDto updated)

    | CommitmentFulfilled(id, _, traceId) ->
        state
        |> Map.change id (Option.map (fun dto ->
            { dto with
                LifecycleState = "Fulfilled"
                DecisionTraceability = Some traceId }))

    | CommitmentCancelled(id, _, _, traceId) ->
        state
        |> Map.change id (Option.map (fun dto ->
            { dto with
                LifecycleState = "Cancelled"
                DecisionTraceability = Some traceId }))

let seedFromAggregates (aggregates: OrderCommitment list) : State =
    aggregates
    |> List.fold (fun state c -> Map.add c.CommitmentId (mapToDto c) state) initial

