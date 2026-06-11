namespace Medhavi.Analytics.PlanningHorizon

open System

// =============================================================================
// Plane 3 — Capacity / Gantt Projection
// =============================================================================

/// Operation lifecycle status
type OperationStatus =
    | Planned
    | Released
    | InProgress
    | Completed
    | Cancelled

/// A single scheduled operation on a resource
type OperationView =
    { OperationId: string
      WorkOrderId: string option
      SkuId: string
      SkuCode: string
      RoutingStepId: string
      OperationCode: string
      Quantity: decimal
      SetupMinutes: decimal
      RunMinutes: decimal
      StartTime: DateTimeOffset
      EndTime: DateTimeOffset
      Status: OperationStatus
      DemandOrderId: string option // if demand-driven (pegged)
      PeggedDemandQty: decimal option
      IsFirm: bool
      IsFrozen: bool
      IsExpedited: bool }

/// Product changeover on a resource (setup transition)
type ChangeoverView =
    { FromSkuId: string
      ToSkuId: string
      SetupMinutes: decimal
      StartTime: DateTimeOffset }

/// Maintenance / downtime classification
type MaintenanceType =
    | PlannedMaintenance
    | UnplannedDowntime
    | CleaningTime
    | CalibrationTime

/// Maintenance window blocking resource capacity
type MaintenanceView =
    { Type: MaintenanceType
      StartTime: DateTimeOffset
      EndTime: DateTimeOffset
      Description: string option }

type CapacityBucketView =
    { AvailableHours: decimal
      CalendarHours: decimal
      MaintenanceHours: decimal }

/// One resource group x one planning period.
type CapacityPeriodView =
    { Period: PlanningPeriod
      ResourceGroupId: string
      ResourceGroupName: string
      PlantId: string
      // Capacity envelope
      AvailableHours: decimal // after maintenance deduction
      CalendarHours: decimal // total calendar hours before deductions
      MaintenanceHours: decimal
      // Load
      SetupHours: decimal
      ProductiveHours: decimal
      UsedHours: decimal // = Setup + Productive
      OverloadHours: decimal // max(0, UsedHours - AvailableHours)
      UtilizationPct: decimal // UsedHours / AvailableHours * 100
      // Remaining capacity per SKU
      MaxProducibleByProduct: Map<string, decimal> // skuId -> max units
      // Detail
      Operations: OperationView list
      Changeovers: ChangeoverView list
      Maintenance: MaintenanceView list
      // Bottleneck detection
      IsBottleneck: bool
      BottleneckReason: string option }

/// A resource group across all periods
type GanttResourceRow =
    { ResourceGroupId: string
      ResourceGroupName: string
      Cells: CapacityPeriodView list }

type GanttGrid =
    { PlantId: string
      Periods: PlanningPeriod list
      Resources: GanttResourceRow list
      GeneratedAt: DateTimeOffset
      PlanContext: PlanContext }

module CapacityProjection =

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
