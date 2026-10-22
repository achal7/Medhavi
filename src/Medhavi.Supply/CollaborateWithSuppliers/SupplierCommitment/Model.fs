/// SE-S-050 — Supplier Commitment Domain Model
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-050, AB-S-040, EV-S-040)
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCommitment.Model

open Medhavi.SemanticModel
open Medhavi.Supply

type SupplierCommitmentState =
    | Proposed
    | Acknowledged
    | Committed
    | PartiallyCommitted
    | Rejected
    | AlternativeProposed
    | Confirmed

type CollaborationEventType =
    | EnterpriseRequest
    | SupplierAcknowledgement
    | SupplierCommitment
    | SupplierRejection
    | SupplierAlternative
    | EnterpriseConfirmation

type CollaborationActor =
    | Enterprise
    | Supplier

type CommitmentConfidenceLevel =
    | Firm
    | Conditional
    | Tentative
    | Estimated

type SupplierResponseAcceptanceOutcome =
    | AcceptAsFirm
    | AcceptWithBuffer of AdditionalSafetyStock: decimal * ExtendedLeadTimeDays: decimal
    | EscalateForReview of Reason: string
    | RejectAndSeekAlternative of Reason: string

type CollaborationEvent =
    {
        SequenceNumber: int
        EventType: CollaborationEventType
        EventTimestamp: Timestamp
        Actor: CollaborationActor
        Details: string
    }

type SupplierCommitment =
    {
        CommitmentId: SupplierCommitmentId
        SupplierId: SupplierId
        ItemId: ItemId
        RequiredDate: Timestamp
        RequestedQuantity: Quantity
        RequestedDate: Timestamp
        RequestTimestamp: Timestamp
        CurrentState: SupplierCommitmentState
        CommittedQuantity: Quantity option
        CommittedDate: Timestamp option
        ResponseTimestamp: Timestamp option
        Confidence: CommitmentConfidenceLevel
        AlternativeDescription: string option
        AlternativeQuantity: Quantity option
        AlternativeDate: Timestamp option
        SupplierAssumptions: string list
        CollaborationEvents: CollaborationEvent list
        AcceptanceOutcome: SupplierResponseAcceptanceOutcome option
        DataQualityFlag: string option
    }

type RecordEnterpriseRequestCmd =
    {
        SupplierId: SupplierId
        ItemId: ItemId
        RequiredDate: Timestamp
        RequestedQuantity: Quantity
        RequestedDate: Timestamp
        RequestTimestamp: Timestamp
    }

type RecordSupplierResponseCmd =
    {
        SupplierId: SupplierId
        ItemId: ItemId
        RequiredDate: Timestamp
        ResponseEventType: CollaborationEventType
        CommittedQuantity: Quantity option
        CommittedDate: Timestamp option
        AlternativeDescription: string option
        AlternativeQuantity: Quantity option
        AlternativeDate: Timestamp option
        SupplierAssumptions: string list
        ResponseTimestamp: Timestamp
    }

type SupplierCommitmentEvent =
    | SupplierCommitmentChanged of commitment: SupplierCommitment * appendedEvent: CollaborationEvent

let evolve (stateOpt: SupplierCommitment option) (event: SupplierCommitmentEvent) : SupplierCommitment option =
    match event with
    | SupplierCommitmentChanged(commitment, _) -> Some commitment

let replay (events: SupplierCommitmentEvent seq) : SupplierCommitment option = Seq.fold evolve None events
