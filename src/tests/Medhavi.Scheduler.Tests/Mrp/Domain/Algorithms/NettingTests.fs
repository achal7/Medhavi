namespace Medhavi.Scheduler.Tests.Mrp.Domain.Algorithms

open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.Scheduler.Mrp.Domain
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.Policies
open Medhavi.Scheduler.Mrp.Domain.Algorithms
open Medhavi.Scheduler.Mrp.Steps
open Medhavi.Scheduler.Tests.TestCommon

module NettingTests =

    [<Tests>]
    let tests =
        testList "MRP Domain - Time-Phased Netting Tests" [

            testCase "Scenario: Netting Shortage Calculation - should calculate correct shortage when stock is zero" (fun () ->
                let sku = skuFG
                let node = nodeWarehouse
                let sp = spWarehouse
                let onhand = createQty 0m
                let safetyStock = createQty 5m
                let demands = [ { defaultDemand sku with Quantity = createQty 25m } ]
                let policy = NettingPolicy.defaults

                let netRequirements, _ = Netting.netDemands sku node sp onhand [] [] safetyStock demands policy
                
                test <@ List.length netRequirements = 1 @>
                let nr = List.head netRequirements
                test <@ Quantity.value nr.NetRequirement = 30m @> // 25 demand + 5 safety - 0 onhand = 30
            )

            testCase "Scenario: Netting Shortage Calculation - should calculate zero shortage when supply is sufficient" (fun () ->
                let sku = skuFG
                let node = nodeWarehouse
                let sp = spWarehouse
                let onhand = createQty 50m
                let safetyStock = createQty 5m
                let demands = [ { defaultDemand sku with Quantity = createQty 25m } ]
                let policy = NettingPolicy.defaults

                let netRequirements, _ = Netting.netDemands sku node sp onhand [] [] safetyStock demands policy
                
                test <@ List.length netRequirements = 1 @>
                let nr = List.head netRequirements
                test <@ Quantity.value nr.NetRequirement = 0m @> // 50 onhand >= 25 demand + 5 safety -> no shortage
            )

            testCase "Scenario: Netting Shortage Calculation - should calculate correct shortage with partial on-hand stock" (fun () ->
                let sku = skuFG
                let node = nodeWarehouse
                let sp = spWarehouse
                let onhand = createQty 10m
                let safetyStock = createQty 5m
                let demands = [ { defaultDemand sku with Quantity = createQty 25m } ]
                let policy = NettingPolicy.defaults

                let netRequirements, _ = Netting.netDemands sku node sp onhand [] [] safetyStock demands policy
                
                test <@ List.length netRequirements = 1 @>
                let nr = List.head netRequirements
                test <@ Quantity.value nr.NetRequirement = 20m @> // 25 demand + 5 safety - 10 onhand = 20
            )

            testCase "Scenario: Pre-netting Step Firmed Pegs Adjustments - should reduce demand and inbound supply quantities" (fun () ->
                let sku = skuFG
                let sp = spWarehouse
                
                let demand1 = { defaultDemand sku with DemandId = "dem-1"; Quantity = createQty 20m; RequiredDate = createTimestampYmd 2026 6 1 }
                let demand2 = { defaultDemand sku with DemandId = "dem-2"; Quantity = createQty 10m; RequiredDate = createTimestampYmd 2026 6 2 }
                
                let inbound = [ (createTimestampYmd 2026 6 1, createQty 30m, true, "sup-1") ]
                let reservations = [ (createTimestampYmd 2026 6 1, createQty 5m, "res-1") ]

                // Firmed peg: demand 1 pegged to supply 1 for 15m
                let d1Ref = { defaultDemandRef sku with DemandId = "dem-1"; NeedDate = createTimestampYmd 2026 6 1; Quantity = createQty 20m }
                let s1Ref = { defaultSupplyRef sku with SupplyId = "sup-1"; DeliveryDate = createTimestampYmd 2026 6 1; Quantity = createQty 30m }
                
                let firmedPeg = 
                    { defaultPeggingLink d1Ref s1Ref with 
                        PeggedQty = createQty 15m
                        IsLocked = true }

                let adjustedDemands, adjustedInbound, adjustedReservations =
                    NettingStep.adjustForFirmedPegs sku sp [ demand1; demand2 ] inbound reservations [ firmedPeg ]

                // demand 1 was 20m, pegged 15m -> remaining adjusted demand should be 5m
                let adjD1 = adjustedDemands |> List.find (fun d -> d.DemandId = "dem-1")
                test <@ Quantity.value adjD1.Quantity = 5m @>

                // demand 2 had no peg -> remaining should be 10m
                let adjD2 = adjustedDemands |> List.find (fun d -> d.DemandId = "dem-2")
                test <@ Quantity.value adjD2.Quantity = 10m @>

                // inbound supply 1 was 30m, pegged 15m -> remaining adjusted supply should be 15m
                test <@ List.length adjustedInbound = 1 @>
                let (_, adjSQty, _, adjSId) = List.head adjustedInbound
                test <@ adjSId = "sup-1" @>
                test <@ Quantity.value adjSQty = 15m @>

                // reservation had no peg -> remaining should be 5m
                test <@ List.length adjustedReservations = 1 @>
                let (_, adjRQty, adjRId) = List.head adjustedReservations
                test <@ adjRId = "res-1" @>
                test <@ Quantity.value adjRQty = 5m @>
            )

            testCase "Scenario: Netting Adjustments with Material Reservations - should protect reservations from demands" (fun () ->
                let sku = skuFG
                let node = nodeWarehouse
                let sp = spWarehouse
                
                let onhand = createQty 15m
                let safetyStock = createQty 0m
                let demands = [ { defaultDemand sku with Quantity = createQty 10m; RequiredDate = createTimestampYmd 2026 6 2 } ]
                
                // Reservation on T+1 of 8m. This reduces the available on-hand for the demand on T+2!
                let reservations = [ (createTimestampYmd 2026 6 1, createQty 8m) ]
                
                let netRequirements, _ = Netting.netDemands sku node sp onhand [] reservations safetyStock demands NettingPolicy.defaults
                
                test <@ List.length netRequirements = 1 @>
                let nr = List.head netRequirements
                // Available before demand = 15 (onhand) - 8 (reservation) = 7
                // Demand = 10 -> Shortage = 10 - 7 = 3m
                test <@ Quantity.value nr.NetRequirement = 3m @>
            )
        ]
