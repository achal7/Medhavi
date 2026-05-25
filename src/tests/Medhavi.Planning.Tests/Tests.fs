namespace Medhavi.Planning.Tests

open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Planning

module NettingTests =

    [<Tests>]
    let tests =
        testList "MRP Netting Engine Tests" [
            testCase "should calculate correct shortage when demand exceeds stock" (fun () ->
                let sku = SkuId "sku-abc"
                let shortfall = NettingEngine.runNetting sku 10.0m 25.0m 5.0m
                test <@ shortfall = 20.0m @>
            )
            testCase "should calculate zero shortage when supply is sufficient" (fun () ->
                let sku = SkuId "sku-abc"
                let shortfall = NettingEngine.runNetting sku 50.0m 25.0m 5.0m
                test <@ shortfall = 0.0m @>
            )
        ]
