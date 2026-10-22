/// AB-S-031 — Publish Capacity Health Assessment Behavior
/// Traces to: Supply Intelligence Specification §4.3.1 (AB-S-031, EV-S-031)
module Medhavi.Supply.ManageCapacity.CapacityHealth.Behaviors

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.SemanticModel
open Medhavi.Supply
open Decisions
open Model
open Policies
open Rules

type HealthOutcome = Published of version: int * supersededOpt: int option

type ScopePositionsProvider = PlanningScopeId -> CapacityPositionFact list

/// AB-S-031: Execute Publish Capacity Health Assessment
let publish
    (positionsProvider: ScopePositionsProvider)
    (policy: CapacityHealthPolicy)
    : Decide<CapacityHealthAssessment, PublishCapacityHealthCmd, CapacityHealthEvent> =
    fun (cmd: PublishCapacityHealthCmd) (stateOpt: CapacityHealthAssessment option) ->
        result {
            do! validateIdentity cmd.AssessmentScopeId cmd.EvaluationPeriodId

            let nextVersion = getNextVersionNumber stateOpt
            do! validateVersionNotPublished stateOpt nextVersion

            let positions = positionsProvider cmd.AssessmentScopeId

            let updatedAssessment, newVersion, supersededOpt =
                classifyHealth cmd.AssessmentScopeId cmd.EvaluationPeriodId positions stateOpt cmd.Timestamp policy

            let events =
                [
                    CapacityHealthAssessmentPublished(updatedAssessment, newVersion.VersionNumber, supersededOpt)
                ]

            let outcome = Published(newVersion.VersionNumber, supersededOpt)
            let traceOutcome = { Outcome = outcome; Evaluations = [] }

            return
                buildDecisionWithTrace
                    evolve
                    stateOpt
                    events
                    ArsIdentifiers.Decisions.classifyCapacityHealth.Id
                    []
                    ArsIdentifiers.Capabilities.manageCapacity.Id
                    traceOutcome
                    (Some ArsIdentifiers.Policies.capacityManagementPolicy.Id)
                    (Some 1)
                    [ ArsIdentifiers.SemanticObjects.capacityHealthAssessment.Id ]
                    (Some
                        $"Capacity health assessment published for scope {PlanningScopeId.value cmd.AssessmentScopeId}: Version {newVersion.VersionNumber}, State = {newVersion.OverallCapacityHealthState}")
        }
