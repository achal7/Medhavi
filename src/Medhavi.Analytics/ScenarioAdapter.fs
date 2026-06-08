namespace Medhavi.Analytics

open System
open Medhavi.Analytics.PlanningHorizon

type DemandOverride    = { DemandLineId: string; NewQuantity: decimal option; NewRequestedDate: DateOnly option }
type CapacityOverride  = { ResourceGroupId: string; Period: PlanningPeriod; AvailableHoursOverride: decimal }
type InventoryOverride = { SkuId: string; StockingPointId: string; AsOf: DateOnly; OnHandOverride: decimal }
type PolicyOverride    = { KpiId: string; WeightOverride: decimal }

type ScenarioOverlay =
    { ScenarioId: string
      DemandOverrides   : DemandOverride list
      CapacityOverrides : CapacityOverride list
      InventoryOverrides: InventoryOverride list
      PolicyOverrides   : PolicyOverride list }

module ScenarioAdapter =

    let toScenarioOverlay (scenarioId: string) (overrides: Medhavi.Scenario.ScenarioDataOverride list) : ScenarioOverlay =
        let demand =
            overrides
            |> List.choose (function
                | Medhavi.Scenario.DemandOverride(demandId, qty, _) ->
                    Some { DemandLineId = demandId; NewQuantity = Some qty; NewRequestedDate = None }
                | _ -> None)

        let capacity =
            overrides
            |> List.choose (function
                | Medhavi.Scenario.CapacityOverride(resourceId, date, qty) ->
                    let period = PlanningPeriod.PlanningDay(DateOnly.FromDateTime(date.DateTime))
                    Some { ResourceGroupId = resourceId; Period = period; AvailableHoursOverride = qty }
                | _ -> None)

        let inventory =
            overrides
            |> List.choose (function
                | Medhavi.Scenario.InventoryOverride(skuId, stockingPointId, qty) ->
                    let today = DateOnly.FromDateTime(DateTime.UtcNow)
                    Some { SkuId = skuId; StockingPointId = stockingPointId; AsOf = today; OnHandOverride = qty }
                | _ -> None)

        { ScenarioId = scenarioId
          DemandOverrides = demand
          CapacityOverrides = capacity
          InventoryOverrides = inventory
          PolicyOverrides = [] }

    let applyDemandOverlay (overlay: ScenarioOverlay) (line: DemandLineView) : DemandLineView =
        overlay.DemandOverrides
        |> List.tryFind (fun o -> o.DemandLineId = line.DemandLineId)
        |> Option.map (fun o ->
            { line with
                RequestedQty          = o.NewQuantity      |> Option.defaultValue line.RequestedQty
                RequestedDeliveryDate = o.NewRequestedDate |> Option.defaultValue line.RequestedDeliveryDate })
        |> Option.defaultValue line

    let applyInventoryOverlay (overlay: ScenarioOverlay) (snapshot: InventorySnapshot) : InventorySnapshot =
        overlay.InventoryOverrides
        |> List.tryFind (fun o -> o.SkuId = snapshot.SkuId && o.StockingPointId = snapshot.StockingPointId)
        |> Option.map (fun o ->
            { snapshot with
                OnHandQty = o.OnHandOverride })
        |> Option.defaultValue snapshot
