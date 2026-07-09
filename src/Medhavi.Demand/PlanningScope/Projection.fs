module Medhavi.Demand.PlanningScope.Projection

open Medhavi.SharedKernel
open Medhavi.Infrastructure.Projections
open Medhavi.Demand
open Medhavi.Demand.PlanningScope.Model
open Medhavi.Contracts.Demand.PlanningScope

type PlanningScopeProjectionState = Map<string, PlanningScope>

let mapToDTO (scope: Model.PlanningScope) : PlanningScope =
    { ScopeId = PlanningScopeId.value scope.Id
      SkuId = SkuId.value scope.SkuId
      StockingPointId = StockingPointId.value scope.StockingPointId
      CustomerId = scope.CustomerId |> Option.map CustomerId.value
      PlanningPeriod = scope.PlanningPeriod
      Status =
        match scope.Status with
        | PlanningScopeStatus.Active -> "Active"
        | PlanningScopeStatus.Archived -> "Archived" }

let evolveProjection (state: PlanningScopeProjectionState) (evt: PlanningScopeEvent) =
    match evt with
    | ScopeDetermined scope -> Map.add (PlanningScopeId.value scope.Id) (mapToDTO scope) state
    | ScopeArchived scopeId ->
        state |> Map.change (PlanningScopeId.value scopeId) (Option.map(fun s -> { s with Status = "Archived" }))

type PlanningScopeAgent = ProjectionAgent<PlanningScopeProjectionState, PlanningScopeEvent>

let createProjectionAgent () = ProjectionAgent(evolveProjection, Map.empty, "PlanningScopeReadModel")

let createQueryService (agent: PlanningScopeAgent) = QueryServiceBase.getQueryService agent id

let seedProjections (agent: PlanningScopeAgent) (list: Model.PlanningScope list) =
    let m = list |> List.map(mapToDTO >> (fun d -> d.ScopeId, d)) |> Map.ofList
    agent.SetState m
