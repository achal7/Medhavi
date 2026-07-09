module Medhavi.Demand.Tests.Edp.DecisionsTests

open Expecto
open Medhavi.Demand.EnterpriseDemandPicture.Model
open Medhavi.Demand.Tests.Edp.Builders
open Medhavi.Demand.Tests.Builders
open Medhavi.SharedKernel

[<Tests>]
let tests =
    testList
        "EnterpriseDemandPicture Evolve"
        [

          testCase "EdpRevised creates new state with AwaitingCalculation"
          <| fun _ ->
              let edp = defaultEdp
              let event = EdpRevised edp
              let result = evolve event None
              Expect.isSome result "State should be created"

              Expect.equal
                  result.Value.Status
                  AwaitingPlanningDemandCalculation
                  "Status should be AwaitingPlanningDemandCalculation"

          testCase "EdpCalculated transitions to ReadyForPublication"
          <| fun _ ->
              let state = defaultEdp

              let newEdp =
                  { state with
                      Status = ReadyForPublication
                      PlanningDemand =
                          Map.ofList
                              [ Medhavi.Contracts.PlanningWeek(2027, 27),
                                { OperationalDemand = quantity 100m
                                  Adjustment = quantity 0m
                                  Override = quantity 0m
                                  FinalQuantity = quantity 100m } ] }

              let event = EdpCalculated newEdp
              let result = evolve event (Some state)
              Expect.equal result.Value.Status ReadyForPublication "Status should be ReadyForPublication"

          testCase "EdpPublished transitions to Published and sets PublicationTime"
          <| fun _ ->
              let state = defaultEdp |> withStatus ReadyForPublication
              let pubTime = Timestamp.now

              let newEdp =
                  { state with
                      Status = Published
                      PublicationTime = Some pubTime }

              let event = EdpPublished(newEdp, None)
              let result = evolve event (Some state)
              Expect.equal result.Value.Status Published "Status should be Published"
              Expect.equal result.Value.PublicationTime (Some pubTime) "PublicationTime should be set" ]
