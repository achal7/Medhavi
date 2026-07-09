module Medhavi.Demand.DemandBehaviourAssignment.Projection

open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel
open Medhavi.Demand.DemandBehaviourAssignment.Model
open Medhavi.Contracts.Demand.DemandBehaviourAssignment

type BehaviourProjectionState = Map<string, DemandBehaviourAssignment>

let mapToContract (ass: Model.DemandBehaviourAssignment) : DemandBehaviourAssignment =
    { EntityType = ass.EntityType
      EntityId = ass.EntityId
      BehaviourDimension = ass.BehaviourDimension
      CurrentClassification = ass.CurrentClassification
      ClassificationConfidence = PositiveDecimal.value ass.ClassificationConfidence
      EvidenceSummary = ass.EvidenceSummary
      LastClassified = Timestamp.value ass.LastClassified
      BusinessTime = Timestamp.value ass.BusinessTime
      TransactionTime = Timestamp.value ass.TransactionTime }

let evolveProjection (state: BehaviourProjectionState) (evt: DemandBehaviourAssignmentEvent) =
    match evt with
    | DemandBehaviourClassificationUpdated(ass, _) ->
        Map.add ass.AssignmentId (mapToContract ass) state

type BehaviourAgent = ProjectionAgent<BehaviourProjectionState, DemandBehaviourAssignmentEvent>

let createProjectionAgent () = ProjectionAgent(evolveProjection, Map.empty, "DemandBehaviourAssignmentReadModel")

let createQueryService (agent: BehaviourAgent) : DemandBehaviourAssignmentQueries =
    QueryServiceBase.getQueryService agent id

let seedProjections (agent: BehaviourAgent) (list: Model.DemandBehaviourAssignment list) =
    let m =
        list |> List.map(fun a -> a.AssignmentId, mapToContract a) |> Map.ofList

    agent.SetState m
