module Medhavi.Demand.Tests.ForecastPublication.ComputationServiceTests

open Expecto
open Medhavi.Contracts
open Medhavi.SharedKernel
open Medhavi.Demand.ForecastPublication.ComputationService
open Medhavi.Demand.Tests.Builders

let getHistory _ _ = task { return [ 100m; 110m; 120m ] }

[<Tests>]
let tests =
    testList
        "ForecastComputationService"
        [ testCaseTask "returns forecasts for each coverage item using SES"
          <| fun () ->
              task {
                  let service = create getHistory

                  let input: ForecastComputationInput =
                      { Coverage = [ (skuId "SKU1", stockingPointId "SP1") ]
                        ForecastHorizon = "30.00:00:00"
                        BucketConfig = "Weekly"
                        ModelId = "Model-1"
                        TargetPeriod = Some (PlanningPeriod.PlanningWeek(2027, 27))
                        TargetReconciliationTotal = None }

                  let! result = service.ComputeForecasts input
                  Expect.isOk result ""

                  match result with
                  | Ok forecasts ->
                      Expect.hasLength forecasts 1 "Should have one forecast"
                      let fc = forecasts[0]
                      Expect.equal fc.Mean 108.1m "Mean should be calculated via SES (108.1)"
                  | _ -> failwith "unexpected"
              }

          testCaseTask "reconciles forecasts proportionally to a target total"
          <| fun () ->
              task {
                  let service = create getHistory

                  let input: ForecastComputationInput =
                      { Coverage = [ (skuId "SKU1", stockingPointId "SP1"); (skuId "SKU2", stockingPointId "SP1") ]
                        ForecastHorizon = "30.00:00:00"
                        BucketConfig = "Weekly"
                        ModelId = "Model-1"
                        TargetPeriod = Some (PlanningPeriod.PlanningWeek(2027, 27))
                        TargetReconciliationTotal = Some 300.0m }

                  let! result = service.ComputeForecasts input
                  Expect.isOk result ""

                  match result with
                  | Ok forecasts ->
                      Expect.hasLength forecasts 2 "Should have two forecasts"
                      let sum = forecasts |> List.map (fun f -> f.Mean) |> List.sum
                      Expect.equal sum 300.0m "Sum of reconciled forecasts should match target total exactly"
                  | _ -> failwith "unexpected"
              } ]
