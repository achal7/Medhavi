namespace Medhavi.Scheduler.Tests.Mrp.Domain.Algorithms

open System
open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain.Algorithms
open Medhavi.Scheduler.Mrp.Domain.Algorithms.SafetyStock
open Medhavi.Scheduler.Tests.TestCommon

module SafetyStockTests =

    [<Tests>]
    let tests =
        testList "MRP Domain - Safety Stock Calculation Tests" [

            testCase "Scenario: Z-Score Calculations - should return correct and interpolated standard normal z-scores" (fun () ->
                // Case 1: Standard mappings
                test <@ SafetyStock.getZScore 0.50 = 0.0 @>
                test <@ SafetyStock.getZScore 0.90 = 1.28 @>
                test <@ SafetyStock.getZScore 0.95 = 1.65 @>
                test <@ SafetyStock.getZScore 0.99 = 2.33 @>

                // Case 2: Out of bounds
                test <@ SafetyStock.getZScore 0.10 = 0.0 @>
                test <@ SafetyStock.getZScore 0.999 = 3.09 @>

                // Case 3: Interpolated values
                // 0.92 is between 0.90 (z=1.28) and 0.95 (z=1.65)
                let z92 = SafetyStock.getZScore 0.92
                test <@ z92 > 1.28 && z92 < 1.65 @>
            )

            testCase "Scenario: Demand Statistics - should calculate standard deviation from historical demand" (fun () ->
                let history = [ 10m; 12m; 23m; 23m; 16m; 23m; 21m; 16m ]
                // Mean = (10+12+23+23+16+23+21+16)/8 = 144 / 8 = 18
                // Variance = ((10-18)^2 + ...)/8 = (64 + 36 + 25 + 25 + 4 + 25 + 9 + 4)/8 = 192 / 8 = 24
                // StdDev = sqrt(24) = 4.898979 -> round to 4.90
                let stdDev = SafetyStock.calculateDemandStdDev history
                test <@ stdDev = 4.90m @>

                // Empty / single history -> returns 0
                test <@ SafetyStock.calculateDemandStdDev [] = 0m @>
                test <@ SafetyStock.calculateDemandStdDev [ 10m ] = 0m @>
            )

            testCase "Scenario: Lead Time Average - should calculate average lead time in days" (fun () ->
                let history = [ TimeSpan.FromDays 2.0; TimeSpan.FromDays 4.0; TimeSpan.FromDays 6.0 ]
                let avg = SafetyStock.calculateAverageLeadTime history
                test <@ avg = 4.0 @>

                test <@ SafetyStock.calculateAverageLeadTime [] = 0.0 @>
            )

            testCase "Scenario: Safety Stock Formula - should calculate safety stock using standard normal formula" (fun () ->
                // safetyStock = zScore * demandStdDev * sqrt(leadTime)
                // serviceLevel = 95% -> zScore = 1.65
                // demandStdDev = 10
                // leadTimeDays = 4 -> sqrt = 2
                // safetyStock = 1.65 * 10 * 2 = 33
                let safetyStock = SafetyStock.calculateSafetyStock 0.95 10m 4.0
                test <@ Quantity.value safetyStock = 33m @>

                // Zero inputs yield zero safety stock
                test <@ (SafetyStock.calculateSafetyStock 0.95 0m 4.0).IsZero @>
                test <@ (SafetyStock.calculateSafetyStock 0.95 10m 0.0).IsZero @>
            )

            testCase "Scenario: Parameters-Based Calculation - should apply correct precedence rules" (fun () ->
                // Case 1: Static override takes precedence
                let params1 = 
                    { SafetyStockParameters.empty with 
                        StaticOverride = Some (createQty 15m)
                        ServiceLevel = Some 0.95 }
                let ss1 = SafetyStock.calculateFromParameters params1
                test <@ Quantity.value ss1 = 15m @>

                // Case 2: Dynamic calculation with stddev and lead time days
                let params2 =
                    { SafetyStockParameters.empty with
                        ServiceLevel = Some 0.95
                        DemandStdDev = Some 10m
                        LeadTimeDays = Some 4.0 }
                let ss2 = SafetyStock.calculateFromParameters params2
                test <@ Quantity.value ss2 = 33m @>

                // Case 3: Dynamic calculation with historical data
                let params3 =
                    { SafetyStockParameters.empty with
                        ServiceLevel = Some 0.95
                        DemandHistory = Some [ 10m; 12m; 23m; 23m; 16m; 23m; 21m; 16m ] // stddev = 4.90
                        LeadTimeHistory = Some [ TimeSpan.FromDays 2.0; TimeSpan.FromDays 4.0; TimeSpan.FromDays 6.0 ] // avg = 4.0 -> sqrt = 2
                        // ss = 1.65 * 4.90 * 2 = 16.17
                    }
                let ss3 = SafetyStock.calculateFromParameters params3
                test <@ Quantity.value ss3 = 16.17m @>
            )
        ]
