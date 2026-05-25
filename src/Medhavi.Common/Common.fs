namespace Medhavi.Common

open System
open System.Text.Json.Serialization

type Timestamp =
    | Timestamp of DateTimeOffset

    static member (+)(Timestamp a, b: TimeSpan) = Timestamp(a + b)
    static member (-)(Timestamp t1, Timestamp t2) = t1 - t2

module Timestamp =
    let now = Timestamp(DateTimeOffset.UtcNow)
    let minValue = Timestamp(DateTimeOffset.MinValue)
    let maxValue = Timestamp(DateTimeOffset.MaxValue)
    let value (Timestamp v) = v

    let minOf (Timestamp a) (Timestamp b) = Timestamp(min a b)
    let maxOf (Timestamp a) (Timestamp b) = Timestamp(max a b)

    let add (Timestamp a) (span: TimeSpan) = Timestamp(a + span)
    let subtract (Timestamp a) (span: TimeSpan) = Timestamp(a - span)

    let isAfter (Timestamp a) (Timestamp b) = a > b
    let isBefore (Timestamp a) (Timestamp b) = a < b
