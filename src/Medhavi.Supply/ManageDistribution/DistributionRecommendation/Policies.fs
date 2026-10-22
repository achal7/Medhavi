/// PO-S-090 — Distribution Policy Domain Representation
/// Traces to: Supply Intelligence Specification §8 (PO-S-090)
module Medhavi.Supply.ManageDistribution.DistributionRecommendation.Policies

open Medhavi.Supply.ManageDistribution.DistributionRecommendation.Model

type DistributionPolicyParameters =
    {
        Version: int
        TimeCostPenaltyRate: decimal // lambda_time in BA-S-050
        ExpeditedTimeCostMultiplier: decimal
        DockCongestionBufferPercentage: decimal
        DefaultAllocationPolicy: AllocationPolicy
        MinimumConfidenceThreshold: decimal
        StabilityToleranceThreshold: decimal
        InventoryWeight: decimal
        LaneReliabilityWeight: decimal
        TransitTimeAccuracyWeight: decimal
        ReceivingCapacityWeight: decimal
        DemandStabilityWeight: decimal
        DisruptionExposureWeight: decimal
    }

let defaultDistributionPolicy =
    {
        Version = 1
        TimeCostPenaltyRate = 0.50m
        ExpeditedTimeCostMultiplier = 2.0m
        DockCongestionBufferPercentage = 0.90m
        DefaultAllocationPolicy = AllocationPolicy.PriorityBased
        MinimumConfidenceThreshold = 70.0m
        StabilityToleranceThreshold = 70.0m
        InventoryWeight = 0.25m
        LaneReliabilityWeight = 0.20m
        TransitTimeAccuracyWeight = 0.20m
        ReceivingCapacityWeight = 0.15m
        DemandStabilityWeight = 0.10m
        DisruptionExposureWeight = 0.10m
    }
