namespace Medhavi.Scheduler.Planning.Application

open Medhavi.Contracts.Scenario
open Medhavi.SharedKernel
open Medhavi.Scheduler.Planning.Domain

/// Pluggable solver strategy function signature — MRP heuristic today, optimizer tomorrow.
type SolvePlan = ScenarioId -> PlanningMode -> PlanningInputData -> PlanRunHorizon -> Async<Result<PlanningResult, DomainError list>>
