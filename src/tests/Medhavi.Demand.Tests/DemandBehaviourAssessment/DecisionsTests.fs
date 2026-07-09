module Medhavi.Demand.Tests.DemandBehaviourAssessment.DecisionsTests

open System
open Expecto
open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.Demand.DemandBehaviourAssessment.Model
open Medhavi.Demand.DemandBehaviourAssessment.Decisions
open Medhavi.Demand.Tests.Builders

let defaultSignal: DemandSignal =
    { SignalId = "sig-1"
      Source = "POS"
      SourceReliability = 90m
      Timestamp = DateTimeOffset.UtcNow
      Value = 130m
      StatisticalBound = 10m
      RecentBaseline = 100m }

let defaultEvaluateCmd: EvaluateSignalCmd =
    { Signal = defaultSignal
      SkuId = skuId "SKU-001"
      StockingPointId = stockingPointId "SP-001"
      IsHighPriority = false }

let defaultState: DemandBehaviourAssessment =
    { SkuId = skuId "SKU-001"
      StockingPointId = stockingPointId "SP-001"
      CurrentState = Normal
      LastUpdated = Timestamp.now
      CurrentDeviation = None
      Confidence = PositiveDecimal.Zero
      CorroboratingSignalCount = 0
      BaselineReference = "Baseline-1"
      ActiveSources = []
      LastSignalTime = None
      LastStateChange = None
      BusinessTime = Timestamp.now
      TransactionTime = Timestamp.now }

[<Tests>]
let tests =
    testList
        "DemandBehaviourAssessment Decisions"
        [ testList
              "DE-D-030 evaluate signal"
              [ testCase "noise signal returns empty events"
                <| fun _ ->
                    let cmd =
                        { defaultEvaluateCmd with
                            Signal = { defaultSignal with Value = 105m } } // dev = 0.5, noise

                    let result = decide (EvaluateSignal cmd) None
                    Expect.isOk result "Evaluate Signal must be successfully processed"
                    let decision = result |> okOrFail
                    Expect.isEmpty decision.Events "No events for noise"

                testCase "significant deviation changes state to Elevated"
                <| fun _ ->
                    let cmd =
                        { defaultEvaluateCmd with
                            Signal = defaultSignal } // dev = 3.0, >2.5

                    let result = decide (EvaluateSignal cmd) None
                    Expect.isOk result "Evaluate signal must be successfull processed"
                    let decision = result |> okOrFail
                    Expect.hasLength decision.Events 1 "Evaluate signal returns events"

                    match decision.Events.Head with
                    | BehaviourStateChanged(ass, _) ->
                        Expect.equal ass.CurrentState Elevated "Evaluate signal has current state: Evaluated"
                    | _ -> failwith "Unexpected event"

                testCase "critical deviation with corroboration changes state to Critical"
                <| fun _ ->
                    // First, feed two signals to build corroboration, then test with third
                    // We'll simulate by providing state with ActiveSources already built
                    let stateWithSources =
                        { defaultState with
                            ActiveSources = [ "POS"; "WEB" ]
                            CorroboratingSignalCount = 2
                            LastSignalTime = Some(Timestamp.now) }

                    let criticalCmd =
                        { defaultEvaluateCmd with
                            Signal = { defaultSignal with Value = 150m } } // dev = 5.0, >4.0

                    let result = decide (EvaluateSignal criticalCmd) (Some stateWithSources)
                    Expect.isOk result "Evaluate signal must be successfull processed"
                    let decision = result |> okOrFail
                    Expect.hasLength decision.Events 1 "Evaluate signal must return event"

                    match decision.Events.Head with
                    | BehaviourStateChanged(ass, _) ->
                        Expect.equal ass.CurrentState Critical "Current state must be critical"
                    | _ -> failwith "Unexpected event" ]

          testList
              "DE-D-031 triggerForecastRefresh"
              [ testCase "returns false for non‑Critical state"
                <| fun _ ->
                    let state =
                        { defaultState with
                            CurrentState = Elevated }

                    let result = triggerForecastRefresh state 48.0 0.10m
                    Expect.isOk result "Trigger forecast refresh should be porceessed successfully"
                    Expect.isFalse (result |> okOrFail) "Should be false for non-Critical"

                testCase "returns false when forecast is recent"
                <| fun _ ->
                    let state =
                        { defaultState with
                            CurrentState = Critical }

                    let result = triggerForecastRefresh state 12.0 0.10m
                    Expect.isFalse (result |> okOrFail) "Should be false when forecast age < 24h"

                testCase "returns false when expected improvement is low"
                <| fun _ ->
                    let state =
                        { defaultState with
                            CurrentState = Critical }

                    let result = triggerForecastRefresh state 48.0 0.02m
                    Expect.isFalse (result |> okOrFail) "Should be false when improvement < 0.05"

                testCase "returns true when conditions are met"
                <| fun _ ->
                    let state =
                        { defaultState with
                            CurrentState = Critical }

                    let result = triggerForecastRefresh state 48.0 0.10m
                    Expect.isTrue (result |> okOrFail) "Should trigger refresh" ] ]
