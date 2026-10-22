/// SE-S-031 — Inventory Policy Business Rules
module Medhavi.Supply.ManageInventory.InventoryPolicy.Rules

open System
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Medhavi.Supply.ManageInventory
open Model

/// BR-S-031: Inventory Policy Assignment Aggregate Identity
let validateAggregateIdentity (item: ItemId) (location: LocationId) : Result<unit, DomainError> =
    if String.IsNullOrWhiteSpace(ItemId.value item) then
        Error(DomainError.validation "Item identity is invalid for Inventory Policy Assignment")
    elif String.IsNullOrWhiteSpace(LocationId.value location) then
        Error(DomainError.validation "Location identity is invalid for Inventory Policy Assignment")
    else
        Ok()

/// BR-S-141: Inventory Policy Update Eligibility
let validateUpdateEligibility (stateOpt: InventoryPolicyAssignment option) : Result<unit, DomainError> =
    match stateOpt with
    | Some policy when policy.LifecycleState = PolicyLifecycleState.Archived ->
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.inventoryPolicyUpdateEligibility "Archived inventory policy cannot be updated")
    | _ -> Ok()

/// BR-S-046 / BR-S-150: Policy Parameter Validity
let validatePolicyParameters (params': InventoryPolicyParameters) : Result<unit, DomainError> =
    if Quantity.value params'.SafetyStockQuantity < 0m then
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.inventoryPolicyParameterValidity "Safety stock cannot be negative")
    elif params'.TargetServiceLevel < 0.50m || params'.TargetServiceLevel > 0.999m then
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.inventoryPolicyParameterValidity "Target service level must be between 0.50 and 0.999")
    else
        match params'.ReplenishmentStrategy with
        | ReplenishmentStrategy.ReorderPoint ->
            match params'.ReorderPointQuantity with
            | Some rop when Quantity.value rop < Quantity.value params'.SafetyStockQuantity ->
                Error(
                    ruleViolationWithDetail ArsIdentifiers.Rules.inventoryPolicyParameterValidity "Reorder point must be greater than or equal to safety stock"
                )
            | None -> Error(ruleViolationWithDetail ArsIdentifiers.Rules.inventoryPolicyParameterValidity "Reorder point is required for ReorderPoint strategy")
            | _ -> Ok()
        | ReplenishmentStrategy.MinMax ->
            match params'.MinimumQuantity, params'.MaximumQuantity with
            | Some minQ, Some maxQ when Quantity.value maxQ <= Quantity.value minQ ->
                Error(ruleViolationWithDetail ArsIdentifiers.Rules.inventoryPolicyParameterValidity "Maximum quantity must be greater than minimum quantity")
            | None, _
            | _, None ->
                Error(
                    ruleViolationWithDetail
                        ArsIdentifiers.Rules.inventoryPolicyParameterValidity
                        "Minimum and Maximum quantities are required for MinMax strategy"
                )
            | _ -> Ok()
        | _ -> Ok()

let getNextSequenceNumber (history: InventoryPolicyChangeEvent list) : int =
    match history with
    | [] -> 1
    | events ->
        let maxSeq = events |> List.map(fun e -> e.SequenceNumber) |> List.fold max 0
        maxSeq + 1
