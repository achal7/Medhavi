module Medhavi.Demand.Tests.Domain.DemandObservationRulesTests

open System
open Expecto
open Medhavi.Demand.DemandObservation.Model
open Medhavi.Demand.DemandObservation.Rules
open Medhavi.Demand.Tests
open Medhavi.Demand.Tests.Builders.DemandSignalBuilder

[<Tests>]
let tests =
    testList
        "DemandObservation Rules"
        [

          testList
              "BR-D-010 signalTimeliness"
              [ testCase "rejects signal older than max latency"
                <| fun _ ->
                    let signal = defaultSignal |> withTimestamp(DateTimeOffset.UtcNow.AddHours(-2.0))
                    let result = signalTimeliness signal (TimeSpan.FromHours 1.0)
                    Expect.isError result "Old signal should be rejected"

                testCase "accepts signal within latency"
                <| fun _ ->
                    let signal = defaultSignal |> withTimestamp(DateTimeOffset.UtcNow.AddMinutes(-30.0))
                    let result = signalTimeliness signal (TimeSpan.FromHours 1.0)
                    Expect.isOk result "Recent signal should be accepted" ]

          testList
              "BR-D-011 signalRange"
              [ testCase "rejects deviation exceeding bound"
                <| fun _ ->
                    let signal = defaultSignal |> withValue 200m // baseline=90, bound=50 → diff=110 > 50
                    let result = signalRange signal
                    Expect.isError result "Out-of-range value should be rejected"

                testCase "accepts deviation within bound"
                <| fun _ ->
                    let signal = defaultSignal |> withValue 120m // diff=30 ≤ 50
                    let result = signalRange signal
                    Expect.isOk result "In-range value should be accepted" ]

          testList
              "BR-D-012 signalSourceReliability"
              [ testCase "rejects reliability below threshold"
                <| fun _ ->
                    let signal = defaultSignal |> withReliability 40.0M
                    let result = signalSourceReliability signal 60.0M
                    Expect.isError result "Unreliable source should be rejected"

                testCase "accepts reliability meeting threshold"
                <| fun _ ->
                    let signal = defaultSignal |> withReliability 80.0M
                    let result = signalSourceReliability signal 60.0M
                    Expect.isOk result "Reliable source should be accepted" ]

          testList
              "BR-D-014 evaluateOnlyFromReceived"
              [ testCase "rejects Accepted status"
                <| fun _ -> Expect.isError (evaluateOnlyFromReceived Accepted) "Accepted should be rejected"
                testCase "rejects Quarantined status"
                <| fun _ -> Expect.isError (evaluateOnlyFromReceived Quarantined) "Quarantined should be rejected"
                testCase "rejects Rejected status"
                <| fun _ -> Expect.isError (evaluateOnlyFromReceived Rejected) "Rejected should be rejected"
                testCase "accepts Received status"
                <| fun _ -> Expect.isOk (evaluateOnlyFromReceived Received) "Received should be accepted" ]

          testList
              "BR-D-004 mustBeAcceptedAndUnassigned"
              [ testCase "rejects non-Accepted status"
                <| fun _ -> Expect.isError (mustBeAcceptedAndUnassigned Quarantined None) "Should reject"
                testCase "rejects already assigned scope"
                <| fun _ ->
                    let scopeId = Builders.scopeId "SCOPE-1"
                    Expect.isError (mustBeAcceptedAndUnassigned Accepted (Some scopeId)) "Should reject"
                testCase "accepts Accepted with no scope"
                <| fun _ -> Expect.isOk (mustBeAcceptedAndUnassigned Accepted None) "Should accept" ] ]
