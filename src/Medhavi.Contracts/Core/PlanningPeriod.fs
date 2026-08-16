// =============================================================================
// Medhavi.Contracts.Core.PlanningPeriod
// Traceability: SE‑C‑034 Planning Period contracts
// Contains: DTO, Commands, Notifications, API gateway record, Queries alias
// =============================================================================
namespace Medhavi.Contracts.Core

open System.Threading.Tasks
open Medhavi.Contracts

type PlanningPeriod =
    { Id: string
      Name: string
      State: string }

type CreatePlanningPeriodReq = { Id: string; Name: string }
type DeprecatePlanningPeriodReq = { Id: string; Reason: string }
type RetirePlanningPeriodReq = { Id: string; Reason: string }

type PlanningPeriodCreatedNotification = { Id: string; Name: string }
type PlanningPeriodDeprecatedNotification = { Id: string; Reason: string }
type PlanningPeriodRetiredNotification = { Id: string; Reason: string }

type PlanningPeriodApi =
    { Create: CreatePlanningPeriodReq -> Task<Result<PlanningPeriod, ApiError>>
      Deprecate: DeprecatePlanningPeriodReq -> Task<Result<PlanningPeriod, ApiError>>
      Retire: RetirePlanningPeriodReq -> Task<Result<PlanningPeriod, ApiError>> }

type PlanningPeriodQueries = QueryService<PlanningPeriod, string>
