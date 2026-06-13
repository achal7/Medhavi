module Medhavi.Analytics.PlanningPeriod

open System
open Medhavi.Contracts.Analytics


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
    | PlanningWeek(y, w) -> $"%d{y}-W%02d{w}"
    | PlanningMonth(y, m) -> $"%d{y}-%02d{m}"
    | PlanningQuarter(y, q) -> $"%d{y}-Q%d{q}"

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
              let isoCalendar =
                  System.Globalization.ISOWeek.GetYear(current.ToDateTime(TimeOnly.MinValue))

              let isoWeek =
                  System.Globalization.ISOWeek.GetWeekOfYear(current.ToDateTime(TimeOnly.MinValue))

              yield PlanningWeek(isoCalendar, isoWeek)
              current <- current.AddDays(7) ]
        |> List.distinctBy (fun p ->
            match p with
            | PlanningWeek(y, w) -> (y, w)
            | _ -> (0, 0))
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
