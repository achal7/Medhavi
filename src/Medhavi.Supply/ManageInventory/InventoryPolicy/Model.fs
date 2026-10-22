/// SE-S-031 — Inventory Policy Assignment Domain Model
module Medhavi.Supply.ManageInventory.InventoryPolicy.Model

open Medhavi.SemanticModel
open Medhavi.Supply.ManageInventory

type PolicyLifecycleState =
    | Active
    | Archived

type InventoryPolicyParameters =
    { ReplenishmentStrategy: ReplenishmentStrategy
      SafetyStockQuantity: Quantity
      ReorderPointQuantity: Quantity option
      MinimumQuantity: Quantity option
      MaximumQuantity: Quantity option
      TargetStockLevel: Quantity
      TargetServiceLevel: decimal
      ReviewFrequencyDays: int option
      LeadTimeDays: int
      DemandVariability: decimal }

type InventoryPolicyChangeEvent =
    { SequenceNumber: int
      ChangeTimestamp: Timestamp
      PreviousParameters: InventoryPolicyParameters option
      NewParameters: InventoryPolicyParameters
      TriggeringEvidence: string list
      ReasonCode: string }

/// SE-S-031 Aggregate Root
type InventoryPolicyAssignment =
    { Item: ItemId
      Location: LocationId
      LifecycleState: PolicyLifecycleState
      Parameters: InventoryPolicyParameters
      EffectiveDate: Timestamp
      AssignmentTimestamp: Timestamp
      ChangeHistory: InventoryPolicyChangeEvent list }

type UpdateInventoryPolicyCmd =
    { Item: ItemId
      Location: LocationId
      TargetServiceLevel: decimal option
      ManualOverrideParameters: InventoryPolicyParameters option
      Timestamp: Timestamp }

/// EV-S-021: Inventory Policy Assignment Updated
type InventoryPolicyEvent =
    | InventoryPolicyAssignmentUpdated of assignment: InventoryPolicyAssignment * changeEventOpt: InventoryPolicyChangeEvent option

let evolve (stateOpt: InventoryPolicyAssignment option) (event: InventoryPolicyEvent) : InventoryPolicyAssignment option =
    match event with
    | InventoryPolicyAssignmentUpdated(assignment, _) -> Some assignment

let replay (events: InventoryPolicyEvent seq) : InventoryPolicyAssignment option =
    Seq.fold evolve None events
