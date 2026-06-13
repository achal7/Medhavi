module Medhavi.Analytics.PlanningHorizon.CapacityProjection

open System
open Medhavi.Contracts.Capacity
open Medhavi.Contracts.Analytics


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
