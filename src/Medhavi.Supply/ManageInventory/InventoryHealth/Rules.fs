/// SE-S-032 — Inventory Health Business Rules
module Medhavi.Supply.ManageInventory.InventoryHealth.Rules

open System
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

/// BR-S-032: Inventory Health Assessment Aggregate Identity
let validateAggregateIdentity (scope: PlanningScopeId) (period: EvaluationPeriodId) : Result<unit, DomainError> =
    if String.IsNullOrWhiteSpace(PlanningScopeId.value scope) then
        Error(DomainError.validation "PlanningScopeId is invalid for Inventory Health Assessment")
    elif String.IsNullOrWhiteSpace(EvaluationPeriodId.value period) then
        Error(DomainError.validation "EvaluationPeriodId is invalid for Inventory Health Assessment")
    else
        Ok ()

/// BR-S-142: Inventory Health Assessment Eligibility
let validateAssessmentEligibility (itemLocations: (ItemId * LocationId) list) : Result<unit, DomainError> =
    if itemLocations.IsEmpty then
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.inventoryHealthAssessmentEligibility "Assessment scope contains zero active item-locations")
    else
        Ok ()

let getNextVersionNumber (versions: InventoryHealthAssessmentVersion list) : int =
    match versions with
    | [] -> 1
    | vs ->
        let maxVer = vs |> List.map (fun v -> v.VersionNumber) |> List.fold max 0
        maxVer + 1
