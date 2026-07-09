module Medhavi.Demand.Tests.ForecastPublication.EvolveTests

open System
open Expecto
open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.Demand.ForecastPublication.Model
open Medhavi.Demand.Tests.Builders

let defaultPub: ForecastPublication =
    { Id = ForecastPublicationId "FP-001"
      PlanningScopeIds = []
      ForecastHorizon = TimeSpan.FromDays 30.0
      TimeBucketConfig = "Week"
      Status = Draft
      Version = 1
      ChampionModelId = None
      OverallConfidenceIndex = None
      Forecasts = Map.empty
      Assumptions = Map.empty
      Overrides = Map.empty
      Coverage = []
      TransactionTime = Timestamp.now
      PublicationTime = None
      SupersededPublicationId = None }

[<Tests>]
let tests =
    testList
        "ForecastPublication Evolve"
        [ testCase "ForecastCycleInitiated creates new Draft"
          <| fun _ ->
              let evt = ForecastCycleInitiated(defaultPub, [], [])
              let result = evolve evt None
              Expect.equal result.Value.Status Draft "Status must be Draft"

          testCase "ForecastContextPrepared updates assumptions and coverage"
          <| fun _ ->
              let state = defaultPub

              let updated =
                  { state with
                      Assumptions =
                          Map
                              [ "A1",
                                { AssumptionId = "A1"
                                  Statement = "test"
                                  DeclaredBy = "planner"
                                  LifecycleState = Declared
                                  LinkedDriverRef = None
                                  Timestamp = Timestamp.now } ]
                      Coverage = [ (skuId "SKU1", stockingPointId "SP1") ] }

              let evt = ForecastContextPrepared updated
              let result = evolve evt (Some state)
              Expect.equal result.Value.Assumptions.Count 1 "Assumptions count must be 1"
              Expect.equal result.Value.Coverage.Length 1 "Coverage count must be 1"

          testCase "ChampionModelSelected updates model"
          <| fun _ ->
              let state = defaultPub

              let updated =
                  { state with
                      ChampionModelId = Some "Model-2" }

              let evt = ChampionModelSelected(updated, "Model-1", "Model-2", Map.empty)
              let result = evolve evt (Some state)
              Expect.equal result.Value.ChampionModelId (Some "Model-2") "Champion model used must be Model-2"

          testCase "BaselineForecastsGenerated stores forecasts"
          <| fun _ ->
              let state = defaultPub

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

              let updated =
                  { state with
                      Forecasts = Map [ "FC1", fc ] }

              let evt = BaselineForecastsGenerated(updated, [ fc ])
              let result = evolve evt (Some state)
              Expect.equal result.Value.Forecasts.Count 1 "Should store the generated forecasts"

          testCase "ForecastPublicationPublished sets Published"
          <| fun _ ->
              let state =
                  { defaultPub with
                      Status = Published }

              let updated =
                  { state with
                      Status = Published
                      PublicationTime = Some(Timestamp.now) }

              let evt = ForecastPublicationPublished(updated, None)
              let result = evolve evt (Some state)
              Expect.equal result.Value.Status Published "Should set status to Published"
              Expect.isSome result.Value.PublicationTime "Should set publication time" ]
