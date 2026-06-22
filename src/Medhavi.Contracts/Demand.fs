module Medhavi.Contracts.Demand

open System
open System.Threading.Tasks
open System.Text.Json.Serialization

[<JsonFSharpConverter>]
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

[<JsonFSharpConverter>]
type DemandLineStatus =
    | Cancelled
    | Fulfilled
    | Active

[<JsonFSharpConverter>]
type DemandCategory =
    | CustomerOrderDemand // hard demand from a confirmed sales order
    | SalesOrderForecast // statistical forecast before order confirmation
    | InterplantTransfer // demand from another plant/node in the network
    | ServicePart // spare parts / aftermarket demand
    | InternalConsumption // production self-consumption (e.g., components)
    | DependentDemand // exp

type DemandLine =
    { DemandLineId: string
      DemandOrderId: string
      SkuId: string
      SkuCode: string
      SkuName: string
      CustomerId: string
      CustomerName: string
      StockingPointId: string
      Priority: int
      DemandCategory: DemandCategory
      IsFrozen: bool
      FrozenUntilUtc: DateTimeOffset option
      IsFirm: bool
      IsOnHold: bool
      OnHoldReason: string option
      CancelReason: string option
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
      Status: DemandLineStatus
      UnitOfMeasure: string
      // --- Pegging ---
      PeggedSupply: PeggedSupplySummary list }

let private formatDate (dt: DateTimeOffset) = dt.ToString("yyyy-MM-dd")

module DeamandLine =
    let statusDescription line =
        match line.Status with
        | DemandLineStatus.Cancelled ->
                let reason = line.CancelReason |> Option.defaultValue ""
                $"Cancelled: {reason}"
        | DemandLineStatus.Fulfilled -> $"Fulfilled (Qty: {line.FulfilledQty})"
        | DemandLineStatus.Active ->
            if line.IsOnHold then
                let reason = line.OnHoldReason |> Option.defaultValue ""
                $"OnHold: {reason}"
            elif line.IsFrozen then
                match line.FrozenUntilUtc with
                | Some dt -> $"Frozen until {formatDate dt}"
                | None -> "Frozen"
            elif line.ConfirmedDeliveryDate.IsSome then
                $"Promised on {line.ConfirmedDeliveryDate.Value} (Qty: {line.ConfirmedQty})"
            else
                "Ingested"

/// Aggregated demand view for a single PlanningPeriod.
type DemandPeriodView =
    { Period: Analytics.PlanningPeriod
      PlantId: string
      SkuId: string option // None = all SKUs aggregated
      TotalDemandQty: decimal
      FirmDemandQty: decimal // non-cancellable firm orders
      ForecastDemandQty: decimal // statistical forecast (softer)
      ConfirmedQty: decimal // APS has committed to this qty
      OpenShortfallQty: decimal // unmet demand
      DemandLines: DemandLine list
      // Feasibility window quantities
      EarliestPossibleQty: decimal // qty where EDD <= period end
      LatestAcceptableQty: decimal // qty where LDD >= period start
      AtRiskDemandCount: int
      CriticalDemandCount: int }

type DemandDefineReq =
    { DemandLineId: string
      DemandOrderId: string
      SkuId: string
      StockingPointId: string
      CustomerId: string
      Quantity: decimal
      UnitOfMeasure: string
      OrderDate: DateTimeOffset
      EarliestDeliveryDate: DateTimeOffset option
      RequestedDeliveryDate: DateTimeOffset
      LatestDeliveryDate: DateTimeOffset option
      ConfirmedDeliveryDate: DateTimeOffset option
      ActualDeliveryDate: DateTimeOffset option
      Priority: int
      DemandCategory: string
      IsFirm: bool
      IsFrozen: bool }

type FulfillDemandLineReq =
    { DemandLineId: string
      Quantity: decimal }

type PromiseDemandReq =
    { DemandLineId: string
      PromisedDate: DateTimeOffset
      ConfirmedQty: decimal
      PeggedSupply: PeggedSupplySummary list }

type FreezeDemandReq =
    { DemandLineId: string
      FrozenUntilUtc: DateTimeOffset }

type ReleaseDemandReq =
    { DemandLineId: string
      ReleaseFromHold: bool
      Unfreeze: bool }

type CancelDemandReq =
    { DemandLineId: string
      Reason: string
      CancelledAtUtc: DateTimeOffset
      ForceOverride: bool }

type DemandCreatedNotification = { DemandLineId: string }

type DemandUpdatedNotification = { DemandLineId: string }

type DemandDeletedNotification = { DemandLineId: string }

type DemandLineQueries = QueryService<DemandLine, string>

type DemandLineApi =
    { Define: DemandDefineReq -> Task<Result<unit, ApiError>>
      DefineBulk: DemandDefineReq list -> Task<Result<unit, ApiError>>
      Fulfill: FulfillDemandLineReq -> Task<Result<unit, ApiError>>
      Promise: PromiseDemandReq -> Task<Result<unit, ApiError>>
      Freeze: FreezeDemandReq -> Task<Result<unit, ApiError>>
      Release: ReleaseDemandReq -> Task<Result<unit, ApiError>>
      Cancel: CancelDemandReq -> Task<Result<unit, ApiError>> }
