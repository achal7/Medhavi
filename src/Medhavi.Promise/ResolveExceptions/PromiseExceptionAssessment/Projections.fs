/// CA-P-009 Resolve Exceptions — Projections
module Medhavi.Promise.ResolveExceptions.PromiseExceptionAssessment.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let mapPlanToDto (p: MitigationActionPlan) : MitigationActionPlanDto =
    { PlanId = p.PlanId
      ActionType = p.ActionType
      EstimatedExpediteCost = p.EstimatedExpediteCost
      NewProjectedDeliveryDate = Timestamp.value p.NewProjectedDeliveryDate
      ProjectedGrossMarginPct = p.ProjectedGrossMarginPct
      Authorized = p.Authorized }

let mapToDto (a: PromiseExceptionAssessment) : PromiseExceptionAssessmentDto =
    { ExceptionId = PromiseExceptionId.value a.ExceptionId
      CommitmentId = OrderCommitmentId.value a.CommitmentId
      OrderLineId = OrderLineId.value a.OrderLineId
      DisruptionType = sprintf "%A" a.DisruptionType
      SeverityTier = sprintf "%A" a.SeverityTier
      RootCauseDescription = a.RootCauseDescription
      CandidatePlans = a.CandidatePlans |> List.map mapPlanToDto
      SelectedPlanId = a.SelectedPlanId
      LifecycleState =
        match a.LifecycleState with
        | Detected -> "Detected"
        | Triaging -> "Triaging"
        | MitigationApproved planId -> $"MitigationApproved:{planId}"
        | ExecutedResolved _ -> "ExecutedResolved"
        | DeCommitted reason -> $"DeCommitted:{reason}"
      DecisionTraceability = a.DecisionTraceability }

type State = Map<PromiseExceptionId, PromiseExceptionAssessmentDto>

let initial: State = Map.empty

let apply (state: State) (event: PromiseExceptionEvent) : State =
    match event with
    | DisruptionCaseOpened assessment ->
        state |> Map.add assessment.ExceptionId (mapToDto assessment)

    | DisruptionRecoveryExecuted(id, _, planId, resTime, traceId) ->
        state
        |> Map.change id (Option.map (fun dto ->
            let updatedPlans =
                dto.CandidatePlans
                |> List.map (fun p -> if p.PlanId = planId then { p with Authorized = true } else p)
            { dto with
                CandidatePlans = updatedPlans
                SelectedPlanId = Some planId
                LifecycleState = "ExecutedResolved"
                DecisionTraceability = Some traceId }))

let seedFromAggregates (aggregates: PromiseExceptionAssessment list) : State =
    aggregates
    |> List.fold (fun state a -> Map.add a.ExceptionId (mapToDto a) state) initial

