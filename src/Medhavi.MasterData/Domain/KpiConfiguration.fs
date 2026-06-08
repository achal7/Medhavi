namespace Medhavi.MasterData.KpiConfiguration

open System
open Medhavi.SharedKernel

// =============================================================================
// KPI Configuration — planner-managed KPI definitions and optimizer weights
// =============================================================================

/// Planner-configurable KPI definition.
/// Stored in MasterData because planners control weights, targets, and enable/disable
/// flags without code changes. The FormulaRegistry in Medhavi.Analytics provides the
/// actual calculation logic; this record provides the configuration envelope.
type KpiConfig =
    {
        KpiId: string
        Name: string
        Description: string
        /// Category grouping for UI display: "Business" | "Operational" | "Financial"
        Category: string
        /// KPI invalidation class: "PlanRunDependent" | "OperationalState" | "ExecutionRealTime"
        KpiClass: string
        /// Whether this KPI is currently active (planners can turn off individual KPIs)
        IsEnabled: bool
        /// Display unit (e.g., "%", "days", "units")
        Unit: string
        /// Target value (e.g., OTD target = 95%)
        Target: PositiveDecimal option
        /// Alert threshold — triggers warning when crossed
        AlertThreshold: PositiveDecimal option
        /// Direction: true = higher is better (OTD%), false = lower is better (lateness)
        HigherIsBetter: bool
        /// Weight this KPI carries in the optimizer's multi-objective function
        OptimizerWeight: PositiveDecimal
        /// UI display order within its category
        DisplayOrder: int
        /// Optional color for UI rendering
        Color: string option
        /// Audit fields
        LastModifiedBy: string
        LastModifiedAt: Timestamp
    }

/// Per-scenario or global optimizer objective weights.
/// When ScenarioId = None, this is the global default weights config.
/// When ScenarioId = Some id, these weights override the defaults for that scenario's plan runs.
type KpiWeightsConfig =
    {
        ConfigId: string
        /// None = global defaults; Some scenarioId = scenario-specific override
        ScenarioId: string option
        LatenessWeight: PositiveDecimal
        ShortfallWeight: PositiveDecimal
        EarlinessWeight: PositiveDecimal
        OverUtilWeight: PositiveDecimal
        UnderUtilWeight: PositiveDecimal
        ChurnWeight: PositiveDecimal
        ProductionCostWeight: PositiveDecimal
        HoldingCostWeight: PositiveDecimal
        TransportCostWeight: PositiveDecimal
        SetupCostWeight: PositiveDecimal
        CO2Weight: PositiveDecimal
        IsDefault: bool
        CreatedAt: Timestamp
    }
