module Medhavi.Contracts.Supply

open System
open Medhavi.Contracts.Analytics

/// Classification of supply order types for aggregation
type SupplyType =
    | PlannedProductionOrder
    | FirmProductionOrder
    | PurchaseOrder
    | PlannedPurchaseOrder
    | InterplantTransferInbound
    | InterplantTransferOutbound
    | StockTransfer

type Inventory =
    { Id: string
      SkuId: string
      StockingPointId: string
      Quantity: decimal
      UnitOfMeasure: string
      InTransitInbound: decimal
      InTransitOutbound: decimal
      QualityHold: decimal
      Damaged: decimal
      AvailableToPromise: decimal
      Created: DateTimeOffset
      Modified: DateTimeOffset }

type SeasonalAdjustment =
    { PeriodStart: DateTimeOffset
      PeriodEnd: DateTimeOffset
      AdjustmentFactor: decimal }

type ReplenishmentPolicy =
    { Safety: decimal
      MinQty: decimal option
      MaxQty: decimal option
      CoverDays: decimal option
      LotSize: decimal option
      Expedite: bool }

type InventoryTarget =
    { Id: string
      SkuId: string
      StockingPointId: string
      ReplenishmentPolicy: ReplenishmentPolicy option
      SafetyStockQty: decimal option
      MinQty: decimal option
      MaxQty: decimal option
      TargetServiceLevel: decimal option
      CoverDays: decimal option
      SeasonalAdjustments: SeasonalAdjustment list
      EffectiveStart: DateTimeOffset option
      EffectiveEnd: DateTimeOffset option
      IsActive: bool }

type PriceTier =
    { TierNumber: int
      MinQuantity: decimal
      MaxQuantity: decimal option
      PricePerUnit: decimal
      Currency: string }

type SupplierPreference = { Priority: int; IsPreferred: bool }

type SupplierCapacityWindow =
    { WindowId: string
      StartDate: DateTimeOffset
      EndDate: DateTimeOffset
      MaxQuantity: decimal
      AvailableQuantity: decimal }

type SupplierOffer =
    { Id: string
      SupplierId: string
      SkuId: string
      StockingPointId: string option
      Moq: decimal option
      LotSize: decimal option
      LeadTimeP50Minutes: float option
      LeadTimeP95Minutes: float option
      PriceTiers: PriceTier list
      Reliability: decimal option
      Incoterm: string option
      CapacityWindows: SupplierCapacityWindow list
      IsActive: bool }

type SupplyOrder =
    { Id: string
      OrderType: string
      SkuId: string
      StockingPointId: string
      Quantity: decimal
      UnitOfMeasure: string
      State: string
      RoutingId: string option
      SupplierId: string option
      IsFirm: bool
      IsExpedited: bool
      IsLocked: bool
      UsesLeadTimeQuantity: bool
      RequiredDeliveryDate: DateTimeOffset option
      CreatedDate: DateTimeOffset
      ModifiedDate: DateTimeOffset
      CompletedQuantity: decimal
      ScrapQuantity: decimal }

/// A single supply element (work order, PO, transfer) for drill-down
type SupplyElementView =
    { SupplyOrderId: string
      SupplyType: SupplyType
      SkuId: string
      SkuCode: string
      StockingPointId: string
      PlannedQty: decimal
      ConfirmedQty: decimal
      PlannedDate: DateOnly
      IsFirm: bool
      IsLocked: bool
      IsExpedited: bool
      RoutingId: string option
      SupplierId: string option
      LeadTimeDays: decimal option }

/// Point-in-time inventory snapshot for a SKU at a stocking point
type InventorySnapshot =
    { SkuId: string
      StockingPointId: string
      OnHandQty: decimal
      AvailableToPromise: decimal // on-hand minus active reservations
      QualityHoldQty: decimal
      DamagedQty: decimal
      InTransitInboundQty: decimal
      InTransitOutboundQty: decimal
      SafetyStockQty: decimal
      MaxStockQty: decimal option
      DaysOfSupply: decimal
      SnapshotDate: DateOnly }

type MaterialReservation =
    { Id: string
      IdempotencyKey: string
      SkuId: string
      StockingPointId: string
      Quantity: decimal
      State: string // "Tentative", "Confirmed", "Released", "Expired", "Reduced"
      RequiredDate: DateTimeOffset
      ExpiryTime: DateTimeOffset
      Created: DateTimeOffset
      Modified: DateTimeOffset }

type MaterialSnapshot =
    { OnHand: decimal
      Inbound: (DateTimeOffset * decimal) list
      Reservations: (DateTimeOffset * decimal) list
      Safety: decimal }

/// Aggregated material view for a single PlanningPeriod.
type MaterialPeriodView =
    { Period: PlanningPeriod
      SkuId: string
      SkuCode: string
      SkuName: string
      PlantId: string
      StockingPointId: string
      // Opening / closing inventory
      OpeningStock: decimal // start-of-period on-hand
      ClosingStock: decimal // = Opening + Receipts - Issues
      ProjectedStock: decimal // after all planned supply actions
      // Receipts (inbound supply)
      PlannedProduction: decimal
      FirmProduction: decimal
      PlannedPurchases: decimal
      FirmPurchases: decimal
      InboundTransfers: decimal
      TotalReceipts: decimal
      // Issues (outbound consumption)
      DemandConsumption: decimal
      ForecastConsumption: decimal
      OutboundTransfers: decimal
      TotalIssues: decimal
      // Safety stock status
      SafetyStockQty: decimal
      IsBelowSafetyStock: bool
      SafetyStockGap: decimal // max(0, SafetyStockQty - ProjectedStock)
      // Detail
      SupplyElements: SupplyElementView list
      InventorySnapshots: InventorySnapshot list
      // Capacity constraint (populated by Capacity plane)
      MaxProducibleQty: decimal }


