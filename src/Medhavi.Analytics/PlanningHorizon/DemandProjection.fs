namespace Medhavi.Analytics.PlanningHorizon

open System

// =============================================================================
// Plane 1 — Demand Projection
// =============================================================================

/// Risk classification for a demand line's on-time delivery status
type LatenessRisk =
    | OnTrack // ConfirmedDeliveryDate <= RequestedDeliveryDate
    | AtRisk of daysLate: int // late but within LatestDeliveryDate
    | Critical // past LatestDeliveryDate or no supply plan at all

/// Summary of a supply order pegged (linked) to a demand line
type PeggedSupplySummary =
    { SupplyOrderId: string
      SupplyType: string // "PlannedProduction", "PurchaseOrder", etc.
      Quantity: decimal
      PlannedDate: DateOnly }

/// Denormalized demand line for the planning board.
/// The RequestedDeliveryDate determines which PlanningPeriod bucket this line falls into.
type DemandLineView =
    { DemandLineId: string
      DemandOrderId: string
      SkuId: string
      SkuCode: string
      SkuName: string
      CustomerId: string
      CustomerName: string
      Priority: int
      DemandCategory: string // "CustomerOrder" | "Forecast" etc.
      IsFirm: bool
      // --- Dates ---
      EarliestDeliveryDate: DateOnly option
      RequestedDeliveryDate: DateOnly // determines the bucket
      LatestDeliveryDate: DateOnly option
      ConfirmedDeliveryDate: DateOnly option
      // --- Quantities ---
      RequestedQty: decimal
      OpenQty: decimal
      FulfilledQty: decimal
      ConfirmedQty: decimal // APS promise — what has been planned
      ShortfallQty: decimal // max(0, OpenQty - ConfirmedQty)
      LatenessRisk: LatenessRisk
      // --- Pegging ---
      PeggedSupply: PeggedSupplySummary list }

/// Aggregated demand view for a single PlanningPeriod.
type DemandPeriodView =
    { Period: PlanningPeriod
      PlantId: string
      SkuId: string option // None = all SKUs aggregated
      TotalDemandQty: decimal
      FirmDemandQty: decimal // non-cancellable firm orders
      ForecastDemandQty: decimal // statistical forecast (softer)
      ConfirmedQty: decimal // APS has committed to this qty
      OpenShortfallQty: decimal // unmet demand
      DemandLines: DemandLineView list
      // Feasibility window quantities
      EarliestPossibleQty: decimal // qty where EDD <= period end
      LatestAcceptableQty: decimal // qty where LDD >= period start
      AtRiskDemandCount: int
      CriticalDemandCount: int }

/// Builds aggregated DemandPeriodView from raw demand lines.
module DemandProjection =

    /// Build a demand period view by filtering lines into a period and aggregating.
    let buildPeriodView
        (period: PlanningPeriod)
        (plantId: string)
        (skuId: string option)
        (lines: DemandLineView list)
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
