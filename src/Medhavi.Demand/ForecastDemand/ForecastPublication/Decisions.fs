/// Forecast Demand Business Decisions
/// Traces to: DE-D-013, DE-D-003, DE-D-005, DE-D-004 (Specification Chapter 6)
module Medhavi.Demand.ForecastDemand.ForecastPublication.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Policies
open Rules

// =============================================================================
// DE-D-013 — Select Champion Model
// =============================================================================

type ChampionModelAlternative =
    | PromoteChampion of ModelId: string
    | RetainCurrentChampion of ModelId: string

type ChampionModelDecision =
    { SelectedAlternative: ChampionModelAlternative
      SelectedModelId: string
      Rationale: string }

let selectChampionModel
    (rules: Rule<ChampionModelSelectionInput> list)
    (input: ChampionModelSelectionInput)
    (currentChampionId: string)
    : Result<DecisionOutcome<ChampionModelDecision>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let passed = evaluations |> List.forall (fun e -> e.Passed)

        let selectedAlternative, selectedModel, rationale =
            if passed && input.WapeImprovementPercentage >= input.Policy.MinWapeImprovementPercentage then
                PromoteChampion input.CandidateModelId,
                input.CandidateModelId,
                sprintf "Candidate model %s demonstrated %.2f%% WAPE improvement over %d evaluation periods per PO-D-017."
                    input.CandidateModelId input.WapeImprovementPercentage input.EvaluationPeriodsCount
            else
                RetainCurrentChampion currentChampionId,
                currentChampionId,
                sprintf "Candidate model %s did not meet the required %.2f%% WAPE improvement threshold; retaining %s."
                    input.CandidateModelId input.Policy.MinWapeImprovementPercentage currentChampionId

        let decision =
            { SelectedAlternative = selectedAlternative
              SelectedModelId = selectedModel
              Rationale = rationale }

        return
            { Outcome = decision
              Evaluations = evaluations }
    }

// =============================================================================
// DE-D-003 — Generate Forecast for Series (Forecastability Check)
// =============================================================================

type ForecastabilityAlternative =
    | Forecastable
    | Unforecastable of FallbackMethod: FallbackForecastingMethod

type ForecastabilityDecision =
    { SelectedAlternative: ForecastabilityAlternative
      IsForecastable: bool
      FallbackMethod: FallbackForecastingMethod option
      Rationale: string }

let evaluateSeriesForecastability
    (rules: Rule<SeriesForecastabilityInput> list)
    (input: SeriesForecastabilityInput)
    : Result<DecisionOutcome<ForecastabilityDecision>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let passed = evaluations |> List.forall (fun e -> e.Passed)

        let alternative, isForecastable, fallback, rationale =
            if passed then
                Forecastable, true, None, "Series meets historical data sufficiency and sparsity requirements per PO-D-019."
            else
                Unforecastable input.Policy.DefaultFallbackMethod,
                false,
                Some input.Policy.DefaultFallbackMethod,
                sprintf "Series possesses insufficient data (%d periods) or excessive sparsity (%.1f%%); applying %A fallback per PO-D-019."
                    input.HistoricalDataPointCount input.SparsityZeroPercentage input.Policy.DefaultFallbackMethod

        let decision =
            { SelectedAlternative = alternative
              IsForecastable = isForecastable
              FallbackMethod = fallback
              Rationale = rationale }

        return
            { Outcome = decision
              Evaluations = evaluations }
    }

// =============================================================================
// DE-D-005 — Evaluate Forecast Override
// =============================================================================

type OverrideAlternative =
    | AcceptOverride
    | RejectOverride of Reason: string

type OverrideDecision =
    { SelectedAlternative: OverrideAlternative
      IsAccepted: bool
      Rationale: string }

let evaluateForecastOverride
    (rules: Rule<OverrideEvaluationInput> list)
    (input: OverrideEvaluationInput)
    : Result<DecisionOutcome<OverrideDecision>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        let alternative, isAccepted, rationale =
            if failed.IsEmpty then
                AcceptOverride,
                true,
                "Planner override satisfies deviation bounds and justification requirements per PO-D-022."
            else
                let reasons = failed |> List.map (fun e -> e.RuleId) |> String.concat ", "
                RejectOverride reasons,
                false,
                sprintf "Planner override rejected due to rule violations: %s" reasons

        let decision =
            { SelectedAlternative = alternative
              IsAccepted = isAccepted
              Rationale = rationale }

        return
            { Outcome = decision
              Evaluations = evaluations }
    }

// =============================================================================
// DE-D-004 — Approve Forecast Publication
// =============================================================================

type PublicationApprovalAlternative =
    | ApprovePublication
    | DeferPublication of Reason: string

type PublicationApprovalDecision =
    { SelectedAlternative: PublicationApprovalAlternative
      IsApproved: bool
      Rationale: string }

let evaluatePublicationApproval
    (rules: Rule<PublicationApprovalInput> list)
    (input: PublicationApprovalInput)
    : Result<DecisionOutcome<PublicationApprovalDecision>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        let alternative, isApproved, rationale =
            if failed.IsEmpty then
                ApprovePublication,
                true,
                "Forecast publication satisfies all completeness and confidence governance criteria per PO-D-020."
            else
                let reasons = failed |> List.map (fun e -> e.RuleId) |> String.concat ", "
                DeferPublication reasons,
                false,
                sprintf "Forecast publication deferred due to governance violations: %s" reasons

        let decision =
            { SelectedAlternative = alternative
              IsApproved = isApproved
              Rationale = rationale }

        return
            { Outcome = decision
              Evaluations = evaluations }
    }
