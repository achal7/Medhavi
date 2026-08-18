/// Forecast Demand Business Rules
/// Traces to: BR-D-200, BR-D-201, BR-D-202, BR-D-203, BR-D-204, BR-D-205, BR-D-206, BR-D-401 (Specification Chapter 7)
module Medhavi.Demand.ForecastDemand.ForecastPublication.Rules

open Medhavi.Foundation.Contracts
open Medhavi.Demand.ArsIdentifiers
open Model
open Policies

/// Input context for Champion Model Selection (DE-D-013)
type ChampionModelSelectionInput =
    { CandidateModelId: string
      WapeImprovementPercentage: decimal
      EvaluationPeriodsCount: int
      Policy: ForecastModelGovernancePolicy }

/// BR-D-401 — Authorised Forecasting Strategy
let authorisedForecastingStrategy: Rule<ChampionModelSelectionInput> =
    Rule.create
        Rules.authorisedForecastingStrategy.Id
        Rules.authorisedForecastingStrategy.Explanation
        (fun input ->
            input.Policy.AllowedChampionModels |> List.contains input.CandidateModelId)
        (fun input ->
            sprintf "CandidateModel: %s, AllowedModels: %A" input.CandidateModelId input.Policy.AllowedChampionModels)

/// Rule set for Champion Model Selection (DE-D-013)
let championModelRules: Rule<ChampionModelSelectionInput> list =
    [ authorisedForecastingStrategy ]

/// Input context for Series Forecastability Evaluation (DE-D-003)
type SeriesForecastabilityInput =
    { HistoricalDataPointCount: int
      SparsityZeroPercentage: decimal
      Policy: UnforecastableSeriesPolicy }

/// BR-D-201 — Forecastability Minimum Data Requirements
let forecastabilityMinimumData: Rule<SeriesForecastabilityInput> =
    Rule.create
        Rules.forecastabilityMinimumData.Id
        Rules.forecastabilityMinimumData.Explanation
        (fun input ->
            input.HistoricalDataPointCount >= input.Policy.MinHistoricalPeriods
            && input.SparsityZeroPercentage < input.Policy.MaxSparsityZeroPercentage)
        (fun input ->
            sprintf "DataPoints: %d (min %d), Sparsity: %.1f%% (max %.1f%%)"
                input.HistoricalDataPointCount
                input.Policy.MinHistoricalPeriods
                input.SparsityZeroPercentage
                input.Policy.MaxSparsityZeroPercentage)

/// Rule set for Series Forecastability (DE-D-003)
let seriesForecastabilityRules: Rule<SeriesForecastabilityInput> list =
    [ forecastabilityMinimumData ]

/// Input context for Planner Override Evaluation (DE-D-005)
type OverrideEvaluationInput =
    { OriginalValue: decimal
      OverrideValue: decimal
      Justification: string
      Policy: ForecastOverrideAuthorizationPolicy }

/// Helper to compute override deviation percentage
let private computeOverrideDeviation (input: OverrideEvaluationInput) : decimal =
    if input.OriginalValue > 0.0m then
        abs (input.OverrideValue - input.OriginalValue) / input.OriginalValue * 100.0m
    else
        0.0m

/// BR-D-202 — Override Authorization Thresholds
let overrideAuthorizationThresholds: Rule<OverrideEvaluationInput> =
    Rule.create
        Rules.overrideAuthorizationThresholds.Id
        Rules.overrideAuthorizationThresholds.Explanation
        (fun input ->
            let dev = computeOverrideDeviation input
            dev <= input.Policy.MaxPlannerDeviationPercentage)
        (fun input ->
            let dev = computeOverrideDeviation input
            sprintf "Deviation: %.1f%%, MaxAllowed: %.1f%%" dev input.Policy.MaxPlannerDeviationPercentage)

/// BR-D-203 — Override Reason Code / Justification Mandatory
let overrideReasonCodeMandatory: Rule<OverrideEvaluationInput> =
    Rule.create
        Rules.overrideReasonCodeMandatory.Id
        Rules.overrideReasonCodeMandatory.Explanation
        (fun input ->
            let trimmed = if isNull input.Justification then "" else input.Justification.Trim()
            trimmed.Length >= input.Policy.MinJustificationLength)
        (fun input ->
            let trimmed = if isNull input.Justification then "" else input.Justification.Trim()
            sprintf "JustificationLength: %d, MinRequired: %d"
                trimmed.Length
                input.Policy.MinJustificationLength)

/// Rule set for Planner Override Evaluation (DE-D-005)
let overrideEvaluationRules: Rule<OverrideEvaluationInput> list =
    [ overrideAuthorizationThresholds
      overrideReasonCodeMandatory ]

/// Input context for Forecast Publication Governance Approval (DE-D-004)
type PublicationApprovalInput =
    { CompletenessScore: decimal
      ConfidenceIndex: decimal
      LineCount: int
      Policy: ForecastPublicationGovernancePolicy }

/// BR-D-206 — Publication Completeness Governance Checks
let publicationCompletenessGovernance: Rule<PublicationApprovalInput> =
    Rule.create
        Rules.publicationCompletenessGovernance.Id
        Rules.publicationCompletenessGovernance.Explanation
        (fun input ->
            input.LineCount > 0
            && input.CompletenessScore >= input.Policy.MinCompletenessPercentage
            && input.ConfidenceIndex >= input.Policy.MinOverallConfidenceIndex)
        (fun input ->
            sprintf "LineCount: %d, Completeness: %.1f%% (min %.1f%%), Confidence: %.2f (min %.2f)"
                input.LineCount
                input.CompletenessScore
                input.Policy.MinCompletenessPercentage
                input.ConfidenceIndex
                input.Policy.MinOverallConfidenceIndex)

/// Rule set for Forecast Publication Approval (DE-D-004)
let publicationApprovalRules: Rule<PublicationApprovalInput> list =
    [ publicationCompletenessGovernance ]
