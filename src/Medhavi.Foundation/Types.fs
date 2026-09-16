namespace Medhavi.Foundation

open System

type RuleError = { RuleId: string; Message: string }

type SystemTimestamp =
    | Timestamp of DateTimeOffset

    static member (+)(Timestamp a, b: TimeSpan) = Timestamp((a + b).ToUniversalTime())
    static member (-)(Timestamp t1, Timestamp t2) = t1 - t2
    static member (-)(Timestamp a, b: TimeSpan) = Timestamp((a - b).ToUniversalTime())
    static member now = Timestamp(DateTimeOffset.UtcNow)

module SystemTimestamp =
    /// Create a Timestamp, enforcing UTC normalization
    let create (dto: DateTimeOffset) = Timestamp(dto.ToUniversalTime())
    let minValue = Timestamp(DateTimeOffset.MinValue.ToUniversalTime())
    let maxValue = Timestamp(DateTimeOffset.MaxValue.ToUniversalTime())
    let value (Timestamp v) = v

    let minOf (Timestamp a) (Timestamp b) = Timestamp((min a b).ToUniversalTime())
    let maxOf (Timestamp a) (Timestamp b) = Timestamp((max a b).ToUniversalTime())

    let add (Timestamp a) (span: TimeSpan) = Timestamp((a + span).ToUniversalTime())
    let subtract (Timestamp a) (span: TimeSpan) = Timestamp((a - span).ToUniversalTime())

    let isAfter (Timestamp a) (Timestamp b) = a > b
    let isBefore (Timestamp a) (Timestamp b) = a < b

[<Struct>]
type DurationMinutes = private DurationMinutes of decimal

module DurationMinutes =
    let create value =
        if value < 0m then
            Error "Duration cannot be negative."
        else
            Ok(DurationMinutes value)

    let value (DurationMinutes value) = value
    let zero = DurationMinutes 0m