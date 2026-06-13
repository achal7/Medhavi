module Medhavi.Analytics.PlanningHorizon.MaterialProjection

open System
open Medhavi.Contracts.Analytics
open Medhavi.Analytics
open Medhavi.Contracts.Supply
open Medhavi.Contracts.Demand

/// Build a material period view by netting supply elements and inventory snapshot against demand.
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
