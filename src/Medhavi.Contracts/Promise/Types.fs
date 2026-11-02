namespace Medhavi.Contracts.Promise

open System

/// Governed order line promise evaluation statuses
[<RequireQualifiedAccess>]
type PromiseEvaluationStatus =
    | Pending
    | Evaluated
    | Committed
    | Quarantined
    | Rejected

/// Governed fulfillment option categories
[<RequireQualifiedAccess>]
type FulfillmentOptionCategory =
    | SingleNodeImmediate
    | MultiNodeSplit
    | InboundReplenishment
    | CapableToPromiseManufactured
    | ProductSubstitution

/// Governed promise risk severity tiers
[<RequireQualifiedAccess>]
type PromiseRiskTier =
    | Low
    | Medium
    | High
    | Critical

/// Governed promise exception disruption types
[<RequireQualifiedAccess>]
type PromiseDisruptionType =
    | SupplyShortfall
    | CapacityOverload
    | CarrierDelay
    | AllocationExhaustion
    | CustomerRequestedChange

