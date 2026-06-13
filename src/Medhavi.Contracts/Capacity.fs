module Medhavi.Contracts.Capacity

open System
open Medhavi.Contracts.Analytics

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
      Scenario: string }
