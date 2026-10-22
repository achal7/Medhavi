/// BR-S-041, BR-S-161, BR-S-164, BR-S-165, BR-S-166, BR-S-169, BR-S-170, BR-S-175
/// Traces to: Supply Intelligence Specification §7
module Medhavi.Supply.ManageCapacity.CapacityHealth.Rules

open System
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

/// BR-S-041: Capacity Health Assessment Identity
let validateIdentity (scopeId: PlanningScopeId) (periodId: EvaluationPeriodId) : Result<unit, DomainError> =
    if String.IsNullOrWhiteSpace(PlanningScopeId.value scopeId) then
        Error(DomainError.validation "PlanningScopeId cannot be empty (BR-S-041)")
    elif String.IsNullOrWhiteSpace(EvaluationPeriodId.value periodId) then
        Error(DomainError.validation "EvaluationPeriodId cannot be empty (BR-S-041)")
    else
        Ok()

/// BR-S-165: Immutability of Published Health Assessment versions
let validateVersionNotPublished (existingOpt: CapacityHealthAssessment option) (targetVersion: int) : Result<unit, DomainError> =
    match existingOpt with
    | None -> Ok()
    | Some existing ->
        let alreadyPublished =
            existing.Versions
            |> List.exists (fun v -> v.VersionNumber = targetVersion && v.State = HealthVersionLifecycleState.Published)
        if alreadyPublished then
            Error(ruleViolationWithDetail ArsIdentifiers.Rules.publishedCapacityHealthAssessmentImmutability $"Capacity Health Assessment version {targetVersion} is already Published and immutable.")
        else
            Ok()

/// BR-S-164: Next version monotonic increment
let getNextVersionNumber (existingOpt: CapacityHealthAssessment option) : int =
    match existingOpt with
    | None -> 1
    | Some existing ->
        match existing.Versions with
        | [] -> 1
        | v :: _ -> v.VersionNumber + 1
