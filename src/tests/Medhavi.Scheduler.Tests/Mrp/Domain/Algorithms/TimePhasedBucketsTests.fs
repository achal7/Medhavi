namespace Medhavi.Scheduler.Tests.Mrp.Domain.Algorithms

open System
open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.Policies
open Medhavi.Scheduler.Mrp.Domain.Algorithms
open Medhavi.Scheduler.Mrp.Domain.Algorithms.TimePhasedBuckets
open Medhavi.Scheduler.Tests.TestCommon

module TimePhasedBucketsTests =

    [<Tests>]
    let tests =
        testList "MRP Domain - Time-Phased Buckets Tests" [

            testCase "Scenario: Period Normalization - should snap timestamps correctly to starts of day/week/month" (fun () ->
                // Thursday, 2026-06-11 15:30:00
                let baseDate = DateTimeOffset(2026, 6, 11, 15, 30, 0, TimeSpan.Zero)
                let ts = createTimestamp baseDate

                // Daily: snaps to 2026-06-11 00:00:00
                let dailyStart = TimePhasedBuckets.getPeriodStart Daily ts |> Timestamp.value
                let dy, dm, dd, dh = dailyStart.Year, dailyStart.Month, dailyStart.Day, dailyStart.Hour
                test <@ dy = 2026 && dm = 6 && dd = 11 && dh = 0 @>

                // Weekly: Monday, 2026-06-08 00:00:00
                let weeklyStart = TimePhasedBuckets.getPeriodStart Weekly ts |> Timestamp.value
                let wy, wm, wd, wh = weeklyStart.Year, weeklyStart.Month, weeklyStart.Day, weeklyStart.Hour
                test <@ wy = 2026 && wm = 6 && wd = 8 && wh = 0 @>

                // Monthly: 2026-06-01 00:00:00
                let monthlyStart = TimePhasedBuckets.getPeriodStart Monthly ts |> Timestamp.value
                let my, mm, md, mh = monthlyStart.Year, monthlyStart.Month, monthlyStart.Day, monthlyStart.Hour
                test <@ my = 2026 && mm = 6 && md = 1 && mh = 0 @>
            )

            testCase "Scenario: Sequence Generation - should generate a continuous non-overlapping sequence of periods" (fun () ->
                let startT = createTimestampYmd 2026 6 1
                let endT = createTimestampYmd 2026 6 5

                // Daily sequence from June 1 to June 5 (exclusive of June 5) -> June 1, 2, 3, 4 (4 periods)
                let periods = TimePhasedBuckets.generatePeriods Daily startT endT
                test <@ List.length periods = 4 @>
                
                let firstPeriodStart, firstPeriodEnd = List.head periods
                test <@ Timestamp.value firstPeriodStart = DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero) @>
                test <@ Timestamp.value firstPeriodEnd = DateTimeOffset(2026, 6, 2, 0, 0, 0, TimeSpan.Zero) @>

                let lastPeriodStart, lastPeriodEnd = List.last periods
                test <@ Timestamp.value lastPeriodStart = DateTimeOffset(2026, 6, 4, 0, 0, 0, TimeSpan.Zero) @>
                test <@ Timestamp.value lastPeriodEnd = DateTimeOffset(2026, 6, 5, 0, 0, 0, TimeSpan.Zero) @>
            )

            testCase "Scenario: Buckets Aggregation - should aggregate requirements and project available balance" (fun () ->
                let sku = skuFG
                let startT = createTimestampYmd 2026 6 1
                let endT = createTimestampYmd 2026 6 4 // 3 days: June 1, June 2, June 3
                let initialOnHand = createQty 10m

                // Requirements
                // T+0 (June 1): Gross = 8, Reservation = 2, Net = 0
                // T+1 (June 2): Gross = 15, Net = 15
                let netReq1 = 
                    { SkuId = sku; NodeId = nodeWarehouse; StockingPointId = spWarehouse
                      GrossRequirement = createQty 8m; OnHand = createQty 10m; Inbound = createQty 0m
                      Reservations = createQty 2m; SafetyStock = createQty 0m; NetRequirement = createQty 0m
                      RequiredDate = createTimestampYmd 2026 6 1; BomPath = None; PeggingRefs = [] }
                let netReq2 =
                    { SkuId = sku; NodeId = nodeWarehouse; StockingPointId = spWarehouse
                      GrossRequirement = createQty 15m; OnHand = createQty 0m; Inbound = createQty 0m
                      Reservations = createQty 0m; SafetyStock = createQty 0m; NetRequirement = createQty 15m
                      RequiredDate = createTimestampYmd 2026 6 2; BomPath = None; PeggingRefs = [] }

                // Proposals
                // T+1 (June 2): Planned = 15
                let prop : Netting.NettingSupplyProposal = 
                    { SkuId = sku; NodeId = nodeWarehouse; StockingPointId = spWarehouse
                      Quantity = createQty 15m; DueDate = createTimestampYmd 2026 6 2; PeggingRefs = [] }

                let buckets = 
                    TimePhasedBuckets.aggregateTimePhasedBuckets 
                        sku nodeWarehouse spWarehouse Daily startT endT initialOnHand [ netReq1; netReq2 ] [ prop ]

                test <@ List.length buckets = 3 @>

                // Bucket 1 (June 1): OnHand = 10 -> -Gross(8) -Reservations(2) -> ProjectedAvailable = 0
                let b1 = buckets.[0]
                test <@ Quantity.value b1.GrossRequirements = 8m @>
                test <@ Quantity.value b1.ProjectedAvailable = 0m @>
                test <@ Quantity.value b1.NetRequirements = 0m @>

                // Bucket 2 (June 2): OnHand = 0 -> Gross = 15 -> Net = 15 -> Planned Proposal = 15 -> ProjectedAvailable = 0 + 15 - 15 = 0
                let b2 = buckets.[1]
                test <@ Quantity.value b2.GrossRequirements = 15m @>
                test <@ Quantity.value b2.NetRequirements = 15m @>
                test <@ Quantity.value b2.PlannedProposals = 15m @>
                test <@ Quantity.value b2.ProjectedAvailable = 0m @>

                // Bucket 3 (June 3): Idle bucket -> ProjectedAvailable = 0
                let b3 = buckets.[2]
                test <@ Quantity.value b3.GrossRequirements = 0m @>
                test <@ Quantity.value b3.ProjectedAvailable = 0m @>
            )
        ]
