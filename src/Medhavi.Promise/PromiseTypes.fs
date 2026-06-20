module Medhavi.Promise.PromiseTypes

open System
open Medhavi.SharedKernel
open Medhavi.Contracts.Transport
open Medhavi.Contracts.Promise

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
type PromiseRequestCmd =
    { Order: Order
      AsOfDate: DateTimeOffset
      CustomerTier: string option
      SkuTier: string option
      Currency: string option }

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
      Reliability: decimal option
      Moq: decimal option
      LeadTimeP50: TimeSpan option
      LeadTimeP95: TimeSpan option
      Incoterm: string option }

/// Routing choice (primary or alternate)
type RoutingChoice =
    { RoutingId: RoutingId
      AlternateUsed: bool
      EstimatedDuration: TimeSpan option
      Reliability: decimal option }

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
      Routing: RoutingChoice option
      Itinerary: Itinerary option
      Material: MaterialSnapshot option
      Cost: CostBreakdown option
      Confidence: float option
      Reservations: ReservationId list
      Meta: Map<string, obj> }

/// Promise result (for multi-line handling)
type PromiseResult =
    { Responses: PromiseResponse list }

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
