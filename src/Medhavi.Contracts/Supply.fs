module Medhavi.Contracts.Supply

open System
open System.Threading.Tasks
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

type InventoryDefineReq =
    { Id: string
      SkuId: string
      StockingPointId: string
      Quantity: decimal
      UnitOfMeasure: string }

type InventoryApi =
    { Define: InventoryDefineReq -> Task<Result<Inventory, ApiError>>
      DefineBulk: InventoryDefineReq list -> Task<Result<Inventory list, ApiError>>
      Remove: string -> Task<Result<Inventory, ApiError>> }

type InventoryQueryService = QueryService<Inventory, string>

// Inventory Target
// -----------------------------------------------------------------
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

type InventoryTargetApi =
    { Define: InventoryTargetDefineReq -> Task<Result<InventoryTarget, ApiError>>
      DefineBulk: InventoryTargetDefineReq list -> Task<Result<InventoryTarget list, ApiError>>
      Update: InventoryTargetUpdateReq -> Task<Result<InventoryTarget, ApiError>>
      Activate: string -> Task<Result<InventoryTarget, ApiError>>
      Deactivate: string -> Task<Result<InventoryTarget, ApiError>> }

type InventoryTargetQueryService = QueryService<InventoryTarget, string>

// Supplier Offer
// --------------------------------------------------------------
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

type SupplierOfferApi =
    { Define: SupplierOfferDefineReq -> Task<Result<SupplierOffer, ApiError>>
      DefineBulk: SupplierOfferDefineReq list -> Task<Result<SupplierOffer list, ApiError>>
      Update: SupplierOfferUpdateReq -> Task<Result<SupplierOffer, ApiError>>
      Revoke: string -> Task<Result<SupplierOffer, ApiError>>
      ChangeStatus: SupplierOfferChangeStatusReq -> Task<Result<SupplierOffer, ApiError>> }

type SupplierOfferQueryService = QueryService<SupplierOffer, string>

// Supply Order
// --------------------------------------------------------------------
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

type SupplyOrderApi =
    { Create: SupplyOrderCreateReq -> Task<Result<SupplyOrder, ApiError>>
      CreateBulk: SupplyOrderCreateReq list -> Task<Result<SupplyOrder list, ApiError>>
      ProcessStatusUpdates: SupplyOrderUpdateReq list -> Task<Result<SupplyOrder list, ApiError>>
      Start: SupplyOrderStartReq -> Task<Result<SupplyOrder, ApiError>>
      PartialComplete: SupplyOrderPartialCompleteReq -> Task<Result<SupplyOrder, ApiError>>
      Complete: SupplyOrderCompleteReq -> Task<Result<SupplyOrder, ApiError>>
      Plan: SupplyOrderPlanReq -> Task<Result<SupplyOrder, ApiError>>
      Confirm: SupplyOrderConfirmReq -> Task<Result<SupplyOrder, ApiError>>
      Release: SupplyOrderReleaseReq -> Task<Result<SupplyOrder, ApiError>>
      Cancel: SupplyOrderCancelReq -> Task<Result<SupplyOrder, ApiError>>
      Lock: SupplyOrderLockReq -> Task<Result<SupplyOrder, ApiError>>
      AutoFirmOrders: DateTimeOffset -> int -> Task<Result<unit, ApiError>> }

type SupplyOrderQueryService = QueryService<SupplyOrder, string>

// Material Reservation
// -------------------------------------------------------------------------
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

type MaterialProviderApi =
    { GetSnapshot: string -> string -> DateTimeOffset -> Async<Result<MaterialSnapshot, ApiError>>
      GetNetAvailable: string -> string -> DateTimeOffset -> Async<Result<decimal, ApiError>>
      GetTimePhasedAvailability:
          string -> string -> DateTimeOffset -> int -> int -> Async<Result<(DateTimeOffset * decimal) list, ApiError>>
      GetDateWiseAvailability:
          string -> string -> DateTimeOffset -> int -> Async<Result<(DateTimeOffset * decimal) list, ApiError>>
      GetSupplierOptions:
          string -> string option -> decimal -> DateTimeOffset -> Async<Result<SupplierOffer list, ApiError>> }

type MaterialReservationApi =
    { CreateTentative: MaterialReservationCreateReq -> Task<Result<MaterialReservation, ApiError>>
      Confirm: MaterialReservationConfirmReq -> Task<Result<MaterialReservation, ApiError>>
      Release: MaterialReservationReleaseReq -> Task<Result<MaterialReservation, ApiError>>
      Reduce: MaterialReservationReduceReq -> Task<Result<MaterialReservation, ApiError>>
      Expire: MaterialReservationExpireReq -> Task<Result<MaterialReservation, ApiError>> }

type MaterialReservationQueryService = QueryService<MaterialReservation, string>
type MaterialSnapshotQueryService = QueryService<MaterialSnapshot, string>

type SupplyProposal =
    { Id: string
      ProposalType: string // "PlannedPurchaseOrder" | "PlannedWorkOrder" | "PlannedTransferOrder"
      SkuId: string
      NodeId: string
      StockingPointId: string
      Quantity: decimal
      DueDate: DateTimeOffset
      StartDate: DateTimeOffset option
      RoutingId: string option
      SupplierId: string option
      Priority: int
      IsExpedite: bool
      Status: string // "Planned" | "Firmed" | "Released" | "Cancelled"
      PeggingRefs: string list
      CapacityCheckedDate: DateTimeOffset option
      CreatedAt: DateTimeOffset }

type SupplyProposalQueryService = QueryService<SupplyProposal, string>
