module Medhavi.Contracts.Demand

open System
open System.Threading.Tasks
open Medhavi.Contracts.Projections

/// Risk classification for a demand line's on-time delivery status
type LatenessRisk =
    | OnTrack // ConfirmedDeliveryDate <= RequestedDeliveryDate
    | AtRisk of daysLate: int // late but within LatestDeliveryDate
    | Critical // past LatestDeliveryDate or no supply plan at all

/// Summary of a supply order pegged (linked) to a demand line
type PeggedSupplySummary =
    { SupplyOrderId: string
      SupplyType: string // "PlannedProduction", "PurchaseOrder", etc.
      Quantity: decimal
      PlannedDate: DateOnly }

/// Denormalized demand line for the planning board.
/// The RequestedDeliveryDate determines which PlanningPeriod bucket this line falls into.
type DemandLine =
    { DemandLineId: string
      DemandOrderId: string
      SkuId: string
      SkuCode: string
      SkuName: string
      CustomerId: string
      CustomerName: string
      StockingPointId: string
      Priority: int
      DemandCategory: string // "CustomerOrder" | "Forecast" etc.
      IsFirm: bool
      // --- Dates ---
      EarliestDeliveryDate: DateOnly option
      RequestedDeliveryDate: DateOnly // determines the bucket
      LatestDeliveryDate: DateOnly option
      ConfirmedDeliveryDate: DateOnly option
      // --- Quantities ---
      RequestedQty: decimal
      OpenQty: decimal
      FulfilledQty: decimal
      ConfirmedQty: decimal // APS promise — what has been planned
      ShortfallQty: decimal // max(0, OpenQty - ConfirmedQty)
      LatenessRisk: LatenessRisk
      Status: string
      UnitOfMeasure: string
      // --- Pegging ---
      PeggedSupply: PeggedSupplySummary list }

/// Aggregated demand view for a single PlanningPeriod.
type DemandPeriodView =
    { Period: Analytics.PlanningPeriod
      PlantId: string
      SkuId: string option // None = all SKUs aggregated
      TotalDemandQty: decimal
      FirmDemandQty: decimal // non-cancellable firm orders
      ForecastDemandQty: decimal // statistical forecast (softer)
      ConfirmedQty: decimal // APS has committed to this qty
      OpenShortfallQty: decimal // unmet demand
      DemandLines: DemandLine list
      // Feasibility window quantities
      EarliestPossibleQty: decimal // qty where EDD <= period end
      LatestAcceptableQty: decimal // qty where LDD >= period start
      AtRiskDemandCount: int
      CriticalDemandCount: int }

type DemandDefineReq =
    { DemandLineId: string
      DemandOrderId: string
      SkuId: string
      StockingPointId: string
      CustomerId: string
      Quantity: decimal
      UnitOfMeasure: string
      OrderDate: DateTimeOffset
      EarliestDeliveryDate: DateTimeOffset option
      RequestedDeliveryDate: DateTimeOffset
      LatestDeliveryDate: DateTimeOffset option
      ConfirmedDeliveryDate: DateTimeOffset option
      ActualDeliveryDate: DateTimeOffset option
      Priority: int
      DemandCategory: string
      IsFirm: bool
      IsFrozen: bool }

type FulfillDemandLineReq =
    { DemandLineId: string
      Quantity: decimal }

/// Notification emitted when a new demand line is created
type DemandCreatedNotification = { DemandLineId: string }

/// Notification emitted when an existing demand line is updated
type DemandUpdatedNotification = { DemandLineId: string }

/// Notification emitted when an existing demand line is deleted
type DemandDeletedNotification = { DemandLineId: string }

type DemandLineQueries = QueryService<DemandLine, string>

type DemandLineApi =
    { Define: DemandDefineReq -> Task<Result<unit, ApiError>>
      DefineBulk: DemandDefineReq list -> Task<Result<unit, ApiError>>
      Fulfill: FulfillDemandLineReq -> Task<Result<unit, ApiError>> }
