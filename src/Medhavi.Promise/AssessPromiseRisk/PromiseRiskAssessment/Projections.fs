/// CA-P-007 Assess Promise Risk and Resilience — Projections
module Medhavi.Promise.AssessPromiseRisk.PromiseRiskAssessment.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let mapToDto (a: PromiseRiskAssessment) : PromiseRiskAssessmentDto =
    { AssessmentId = PromiseRiskAssessmentId.value a.AssessmentId
      CommitmentId = OrderCommitmentId.value a.CommitmentId
      OrderLineId = OrderLineId.value a.OrderLineId
      Item = ItemId.value a.Item
      BreachProbabilityPct = a.BreachProbabilityPct
      ProjectedLateDays = a.ProjectedLateDays
      BufferConsumptionRatio = a.BufferConsumptionRatio
      SeverityTier = sprintf "%A" a.SeverityTier
      RiskDrivers = a.RiskDrivers
      AssessmentTime = Timestamp.value a.AssessmentTimestamp
      LifecycleState =
        match a.AssessmentStatus with
        | Evaluated -> "Evaluated"
        | MonitoredActive -> "MonitoredActive"
        | EscalatedToException _ -> "EscalatedToException"
        | NormalizedResolved _ -> "NormalizedResolved"
        | Closed _ -> "Closed"
      DecisionTraceability = a.DecisionTraceability }

type State = Map<PromiseRiskAssessmentId, PromiseRiskAssessmentDto>

let initial: State = Map.empty

let apply (state: State) (event: PromiseRiskEvent) : State =
    match event with
    | CommitmentRiskAssessed assessment ->
        state |> Map.add assessment.AssessmentId (mapToDto assessment)

    | CommitmentRiskEscalated(id, _, _, _, traceId) ->
        state
        |> Map.change id (Option.map (fun dto ->
            { dto with
                LifecycleState = "EscalatedToException"
                DecisionTraceability = Some traceId }))

    | RiskFlagCleared(id, _, _, traceId) ->
        state
        |> Map.change id (Option.map (fun dto ->
            { dto with
                LifecycleState = "NormalizedResolved"
                BreachProbabilityPct = 0.0m
                ProjectedLateDays = 0.0m
                DecisionTraceability = Some traceId }))

let seedFromAggregates (aggregates: PromiseRiskAssessment list) : State =
    aggregates
    |> List.fold (fun state a -> Map.add a.AssessmentId (mapToDto a) state) initial

