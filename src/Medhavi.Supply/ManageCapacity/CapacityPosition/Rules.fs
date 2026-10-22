/// BR-S-040, BR-S-160, BR-S-162, BR-S-163, BR-S-167, BR-S-168, BR-S-171, BR-S-172, BR-S-173, BR-S-174
/// Traces to: Supply Intelligence Specification §7
module Medhavi.Supply.ManageCapacity.CapacityPosition.Rules

open System
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

/// BR-S-040: Capacity Position Assessment Identity
let validateIdentity (resourceId: PhysicalResourceId) : Result<unit, DomainError> =
    if String.IsNullOrWhiteSpace(PhysicalResourceId.value resourceId) then
        Error(DomainError.validation "PhysicalResourceId cannot be empty (BR-S-040)")
    else
        Ok()

/// BR-S-160: Capacity Position Update Eligibility
let validateEligibility (resourceId: PhysicalResourceId) (resourceRate: decimal) : Result<unit, DomainError> =
    if resourceRate <= 0.0m then
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.capacityPositionUpdateEligibility $"Resource '{PhysicalResourceId.value resourceId}' has non-positive capacity rate ({resourceRate}); must be an active physical resource with valid capacity.")
    else
        Ok()

/// BR-S-163 & BR-S-167: Sequence monotonicity check for change events
let validateNextSequenceNumber (currentHistory: CapacityAssessmentChangeEvent list) (nextSeq: int) : Result<unit, DomainError> =
    match currentHistory with
    | [] ->
        if nextSeq = 1 then Ok()
        else Error(ruleViolationWithDetail ArsIdentifiers.Rules.capacityPositionAssessmentHistoryImmutability $"First change event sequence must be 1, got {nextSeq}")
    | last :: _ ->
        if nextSeq = last.SequenceNumber + 1 then Ok()
        else Error(ruleViolationWithDetail ArsIdentifiers.Rules.capacityPositionAssessmentHistoryImmutability $"Change event sequence must strictly increase monotonically from {last.SequenceNumber}, got {nextSeq}")

/// Material change predicate to determine whether an EV-S-030 event and child change entity should be recorded
let isMaterialChange
    (prevOpt: CapacityPositionAssessment option)
    (newUtil: CapacityUtilizationStatus)
    (newConstraint: CapacityConstraintStatus)
    (newBottleneck: BottleneckAssessmentStatus)
    : bool =
    match prevOpt with
    | None -> true
    | Some prev ->
        prev.UtilizationStatus <> newUtil
        || prev.ConstraintStatus <> newConstraint
        || prev.BottleneckStatus <> newBottleneck
