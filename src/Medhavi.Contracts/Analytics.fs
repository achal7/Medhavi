namespace Medhavi.Contracts.Analytics

open System

// =============================================================================
// Planning Period — the fundamental time bucketing type for all projections
// =============================================================================

/// Granularity of time-axis slicing for the planning board
type PlanningGranularity =
    | Day
    | Week
    | Month
    | Quarter

/// A typed planning period — the bucket key for all five projection planes.
/// Day is the finest grain; Week/Month/Quarter roll up from Day aggregations.
type PlanningPeriod =
    | PlanningDay of DateOnly
    | PlanningWeek of year: int * isoWeek: int
    | PlanningMonth of year: int * month: int
    | PlanningQuarter of year: int * quarter: int

type PlanningHorizonQueryDto =
    { PlantId: string
      StartDate: DateOnly
      EndDate: DateOnly
      Granularity: string // "Day" | "Week" | "Month" | "Quarter"
      Context: string // "Live" | "Scenario:scenarioId"
      SkuFilter: string list option
      ResourceFilter: string list option }

type KpiQueryDto =
    { PlantId: string
      Periods: string list
      Context: string
      SkuFilter: string list option }

type PlanContext =
    | Live
    | Scenario of scenarioId: string
