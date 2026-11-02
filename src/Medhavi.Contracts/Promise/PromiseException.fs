namespace Medhavi.Contracts.Promise

open System
open System.Threading.Tasks
open Medhavi.Contracts

/// Proposed recovery mitigation plan for a disruption
type MitigationActionPlanDto =
    { PlanId: string
      ActionType: string
      EstimatedExpediteCost: decimal
      NewProjectedDeliveryDate: DateTimeOffset
      ProjectedGrossMarginPct: decimal
      Authorized: bool }

/// SE-P-080 Promise Exception Assessment Data Transfer Object
type PromiseExceptionAssessmentDto =
    { ExceptionId: string
      CommitmentId: string
      OrderLineId: string
      DisruptionType: string
      SeverityTier: string
      RootCauseDescription: string
      CandidatePlans: MitigationActionPlanDto list
      SelectedPlanId: string option
      LifecycleState: string
      DecisionTraceability: string option }

/// External request to raise a promise fulfillment disruption incident
type RaisePromiseExceptionReq =
    { ExceptionId: string
      CommitmentId: string
      OrderLineId: string
      DisruptionType: string
      Description: string
      IncidentTime: DateTimeOffset }

/// External request to authorize and execute a disruption mitigation plan
type ResolvePromiseExceptionReq =
    { ExceptionId: string
      SelectedPlanId: string
      AuthorizationTime: DateTimeOffset }

// ---------- Business Notifications ----------

/// BN-P-016: Promise Exception Detected Notification
type PromiseExceptionDetectedNotification =
    { ExceptionId: string
      CommitmentId: string
      OrderLineId: string
      DisruptionType: string
      SeverityTier: string
      NotificationTime: DateTimeOffset }

/// BN-P-017: Resolution Action Plan Authorized Notification
type ResolutionActionPlanAuthorizedNotification =
    { ExceptionId: string
      SelectedPlanId: string
      ActionType: string
      NotificationTime: DateTimeOffset }

/// BN-P-023: Re-Promise Mitigation Proposal Notification
type RePromiseMitigationProposalNotification =
    { ExceptionId: string
      CommitmentId: string
      ProposedDeliveryDate: DateTimeOffset
      NotificationTime: DateTimeOffset }

/// Public API for Promise Exception & Disruption Resolution (CA-P-009)
type PromiseExceptionApi =
    { RaiseException: RaisePromiseExceptionReq -> Task<Result<PromiseExceptionAssessmentDto, ApiError>>
      ResolveException: ResolvePromiseExceptionReq -> Task<Result<PromiseExceptionAssessmentDto, ApiError>> }

/// Query service alias for Promise Exceptions
type PromiseExceptionQueries = QueryService<PromiseExceptionAssessmentDto, string>

