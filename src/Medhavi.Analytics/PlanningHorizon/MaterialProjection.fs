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

/// Builds aggregated MaterialPeriodView from supply elements and inventory snapshot.
module MaterialProjection =
    /// Build a material period view by netting supply elements against demand.
    let buildPeriodView
        (period: PlanningPeriod)
        (plantId: string)
        (snapshot: InventorySnapshot)
        (supply: SupplyElementView list)
        (demand: DemandPeriodView)
        : MaterialPeriodView =

        let inP (date: DateOnly) = PlanningPeriod.contains date period

        let sumByType t =
            supply
            |> List.filter (fun s -> s.SupplyType = t && inP s.PlannedDate)
            |> List.sumBy (fun s -> s.PlannedQty)

        let prodPlanned = sumByType PlannedProductionOrder
        let prodFirm = sumByType FirmProductionOrder
        let purPlanned = sumByType PlannedPurchaseOrder
        let purFirm = sumByType PurchaseOrder
        let inbound = sumByType InterplantTransferInbound
        let outbound = sumByType InterplantTransferOutbound

        let totalReceipts =
            prodPlanned
            + prodFirm
            + purPlanned
            + purFirm
            + inbound

        let totalIssues = demand.ConfirmedQty + outbound
        let projected = snapshot.OnHandQty + totalReceipts - totalIssues

        { Period = period
          SkuId = snapshot.SkuId
          SkuCode = ""
          SkuName = ""
          PlantId = plantId
          StockingPointId = snapshot.StockingPointId
          OpeningStock = snapshot.OnHandQty
          ClosingStock = projected
          ProjectedStock = projected
          PlannedProduction = prodPlanned
          FirmProduction = prodFirm
          PlannedPurchases = purPlanned
          FirmPurchases = purFirm
          InboundTransfers = inbound
          TotalReceipts = totalReceipts
          DemandConsumption = demand.ConfirmedQty
          ForecastConsumption = demand.ForecastDemandQty
          OutboundTransfers = outbound
          TotalIssues = totalIssues
          SafetyStockQty = snapshot.SafetyStockQty
          IsBelowSafetyStock = projected < snapshot.SafetyStockQty
          SafetyStockGap = max 0m (snapshot.SafetyStockQty - projected)
          SupplyElements = supply |> List.filter (fun s -> inP s.PlannedDate)
          InventorySnapshots = [ snapshot ]
          MaxProducibleQty = 0m // filled by CapacityProjection cross-reference
        }
