/// Projections for Supplier Commitment Aggregate
/// Maps pure domain models to Medhavi.Contracts.Supply DTOs and provides projection reducers
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCommitment.Projections

open Medhavi.Contracts.Supply
open Medhavi.SemanticModel
open Model
open Medhavi.Supply

let mapEventToDto (e: CollaborationEvent) : CollaborationEventDto =
    let eventType =
        match e.EventType with
        | CollaborationEventType.EnterpriseRequest -> Medhavi.Contracts.Supply.CollaborationEventType.EnterpriseRequest
        | CollaborationEventType.SupplierAcknowledgement -> Medhavi.Contracts.Supply.CollaborationEventType.SupplierAcknowledgement
        | CollaborationEventType.SupplierCommitment -> Medhavi.Contracts.Supply.CollaborationEventType.SupplierCommitment
        | CollaborationEventType.SupplierRejection -> Medhavi.Contracts.Supply.CollaborationEventType.SupplierRejection
        | CollaborationEventType.SupplierAlternative -> Medhavi.Contracts.Supply.CollaborationEventType.SupplierAlternative
        | CollaborationEventType.EnterpriseConfirmation -> Medhavi.Contracts.Supply.CollaborationEventType.EnterpriseConfirmation

    let actor =
        match e.Actor with
        | CollaborationActor.Enterprise -> Medhavi.Contracts.Supply.CollaborationActor.Enterprise
        | CollaborationActor.Supplier -> Medhavi.Contracts.Supply.CollaborationActor.Supplier

    { SequenceNumber = e.SequenceNumber
      EventType = eventType
      EventTimestamp = Timestamp.value e.EventTimestamp
      Actor = actor
      Details = e.Details }

let mapStateToDto (s: SupplierCommitmentState) : Medhavi.Contracts.Supply.SupplierCommitmentState =
    match s with
    | SupplierCommitmentState.Proposed -> Medhavi.Contracts.Supply.SupplierCommitmentState.Proposed
    | SupplierCommitmentState.Acknowledged -> Medhavi.Contracts.Supply.SupplierCommitmentState.Acknowledged
    | SupplierCommitmentState.Committed -> Medhavi.Contracts.Supply.SupplierCommitmentState.Committed
    | SupplierCommitmentState.PartiallyCommitted -> Medhavi.Contracts.Supply.SupplierCommitmentState.PartiallyCommitted
    | SupplierCommitmentState.Rejected -> Medhavi.Contracts.Supply.SupplierCommitmentState.Rejected
    | SupplierCommitmentState.AlternativeProposed -> Medhavi.Contracts.Supply.SupplierCommitmentState.AlternativeProposed
    | SupplierCommitmentState.Confirmed -> Medhavi.Contracts.Supply.SupplierCommitmentState.Confirmed

let mapConfidenceToDto (c: CommitmentConfidenceLevel) : Medhavi.Contracts.Supply.CommitmentConfidenceLevel =
    match c with
    | CommitmentConfidenceLevel.Firm -> Medhavi.Contracts.Supply.CommitmentConfidenceLevel.Firm
    | CommitmentConfidenceLevel.Conditional -> Medhavi.Contracts.Supply.CommitmentConfidenceLevel.Conditional
    | CommitmentConfidenceLevel.Tentative -> Medhavi.Contracts.Supply.CommitmentConfidenceLevel.Tentative
    | CommitmentConfidenceLevel.Estimated -> Medhavi.Contracts.Supply.CommitmentConfidenceLevel.Estimated

let mapOutcomeToDto (o: SupplierResponseAcceptanceOutcome) : Medhavi.Contracts.Supply.SupplierResponseAcceptanceOutcome =
    match o with
    | SupplierResponseAcceptanceOutcome.AcceptAsFirm -> Medhavi.Contracts.Supply.SupplierResponseAcceptanceOutcome.AcceptAsFirm
    | SupplierResponseAcceptanceOutcome.AcceptWithBuffer(s, l) -> Medhavi.Contracts.Supply.SupplierResponseAcceptanceOutcome.AcceptWithBuffer(s, l)
    | SupplierResponseAcceptanceOutcome.EscalateForReview r -> Medhavi.Contracts.Supply.SupplierResponseAcceptanceOutcome.EscalateForReview r
    | SupplierResponseAcceptanceOutcome.RejectAndSeekAlternative r -> Medhavi.Contracts.Supply.SupplierResponseAcceptanceOutcome.RejectAndSeekAlternative r

let mapCommitmentToDto (c: SupplierCommitment) : SupplierCommitmentDto =
    { SupplierId = SupplierId.value c.SupplierId
      ItemId = ItemId.value c.ItemId
      RequiredDate = Timestamp.value c.RequiredDate
      RequestedQuantity = Quantity.value c.RequestedQuantity
      RequestedDate = Timestamp.value c.RequestedDate
      RequestTimestamp = Timestamp.value c.RequestTimestamp
      CurrentState = mapStateToDto c.CurrentState
      CommittedQuantity = c.CommittedQuantity |> Option.map Quantity.value
      CommittedDate = c.CommittedDate |> Option.map Timestamp.value
      ResponseTimestamp = c.ResponseTimestamp |> Option.map Timestamp.value
      Confidence = mapConfidenceToDto c.Confidence
      AlternativeDescription = c.AlternativeDescription
      AlternativeQuantity = c.AlternativeQuantity |> Option.map Quantity.value
      AlternativeDate = c.AlternativeDate |> Option.map Timestamp.value
      SupplierAssumptions = c.SupplierAssumptions
      CollaborationEvents = c.CollaborationEvents |> List.map mapEventToDto
      AcceptanceOutcome = c.AcceptanceOutcome |> Option.map mapOutcomeToDto
      DataQualityFlag = c.DataQualityFlag }

let apply (state: Map<SupplierCommitmentId, SupplierCommitmentDto>) (event: SupplierCommitmentEvent) : Map<SupplierCommitmentId, SupplierCommitmentDto> =
    match event with
    | SupplierCommitmentChanged(commitment, _) ->
        Map.add commitment.CommitmentId (mapCommitmentToDto commitment) state

let seedFromAggregates (aggregates: SupplierCommitment list) : Map<SupplierCommitmentId, SupplierCommitmentDto> =
    aggregates
    |> List.map (fun a -> a.CommitmentId, mapCommitmentToDto a)
    |> Map.ofList

