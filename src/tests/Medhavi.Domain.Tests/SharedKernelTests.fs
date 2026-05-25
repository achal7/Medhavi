module Medhavi.Domain.Tests.SharedKernelTests

open System
open System.Text.Json
open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Scenario

type CounterState = { Count: int }

type CounterCommand =
    | Increment of int
    | Decrement of int

type CounterEvent =
    | Incremented of int
    | Decremented of int

let roundTrip<'T> (value: 'T) : 'T =
    let json = JsonSerializer.Serialize(value)
    JsonSerializer.Deserialize<'T>(json)

[<Tests>]
let tests =
    testList
        "SharedKernel Primitives Tests"
        [ testCase "SkuId equality and validation" (fun () ->
              let sku1 = SkuId.create "SKU-001"
              let sku2 = SkuId.create "SKU-001"
              let sku3 = SkuId.create "SKU-002"
              let skuEmpty = SkuId.create "  "

              let isEq = (sku1 = sku2)
              let isNotEq = (sku1 <> sku3)
              let isEmptyError = skuEmpty.IsError

              test <@ isEq @>
              test <@ isNotEq @>
              test <@ isEmptyError @>)

          testCase "Qty ratio and trySubtract validation" (fun () ->
              let q1 = Qty.create 10m
              let q2 = Qty.create 5m
              let qZero = Qty.create 0m

              let ratioOk = Qty.ratio q1 q2
              let ratioError = Qty.ratio q1 qZero

              let isRatioOk = (ratioOk = Ok 2m)
              let isRatioError = ratioError.IsError

              test <@ isRatioOk @>
              test <@ isRatioError @>

              let subOk = Qty.trySubtract q1 q2
              let subError = Qty.trySubtract q2 q1

              let isSubOk = (subOk = Ok(Qty.create 5m))
              let isSubError = subError.IsError

              test <@ isSubOk @>
              test <@ isSubError @>)

          testCase "NonNegativeQty and PositiveQty type safety" (fun () ->
              let nnOk = NonNegativeQty.create 0m
              let nnError = NonNegativeQty.create -1m

              let isNnOk = nnOk.IsOk
              let isNnError = nnError.IsError

              test <@ isNnOk @>
              test <@ isNnError @>

              let pOk = PositiveQty.create 1m
              let pError = PositiveQty.create 0m

              let isPOk = pOk.IsOk
              let isPError = pError.IsError

              test <@ isPOk @>
              test <@ isPError @>)

          testCase "Window validation and invariants" (fun () ->
              let t1 = Timestamp(DateTimeOffset.UtcNow)
              let t2 = Timestamp(DateTimeOffset.UtcNow.AddHours(1.0))

              let winOk = Window.create t1 t2
              let winError = Window.create t2 t1

              let isWinOk = winOk.IsOk
              let isWinError = winError.IsError

              test <@ isWinOk @>
              test <@ isWinError @>

              match winOk with
              | Ok w ->
                  let isStartEqual = (w.Start = t1)
                  let isEndEqual = (w.End = t2)
                  test <@ isStartEqual @>
                  test <@ isEndEqual @>
              | Error e -> failwith e.Message)

          testCase "JSON serialization round-trip" (fun () ->
              let sku =
                  SkuId.create "SKU-999"
                  |> function
                      | Ok x -> x
                      | Error _ -> failwith "invalid"

              let qty = Qty.create 123.45m
              let money = { Amount = 100.50m; Currency = "USD" }

              let roundTripSku = roundTrip sku
              let roundTripQty = roundTrip qty
              let roundTripMoney = roundTrip money

              let isSkuRtOk = (SkuId.value roundTripSku = "SKU-999")
              let isQtyRtOk = (Qty.value roundTripQty = 123.45m)
              let isMoneyRtOk = (roundTripMoney = money)

              test <@ isSkuRtOk @>
              test <@ isQtyRtOk @>
              test <@ isMoneyRtOk @>)

          testCase "Aggregate API and pattern match compile verification" (fun () ->
              let decide (cmd: CounterCommand) (stateOpt: CounterState option) : Result<CounterState * CounterEvent list, DomainError list> =
                  let state = stateOpt |> Option.defaultValue { Count = 0 }
                  match cmd with
                  | Increment value -> Ok({ Count = state.Count + value }, [ Incremented value ])
                  | Decrement value -> Ok({ Count = state.Count - value }, [ Decremented value ])

              let evolve (ev: CounterEvent) (stateOpt: CounterState option) : CounterState option =
                  let state = stateOpt |> Option.defaultValue { Count = 0 }
                  match ev with
                  | Incremented value -> Some { Count = state.Count + value }
                  | Decremented value -> Some { Count = state.Count - value }

              let result = Aggregate.handleCommand decide evolve (Increment 5) [ Incremented 10 ]

              match result with
              | Ok(finalState, events) ->
                  let isCountFifteen = (finalState.Count = 15)
                  let isEventsOk = (events = [ Incremented 5 ])
                  test <@ isCountFifteen @>
                  test <@ isEventsOk @>
              | Error _ -> failwith "Decision failed") ]