type InventoryDefineReq =
    { Id: string
      SkuId: string
      StockingPointId: string
      Quantity: decimal
      UnitOfMeasure: string }

type InventoryTargetDefineReq =
    { SkuId: string
      StockingPointId: string
      ReplenishmentPolicy: ReplenishmentPolicy option
      SafetyStockQty: decimal option
      MinQty: decimal option
      MaxQty: decimal option
      TargetServiceLevel: decimal option
      CoverDays: decimal option
      SeasonalAdjustments: SeasonalAdjustment list
      EffectiveStart: DateTimeOffset option
      EffectiveEnd: DateTimeOffset option
      IsActive: bool }

type InventoryTargetUpdateReq =
    { Id: string
      SkuId: string
      StockingPointId: string
      ReplenishmentPolicy: ReplenishmentPolicy option
      SafetyStockQty: decimal option
      MinQty: decimal option
      MaxQty: decimal option
      TargetServiceLevel: decimal option
      CoverDays: decimal option
      SeasonalAdjustments: SeasonalAdjustment list option
      EffectiveStart: DateTimeOffset option
      EffectiveEnd: DateTimeOffset option }

type PriceTierReq =
    { TierNumber: int
      MinQuantity: decimal
      MaxQuantity: decimal option
      PricePerUnit: decimal
      Currency: string }

type CapacityWindowReq =
    { WindowId: string
      StartDate: DateTimeOffset
      EndDate: DateTimeOffset
      MaxQuantity: decimal
      AvailableQuantity: decimal }

type SupplierOfferDefineReq =
    { Id: string
      SupplierId: string
      SkuId: string
      StockingPointId: string option
      Moq: decimal option
      LotSize: decimal option
      LeadTimeP50Minutes: decimal option
      LeadTimeP95Minutes: decimal option
      PriceTiers: PriceTierReq list
      Reliability: decimal option
      Incoterm: string option
      CapacityWindows: CapacityWindowReq list
      CreatedDate: DateTimeOffset }

type SupplierOfferUpdateReq =
    { Id: string
      Moq: decimal option
      LotSize: decimal option
      LeadTimeP50Minutes: decimal option
      LeadTimeP95Minutes: decimal option
      PriceTiers: PriceTierReq list option
      Reliability: decimal option
      Incoterm: string option
      CapacityWindows: CapacityWindowReq list option
      ModifiedDate: DateTimeOffset }

type SupplierOfferChangeStatusReq =
    { Id: string
      IsActive: bool
      ModifiedDate: DateTimeOffset }

type SupplyOrderCreateReq =
    { Id: string
      OrderType: string
      SkuId: string
      StockingPointId: string
      Quantity: decimal
      UnitOfMeasure: string
      RoutingId: string option
      SupplierId: string option
      IsFirm: bool
      IsExpedited: bool
      IsLocked: bool
      UsesLeadTimeQuantity: bool
      RequiredDeliveryDate: DateTimeOffset option
      CreatedDate: DateTimeOffset }

type SupplyOrderStartReq =
    { Id: string
      StartedDate: DateTimeOffset }

type SupplyOrderPartialCompleteReq =
    { Id: string
      CompletedQuantity: decimal
      ScrapQuantity: decimal
      CompletedDate: DateTimeOffset
      FeedbackId: string option }

type SupplyOrderCompleteReq =
    { Id: string
      ScrapQuantity: decimal
      CompletedDate: DateTimeOffset
      FeedbackId: string option }

type SupplyOrderPlanReq =
    { Id: string
      PlannedDeliveryDate: DateTimeOffset }

type SupplyOrderConfirmReq =
    { Id: string
      ConfirmedDate: DateTimeOffset }

type SupplyOrderReleaseReq =
    { Id: string
      ReleasedDate: DateTimeOffset }

type SupplyOrderCancelReq =
    { Id: string
      CancelledDate: DateTimeOffset }

type SupplyOrderLockReq =
    { Id: string
      Locked: bool
      ModifiedDate: DateTimeOffset }

type SupplyOrderUpdateReq =
    { SupplyOrderId: string
      ProductId: string
      StockingPointId: string
      Quantity: decimal
      ExpectedDeliveryUtc: DateTimeOffset
      Status: string }

type MaterialReservationCreateReq =
    { Id: string
      IdempotencyKey: string
      SkuId: string
      StockingPointId: string
      Quantity: decimal
      RequiredDate: DateTimeOffset
      ExpiryTime: DateTimeOffset }

type MaterialReservationConfirmReq = { Id: string }
type MaterialReservationReleaseReq = { Id: string }
type MaterialReservationReduceReq = { Id: string; NewQuantity: decimal }
type MaterialReservationExpireReq = { Id: string }
