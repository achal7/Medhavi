module Medhavi.Demand.Tests.PlanningScope.DecisionsTests

open Expecto
open Medhavi.Demand.PlanningScope.Model
open Medhavi.Demand.PlanningScope.Decisions
open Medhavi.Demand.Tests.PlanningScope.EvolveTests
open Medhavi.Demand.Tests.Builders

let defaultDetermineCmd: DeterminePlanningScopeCmd =
    { ScopeId = scopeId "SCOPE-001"
      SkuId = skuId "SKU-001"
      StockingPointId = stockingPointId "SP-001"
      CustomerId = None
      PlanningPeriod = Medhavi.Contracts.PlanningWeek(2027, 27) }

[<Tests>]
let tests =
    testList
        "PlanningScope Decisions"
        [

          testList
              "AB-PS-001 determineScope"
              [ testCase "creates new scope when none exists"
                <| fun _ ->
                    let cmd = defaultDetermineCmd
                    let result = determineScope cmd None
                    Expect.isOk result "Should create scope"

                    match result with
                    | Ok events ->
                        Expect.hasLength events 1 "Should emit one event"

                        match events.Head with
                        | ScopeDetermined scope -> Expect.equal scope.Status Active "Status should be Active"
                        | _ -> failwith "Expected ScopeDetermined"
                    | _ -> ()

                testCase "returns empty when scope already exists (idempotent)"
                <| fun _ ->
                    let existing = defaultScope
                    let cmd = defaultDetermineCmd
                    let result = determineScope cmd (Some existing)
                    Expect.isOk result "Should be ok"

                    match result with
                    | Ok events -> Expect.isEmpty events "Should emit no events for existing scope"
                    | _ -> () ]

          testList
              "archiveScope"
              [ testCase "archives an Active scope"
                <| fun _ ->
                    let scope = defaultScope
                    let result = archiveScope scope
                    Expect.isOk result "Should archive"

                    match result with
                    | Ok events ->
                        Expect.hasLength events 1 "Should emit one event"

                        match events.Head with
                        | ScopeArchived id -> Expect.equal id scope.Id "Id should match"
                        | _ -> failwith "Expected ScopeArchived"
                    | _ -> ()

                testCase "rejects already Archived scope"
                <| fun _ ->
                    let scope = defaultScope |> withStatus Archived
                    let result = archiveScope scope
                    Expect.isError result "Should reject already Archived" ]

          testList
              "decide command routing"
              [ testCase "Determine command with None state creates scope"
                <| fun _ ->
                    let cmd = PlanningScopeCommand.Determine defaultDetermineCmd
                    let result = decide cmd None
                    Expect.isOk result "Should succeed"

                testCase "Archive command with Some Active state archives"
                <| fun _ ->
                    let scope = defaultScope
                    let cmd = PlanningScopeCommand.Archive { ScopeId = scope.Id }
                    let result = decide cmd (Some scope)
                    Expect.isOk result "Should succeed"

                testCase "Archive command with None state fails"
                <| fun _ ->
                    let cmd = PlanningScopeCommand.Archive { ScopeId = scopeId "SCOPE-999" }
                    let result = decide cmd None
                    Expect.isError result "Should fail when scope missing" ] ]
