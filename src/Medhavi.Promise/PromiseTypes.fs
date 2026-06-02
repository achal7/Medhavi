module Medhavi.Promise.PromiseTypes

open System
open Medhavi.SharedKernel
open Medhavi.Transport

/// Promise date range with confidence intervals as specified in PDD
type PromiseDateRange =
    { Earliest: DateTimeOffset  // optimistic date (earliest possible arrival)
      Committed: DateTimeOffset  // p50 date (most likely)
      Latest: DateTimeOffset }   // conservative date (p95/p85 date)

/// Cost breakdown for a promise evaluation
type CostBreakdown =
    { MaterialCost: decimal
      ProductionCost: decimal
      TransportCost: decimal
      HoldingCost: decimal
      LatenessPenalty: decimal }

    member this.TotalCost =
        this.MaterialCost + this.ProductionCost + this.TransportCost + this.HoldingCost + this.LatenessPenalty

module CostBreakdown =
    let create material production transport holding lateness =
        { MaterialCost = material
          ProductionCost = production
          TransportCost = transport
          HoldingCost = holding
          LatenessPenalty = lateness }

    let empty = create 0m 0m 0m 0m 0m

    let add a b =
        { MaterialCost = a.MaterialCost + b.MaterialCost
          ProductionCost = a.ProductionCost + b.ProductionCost
          TransportCost = a.TransportCost + b.TransportCost
          HoldingCost = a.HoldingCost + b.HoldingCost
          LatenessPenalty = a.LatenessPenalty + b.LatenessPenalty }

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

/// Promise decision status
type PromiseDecisionStatus =
    | Accepted
    | Rejected

/// Order line for promise evaluation
type OrderLine =
    { LineId: string
      SkuId: SkuId
      StockingPointId: StockingPointId
      Quantity: Quantity
      DueDate: DateTimeOffset
      Priority: int
      IsExpedited: bool
      Origin: StockingPointId option
      Destination: StockingPointId option }

/// Order for promise evaluation
type Order =
    { OrderId: OrderId
      Lines: OrderLine list
      CustomerId: string option
      RequestDate: DateTimeOffset }

/// Promise request
type PromiseRequest =
    { Order: Order
      AsOfDate: DateTimeOffset
      CustomerTier: string option
      SkuTier: string option }

/// Material snapshot used for promise calculations
type MaterialSnapshot =
    { SkuId: SkuId
      StockingPointId: StockingPointId
      OnHand: decimal
      Inbound: (DateTimeOffset * decimal) list
      Reservations: decimal
      Safety: decimal }

/// Supplier option for shortfall coverage
type SupplierOption =
    { SupplierId: string
      Earliest: DateTimeOffset
      Qty: decimal
      Cost: decimal
      Reliability: float option
      Moq: decimal option
      LeadTimeP50: TimeSpan option
      LeadTimeP95: TimeSpan option
      Incoterm: string option }

/// Routing choice (primary or alternate)
type RoutingChoice =
    { RoutingId: RoutingId
      AlternateUsed: bool
      EstimatedDuration: TimeSpan option
      Reliability: float option }

/// Routing selection (primary plus alternates)
type RoutingSelection =
    { Primary: RoutingChoice
      Alternates: RoutingChoice list }

/// Reservations across material, capacity, and transport
type ReservationScope =
    | Material
    | Capacity
    | Transport

/// Reservation request
type ReservationRequest =
    { Scope: ReservationScope
      Reference: string
      SkuId: SkuId
      StockingPointId: StockingPointId
      Quantity: decimal
      Duration: TimeSpan option
      WindowStart: DateTimeOffset
      WindowEnd: DateTimeOffset }

/// Reservation ID
type ReservationId = string

/// Promise response
type PromiseResponse =
    { Decision: PromiseDecisionStatus
      PromiseDate: PromiseDateRange option
      Limiter: PromiseLimiter option
      Routing: RoutingChoice option
      Itinerary: Itinerary option
      Material: MaterialSnapshot option
      Cost: CostBreakdown option
      Confidence: float option
      Reservations: ReservationId list
      Meta: Map<string, obj> }

/// Promise result (for multi-line handling)
type PromiseResult =
    { Responses: PromiseResponse list
      Limiters: PromiseLimiter list }

/// Provider errors used for degradation mapping
type ProviderError =
    | Timeout
    | Unavailable
    | StaleData
    | Unauthorized
    | ValidationFailed of string
    | NoTransportLeg
    | NoTransportCapacity
    | TransportCutoffMissed
    | TransportRegulatoryBlocked

/// Capacity check result
type CapacityCheckResult =
    { IsFeasible: bool
      SuggestedDate: DateTimeOffset
      RequiredLoads: Map<string, decimal> // stepId -> required minutes
      BottleneckResourceId: string option
      LatenessReason: string option
      EarliestAvailable: DateTimeOffset }

/// Providers (pluggable; full or light based on implementation)

/// Material provider interface
type MaterialProvider =
    { GetSnapshot: SkuId * StockingPointId * DateTimeOffset -> Async<Result<MaterialSnapshot, ProviderError>>
      GetSupplierOptions:
          SkuId * StockingPointId * decimal * DateTimeOffset -> Async<Result<SupplierOption list, ProviderError>> }

/// Capacity provider interface - aligns with Capacity CTP interface from PDD
type CapacityProvider =
    { CheckCapacity:
        SkuId * decimal * DateTimeOffset -> Async<Result<CapacityCheckResult, ProviderError>> }

/// Transport provider interface - aligns with Transport ATP interface from PDD
type TransportProvider =
    { GetOptions:
        string * string * DateTimeOffset -> Async<Result<Itinerary list, ProviderError>> }

/// Routing provider interface
type RoutingProvider =
    { Select: SkuId * StockingPointId -> Async<Result<RoutingSelection, ProviderError>> }

/// Reservation provider interface
type ReservationProvider =
    { CreateTentative: ReservationRequest list -> Async<Result<ReservationId list, ProviderError>>
      Confirm: ReservationId list -> Async<Result<unit, ProviderError>>
      Release: ReservationId list -> Async<Result<unit, ProviderError>> }

/// FX provider interface
type FxProvider =
    { GetRate: string * string * DateTimeOffset -> Async<Result<decimal option, ProviderError>> }

/// Tenant provider interface
type TenantProvider =
    { GetTenant: unit -> string * TimeZoneInfo * string option }