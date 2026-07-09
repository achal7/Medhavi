module Medhavi.Demand.DemandLearning.Projection

open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.Demand.DemandLearning.Model
open Medhavi.Contracts.Demand.DemandLearning

type LearningProjectionState = Map<string, DemandLearning>

let mapToContract (learning: Model.DemandLearning) : DemandLearning =
    { LearningId = DemandLearningId.value learning.Id
      LearningType = learning.LearningType
      LearningStatement = learning.LearningStatement
      SupportingEvidence = learning.SupportingEvidence
      EvidenceStrength = learning.EvidenceStrength
      SourceAnalysisRef = learning.SourceAnalysisRef
      BusinessTime = Timestamp.value learning.BusinessTime
      TransactionTime = Timestamp.value learning.TransactionTime }

let evolveProjection (state: LearningProjectionState) (evt: DemandLearningEvent) =
    match evt with
    | DemandLearningRecorded learning -> Map.add (DemandLearningId.value learning.Id) (mapToContract learning) state

type LearningAgent = ProjectionAgent<LearningProjectionState, DemandLearningEvent>

let createProjectionAgent () = ProjectionAgent(evolveProjection, Map.empty, "DemandLearningReadModel")

let createQueryService (agent: LearningAgent) : DemandLearningQueries = QueryServiceBase.getQueryService agent id

let seedProjections (agent: LearningAgent) (list: Model.DemandLearning list) =
    let m = list |> List.map(fun l -> DemandLearningId.value l.Id, mapToContract l) |> Map.ofList
    agent.SetState m
