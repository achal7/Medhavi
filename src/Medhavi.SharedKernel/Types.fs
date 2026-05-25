namespace Medhavi.SharedKernel

open System
open System.Text.Json.Serialization

[<JsonFSharpConverter>]
type Money = { Amount: decimal; Currency: string }

[<JsonFSharpConverter>]
type Version = private Version of int

module Version =
    let initial = Version 1
    let increment (Version v) = Version(v + 1)
    let value (Version v) = v
    let equals (Version v1) (Version v2) = v1 = v2

    let create value =
        if value < 0 then
            Error "Version must be non-negative"
        else
            Ok(Version value)

/// Versioned aggregate for optimistic concurrency
type VersionedAggregate<'Aggregate> = { Aggregate: 'Aggregate; Version: int }

[<Struct>]
[<JsonFSharpConverter>]
type Qty =
    private
    | Qty of decimal

    static member Zero = Qty 0m

    static member (+)(Qty a, Qty b) = Qty(a + b)

    static member (-)(Qty a, Qty b) = Qty(a - b)

    static member (*)(Qty a, scalar: decimal) = Qty(a * scalar)

    static member (/)(Qty a, scalar: decimal) = Qty(a / scalar)

    static member (~-)(Qty a) = Qty(-a)

    static member op_Explicit(Qty a) : decimal = a

module Qty =

    let create value = Qty value

    let value (Qty v) = v

    let zero = Qty.Zero

    let abs (Qty v) = Qty(abs v)

    let sum (items: Qty seq) = Seq.fold (+) zero items

    let createOrDefault (value: decimal) : Qty = if value < 0m then Qty.Zero else Qty value

    // Utility functions
    let isZero (Qty v) = v = 0m
    let isPositive (Qty v) = v > 0m

    let minOf (Qty a) (Qty b) = Qty(min a b)
    let maxOf (Qty a) (Qty b) = Qty(max a b)

    /// Safe subtraction - clamps to zero if result would be negative
    let subtract (Qty a) (Qty b) = Qty(max 0m (a - b))

    /// Try subtract - returns Error if result would be negative
    let trySubtract (Qty a) (Qty b) : Result<Qty, DomainError> =
        if a >= b then
            Ok(Qty(a - b))
        else
            Error(DomainError.validation ("Subtraction would result in negative quantity"))

    /// Ratio between two quantities (a / b)
    let ratio (Qty a) (Qty b) : Result<decimal, DomainError> =
        if b = 0m then
            Error(DomainError.validation "Division by zero quantity is not allowed")
        else
            Ok(a / b)

    /// Scale by a factor
    let scale (factor: decimal) (Qty v) = Qty(v * factor)

[<JsonFSharpConverter>]
type NonNegativeQty = private NonNegativeQty of Qty

module NonNegativeQty =
    let create (value: decimal) : Result<NonNegativeQty, DomainError> =
        if value < 0m then
            Error(DomainError.validation "Quantity must be non-negative")
        else
            Ok(NonNegativeQty(Qty value))

    let value (NonNegativeQty v) = v

[<JsonFSharpConverter>]
type PositiveQty = private PositiveQty of Qty

module PositiveQty =
    let create (value: decimal) : Result<PositiveQty, DomainError> =
        if value <= 0m then
            Error(DomainError.validation "Quantity must be positive")
        else
            Ok(PositiveQty(Qty value))

    let value (PositiveQty v) = v

/// Positive decimal (>= 0m)
[<Struct>]
[<JsonFSharpConverter>]
type PositiveDecimal =
    private
    | PositiveDecimal of decimal

    static member create(value: decimal) =
        if value < 0m then
            Error(DomainError.validation "Value must be non-negative")
        else
            Ok(PositiveDecimal value)

    static member value(PositiveDecimal v) = v

    /// Required by SRTP (List.sum / sumBy)
    static member Zero = PositiveDecimal 0m

    /// Arithmetic
    static member (+)(PositiveDecimal a, PositiveDecimal b) = PositiveDecimal(a + b)

    static member (-)(PositiveDecimal a, PositiveDecimal b) =
        if a - b < 0m then
            failwith "PositiveDecimal subtraction underflow"
        else
            PositiveDecimal(a - b)

    static member (*)(PositiveDecimal v, scalar: decimal) = PositiveDecimal(v * scalar)

    static member (*)(scalar: decimal, PositiveDecimal v) = PositiveDecimal(scalar * v)

    static member (/)(PositiveDecimal v, scalar: decimal) = PositiveDecimal(v / scalar)

    static member op_Multiply(PositiveDecimal a, PositiveDecimal b) = PositiveDecimal(a * b)

/// Percent in the range [0.0, 1.0]
[<JsonFSharpConverter>]
type Percent = private Percent of decimal

module Percent =
    let create (value: decimal) =
        if value < 0m || value > 1m then
            Error(DomainError.validation "Percent must be between 0.0 and 1.0")
        else
            Ok(Percent value)

    let value (Percent v) = v

[<JsonFSharpConverter>]
type Timestamp =
    | Timestamp of DateTimeOffset

    static member (+)(Timestamp a, b: TimeSpan) = Timestamp(a + b)
    static member (-)(Timestamp t1, Timestamp t2) = t1 - t2
    static member now = Timestamp(DateTimeOffset.UtcNow)

module Timestamp =
    let minValue = Timestamp(DateTimeOffset.MinValue)
    let maxValue = Timestamp(DateTimeOffset.MaxValue)
    let value (Timestamp v) = v

    let minOf (Timestamp a) (Timestamp b) = Timestamp(min a b)
    let maxOf (Timestamp a) (Timestamp b) = Timestamp(max a b)

    let add (Timestamp a) (span: TimeSpan) = Timestamp(a + span)
    let subtract (Timestamp a) (span: TimeSpan) = Timestamp(a - span)

    let isAfter (Timestamp a) (Timestamp b) = a > b
    let isBefore (Timestamp a) (Timestamp b) = a < b

[<JsonFSharpConverter>]
type Window =
    private
        { StartVal: Timestamp
          EndVal: Timestamp }

    member this.Start = this.StartVal
    member this.End = this.EndVal

module Window =
    let overlaps (a: Window) (b: Window) = a.Start < b.End && b.Start < a.End

    let contains (outer: Window) (inner: Window) =
        outer.Start <= inner.Start
        && outer.End >= inner.End

    let applySlack (slack: TimeSpan) (w: Window) =
        { StartVal = w.Start
          EndVal = w.End + slack }

    let isBefore (t: Timestamp) (w: Window) = t < w.Start

    /// Validate cutoff: departure must be >= earliest and before cutoff end.
    let meetsCutoff (earliest: DateTimeOffset) (cutoffEnd: DateTimeOffset) (departure: DateTimeOffset) =
        departure >= earliest && departure <= cutoffEnd

    let startTime (window: Window) = Timestamp.value window.Start
    let endTime (window: Window) = Timestamp.value window.End
    let duration (window: Window) = window.End - window.Start

    let createFromTime (startTime: DateTimeOffset) (endTime: DateTimeOffset) =
        match startTime < endTime with
        | true ->
            Ok
                { StartVal = Timestamp startTime
                  EndVal = Timestamp endTime }
        | false -> Error(DomainError.validation $"Start {startTime} is after end time {endTime} ")

    let create (startTime: Timestamp) (endTime: Timestamp) =
        createFromTime (Timestamp.value startTime) (Timestamp.value endTime)
