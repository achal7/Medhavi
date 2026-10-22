// Traceability: PO-S-140 Learning Policy
module Medhavi.Supply.LearnFromSupply.SupplyLearning.Policies

open Medhavi.Supply.ArsIdentifiers

/// PO-S-140 Learning Policy Configuration Record
/// Governs recurrence thresholds, multi-period evidence span, cross-capability corroboration,
/// pattern confidence computation, intervention confidence assessment, and outcome determination rules.
type LearningPolicy =
    {
        PolicyId: string
        Version: int
        MinRecurrencePeriods: int
        MinRecurrenceFrequency: decimal
        MinEvidenceHorizonMonths: int
        MinCrossCapabilitySources: int
        PatternConfidenceHighThreshold: decimal
        PatternConfidenceMediumThreshold: decimal
        InterventionConfidenceHighThreshold: decimal
        InterventionConfidenceMediumThreshold: decimal
        AnalogueHighThreshold: decimal
        StatisticalSignificanceThreshold: decimal
        PlanStabilityThreshold: decimal
        BottleneckPersistenceThreshold: decimal
        SupplierOtifDriftThreshold: decimal
        InventoryStockoutChurnThreshold: int
        ExceptionRecurrenceThreshold: int
    }

/// Default authoritative enterprise configuration per PO-S-140
let defaultLearningPolicy: LearningPolicy =
    {
        PolicyId = Policies.learningPolicy.Id
        Version = 1
        MinRecurrencePeriods = 3
        MinRecurrenceFrequency = 0.50m
        MinEvidenceHorizonMonths = 6
        MinCrossCapabilitySources = 2
        PatternConfidenceHighThreshold = 0.75m
        PatternConfidenceMediumThreshold = 0.45m
        InterventionConfidenceHighThreshold = 0.70m
        InterventionConfidenceMediumThreshold = 0.40m
        AnalogueHighThreshold = 0.75m
        StatisticalSignificanceThreshold = 0.05m
        PlanStabilityThreshold = 0.70m
        BottleneckPersistenceThreshold = 0.80m
        SupplierOtifDriftThreshold = 0.15m
        InventoryStockoutChurnThreshold = 3
        ExceptionRecurrenceThreshold = 3
    }
