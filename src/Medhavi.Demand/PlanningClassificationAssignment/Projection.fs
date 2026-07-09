module Medhavi.Demand.PlanningClassificationAssignment.Projection

open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.Demand.PlanningClassificationAssignment.Model
open Medhavi.Contracts.Demand.PlanningClassificationAssignment

type ClassificationProjectionState = Map<string, PlanningClassificationAssignment>

let mapToContract (ass: Model.PlanningClassificationAssignment) : PlanningClassificationAssignment =
    { EntityType = ass.EntityType
      EntityId = ass.EntityId
      ClassificationType = ass.ClassificationType.AsString()
      CurrentClassification = ass.CurrentClassification
      ClassificationConfidence = PositiveDecimal.value ass.ClassificationConfidence
      LastClassified = Timestamp.value ass.LastClassified
      BusinessTime = Timestamp.value ass.BusinessTime
      TransactionTime = Timestamp.value ass.TransactionTime }

let evolveProjection (state: ClassificationProjectionState) (evt: PlanningClassificationEvent) =
    match evt with
    | PlanningClassificationUpdated(ass, _) ->
        Map.add ass.AssignmentId (mapToContract ass) state

type ClassificationAgent = ProjectionAgent<ClassificationProjectionState, PlanningClassificationEvent>

let createProjectionAgent () = ProjectionAgent(evolveProjection, Map.empty, "PlanningClassificationReadModel")

let createQueryService (agent: ClassificationAgent) : PlanningClassificationQueries =
    QueryServiceBase.getQueryService agent id

let seedProjections (agent: ClassificationAgent) (list: Model.PlanningClassificationAssignment list) =
    let m =
        list
        |> List.map(fun ass -> ass.AssignmentId, mapToContract ass)
        |> Map.ofList

    agent.SetState m
