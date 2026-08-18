/// Forecast Demand Policies
/// Traces to: PO-D-017, PO-D-019, PO-D-020, PO-D-022, PO-D-023, PO-D-024 (Specification Chapter 8)
module Medhavi.Demand.ForecastDemand.ForecastPublication.Policies

/// PO-D-017 — Forecast Model Governance Policy
type ForecastModelGovernancePolicy =
    { PolicyId: string
      Version: int
      MinWapeImprovementPercentage: decimal
      MinEvaluationPeriods: int
      AllowedChampionModels: string list }

module ForecastModelGovernancePolicy =
    let defaultPolicy: ForecastModelGovernancePolicy =
        { PolicyId = "PO-D-017"
          Version = 1
          MinWapeImprovementPercentage = 2.0m // 2% WAPE improvement required to dethrone champion
          MinEvaluationPeriods = 6
          AllowedChampionModels = [ "HoltWinters"; "CrostonSba"; "LinearTrend"; "SES"; "AutoSelect" ] }

/// Fallback methods for unforecastable demand series
type FallbackForecastingMethod =
    | SimpleMovingAverage
    | LastYearSamePeriod
    | ZeroDemand

/// PO-D-019 — Unforecastable Series Policy
type UnforecastableSeriesPolicy =
    { PolicyId: string
      Version: int
      MinHistoricalPeriods: int
      MaxSparsityZeroPercentage: decimal
      DefaultFallbackMethod: FallbackForecastingMethod }

module UnforecastableSeriesPolicy =
    let defaultPolicy: UnforecastableSeriesPolicy =
        { PolicyId = "PO-D-019"
          Version = 1
          MinHistoricalPeriods = 4
          MaxSparsityZeroPercentage = 80.0m // >80% zeroes considered intermittent/unforecastable by standard models
          DefaultFallbackMethod = SimpleMovingAverage }

/// PO-D-020 — Forecast Publication Governance Policy
type ForecastPublicationGovernancePolicy =
    { PolicyId: string
      Version: int
      MinCompletenessPercentage: decimal
      MinOverallConfidenceIndex: decimal }

module ForecastPublicationGovernancePolicy =
    let defaultPolicy: ForecastPublicationGovernancePolicy =
        { PolicyId = "PO-D-020"
          Version = 1
          MinCompletenessPercentage = 95.0m
          MinOverallConfidenceIndex = 0.50m }

/// PO-D-022 — Forecast Override Authorization Policy
type ForecastOverrideAuthorizationPolicy =
    { PolicyId: string
      Version: int
      MaxPlannerDeviationPercentage: decimal
      CriticalDeviationThresholdPercentage: decimal
      MinJustificationLength: int }

module ForecastOverrideAuthorizationPolicy =
    let defaultPolicy: ForecastOverrideAuthorizationPolicy =
        { PolicyId = "PO-D-022"
          Version = 1
          MaxPlannerDeviationPercentage = 30.0m // Max 30% adjustment without elevated approval
          CriticalDeviationThresholdPercentage = 50.0m // >50% requires critical notification BN-D-013
          MinJustificationLength = 10 }

/// PO-D-023 — Forecast Model Parameters Policy
type ForecastModelParametersPolicy =
    { PolicyId: string
      Version: int
      SesAlpha: decimal
      HoltWintersAlpha: decimal
      HoltWintersBeta: decimal
      HoltWintersGamma: decimal
      DefaultSeasonLength: int
      PredictionIntervalConfidence: decimal
      DefaultZScore: decimal }

module ForecastModelParametersPolicy =
    let defaultPolicy: ForecastModelParametersPolicy =
        { PolicyId = "PO-D-023"
          Version = 1
          SesAlpha = 0.3m
          HoltWintersAlpha = 0.3m
          HoltWintersBeta = 0.1m
          HoltWintersGamma = 0.2m
          DefaultSeasonLength = 4
          PredictionIntervalConfidence = 0.95m
          DefaultZScore = 1.96m }
