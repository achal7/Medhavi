namespace Medhavi.Scheduler.Tests.Mrp.Application

open System
open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
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
                let sp = stockingPointWarehouse
                let fcId = "fc-1"

                let forecast =
                    { DemandId = fcId
                      SkuId = sku
                      NodeId = node
                      StockingPointId = sp
                      Quantity = createQty 100m
                      RequiredDate = createTimestamp DateTimeOffset.UtcNow
                      Source = Forecast fcId
                      Priority = None }

                let order =
                    { DemandId = "ord-1"
                      SkuId = sku
                      NodeId = node
                      StockingPointId = sp
                      Quantity = createQty 40m
                      RequiredDate = createTimestamp (DateTimeOffset.UtcNow.AddHours(2.0))
                      Source = CustomerOrder("ord-1", "1")
                      Priority = Some 1 }

                let policy =
                    { ForecastConsumptionPolicy.Enabled = true
                      ConsumptionWindow = TimeSpan.FromDays(2.0)
                      Strategy = BidirectionalConsumption }

                let consumed = ForecastConsumption.consumeForecasts policy [ forecast ] [ order ]
                
                test <@ List.length consumed = 1 @>
                test <@ Quantity.value (List.head consumed).Quantity = 60m @>
            )
        ]
