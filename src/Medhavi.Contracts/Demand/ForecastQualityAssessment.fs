namespace Medhavi.Contracts.Demand.ForecastQualityAssessment

open System
open System.Threading.Tasks
open Medhavi.Contracts

// ---------- Read Model ----------
type ForecastQualityAssessment =
    { AssessmentId: string
      PlanningScopeId: string
      EvaluationPeriodStart: DateTimeOffset
      EvaluationPeriodEnd: DateTimeOffset
      Status: string // "Draft" | "Published" | "Superseded"
      Version: int
      WAPE: decimal
      MAPE: decimal
      ForecastBias: decimal
      ForecastAccuracy: decimal
      FVA: decimal option
      ForecastStability: decimal option
      OverrideEffectiveness: decimal option
      OverallQualityScore: decimal option
      SourceForecastPublicationRefs: string list
      SourceDemandHistoryRefs: string list
      ForecastMeasurementPolicyVersionRef: string
      TransactionTime: DateTimeOffset
      PublicationTime: DateTimeOffset option }

// ---------- Request ----------
type EvaluateForecastQualityReq =
    { PlanningScopeId: string
      EvaluationPeriodStart: DateTimeOffset
      EvaluationPeriodEnd: DateTimeOffset
      SourceForecastPublicationRefs: string list
      SourceDemandHistoryRefs: string list
      ForecastMeasurementPolicyVersionRef: string }

// ---------- API ----------
type ForecastQualityApi =
    { Evaluate: EvaluateForecastQualityReq -> Task<Result<string, ApiError>> }

// ---------- Queries ----------
type ForecastQualityAssessmentQueries = QueryService<ForecastQualityAssessment, string>

// ---------- Notifications ----------
type ForecastQualityAssessmentPublishedNotification =
    { AssessmentId: string
      PlanningScopeId: string
      EvaluationPeriodStart: DateTimeOffset
      EvaluationPeriodEnd: DateTimeOffset
      Version: int
      KeyMetricsSummary: string
      OverallQualityScore: decimal option }
