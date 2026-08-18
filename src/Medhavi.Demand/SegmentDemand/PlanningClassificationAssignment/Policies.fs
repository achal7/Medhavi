/// Segment Demand Policies
/// Traces to: PO-D-035, PO-D-036 (Specification Chapter 8)
module Medhavi.Demand.SegmentDemand.PlanningClassificationAssignment.Policies

/// PO-D-035: Segmentation Policy
/// Governs ABC Pareto volume/revenue thresholds, XYZ coefficient of variation cutoffs, and minimum evidence.
type SegmentationPolicy =
    { PolicyId: string
      Version: int
      AbcClassACutoff: decimal
      AbcClassBCutoff: decimal
      XyzClassXCutoff: decimal
      XyzClassYCutoff: decimal
      MinimumHistoryPeriods: int
      PolicyVersion: string }

module SegmentationPolicy =
    let defaultPolicy: SegmentationPolicy =
        { PolicyId = "PO-D-035"
          Version = 1
          AbcClassACutoff = 80.0m
          AbcClassBCutoff = 95.0m
          XyzClassXCutoff = 0.50m
          XyzClassYCutoff = 1.00m
          MinimumHistoryPeriods = 6
          PolicyVersion = "PO-D-035-v1.0" }

/// PO-D-036: Segmentation Override Policy
/// Governs manual planner overrides of planning classifications.
type SegmentationOverridePolicy =
    { PolicyId: string
      Version: int
      RequireJustification: bool
      MinimumJustificationLength: int
      PolicyVersion: string }

module SegmentationOverridePolicy =
    let defaultPolicy: SegmentationOverridePolicy =
        { PolicyId = "PO-D-036"
          Version = 1
          RequireJustification = true
          MinimumJustificationLength = 10
          PolicyVersion = "PO-D-036-v1.0" }
