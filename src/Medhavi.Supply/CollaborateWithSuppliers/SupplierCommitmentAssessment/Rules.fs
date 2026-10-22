/// BR-S-050, BR-S-180, BR-S-182, BR-S-183
/// Traces to: Supply Intelligence Specification §7
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCommitmentAssessment.Rules

open System
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

/// BR-S-050: Supplier Commitment Assessment Identity Invariant
let validateIdentity (supplierId: SupplierId) : Result<unit, DomainError> =
    if String.IsNullOrWhiteSpace(SupplierId.value supplierId) then
        Error(DomainError.validation "SupplierId cannot be empty (BR-S-050)")
    else
        Ok()

/// BR-S-180: Assessment Update Eligibility
let validateEligibility (state: AssessmentLifecycleState) : Result<unit, DomainError> =
    match state with
    | AssessmentLifecycleState.Active -> Ok()
    | AssessmentLifecycleState.Archived ->
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.supplierCommitmentUpdateEligibility "Archived Supplier Commitment Assessment cannot be updated")

/// BR-S-183: Monotonic change event sequence validation
let validateNextSequenceNumber (history: SupplierCommitmentAssessmentChangeEvent list) (nextSeq: int) : Result<unit, DomainError> =
    match history with
    | [] ->
        if nextSeq = 1 then Ok()
        else Error(ruleViolationWithDetail ArsIdentifiers.Rules.supplierCommitmentAssessmentStateChangeImmutability $"First assessment change event sequence must be 1, got {nextSeq}")
    | last :: _ ->
        if nextSeq = last.SequenceNumber + 1 then Ok()
        else Error(ruleViolationWithDetail ArsIdentifiers.Rules.supplierCommitmentAssessmentStateChangeImmutability $"Assessment change event sequence must strictly increase from {last.SequenceNumber}, got {nextSeq}")
