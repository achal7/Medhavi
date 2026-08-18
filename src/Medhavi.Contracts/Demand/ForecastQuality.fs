namespace Medhavi.Contracts.Demand

open System
open System.Threading.Tasks
open Medhavi.Contracts

// =============================================================================
// SE-D-008 & SE-D-014 — Forecast Quality Assessment Public Contracts
// =============================================================================

/// Comprehensive statistical metrics measuring forecast accuracy, bias, FVA, and tracking signals (SE-D-008, BA-D-008)
type ForecastQualityMetricsDto =
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

/// Single immutable version of a Forecast Quality Assessment (SE-D-014)
type ForecastQualityAssessmentVersionDto =
    { VersionNumber: int
      Metrics: ForecastQualityMetricsDto
      OverallQualityState: string
      PolicyVersion: string
      LifecycleState: string
      CreatedAt: DateTimeOffset
      PublishedAt: DateTimeOffset option
      Rationale: string }

/// Authoritative aggregate read model DTO for Forecast Quality Assessment (SE-D-008)
type ForecastQualityAssessmentDto =
    { AssessmentId: string
      ScopeId: string
      EvaluationPeriodStart: DateTimeOffset
      EvaluationPeriodEnd: DateTimeOffset
      CurrentPublishedVersion: int option
      LatestQualityState: string
      LatestMetrics: ForecastQualityMetricsDto
      Versions: ForecastQualityAssessmentVersionDto list
      LastUpdated: DateTimeOffset }

// ---------- Observation Pair Payload ----------

/// Individual time-series observation comparison point for accuracy computation
type ObservationPairDto =
    { ItemId: string
      LocationId: string
      Period: DateTimeOffset
      SystemForecast: decimal
      FinalForecast: decimal
      ActualDemand: decimal }

// ---------- Commands / Requests ----------

type EvaluateForecastQualityReq =
    { ScopeId: string
      EvaluationPeriodStart: DateTimeOffset
      EvaluationPeriodEnd: DateTimeOffset
      ForecastPublicationId: string option
      Observations: ObservationPairDto list
      CompletenessScore: decimal }

type PublishForecastQualityAssessmentReq =
    { ScopeId: string
      EvaluationPeriodStart: DateTimeOffset
      EvaluationPeriodEnd: DateTimeOffset
      VersionNumber: int }

// ---------- API Record ----------

type ForecastQualityApi =
    { EvaluateQuality: EvaluateForecastQualityReq -> Task<Result<ForecastQualityAssessmentDto, ApiError>>
      PublishAssessment: PublishForecastQualityAssessmentReq -> Task<Result<ForecastQualityAssessmentDto, ApiError>> }

/// Query service alias
type ForecastQualityQueries = QueryService<ForecastQualityAssessmentDto, string>
