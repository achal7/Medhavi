// =============================================================================
// Medhavi.Contracts.Core.UnitOfMeasure
// Traceability: SE‑C‑032 Unit of Measure (UoM) contracts
// Contains: DTO, Commands, Notifications, API gateway record, Queries alias
// =============================================================================
namespace Medhavi.Contracts.Core

open System.Threading.Tasks
open Medhavi.Contracts

// ---------- DTO ----------
// Traceability: SE-C-032 Unit of Measure read-side DTO
type UnitOfMeasure =
    { Id: string
      Name: string
      UnitClassification: string
      State: string }

// ---------- Command Payloads ----------
type CreateUnitOfMeasureReq =
    { Id: string
      Name: string
      UnitClassification: string }

type DeprecateUnitOfMeasureReq = { Id: string; Reason: string }

type RetireUnitOfMeasureReq = { Id: string; Reason: string }

// ---------- Business Notifications ----------
// Traceability: BN-C-070 Unit of Measure notifications
type UnitOfMeasureAdmittedNotification =
    { Id: string
      Name: string
      UnitClassification: string }

type UnitOfMeasureDeprecatedNotification = { Id: string; Reason: string }

type UnitOfMeasureRetiredNotification = { Id: string; Reason: string }

// ---------- API Record & Query Service ----------
type UnitOfMeasureApi =
    { Create: CreateUnitOfMeasureReq -> Task<Result<UnitOfMeasure, ApiError>>
      Deprecate: DeprecateUnitOfMeasureReq -> Task<Result<UnitOfMeasure, ApiError>>
      Retire: RetireUnitOfMeasureReq -> Task<Result<UnitOfMeasure, ApiError>> }

type UnitOfMeasureQueries = QueryService<UnitOfMeasure, string>
