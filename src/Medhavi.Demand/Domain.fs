namespace Medhavi.Demand

open System
open Medhavi.SharedKernel

/// Customer Order aggregate
type CustomerOrder =
    { OrderId: OrderId
      LineId: string
      SkuId: SkuId
      NodeId: NodeId
      Quantity: Quantity
      DueDate: Timestamp
      Priority: int // E.g., Gold = 1, Silver = 2, Bronze = 3
      IsExpedited: bool }

/// Demand Forecast aggregate
type Forecast =
    { ForecastId: string
      SkuId: SkuId
      NodeId: NodeId
      Quantity: Quantity
      PeriodStart: Timestamp
      PeriodEnd: Timestamp }

// =============================================================================
// APS Demand Types — Full demand model with standard APS date semantics
// =============================================================================

/// Classification of where a demand signal originated
type DemandCategory =
    | CustomerOrderDemand // hard demand from a confirmed sales order
    | SalesOrderForecast // statistical forecast before order confirmation
    | InterplantTransfer // demand from another plant/node in the network
    | ServicePart // spare parts / aftermarket demand
    | InternalConsumption // production self-consumption (e.g., components)

/// Demand fulfillment lifecycle
type DemandStatus =
    | Open
    | PartiallyFulfilled
    | Fulfilled
    | Cancelled
    | OnHold

/// Full APS demand line with all standard date fields.
/// In SAP APO, Kinaxis, o9 every demand line carries EDD/RDD/LDD/CDD/ActualDD.
/// The RequestedDeliveryDate is the primary planning target and projection bucket key.
type DemandLine =
    { DemandLineId: string
      DemandOrderId: string
      SkuId: SkuId
      StockingPointId: StockingPointId
      CustomerId: string
      Quantity: Quantity
      UnitOfMeasure: string
      // --- APS date fields ---
      OrderDate: DateTimeOffset
      EarliestDeliveryDate: DateTimeOffset option // customer's earliest acceptable date
      RequestedDeliveryDate: DateTimeOffset // primary target / projection bucket key
      LatestDeliveryDate: DateTimeOffset option // hard upper bound; after this = penalty
      ConfirmedDeliveryDate: DateTimeOffset option // APS promise result from planning
      ActualDeliveryDate: DateTimeOffset option // execution reality (for KPI retrospective)
      // --- Classification ---
      Priority: int // 1 = highest (drives SLA tier, optimizer priority)
      DemandCategory: DemandCategory
      IsFirm: bool // firm demand: APS cannot defer or cancel
      IsFrozen: bool // frozen period: can adjust qty but not cancel
      // --- Fulfillment state ---
      OpenQuantity: Quantity
      FulfilledQuantity: Quantity
      Status: DemandStatus }
