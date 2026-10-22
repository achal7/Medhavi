/// PO-S-050 — Capacity Health Evaluation Policy Configuration
/// Traces to: Supply Intelligence Specification §8 (PO-S-050)
module Medhavi.Supply.ManageCapacity.CapacityHealth.Policies

type CapacityHealthPolicy =
    { HighVolatilityCvThreshold: decimal
      HighChangeEventsThreshold: int
      UnderutilizedScopeRatioThreshold: decimal
      CriticallyConstrainedRatioThreshold: decimal }

let defaultPolicy =
    { HighVolatilityCvThreshold = 0.30m
      HighChangeEventsThreshold = 5
      UnderutilizedScopeRatioThreshold = 0.50m
      CriticallyConstrainedRatioThreshold = 0.05m }
