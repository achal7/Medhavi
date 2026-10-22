module Medhavi.Supply.DetectSupplyExceptions.SupplyExceptionAssessment.Policies

open System
open Medhavi.Supply

// =============================================================================
// PO-S-120 — Exception Detection Policy
// Governs exception types, triggering criteria, severity classification,
// horizon rules, lifecycle transitions, and evidence sufficiency thresholds.
// =============================================================================

type ServiceLevelTriggerCriteria =
    {
        ConsecutivePeriodBreachThreshold: int
        MinimumServiceLevelPercent: decimal
        CriticalSinglePeriodServiceLevelPercent: decimal
    }

type SupplyDisruptionTriggerCriteria =
    {
        StoppageDaysThreshold: decimal
        RequireMateriality: bool
    }

type InventoryTriggerCriteria =
    {
        StockoutDaysThreshold: decimal
        ExcessPercentageThreshold: decimal
        ExcessDurationDaysThreshold: decimal
    }

type CapacityTriggerCriteria =
    {
        ConsecutiveOverloadPeriodsThreshold: int
        UtilizationCapThreshold: decimal
    }

type SupplierCollaborationTriggerCriteria =
    {
        ConsecutiveUnderperformingMonthsThreshold: int
        OtifThresholdPercent: decimal
    }

type EmergingRiskTriggerCriteria =
    { ProbabilityThresholdPercent: decimal }

type HorizonThresholds =
    {
        ImmediateMaxHours: decimal
        ShortTermMaxDays: decimal
        MediumTermMaxDays: decimal
    }

type EvidenceSufficiencyThresholds =
    {
        DetectedMinReliabilityPercent: decimal
        ConfirmedMinIndependentSources: int
        ConfirmedSingleSourceReliabilityPercent: decimal
    }

type EnterpriseEscalationCriteria =
    {
        MinBusinessImpactForEscalation: SeverityBusinessImpact
        EscalateOnMultiFacilityScope: bool
    }

/// PO-S-120 Exception Detection Policy record
type ExceptionDetectionPolicy =
    {
        PolicyId: string
        Version: string
        ServiceLevelCriteria: ServiceLevelTriggerCriteria
        SupplyDisruptionCriteria: SupplyDisruptionTriggerCriteria
        InventoryCriteria: InventoryTriggerCriteria
        CapacityCriteria: CapacityTriggerCriteria
        SupplierCollaborationCriteria: SupplierCollaborationTriggerCriteria
        EmergingRiskCriteria: EmergingRiskTriggerCriteria
        HorizonThresholds: HorizonThresholds
        EvidenceSufficiency: EvidenceSufficiencyThresholds
        EscalationCriteria: EnterpriseEscalationCriteria
        IsActive: bool
    }

let defaultPolicy: ExceptionDetectionPolicy =
    {
        PolicyId = "PO-S-120"
        Version = "1.0.0"
        ServiceLevelCriteria =
            {
                ConsecutivePeriodBreachThreshold = 3
                MinimumServiceLevelPercent = 95.0m
                CriticalSinglePeriodServiceLevelPercent = 90.0m
            }
        SupplyDisruptionCriteria =
            {
                StoppageDaysThreshold = 5.0m
                RequireMateriality = true
            }
        InventoryCriteria =
            {
                StockoutDaysThreshold = 7.0m
                ExcessPercentageThreshold = 150.0m
                ExcessDurationDaysThreshold = 30.0m
            }
        CapacityCriteria =
            {
                ConsecutiveOverloadPeriodsThreshold = 3
                UtilizationCapThreshold = 100.0m
            }
        SupplierCollaborationCriteria =
            {
                ConsecutiveUnderperformingMonthsThreshold = 2
                OtifThresholdPercent = 85.0m
            }
        EmergingRiskCriteria = { ProbabilityThresholdPercent = 70.0m }
        HorizonThresholds =
            {
                ImmediateMaxHours = 48.0m
                ShortTermMaxDays = 14.0m
                MediumTermMaxDays = 60.0m
            }
        EvidenceSufficiency =
            {
                DetectedMinReliabilityPercent = 80.0m
                ConfirmedMinIndependentSources = 2
                ConfirmedSingleSourceReliabilityPercent = 95.0m
            }
        EscalationCriteria =
            {
                MinBusinessImpactForEscalation = SeverityBusinessImpact.Major
                EscalateOnMultiFacilityScope = true
            }
        IsActive = true
    }
