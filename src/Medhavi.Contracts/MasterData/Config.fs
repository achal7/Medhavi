namespace Medhavi.Contracts.MasterData.Config

open System

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
        Target: decimal option
        /// Alert threshold — triggers warning when crossed
        AlertThreshold: decimal option
        /// Direction: true = higher is better (OTD%), false = lower is better (lateness)
        HigherIsBetter: bool
        /// Weight this KPI carries in the optimizer's multi-objective function
        OptimizerWeight: decimal
        /// UI display order within its category
        DisplayOrder: int
        /// Optional color for UI rendering
        Color: string option
        /// Audit fields
        LastModifiedBy: string
        LastModifiedAt: DateTimeOffset
    }
