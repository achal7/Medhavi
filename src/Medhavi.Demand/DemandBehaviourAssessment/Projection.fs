module Medhavi.Demand.DemandBehaviourAssessment.Projection

open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel
open Medhavi.Demand.DemandBehaviourAssessment.Model
open Medhavi.Contracts.Demand.SenseDemand

type AssessmentProjectionState = Map<string, DemandBehaviourAssessment>

let mapToContract (ass: Model.DemandBehaviourAssessment) : DemandBehaviourAssessment =
    { SkuId = SkuId.value ass.SkuId
      StockingPointId = StockingPointId.value ass.StockingPointId
      CurrentState = ass.CurrentState.AsString()
      LastDeviation = ass.CurrentDeviation |> Option.map PositiveDecimal.value |> Option.defaultValue 0m
      Confidence = PositiveDecimal.value ass.Confidence
      BaselineReference = ass.BaselineReference
      BusinessTime = Timestamp.value ass.BusinessTime
      TransactionTime = Timestamp.value ass.TransactionTime }

let evolveProjection (state: AssessmentProjectionState) (evt: DemandBehaviourAssessmentEvent) =
    match evt with
    | BehaviourStateChanged(ass, _) -> Map.add ass.AssignmentId (mapToContract ass) state
    | BehaviourAssessmentAcknowledged _ -> state

type AssessmentAgent = ProjectionAgent<AssessmentProjectionState, DemandBehaviourAssessmentEvent>

let createProjectionAgent () = ProjectionAgent(evolveProjection, Map.empty, "DemandBehaviourAssessmentReadModel")

let createQueryService (agent: AssessmentAgent) : SenseDemandQueries = QueryServiceBase.getQueryService agent id

let seedProjections (agent: AssessmentAgent) (list: Model.DemandBehaviourAssessment list) =
    let m = list |> List.map(fun ass -> ass.AssignmentId, mapToContract ass) |> Map.ofList

    agent.SetState m
