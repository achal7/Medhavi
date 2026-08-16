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
      EvidenceReference: string
      Severity: string option
      RegistrationTime: DateTimeOffset
      ResolutionTime: DateTimeOffset option
      ResolutionEvidence: string option
      LifecycleState: string }

/// External request to register a new exception
type RegisterExceptionReq =
    { ExceptionId: string
      ConstraintReference: string
      Classification: string
      AffectedScopeType: string
      AffectedScopeIdentifier: string
      EvidenceReference: string
      Severity: string option
      RegistrationTime: DateTimeOffset }

/// External request to resolve an existing exception
type ResolveExceptionReq =
    { ExceptionId: string
      ResolutionTime: DateTimeOffset
      ResolutionEvidence: string }

/// BN-C-020a: Exception Registered Notification
type ExceptionRegisteredNotification =
    { ExceptionId: string
      ConstraintReference: string
      Classification: string
      AffectedScopeIdentifier: string
      Severity: string option
      RegistrationTime: DateTimeOffset }

/// BN-C-020b: Exception Resolved Notification
type ExceptionResolvedNotification =
    { ExceptionId: string
      ResolutionTime: DateTimeOffset
      ResolutionEvidence: string }


/// Public API for Core Exception Management (CA-C-020)
type ExceptionApi =
    { Register: RegisterExceptionReq -> Task<Result<ExceptionDto, ApiError>>
      Resolve: ResolveExceptionReq -> Task<Result<ExceptionDto, ApiError>> }

/// Query service alias for Exceptions
type ExceptionQueries = QueryService<ExceptionDto, string>
