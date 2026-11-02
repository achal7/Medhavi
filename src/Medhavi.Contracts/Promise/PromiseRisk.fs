namespace Medhavi.Contracts.Promise

open System
open System.Threading.Tasks
open Medhavi.Contracts

/// SE-P-060 Promise Risk Assessment Data Transfer Object
type PromiseRiskAssessmentDto =
    { AssessmentId: string
      CommitmentId: string
      OrderLineId: string
      Item: string
      BreachProbabilityPct: decimal
      ProjectedLateDays: decimal
      BufferConsumptionRatio: decimal
      SeverityTier: string
      RiskDrivers: string list
      AssessmentTime: DateTimeOffset
      LifecycleState: string
      DecisionTraceability: string option }

/// External request to evaluate longitudinal risk against telemetry
type AssessCommitmentRiskReq =
    { AssessmentId: string
      CommitmentId: string
      OrderLineId: string
      Item: string
      AssessmentTime: DateTimeOffset }

/// External request to escalate an imminent promise breach to exception triage
type EscalateRiskBreachReq =
    { AssessmentId: string
      EscalationReason: string
      EscalationTime: DateTimeOffset }

// ---------- Business Notifications ----------

/// BN-P-013: Promise at Risk Detected Notification
type PromiseAtRiskDetectedNotification =
    { AssessmentId: string
      CommitmentId: string
      OrderLineId: string
      BreachProbabilityPct: decimal
      ProjectedLateDays: decimal
      SeverityTier: string
      NotificationTime: DateTimeOffset }

/// BN-P-014: Promise Risk Cleared Notification
type PromiseRiskClearedNotification =
    { AssessmentId: string
      CommitmentId: string
      OrderLineId: string
      NotificationTime: DateTimeOffset }

/// Public API for Promise Risk Assessment (CA-P-007)
type PromiseRiskApi =
    { Assess: AssessCommitmentRiskReq -> Task<Result<PromiseRiskAssessmentDto, ApiError>>
      Escalate: EscalateRiskBreachReq -> Task<Result<PromiseRiskAssessmentDto, ApiError>> }

/// Query service alias for Promise Risk Assessments
type PromiseRiskQueries = QueryService<PromiseRiskAssessmentDto, string>

