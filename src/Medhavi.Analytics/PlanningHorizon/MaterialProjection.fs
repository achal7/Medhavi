namespace Medhavi.Analytics.PlanningHorizon

open System

// =============================================================================
// Plane 2 — Material (Supply / Inventory) Projection
// =============================================================================

/// Classification of supply order types for aggregation
type SupplyType =
    | PlannedProductionOrder
    | FirmProductionOrder
    | PurchaseOrder
    | PlannedPurchaseOrder
    | InterplantTransferInbound
    | InterplantTransferOutbound
    | StockTransfer

/// A single supply element (work order, PO, transfer) for drill-down
type SupplyElementView =
    {
        SupplyOrderId   : string
        SupplyType      : SupplyType
        SkuId           : string
        SkuCode         : string
        StockingPointId : string
        PlannedQty      : decimal
        ConfirmedQty    : decimal
        PlannedDate     : DateOnly
        IsFirm          : bool
        IsLocked        : bool
        IsExpedited     : bool
        RoutingId       : string option
        SupplierId      : string option
        LeadTimeDays    : decimal option
    }

/// Point-in-time inventory snapshot for a SKU at a stocking point
type InventorySnapshot =
    {
        SkuId                : string
        StockingPointId      : string
        OnHandQty            : decimal
        AvailableToPromise   : decimal     // on-hand minus active reservations
        QualityHoldQty       : decimal
        DamagedQty           : decimal
        InTransitInboundQty  : decimal
        InTransitOutboundQty : decimal
        SafetyStockQty       : decimal
        MaxStockQty          : decimal option
        DaysOfSupply         : decimal
        SnapshotDate         : DateOnly
    }

/// Aggregated material view for a single PlanningPeriod.
/// Shown as one cell in the "Material" row of the planning board.
type MaterialPeriodView =
    {
        Period             : PlanningPeriod
        SkuId              : string
        SkuCode            : string
        SkuName            : string
        PlantId            : string
        StockingPointId    : string
        // Opening / closing inventory
        OpeningStock       : decimal     // start-of-period on-hand
        ClosingStock       : decimal     // = Opening + Receipts - Issues
        ProjectedStock     : decimal     // after all planned supply actions
        // Receipts (inbound supply)
        PlannedProduction  : decimal
        FirmProduction     : decimal
        PlannedPurchases   : decimal
        FirmPurchases      : decimal
        InboundTransfers   : decimal
        TotalReceipts      : decimal
        // Issues (outbound consumption)
        DemandConsumption  : decimal
        ForecastConsumption: decimal
        OutboundTransfers  : decimal
        TotalIssues        : decimal
        // Safety stock status
        SafetyStockQty     : decimal
        IsBelowSafetyStock : bool
        SafetyStockGap     : decimal     // max(0, SafetyStockQty - ProjectedStock)
        // Detail
        SupplyElements     : SupplyElementView list
        InventorySnapshots : InventorySnapshot list
        // Capacity constraint (populated by Capacity plane)
        MaxProducibleQty   : decimal
    }
