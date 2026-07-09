module Medhavi.Demand.Tests.DemandBehaviourAssessment.RulesTests

open Expecto
open Medhavi.Demand.DemandBehaviourAssessment.Rules

[<Tests>]
let tests =
    testList
        "DemandBehaviourAssessment Rules"
        [

          testList
              "calculateDeviation"
              [ testCase "positive deviation"
                <| fun _ ->
                    let result = calculateDeviation 120m 100m 10m
                    Expect.equal result 2m "Should be 2"

                testCase "negative deviation"
                <| fun _ ->
                    let result = calculateDeviation 80m 100m 10m
                    Expect.equal result -2m "Should be -2"

                testCase "zero bound returns zero"
                <| fun _ ->
                    let result = calculateDeviation 120m 100m 0m
                    Expect.equal result 0m "Should be 0 when bound is zero" ]

          testList
              "getSignificantThreshold"
              [ testCase "high priority uses reduced threshold"
                <| fun _ ->
                    let result = getSignificantThreshold true
                    Expect.equal result 1.5m "High priority should use 1.5"

                testCase "normal priority uses default threshold"
                <| fun _ ->
                    let result = getSignificantThreshold false
                    Expect.equal result 2.5m "Normal priority should use 2.5" ]

          testList
              "isNoise"
              [ testCase "deviation below noise threshold is noise"
                <| fun _ -> Expect.isTrue (isNoise 0.5m) "0.5 < 1.0 should be noise"

                testCase "deviation above noise threshold is not noise"
                <| fun _ -> Expect.isFalse (isNoise 1.2m) "1.2 >= 1.0 should not be noise"

                testCase "negative deviation below noise threshold is noise"
                <| fun _ -> Expect.isTrue (isNoise -0.8m) "abs(-0.8) < 1.0 should be noise" ] ]
