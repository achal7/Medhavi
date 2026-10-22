/// BR-S-181, BR-S-184, BR-S-185, BR-S-186, BR-S-187
/// Traces to: Supply Intelligence Specification §7
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCollaborationHealth.Rules

open System
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

/// BR-S-181: Identity validation
let validateIdentity (scopeId: PlanningScopeId) (periodId: EvaluationPeriodId) : Result<unit, DomainError> =
    if String.IsNullOrWhiteSpace(PlanningScopeId.value scopeId) then
        Error(DomainError.validation "AssessmentScopeId cannot be empty (BR-S-181)")
    elif String.IsNullOrWhiteSpace(EvaluationPeriodId.value periodId) then
        Error(DomainError.validation "EvaluationPeriodId cannot be empty (BR-S-181)")
    else
        Ok()

/// BR-S-187: Version monotonicity
let getNextVersionNumber (stateOpt: SupplierCollaborationHealthAssessment option) : int =
    match stateOpt with
    | None -> 1
    | Some assessment ->
        match assessment.Versions with
        | [] -> 1
        | versions -> (versions |> List.map (fun v -> v.VersionNumber) |> List.max) + 1

/// BR-S-185: Published version immutability
let validateVersionNotPublished (stateOpt: SupplierCollaborationHealthAssessment option) (versionNumber: int) : Result<unit, DomainError> =
    match stateOpt with
    | None -> Ok()
    | Some assessment ->
        let existing = assessment.Versions |> List.tryFind (fun v -> v.VersionNumber = versionNumber)
        match existing with
        | Some v when v.State = HealthVersionLifecycleState.Published ->
            Error(ruleViolationWithDetail ArsIdentifiers.Rules.publishedSupplierCollaborationHealthAssessmentImmutability $"Version {versionNumber} is already Published and immutable")
        | _ -> Ok()
