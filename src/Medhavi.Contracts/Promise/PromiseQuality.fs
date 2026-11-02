namespace Medhavi.Contracts.Promise

open System
open System.Threading.Tasks
open Medhavi.Contracts

/// SE-P-070 Promise Quality Assessment Data Transfer Object
type PromiseQualityAssessmentDto =
    { AssessmentId: string
      CommitmentId: string
      OrderLineId: string
      RevisionZeroCommittedDate: DateTimeOffset
      ActualDeliveryDate: DateTimeOffset
      PromisedQuantity: decimal
      DeliveredQuantity: decimal
      IsOnTime: bool
      IsInFull: bool
      OTIFPassed: bool
      LateDays: decimal
      AttributedFailureCategory: string option
      LiquidatedDamagesAccrued: decimal
      AssessmentTime: DateTimeOffset
      LifecycleState: string
      DecisionTraceability: string option }

/// Performance scorecard aggregating promise service metrics for a planning period
type PromiseScorecardDto =
    { ScorecardId: string
      PlanningScopeId: string
      EvaluationPeriod: string
      TotalCommitmentsAudited: int
      OTIFRatePct: decimal
      FirstPromiseAccuracyPct: decimal
      OrderLineFillRatePct: decimal
      AveragePromisingCycleSeconds: decimal
      PublishedAt: DateTimeOffset }

/// External request to audit delivery proof of delivery against initial commitment
type AuditCommitmentQualityReq =
    { AssessmentId: string
      CommitmentId: string
      ProofOfDeliveryDate: DateTimeOffset
      DeliveredQuantity: decimal
      AuditTime: DateTimeOffset }

/// External request to log a customer fulfillment dispute
type RecordPromiseDisputeReq =
    { AssessmentId: string
      DisputeReason: string
      ClaimedLossAmount: decimal
      DisputeTime: DateTimeOffset }

// ---------- Business Notifications ----------

/// BN-P-018: Promise Service Level Breached Notification
type PromiseServiceLevelBreachedNotification =
    { AssessmentId: string
      CommitmentId: string
      CustomerId: string
      LateDays: decimal
      LiquidatedDamagesAccrued: decimal
      NotificationTime: DateTimeOffset }

/// BN-P-019: Promise Performance Scorecard Published Notification
type PromisePerformanceScorecardPublishedNotification =
    { ScorecardId: string
      PlanningScopeId: string
      EvaluationPeriod: string
      OTIFRatePct: decimal
      FirstPromiseAccuracyPct: decimal
      NotificationTime: DateTimeOffset }

/// Public API for Promise Performance & Quality (CA-P-010)
type PromiseQualityApi =
    { AuditQuality: AuditCommitmentQualityReq -> Task<Result<PromiseQualityAssessmentDto, ApiError>>
      RecordDispute: RecordPromiseDisputeReq -> Task<Result<PromiseQualityAssessmentDto, ApiError>> }

/// Query service alias for Promise Quality Assessments
type PromiseQualityQueries = QueryService<PromiseQualityAssessmentDto, string>

