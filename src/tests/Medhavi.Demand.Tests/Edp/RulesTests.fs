module Medhavi.Demand.Tests.Edp.RulesTests

open Expecto
open Medhavi.Demand.EnterpriseDemandPicture.Model
open Medhavi.Demand.EnterpriseDemandPicture.Rules

[<Tests>]
let tests =
    testList
        "EnterpriseDemandPicture Rules"
        [

          testList
              "BR-D-006 publishedImmutable"
              [ testCase "rejects Published status"
                <| fun _ -> Expect.isError (publishedImmutable Published) "Should reject Published"
                testCase "accepts Draft status" <| fun _ -> Expect.isOk (publishedImmutable Draft) "Should accept Draft"
                testCase "accepts AwaitingPlanningDemandCalculation status"
                <| fun _ -> Expect.isOk (publishedImmutable AwaitingPlanningDemandCalculation) "Should accept Awaiting"
                testCase "accepts ReadyForPublication status"
                <| fun _ -> Expect.isOk (publishedImmutable ReadyForPublication) "Should accept ReadyForPublication" ]

          testList
              "BR-D-056 supersededImmutable"
              [ testCase "rejects Superseded status"
                <| fun _ -> Expect.isError (supersededImmutable Superseded) "Should reject Superseded"
                testCase "accepts other statuses"
                <| fun _ -> Expect.isOk (supersededImmutable Draft) "Should accept Draft" ] ]
