/// PO-S-050 — Capacity Management Policy
/// Traces to: Supply Intelligence Specification §8 (PO-S-050)
module Medhavi.Supply.ManageCapacity.CapacityPosition.Policies

type CapacityManagementPolicy =
    { TargetLowUtilization: decimal
      TargetHighUtilization: decimal
      NearCapacityThreshold: decimal
      OverloadThreshold: decimal
      NearConstraintThreshold: decimal
      ActiveConstraintThreshold: decimal
      BottleneckThreshold: decimal
      MinEconomicRatio: decimal
      OvertimeRatio: decimal
      MaxPlannedRatio: decimal
      AbsoluteRatio: decimal
      ResourceCapacityRating: decimal }

let defaultPolicy =
    { TargetLowUtilization = 0.70m
      TargetHighUtilization = 0.85m
      NearCapacityThreshold = 0.85m
      OverloadThreshold = 1.00m
      NearConstraintThreshold = 0.85m
      ActiveConstraintThreshold = 0.95m
      BottleneckThreshold = 0.95m
      MinEconomicRatio = 0.40m
      OvertimeRatio = 1.15m
      MaxPlannedRatio = 1.25m
      AbsoluteRatio = 1.40m
      ResourceCapacityRating = 1.00m }
