namespace Medhavi.Scheduler.Tests.Mrp.Application

open System
open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.Policies
open Medhavi.Scheduler.Mrp.Domain.Algorithms
open Medhavi.Scheduler.Tests.TestCommon

module ForecastConsumptionTests =

    [<Tests>]
    let tests =
        testList "MRP Application - Forecast Consumption Tests" [

            testCase "Scenario: Forecast Consumption Window - should consume forecast inside window" (fun () ->
                let sku = skuFG
                let node = nodeWarehouse
                let fcId = "fc-1"

                let forecast =
                    { ForecastId = fcId
                      SkuId = sku
                      NodeId = node
                      Quantity = createQty 100m
                      PeriodStart = createTimestamp DateTimeOffset.UtcNow
                      PeriodEnd = createTimestamp (DateTimeOffset.UtcNow.AddDays(1.0)) }

                let order =
                    { OrderId = OrderId.create "ord-1" |> getOk
                      LineId = "1"
                      SkuId = sku
                      NodeId = node
                      Quantity = createQty 40m
                      DueDate = createTimestamp (DateTimeOffset.UtcNow.AddHours(2.0))
                      Priority = 1
                      IsExpedited = false }

                let policy =
                    { ForecastConsumptionPolicy.Enabled = true
                      ConsumptionWindow = TimeSpan.FromDays(2.0)
                      Strategy = BidirectionalConsumption }

                let consumed = ForecastConsumption.consumeForecasts policy [ forecast ] [ order ]
                
                test <@ List.length consumed = 1 @>
                test <@ Quantity.value (List.head consumed).Quantity = 60m @>
            )
        ]
