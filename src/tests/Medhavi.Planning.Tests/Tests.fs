namespace Medhavi.Planning.Tests

open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Planning

module NettingTests =

    [<Tests>]
    let tests =
        testList
            "MRP Netting Engine Tests"
            [ testCase "should calculate correct shortage when demand exceeds stock" (fun () ->
                  let sku = SkuId.create "sku-abc" |> function Ok x -> x | Error e -> failwith e.Message
                  let onhand = Qty.create 10.0m
                  let demandQty = Qty.create 25.0m
                  let safetyStock = Qty.create 5.0m
                  let shortfall = NettingEngine.runNetting sku onhand demandQty safetyStock
                  test <@ Qty.value shortfall = 20.0m @>)
              testCase "should calculate zero shortage when supply is sufficient" (fun () ->
                  let sku = SkuId.create "sku-abc" |> function Ok x -> x | Error e -> failwith e.Message
                  let onhand = Qty.create 50.0m
                  let demandQty = Qty.create 25.0m
                  let safetyStock = Qty.create 5.0m
                  let shortfall = NettingEngine.runNetting sku onhand demandQty safetyStock
                  test <@ Qty.value shortfall = 0.0m @>) ]
