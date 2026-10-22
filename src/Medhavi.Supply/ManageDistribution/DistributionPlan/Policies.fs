/// PO-S-090 & PO-S-091 — Distribution Plan Publication Policy
/// Traces to: Supply Intelligence Specification §8 (PO-S-090, PO-S-091)
module Medhavi.Supply.ManageDistribution.DistributionPlan.Policies

type DistributionPlanPublicationPolicyParameters =
    {
        Version: int
        MinimumFeasibilityPercentage: decimal
        MinimumConfidenceThreshold: decimal
        MinimumAverageStabilityScore: decimal
        MaximumAllowedHighVolatilityMovements: int
        AllowPublicationWithFlags: bool
    }

let defaultDistributionPlanPublicationPolicy =
    {
        Version = 1
        MinimumFeasibilityPercentage = 80.0m
        MinimumConfidenceThreshold = 60.0m
        MinimumAverageStabilityScore = 60.0m
        MaximumAllowedHighVolatilityMovements = 5
        AllowPublicationWithFlags = true
    }
