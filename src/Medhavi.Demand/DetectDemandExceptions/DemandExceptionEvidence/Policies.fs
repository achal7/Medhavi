/// PO-D-044 — Demand Exception Evidence Policy
/// Governs detection thresholds, severity mapping, and recovery hysteresis
module Medhavi.Demand.DetectDemandExceptions.DemandExceptionEvidence.Policies

/// PO-D-044: Demand Exception Evidence Policy
/// Governs multi-dimensional detection thresholds, severity tiers, and resolution conditions
type DemandExceptionEvidencePolicy =
    { PolicyId: string
      PolicyVersion: string
      Version: int
      // Forecast Bias thresholds (absolute %)
      ForecastBiasCriticalThreshold: decimal
      ForecastBiasHighThreshold: decimal
      ForecastBiasMediumThreshold: decimal
      // WAPE thresholds (%)
      WapeCriticalThreshold: decimal
      WapeHighThreshold: decimal
      WapeMediumThreshold: decimal
      // Completeness thresholds (%)
      CompletenessCriticalThreshold: decimal
      CompletenessHighThreshold: decimal
      CompletenessMediumThreshold: decimal
      // Tracking Signal limits (|TS|)
      TrackingSignalCriticalLimit: decimal
      TrackingSignalHighLimit: decimal
      // Negative Forecast Value Add thresholds (% points)
      NegativeFvaCriticalThreshold: decimal
      NegativeFvaHighThreshold: decimal
      // Recovery hysteresis factor (e.g. 0.80 = value must return to <= 80% of warning threshold to resolve)
      HysteresisRecoveryFactor: decimal }

module DemandExceptionEvidencePolicy =
    let defaultPolicy: DemandExceptionEvidencePolicy =
        { PolicyId = "PO-D-044"
          PolicyVersion = "PO-D-044:v1.0"
          Version = 1
          ForecastBiasCriticalThreshold = 25.0m
          ForecastBiasHighThreshold = 15.0m
          ForecastBiasMediumThreshold = 10.0m
          WapeCriticalThreshold = 50.0m
          WapeHighThreshold = 35.0m
          WapeMediumThreshold = 25.0m
          CompletenessCriticalThreshold = 50.0m
          CompletenessHighThreshold = 70.0m
          CompletenessMediumThreshold = 85.0m
          TrackingSignalCriticalLimit = 5.0m
          TrackingSignalHighLimit = 4.0m
          NegativeFvaCriticalThreshold = -15.0m
          NegativeFvaHighThreshold = -5.0m
          HysteresisRecoveryFactor = 0.80m }
