module Medhavi.Demand.Tests.Edp.EvolveTests

open Expecto
open Medhavi.Demand.EnterpriseDemandPicture.Decisions
open Medhavi.Demand.EnterpriseDemandPicture.Model
open Medhavi.Demand.Tests.Edp.Builders
open Medhavi.Demand.Tests.Builders

[<Tests>]
let tests =
    testList
        "EnterpriseDemandPicture Decisions"
        [

          testList
              "AB-EDP-001 revise"
              [ testCase "creates first version with observation quantity"
                <| fun _ ->
                    let cmd = defaultReviseCmd
                    let result = revise cmd None
                    Expect.isOk result "Should create first version"

                    match result with
                    | Ok events ->
                        Expect.hasLength events 1 "Should emit one event"

                        match events.Head with
                        | EdpRevised edp ->
                            Expect.equal edp.Version 1 "Version should be 1"

                            Expect.equal
                                edp.Status
                                AwaitingPlanningDemandCalculation
                                "Status should be AwaitingPlanningDemandCalculation"

                            let expectedQty = quantity 50m

                            Expect.equal
                                edp.OperationalDemand[cmd.Period]
                                expectedQty
                                "Operational demand should include observation quantity"
                        | _ -> failwith "Expected EdpRevised"
                    | _ -> ()

                testCase "creates new version and accumulates operational demand"
                <| fun _ ->
                    let existing = defaultEdp // already has 100m for week 27
                    let cmd = defaultReviseCmd // adds 50m
                    let result = revise cmd (Some existing)
                    Expect.isOk result "Should create new version"

                    match result with
                    | Ok events ->
                        match events.Head with
                        | EdpRevised edp ->
                            Expect.equal edp.Version 2 "Version should be 2"
                            Expect.equal edp.OperationalDemand[cmd.Period] (quantity 150m) "Should accumulate to 150m"
                        | _ -> failwith "Expected EdpRevised"
                    | _ -> ()

                testCase "succeeds on Published EDP by creating new version"
                <| fun _ ->
                    let published = defaultEdp |> withStatus Published
                    let result = revise defaultReviseCmd (Some published)
                    Expect.isOk result "Should succeed"
                    match result with
                    | Ok events ->
                        match events.Head with
                        | EdpRevised edp ->
                            Expect.equal edp.Version 2 "Version should be 2"
                            Expect.equal edp.Status AwaitingPlanningDemandCalculation "Status should reset to Awaiting"
                        | _ -> failwith "Expected EdpRevised"
                    | _ -> ()

                testCase "fails on Superseded EDP"
                <| fun _ ->
                    let superseded = defaultEdp |> withStatus Superseded
                    let result = revise defaultReviseCmd (Some superseded)
                    Expect.isError result "Should reject Superseded" ]

          testList
              "AB-EDP-002 calculate"
              [ testCase "computes PlanningDemand and transitions to ReadyForPublication"
                <| fun _ ->
                    let state = defaultEdp // status = AwaitingPlanningDemandCalculation
                    let result = calculate defaultCalculateCmd state
                    Expect.isOk result "Should calculate"

                    match result with
                    | Ok events ->
                        match events.Head with
                        | EdpCalculated edp ->
                            Expect.equal edp.Status ReadyForPublication "Status should be ReadyForPublication"
                            Expect.isTrue (edp.PlanningDemand.Count > 0) "PlanningDemand should be populated"
                        | _ -> failwith "Expected EdpCalculated"
                    | _ -> ()

                testCase "fails if not in AwaitingPlanningDemandCalculation"
                <| fun _ ->
                    let state = defaultEdp |> withStatus ReadyForPublication
                    let result = calculate defaultCalculateCmd state
                    Expect.isError result "Should reject non‑Awaiting status"

                testCase "fails on Published EDP"
                <| fun _ ->
                    let state = defaultEdp |> withStatus Published
                    let result = calculate defaultCalculateCmd state
                    Expect.isError result "Should reject Published" ]

          testList
              "AB-EDP-003 publish"
              [ testCase "publishes and records time"
                <| fun _ ->
                    let state = defaultEdp |> withStatus ReadyForPublication
                    let result = publish defaultPublishCmd state
                    Expect.isOk result "Should publish"

                    match result with
                    | Ok events ->
                        match events.Head with
                        | EdpPublished(edp, _) ->
                            Expect.equal edp.Status Published "Status should be Published"
                            Expect.isSome edp.PublicationTime "PublicationTime should be set"
                        | _ -> failwith "Expected EdpPublished"
                    | _ -> ()

                testCase "fails if not ReadyForPublication"
                <| fun _ ->
                    let state = defaultEdp // AwaitingPlanningDemandCalculation
                    let result = publish defaultPublishCmd state
                    Expect.isError result "Should reject non‑Ready status"

                testCase "fails on already Published"
                <| fun _ ->
                    let state = defaultEdp |> withStatus Published
                    let result = publish defaultPublishCmd state
                    Expect.isError result "Should reject Published" ]

          testList
              "decide command routing"
              [ testCase "Revise command succeeds with no existing state"
                <| fun _ ->
                    let cmd = EdpCommand.Revise defaultReviseCmd
                    let result = decide cmd None
                    Expect.isOk result "Should succeed"

                testCase "Calculate command fails with None state"
                <| fun _ ->
                    let cmd = EdpCommand.Calculate defaultCalculateCmd
                    let result = decide cmd None
                    Expect.isError result "Should fail"

                testCase "Publish command fails with None state"
                <| fun _ ->
                    let cmd = EdpCommand.Publish defaultPublishCmd
                    let result = decide cmd None
                    Expect.isError result "Should fail" ] ]
