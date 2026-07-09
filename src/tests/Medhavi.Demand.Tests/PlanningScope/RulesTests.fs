module Medhavi.Demand.Tests.PlanningScope.RulesTests

open Expecto
open Medhavi.Demand.PlanningScope.Model
open Medhavi.Demand.PlanningScope.Rules

[<Tests>]
let tests =
    testList
        "PlanningScope Rules"
        [

          testList
              "BR-D-048 neverDeleted"
              [ testCase "rejects Archived status"
                <| fun _ -> Expect.isError (neverDeleted Archived) "Should reject Archived"
                testCase "accepts Active status" <| fun _ -> Expect.isOk (neverDeleted Active) "Should accept Active" ] ]
