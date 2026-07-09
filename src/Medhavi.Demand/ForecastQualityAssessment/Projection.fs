module Medhavi.Demand.ForecastQualityAssessment.Projection

open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.Demand.ForecastQualityAssessment
open Medhavi.Contracts.Demand.ForecastQualityAssessment

type AssessmentProjectionState = Map<string, ForecastQualityAssessment>

let mapToContract (ass: Model.ForecastQualityAssessment) : ForecastQualityAssessment =
    { AssessmentId = ForecastQualityAssessmentId.value ass.Id
      PlanningScopeId = PlanningScopeId.value ass.PlanningScopeId
      EvaluationPeriodStart = Timestamp.value ass.EvaluationPeriodStart
      EvaluationPeriodEnd = Timestamp.value ass.EvaluationPeriodEnd
      Status =
        match ass.Status with
        | Model.Draft -> "Draft"
        | Model.Published -> "Published"
        | Model.Superseded -> "Superseded"
      Version = ass.Version
      WAPE = ass.CoreMetrics.WAPE
      MAPE = ass.CoreMetrics.MAPE
      ForecastBias = ass.CoreMetrics.ForecastBias
      ForecastAccuracy = ass.CoreMetrics.ForecastAccuracy
      FVA = ass.OptionalMetrics.FVA
      ForecastStability = ass.OptionalMetrics.ForecastStability
      OverrideEffectiveness = ass.OptionalMetrics.OverrideEffectiveness
      OverallQualityScore = ass.OverallQualityScore |> Option.map PositiveDecimal.value
      SourceForecastPublicationRefs = ass.SourceForecastPublicationRefs
      SourceDemandHistoryRefs = ass.SourceDemandHistoryRefs
      ForecastMeasurementPolicyVersionRef = ass.ForecastMeasurementPolicyVersionRef
      TransactionTime = Timestamp.value ass.TransactionTime
      PublicationTime = ass.PublicationTime |> Option.map Timestamp.value }

let evolveProjection (state: AssessmentProjectionState) (evt: Model.ForecastQualityAssessmentEvent) =
    match evt with
    | Model.ForecastQualityAssessed ass -> Map.add ass.AssignmentId (mapToContract ass) state

type AssessmentAgent = ProjectionAgent<AssessmentProjectionState, Model.ForecastQualityAssessmentEvent>

let createProjectionAgent () = ProjectionAgent(evolveProjection, Map.empty, "ForecastQualityAssessmentReadModel")

let createQueryService (agent: AssessmentAgent) : ForecastQualityAssessmentQueries =
    QueryServiceBase.getQueryService agent id

let seedProjections (agent: AssessmentAgent) (list: Model.ForecastQualityAssessment list) =
    let m = list |> List.map(fun a -> a.AssignmentId, mapToContract a) |> Map.ofList
    agent.SetState m
