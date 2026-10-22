/// DE-S-042 — Classify Collaboration Health Decision
/// Traces to: Supply Intelligence Specification §6 (DE-S-042)
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCollaborationHealth.Decisions

open System
open Medhavi.Common
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Policies

/// Classifies a single supplier's collaboration health based on its assessment facts (DE-S-042)
let classifySupplierHealth (assessment: SupplierCommitmentAssessmentFact) : SupplierCollaborationHealthClassification =
    let otif = assessment.OnTimeDeliveryRate
    let ackHours = assessment.AverageAcknowledgementHours
    let riskCount = assessment.ActiveRiskSignalsCount
    let healthState = assessment.AuthoritativeHealthState

    if healthState = SupplierCollaborationHealthState.Unassessed then
        SupplierCollaborationHealthClassification.Unassessed
    elif otif < 0.85m || riskCount >= 2 || healthState = SupplierCollaborationHealthState.CriticallyDependent || healthState = SupplierCollaborationHealthState.AtRisk then
        SupplierCollaborationHealthClassification.AtRisk
    elif otif < 0.95m || ackHours > 24.0m || healthState = SupplierCollaborationHealthState.Unpredictable || healthState = SupplierCollaborationHealthState.Underperforming then
        SupplierCollaborationHealthClassification.NeedsImprovement
    elif otif >= 0.98m && ackHours <= 12.0m && riskCount = 0 && healthState = SupplierCollaborationHealthState.Healthy then
        SupplierCollaborationHealthClassification.Excellent
    elif otif >= 0.95m && ackHours <= 24.0m then
        SupplierCollaborationHealthClassification.Good
    else
        SupplierCollaborationHealthClassification.NeedsImprovement

