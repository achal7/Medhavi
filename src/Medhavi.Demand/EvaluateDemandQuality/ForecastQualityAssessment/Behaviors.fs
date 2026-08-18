/// AB-D-014 — Evaluate & Publish Forecast Quality Assessment Behaviors
module Medhavi.Demand.EvaluateDemandQuality.ForecastQualityAssessment.Behaviors

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers
open Policies
open Model
open Rules

let private buildDecisionTrace policyId policyVersion decisionId decisionOutcome state events summary =
    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        ArsIdentifiers.Capabilities.evaluateDemandQuality.Id
        decisionOutcome
        policyId
        policyVersion
        [ ArsIdentifiers.SemanticObjects.forecastQualityAssessment.Id
          ArsIdentifiers.SemanticObjects.forecastQualityAssessmentVersion.Id ]
        (Some summary)

/// AB-D-014: Evaluate Forecast Quality Decider
let evaluateForecastQuality
    (policy: ForecastMeasurementPolicy)
    : Decide<ForecastQualityAssessment, EvaluateForecastQualityCmd, ForecastQualityEvent> =
    fun (cmd: EvaluateForecastQualityCmd) (state: ForecastQualityAssessment option) ->
        result {
            let metrics = Algorithms.computeMetrics cmd.Observations cmd.CompletenessScore policy
            let qualityState = Algorithms.determineQualityState metrics policy
            let rationale = Algorithms.generateRationale metrics qualityState policy

            let nextVersionNumber =
                match state with
                | Some existing ->
                    if existing.Versions.IsEmpty then
                        1
                    else
                        (existing.Versions |> List.map(fun v -> v.VersionNumber) |> List.max) + 1
                | None -> 1

            let newVersion: ForecastQualityAssessmentVersion =
                { VersionNumber = nextVersionNumber
                  Metrics = metrics
                  OverallQualityState = qualityState
                  PolicyVersion = policy.PolicyVersion
                  LifecycleState = Draft
                  CreatedAt = Timestamp.value cmd.EvaluationTime
                  PublishedAt = None
                  Rationale = rationale }

            let updatedAssessment: ForecastQualityAssessment =
                match state with
                | Some existing ->
                    { existing with
                        Versions = newVersion :: existing.Versions
                        LastUpdated = cmd.EvaluationTime }
                | None ->
                    { AssessmentId = cmd.AssessmentId
                      Scope = cmd.Scope
                      EvaluationPeriodStart = cmd.EvaluationPeriodStart
                      EvaluationPeriodEnd = cmd.EvaluationPeriodEnd
                      CurrentPublishedVersion = None
                      Versions = [ newVersion ]
                      LastUpdated = cmd.EvaluationTime }

            let events = [ ForecastQualityEvaluated(updatedAssessment, newVersion) ]

            let dummyDecision: DecisionOutcome<ForecastQualityAssessmentVersion> =
                { Outcome = newVersion
                  Evaluations = [] }

            let summary =
                sprintf
                    "Forecast quality evaluated for scope '%s' (v%d): WAPE=%.2f%%, Bias=%.2f%%, Accuracy=%.2f%%, State=%s"
                    (PlanningScopeId.value cmd.Scope)
                    nextVersionNumber
                    metrics.Wape
                    metrics.ForecastBias
                    metrics.ForecastAccuracy
                    qualityState.AsString

            return
                buildDecisionTrace
                    (Some policy.PolicyId)
                    (Some policy.Version)
                    "DE-D-014-EVAL"
                    dummyDecision
                    state
                    events
                    summary
        }

/// AB-D-014: Publish Forecast Quality Assessment Decider (DE-D-011)
let publishForecastQualityAssessment
    (policy: ForecastMeasurementPolicy)
    : Decide<ForecastQualityAssessment, PublishForecastQualityAssessmentCmd, ForecastQualityEvent> =
    fun (cmd: PublishForecastQualityAssessmentCmd) (state: ForecastQualityAssessment option) ->
        result {
            let! existing =
                match state with
                | Some agg -> Ok agg
                | None -> Error(DomainError.validation "Cannot publish non-existent ForecastQualityAssessment")

            let input: PublicationRuleInput =
                { Assessment = existing
                  TargetVersionNumber = cmd.VersionNumber
                  Policy = policy }

            let! decision = Decisions.evaluatePublication Rules.publicationRules input
            let targetVersion = decision.Outcome

            // Transition target version to Published and previous Published version to Superseded
            let updatedVersions =
                existing.Versions
                |> List.map(fun v ->
                    if v.VersionNumber = cmd.VersionNumber then
                        { v with
                            LifecycleState = Published
                            PublishedAt = Some(Timestamp.value cmd.PublicationTime) }
                    elif v.LifecycleState = Published then
                        { v with LifecycleState = Superseded }
                    else
                        v)

            let publishedVersion = updatedVersions |> List.find(fun v -> v.VersionNumber = cmd.VersionNumber)

            let updatedAssessment: ForecastQualityAssessment =
                { existing with
                    CurrentPublishedVersion = Some cmd.VersionNumber
                    Versions = updatedVersions
                    LastUpdated = cmd.PublicationTime }

            let events = [ ForecastQualityAssessmentPublished(updatedAssessment, publishedVersion) ]

            let summary =
                sprintf
                    "Forecast quality assessment v%d published for scope '%s'. Quality State: %s (WAPE: %.2f%%, Bias: %.2f%%)"
                    cmd.VersionNumber
                    (PlanningScopeId.value cmd.Scope)
                    publishedVersion.OverallQualityState.AsString
                    publishedVersion.Metrics.Wape
                    publishedVersion.Metrics.ForecastBias

            return
                buildDecisionTrace
                    (Some policy.PolicyId)
                    (Some policy.Version)
                    ArsIdentifiers.Decisions.publishForecastQualityAssessment.Id
                    decision
                    state
                    events
                    summary
        }
