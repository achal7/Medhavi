namespace Medhavi.Scheduler.Tests.Mrp.Domain.Algorithms

open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.Policies
open Medhavi.Scheduler.Mrp.Domain.Algorithms
open Medhavi.Scheduler.Tests.TestCommon

module LotSizingTests =

    [<Tests>]
    let tests =
        testList "MRP Domain - Lot Sizing Tests" [

            testCase "Scenario: Lot-For-Lot Policy - should size exactly the net requirement quantity" (fun () ->
                let req = createQty 25m
                let sized = LotSizing.lotForLot req
                test <@ Quantity.value sized = 25m @>
            )

            testCase "Scenario: Fixed Lot Size Policy - should size to next multiple of lot size" (fun () ->
                let lotSize = createQty 100m
                
                // Case 1: Requirement smaller than lot size -> returns 100
                let sized1 = LotSizing.fixedLot lotSize (createQty 30m)
                test <@ Quantity.value sized1 = 100m @>

                // Case 2: Requirement is a clean multiple -> returns exact multiple
                let sized2 = LotSizing.fixedLot lotSize (createQty 200m)
                test <@ Quantity.value sized2 = 200m @>

                // Case 3: Requirement is slightly over multiple -> rounds up to next multiple
                let sized3 = LotSizing.fixedLot lotSize (createQty 210m)
                test <@ Quantity.value sized3 = 300m @>
            )

            testCase "Scenario: Minimum Lot Size Policy - should ensure sized quantity is at least the minimum" (fun () ->
                let minQty = createQty 50m

                // Case 1: Requirement is less than min -> returns min
                let sized1 = LotSizing.minimumLot minQty (createQty 20m)
                test <@ Quantity.value sized1 = 50m @>

                // Case 2: Requirement is greater than min -> returns requirement
                let sized2 = LotSizing.minimumLot minQty (createQty 80m)
                test <@ Quantity.value sized2 = 80m @>
            )

            testCase "Scenario: EOQ Policy - should calculate Economic Order Quantity using Wilson formula" (fun () ->
                // annualDemand = 1000, orderingCost = 20, holdingCost = 4
                // EOQ = sqrt(2 * 1000 * 20 / 4) = sqrt(10000) = 100
                let annualDemand = createQty 1000m
                let orderingCost = PositiveDecimal.createSafe 20m
                let holdingCost = PositiveDecimal.createSafe 4m
                
                let eoqQty = LotSizing.eoq annualDemand orderingCost holdingCost
                test <@ Quantity.value eoqQty = 100m @>

                // Apply EOQ logic
                let sized1 = LotSizing.applyEoq annualDemand orderingCost holdingCost (createQty 40m)
                test <@ Quantity.value sized1 = 100m @> // 40 < 100 -> returns 100

                let sized2 = LotSizing.applyEoq annualDemand orderingCost holdingCost (createQty 150m)
                test <@ Quantity.value sized2 = 150m @> // 150 > 100 -> returns 150
            )

            testCase "Scenario: Period Order Quantity Policy - should combine requirements over N periods" (fun () ->
                let demands = [ createQty 10m; createQty 20m; createQty 30m; createQty 40m ]

                // POQ = 3 -> combine first 3 periods: 10 + 20 + 30 = 60
                let poqQty = LotSizing.periodOrderQuantity 3 demands
                test <@ Quantity.value poqQty = 60m @>

                // POQ = 1 -> exactly first period: 10
                let poqQtySingle = LotSizing.periodOrderQuantity 1 demands
                test <@ Quantity.value poqQtySingle = 10m @>
            )

            testCase "Scenario: Silver-Meal Heuristic - should optimize combined periods based on holding and setup costs" (fun () ->
                let orderingCost = PositiveDecimal.createSafe 40m
                let holdingCost = PositiveDecimal.createSafe 2m
                let demands = [ createQty 10m; createQty 15m; createQty 25m; createQty 100m ]

                // Period 1 cost: (40 + 0) / 1 = 40.0
                // Period 2 cost: (40 + 15 * 2 * 1) / 2 = (40 + 30) / 2 = 35.0 (declining, continue)
                // Period 3 cost: (40 + 30 + 25 * 2 * 2) / 3 = (70 + 100) / 3 = 56.6 (increasing, stop at period 2)
                let optimalPeriods = LotSizing.silverMealPeriods orderingCost holdingCost demands
                test <@ optimalPeriods = 2 @>
            )

            testCase "Scenario: Rounding Lot Size Policy - should round to nearest lot size multiples" (fun () ->
                let lotSize = createQty 10m

                // Case 1: Round Up = true -> behaves like fixed lot
                let sizedUp = LotSizing.roundToLot lotSize true (createQty 23m)
                test <@ Quantity.value sizedUp = 30m @>

                // Case 2: Round Up = false -> rounds to nearest multiple (23 is closer to 20 than 30)
                let sizedNearestDown = LotSizing.roundToLot lotSize false (createQty 23m)
                test <@ Quantity.value sizedNearestDown = 20m @>

                // Case 3: Round Up = false, rounds up if closer (27 is closer to 30 than 20)
                let sizedNearestUp = LotSizing.roundToLot lotSize false (createQty 27m)
                test <@ Quantity.value sizedNearestUp = 30m @>
            )

            testCase "Scenario: Composite Sizing and Constraints - should respect lot sizing and min/max bounds" (fun () ->
                let lotSizing = Some (FixedLot (createQty 50m)) // size multiples of 50
                let minQty = Some (createQty 75m)              // must be at least 75
                let maxQty = Some (createQty 120m)             // must be at most 120
                
                // Requirement = 20: Sized to 50 (lot sizing) -> elevated to 75 (min constraint)
                let sized1 = LotSizing.applyWithConstraints lotSizing minQty maxQty (createQty 20m) []
                test <@ Quantity.value sized1 = 75m @>

                // Requirement = 90: Sized to 100 (lot sizing) -> within bounds [75, 120] -> returns 100
                let sized2 = LotSizing.applyWithConstraints lotSizing minQty maxQty (createQty 90m) []
                test <@ Quantity.value sized2 = 100m @>

                // Requirement = 140: Sized to 150 (lot sizing) -> capped to 120 (max constraint)
                let sized3 = LotSizing.applyWithConstraints lotSizing minQty maxQty (createQty 140m) []
                test <@ Quantity.value sized3 = 120m @>
            )
        ]
