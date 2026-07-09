module Medhavi.Demand.Tests.ForecastPublication.RulesTests

open Expecto
open Medhavi.Demand.ForecastPublication.Model
open Medhavi.Demand.ForecastPublication.Rules

[<Tests>]
let tests =
    testList
        "ForecastPublication Rules"
        [

          testList
              "BR-D-029 publishedImmutable"
              [ testCase "rejects Published status"
                <| fun _ -> Expect.isError (publishedImmutable Published) "Should reject Published"
                testCase "accepts Draft status" <| fun _ -> Expect.isOk (publishedImmutable Draft) "Should accept Draft"
                testCase "accepts Superseded status"
                <| fun _ -> Expect.isOk (publishedImmutable Superseded) "Should accept Superseded" ]

          testList
              "BR-D-036 championSelectionSignificance"
              [ testCase "rejects p-value > 0.05"
                <| fun _ ->
                    let result = championSignificance 8.0m 10.0m 0.06m
                    Expect.isError result "Should reject high p-value"
                testCase "rejects candidate WAPE not lower"
                <| fun _ ->
                    let result = championSignificance 10.0m 10.0m 0.01m
                    Expect.isError result "Should reject when WAPE not improved"
                testCase "accepts significant improvement"
                <| fun _ ->
                    let result = championSignificance 7.0m 10.0m 0.01m
                    Expect.isOk result "Should accept significant improvement" ]

          testList
              "BR-D-037 noHarm"
              [ testCase "rejects bias increase > tolerance"
                <| fun _ ->
                    let result = noHarm 0.05m 0.02m 0.1m 0.1m
                    Expect.isError result "Should reject bias increase"
                testCase "rejects stability degradation > tolerance"
                <| fun _ ->
                    let result = noHarm 0.02m 0.02m 0.2m 0.1m
                    Expect.isError result "Should reject stability degradation"
                testCase "accepts within tolerances"
                <| fun _ ->
                    let result = noHarm 0.03m 0.02m 0.12m 0.1m
                    Expect.isOk result "Should accept within tolerance" ]

          testList
              "BR-D-042 overrideJustification"
              [ testCase "rejects empty justification"
                <| fun _ -> Expect.isError (overrideJustification "") "Should reject empty"
                testCase "accepts non-empty justification"
                <| fun _ -> Expect.isOk (overrideJustification "valid reason") "Should accept" ]

          testList
              "BR-D-043 overrideDeviation"
              [ testCase "rejects deviation > limit"
                <| fun _ ->
                    let result = overrideDeviation 100m 160m 50m
                    Expect.isError result "Should reject >50% deviation"
                testCase "accepts deviation within limit"
                <| fun _ ->
                    let result = overrideDeviation 100m 140m 50m
                    Expect.isOk result "Should accept <=50% deviation" ] ]
