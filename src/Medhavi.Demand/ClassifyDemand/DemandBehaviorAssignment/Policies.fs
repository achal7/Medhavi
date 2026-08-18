/// PO-D-037 & PO-D-038 — Demand Behavior Classification Policies
module Medhavi.Demand.ClassifyDemand.DemandBehaviorAssignment.Policies

/// PO-D-037: Classification Policy
/// Governs ADI, CV², seasonal autocorrelation, and trend p-value thresholds for demand behavior classification
type ClassificationPolicy =
    { PolicyId: string
      PolicyVersion: string
      Version: int
      IntermittentAdiThreshold: decimal
      LumpyCv2Threshold: decimal
      SeasonalAutocorrelationLag: int
      SeasonalAutocorrelationThreshold: decimal
      TrendPValueThreshold: decimal
      MinimumHistoryDataPoints: int }

module ClassificationPolicy =
    let defaultPolicy: ClassificationPolicy =
        { PolicyId = "PO-D-037"
          PolicyVersion = "PO-D-037:v1.0"
          Version = 1
          IntermittentAdiThreshold = 1.32m
          LumpyCv2Threshold = 0.49m
          SeasonalAutocorrelationLag = 7
          SeasonalAutocorrelationThreshold = 0.40m
          TrendPValueThreshold = 0.05m
          MinimumHistoryDataPoints = 12 }

/// PO-D-038: Classification Override Policy
/// Governs manual planner classification overrides, mandatory justification, and audit review
type ClassificationOverridePolicy =
    { PolicyId: string
      PolicyVersion: string
      Version: int
      AllowPlannerOverride: bool
      RequireJustification: bool
      AuditRetentionDays: int }

module ClassificationOverridePolicy =
    let defaultPolicy: ClassificationOverridePolicy =
        { PolicyId = "PO-D-038"
          PolicyVersion = "PO-D-038:v1.0"
          Version = 1
          AllowPlannerOverride = true
          RequireJustification = true
          AuditRetentionDays = 365 }
