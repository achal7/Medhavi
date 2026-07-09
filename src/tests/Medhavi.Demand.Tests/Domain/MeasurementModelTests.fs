module Medhavi.Demand.Tests.Domain.MeasurementModelTests

open System
open Expecto
open FsCheck.FSharp
open Swensen.Unquote
open Medhavi.Demand.Domain.MeasurementModel

// =============================================================================
// Helper generators for FsCheck
// =============================================================================

let genPositiveDecimal = Gen.choose(0, 10000) |> Gen.map decimal

let genForecastActual =
    Gen.map3
        (fun f a p ->
            { Period = string p
              ForecastQuantity = f
              ActualQuantity = a })
        genPositiveDecimal
        genPositiveDecimal
        (Gen.choose(1, 52))

type Arbitraries =
    static member ForecastActual() = Arb.fromGen genForecastActual

let config =
    { FsCheckConfig.defaultConfig with
        arbitrary = [ typeof<Arbitraries> ] }

// =============================================================================
// PI‑DI‑003: WAPE - worked example
// =============================================================================

let wapeWorkedExampleData: ForecastActual list =
    [ { Period = "Week1"
        ForecastQuantity = 100m
        ActualQuantity = 110m }
      { Period = "Week2"
        ForecastQuantity = 120m
        ActualQuantity = 100m }
      { Period = "Week3"
        ForecastQuantity = 130m
        ActualQuantity = 140m } ]

[<Tests>]
let wapeTests =
    testList
        "WAPE"
        [ testCase "Specification worked example returns 11.43%"
          <| fun () ->
              let result = wape wapeWorkedExampleData
              test <@ abs(result - 11.4285714286m) < 0.01m @>

          testCase "Total actual zero throws"
          <| fun () ->
              let data =
                  [ { Period = "W1"
                      ForecastQuantity = 100m
                      ActualQuantity = 0m } ]

              Expect.throws (fun () -> wape data |> ignore) "Should throw on zero total actual"

          testPropertyWithConfig config "WAPE is never negative"
          <| fun (data: ForecastActual list) ->
              let totalActual = data |> List.sumBy(fun fa -> fa.ActualQuantity)

              (totalActual > 0m)
              ==> lazy
                  let result = wape data
                  result >= 0m ]

// =============================================================================
// PI‑DI‑002: Forecast Accuracy
// =============================================================================

[<Tests>]
let forecastAccuracyTests =
    testList
        "Forecast Accuracy"
        [ testCase "100 − WAPE"
          <| fun () ->
              let wapeVal = wape wapeWorkedExampleData
              let accuracy = forecastAccuracy wapeVal
              test <@ abs(accuracy - 88.5714285714m) < 0.01m @>

          testPropertyWithConfig config "Accuracy = 100 − WAPE for all valid inputs"
          <| fun (data: ForecastActual list) ->
              let totalActual = data |> List.sumBy(fun fa -> fa.ActualQuantity)

              (totalActual > 0m)
              ==> lazy
                  let wapeVal = wape data
                  let accuracy = forecastAccuracy wapeVal
                  test <@ abs((100m - wapeVal) - accuracy) < 0.0001m @> ]

// =============================================================================
// PI‑DI‑004: MAPE
// =============================================================================

let mapeWorkedExampleData =
    [ { Period = "Week1"
        ForecastQuantity = 100m
        ActualQuantity = 110m }
      { Period = "Week2"
        ForecastQuantity = 120m
        ActualQuantity = 100m }
      { Period = "Week3"
        ForecastQuantity = 130m
        ActualQuantity = 140m } ]

[<Tests>]
let mapeTests =
    testList
        "MAPE"
        [ testCase "Specification worked example returns 12.08%"
          <| fun () ->
              let result = mape mapeWorkedExampleData
              test <@ abs(result - 12.076m) < 0.01m @>

          testCase "All actuals zero throws"
          <| fun () ->
              let data =
                  [ { Period = "W1"
                      ForecastQuantity = 100m
                      ActualQuantity = 0m } ]

              Expect.throws (fun () -> mape data |> ignore) "MAPE undefined with zero actuals"

          testPropertyWithConfig config "MAPE is never negative"
          <| fun (data: ForecastActual list) ->
              let nonZero = data |> List.filter(fun fa -> fa.ActualQuantity <> 0m)

              (nonZero.Length > 0)
              ==> lazy
                  let result = mape nonZero
                  result >= 0m ]

// =============================================================================
// PI‑DI‑005: Forecast Bias
// =============================================================================

let biasWorkedExampleData =
    [ { Period = "Week1"
        ForecastQuantity = 100m
        ActualQuantity = 110m } // −10
      { Period = "Week2"
        ForecastQuantity = 120m
        ActualQuantity = 100m } // +20
      { Period = "Week3"
        ForecastQuantity = 130m
        ActualQuantity = 140m } ] // −10

[<Tests>]
let forecastBiasTests =
    testList
        "Forecast Bias"
        [ testCase "Specification worked example - zero bias"
          <| fun () ->
              let absBias = forecastBiasAbsolute biasWorkedExampleData
              test <@ absBias = 0m @>

          testCase "Specification worked example - biased (positive) returns +23.33"
          <| fun () ->
              let biasedData =
                  [ { Period = "W1"
                      ForecastQuantity = 130m
                      ActualQuantity = 110m }
                    { Period = "W2"
                      ForecastQuantity = 140m
                      ActualQuantity = 100m }
                    { Period = "W3"
                      ForecastQuantity = 150m
                      ActualQuantity = 140m } ]

              let absBias = forecastBiasAbsolute biasedData
              test <@ abs(absBias - 23.333m) < 0.1m @>

          testCase "Empty data returns zero bias" <| fun () -> test <@ forecastBiasAbsolute [] = 0m @> ]

