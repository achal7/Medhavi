module Medhavi.Supply.PlanSupply.SupplyPlan.Policies

open System

/// PO-S-021 — Constraint Relaxation Policy
/// Governs the escalation and approval of planning constraint relaxations when a feasible plan cannot be produced.
type ConstraintRelaxationPolicy =
    { MaxReviewPeriodHours: int
      AutoEscalateHardConstraints: bool }

module ConstraintRelaxationPolicy =
    let defaultPolicy =
        { MaxReviewPeriodHours = 24
          AutoEscalateHardConstraints = true }

/// PO-S-022 — Supply Plan Acceptance Policy
/// Governs automatic acceptance and escalation criteria for draft supply plans based on quality thresholds and plan stability.
type SupplyPlanAcceptancePolicy =
    { MinServiceLevelTarget: decimal // e.g. 0.95m (95%)
      MaxCapacityUtilizationLimit: decimal // e.g. 0.95m (95%)
      MinStabilityScoreTarget: decimal // e.g. 0.70m (70%)
      WarningTolerance: decimal } // e.g. 0.05m (5%)

module SupplyPlanAcceptancePolicy =
    let defaultPolicy =
        { MinServiceLevelTarget = 0.95m
          MaxCapacityUtilizationLimit = 0.95m
          MinStabilityScoreTarget = 0.70m
          WarningTolerance = 0.05m }

/// PO-S-023 — Supply Plan Publication Cadence Policy
/// Governs the publication cadence across operational and tactical horizons.
type SupplyPlanPublicationCadencePolicy =
    { OperationalPublicationIntervalDays: int // e.g. 7 days
      TacticalPublicationIntervalDays: int // e.g. 30 days
      AllowImmediatePostAcceptancePublication: bool }

module SupplyPlanPublicationCadencePolicy =
    let defaultPolicy =
        { OperationalPublicationIntervalDays = 7
          TacticalPublicationIntervalDays = 30
          AllowImmediatePostAcceptancePublication = true }
