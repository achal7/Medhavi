module Medhavi.Demand.DemandPlanningCondition.Projection

open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.Demand.DemandPlanningCondition
open Medhavi.Contracts.Demand.DemandLearning

type ConditionProjectionState = Map<string, DemandPlanningCondition>

let mapToContract (cond: Model.DemandPlanningCondition) : DemandPlanningCondition =
    { ConditionId = DemandPlanningConditionId.value cond.Id
      PlanningEntity = cond.PlanningEntity
      ConditionType = cond.ConditionType
      CurrentStatus = cond.CurrentStatus.ToString()
      Severity = cond.Severity.ToString()
      DetectionEvidence = cond.DetectionEvidence
      DetectionTimestamp = Timestamp.value cond.DetectionTimestamp
      ResolutionTimestamp = cond.ResolutionTimestamp |> Option.map Timestamp.value
      ResolutionEvidence = cond.ResolutionEvidence
      BusinessTime = Timestamp.value cond.BusinessTime
      TransactionTime = Timestamp.value cond.TransactionTime }

let evolveProjection (state: ConditionProjectionState) (evt:Model. DemandPlanningConditionEvent) =
    match evt with
    | Model.ConditionRecognized(cond, _) -> Map.add (DemandPlanningConditionId.value cond.Id) (mapToContract cond) state
    | Model.ConditionResolved(cond, _) -> Map.add (DemandPlanningConditionId.value cond.Id) (mapToContract cond) state

type ConditionAgent = ProjectionAgent<ConditionProjectionState, Model.DemandPlanningConditionEvent>

let createProjectionAgent () = ProjectionAgent(evolveProjection, Map.empty, "DemandPlanningConditionReadModel")

let createQueryService (agent: ConditionAgent) = QueryServiceBase.getQueryService agent id

let seedProjections (agent: ConditionAgent) (list: Model.DemandPlanningCondition list) =
    let m = list |> List.map(fun c -> DemandPlanningConditionId.value c.Id, mapToContract c) |> Map.ofList
    agent.SetState m
