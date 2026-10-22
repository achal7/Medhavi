/// AB-S-030 — Update Capacity Position Assessment Behavior
/// Traces to: Supply Intelligence Specification §4.3.1 (AB-S-030, EV-S-030)
module Medhavi.Supply.ManageCapacity.CapacityPosition.Behaviors

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.SemanticModel
open Medhavi.Supply
open Decisions
open Model
open Policies
open Rules

type PositionFactsProvider = PhysicalResourceId -> ResourceCapacityFacts * ResourceLoadFact

type PositionOutcome =
    | PositionUpdated of utilization: string * constraintStatus: string * bottleneck: string
    | PositionUnchanged

/// AB-S-030: Execute Update Capacity Position Assessment
let update
    (factsProvider: PositionFactsProvider)
    (policy: CapacityManagementPolicy)
    : Decide<CapacityPositionAssessment, UpdateCapacityPositionCmd, CapacityPositionEvent> =
    fun (cmd: UpdateCapacityPositionCmd) (stateOpt: CapacityPositionAssessment option) ->
        result {
            do! validateIdentity cmd.ResourceId

            let capacityFacts, loadFact = factsProvider cmd.ResourceId
            do! validateEligibility cmd.ResourceId capacityFacts.RatePerHour

            let newAssessment, changeEventOpt =
                evaluatePositionStatus
                    cmd.ResourceId
                    capacityFacts
                    loadFact
                    stateOpt
                    cmd.TriggeringEvidence
                    cmd.Timestamp
                    policy

            let events = [ CapacityPositionAssessmentChanged(newAssessment, changeEventOpt) ]

            let outcome =
                match changeEventOpt with
                | Some _ ->
                    PositionUpdated(
                        string newAssessment.UtilizationStatus,
                        string newAssessment.ConstraintStatus,
                        string newAssessment.BottleneckStatus)
                | None -> PositionUnchanged

            let traceOutcome = { Outcome = outcome; Evaluations = [] }

            return
                buildDecisionWithTrace
                    evolve
                    stateOpt
                    events
                    ArsIdentifiers.Decisions.determineCapacityPositionStatus.Id
                    []
                    ArsIdentifiers.Capabilities.manageCapacity.Id
                    traceOutcome
                    (Some ArsIdentifiers.Policies.capacityManagementPolicy.Id)
                    (Some 1)
                    [ ArsIdentifiers.SemanticObjects.capacityPositionAssessment.Id ]
                    (Some $"Capacity position updated for {PhysicalResourceId.value cmd.ResourceId}: {newAssessment.UtilizationStatus} / {newAssessment.ConstraintStatus}")
        }
