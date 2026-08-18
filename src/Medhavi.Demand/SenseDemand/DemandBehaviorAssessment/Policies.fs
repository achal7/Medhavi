/// Sense Demand Policies
/// Traces to: PO-D-031, PO-D-032, PO-D-034 (Specification Chapter 8)
module Medhavi.Demand.SenseDemand.DemandBehaviorAssessment.Policies

/// PO-D-031: Demand Sensing Policy
/// Governs deviation thresholds, corroboration requirements, and state transition rules.
type DemandSensingPolicy =
    { PolicyId: string
      Version: int
      SignificantThreshold: decimal
      CriticalThreshold: decimal
      NoiseThreshold: decimal
      CorroborationMinimum: int
      HighPrioritySignificantThreshold: decimal }

module DemandSensingPolicy =
    let defaultPolicy: DemandSensingPolicy =
        { PolicyId = "PO-D-031"
          Version = 1
          SignificantThreshold = 2.5m
          CriticalThreshold = 4.0m
          NoiseThreshold = 1.0m
          CorroborationMinimum = 2
          HighPrioritySignificantThreshold = 2.0m }

/// PO-D-032: Forecast Refresh Trigger Policy
/// Governs when a Critical demand behavior state triggers an evaluation for out-of-cycle forecast refresh.
type ForecastRefreshTriggerPolicy =
    { PolicyId: string
      Version: int
      ForecastFreshnessThresholdHours: int
      MinExpectedAccuracyImprovementWape: decimal }

module ForecastRefreshTriggerPolicy =
    let defaultPolicy: ForecastRefreshTriggerPolicy =
        { PolicyId = "PO-D-032"
          Version = 1
          ForecastFreshnessThresholdHours = 4
          MinExpectedAccuracyImprovementWape = 0.02m }

/// PO-D-034: Forecast Refresh Execution Policy
/// Governs partial vs full forecast refresh scope and approvals.
type ForecastRefreshExecutionPolicy =
    { PolicyId: string
      Version: int
      PartialRefreshScope: string
      FullRefreshApprovalRequired: bool }

module ForecastRefreshExecutionPolicy =
    let defaultPolicy: ForecastRefreshExecutionPolicy =
        { PolicyId = "PO-D-034"
          Version = 1
          PartialRefreshScope = "AffectedItemLocations"
          FullRefreshApprovalRequired = true }
