/// AB-S-071 — Publish Distribution Plan Aggregate Behavior
/// Traces to: Supply Intelligence Specification §4.3.1 (AB-S-071, EV-S-071)
module Medhavi.Supply.ManageDistribution.DistributionPlan.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Model
open Policies
open Algorithms
open Rules
open Decisions

/// AB-S-071 — Execute pure functional distribution plan publication from active facts
let publish
    (policy: DistributionPlanPublicationPolicyParameters)
    (activeRecommendations: ActiveDistributionRecommendationFact list)
    : Decide<DistributionPlanAggregate, PublishDistributionPlanCmd, DistributionPlanEvent> =
    fun (cmd: PublishDistributionPlanCmd) (stateOpt: DistributionPlanAggregate option) ->
        result {
            // 1. Compose plan baseline via BA-S-051 using decoupled facts
            let compositionInput: PlanCompositionInput =
                { Recommendations = activeRecommendations
                  Policy = policy
                  PreviousPlan = stateOpt }

            let composition = Algorithms.compose compositionInput

            // 2. Evaluate Decision DE-S-072
            let publicationInput: PlanPublicationInput =
                { Composition = composition
                  Policy = policy
                  HasActiveRecommendations = not activeRecommendations.IsEmpty
                  IsPolicyCurrent = true }

            let! decision = evaluatePublication publicationRules publicationInput

            match decision.Outcome with
            | PlanPublicationDecisionOutcome.Hold reasons ->
                return! Error(ruleViolationWithDetail ArsIdentifiers.Rules.distributionPlanPublicationEligibility (String.concat "; " reasons))
            | PlanPublicationDecisionOutcome.Publish comp
            | PlanPublicationDecisionOutcome.PublishWithFlags(_, comp) ->
                // 3. Determine next version number (monotonicity)
                let versionNumber =
                    match stateOpt with
                    | Some prev -> prev.VersionNumber + 1
                    | None -> 1

                let signals =
                    match decision.Outcome with
                    | PlanPublicationDecisionOutcome.PublishWithFlags(flags, _) ->
                        let flagSignals =
                            flags |> List.map (fun f -> { SignalType = "PublicationFlag"; Description = f; AffectedMovementCount = 0 })
                        comp.RecommendationSignals @ flagSignals
                    | _ -> comp.RecommendationSignals

                let newAggregate: DistributionPlanAggregate =
                    { PlanId = cmd.PlanId
                      PlanningScopeId = cmd.PlanningScopeId
                      PlanningCycle = cmd.PlanningCycle
                      VersionNumber = versionNumber
                      LifecycleState = DistributionPlanLifecycleState.Published
                      PublicationReason = cmd.PublicationReason
                      TotalMovementCount = comp.TotalMovements
                      TotalQuantityMoved = comp.TotalQuantityMoved
                      LaneUtilizationSummary = comp.LaneUtilizationSummary
                      NetworkBalanceSummary = comp.NetworkBalanceSummary
                      FeasibilitySummary = comp.FeasibilitySummary
                      StabilitySummary = comp.StabilitySummary
                      RiskSignals = comp.RiskSignals
                      RecoveryOpportunities = comp.RecoveryOpportunities
                      RecommendationSignals = signals
                      ConfidenceDistribution = comp.ConfidenceDistribution
                      SourceRecommendationIds = activeRecommendations |> List.map (fun r -> r.RequirementId)
                      PublishedTimestamp = Some cmd.Timestamp
                      SupersededTimestamp = None
                      SupersededByVersion = None }

                let supersededVersion = stateOpt |> Option.map (fun prev -> prev.VersionNumber)
                let events = [ DistributionPlanPublished(newAggregate, supersededVersion) ]
                let traceId = Guid.NewGuid().ToString()

                let summary =
                    match decision.Outcome with
                    | PlanPublicationDecisionOutcome.Publish _ -> "Distribution Plan published"
                    | PlanPublicationDecisionOutcome.PublishWithFlags(flags, _) ->
                        let joined = String.concat "; " flags
                        sprintf "Distribution Plan published with flags: %s" joined
                    | PlanPublicationDecisionOutcome.Hold reasons ->
                        let joined = String.concat "; " reasons
                        sprintf "Distribution Plan held: %s" joined

                return
                    buildDecisionWithTrace
                        evolve
                        stateOpt
                        events
                        traceId
                        []
                        ArsIdentifiers.Capabilities.manageDistribution.Id
                        decision
                        (Some ArsIdentifiers.Policies.distributionPlanPublicationCadencePolicy.Id)
                        (Some policy.Version)
                        [ ArsIdentifiers.SemanticObjects.distributionPlan.Id ]
                        (Some summary)
        }
