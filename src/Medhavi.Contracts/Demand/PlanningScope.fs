module Medhavi.Contracts.Demand.PlanningScope

open System
open System.Threading.Tasks
open Medhavi.Contracts

type PlanningScope =
    { ScopeId: string
      SkuId: string
      StockingPointId: string
      CustomerId: string option
      PlanningPeriod: PlanningPeriod
      Status: string }

type DeterminePlanningScopeReq =
    { SkuId: string
      StockingPointId: string
      CustomerId: string option
      PlanningPeriod: PlanningPeriod
      BucketType: string }

type ArchivePlanningScopeReq = { ScopeId: string }

type PlanningScopeQueries = QueryService<PlanningScope, string>

type PlanningScopeApi =
    { Determine: DeterminePlanningScopeReq -> Task<Result<string, ApiError>>
      Archive: ArchivePlanningScopeReq -> Task<Result<unit, ApiError>> }
