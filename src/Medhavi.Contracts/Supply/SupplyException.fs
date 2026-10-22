namespace Medhavi.Contracts.Supply

open System
open System.Threading.Tasks
open Medhavi.Contracts

// =============================================================================
// CA-S-011 & SE-S-110 — Supply Exception Public Contracts
// =============================================================================

[<RequireQualifiedAccess>]
type SupplyExceptionLifecycleState =
    | Detected
    | Confirmed
    | Resolved
    | Archived

[<RequireQualifiedAccess>]
type ExceptionSeverityImpact =
    | Negligible
    | Minor
    | Moderate
    | Major
    | Severe

[<RequireQualifiedAccess>]
type ExceptionSeverityUrgency =
    | Immediate
    | ShortTerm
    | MediumTerm
    | Low

[<RequireQualifiedAccess>]
type ExceptionHorizon =
    | Immediate
    | ShortTerm
    | MediumTerm
    | Future
    | Uncertain

[<RequireQualifiedAccess>]
type ExceptionConfidence =
    | High
    | Medium
    | Low

/// State change audit record appended to SE-S-110 history
type StateChangeEventDto =
    { EventId: string
      PreviousState: string option
      NewState: string
      TriggeringEvidence: string
      Rationale: string
      Timestamp: DateTimeOffset }

/// Multi-dimensional severity vector per PO-S-120
type SeverityVectorDto =
    { BusinessImpact: string
      Urgency: string
      Scope: string
      PotentialImpact: string
      CurrentImpact: string }

/// Authoritative DTO for SE-S-110 Supply Exception Assessment
type SupplyExceptionAssessmentDto =
    { AssessmentId: string
      GovernedPolicyReference: string
      ViolatedRuleId: string
      ExceptionType: string
      ScopeEntityType: string
      ScopeEntityId: string
      PlanningScopeId: string
      LifecycleState: string
      BusinessImpact: string
      Urgency: string
      Scope: string
      PotentialImpact: string
      CurrentImpact: string
      ExceptionHorizon: string
      DetectionTimestamp: DateTimeOffset
      ResolutionTimestamp: DateTimeOffset option
      EvidenceReferences: string list
      EvaluationConfidence: string
      ReasonCodes: string list
      CorrelatedExceptionIds: string list
      History: StateChangeEventDto list
      CreatedTimestamp: DateTimeOffset
      UpdatedTimestamp: DateTimeOffset }

// ---------- Commands / Requests ----------

/// Request payload to evaluate a scope for potential supply exceptions
type EvaluateSupplyExceptionReq =
    { GovernedPolicyReference: string
      ScopeEntityType: string
      ScopeEntityId: string
      PlanningScopeId: string
      ExceptionType: string
      TriggeringEvidence: string list
      EvaluationTimestamp: DateTimeOffset }

// ---------- Business Notifications (Published by CA-S-011) ----------

/// BN-S-100: Supply Exception Assessment Updated (domain-internal notification)
type SupplyExceptionAssessmentUpdatedNotification =
    { ExceptionIdentifier: string
      ExceptionType: string
      GovernedPolicyReference: string
      ExceptionScope: string
      LifecycleState: string
      BusinessImpact: string
      Urgency: string
      ExceptionHorizon: string
      ChangeDescription: string
      PublicationTimestamp: DateTimeOffset }

/// BN-S-101: Supply Exception Detection Evidence (directed to Core CA-C-020)
type SupplyExceptionDetectionEvidenceNotification =
    { EvidenceIdentifier: string
      ConstraintReference: string
      AffectedScopeType: string
      AffectedScopeIdentifier: string
      ExceptionClassification: string
      BusinessImpact: string
      Urgency: string
      TriggeringEvidence: string
      SupplyExceptionIdentifier: string
      PublicationTimestamp: DateTimeOffset }

/// BN-S-102: Supply Exception Resolution Evidence (directed to Core CA-C-020)
type SupplyExceptionResolutionEvidenceNotification =
    { ResolutionIdentifier: string
      ConstraintReference: string
      AffectedScopeType: string
      AffectedScopeIdentifier: string
      ResolutionEvidence: string
      SupplyExceptionIdentifier: string
      ResolutionTimestamp: DateTimeOffset }

// ---------- Public API & Query Aliases ----------

type SupplyExceptionApi =
    { EvaluateException: EvaluateSupplyExceptionReq -> Task<Result<SupplyExceptionAssessmentDto, ApiError>> }

type SupplyExceptionQueries = QueryService<SupplyExceptionAssessmentDto, string>
