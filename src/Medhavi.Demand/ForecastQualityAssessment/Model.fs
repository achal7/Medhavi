module Medhavi.Demand.ForecastQualityAssessment.Model

open Medhavi.SharedKernel
open Medhavi.Demand

// =============================================================================
// SE‑D‑039 — Forecast Quality Assessment
// =============================================================================

type CoreMetrics =
    { WAPE: decimal
      MAPE: decimal
      ForecastBias: decimal
      ForecastAccuracy: decimal }

type OptionalMetrics =
    { FVA: decimal option
      ForecastStability: decimal option
      OverrideEffectiveness: decimal option }

type AssessmentStatus =
    | Draft
    | Published
    | Superseded

type ForecastQualityAssessment =
    { Id: ForecastQualityAssessmentId
      PlanningScopeId: PlanningScopeId
      EvaluationPeriodStart: Timestamp
      EvaluationPeriodEnd: Timestamp
      Status: AssessmentStatus
      Version: int
      CoreMetrics: CoreMetrics
      OptionalMetrics: OptionalMetrics
      OverallQualityScore: PositiveDecimal option
      SourceForecastPublicationRefs: string list
      SourceDemandHistoryRefs: string list
      ForecastMeasurementPolicyVersionRef: string
      TransactionTime: Timestamp
      PublicationTime: Timestamp option
      SupersededAssessmentId: string option }

    member this.AssignmentId = ForecastQualityAssessmentId.value this.Id

// ---------- Commands ----------
type EvaluateForecastQualityCmd =
    { PlanningScopeId: PlanningScopeId
      AssessmentId: ForecastQualityAssessmentId
      EvaluationPeriodStart: Timestamp
      EvaluationPeriodEnd: Timestamp
      CoreMetrics: CoreMetrics
      OptionalMetrics: OptionalMetrics
      OverallQualityScore: PositiveDecimal option
      ActualDataCount: int
      ExpectedDataCount: int
      CompletenessThreshold: decimal
      MinEvaluationPeriodDays: int
      SourceForecastPublicationRefs: string list
      SourceDemandHistoryRefs: string list
      ForecastMeasurementPolicyVersionRef: string
      PublicationTime: Timestamp }

type ForecastQualityAssessmentCommand =
    | Evaluate of EvaluateForecastQualityCmd

    member this.AssignmentId =
        match this with
        | Evaluate c -> ForecastQualityAssessmentId.value c.AssessmentId

// ---------- Events ----------
type ForecastQualityAssessmentEvent = ForecastQualityAssessed of ForecastQualityAssessment

// ---------- Evolve ----------
let evolve
    (evt: ForecastQualityAssessmentEvent)
    (stateOpt: ForecastQualityAssessment option)
    : ForecastQualityAssessment option =
    match evt with
    | ForecastQualityAssessed ass -> Some ass