// =============================================================================
// PI‑DI‑006: Forecast Value Added (FVA)
// =============================================================================

[<Tests>]
let fvaTests =
    testList
        "FVA"
        [ testCase "FVA positive when process beats naive"
          <| fun () ->
              let result = fva 15.63m 8.33m
              test <@ result = 7.30m @>

          testCase "FVA negative when process worse than naive"
          <| fun () ->
              let result = fva 15.63m 18.50m
              test <@ result = -2.87m @>

          testCase "FVA zero when no difference" <| fun () -> test <@ fva 10m 10m = 0m @> ]

// =============================================================================
// PI‑DI‑007: Forecast Stability
// =============================================================================

let cycle1 =
    { CycleId = "C1"
      GeneratedAt = DateTimeOffset.UtcNow
      Forecasts = Map.ofList [ ("W5", 200m); ("W6", 180m); ("W7", 150m) ] }

let cycle2 =
    { CycleId = "C2"
      GeneratedAt = DateTimeOffset.UtcNow
      Forecasts = Map.ofList [ ("W5", 220m); ("W6", 190m); ("W7", 175m) ] }

let cycle3 =
    { CycleId = "C3"
      GeneratedAt = DateTimeOffset.UtcNow
      Forecasts = Map.ofList [ ("W5", 210m); ("W6", 185m); ("W7", 170m) ] }

[<Tests>]
let forecastStabilityTests =
    testList
        "Forecast Stability"
        [ testCase "Two cycles - worked example"
          <| fun () ->
              let result = forecastStability [ cycle1; cycle2 ]
              test <@ result > 0m @> // at least some change

          testCase "Single cycle returns zero stability"
          <| fun () -> test <@ forecastStability [ cycle1 ] = 0m @> ]

// =============================================================================
// PI‑DI‑008: Forecast Value Realization
// =============================================================================

[<Tests>]
let forecastValueRealizationTests =
    testList
        "Forecast Value Realization"
        [ testCase "Specification worked example - 91.8%"
          <| fun () ->
              let result = forecastValueRealization 89.2m 97.2m
              test <@ abs(result - 91.769m) < 0.1m @>

          testCase "Zero max potential returns zero"
          <| fun () -> test <@ forecastValueRealization 50m 0m = 0m @> ]

// =============================================================================
// PI‑DI‑009: Demand Plan Adherence
// =============================================================================

[<Tests>]
let demandPlanAdherenceTests =
    testList
        "Demand Plan Adherence"
        [ testCase "Specification worked example - 73.9%"
          <| fun () ->
              let entries = [ (100m, 98m); (120m, 130m); (110m, 108m); (130m, 135m) ]
              let result = demandPlanAdherence entries 5m
              test <@ abs(result - 73.913m) < 0.1m @>

          testCase "All within tolerance returns 100%"
          <| fun () ->
              let entries = [ (100m, 100m); (200m, 205m) ]
              test <@ demandPlanAdherence entries 5m = 100m @> ]

// =============================================================================
// PI‑DI‑010: Service Level
// =============================================================================

[<Tests>]
let serviceLevelTests =
    testList
        "Service Level"
        [ testCase "Specification worked example - 83.53%"
          <| fun () ->
              let result = serviceLevel 355m 425m
              test <@ abs(result - 83.529m) < 0.1m @> ]

// =============================================================================
// PI‑DI‑011: Order Fill Rate
// =============================================================================

[<Tests>]
let orderFillRateTests =
    testList
        "Order Fill Rate"
        [ testCase "Specification worked example - 60%" <| fun () -> test <@ orderFillRate 3 5 = 60m @> ]

// =============================================================================
// PI‑DI‑012: OTIF
// =============================================================================

[<Tests>]
let otifTests =
    testList
        "OTIF"
        [ testCase "Specification worked example - 60%" <| fun () -> test <@ otif 3 5 = 60m @> ]

// =============================================================================
// PI‑DI‑013: Perfect Order Rate
// =============================================================================

[<Tests>]
let perfectOrderRateTests =
    testList
        "Perfect Order Rate"
        [ testCase "Specification worked example - 40%"
          <| fun () -> test <@ perfectOrderRate 2 5 = 40m @> ]

// =============================================================================
// PI‑DI‑014: Customer Request Fulfilment Rate
// =============================================================================

[<Tests>]
let customerRequestFulfilmentRateTests =
    testList
        "Customer Request Fulfilment Rate"
        [ testCase "Specification worked example - 60%"
          <| fun () -> test <@ customerRequestFulfilmentRate 3 5 = 60m @> ]

// =============================================================================
// PI‑DI‑015: Demand Satisfaction Rate
// =============================================================================

[<Tests>]
let demandSatisfactionRateTests =
    testList
        "Demand Satisfaction Rate"
        [ testCase "Specification worked example - 84.71%"
          <| fun () ->
              let result = demandSatisfactionRate 360m 425m
              test <@ abs(result - 84.706m) < 0.1m @> ]
