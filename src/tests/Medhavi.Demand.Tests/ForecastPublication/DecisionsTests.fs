module Medhavi.Demand.Tests.ForecastPublication.DecisionTests

open Expecto
open Medhavi.Demand.ForecastPublication.Model
open Medhavi.Demand.ForecastPublication.Decisions
open Medhavi.Demand.Tests.Builders.ForecastPub
open Medhavi.Demand.Tests.Builders
open Medhavi.SharedKernel
open Medhavi.Common.Validation
open Medhavi.Demand
open Medhavi.Demand.Tests.Builders

[<Tests>]
let tests =
    testList
        "ForecastPublication Decisions"
        [ testList
              "initiate cycle"
              [ testCase "creates new Draft publication"
                <| fun _ ->
                    let cmdRes = defaultInitiateReq |> Medhavi.Demand.ForecastPublication.ACL.toInitiateCmd

                    match cmdRes with
                    | Valid cmd ->
                        let result = initiate cmd
                        Expect.isOk result "Should initiate successfully"

                        match result with
                        | Ok [ ForecastCycleInitiated(pub, _, _) ] ->
                            Expect.equal pub.Status Draft "Status should be Draft"
                            Expect.equal pub.Version 1 "Version should be 1"
                        | _ -> failwith "Expected ForecastCycleInitiated event"
                    | Invalid issues -> failwith $"Validation issues while creating initiate command: {issues}" ]

          testList
              "BR-D-029 publishedImmutable"
              [ testCase "rejects context preparation when published"
                <| fun _ ->
                    let state = { defaultPub with Status = Published }

                    let cmd: PrepareForecastContextCmd =
                        { PublicationId = state.Id
                          Assumptions = []
                          Coverage = [] }

                    let result = prepareContext cmd state
                    Expect.isError result "Should reject modification of published publication"

                testCase "rejects override recording when published"
                <| fun _ ->
                    let state = { defaultPub with Status = Published }

                    let cmd: RecordForecastOverrideCmd =
                        { PublicationId = state.Id
                          ForecastId = forecastId "FC-001"
                          NewValue = positiveDecimal 150m
                          Justification = "urgent change"
                          PlannerIdentity = "PL-01" }

                    let result = recordOverride cmd state
                    Expect.isError result "Should reject override for published publication" ]

          testList
              "BR-D-042 overrideJustification"
              [ testCase "rejects override with empty justification"
                <| fun _ ->
                    let state =
                        { defaultPub with
                            Forecasts = Map.ofList [ ForecastId.value defaultForecast.ForecastId, defaultForecast ] }

                    let cmd: RecordForecastOverrideCmd =
                        { PublicationId = state.Id
                          ForecastId = defaultForecast.ForecastId
                          NewValue = positiveDecimal 150m
                          Justification = "   "
                          PlannerIdentity = "PL-01" }

                    let result = recordOverride cmd state
                    Expect.isError result "Should require non-empty justification" ]

          testList
              "BR-D-043 overrideDeviation"
              [ testCase "rejects override exceeding max deviation of 50%"
                <| fun _ ->
                    let state =
                        { defaultPub with
                            Forecasts = Map.ofList [ ForecastId.value defaultForecast.ForecastId, defaultForecast ] } // mean = 100

                    let cmd: RecordForecastOverrideCmd =
                        { PublicationId = state.Id
                          ForecastId = defaultForecast.ForecastId
                          NewValue = positiveDecimal 200m // 100% deviation, exceeds 50%
                          Justification = "big order"
                          PlannerIdentity = "PL-01" }

                    let result = recordOverride cmd state
                    Expect.isError result "Should reject deviation exceeding limit"

                testCase "accepts override within max deviation of 50%"
                <| fun _ ->
                    let state =
                        { defaultPub with
                            Forecasts = Map.ofList [ ForecastId.value defaultForecast.ForecastId, defaultForecast ] } // mean = 100

                    let cmd: RecordForecastOverrideCmd =
                        { PublicationId = state.Id
                          ForecastId = defaultForecast.ForecastId
                          NewValue = positiveDecimal 130m // 30% deviation, within 50%
                          Justification = "minor adjustment"
                          PlannerIdentity = "PL-01" }

                    let result = recordOverride cmd state
                    Expect.isOk result "Should accept override within limit" ]

          testList
              "BR-D-045 originalPreserved"
              [ testCase "preserves original system forecast during override"
                <| fun _ ->
                    let state =
                        { defaultPub with
                            Forecasts = Map.ofList [ ForecastId.value defaultForecast.ForecastId, defaultForecast ] }

                    let cmd: RecordForecastOverrideCmd =
                        { PublicationId = state.Id
                          ForecastId = defaultForecast.ForecastId
                          NewValue = positiveDecimal 130m
                          Justification = "minor adjustment"
                          PlannerIdentity = "PL-01" }

                    let result = recordOverride cmd state
                    Expect.isOk result "Should record override"

                    match result with
                    | Ok [ ForecastOverrideRecorded(pub, ovr) ] ->
                        Expect.equal ovr.OriginalValue 100m "Original system value should be preserved"
                        Expect.equal ovr.OverrideValue 130m "Override value should be recorded"
                        // Check that the original forecast in the forecasts map remains untouched
                        let originalForecast = pub.Forecasts.[ForecastId.value defaultForecast.ForecastId]
                        Expect.equal originalForecast.Mean 100m "Forecast mean in collection must remain unmodified"
                    | _ -> failwith "Expected ForecastOverrideRecorded event" ]

          testList
              "generate baseline"
              [ testCase "accepts valid pre-computed forecast lines"
                <| fun _ ->
                    let state = defaultPub

                    let cmd: GenerateBaselineForecastsCmd =
                        { PublicationId = state.Id
                          Forecasts = [ defaultForecast ] }

                    let result = generateBaselineForecasts cmd state
                    Expect.isOk result "Should accept valid forecast lines"

                    match result with
                    | Ok [ BaselineForecastsGenerated(pub, _) ] ->
                        Expect.hasLength (Map.toList pub.Forecasts) 1 "Should store the forecast line"
                        let fc = pub.Forecasts.[ForecastId.value defaultForecast.ForecastId]
                        Expect.equal fc.Mean 100m "Mean should match"
                    | _ -> failwith "Expected BaselineForecastsGenerated event"

                testCase "rejects negative forecast value"
                <| fun _ ->
                    let state = defaultPub
                    let invalidFc = { defaultForecast with Mean = -10m }

                    let cmd: GenerateBaselineForecastsCmd =
                        { PublicationId = state.Id
                          Forecasts = [ invalidFc ] }

                    let result = generateBaselineForecasts cmd state
                    Expect.isError result "Should reject negative mean"

                testCase "rejects prediction interval violation (bounds)"
                <| fun _ ->
                    let state = defaultPub

                    let invalidInterval =
                        { defaultForecast.PredictionInterval with
                            LowerBound = positiveDecimal 110m // lower bound (110) > mean (100)
                            UpperBound = positiveDecimal 120m }

                    let invalidFc =
                        { defaultForecast with
                            PredictionInterval = invalidInterval }

                    let cmd: GenerateBaselineForecastsCmd =
                        { PublicationId = state.Id
                          Forecasts = [ invalidFc ] }

                    let result = generateBaselineForecasts cmd state
                    Expect.isError result "Should reject invalid prediction interval bounds" ]

          testList
              "publish publication"
              [ testCase "transitions draft to published and sets PublicationTime"
                <| fun _ ->
                    let state = defaultPub
                    let cmd: PublishForecastPublicationCmd = { PublicationId = state.Id }
                    let result = publishPublication cmd state
                    Expect.isOk result "Should publish draft"

                    match result with
                    | Ok [ ForecastPublicationPublished(pub, _) ] ->
                        Expect.equal pub.Status Published "Status should be Published"
                        Expect.isSome pub.PublicationTime "PublicationTime should be set"
                    | _ -> failwith "Expected ForecastPublicationPublished event"

                testCase "rejects publication if already published"
                <| fun _ ->
                    let state = { defaultPub with Status = Published }
                    let cmd: PublishForecastPublicationCmd = { PublicationId = state.Id }
                    let result = publishPublication cmd state
                    Expect.isError result "Should reject already published" ]
          testList
              "generateBaselineForecasts"
              [ testCase "validates non-empty forecasts"
                <| fun _ ->
                    let state = defaultPub

                    let cmd =
                        { PublicationId = ForecastPublicationId "FP-001"
                          Forecasts = [] }

                    let result = generateBaselineForecasts cmd state
                    Expect.isError result "Should reject empty forecasts"
                testCase "accepts valid forecasts"
                <| fun _ ->
                    let fc =
                        { ForecastId = ForecastId.create "FC1" |> okOrFail
                          SkuId = skuId "SKU1"
                          StockingPointId = stockingPointId "SP1"
                          PlanningPeriod = Medhavi.Contracts.PlanningWeek(2027, 27)
                          Mean = 100m
                          PredictionInterval =
                            { LowerBound = PositiveDecimal.createSafe 80m
                              UpperBound = PositiveDecimal.createSafe 120m
                              ConfidenceLevel = PositiveDecimal.createSafe 0.95m }
                          Confidence = PositiveDecimal.createSafe 0.95m
                          ModelId = "M"
                          GeneratedAt = Timestamp.now
                          OverrideReason = None }

                    let cmd =
                        { PublicationId = ForecastPublicationId "FP-001"
                          Forecasts = [ fc ] }

                    let result = generateBaselineForecasts cmd defaultPub
                    Expect.isOk result "Should accept valid forecasts" ] ]
