// =============================================================================
// Medhavi.Contracts.Core.PlanningScope
// Traceability: SE‑C‑010 Planning Scope contracts
// Contains: DTO, Commands, Notifications, API gateway record, Queries alias
// =============================================================================
namespace Medhavi.Contracts.Core

open System.Threading.Tasks
open Medhavi.Contracts

type PlanningScope =
    { Id: string
      Name: string
      BoundaryDefinition: string
      State: string }

type CreatePlanningScopeReq =
    { Id: string
      Name: string
      BoundaryDefinition: string }

type ActivatePlanningScopeReq = { Id: string }
type InactivatePlanningScopeReq = { Id: string; Reason: string }
type RetirePlanningScopeReq = { Id: string; Reason: string }

type PlanningScopeCreatedNotification =
    { Id: string
      Name: string
      BoundaryDefinition: string }

type PlanningScopeActivatedNotification = { Id: string }
type PlanningScopeInactivatedNotification = { Id: string; Reason: string }
type PlanningScopeRetiredNotification = { Id: string; Reason: string }

type PlanningScopeApi =
    { Create: CreatePlanningScopeReq -> Task<Result<PlanningScope, ApiError>>
      Activate: ActivatePlanningScopeReq -> Task<Result<PlanningScope, ApiError>>
      Inactivate: InactivatePlanningScopeReq -> Task<Result<PlanningScope, ApiError>>
      Retire: RetirePlanningScopeReq -> Task<Result<PlanningScope, ApiError>> }

type PlanningScopeQueries = QueryService<PlanningScope, string>
