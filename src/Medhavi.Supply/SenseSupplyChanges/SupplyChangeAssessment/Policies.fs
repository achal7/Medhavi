/// PO-S-100 — Supply Change Policy Configuration
/// Traces to: Supply Intelligence Specification §8 (PO-S-100)
module Medhavi.Supply.SenseSupplyChanges.SupplyChangeAssessment.Policies

open System

/// PO-S-100: Supply Change Policy Parameters
type SupplyChangePolicyConfiguration =
    {
        PolicyVersion: string
        InventoryDetectionThresholdPercent: decimal
        InventoryCriticalThresholdPercent: decimal
        CommitmentDateShiftThresholdDays: decimal
        CapacityThresholdPercent: decimal
        CapacityBottleneckThresholdPercent: decimal
        OrderQuantityDeltaThresholdPercent: decimal
        OrderDeliveryDateShiftThresholdDays: decimal
        CorroborationMinimumRequired: int
        SourceReliabilityWeight: decimal
        EvidenceStrengthWeight: decimal
        CorroborationWeight: decimal
        DelayThresholdDays: decimal
        EarlyThresholdDays: decimal
        QuantityVarianceThresholdPercent: decimal
        CorroborationTimeWindowHours: decimal
    }

let defaultPolicy: SupplyChangePolicyConfiguration =
    {
        PolicyVersion = "1.0.0"
        InventoryDetectionThresholdPercent = 5.0m
        InventoryCriticalThresholdPercent = 10.0m
        CommitmentDateShiftThresholdDays = 2.0m
        CapacityThresholdPercent = 5.0m
        CapacityBottleneckThresholdPercent = 2.0m
        OrderQuantityDeltaThresholdPercent = 5.0m
        OrderDeliveryDateShiftThresholdDays = 2.0m
        CorroborationMinimumRequired = 2
        SourceReliabilityWeight = 0.50m
        EvidenceStrengthWeight = 0.30m
        CorroborationWeight = 0.20m
        DelayThresholdDays = 2.0m
        EarlyThresholdDays = 1.0m
        QuantityVarianceThresholdPercent = 5.0m
        CorroborationTimeWindowHours = 24.0m
    }
