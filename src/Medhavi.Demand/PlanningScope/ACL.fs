module Medhavi.Demand.PlanningScope.ACL

open Medhavi.Common.Validation
open Medhavi.Contracts.Demand.PlanningScope
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Failure
open Medhavi.Demand
open Medhavi.Demand.PlanningScope.Model

let toDetermineCmd (req: DeterminePlanningScopeReq) : Validation<DeterminePlanningScopeCmd, DomainError> =
    let scopeId = PlanningScopeId.create(req.SkuId, req.StockingPointId, req.CustomerId, req.PlanningPeriod)

    let make sku sp id cid =
        { ScopeId = id
          SkuId = sku
          StockingPointId = sp
          CustomerId = cid
          PlanningPeriod = req.PlanningPeriod }

    make <!> (SkuId.create req.SkuId |> fromResult)
    <*> (StockingPointId.create req.StockingPointId |> fromResult)
    <*> (scopeId |> fromResult)
    <*> (match req.CustomerId with
         | Some q -> CustomerId.create q |> Result.map Some |> fromResult
         | None -> Valid None)

let toArchiveCmd (req: ArchivePlanningScopeReq) : Validation<ArchivePlanningScopeCmd, DomainError> =
    let make scopeId = { ScopeId = scopeId }
    make <!> (PlanningScopeId.fromString req.ScopeId |> fromResult)
