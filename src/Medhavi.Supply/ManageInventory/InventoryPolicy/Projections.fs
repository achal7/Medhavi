/// SE-S-031 — Inventory Policy Projections
module Medhavi.Supply.ManageInventory.InventoryPolicy.Projections

open Medhavi.Contracts.Supply
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

let mapParametersToDto (p: InventoryPolicyParameters) : InventoryPolicyParametersDto =
    {
        ReplenishmentStrategy = p.ReplenishmentStrategy.ToString()
        SafetyStockQuantity = Quantity.value p.SafetyStockQuantity
        ReorderPointQuantity = p.ReorderPointQuantity |> Option.map Quantity.value
        MinimumQuantity = p.MinimumQuantity |> Option.map Quantity.value
        MaximumQuantity = p.MaximumQuantity |> Option.map Quantity.value
        TargetStockLevel = Quantity.value p.TargetStockLevel
        TargetServiceLevel = p.TargetServiceLevel
        ReviewFrequencyDays = p.ReviewFrequencyDays
        LeadTimeDays = p.LeadTimeDays
        DemandVariability = p.DemandVariability
    }

let mapChangeEventToDto (ev: InventoryPolicyChangeEvent) : InventoryPolicyChangeEventDto =
    {
        SequenceNumber = ev.SequenceNumber
        ChangeTimestamp = Timestamp.value ev.ChangeTimestamp
        PreviousParameters = ev.PreviousParameters |> Option.map mapParametersToDto
        NewParameters = mapParametersToDto ev.NewParameters
        TriggeringEvidence = ev.TriggeringEvidence
        ReasonCode = ev.ReasonCode
    }

let mapToDto (assignment: InventoryPolicyAssignment) : InventoryPolicyAssignmentDto =
    {
        ItemId = ItemId.value assignment.Item
        LocationId = LocationId.value assignment.Location
        LifecycleState = sprintf "%A" assignment.LifecycleState
        Parameters = mapParametersToDto assignment.Parameters
        EffectiveDate = Timestamp.value assignment.EffectiveDate
        AssignmentTimestamp = Timestamp.value assignment.AssignmentTimestamp
        ChangeHistory = assignment.ChangeHistory |> List.map mapChangeEventToDto
    }

let seedFromAggregates
    (aggregates: InventoryPolicyAssignment list)
    : Map<ItemLocationId, InventoryPolicyAssignmentDto> =
    aggregates
    |> List.map(fun a ->
        let key = ItemLocationId.ofItemAndLocation a.Item a.Location
        key, mapToDto a)
    |> Map.ofList

let apply
    (state: Map<ItemLocationId, InventoryPolicyAssignmentDto>)
    (event: InventoryPolicyEvent)
    : Map<ItemLocationId, InventoryPolicyAssignmentDto> =
    match event with
    | InventoryPolicyAssignmentUpdated(assignment, _) ->
        let key = ItemLocationId.ofItemAndLocation assignment.Item assignment.Location
        Map.add key (mapToDto assignment) state
