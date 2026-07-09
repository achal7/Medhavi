module Medhavi.Demand.DemandExplanation.Projection

open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.Demand.DemandExplanation.Model
open Medhavi.Contracts.Demand.DemandLearning

type ExplanationProjectionState = Map<string, DemandExplanation>

let mapToContract (exp: Model.DemandExplanation) : DemandExplanation = {
    ExplanationId                   = DemandExplanationId.value exp.Id
    ExplainedArtifactType           = exp.ExplainedArtifactType
    ExplainedArtifactId             = exp.ExplainedArtifactId
    NaturalLanguageExplanation      = exp.NaturalLanguageExplanation
    ExplanationGenerationTimestamp  = Timestamp.value exp.ExplanationGenerationTimestamp
    TemplateVersionRef              = exp.TemplateVersionRef
    BusinessTime                    = Timestamp.value exp.BusinessTime
    TransactionTime                 = Timestamp.value exp.TransactionTime
}

let evolveProjection (state: ExplanationProjectionState) (evt: DemandExplanationEvent) =
    match evt with
    | DemandExplanationRecorded exp ->
        Map.add (DemandExplanationId.value exp.Id) (mapToContract exp) state

type ExplanationAgent = ProjectionAgent<ExplanationProjectionState, DemandExplanationEvent>

let createProjectionAgent () = ProjectionAgent(evolveProjection, Map.empty, "DemandExplanationReadModel")

let createQueryService (agent: ExplanationAgent) : DemandExplanationQueries =
   QueryServiceBase.getQueryService agent id

let seedProjections (agent: ExplanationAgent) (list: Model.DemandExplanation list) =
    let m =
        list |> List.map (fun e -> DemandExplanationId.value e.Id, mapToContract e) |> Map.ofList
    agent.SetState m
