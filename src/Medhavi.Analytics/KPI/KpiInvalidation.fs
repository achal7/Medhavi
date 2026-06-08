namespace Medhavi.Analytics.KPI

open System
open Medhavi.Analytics.PlanningHorizon
open Medhavi.Contracts.Integration

module KpiInvalidation =

    let classifyEvent = function
        | MrpRunCompleted _       | OptimizerRunCompleted _     -> PlanRunDependent
        | CapacityCalendarChanged _ | InventoryAdjusted _
        | ShipmentStatusChanged _ | SafetyStockPolicyChanged _  -> OperationalState
        | WorkOrderStatusChanged _ | DemandFulfilled _          -> ExecutionRealTime

    /// Map each event to the KPI cache keys it invalidates
    let keysAffectedBy (event: KpiInvalidationEvent) : KpiCacheKey list =
        match event with
        | MrpRunCompleted(plantId, _, _, dates) ->
            [ for d in dates do
                let p = PlanningPeriod.PlanningDay d
                yield { KpiId = "OTD";  Period = p; PlantId = Some plantId; SkuId = None; ScenarioId = None }
                yield { KpiId = "OTIF"; Period = p; PlantId = Some plantId; SkuId = None; ScenarioId = None }
                yield { KpiId = "FillRate"; Period = p; PlantId = Some plantId; SkuId = None; ScenarioId = None } ]
        | CapacityCalendarChanged(_, plantId, dates) ->
            [ for d in dates do
                yield { KpiId = "Utilization"; Period = PlanningPeriod.PlanningDay d; PlantId = Some plantId; SkuId = None; ScenarioId = None } ]
        | InventoryAdjusted(_, skuId, date) ->
            [ { KpiId = "DaysOfSupply"; Period = PlanningPeriod.PlanningDay date; PlantId = None; SkuId = Some skuId; ScenarioId = None }
              { KpiId = "SafetyStockCoverage"; Period = PlanningPeriod.PlanningDay date; PlantId = None; SkuId = Some skuId; ScenarioId = None } ]
        | WorkOrderStatusChanged(_, plantId, date) ->
            [ { KpiId = "Utilization"; Period = PlanningPeriod.PlanningDay date; PlantId = Some plantId; SkuId = None; ScenarioId = None }
              { KpiId = "ScheduleAdherence"; Period = PlanningPeriod.PlanningDay date; PlantId = Some plantId; SkuId = None; ScenarioId = None } ]
        | DemandFulfilled(_, plantId, date) ->
            [ { KpiId = "OTD";  Period = PlanningPeriod.PlanningDay date; PlantId = Some plantId; SkuId = None; ScenarioId = None }
              { KpiId = "OTIF"; Period = PlanningPeriod.PlanningDay date; PlantId = Some plantId; SkuId = None; ScenarioId = None } ]
        | _ -> []
