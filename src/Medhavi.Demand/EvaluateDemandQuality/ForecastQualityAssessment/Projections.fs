/// SE-D-008 — Forecast Quality Assessment Read Model Projections
/// Pure Functional Projection Fold (Layer E: Catamorphism)
module Medhavi.Demand.EvaluateDemandQuality.ForecastQualityAssessment.Projections

open Medhavi.SemanticModel
open Medhavi.Demand
open Medhavi.Contracts.Demand
open Model

let mapMetricsToDto (metrics: ForecastQualityMetrics) : ForecastQualityMetricsDto =
    { Wape = metrics.Wape
      ForecastBias = metrics.ForecastBias
      ForecastAccuracy = metrics.ForecastAccuracy
      TrackingSignal = metrics.TrackingSignal
      IsOutOfControl = metrics.IsOutOfControl
      ForecastValueAdd = metrics.ForecastValueAdd
      OverrideEffectiveness = metrics.OverrideEffectiveness
      ForecastStability = metrics.ForecastStability
      Mape = metrics.Mape
      CompletenessScore = metrics.CompletenessScore }

let mapVersionToDto (version: ForecastQualityAssessmentVersion) : ForecastQualityAssessmentVersionDto =
    { VersionNumber = version.VersionNumber
      Metrics = mapMetricsToDto version.Metrics
      OverallQualityState = version.OverallQualityState.AsString
      PolicyVersion = version.PolicyVersion
      LifecycleState = version.LifecycleState.AsString
      CreatedAt = version.CreatedAt
      PublishedAt = version.PublishedAt
      Rationale = version.Rationale }

let mapToDto (assessment: ForecastQualityAssessment) : ForecastQualityAssessmentDto =
    let latestVersion =
        assessment.Versions
        |> List.sortByDescending (fun v -> v.VersionNumber)
        |> List.tryHead

    let latestQualityState =
        latestVersion
        |> Option.map (fun v -> v.OverallQualityState.AsString)
        |> Option.defaultValue "Unknown"

    let latestMetrics =
        latestVersion
        |> Option.map (fun v -> mapMetricsToDto v.Metrics)
        |> Option.defaultValue
            { Wape = 0.0m
              ForecastBias = 0.0m
              ForecastAccuracy = 0.0m
              TrackingSignal = None
              IsOutOfControl = false
              ForecastValueAdd = None
              OverrideEffectiveness = None
              ForecastStability = None
              Mape = None
              CompletenessScore = 0.0m }

    { AssessmentId = ForecastQualityAssessmentId.value assessment.AssessmentId
      ScopeId = PlanningScopeId.value assessment.Scope
      EvaluationPeriodStart = Timestamp.value assessment.EvaluationPeriodStart
      EvaluationPeriodEnd = Timestamp.value assessment.EvaluationPeriodEnd
      CurrentPublishedVersion = assessment.CurrentPublishedVersion
      LatestQualityState = latestQualityState
      LatestMetrics = latestMetrics
      Versions = assessment.Versions |> List.map mapVersionToDto
      LastUpdated = Timestamp.value assessment.LastUpdated }

/// Projection state: Map of ForecastQualityAssessmentId to DTO
type State = Map<ForecastQualityAssessmentId, ForecastQualityAssessmentDto>

let initial: State = Map.empty

/// Pure projection fold (Layer E: Catamorphism)
let apply (state: State) (event: ForecastQualityEvent) : State =
    match event with
    | ForecastQualityEvaluated(assessment, _)
    | ForecastQualityAssessmentPublished(assessment, _) ->
        let dto = mapToDto assessment
        Map.add assessment.AssessmentId dto state

/// Seed projection from existing aggregates
let seedFromAggregates (aggregates: ForecastQualityAssessment list) : State =
    aggregates
    |> List.fold
        (fun state assessment ->
            let dto = mapToDto assessment
            Map.add assessment.AssessmentId dto state)
        initial
