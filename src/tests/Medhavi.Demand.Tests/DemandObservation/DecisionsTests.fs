module Medhavi.Demand.Tests.DemandObservation.DecisionsTests

open System
open Expecto
open Medhavi.Demand.DemandObservation.Model
open Medhavi.Demand.DemandObservation.Decisions
open Medhavi.Demand.Tests.Builders.DemandSignalBuilder
open Medhavi.Demand.Tests.Builders
open Medhavi.Demand.Tests.DemandObservation.EvolveTests

let defaultEstablishCmd: EstablishObservationCmd =
    { ObservationId = obsId "OBS-TEST-001"
      SkuId = skuId "SKU-001"
      StockingPointId = stockingPointId "SP-001"
      Quantity = quantity 100m
      ObservationType = SalesOrder
      BusinessTime = timestamp(DateTimeOffset.UtcNow.AddHours(-1.0))
      CustomerId = None
      PromotionRef = None
      CampaignRef = None
      ContractRef = None
      Provenance =
        { SourceSystem = "ERP"
          ExternalRef = "ORD-001"
          MessageId = "msg-001"
          Revision = Medhavi.SharedKernel.Revision 1
          ScenarioId = None } }

[<Tests>]
let tests =
    testList
        "DemandObservation Decisions"
        [

          testList
              "AB-D-001 establishObservation"
              [ testCase "creates observation in Received state"
                <| fun _ ->
                    let cmd = defaultEstablishCmd
                    let result = establishObservation cmd
                    Expect.isOk result "Should succeed"

                    match result with
                    | Ok events ->
                        Expect.hasLength events 1 "Should emit one event"

                        match events.Head with
                        | ObservationEstablished obs ->
                            Expect.equal obs.Status Received "Status should be Received"
                            Expect.equal obs.Id cmd.ObservationId "Id should match"
                            Expect.equal obs.Quantity cmd.Quantity "Quantity should match"
                        | _ -> failwith "Expected ObservationEstablished event"
                    | _ -> () ]

          testList
              "DE-D-010 acceptDemandObservation"
              [ testCase "accepts valid signal and returns Accepted"
                <| fun _ ->
                    let obs = defaultReceived
                    let signal = defaultSignal
                    let result = acceptDemandObservation (Some signal) obs
                    Expect.isOk result "Should accept valid signal"

                    match result with
                    | Ok events ->
                        Expect.hasLength events 1 "Should emit one event"

                        match events.Head with
                        | ObservationAccepted(obsId, decision) ->
                            Expect.equal obsId obs.Id "Id should match"
                            Expect.isNone decision.WarningCode "No warning expected"
                        | _ -> failwith "Expected ObservationAccepted event"
                    | _ -> ()

                testCase "accepts without signal"
                <| fun _ ->
                    let obs = defaultReceived
                    let result = acceptDemandObservation None obs
                    Expect.isOk result "Should accept without signal"

                testCase "rejects old signal (BR-D-010)"
                <| fun _ ->
                    let obs = defaultReceived
                    let signal = defaultSignal |> withTimestamp(DateTimeOffset.UtcNow.AddHours(-2.0))
                    let result = acceptDemandObservation (Some signal) obs
                    Expect.isOk result "Should succeed with Ok"

                    match result with
                    | Ok [ ObservationQuarantined _ ] -> ()
                    | _ -> failwith "Expected ObservationQuarantined"

                testCase "rejects out-of-range signal (BR-D-011)"
                <| fun _ ->
                    let obs = defaultReceived
                    let signal = defaultSignal |> withValue 500m
                    let result = acceptDemandObservation (Some signal) obs
                    Expect.isOk result "Should succeed with Ok"

                    match result with
                    | Ok [ ObservationRejected _ ] -> ()
                    | _ -> failwith "Expected ObservationRejected"

                testCase "rejects unreliable source (BR-D-012)"
                <| fun _ ->
                    let obs = defaultReceived
                    let signal = defaultSignal |> withReliability 20.0M
                    let result = acceptDemandObservation (Some signal) obs
                    Expect.isOk result "Should succeed with Ok"

                    match result with
                    | Ok [ ObservationQuarantined _ ] -> ()
                    | _ -> failwith "Expected ObservationQuarantined" ]

          testList
              "AB-? assignScope"
              [ testCase "assigns scope to Accepted observation"
                <| fun _ ->
                    let obs = defaultReceived |> withStatus Accepted
                    let scope = scopeId "SCOPE-001"

                    let cmd =
                        { ObservationId = obs.Id
                          PlanningScopeId = scope }

                    let result = assignScope cmd obs
                    Expect.isOk result "Should succeed"

                    match result with
                    | Ok events ->
                        Expect.hasLength events 1 "Should emit one event"

                        match events.Head with
                        | ObservationScopeAssigned(obsId, scopeId) ->
                            Expect.equal obsId obs.Id "Id should match"
                            Expect.equal scopeId scope "ScopeId should match"
                        | _ -> failwith "Expected ObservationScopeAssigned"
                    | _ -> ()

                testCase "rejects non-Accepted status"
                <| fun _ ->
                    let obs = defaultReceived // still Received

                    let cmd =
                        { ObservationId = obs.Id
                          PlanningScopeId = scopeId "SCOPE-001" }

                    let result = assignScope cmd obs
                    Expect.isError result "Should reject non-Accepted"

                testCase "rejects already assigned"
                <| fun _ ->
                    let obs = defaultReceived |> withStatus Accepted |> withScope "SCOPE-001"

                    let cmd =
                        { ObservationId = obs.Id
                          PlanningScopeId = scopeId "SCOPE-002" }

                    let result = assignScope cmd obs
                    Expect.isError result "Should reject already assigned" ]

          testList
              "decide command routing"
              [ testCase "Establish command with None state succeeds"
                <| fun _ ->
                    let cmd = ObservationCommand.Establish defaultEstablishCmd
                    let result = decide cmd None
                    Expect.isOk result "Establish should succeed with no state"

                testCase "Establish command with Some state is idempotent"
                <| fun _ ->
                    let cmd = ObservationCommand.Establish defaultEstablishCmd
                    let result = decide cmd (Some defaultReceived)
                    Expect.isOk result "Should succeed idempotently when state already exists"
                    match result with
                    | Ok decision ->
                        Expect.isEmpty decision.Events "Idempotent establish should return no new events"
                    | Error e -> failwithf "Should be Ok: %A" e

                testCase "Evaluate command with Some Received state succeeds"
                <| fun _ ->
                    let obs = defaultReceived

                    let cmd =
                        ObservationCommand.Evaluate
                            { ObservationId = obs.Id
                              Signal = None }

                    let result = decide cmd (Some obs)
                    Expect.isOk result "Evaluate should succeed on Received"

                testCase "Evaluate command with None state fails"
                <| fun _ ->
                    let cmd =
                        ObservationCommand.Evaluate
                            { ObservationId = obsId "OBS-999"
                              Signal = None }

                    let result = decide cmd None
                    Expect.isError result "Evaluate should fail when state is None" ] ]
