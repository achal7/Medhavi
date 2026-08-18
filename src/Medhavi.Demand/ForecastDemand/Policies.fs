// =============================================================================
// ForecastDemand Policies
// Traces to: PO‑D‑017, PO‑D‑019, PO‑D‑020, PO‑D‑022
// =============================================================================
module Medhavi.Demand.ForecastDemand.Policies

/// PO‑D‑020 – Forecast Publication Governance.
type ForecastPublicationGovernancePolicy =
    { AutoPublishConfidenceThreshold: decimal
      MinConfidenceForPublication: decimal
      CompletenessThreshold: decimal
      ConfidenceIndexWeights:
          {| ModelConfidence: decimal
             DataCompleteness: decimal
             SignalQuality: decimal |} }

let defaultPublicationGovernancePolicy =
    { AutoPublishConfidenceThreshold = 70m
      MinConfidenceForPublication = 50m
      CompletenessThreshold = 0.95m
      ConfidenceIndexWeights =
        {| ModelConfidence = 0.50m
           DataCompleteness = 0.30m
           SignalQuality = 0.20m |} }

/// PO‑D‑019 – Unforecastable Series Policy.
type UnforecastableSeriesPolicy =
    { MinHistoryPeriods: int
      FallbackMethod: string }

let defaultUnforecastableSeriesPolicy =
    { MinHistoryPeriods = 12
      FallbackMethod = "Analog" }

/// PO‑D‑022 – Forecast Override Authorization Policy.
type OverrideAuthorizationPolicy =
    { MaxDeviationPercent: decimal
      RequireManagerForExcess: bool }

let defaultOverrideAuthorizationPolicy =
    { MaxDeviationPercent = 30m
      RequireManagerForExcess = true }

/// PO‑D‑017 – Forecast Model Governance Policy.
type ForecastModelGovernancePolicy =
    { MinEvaluationPeriods: int
      StatisticalSignificanceThreshold: decimal
      BiasDegradationTolerance: decimal
      StabilityDegradationTolerance: decimal
      HighPriorityWapeTolerance: decimal }

let defaultModelGovernancePolicy =
    { MinEvaluationPeriods = 4
      StatisticalSignificanceThreshold = 0.05m
      BiasDegradationTolerance = 0.5m
      StabilityDegradationTolerance = 5m
      HighPriorityWapeTolerance = 1m }

/// Model parameters (used by algorithms).
type ForecastModelParametersPolicy =
    { SmoothingAlpha: decimal
      PredictionIntervalConfidence: decimal
      MinHistoryForVariance: int }

let defaultModelParametersPolicy =
    { SmoothingAlpha = 0.3m
      PredictionIntervalConfidence = 0.90m
      MinHistoryForVariance = 12 }
