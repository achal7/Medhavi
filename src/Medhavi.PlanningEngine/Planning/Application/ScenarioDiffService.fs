namespace Medhavi.Scheduler.Planning.Application

open Medhavi.Contracts.Scenario
open Medhavi.Scheduler.Planning.Domain

type OrderDiff =
    | Added of PlannedOrder
    | Removed of PlannedOrder
    | Changed of before: PlannedOrder * after: PlannedOrder
    | Unchanged of PlannedOrder

type ShortageDiff =
    | ShortageAppeared of Shortage
    | ShortageResolved of Shortage
    | ShortageChanged of before: Shortage * after: Shortage

type KpiDelta =
    { ServiceLevelDelta: float
      TotalCostDelta: decimal
      HardViolationDelta: int
      ObjectiveValueDelta: decimal }

type ScenarioDiff =
    { BaselineVersionId: PlanVersionId
      DeltaVersionId: PlanVersionId
      OrderDiffs: OrderDiff list
      ShortageDiffs: ShortageDiff list
      KpiDelta: KpiDelta
      AddedCount: int
      RemovedCount: int
      ChangedCount: int
      UnchangedCount: int }

module ScenarioDiffService =

    let private orderKey (o: PlannedOrder) = (o.SkuId, o.StockingPointId, o.Period)

    let private shortageKey (s: Shortage) = (s.SkuId, s.StockingPointId, s.Period)

    let private diffOrders (before: PlannedOrder list) (after: PlannedOrder list) : OrderDiff list =
        let beforeMap =
            before
            |> List.map (fun o -> orderKey o, o)
            |> Map.ofList

        let afterMap =
            after
            |> List.map (fun o -> orderKey o, o)
            |> Map.ofList

        let added =
            afterMap
            |> Map.toList
            |> List.choose (fun (k, o) ->
                if Map.containsKey k beforeMap then
                    None
                else
                    Some(Added o))

        let removedOrChanged =
            beforeMap
            |> Map.toList
            |> List.map (fun (k, b) ->
                match Map.tryFind k afterMap with
                | None -> Removed b
                | Some a ->
                    if b.Quantity = a.Quantity then
                        Unchanged b
                    else
                        Changed(b, a))

        added @ removedOrChanged

    let private diffShortages (before: Shortage list) (after: Shortage list) : ShortageDiff list =
        let beforeMap =
            before
            |> List.map (fun s -> shortageKey s, s)
            |> Map.ofList

        let afterMap =
            after
            |> List.map (fun s -> shortageKey s, s)
            |> Map.ofList

        let appeared =
            afterMap
            |> Map.toList
            |> List.choose (fun (k, s) ->
                if Map.containsKey k beforeMap then
                    None
                else
                    Some(ShortageAppeared s))

        let resolvedOrChanged =
            beforeMap
            |> Map.toList
            |> List.map (fun (k, sb) ->
                match Map.tryFind k afterMap with
                | None -> ShortageResolved sb
                | Some sa ->
                    if sb.Quantity = sa.Quantity then
                        ShortageResolved sb
                    else
                        ShortageChanged(sb, sa))

        appeared @ resolvedOrChanged

    let diff (baseline: PlanningResult) (delta: PlanningResult) : ScenarioDiff =
        let baselineId = PlanVersionId.create baseline.InputFingerprintHash
        let deltaId = PlanVersionId.create delta.InputFingerprintHash
        let orderDiffs = diffOrders baseline.PlannedOrders delta.PlannedOrders
        let shortageDiffs = diffShortages baseline.Shortages delta.Shortages

        let countOf pred = orderDiffs |> List.filter pred |> List.length

        let kpiDelta =
            { ServiceLevelDelta = delta.KpiSummary.ServiceLevel - baseline.KpiSummary.ServiceLevel
              TotalCostDelta = delta.KpiSummary.TotalCost - baseline.KpiSummary.TotalCost
              HardViolationDelta = delta.KpiSummary.HardConstraintViolations - baseline.KpiSummary.HardConstraintViolations
              ObjectiveValueDelta = delta.KpiSummary.ObjectiveValue - baseline.KpiSummary.ObjectiveValue }

        { BaselineVersionId = baselineId
          DeltaVersionId = deltaId
          OrderDiffs = orderDiffs
          ShortageDiffs = shortageDiffs
          KpiDelta = kpiDelta
          AddedCount =
            countOf (function
                | Added _ -> true
                | _ -> false)
          RemovedCount =
            countOf (function
                | Removed _ -> true
                | _ -> false)
          ChangedCount =
            countOf (function
                | Changed _ -> true
                | _ -> false)
          UnchangedCount =
            countOf (function
                | Unchanged _ -> true
                | _ -> false) }
