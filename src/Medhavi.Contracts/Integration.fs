namespace Medhavi.Contracts.Integration

open System

type KpiInvalidationEvent =
    | MrpRunCompleted of plantId: string * stockingPointId: string * runId: string * periodsCovered: DateOnly list
    | OptimizerRunCompleted of plantId: string * scenarioId: string option * periodsCovered: DateOnly list
    | CapacityCalendarChanged of resourceGroupId: string * plantId: string * affectedDates: DateOnly list
    | InventoryAdjusted of stockingPointId: string * skuId: string * asOfDate: DateOnly
    | ShipmentStatusChanged of legId: string * shipmentId: string * date: DateOnly
    | SafetyStockPolicyChanged of skuId: string * stockingPointId: string
    | WorkOrderStatusChanged of workOrderId: string * plantId: string * date: DateOnly
    | DemandFulfilled of demandLineId: string * plantId: string * date: DateOnly
