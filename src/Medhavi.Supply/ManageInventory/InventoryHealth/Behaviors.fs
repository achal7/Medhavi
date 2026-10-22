/// SE-S-032 — Inventory Health Behaviors
/// Traces to: AB-S-022 (Publish Inventory Health Assessment)
module Medhavi.Supply.ManageInventory.InventoryHealth.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Policies
open Rules
open Decisions

let publish
    (policy: InventoryHealthAssessmentPolicy)
    (summariesProvider: PlanningScopeId -> EvaluationPeriodId -> ItemHealthSummary list * decimal)
    : Decide<InventoryHealthAssessment, PublishInventoryHealthCmd, InventoryHealthEvent> =
    fun (cmd: PublishInventoryHealthCmd) (stateOpt: InventoryHealthAssessment option) ->
        result {
            do! validateAggregateIdentity cmd.AssessmentScopeId cmd.EvaluationPeriodId

            let currentAssessment =
                match stateOpt with
                | Some existing -> existing
                | None ->
                    { AssessmentScopeId = cmd.AssessmentScopeId
                      EvaluationPeriodId = cmd.EvaluationPeriodId
                      Versions = [] }

            let nextVer = getNextVersionNumber currentAssessment.Versions
            let currentPublishedOpt = currentAssessment.Versions |> List.tryFind (fun v -> v.State = HealthVersionLifecycleState.Published)

            let summaries, confidence = summariesProvider cmd.AssessmentScopeId cmd.EvaluationPeriodId
            let! (newPublishedVer, supersededOpt) = classifyInventoryHealth policy currentPublishedOpt nextVer summaries confidence

            let versionWithTimestamp = { newPublishedVer with PublicationTimestamp = Some cmd.Timestamp }

            let updatedVersions =
                currentAssessment.Versions
                |> List.map (fun v ->
                    if supersededOpt = Some v.VersionNumber then
                        { v with State = HealthVersionLifecycleState.Superseded }
                    else
                        v)
                |> fun vs -> versionWithTimestamp :: vs

            let updatedAssessment = { currentAssessment with Versions = updatedVersions }

            let events = [ InventoryHealthAssessmentPublished(updatedAssessment, nextVer, supersededOpt) ]
            let decision = { Outcome = Published(nextVer, supersededOpt); Evaluations = [] }

            return
                buildDecisionWithTrace
                    evolve
                    stateOpt
                    events
                    ArsIdentifiers.Decisions.classifyInventoryHealth.Id
                    []
                    ArsIdentifiers.Capabilities.manageInventory.Id
                    decision
                    (Some ArsIdentifiers.Policies.inventoryHealthAssessmentPolicy.Id)
                    (Some 1)
                    [ ArsIdentifiers.SemanticObjects.inventoryHealthAssessment.Id ]
                    (Some $"Inventory health assessment published: Version {nextVer}")
        }
