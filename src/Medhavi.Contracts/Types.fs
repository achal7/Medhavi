namespace Medhavi.Contracts

open System
open System.Text.Json.Serialization
open System.Threading.Tasks

type UIEventLogItem =
    { EventId: string
      EventType: string
      Stream: string
      Timestamp: DateTimeOffset }

[<JsonFSharpConverter>]
type Role =
    | Planner
    | Supervisor
    | Manager
    | Administrator

type User = { Name: string; Role: Role }

[<JsonFSharpConverter>]
type ApiError =
    { Code: string
      Category: string
      Message: string }

type QueryService<'Entity, 'Id> =
    { GetAll: unit -> Task<'Entity list>
      GetById: 'Id -> Task<'Entity option>
      Exists: 'Id -> Task<bool>
      Filter: ('Entity -> bool) -> Task<'Entity list>
      SubscribeApiEvents: (obj -> unit) -> IDisposable }

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

    member this.ToDateTimeOffset() =
        match this with
        | PlanningDay d -> DateTimeOffset(d.ToDateTime(TimeOnly.MinValue))
        | PlanningWeek(y, w) -> DateTimeOffset(System.Globalization.ISOWeek.ToDateTime(y, w, DayOfWeek.Monday))
        | PlanningMonth(y, m) -> DateTimeOffset(DateTime(y, m, 1))
        | PlanningQuarter(y, q) -> DateTimeOffset(DateTime(y, (q - 1) * 3 + 1, 1))
