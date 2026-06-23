module Medhavi.Scheduler.Mrp.Domain.Algorithms.TimePhasedBuckets

open System
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.Policies

/// Time-phased netting bucket details for read models / reports
type TimePhasedBucket =
    { SkuId: SkuId
      NodeId: NodeId
      StockingPointId: StockingPointId
      PeriodStart: Timestamp
      PeriodEnd: Timestamp
      BucketType: TimeBucketGranularity
      GrossRequirements: Quantity
      ScheduledReceipts: Quantity
      ProjectedAvailable: Quantity
      NetRequirements: Quantity
      PlannedProposals: Quantity
      SafetyStock: Quantity }

/// Normalize a timestamp to the start of its planning bucket period
let getPeriodStart (granularity: TimeBucketGranularity) (timestamp: Timestamp) : Timestamp =
    let dt = Timestamp.value timestamp

    let startDt =
        match granularity with
        | Daily -> DateTimeOffset(dt.Year, dt.Month, dt.Day, 0, 0, 0, TimeSpan.Zero)
        | Weekly ->
            let daysFromMonday = (int dt.DayOfWeek + 6) % 7
            let monday = dt.AddDays(-float daysFromMonday).Date
            DateTimeOffset(monday.Year, monday.Month, monday.Day, 0, 0, 0, TimeSpan.Zero)
        | Monthly -> DateTimeOffset(dt.Year, dt.Month, 1, 0, 0, 0, TimeSpan.Zero)

    Timestamp.create startDt

/// Get the end of a planning bucket period
let getPeriodEnd (granularity: TimeBucketGranularity) (periodStart: Timestamp) : Timestamp =
    let dt = Timestamp.value periodStart

    let endDt =
        match granularity with
        | Daily -> dt.AddDays(1.0)
        | Weekly -> dt.AddDays(7.0)
        | Monthly -> dt.AddMonths(1)

    Timestamp.create endDt

/// Generate a continuous sequence of periods between start and end timestamps
let generatePeriods
    (granularity: TimeBucketGranularity)
    (startDate: Timestamp)
    (endDate: Timestamp)
    : (Timestamp * Timestamp) list =
    let startNormalized = getPeriodStart granularity startDate

    let rec loop current acc =
        if current >= endDate then
            List.rev acc
        else
            let nextEnd = getPeriodEnd granularity current
            loop nextEnd ((current, nextEnd) :: acc)

    loop startNormalized []

/// Aggregate netting results and proposals into time-phased planning buckets
let aggregateTimePhasedBuckets
    (skuId: SkuId)
    (nodeId: NodeId)
    (stockingPointId: StockingPointId)
    (granularity: TimeBucketGranularity)
    (startDate: Timestamp)
    (endDate: Timestamp)
    (initialOnHand: Quantity)
    (netRequirements: NetRequirement list)
    (proposals: Netting.NettingSupplyProposal list)
    : TimePhasedBucket list =

    let periods = generatePeriods granularity startDate endDate
    let inPeriod pStart pEnd t = t >= pStart && t < pEnd

    let rec loop
        (periodsLeft: (Timestamp * Timestamp) list)
        (runningOnHand: Quantity)
        (accBuckets: TimePhasedBucket list)
        =

        match periodsLeft with
        | [] -> List.rev accBuckets
        | (pStart, pEnd) :: rest ->
            let reqsInPeriod =
                netRequirements
                |> List.filter (fun nr -> inPeriod pStart pEnd nr.RequiredDate)

            let propsInPeriod =
                proposals
                |> List.filter (fun p -> inPeriod pStart pEnd p.DueDate)

            let gross =
                reqsInPeriod
                |> List.map (fun r -> r.GrossRequirement)
                |> Quantity.sum

            let inbound =
                reqsInPeriod
                |> List.map (fun r -> r.Inbound)
                |> Quantity.sum

            let reservations =
                reqsInPeriod
                |> List.map (fun r -> r.Reservations)
                |> Quantity.sum

            let net =
                reqsInPeriod
                |> List.map (fun r -> r.NetRequirement)
                |> Quantity.sum

            let planned =
                propsInPeriod
                |> List.map (fun p -> p.Quantity)
                |> Quantity.sum

            let safety =
                if List.isEmpty reqsInPeriod then
                    Quantity.Zero
                else
                    reqsInPeriod
                    |> List.map (fun r -> r.SafetyStock)
                    |> List.max

            let projectedVal =
                let oh = Quantity.value runningOnHand
                let ib = Quantity.value inbound
                let pl = Quantity.value planned
                let res = Quantity.value reservations
                let gr = Quantity.value gross
                oh + ib + pl - res - gr

            let projected = Quantity.clampToZero projectedVal

            let bucket =
                { SkuId = skuId
                  NodeId = nodeId
                  StockingPointId = stockingPointId
                  PeriodStart = pStart
                  PeriodEnd = pEnd
                  BucketType = granularity
                  GrossRequirements = gross
                  ScheduledReceipts = inbound
                  ProjectedAvailable = projected
                  NetRequirements = net
                  PlannedProposals = planned
                  SafetyStock = safety }

            loop rest projected (bucket :: accBuckets)

    loop periods initialOnHand []
