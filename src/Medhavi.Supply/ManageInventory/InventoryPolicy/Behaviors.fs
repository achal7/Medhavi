/// SE-S-031 — Inventory Policy Behaviors
/// Traces to: AB-S-021 (Update Inventory Policy)
module Medhavi.Supply.ManageInventory.InventoryPolicy.Behaviors

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.SemanticModel
open Medhavi.Supply
open Medhavi.Supply.ManageInventory
open Model
open Policies
open Rules
open Decisions

type PolicyUpdateDecisionOutcome =
    | PolicyUpdated of strategy: string * ss: decimal * rop: decimal option
    | PolicyMaintained of reason: string

let update
    (governancePolicy: InventoryPolicyGovernancePolicy)
    (inputProvider: ItemId -> LocationId -> decimal option -> InventoryPolicyParameters option -> PolicyDeterminationInput)
    (evidenceProvider: ItemId -> LocationId -> string list)
    : Decide<InventoryPolicyAssignment, UpdateInventoryPolicyCmd, InventoryPolicyEvent> =
    fun (cmd: UpdateInventoryPolicyCmd) (stateOpt: InventoryPolicyAssignment option) ->
        result {
            do! validateAggregateIdentity cmd.Item cmd.Location
            do! validateUpdateEligibility stateOpt

            let currentAssignmentOpt = stateOpt
            let currentParamsOpt = currentAssignmentOpt |> Option.map (fun a -> a.Parameters)

            let input = inputProvider cmd.Item cmd.Location cmd.TargetServiceLevel cmd.ManualOverrideParameters
            let! decisionOutcome = determineInventoryPolicy governancePolicy currentParamsOpt input
            let triggeringEvidence = evidenceProvider cmd.Item cmd.Location

            match decisionOutcome with
            | MaintainCurrentPolicy reason ->
                let assignment =
                    match currentAssignmentOpt with
                    | Some a -> a
                    | None ->
                        let defaultQty = Quantity.zero
                        { Item = cmd.Item
                          Location = cmd.Location
                          LifecycleState = PolicyLifecycleState.Active
                          Parameters =
                            { ReplenishmentStrategy = ReplenishmentStrategy.ReorderPoint
                              SafetyStockQuantity = defaultQty
                              ReorderPointQuantity = Some defaultQty
                              MinimumQuantity = Some defaultQty
                              MaximumQuantity = Some defaultQty
                              TargetStockLevel = defaultQty
                              TargetServiceLevel = governancePolicy.DefaultServiceLevelTarget
                              ReviewFrequencyDays = Some governancePolicy.DefaultReviewIntervalDays
                              LeadTimeDays = 7
                              DemandVariability = 0m }
                          EffectiveDate = cmd.Timestamp
                          AssignmentTimestamp = cmd.Timestamp
                          ChangeHistory = [] }

                let events = [ InventoryPolicyAssignmentUpdated(assignment, None) ]
                let decision = { Outcome = PolicyMaintained reason; Evaluations = [] }

                return
                    buildDecisionWithTrace
                        evolve
                        stateOpt
                        events
                        ArsIdentifiers.Decisions.determineInventoryPolicy.Id
                        []
                        ArsIdentifiers.Capabilities.manageInventory.Id
                        decision
                        (Some ArsIdentifiers.Policies.inventoryPolicyGovernance.Id)
                        (Some 1)
                        [ ArsIdentifiers.SemanticObjects.inventoryPolicyAssignment.Id ]
                        (Some $"Inventory policy maintained: {reason}")

            | ApplyStatisticalFormula newParams
            | ApplySimulationBasedOptimization newParams
            | ApplyManualOverride newParams ->
                let history =
                    match currentAssignmentOpt with
                    | Some a -> a.ChangeHistory
                    | None -> []

                let nextSeq = getNextSequenceNumber history

                let reasonCode =
                    match decisionOutcome with
                    | ApplyManualOverride _ -> "ManualPlannerOverride"
                    | ApplySimulationBasedOptimization _ -> "SimulationBasedOptimization"
                    | _ -> "StatisticalCalculation"

                let changeEvent: InventoryPolicyChangeEvent =
                    { SequenceNumber = nextSeq
                      ChangeTimestamp = cmd.Timestamp
                      PreviousParameters = currentParamsOpt
                      NewParameters = newParams
                      TriggeringEvidence = triggeringEvidence
                      ReasonCode = reasonCode }

                let updatedAssignment: InventoryPolicyAssignment =
                    { Item = cmd.Item
                      Location = cmd.Location
                      LifecycleState = PolicyLifecycleState.Active
                      Parameters = newParams
                      EffectiveDate = cmd.Timestamp
                      AssignmentTimestamp = cmd.Timestamp
                      ChangeHistory = changeEvent :: history }

                let events = [ InventoryPolicyAssignmentUpdated(updatedAssignment, Some changeEvent) ]

                let ropVal = newParams.ReorderPointQuantity |> Option.map Quantity.value
                let decision = { Outcome = PolicyUpdated(string newParams.ReplenishmentStrategy, Quantity.value newParams.SafetyStockQuantity, ropVal); Evaluations = [] }

                return
                    buildDecisionWithTrace
                        evolve
                        stateOpt
                        events
                        ArsIdentifiers.Decisions.determineInventoryPolicy.Id
                        []
                        ArsIdentifiers.Capabilities.manageInventory.Id
                        decision
                        (Some ArsIdentifiers.Policies.inventoryPolicyGovernance.Id)
                        (Some 1)
                        [ ArsIdentifiers.SemanticObjects.inventoryPolicyAssignment.Id ]
                        (Some $"Inventory policy updated: SS = {Quantity.value newParams.SafetyStockQuantity}")
        }
