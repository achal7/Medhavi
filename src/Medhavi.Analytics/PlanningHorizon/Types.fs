namespace Medhavi.Analytics.PlanningHorizon

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

module PlanningPeriod =

    /// Start date of the period (inclusive)
    let startDate =
        function
        | PlanningDay d -> d
        | PlanningWeek(y, w) ->
            // ISO 8601: week 1 = week containing the first Thursday
            let jan4 = DateOnly(y, 1, 4)

            let dowOffset =
                int jan4.DayOfWeek
                |> fun d -> if d = 0 then 6 else d - 1

            jan4.AddDays(int ((w - 1) * 7 - dowOffset))
        | PlanningMonth(y, m) -> DateOnly(y, m, 1)
        | PlanningQuarter(y, q) -> DateOnly(y, (q - 1) * 3 + 1, 1)

    /// End date of the period (inclusive)
    let endDate =
        function
        | PlanningDay d -> d
        | PlanningWeek _ as p -> (startDate p).AddDays(6)
        | PlanningMonth(y, m) -> DateOnly(y, m, DateTime.DaysInMonth(y, m))
        | PlanningQuarter(y, q) ->
            let em = q * 3
            DateOnly(y, em, DateTime.DaysInMonth(y, em))

    /// Does a date fall inside this period? (inclusive both ends)
    let contains (date: DateOnly) (period: PlanningPeriod) = date >= startDate period && date <= endDate period

    /// Human-readable label for UI display
    let label =
        function
        | PlanningDay d -> d.ToString("yyyy-MM-dd")
        | PlanningWeek(y, w) -> sprintf "%d-W%02d" y w
        | PlanningMonth(y, m) -> sprintf "%d-%02d" y m
        | PlanningQuarter(y, q) -> sprintf "%d-Q%d" y q

// =============================================================================
// Plan Context — live data vs scenario what-if
// =============================================================================

/// Determines whether a projection reads from the committed live plan
/// or overlays scenario what-if deltas on top.
type PlanContext =
    | Live
    | Scenario of scenarioId: string
