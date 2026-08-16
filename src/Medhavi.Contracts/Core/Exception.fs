namespace Medhavi.Contracts.Core.Exception

open System
open System.Threading.Tasks
open Medhavi.Contracts

/// SE-C-019 Exception Data Transfer Object
type ExceptionDto =
    { ExceptionId: string
      ConstraintReference: string
      Classification: string
      AffectedScopeType: string
      AffectedScopeIdentifier: string
      EvidenceReference: string option
      Severity: string option
      RegistrationTime: DateTimeOffset
      ResolutionTime: DateTimeOffset option
      ResolutionEvidence: string option
      LifecycleState: string }

/// External request carrying exception detection evidence (no surrogate ExceptionId)
type ExceptionEvidenceReq =
    { ConstraintReference: string
      Classification: string
      AffectedScopeType: string
      AffectedScopeIdentifier: string
      EvidenceReference: string option
      Severity: string option
      EvidenceTime: DateTimeOffset }

/// External request to resolve an existing exception
type ResolveExceptionReq =
    { ConstraintReference: string
      AffectedScopeType: string
      AffectedScopeIdentifier: string
      ResolutionTime: DateTimeOffset
      ResolutionEvidence: string }

/// BN-C-002: Enterprise Exception Active Notification
type EnterpriseExceptionActiveNotification =
    { ExceptionId: string
      ConstraintReference: string
      Classification: string
      AffectedScopeType: string
      AffectedScopeIdentifier: string
      Severity: string option
      RegistrationTime: DateTimeOffset }

/// BN-C-003: Enterprise Exception Resolved Notification
type EnterpriseExceptionResolvedNotification =
    { ExceptionId: string
      ResolutionTime: DateTimeOffset
      ResolutionEvidence: string }

/// Public API for Core Exception Management (CA-C-020)
type ExceptionApi =
    { ProcessEvidence: ExceptionEvidenceReq -> Task<Result<ExceptionDto, ApiError>>
      Resolve: ResolveExceptionReq -> Task<Result<ExceptionDto, ApiError>> }

/// Query service alias for Exceptions
type ExceptionQueries = QueryService<ExceptionDto, string>
