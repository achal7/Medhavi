namespace Medhavi.Analytics

open System
open Medhavi.Analytics.PlanningHorizon
open Medhavi.Contracts.Analytics
open Medhavi.Contracts.Demand
open Medhavi.Contracts.Supply

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
    open Medhavi.SharedKernel.ScenarioContracts

    let toScenarioOverlay (scenarioId: string) (overrides: ScenarioDataOverride list) : ScenarioOverlay =
        let demand =
            overrides
            |> List.choose (function
                | DemandOverride(demandId, qty, _) ->
                    Some { DemandLineId = demandId; NewQuantity = Some qty; NewRequestedDate = None }
                | _ -> None)

        let capacity =
            overrides
            |> List.choose (function
                | CapacityOverride(resourceId, date, qty) ->
                    let period = PlanningPeriod.PlanningDay(DateOnly.FromDateTime(date.DateTime))
                    Some { ResourceGroupId = resourceId; Period = period; AvailableHoursOverride = qty }
                | _ -> None)

        let inventory =
            overrides
            |> List.choose (function
                | InventoryOverride(skuId, stockingPointId, qty) ->
                    let today = DateOnly.FromDateTime(DateTime.UtcNow)
                    Some { SkuId = skuId; StockingPointId = stockingPointId; AsOf = today; OnHandOverride = qty }
                | _ -> None)

        let policies =
            overrides
            |> List.choose (function
                | KpiWeightOverride(kpiId, weight) ->
                    Some { KpiId = kpiId; WeightOverride = weight }
                | _ -> None)

        { ScenarioId = scenarioId
          DemandOverrides = demand
          CapacityOverrides = capacity
          InventoryOverrides = inventory
          PolicyOverrides = policies }

    let applyDemandOverlay (overlay: ScenarioOverlay) (line: DemandLine) : DemandLine =
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
