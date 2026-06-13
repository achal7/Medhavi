module Medhavi.Analytics.PlanningHorizon.DemandProjection

open System
open Medhavi.Contracts
open Medhavi.Contracts.Demand
open Medhavi.Analytics

/// Build a demand period view by filtering lines into a period and aggregating.
let buildPeriodView
    (period: Analytics.PlanningPeriod)
    (plantId: string)
    (skuId: string option)
    (lines: DemandLine list)
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
