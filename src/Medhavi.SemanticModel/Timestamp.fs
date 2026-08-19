namespace Medhavi.SemanticModel

open System

/// SE-C-022 Timestamp
/// Business Intent: Provide the single authoritative representation of a point in time,
/// expressed in UTC, for all enterprise facts that record when something occurred or was observed.
[<Struct>]
type Timestamp = private Timestamp of DateTimeOffset

module Timestamp =
    /// Creates a Timestamp, enforcing strict UTC normalization.
    let create (dto: DateTimeOffset) : Result<Timestamp, SemanticValidationError> =
        if dto.Offset = TimeSpan.Zero then
            Ok(Timestamp dto)
        else
            // Force conversion to UTC to guarantee semantic consistency
            Ok(Timestamp(dto.ToUniversalTime()))

    /// Creates a Timestamp from a known, trusted UTC DateTimeOffset.
    let ofUtc (dto: DateTimeOffset) : Timestamp = Timestamp(dto.ToUniversalTime())

    /// Extracts the underlying DateTimeOffset.
    let value (Timestamp t) = t

    let minValue = Timestamp <| DateTimeOffset.MinValue.ToUniversalTime()
    let maxValue = Timestamp <| DateTimeOffset.MaxValue.ToUniversalTime()

    /// Calculates the duration between two timestamps.
    let diff (t1: Timestamp) (t2: Timestamp) : TimeSpan = (value t1) - (value t2)

    /// Adds a TimeSpan to a Timestamp.
    let add (t: Timestamp) (span: TimeSpan) : Timestamp = Timestamp((value t).Add(span).ToUniversalTime())

    let addDays (days: float) (t: Timestamp) : Timestamp = Timestamp((value t).AddDays(days).ToUniversalTime())
    /// Subtracts a TimeSpan from a Timestamp.
    let subtract (t: Timestamp) (span: TimeSpan) : Timestamp = Timestamp((value t).Add(span.Negate()).ToUniversalTime())

    let isAfter (t1: Timestamp) (t2: Timestamp) = (value t1) > (value t2)
    let isBefore (t1: Timestamp) (t2: Timestamp) = (value t1) < (value t2)
    let isEqual (t1: Timestamp) (t2: Timestamp) = (value t1) = (value t2)

    let now () = ofUtc DateTimeOffset.UtcNow