/// DE-S-042: Classify collaboration health across scope and determine publication readiness
let classifyScopeHealth
    (scopeId: PlanningScopeId)
    (periodId: EvaluationPeriodId)
    (assessments: SupplierCommitmentAssessmentFact list)
    (stateOpt: SupplierCollaborationHealthAssessment option)
    (timestamp: Timestamp)
    (policy: SupplierCollaborationHealthPolicy)
    : SupplierCollaborationHealthAssessment * SupplierCollaborationHealthVersion * int option =

    let totalCount = assessments.Length
    let previousPublishedOpt =
        match stateOpt with
        | Some a -> a.CurrentPublishedVersion
        | None -> None

    let nextVersionNumber =
        match stateOpt with
        | None -> 1
        | Some a ->
            match a.Versions with
            | [] -> 1
            | vs -> (vs |> List.map (fun v -> v.VersionNumber) |> List.max) + 1

    if totalCount = 0 then
        let version: SupplierCollaborationHealthVersion =
            { VersionNumber = nextVersionNumber
              State = HealthVersionLifecycleState.Published
              AssessmentScopeId = scopeId
              EvaluationPeriodId = periodId
              OverallPublicationOutcome = CollaborationHealthPublicationOutcome.Hold "No supplier commitment assessments found for scope"
              SupplierClassifications = []
              ResponsivenessTrends = []
              CommitmentAccuracyTrends = []
              RecommendationSignals = [ "No active suppliers configured for scope" ]
              CompletenessRatio = 0.0m
              AssessmentConfidence = "Low"
              EvidenceReferences = []
              TransactionTimestamp = timestamp
              PublicationTimestamp = Some timestamp }

        let updatedAssessment =
            match stateOpt with
            | None ->
                { AssessmentScopeId = scopeId
                  EvaluationPeriodId = periodId
                  Versions = [ version ]
                  CurrentPublishedVersion = Some nextVersionNumber }
            | Some existing ->
                let updatedVersions =
                    existing.Versions
                    |> List.map (fun v -> if v.State = HealthVersionLifecycleState.Published then { v with State = HealthVersionLifecycleState.Superseded } else v)
                { existing with
                    Versions = version :: updatedVersions
                    CurrentPublishedVersion = Some nextVersionNumber }

        (updatedAssessment, version, previousPublishedOpt)
    else
        let classifications =
            assessments
            |> List.map (fun a -> (a.SupplierId, classifySupplierHealth a))

        let unassessedCount =
            classifications
            |> List.filter (fun (_, c) -> c = SupplierCollaborationHealthClassification.Unassessed)
            |> List.length

        let unassessedRatio = decimal unassessedCount / decimal totalCount
        let completenessRatio = 1.0m - unassessedRatio

        // Determine Publication Outcome
        let publicationOutcome =
            if unassessedRatio > policy.MaxUnassessedSuppliersRatio then
                CollaborationHealthPublicationOutcome.Hold $"Proportion of unassessed suppliers ({Math.Round(unassessedRatio * 100.0m, 1)}%%) exceeds threshold ({Math.Round(policy.MaxUnassessedSuppliersRatio * 100.0m, 1)}%%)"
            elif unassessedCount > 0 then
                CollaborationHealthPublicationOutcome.PublishWithFlags unassessedCount
            else
                CollaborationHealthPublicationOutcome.Publish

        // Recommendation Signals
        let hasAtRisk = classifications |> List.exists (fun (_, c) -> c = SupplierCollaborationHealthClassification.AtRisk)
        let hasCriticallyDep = assessments |> List.exists (fun a -> a.AuthoritativeHealthState = SupplierCollaborationHealthState.CriticallyDependent)
        let recommendationSignals =
            [ if hasAtRisk then "Conduct urgent operational review on suppliers classified as At Risk"
              if hasCriticallyDep then "Initiate dual-sourcing qualification for single-source critically dependent suppliers"
              if unassessedCount > 0 then $"Obtain delivery telemetry to resolve {unassessedCount} unassessed suppliers" ]

        // Trends
        let responsivenessTrends =
            assessments
            |> List.map (fun a ->
                let trend = if a.AverageAcknowledgementHours <= 24.0m then "Stable" else "Deteriorating"
                (a.SupplierId, trend))

        let accuracyTrends =
            assessments
            |> List.map (fun a ->
                let trend = if a.OnTimeDeliveryRate >= 0.95m then "Stable" else "Deteriorating"
                (a.SupplierId, trend))

        let overallConfidence =
            let highConfCount = assessments |> List.filter (fun a -> a.OverallConfidence = "High") |> List.length
            if decimal highConfCount / decimal totalCount >= policy.HighConfidenceThresholdRatio then "High"
            elif completenessRatio >= 0.70m then "Medium"
            else "Low"

        let version: SupplierCollaborationHealthVersion =
            { VersionNumber = nextVersionNumber
              State = HealthVersionLifecycleState.Published
              AssessmentScopeId = scopeId
              EvaluationPeriodId = periodId
              OverallPublicationOutcome = publicationOutcome
              SupplierClassifications = classifications
              ResponsivenessTrends = responsivenessTrends
              CommitmentAccuracyTrends = accuracyTrends
              RecommendationSignals = recommendationSignals
              CompletenessRatio = completenessRatio
              AssessmentConfidence = overallConfidence
              EvidenceReferences = [ $"SE-S-051 x {totalCount}" ]
              TransactionTimestamp = timestamp
              PublicationTimestamp = Some timestamp }

        let updatedAssessment =
            match stateOpt with
            | None ->
                { AssessmentScopeId = scopeId
                  EvaluationPeriodId = periodId
                  Versions = [ version ]
                  CurrentPublishedVersion = Some nextVersionNumber }
            | Some existing ->
                let updatedVersions =
                    existing.Versions
                    |> List.map (fun v -> if v.State = HealthVersionLifecycleState.Published then { v with State = HealthVersionLifecycleState.Superseded } else v)
                { existing with
                    Versions = version :: updatedVersions
                    CurrentPublishedVersion = Some nextVersionNumber }

        (updatedAssessment, version, previousPublishedOpt)
