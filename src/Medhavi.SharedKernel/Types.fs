namespace Medhavi.SharedKernel

open System

/// Strongly typed product ID
type SkuId = SkuId of string

/// Strongly typed location/stocking point ID
type NodeId = NodeId of string

/// Strongly typed supplier ID
type SupplierId = SupplierId of string

/// Strongly typed order ID
type OrderId = OrderId of string

/// Strongly typed time bucket/period
type Period = {
    Start: DateTimeOffset
    End: DateTimeOffset
}

/// Strongly typed quantity measurement
type Qty = decimal

/// Core Domain Error type
type DomainError =
    | ValidationError of string
    | InvariantViolation of string
    | NotFound of string
    | ConcurrencyConflict of string
