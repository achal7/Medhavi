/// BR-S-050, BR-S-180, BR-S-182, BR-S-183
/// Traces to: Supply Intelligence Specification §7
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCommitment.Rules

open System
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

/// BR-S-050: Supplier Commitment Identity Invariant
let validateIdentity (supplierId: SupplierId) (itemId: ItemId) (requiredDate: Timestamp) : Result<unit, DomainError> =
    if String.IsNullOrWhiteSpace(SupplierId.value supplierId) then
        Error(DomainError.validation "SupplierId cannot be empty (BR-S-050)")
    elif String.IsNullOrWhiteSpace(ItemId.value itemId) then
        Error(DomainError.validation "ItemId cannot be empty (BR-S-050)")
    elif Timestamp.value requiredDate = DateTimeOffset.MinValue then
        Error(DomainError.validation "RequiredDate must be a valid UTC timestamp (BR-S-050)")
    else
        Ok()

/// BR-S-180: Commitment Update Eligibility & Terminal State Invariance
let validateUpdateEligibility (currentStateOpt: SupplierCommitmentState option) : Result<unit, DomainError> =
    match currentStateOpt with
    | None -> Ok()
    | Some SupplierCommitmentState.Confirmed ->
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.supplierCommitmentUpdateEligibility "Supplier commitment is Confirmed and terminal; cannot be updated (Invariants §4.3.1)")
    | Some SupplierCommitmentState.Rejected ->
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.supplierCommitmentUpdateEligibility "Supplier commitment is Rejected and terminal; cannot be updated (Invariants §4.3.1)")
    | Some _ -> Ok()

/// BR-S-183 & ARS §5: Collaboration Event sequence number monotonicity
let validateNextSequenceNumber (events: CollaborationEvent list) (nextSeq: int) : Result<unit, DomainError> =
    match events with
    | [] ->
        if nextSeq = 1 then Ok()
        else Error(ruleViolationWithDetail ArsIdentifiers.Rules.supplierCommitmentAssessmentStateChangeImmutability $"First collaboration event sequence must be 1, got {nextSeq}")
    | last :: _ ->
        if nextSeq = last.SequenceNumber + 1 then Ok()
        else Error(ruleViolationWithDetail ArsIdentifiers.Rules.supplierCommitmentAssessmentStateChangeImmutability $"Collaboration event sequence must strictly increase monotonically from {last.SequenceNumber}, got {nextSeq}")
