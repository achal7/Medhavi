module Medhavi.Demand.DemandBehaviourAssessment.Rules

let defaultSignificantThreshold = 2.5m
let defaultCriticalThreshold = 4.0m
let defaultReducedThreshold = 1.5m
let defaultNoiseThreshold = 1.0m

let calculateDeviation (value: decimal) (baseline: decimal) (bound: decimal) : decimal =
    if bound = 0.0M then 0.0M else (value - baseline) / bound

/// BR-D-052: Lowered threshold for high-priority products
let getSignificantThreshold (isHighPriority: bool) : decimal =
    if isHighPriority then defaultReducedThreshold else defaultSignificantThreshold

/// BR-D-053: Signals below noise threshold shall not trigger state change
let isNoise (deviation: decimal) : bool = abs deviation < defaultNoiseThreshold
