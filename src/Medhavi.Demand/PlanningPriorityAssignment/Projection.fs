module Medhavi.Demand.PlanningPriorityAssignment.Projection

open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.Demand.PlanningPriorityAssignment.Model
open Medhavi.Contracts.Demand.PlanningPriorityAssignment

type PriorityProjectionState = Map<string, PlanningPriorityAssignment>

let mapToContract (ass: Model.PlanningPriorityAssignment) : PlanningPriorityAssignment =
    { EntityType = ass.EntityType
      EntityId = ass.EntityId
      CurrentPriority = ass.CurrentPriority.AsString()
      PriorityScore = PositiveDecimal.value ass.PriorityScore
      DecisionRationale = ass.DecisionRationale
      BusinessValidity = ass.BusinessValidity
      LastEvaluated = Timestamp.value ass.LastEvaluated
      BusinessTime = Timestamp.value ass.BusinessTime
      TransactionTime = Timestamp.value ass.TransactionTime }

let evolveProjection (state: PriorityProjectionState) (evt: PlanningPriorityEvent) =
    match evt with
    | PlanningPriorityUpdated(ass, _) ->
        Map.add ass.AssignmentId (mapToContract ass) state

type PriorityAgent = ProjectionAgent<PriorityProjectionState, PlanningPriorityEvent>

let createProjectionAgent () = ProjectionAgent(evolveProjection, Map.empty, "PlanningPriorityReadModel")

let createQueryService (agent: PriorityAgent) : PlanningPriorityQueries =
    QueryServiceBase.getQueryService agent id

let seedProjections (agent: PriorityAgent) (list: Model.PlanningPriorityAssignment list) =
    let m = list |> List.map(fun a -> a.AssignmentId, mapToContract a) |> Map.ofList
    agent.SetState m
