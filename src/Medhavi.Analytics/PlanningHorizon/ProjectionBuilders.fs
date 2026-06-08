namespace Medhavi.Analytics.PlanningHorizon

/// Builds aggregated DemandPeriodView from raw demand lines.
module DemandProjection =

    /// Build a demand period view by filtering lines into a period and aggregating.
    let buildPeriodView
        (period: PlanningPeriod)
        (plantId: string)
        (skuId: string option)
        (lines: DemandLineView list)
        : DemandPeriodView =

        let filtered =
            lines
            |> List.filter (fun l -> PlanningPeriod.contains l.RequestedDeliveryDate period)
            |> fun all ->
                match skuId with
                | Some s -> all |> List.filter (fun l -> l.SkuId = s)
                | None -> all

        { Period = period
          PlantId = plantId
          SkuId = skuId
          TotalDemandQty = filtered |> List.sumBy (fun l -> l.RequestedQty)
          FirmDemandQty =
            filtered
            |> List.filter (fun l -> l.IsFirm)
            |> List.sumBy (fun l -> l.RequestedQty)
          ForecastDemandQty =
            filtered
            |> List.filter (fun l -> not l.IsFirm)
            |> List.sumBy (fun l -> l.RequestedQty)
          ConfirmedQty = filtered |> List.sumBy (fun l -> l.ConfirmedQty)
          OpenShortfallQty = filtered |> List.sumBy (fun l -> l.ShortfallQty)
          DemandLines = filtered
          EarliestPossibleQty =
            filtered
            |> List.filter (fun l ->
                l.EarliestDeliveryDate
                |> Option.forall (fun d -> d <= PlanningPeriod.endDate period))
            |> List.sumBy (fun l -> l.RequestedQty)
          LatestAcceptableQty =
            filtered
            |> List.filter (fun l ->
                l.LatestDeliveryDate
                |> Option.forall (fun d -> d >= PlanningPeriod.startDate period))
            |> List.sumBy (fun l -> l.RequestedQty)
          AtRiskDemandCount =
            filtered
            |> List.filter (fun l ->
                match l.LatenessRisk with
                | AtRisk _ -> true
                | _ -> false)
            |> List.length
          CriticalDemandCount =
            filtered
            |> List.filter (fun l -> l.LatenessRisk = Critical)
            |> List.length }

/// Builds aggregated MaterialPeriodView from supply elements and inventory snapshot.
module MaterialProjection =

    open System

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

/// Builds CapacityPeriodView (Gantt cell) from operations and capacity data.
module CapacityProjection =

    /// Build a single Gantt cell for a resource group in a period.
    let buildCell
        (period: PlanningPeriod)
        (rgId: string)
        (rgName: string)
        (plantId: string)
        (availHours: decimal)
        (calHours: decimal)
        (maintHours: decimal)
        (ops: OperationView list)
        (changeovers: ChangeoverView list)
        (maintenance: MaintenanceView list)
        (bottleneckThreshold: decimal)
        : CapacityPeriodView =

        let setupH = ops |> List.sumBy (fun o -> o.SetupMinutes / 60m)
        let prodH = ops |> List.sumBy (fun o -> o.RunMinutes / 60m)
        let usedH = setupH + prodH

        let utilPct =
            if availHours = 0m then
                0m
            else
                usedH / availHours * 100m

        let maxByProd =
            ops
            |> List.groupBy (fun o -> o.SkuId)
            |> List.map (fun (sku, skuOps) ->
                let rate =
                    skuOps
                    |> List.averageBy (fun o ->
                        if o.Quantity = 0m then
                            0m
                        else
                            o.RunMinutes / o.Quantity)

                let remainMin = max 0m ((availHours - usedH) * 60m)
                sku, (if rate = 0m then 0m else remainMin / rate))
            |> Map.ofList

        { Period = period
          ResourceGroupId = rgId
          ResourceGroupName = rgName
          PlantId = plantId
          AvailableHours = availHours
          CalendarHours = calHours
          MaintenanceHours = maintHours
          SetupHours = setupH
          ProductiveHours = prodH
          UsedHours = usedH
          OverloadHours = max 0m (usedH - availHours)
          UtilizationPct = utilPct
          MaxProducibleByProduct = maxByProd
          Operations = ops
          Changeovers = changeovers
          Maintenance = maintenance
          IsBottleneck = utilPct > bottleneckThreshold
          BottleneckReason =
            if utilPct > bottleneckThreshold then
                Some(sprintf "Utilization %.1f%% > threshold %.1f%%" (float utilPct) (float bottleneckThreshold))
            else
                None }
