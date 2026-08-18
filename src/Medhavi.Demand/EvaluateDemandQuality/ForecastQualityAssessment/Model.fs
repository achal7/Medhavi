/// SE-D-008 & SE-D-014 — Forecast Quality Assessment Aggregate Model
/// Traces to: Demand Intelligence Specification (SE-D-008, SE-D-014, AB-D-014, FS-D-014, Chapter 4.3.1)
module Medhavi.Demand.EvaluateDemandQuality.ForecastQualityAssessment.Model

open System
open Medhavi.SemanticModel
open Medhavi.Demand
open Medhavi.Foundation.Failure

// ---------- Governed Enums ----------

/// Governed Overall Quality State per PO-D-041 & BA-D-009 (SE-D-014)
type QualityState =
    | Excellent
    | Good
    | Adequate
    | Poor
    | Critical

    member this.AsString =
        match this with
        | Excellent -> "Excellent"
        | Good -> "Good"
        | Adequate -> "Adequate"
        | Poor -> "Poor"
        | Critical -> "Critical"

    static member FromString(s: string) : Result<QualityState, DomainError> =
        match s.Trim().ToLowerInvariant() with
        | "excellent" -> Ok Excellent
        | "good" -> Ok Good
        | "adequate" -> Ok Adequate
        | "poor" -> Ok Poor
        | "critical" -> Ok Critical
        | other -> DomainError.validation $"Unsupported QualityState: '{other}'" |> Error

/// Governed Lifecycle State for Published Knowledge pattern (SE-D-014)
type VersionState =
    | Draft
    | Published
    | Superseded

    member this.AsString =
        match this with
        | Draft -> "Draft"
        | Published -> "Published"
        | Superseded -> "Superseded"

    static member FromString(s: string) : Result<VersionState, DomainError> =
        match s.Trim().ToLowerInvariant() with
        | "draft" -> Ok Draft
        | "published" -> Ok Published
        | "superseded" -> Ok Superseded
        | other -> DomainError.validation $"Unsupported VersionState: '{other}'" |> Error

// ---------- Observation Comparison Entity ----------

/// Individual comparison point between system/final forecast and actual materialized demand
type ObservationComparison =
    { ItemId: ItemId
      LocationId: LocationId
      Period: Timestamp
      SystemForecast: decimal
      FinalForecast: decimal
      ActualDemand: decimal }

// ---------- Comprehensive Forecast Quality Metrics ----------

/// Comprehensive statistical metrics measuring forecast accuracy, bias, FVA, and tracking signals (SE-D-008, BA-D-008)
type ForecastQualityMetrics =
    { /// Weighted Absolute Percentage Error: (Sum |F - A| / Sum A) * 100%
      Wape: decimal
      /// Normalized Forecast Bias: (Sum (F - A) / Sum A) * 100% (+ = Overforecast, - = Underforecast)
      ForecastBias: decimal
      /// Forecast Accuracy: max(0%, 100% - WAPE)
      ForecastAccuracy: decimal
      /// Tracking Signal: Cumulative Forecast Error / Mean Absolute Deviation (CFE / MAD)
      TrackingSignal: decimal option
      /// Whether tracking signal tripped governed control limits (|TS| > threshold, e.g. 4.0)
      IsOutOfControl: bool
      /// Forecast Value Add: WAPE(System Baseline) - WAPE(Final Overridden Forecast)
      ForecastValueAdd: decimal option
      /// Planner Override Effectiveness: % of manual overrides that reduced error
      OverrideEffectiveness: decimal option
      /// Forecast Stability / Churn: 100% - Lag-over-lag cycle variance
      ForecastStability: decimal option
      /// Mean Absolute Percentage Error across non-zero actuals
      Mape: decimal option
      /// Data Completeness Score: % of periods with materialized actuals
      CompletenessScore: decimal }

// ---------- SE-D-014 Forecast Quality Assessment Version ----------

/// Immutable version instance within the Forecast Quality Assessment series
type ForecastQualityAssessmentVersion =
    { VersionNumber: int
      Metrics: ForecastQualityMetrics
      OverallQualityState: QualityState
      PolicyVersion: string
      LifecycleState: VersionState
      CreatedAt: DateTimeOffset
      PublishedAt: DateTimeOffset option
      Rationale: string }

// ---------- SE-D-008 Forecast Quality Assessment Aggregate Root ----------

/// SE-D-008 — Forecast Quality Assessment Aggregate Root
/// Pattern: Published Knowledge (maintains monotonically increasing version series)
/// Identity: Scope + PeriodStart + PeriodEnd (ForecastQualityAssessmentId)
type ForecastQualityAssessment =
    { AssessmentId: ForecastQualityAssessmentId
      Scope: PlanningScopeId
      EvaluationPeriodStart: Timestamp
      EvaluationPeriodEnd: Timestamp
      CurrentPublishedVersion: int option
      Versions: ForecastQualityAssessmentVersion list
      LastUpdated: Timestamp }

// ---------- Commands ----------

/// AB-D-014 Command: Evaluate Forecast Quality from Actuals
type EvaluateForecastQualityCmd =
    { AssessmentId: ForecastQualityAssessmentId
      Scope: PlanningScopeId
      EvaluationPeriodStart: Timestamp
      EvaluationPeriodEnd: Timestamp
      ForecastPublicationId: ForecastPublicationId option
      Observations: ObservationComparison list
      CompletenessScore: decimal
      EvaluationTime: Timestamp }

/// AB-D-014 Command: Publish Forecast Quality Assessment as Authoritative
type PublishForecastQualityAssessmentCmd =
    { AssessmentId: ForecastQualityAssessmentId
      Scope: PlanningScopeId
      EvaluationPeriodStart: Timestamp
      EvaluationPeriodEnd: Timestamp
      VersionNumber: int
      PublicationTime: Timestamp }

// ---------- Enterprise Events ----------

/// Enterprise Events emitted by Forecast Quality Assessment aggregate
type ForecastQualityEvent =
    | ForecastQualityEvaluated of
        Assessment: ForecastQualityAssessment *
        EvaluatedVersion: ForecastQualityAssessmentVersion
    | ForecastQualityAssessmentPublished of
        Assessment: ForecastQualityAssessment *
        PublishedVersion: ForecastQualityAssessmentVersion

// ---------- Pure State Evolution (Layer E: Catamorphism) ----------

let evolve: Medhavi.Foundation.Contracts.Evolve<ForecastQualityAssessment, ForecastQualityEvent> =
    fun (_: ForecastQualityAssessment option) (event: ForecastQualityEvent) ->
        match event with
        | ForecastQualityEvaluated(assessment, _) -> Some assessment
        | ForecastQualityAssessmentPublished(assessment, _) -> Some assessment

/// Replay event sequence to rehydrate aggregate state
let replay (events: ForecastQualityEvent seq) : ForecastQualityAssessment option =
    Seq.fold evolve None events
