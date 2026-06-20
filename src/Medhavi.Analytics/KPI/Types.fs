namespace Medhavi.Analytics.KPI

open System
open Medhavi.Contracts.Analytics

/// KPI status relative to its target and alert threshold
type KpiStatus =
    | Good // at or exceeding target
    | Warning // below target but above alert threshold
    | Critical // below alert threshold
    | NoTarget // no target configured

/// A single KPI value at a specific point in time (a snapshot)
type KpiSnapshot =
    {
        KpiId: string
        Name: string
        Value: decimal
        Unit: string
        Target: decimal option
        AlertThreshold: decimal option
        HigherIsBetter: bool
        Status: KpiStatus
        /// Trend: delta compared to last evaluation
        Delta: decimal option
        EvaluatedAt: DateTimeOffset
    }

/// KPI aggregated per PlanningPeriod — what the planning board shows in the KPI row
type KpiPeriodView =
    { Period: PlanningPeriod
      PlantId: string option
      SkuId: string option
      ScenarioId: string option
      Snapshots: KpiSnapshot list }

// =============================================================================
// KPI Class — governs invalidation and refresh strategy
// =============================================================================

/// Three-class invalidation model for KPIs.
/// - PlanRunDependent: recomputed after plan run (OTD, OTIF, Fill Rate)
/// - OperationalState: invalidated by domain events without plan run (Utilization, DoS)
/// - ExecutionRealTime: always fresh, minimal cache (ScheduleAdherence, ActualOTD)
type KpiClass =
    | PlanRunDependent
    | OperationalState
    | ExecutionRealTime

/// Cache entry key for KPI read model
type KpiCacheKey =
    { KpiId: string
      Period: PlanningPeriod
      PlantId: string option
      SkuId: string option
      ScenarioId: string option }
