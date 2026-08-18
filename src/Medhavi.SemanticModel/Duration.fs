namespace Medhavi.SemanticModel

[<Measure>]
type mins

[<Measure>]
type hrs

[<Measure>]
type days

/// Represents semantic validation errors that can occur during Duration operations.
type DurationValidationError =
    | NegativeDuration of value: decimal
    | DivisionByZero

/// SE-C-024 Duration - Represents a strictly non-negative duration parameterized by a unit of measure.
[<Struct>]
type Duration<[<Measure>] 'u> = private Duration of decimal<'u>

[<RequireQualifiedAccess>]
module Duration =
    open System

    // ---------- Constants ----------
    let MinsPerHour = 60.0m<mins / hrs>
    let HoursPerDay = 24.0m<hrs / days>
    let MinsPerDay = 1440.0m<mins / days>

    // ---------- Creation ----------

    let create (value: decimal<'u>) : Result<Duration<'u>, DurationValidationError> =
        if value < LanguagePrimitives.GenericZero then
            Error(NegativeDuration(decimal value))
        else
            Ok(Duration value)

    /// Creates a Duration representing a specific number of minutes.
    let fromMinutes (value: decimal) : Result<Duration<mins>, DurationValidationError> = create(value * 1.0m<mins>)

    /// Creates a Duration representing a specific number of hours.
    let fromHours (value: decimal) : Result<Duration<hrs>, DurationValidationError> = create(value * 1.0m<hrs>)

    /// Creates a Duration representing a specific number of days.
    let fromDays (value: decimal) : Result<Duration<days>, DurationValidationError> = create(value * 1.0m<days>)

    // ---------- Zero ----------

    let zeroMins = Duration 0.0m<mins>
    let zeroHours = Duration 0.0m<hrs>
    let zeroDays = Duration 0.0m<days>

    // ---------- Accessors ----------

    /// Extracts the underlying decimal value with its unit of measure.
    let value (Duration v) = v

    /// Extracts the underlying decimal value, stripping the unit of measure.
    let toDecimal (Duration v) = decimal v

    // ---------- Math ----------

    /// Adds two durations of the same unit.
    let add (a: Duration<'u>) (b: Duration<'u>) : Duration<'u> =
        let (Duration va) = a
        let (Duration vb) = b
        Duration(va + vb)

    /// Subtracts one duration from another of the same unit. Returns an error if the result would be negative.
    let subtract (a: Duration<'u>) (b: Duration<'u>) : Result<Duration<'u>, DurationValidationError> =
        let (Duration va) = a
        let (Duration vb) = b
        let res = va - vb

        if res < LanguagePrimitives.GenericZero then
            Error(NegativeDuration(decimal res))
        else
            Ok(Duration res)

    /// Multiplies a duration by a dimensionless scalar. Returns an error if the scalar is negative.
    let multiply (scalar: decimal) (d: Duration<'u>) : Result<Duration<'u>, DurationValidationError> =
        if scalar < 0.0m then
            Error(NegativeDuration scalar)
        else
            let (Duration v) = d
            Ok(Duration(v * scalar))

    /// Divides a duration by a dimensionless divisor. Returns an error if the divisor is zero.
    let divide (divisor: decimal) (d: Duration<'u>) : Result<Duration<'u>, DurationValidationError> =
        if divisor = 0.0m then
            Error DivisionByZero
        else
            let (Duration v) = d
            Ok(Duration(v / divisor))

    // ---------- Conversions (Time Units) ----------

    let toHours (d: Duration<mins>) : Duration<hrs> =
        let (Duration v) = d
        Duration(v / MinsPerHour)

    let toDaysFromHours (d: Duration<hrs>) : Duration<days> =
        let (Duration v) = d
        Duration(v / HoursPerDay)

    let toDaysFromMins (d: Duration<mins>) : Duration<days> =
        let (Duration v) = d
        Duration(v / MinsPerDay)

    let toMinutesFromHours (d: Duration<hrs>) : Duration<mins> =
        let (Duration v) = d
        Duration(v * MinsPerHour)

    let toMinutesFromDays (d: Duration<days>) : Duration<mins> =
        let (Duration v) = d
        Duration(v * MinsPerDay)

    let toHoursFromDays (d: Duration<days>) : Duration<hrs> =
        let (Duration v) = d
        Duration(v * HoursPerDay)

    // ---------- Conversions (Standard Types) ----------

    let toTimeSpanFromMins (d: Duration<mins>) : TimeSpan =
        let v = value d
        let dimensionlessValue = v / 1.0m<mins>
        let ticks = int64(dimensionlessValue * decimal TimeSpan.TicksPerMinute)
        TimeSpan(ticks)

    let toTimeSpanFromHours (d: Duration<hrs>) : TimeSpan =
        let v = value d
        let dimensionlessValue = v / 1.0m<hrs>
        let ticks = int64(dimensionlessValue * decimal TimeSpan.TicksPerHour)
        TimeSpan(ticks)

    let toTimeSpanFromDays (d: Duration<days>) : TimeSpan =
        let v = value d
        let dimensionlessValue = v / 1.0m<days>
        let ticks = int64(dimensionlessValue * decimal TimeSpan.TicksPerDay)
        TimeSpan(ticks)

    let fromTimeSpan (ts: TimeSpan) : Result<Duration<mins>, DurationValidationError> =
        fromMinutes(decimal ts.TotalMinutes)

    // ---------- Comparison ----------

    let compare (a: Duration<'u>) (b: Duration<'u>) : int =
        let (Duration va) = a
        let (Duration vb) = b
        compare va vb

    let isGreaterThan (a: Duration<'u>) (b: Duration<'u>) : bool = compare a b > 0

    let isLessThan (a: Duration<'u>) (b: Duration<'u>) : bool = compare a b < 0

    let isEqual (a: Duration<'u>) (b: Duration<'u>) : bool = compare a b = 0

    // ---------- Formatting ----------

    let toString (d: Duration<'u>) : string =
        let (Duration v) = d
        (v / 1.0m<_>).ToString()
