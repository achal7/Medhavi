module Medhavi.Demand.PlanningScope.Model

open Medhavi.Contracts
open Medhavi.Demand
open Medhavi.SharedKernel

type PlanningScopeStatus =
    | Active
    | Archived

type PlanningScope =
    { Id: PlanningScopeId
      SkuId: SkuId
      StockingPointId: StockingPointId
      CustomerId: CustomerId option
      PlanningPeriod: PlanningPeriod
      Status: PlanningScopeStatus
      TransactionTime: Timestamp }

    member this.AssignmentId = PlanningScopeId.value this.Id

// ---------- Commands ----------

type DeterminePlanningScopeCmd =
    { ScopeId: PlanningScopeId
      SkuId: SkuId
      StockingPointId: StockingPointId
      CustomerId: CustomerId option
      PlanningPeriod: PlanningPeriod }

type ArchivePlanningScopeCmd = { ScopeId: PlanningScopeId }

type PlanningScopeCommand =
    | Determine of DeterminePlanningScopeCmd
    | Archive of ArchivePlanningScopeCmd

    member this.AssignmentId =
        match this with
        | Determine c -> PlanningScopeId.value c.ScopeId
        | Archive c -> PlanningScopeId.value c.ScopeId

// ---------- Events ----------
type PlanningScopeEvent =
    | ScopeDetermined of PlanningScope
    | ScopeArchived of PlanningScopeId

// ---------- Evolve ----------
let evolve (event: PlanningScopeEvent) (state: PlanningScope option) : PlanningScope option =
    match event with
    | ScopeDetermined scope -> Some scope
    | ScopeArchived _ -> state |> Option.map(fun s -> { s with Status = Archived })
