namespace Medhavi.Scheduler.Replenishment.Tests

open System
open System.Threading.Tasks
open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Contracts.Domain
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Tests.TestCommon
open Medhavi.Scheduler.Replenishment

module ReplenishmentTests =

    [<Tests>]
    let tests =
        testList
            "Replenishment Engine Tests"
            [ testCase "should calculate correct targets for static safety/min/max policies" (fun () ->
                  let skuId = skuFG
                  let spId = spWarehouse

                  let targetDef =
                      { Id = "target-1"
                        SkuId = SkuId.value skuId
                        StockingPointId = StockingPointId.value spId
                        ReplenishmentPolicy = 
                            Some { Safety = 15.0m
                                   MinQty = Some 20.0m
                                   MaxQty = Some 100.0m
                                   CoverDays = None
                                   LotSize = None
                                   Expedite = false }
                        SafetyStockQty = None
                        MinQty = None
                        MaxQty = None
                        TargetServiceLevel = None
                        CoverDays = None
                        SeasonalAdjustments = []
                        EffectiveStart = None
                        EffectiveEnd = None
                        IsActive = true }

                  let target = 
                      ReplenishmentDomain.calculateTargets 
                          skuId 
                          spId 
                          targetDef
                          []
                          None
                          Timestamp.now

                  test <@ target.SkuId = skuId @>
                  test <@ target.StockingPointId = spId @>
                  test <@ Quantity.value target.SafetyStock = 15.0m @>
                  test <@ Quantity.value target.MinStock = 20.0m @>
                  test <@ target.MaxStock = Some (Quantity.clampToZero 100.0m) @>)

              testCase "should calculate targets with seasonal adjustment factor" (fun () ->
                  let skuId = SkuId.create "SKU-BIKE" |> getOk
                  let spId = StockingPointId.create "SP-WAREHOUSE" |> getOk
                  let now = DateTimeOffset.UtcNow

                  let targetDef =
                      { Id = "target-1"
                        SkuId = SkuId.value skuId
                        StockingPointId = StockingPointId.value spId
                        ReplenishmentPolicy = 
                            Some { Safety = 10.0m
                                   MinQty = Some 20.0m
                                   MaxQty = Some 100.0m
                                   CoverDays = None
                                   LotSize = None
                                   Expedite = false }
                        SafetyStockQty = None
                        MinQty = None
                        MaxQty = None
                        TargetServiceLevel = None
                        CoverDays = None
                        // Seasonal factor of 1.5x active now
                        SeasonalAdjustments = [
                            { PeriodStart = now.AddDays(-1.0)
                              PeriodEnd = now.AddDays(1.0)
                              AdjustmentFactor = 1.5m }
                        ]
                        EffectiveStart = None
                        EffectiveEnd = None
                        IsActive = true }

                  let target = 
                      ReplenishmentDomain.calculateTargets 
                          skuId 
                          spId 
                          targetDef
                          []
                          None
                          (Timestamp.create now)

                  // 10 * 1.5 = 15.0 safety
                  test <@ Quantity.value target.SafetyStock = 15.0m @>
                  // 20 * 1.5 = 30.0 min
                  test <@ Quantity.value target.MinStock = 30.0m @>
                  // 100 * 1.5 = 150.0 max
                  test <@ target.MaxStock = Some (Quantity.clampToZero 150.0m) @>)

              testCase "should calculate dynamic targets incorporating cover days and forecast average demand" (fun () ->
                  let skuId = SkuId.create "SKU-BIKE" |> getOk
                  let spId = StockingPointId.create "SP-WAREHOUSE" |> getOk
                  let now = DateTimeOffset.UtcNow

                  let targetDef =
                      { Id = "target-1"
                        SkuId = SkuId.value skuId
                        StockingPointId = StockingPointId.value spId
                        ReplenishmentPolicy = None
                        SafetyStockQty = None
                        MinQty = None
                        MaxQty = None
                        TargetServiceLevel = None
                        CoverDays = Some 5.0m // 5 cover days
                        SeasonalAdjustments = []
                        EffectiveStart = None
                        EffectiveEnd = None
                        IsActive = true }

                  let forecasts = [
                      { DemandId = "f1"
                        SkuId = skuId
                        NodeId = NodeId.create "node-1" |> getOk
                        StockingPointId = spId
                        Quantity = Quantity.clampToZero 20.0m
                        RequiredDate = Timestamp.create now
                        Source = Forecast "f1"
                        Priority = None }
                      { DemandId = "f2"
                        SkuId = skuId
                        NodeId = NodeId.create "node-1" |> getOk
                        StockingPointId = spId
                        Quantity = Quantity.clampToZero 20.0m
                        RequiredDate = Timestamp.create (now.AddDays(9.0))
                        Source = Forecast "f2"
                        Priority = None }
                  ]

                  let target = 
                      ReplenishmentDomain.calculateTargets 
                          skuId 
                          spId 
                          targetDef
                          forecasts
                          None 
                          (Timestamp.create now)

                  // 5 cover days * 4.0 average daily demand = 20 safety stock target
                  test <@ Quantity.value target.SafetyStock = 20.0m @>
                  test <@ Quantity.value target.MinStock = 20.0m @>
              )

              testCase "should detect shortfall when net stock is below min level (reactive)" (fun () ->
                  let skuId = SkuId.create "SKU-BIKE" |> getOk
                  let spId = StockingPointId.create "SP-WAREHOUSE" |> getOk
                  let now = Timestamp.now

                  let snapshot = 
                      { OnHand = 10.0m
                        Inbound = [ (Timestamp.value now, 5.0m) ]
                        Reservations = [ (Timestamp.value now, 2.0m) ]
                        Safety = 0.0m }

                  let target = 
                      { SkuId = skuId
                        StockingPointId = spId
                        SafetyStock = Quantity.clampToZero 15.0m
                        MinStock = Quantity.clampToZero 20.0m
                        MaxStock = None
                        LotSize = None }

                  let alertOpt = ReplenishmentDomain.detectShortfall snapshot target now
                  test <@ Option.isSome alertOpt @>
                  
                  let alert = Option.get alertOpt
                  test <@ alert.SkuId = skuId @>
                  test <@ Quantity.value alert.NetStock = 13.0m @>
                  test <@ Quantity.value alert.ShortfallQuantity = 7.0m @>
              )

              testCase "should detect proactive shortfall based on projected stockout date" (fun () ->
                  let skuId = SkuId.create "SKU-BIKE" |> getOk
                  let spId = StockingPointId.create "SP-WAREHOUSE" |> getOk
                  let now = DateTimeOffset.UtcNow

                  // Net stock = OnHand (15) + Inbound (0) - Reservations (0) = 15
                  let snapshot = 
                      { OnHand = 15.0m
                        Inbound = []
                        Reservations = []
                        Safety = 0.0m }

                  let target = 
                      { SkuId = skuId
                        StockingPointId = spId
                        SafetyStock = Quantity.clampToZero 10.0m
                        MinStock = Quantity.clampToZero 10.0m
                        MaxStock = None
                        LotSize = None }

                  // Forecasts:
                  // Day 2: demand 10 -> net stock drops to 5 (ok)
                  // Day 5: demand 10 -> net stock drops to -5 (stockout!)
                  let forecasts = [
                      { DemandId = "f1"; SkuId = skuId; NodeId = NodeId.create "node-1" |> getOk; StockingPointId = spId; Quantity = Quantity.clampToZero 10.0m; RequiredDate = Timestamp.create (now.AddDays(2.0)); Source = Forecast "f1"; Priority = None }
                      { DemandId = "f2"; SkuId = skuId; NodeId = NodeId.create "node-1" |> getOk; StockingPointId = spId; Quantity = Quantity.clampToZero 10.0m; RequiredDate = Timestamp.create (now.AddDays(5.0)); Source = Forecast "f2"; Priority = None }
                  ]

                  // Evaluating with ForecastBased(10 days lookahead)
                  let trigger = ReplenishmentTrigger.ForecastBased 10
                  let alertOpt = 
                      ReplenishmentDomain.detectShortfallWithForecast 
                          snapshot 
                          target 
                          forecasts 
                          trigger 
                          (Timestamp.create now)

                  test <@ Option.isSome alertOpt @>
                  let alert = Option.get alertOpt
                  // Cumulative demand up to Day 5 is 20 + 10 (SafetyStock) = 30 trigger level
                  // 30 - 15 (net stock) = 15 shortfall quantity
                  test <@ Quantity.value alert.ShortfallQuantity = 15.0m @>
                  test <@ (Timestamp.value alert.Timestamp - now).Days = 5 @> // timestamp is target stockout date
              )

              testCaseAsync "should trigger planning run when shortfall is detected in application service" (async {
                  let skuId = SkuId.create "SKU-BIKE" |> getOk
                  let spId = StockingPointId.create "SP-WAREHOUSE" |> getOk
                  let now = Timestamp.now

                  let mutable triggered = false
                  let mutable triggeredQty = Quantity.Zero

                  let mockDeps : ReplenishmentDependencies = 
                      { GetSnapshot = fun _ _ _ -> task { 
                            return Ok { OnHand = 5.0m; Inbound = []; Reservations = []; Safety = 0.0m } 
                          }
                        GetTargets = fun () -> task {
                            return [
                                { Id = "target-1"
                                  SkuId = SkuId.value skuId
                                  StockingPointId = StockingPointId.value spId
                                  ReplenishmentPolicy = None
                                  SafetyStockQty = Some 10.0m
                                  MinQty = Some 15.0m
                                  MaxQty = None
                                  TargetServiceLevel = None
                                  CoverDays = None
                                  SeasonalAdjustments = []
                                  EffectiveStart = None
                                  EffectiveEnd = None
                                  IsActive = true }
                            ]
                        }
                        GetForecasts = fun _ _ -> task { return [] }
                        TriggerPlanning = fun _ _ qty _ -> task {
                            triggered <- true
                            triggeredQty <- qty
                            return Ok ()
                        }
                        PublishAlert = fun _ -> task { return () } }

                  let! result = 
                      ReplenishmentService.runReplenishmentEvaluation 
                          mockDeps 
                          ReplenishmentTrigger.StockLevel 
                          now 
                      |> Async.AwaitTask
                  
                  match result with
                  | Ok alerts ->
                      test <@ List.length alerts = 1 @>
                      test <@ triggered = true @>
                      test <@ Quantity.value triggeredQty = 10.0m @>
                  | Error e -> failwithf "Expected Ok, got Error: %A" e
              })
            ]
