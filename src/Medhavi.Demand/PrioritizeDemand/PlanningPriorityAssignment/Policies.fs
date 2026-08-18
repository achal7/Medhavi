/// PO-D-039 & PO-D-040 — Planning Priority Assignment Policies
module Medhavi.Demand.PrioritizeDemand.PlanningPriorityAssignment.Policies

/// PO-D-039: Prioritization Policy
/// Governs scoring methodology, dimension weights, and level cutoffs for planning priority calculation
type PrioritizationPolicy =
    { PolicyId: string
      PolicyVersion: string
      Version: int
      RevenueWeight: decimal
      StrategyWeight: decimal
      RiskWeight: decimal
      ContractualWeight: decimal
      CriticalThreshold: decimal
      HighThreshold: decimal
      MediumThreshold: decimal
      LowThreshold: decimal
      MaxRevenueBaseline: decimal }

module PrioritizationPolicy =
    let defaultPolicy: PrioritizationPolicy =
        { PolicyId = "PO-D-039"
          PolicyVersion = "PO-D-039:v1.0"
          Version = 1
          RevenueWeight = 0.35m
          StrategyWeight = 0.25m
          RiskWeight = 0.20m
          ContractualWeight = 0.20m
          CriticalThreshold = 80.0m
          HighThreshold = 60.0m
          MediumThreshold = 40.0m
          LowThreshold = 0.0m
          MaxRevenueBaseline = 20_000_000.0m }

/// PO-D-040: Prioritization Override Policy
/// Governs manual planner overrides of planning priority assignments
type PrioritizationOverridePolicy =
    { PolicyId: string
      PolicyVersion: string
      Version: int
      AllowPlannerOverride: bool
      RequireJustification: bool
      AuditRetentionDays: int }

module PrioritizationOverridePolicy =
    let defaultPolicy: PrioritizationOverridePolicy =
        { PolicyId = "PO-D-040"
          PolicyVersion = "PO-D-040:v1.0"
          Version = 1
          AllowPlannerOverride = true
          RequireJustification = true
          AuditRetentionDays = 365 }
