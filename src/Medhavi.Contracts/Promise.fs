module Medhavi.Contracts.Promise

open System
/// Order line for promise evaluation
type PromiseOrderLine =
    { LineId: string
      SkuId: string
      StockingPointId: string
      Quantity: decimal
      DueDate: DateTimeOffset
      Priority: int
      IsExpedited: bool
      Origin: string option
      Destination: string option }

/// Order for promise evaluation
type PromiseOrder =
    { OrderId: string
      Lines: PromiseOrderLine list
      CustomerId: string option
      RequestDate: DateTimeOffset }

/// Promise request
type PromiseRequest =
    { Order: PromiseOrder
      AsOfDate: DateTimeOffset
      CustomerTier: string option
      SkuTier: string option
      Currency: string option }

/// Reason codes for promise limiters
type PromiseReasonCode =
    | MaterialShortfall
    | MaterialReservationConflict
    | SafetyViolation
    | SupplierMOQ
    | SupplierLeadtimeExceeded
    | CapacityShortfall
    | CapacityLocked
    | CapacitySafetyBuffer
    | QtyDurationUnsupported
    | NoTransportLeg
    | NoTransportCapacity
    | CutoffMissed
    | RegulatoryBlocked
    | RoutingInvalid
    | RoutingCapacityFail
    | AlternateExhausted
    | FullOrderViolation
    | FullDeliveryViolation
    | CostCapExceeded
    | RiskCapExceeded
    | SearchTimeout
    | DataStale

/// Domains that can limit a promise decision
type PromiseLimiterDomain =
    | Material
    | Capacity
    | Transport
    | Supplier
    | Routing
    | Policy
    | System

/// Limiter output: domain + reason + suggestions for remediation
type PromiseLimiter =
    { Domain: PromiseLimiterDomain
      Code: PromiseReasonCode
      Message: string
      Suggestions: string list }

/// Promise date range with confidence intervals
type PromiseDateRange =
    { Earliest: DateTimeOffset // optimistic date (earliest possible arrival)
      Committed: DateTimeOffset // p50 date (most likely)
      Latest: DateTimeOffset } // conservative date (p95/p85 date)

/// Promise decision status
type PromiseDecisionStatus =
    | Accepted of PromiseDateRange option
    | Rejected of PromiseLimiter

type PromiseCostBreakdown =
    { MaterialCost: decimal
      ProductionCost: decimal
      TransportCost: decimal
      HoldingCost: decimal
      LatenessPenalty: decimal }
    member this.TotalCost =
        this.MaterialCost + this.ProductionCost + this.TransportCost + this.HoldingCost + this.LatenessPenalty

type PromiseRoutingChoice =
    { RoutingId: string
      AlternateUsed: bool
      EstimatedDuration: TimeSpan option
      Reliability: decimal option }

type PromiseEvaluationResponse =
    { Decision: PromiseDecisionStatus
      Routing: PromiseRoutingChoice option
      Cost: PromiseCostBreakdown option
      Confidence: float option
      Reservations: string list }
