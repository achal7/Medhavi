namespace Medhavi.Analytics.PlanningHorizon

open System
open System.Threading.Tasks

// =============================================================================
// Planning Horizon Query Service — the main entry point for projections
// =============================================================================

/// Request parameters for a planning horizon query
type PlanningHorizonRequest =
    {
        PlantId     : string
        StartDate   : DateOnly
        EndDate     : DateOnly
        Granularity : PlanningGranularity
        Context     : PlanContext
        SkuFilter   : string list option        // None = all SKUs
        ResourceFilter : string list option     // None = all resource groups
    }

/// Full planning horizon response — all five planes
type PlanningHorizonResponse =
    {
        Request    : PlanningHorizonRequest
        Periods    : PlanningPeriod list
        Demand     : DemandPeriodView list
        Material   : MaterialPeriodView list
        Capacity   : CapacityPeriodView list
        Transport  : TransportPeriodView list
        GanttGrid  : GanttGrid
        GeneratedAt: DateTimeOffset
    }

/// Query service record (functional injection pattern).
/// Concrete implementation created in Medhavi.Nexus composition root.
type PlanningHorizonQueryService =
    {
        /// Get the full planning horizon across all five planes
        GetPlanningHorizon : PlanningHorizonRequest -> Task<PlanningHorizonResponse>
        /// Get only the demand plane (lighter-weight)
        GetDemandPlane     : PlanningHorizonRequest -> Task<DemandPeriodView list>
        /// Get only the material plane
        GetMaterialPlane   : PlanningHorizonRequest -> Task<MaterialPeriodView list>
        /// Get only the capacity/Gantt plane
        GetCapacityPlane   : PlanningHorizonRequest -> Task<GanttGrid>
        /// Get only the transport plane
        GetTransportPlane  : PlanningHorizonRequest -> Task<TransportPeriodView list>
    }

// =============================================================================
// Period Generation Utilities
// =============================================================================

module PeriodGenerator =

    /// Generate a sequence of PlanningPeriod values covering a date range
    let generate (granularity: PlanningGranularity) (startDate: DateOnly) (endDate: DateOnly) : PlanningPeriod list =
        match granularity with
        | Day ->
            let mutable current = startDate
            [ while current <= endDate do
                yield PlanningDay current
                current <- current.AddDays(1) ]
        | Week ->
            let mutable current = startDate
            [ while current <= endDate do
                let isoCalendar = System.Globalization.ISOWeek.GetYear(current.ToDateTime(TimeOnly.MinValue))
                let isoWeek = System.Globalization.ISOWeek.GetWeekOfYear(current.ToDateTime(TimeOnly.MinValue))
                yield PlanningWeek(isoCalendar, isoWeek)
                current <- current.AddDays(7) ]
            |> List.distinctBy (fun p -> match p with PlanningWeek(y, w) -> (y, w) | _ -> (0, 0))
        | Month ->
            let mutable y = startDate.Year
            let mutable m = startDate.Month
            [ while DateOnly(y, m, 1) <= endDate do
                yield PlanningMonth(y, m)
                m <- m + 1
                if m > 12 then
                    m <- 1
                    y <- y + 1 ]
        | Quarter ->
            let mutable y = startDate.Year
            let mutable q = (startDate.Month - 1) / 3 + 1
            [ while DateOnly(y, (q - 1) * 3 + 1, 1) <= endDate do
                yield PlanningQuarter(y, q)
                q <- q + 1
                if q > 4 then
                    q <- 1
                    y <- y + 1 ]
