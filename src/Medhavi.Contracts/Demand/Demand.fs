namespace Medhavi.Contracts.Demand

open System
open System.Threading.Tasks
open System.Text.Json.Serialization
open Medhavi.Contracts

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
type DemandStatus =
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

type Demand =
    { DemandId: string
      OrderId: string
      SkuId: string
      SkuCode: string
      SkuName: string
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
      Status: DemandStatus
      UnitOfMeasure: string
      // --- Pegging ---
      PeggedSupply: PeggedSupplySummary list }

module Demand =
    let private formatDate (dt: DateTimeOffset) = dt.ToString("yyyy-MM-dd")

    let statusDescription line =
        match line.Status with
        | DemandStatus.Cancelled ->
            let reason = line.CancelReason |> Option.defaultValue ""
            $"Cancelled: {reason}"
        | DemandStatus.Fulfilled -> $"Fulfilled (Qty: {line.FulfilledQty})"
        | DemandStatus.Active ->
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

type DemandPeriodView =
    { Period: PlanningPeriod
      PlantId: string
      SkuId: string option // None = all SKUs aggregated
      TotalDemandQty: decimal
      FirmDemandQty: decimal // non-cancellable firm orders
      ForecastDemandQty: decimal // statistical forecast (softer)
      ConfirmedQty: decimal // APS has committed to this qty
      OpenShortfallQty: decimal // unmet demand
      Demands: Demand list
      // Feasibility window quantities
      EarliestPossibleQty: decimal // qty where EDD <= period end
      LatestAcceptableQty: decimal // qty where LDD >= period start
      AtRiskDemandCount: int
      CriticalDemandCount: int }

type DemandDefineReq =
    { DemandId: string
      OrderId: string
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

type FulfillDemandReq = { DemandId: string; Quantity: decimal }

type PromiseDemandReq =
    { DemandId: string
      PromisedDate: DateTimeOffset
      ConfirmedQty: decimal
      PeggedSupply: PeggedSupplySummary list }

type FreezeDemandReq =
    { DemandId: string
      FrozenUntilUtc: DateTimeOffset }

type ReleaseDemandReq =
    { DemandId: string
      ReleaseFromHold: bool
      Unfreeze: bool }

type CancelDemandReq =
    { DemandId: string
      Reason: string
      CancelledAtUtc: DateTimeOffset
      ForceOverride: bool }

type AcceptDemandSignalReq =
    { SignalId: string
      SignalSource: string
      SourceReliability: decimal
      SignalTimestamp: DateTimeOffset
      SignalValue: decimal
      StatisticalBound: decimal
      RecentBaseline: decimal
      DemandId: string
      OrderId: string
      SkuId: string
      StockingPointId: string
      CustomerId: string
      Quantity: decimal
      UnitOfMeasure: string
      OrderDate: DateTimeOffset
      EarliestDeliveryDate: DateTimeOffset option
      RequestedDeliveryDate: DateTimeOffset
      LatestDeliveryDate: DateTimeOffset option
      Priority: int
      DemandCategory: string
      IsFirm: bool
      IsFrozen: bool }

type AdjustDemandHistoryReq =
    { DemandId: string
      OriginalQuantity: decimal
      NewQuantity: decimal
      Reason: string }

type ReviseDemandReq =
    { DemandId: string
      Quantity: decimal option
      RequestedDeliveryDate: DateTimeOffset option
      EarliestDeliveryDate: DateTimeOffset option
      LatestDeliveryDate: DateTimeOffset option
      Priority: int option
      IsFirm: bool option
      IsFrozen: bool option }

type DemandCreatedNotification = { DemandId: string }

type DemandUpdatedNotification = { DemandId: string }

type DemandDeletedNotification = { DemandId: string }

type DemandQueries = QueryService<Demand, string>

type DemandApi =
    { Fulfill: FulfillDemandReq -> Task<Result<unit, ApiError>>
      Promise: PromiseDemandReq -> Task<Result<unit, ApiError>>

      // Lifecycle Operations (from other aggregate boundaries)
      Define: DemandDefineReq -> Task<Result<string, ApiError>>
      DefineBulk: DemandDefineReq list -> Task<Result<unit, ApiError>>
      Freeze: FreezeDemandReq -> Task<Result<unit, ApiError>>
      Release: ReleaseDemandReq -> Task<Result<unit, ApiError>>
      Cancel: CancelDemandReq -> Task<Result<unit, ApiError>>
      Revise: ReviseDemandReq -> Task<Result<string, ApiError>>

      // Demand Signal & History (CA‑DI‑001)
      AcceptSignal: AcceptDemandSignalReq -> Task<Result<string, ApiError>>
      AdjustHistory: AdjustDemandHistoryReq -> Task<Result<unit, ApiError>> }
