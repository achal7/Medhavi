namespace Medhavi.Scheduler.Planning.Domain

open System
open Medhavi.SharedKernel.ScenarioContracts

/// An immutable, comparable planning time bucket.
/// Wraps a DateTimeOffset snapped to the start of the period defined by Granularity.
[<Struct>]
type TimeBucket =
    private
        { StartDate: DateTimeOffset
          Granularity: TimeBucketGranularity }

module TimeBucket =

    /// Snap a DateTimeOffset to the start of the bucket for the given granularity.
    let private snapToStart (granularity: TimeBucketGranularity) (date: DateTimeOffset) : DateTimeOffset =
        match granularity with
        | Daily -> DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, date.Offset)
        | Weekly ->
            // Snap to Monday of the ISO week
            let dow = int date.DayOfWeek
            let offset = if dow = 0 then 6 else dow - 1 // Monday = 0

            DateTimeOffset(
                date.AddDays(-float offset).Year,
                date.AddDays(-float offset).Month,
                date.AddDays(-float offset).Day,
                0,
                0,
                0,
                date.Offset
            )
        | Monthly -> DateTimeOffset(date.Year, date.Month, 1, 0, 0, 0, date.Offset)

    let create (granularity: TimeBucketGranularity) (date: DateTimeOffset) : TimeBucket =
        { StartDate = snapToStart granularity date
          Granularity = granularity }

    let startDate (bucket: TimeBucket) : DateTimeOffset = bucket.StartDate

    let granularity (bucket: TimeBucket) : TimeBucketGranularity = bucket.Granularity

    /// End of bucket (exclusive start of next bucket).
    let endDate (bucket: TimeBucket) : DateTimeOffset =
        match bucket.Granularity with
        | Daily -> bucket.StartDate.AddDays(1.0)
        | Weekly -> bucket.StartDate.AddDays(7.0)
        | Monthly -> bucket.StartDate.AddMonths(1)

    /// Add a lead-time offset in calendar days, staying in the same granularity.
    let offsetByLeadTimeDays (leadTimeDays: int) (bucket: TimeBucket) : TimeBucket =
        create bucket.Granularity (bucket.StartDate.AddDays(-float leadTimeDays))

    /// Compare two buckets (chronological order).
    let compare (a: TimeBucket) (b: TimeBucket) : int = DateTimeOffset.Compare(a.StartDate, b.StartDate)

    /// True when date falls within [bucket.Start, bucket.End).
    let contains (date: DateTimeOffset) (bucket: TimeBucket) : bool = date >= bucket.StartDate && date < endDate bucket

    /// True when bucket `a` is strictly earlier than bucket `b`.
    let isBefore (a: TimeBucket) (b: TimeBucket) : bool = a.StartDate < b.StartDate
