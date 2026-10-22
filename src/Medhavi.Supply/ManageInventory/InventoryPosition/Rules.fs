/// SE-S-030 — Inventory Position Assessment Business Rules
module Medhavi.Supply.ManageInventory.InventoryPosition.Rules

open System
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

/// BR-S-030: Inventory Position Assessment Aggregate Identity
let validateAggregateIdentity (item: ItemId) (location: LocationId) : Result<unit, DomainError> =
    if String.IsNullOrWhiteSpace(ItemId.value item) then
        Error(DomainError.validation "Item identity is invalid for Inventory Position Assessment")
    elif String.IsNullOrWhiteSpace(LocationId.value location) then
        Error(DomainError.validation "Location identity is invalid for Inventory Position Assessment")
    else
        Ok ()

/// BR-S-140: Inventory Position Update Eligibility
let validateUpdateEligibility (stateOpt: InventoryPositionAssessment option) : Result<unit, DomainError> =
    match stateOpt with
    | Some assessment when assessment.LifecycleState = PositionLifecycleState.Archived ->
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.inventoryPositionUpdateEligibility "Archived inventory position assessment cannot be updated")
    | _ ->
        Ok ()

/// BR-S-144: Validate Change Event Monotonicity
let getNextSequenceNumber (history: InventoryAssessmentChangeEvent list) : int =
    match history with
    | [] -> 1
    | events ->
        let maxSeq = events |> List.map (fun e -> e.SequenceNumber) |> List.fold max 0
        maxSeq + 1
