namespace Medhavi.Analytics.KPI

open System
open System.Threading.Tasks
open Medhavi.SharedKernel
open Medhavi.MasterData.KpiConfiguration
open Medhavi.Analytics.PlanningHorizon

type KpiQueryRequest =
    { PlantId: string
      Periods: PlanningPeriod list
      Context: PlanContext
      SkuFilter: string list option }

type KpiQueryService =
    { GetKpiPeriodViews: KpiQueryRequest -> Task<KpiPeriodView list>
      GetKpiSnapshots: KpiQueryRequest -> Task<KpiSnapshot list> }

module KpiQueryService =

    let evaluateStatus value targetOpt thresholdOpt higherIsBetter =
        match targetOpt with
        | None -> KpiStatus.NoTarget
        | Some target ->
            if higherIsBetter then
                match thresholdOpt with
                | Some thresh when value < thresh -> KpiStatus.Critical
                | _ ->
                    if value >= target then
                        KpiStatus.Good
                    else
                        KpiStatus.Warning
            else
                match thresholdOpt with
                | Some thresh when value > thresh -> KpiStatus.Critical
                | _ ->
                    if value <= target then
                        KpiStatus.Good
                    else
                        KpiStatus.Warning

    let createKpiQueryService
        (sources: ProjectionDataSources)
        (configReader: unit -> Task<KpiConfig list>)
        : KpiQueryService =

        let calculateSnapshots
            (period: PlanningPeriod)
            (request: KpiQueryRequest)
            (configs: KpiConfig list)
            (demands: DemandLineView list)
            (supplies: SupplyElementView list)
            (inventories: Map<string, InventorySnapshot>)
            (operations: OperationView list)
            : KpiSnapshot list =

            let demandPeriodView =
                let skuId = request.SkuFilter |> Option.bind List.tryHead
                DemandProjection.buildPeriodView period request.PlantId skuId demands

            let capacityPeriodView =
                let setupH =
                    operations
                    |> List.sumBy (fun o -> o.SetupMinutes / 60m)

                let prodH =
                    operations
                    |> List.sumBy (fun o -> o.RunMinutes / 60m)

                let usedH = setupH + prodH

                { Period = period
                  ResourceGroupId = "ALL"
                  ResourceGroupName = "All Resources"
                  PlantId = request.PlantId
                  AvailableHours = 100m
                  CalendarHours = 120m
                  MaintenanceHours = 0m
                  SetupHours = setupH
                  ProductiveHours = prodH
                  UsedHours = usedH
                  OverloadHours = 0m
                  UtilizationPct = if usedH = 0m then 0m else usedH / 100m * 100m
                  MaxProducibleByProduct = Map.empty
                  Operations = operations
                  Changeovers = []
                  Maintenance = []
                  IsBottleneck = false
                  BottleneckReason = None }

            let materialPeriodView =
                let inv =
                    match request.SkuFilter with
                    | Some skus ->
                        let skuId = skus |> List.tryHead |> Option.defaultValue ""
                        inventories
                        |> Map.tryFind skuId
                        |> Option.defaultValue
                            { SkuId = skuId
                              StockingPointId = ""
                              OnHandQty = 0m
                              AvailableToPromise = 0m
                              QualityHoldQty = 0m
                              DamagedQty = 0m
                              InTransitInboundQty = 0m
                              InTransitOutboundQty = 0m
                              SafetyStockQty = 0m
                              MaxStockQty = None
                              DaysOfSupply = 0m
                              SnapshotDate = DateOnly.FromDateTime(DateTime.UtcNow) }
                    | None ->
                        let list = inventories |> Map.values |> Seq.toList
                        { SkuId = "ALL"
                          StockingPointId = ""
                          OnHandQty = list |> List.sumBy (fun i -> i.OnHandQty)
                          AvailableToPromise = list |> List.sumBy (fun i -> i.AvailableToPromise)
                          QualityHoldQty = list |> List.sumBy (fun i -> i.QualityHoldQty)
                          DamagedQty = list |> List.sumBy (fun i -> i.DamagedQty)
                          InTransitInboundQty = list |> List.sumBy (fun i -> i.InTransitInboundQty)
                          InTransitOutboundQty = list |> List.sumBy (fun i -> i.InTransitOutboundQty)
                          SafetyStockQty = list |> List.sumBy (fun i -> i.SafetyStockQty)
                          MaxStockQty =
                            let maxes = list |> List.choose (fun i -> i.MaxStockQty)
                            if maxes.IsEmpty then None else Some(List.sum maxes)
                          DaysOfSupply = 0m
                          SnapshotDate = DateOnly.FromDateTime(DateTime.UtcNow) }

                MaterialProjection.buildPeriodView period request.PlantId inv supplies demandPeriodView

            configs
            |> List.filter (fun c -> c.IsEnabled)
            |> List.choose (fun c ->
                let valueOpt =
                    match c.KpiId with
                    | "OTD" -> Some(FormulaRegistry.OTD.Calculate demandPeriodView)
                    | "OTIF" -> Some(FormulaRegistry.OTIF.Calculate demandPeriodView)
                    | "Utilization" -> Some(FormulaRegistry.Utilization.Calculate capacityPeriodView)
                    | "DaysOfSupply" -> Some(FormulaRegistry.DaysOfSupply.Calculate materialPeriodView)
                    | "SafetyStockCoverage" -> Some(FormulaRegistry.SafetyStockCoverage.Calculate materialPeriodView)
                    | "ScheduleAdherence" -> Some(FormulaRegistry.ScheduleAdherence.Calculate capacityPeriodView)
                    | _ -> None

                valueOpt
                |> Option.map (fun v ->
                    let targetVal = c.Target |> Option.map PositiveDecimal.value

                    let threshVal =
                        c.AlertThreshold
                        |> Option.map PositiveDecimal.value

                    { KpiId = c.KpiId
                      Name = c.Name
                      Value = v
                      Unit = c.Unit
                      Target = targetVal
                      AlertThreshold = threshVal
                      HigherIsBetter = c.HigherIsBetter
                      Status = evaluateStatus v targetVal threshVal c.HigherIsBetter
                      Delta = None
                      EvaluatedAt = DateTimeOffset.UtcNow }))

        { GetKpiPeriodViews =
            fun request ->
                task {
                    let! configs = configReader ()

                    let startDate =
                        request.Periods
                        |> List.map PlanningPeriod.startDate
                        |> List.min

                    let endDate =
                        request.Periods
                        |> List.map PlanningPeriod.endDate
                        |> List.max

                    let! demands = sources.Demand.GetDemandLines request.PlantId startDate endDate request.Context
                    let! operations = sources.Capacity.GetOperations request.PlantId startDate endDate request.Context

                    let! skuList =
                        task {
                            match request.SkuFilter with
                            | Some skus -> return skus
                            | None -> return! sources.Material.GetAllSkus()
                        }

                    let! supplies =
                        task {
                            if skuList.IsEmpty then
                                return []
                            else
                                let! list =
                                    skuList
                                    |> List.map (fun sku ->
                                        sources.Material.GetSupplyElements
                                            sku
                                            request.PlantId
                                            startDate
                                            endDate
                                            request.Context)
                                    |> Task.WhenAll

                                return list |> Seq.collect id |> Seq.toList
                        }

                    let! inventories =
                        task {
                            if skuList.IsEmpty then
                                return Map.empty
                            else
                                let! list =
                                    skuList
                                    |> List.map (fun sku ->
                                        task {
                                            let! snapshot =
                                                sources.Material.GetInventorySnapshot sku "" startDate request.Context

                                            return sku, snapshot
                                        })
                                    |> Task.WhenAll

                                return Map.ofArray list
                        }

                    let views =
                        request.Periods
                        |> List.map (fun period ->
                            let periodDemands =
                                demands
                                |> List.filter (fun d -> PlanningPeriod.contains d.RequestedDeliveryDate period)

                            let periodOps =
                                operations
                                |> List.filter (fun o ->
                                    PlanningPeriod.contains (DateOnly.FromDateTime(o.StartTime.Date)) period)

                            let periodSupplies =
                                supplies
                                |> List.filter (fun s -> PlanningPeriod.contains s.PlannedDate period)

                            let snapshots =
                                calculateSnapshots
                                    period
                                    request
                                    configs
                                    periodDemands
                                    periodSupplies
                                    inventories
                                    periodOps

                            { Period = period
                              PlantId = Some request.PlantId
                              SkuId = request.SkuFilter |> Option.bind List.tryHead
                              ScenarioId =
                                match request.Context with
                                | Live -> None
                                | Scenario id -> Some id
                              Snapshots = snapshots })

                    return views
                }

          GetKpiSnapshots =
            fun request ->
                task {
                    let! configs = configReader ()

                    let startDate =
                        request.Periods
                        |> List.map PlanningPeriod.startDate
                        |> List.min

                    let endDate =
                        request.Periods
                        |> List.map PlanningPeriod.endDate
                        |> List.max

                    let! demands = sources.Demand.GetDemandLines request.PlantId startDate endDate request.Context
                    let! operations = sources.Capacity.GetOperations request.PlantId startDate endDate request.Context

                    let! skuList =
                        task {
                            match request.SkuFilter with
                            | Some skus -> return skus
                            | None -> return! sources.Material.GetAllSkus()
                        }

                    let! supplies =
                        task {
                            if skuList.IsEmpty then
                                return []
                            else
                                let! list =
                                    skuList
                                    |> List.map (fun sku ->
                                        sources.Material.GetSupplyElements
                                            sku
                                            request.PlantId
                                            startDate
                                            endDate
                                            request.Context)
                                    |> Task.WhenAll

                                return list |> Seq.collect id |> Seq.toList
                        }

                    let! inventories =
                        task {
                            if skuList.IsEmpty then
                                return Map.empty
                            else
                                let! list =
                                    skuList
                                    |> List.map (fun sku ->
                                        task {
                                            let! snapshot =
                                                sources.Material.GetInventorySnapshot sku "" startDate request.Context

                                            return sku, snapshot
                                        })
                                    |> Task.WhenAll

                                return Map.ofArray list
                        }

                    let globalPeriod = PlanningPeriod.PlanningMonth(startDate.Year, startDate.Month)

                    let snapshots =
                        calculateSnapshots globalPeriod request configs demands supplies inventories operations

                    return snapshots
                } }
