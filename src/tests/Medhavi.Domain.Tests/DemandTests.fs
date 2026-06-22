namespace Medhavi.Domain.Tests

open System
open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Demand.Domain
open Medhavi.Demand.Domain.DemandLineAgg

module DemandDomainTests =

    let testSku = SkuId.create "SKU-A" |> Result.get
    let testSP = StockingPointId.create "LOC-1" |> Result.get
    let testQty = Quantity.create 100.0m |> Result.get

    let createBaseIngest () =
        { DemandLineId = "dl-1"
          DemandOrderId = "ord-1"
          SkuId = testSku
          StockingPointId = testSP
          CustomerId = "cust-1"
          Quantity = testQty
          UnitOfMeasure = "PCS"
          OrderDate = Timestamp.create DateTimeOffset.UtcNow
          EarliestDeliveryDate = None
          RequestedDeliveryDate = Timestamp.create DateTimeOffset.UtcNow
          LatestDeliveryDate = None
          Priority = DemandPriority.Normal
          DemandCategory = DemandCategory.CustomerOrderDemand
          IsFirm = false
          IsFrozen = false
          Provenance = 
              { SourceSystem = "ERP"
                ExternalRef = "ord-1"
                MessageId = "msg-1"
                Revision = Revision.initial
                ScenarioId = None } }

    [<Tests>]
    let tests =
        testList "Demand Aggregate & Forecast Consumption Tests" [
            
            testCase "1. Ingest command creates active state with correct attributes" (fun () ->
                let cmd = createBaseIngest()
                let result = decide (IngestDemandLine cmd) None
                
                match result with
                | Ok res ->
                    let state = res.NewState
                    test <@ state.Status = DemandStatus.Active @>
                    test <@ state.OpenQuantity = testQty @>
                    test <@ state.FulfilledQuantity = Quantity.Zero @>
                    test <@ state.Priority = DemandPriority.Normal @>
                    test <@ state.IsFrozen = false @>
                    test <@ state.IsOnHold = false @>
                | Error err -> failwithf "Failed: %A" err
            )

            testCase "2. Fulfillment should transition status to Fulfilled when open quantity is zero" (fun () ->
                let cmd = createBaseIngest()
                let ingestedState = (decide (IngestDemandLine cmd) None |> Result.get).NewState
                
                let fulfillCmd =
                    { DemandLineId = "dl-1"
                      Quantity = testQty
                      ActualDeliveryDate = Timestamp.create DateTimeOffset.UtcNow }
                
                let result = decide (RecordExecutionFulfillment fulfillCmd) (Some ingestedState)
                
                match result with
                | Ok res ->
                    let state = res.NewState
                    test <@ state.Status = DemandStatus.Fulfilled @>
                    test <@ state.OpenQuantity = Quantity.Zero @>
                    test <@ state.FulfilledQuantity = testQty @>
                | Error err -> failwithf "Failed: %A" err
            )

            testCase "3. Fulfillment should reject when quantity exceeds open quantity" (fun () ->
                let cmd = createBaseIngest()
                let ingestedState = (decide (IngestDemandLine cmd) None |> Result.get).NewState
                
                let overFulfillQty = Quantity.create 110.0m |> Result.get
                let fulfillCmd =
                    { DemandLineId = "dl-1"
                      Quantity = overFulfillQty
                      ActualDeliveryDate = Timestamp.create DateTimeOffset.UtcNow }
                
                let result = decide (RecordExecutionFulfillment fulfillCmd) (Some ingestedState)
                test <@ Result.isError result @>
            )

            testCase "4. Cancellation should reject for firm orders unless override is specified" (fun () ->
                let cmd = { createBaseIngest() with IsFirm = true }
                let ingestedState = (decide (IngestDemandLine cmd) None |> Result.get).NewState
                
                let cancelCmd =
                    { DemandLineId = "dl-1"
                      Reason = "User cancel"
                      CancelledAtUtc = Timestamp.create DateTimeOffset.UtcNow
                      ForceOverride = false }
                
                let result = decide (CancelDemandLine cancelCmd) (Some ingestedState)
                test <@ Result.isError result @>

                let forceCancelCmd = { cancelCmd with ForceOverride = true }
                let forceResult = decide (CancelDemandLine forceCancelCmd) (Some ingestedState)
                test <@ Result.isOk forceResult @>
            )

            testCase "5. Forecast consumption matching by policy" (fun () ->
                let now = DateTimeOffset.UtcNow
                let fc1 =
                    { ForecastId = "fc-1"
                      SkuId = testSku
                      StockingPointId = testSP
                      PeriodStart = Timestamp.create (now.AddDays(-1.0))
                      PeriodEnd = Timestamp.create (now.AddDays(1.0))
                      OriginalQuantity = testQty
                      RemainingQuantity = testQty }
                
                let ord1 =
                    { DemandLineId = "dl-1"
                      DemandOrderId = "ord-1"
                      SkuId = testSku
                      StockingPointId = testSP
                      CustomerId = "cust-1"
                      Quantity = Quantity.create 40.0m |> Result.get
                      UnitOfMeasure = "PCS"
                      OrderDate = Timestamp.create now
                      EarliestDeliveryDate = None
                      RequestedDeliveryDate = Timestamp.create now
                      LatestDeliveryDate = None
                      ConfirmedDeliveryDate = None
                      ActualDeliveryDate = None
                      ConfirmedQty = Quantity.Zero
                      Priority = DemandPriority.High
                      DemandCategory = DemandCategory.CustomerOrderDemand
                      IsFirm = false
                      IsFrozen = false
                      FrozenUntilUtc = None
                      IsOnHold = false
                      OnHoldReason = None
                      CancelReason = None
                      CancelledAtUtc = None
                      Provenance = { SourceSystem = ""; ExternalRef = ""; MessageId = ""; Revision = Revision.initial; ScenarioId = None }
                      OpenQuantity = Quantity.create 40.0m |> Result.get
                      FulfilledQuantity = Quantity.Zero
                      Status = DemandStatus.Active }

                // StrictBucket consumption
                let resStrict = ForecastConsumptionService.consume ConsumptionPolicy.StrictBucket 2 2 [ fc1 ] [ ord1 ]
                test <@ resStrict.Allocations.Length = 1 @>
                test <@ Quantity.value (List.head resStrict.ResidualForecasts).RemainingQuantity = 60.0m @>
                
                // Forecast outside strict bucket
                let fc2 = { fc1 with PeriodStart = now.AddDays(5.0); PeriodEnd = now.AddDays(6.0) }
                let resStrictOut = ForecastConsumptionService.consume ConsumptionPolicy.StrictBucket 2 2 [ fc2 ] [ ord1 ]
                test <@ resStrictOut.Allocations.Length = 0 @>
                
                // BackwardOnly consumption inside window
                let resBackward = ForecastConsumptionService.consume ConsumptionPolicy.BackwardOnly 6 2 [ fc2 ] [ { ord1 with RequestedDeliveryDate = now.AddDays(7.0) } ]
                test <@ resBackward.Allocations.Length = 1 @>
            )
        ]
