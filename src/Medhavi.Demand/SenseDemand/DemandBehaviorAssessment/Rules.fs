/// Sense Demand Business Rules
/// Traces to: BR-D-300, BR-D-301, BR-D-302, BR-D-303, BR-D-304 (Specification Chapter 7)
module Medhavi.Demand.SenseDemand.DemandBehaviorAssessment.Rules

open Medhavi.SemanticModel
open Medhavi.Foundation.Contracts
open Medhavi.Demand.ArsIdentifiers
open Model
open Policies

/// Input context for signal evaluation rules (DE-D-006)
type SignalEvaluationInput =
    { SignalQuantity: Quantity
      BaselineMean: decimal
      BaselineStdDev: decimal
      CorroborationCount: int
      IsHighPriority: bool
      Policy: DemandSensingPolicy }

/// Helper to compute absolute standard deviations from baseline
let private computeDeviation (input: SignalEvaluationInput) : decimal =
    let qty = Quantity.value input.SignalQuantity
    let stdDev = if input.BaselineStdDev > 0m then input.BaselineStdDev else 1m
    abs (qty - input.BaselineMean) / stdDev

/// BR-D-303 — Noise suppression (signals below noise threshold are suppressed)
let noiseSuppression: Rule<SignalEvaluationInput> =
    Rule.create
        Rules.noiseSuppression.Id
        Rules.noiseSuppression.Explanation
        (fun input -> computeDeviation input >= input.Policy.NoiseThreshold)
        (fun input ->
            let dev = computeDeviation input
            sprintf "Deviation: %.2fσ, NoiseThreshold: %.2fσ" dev input.Policy.NoiseThreshold)

/// BR-D-300 — Deviation thresholds for Demand Behavior State Change
let deviationThresholds: Rule<SignalEvaluationInput> =
    Rule.create
        Rules.deviationThresholds.Id
        Rules.deviationThresholds.Explanation
        (fun input ->
            let dev = computeDeviation input
            let threshold =
                if input.IsHighPriority then input.Policy.HighPrioritySignificantThreshold
                else input.Policy.SignificantThreshold
            dev >= threshold)
        (fun input ->
            let dev = computeDeviation input
            let threshold =
                if input.IsHighPriority then input.Policy.HighPrioritySignificantThreshold
                else input.Policy.SignificantThreshold
            sprintf "Deviation: %.2fσ, Threshold: %.2fσ, HighPriority: %b" dev threshold input.IsHighPriority)

/// BR-D-301 — Corroboration requirement for Critical state
let corroborationRequirement: Rule<SignalEvaluationInput> =
    Rule.create
        Rules.corroborationRequirement.Id
        Rules.corroborationRequirement.Explanation
        (fun input ->
            let dev = computeDeviation input
            if dev >= input.Policy.CriticalThreshold then
                input.CorroborationCount >= input.Policy.CorroborationMinimum
            else
                true)
        (fun input ->
            sprintf "CorroborationCount: %d, Required: %d" input.CorroborationCount input.Policy.CorroborationMinimum)

/// BR-D-302 — High-priority sensitivity (lowers Significant threshold for high-priority items)
let highPrioritySensitivity: Rule<SignalEvaluationInput> =
    Rule.create
        Rules.highPrioritySensitivity.Id
        Rules.highPrioritySensitivity.Explanation
        (fun input ->
            if input.IsHighPriority then
                let dev = computeDeviation input
                dev >= input.Policy.HighPrioritySignificantThreshold
            else
                true)
        (fun input ->
            sprintf "IsHighPriority: %b, HighPriorityThreshold: %.2fσ" input.IsHighPriority input.Policy.HighPrioritySignificantThreshold)

/// Rule set for DE-D-006 signal evaluation
let signalEvaluationRules: Rule<SignalEvaluationInput> list =
    [ noiseSuppression
      deviationThresholds
      corroborationRequirement
      highPrioritySensitivity ]

/// Input context for forecast refresh evaluation rules (DE-D-007)
type RefreshEvaluationInput =
    { CurrentState: DemandBehaviorState
      ForecastAgeHours: int
      ExpectedAccuracyImprovementWape: decimal
      Policy: ForecastRefreshTriggerPolicy }

/// BR-D-304 — Forecast Refresh Evaluation on Critical State
let forecastRefreshEvaluation: Rule<RefreshEvaluationInput> =
    Rule.create
        Rules.forecastRefreshEvaluation.Id
        Rules.forecastRefreshEvaluation.Explanation
        (fun input ->
            input.CurrentState = Critical
            && input.ForecastAgeHours > input.Policy.ForecastFreshnessThresholdHours
            && input.ExpectedAccuracyImprovementWape >= input.Policy.MinExpectedAccuracyImprovementWape)
        (fun input ->
            sprintf "State: %A, Age: %d hrs (max %d), WAPE Improvement: %.2f%% (min %.2f%%)"
                input.CurrentState
                input.ForecastAgeHours
                input.Policy.ForecastFreshnessThresholdHours
                (input.ExpectedAccuracyImprovementWape * 100m)
                (input.Policy.MinExpectedAccuracyImprovementWape * 100m))

/// Rule set for DE-D-007 forecast refresh evaluation
let refreshEvaluationRules: Rule<RefreshEvaluationInput> list =
    [ forecastRefreshEvaluation ]
