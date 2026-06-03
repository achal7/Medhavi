namespace Medhavi.Scheduler.Tests

open System
open System.Threading.Tasks
open Expecto
open Swensen.Unquote
open Medhavi.Common.Patterns
open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.Errors
open Medhavi.Scheduler.Mrp.Domain.Policies
open Medhavi.Scheduler.Mrp.Domain.MrpRunAggregate
open Medhavi.Scheduler.Mrp.Domain.Algorithms
open Medhavi.Scheduler.Mrp.Domain.Algorithms.BomExplosion
open Medhavi.Scheduler.Mrp.Pipeline
open Medhavi.Scheduler.Mrp.Steps

module NettingTests =

    let getOk =
        function
        | Ok x -> x
        | Error e -> failwithf "Expected Ok, got Error: %A" e

    // Basic legacy test cases
    let runLegacyNetting sku onhand demandQty safetyStock =
        let totalRequirement = demandQty + safetyStock
        totalRequirement - onhand

    [<Tests>]
    let tests =
        testList
            "MRP Engine Tests"
            [ testCase "should calculate correct shortage when demand exceeds stock (legacy)" (fun () ->
                  let sku = SkuId.create "sku-abc" |> getOk

                  let createQty v =
                      Quantity.create v
                      |> Result.defaultWith (fun e -> failwith e.Message)

                  let onhand = createQty 10.0m
                  let demandQty = createQty 25.0m
                  let safetyStock = createQty 5.0m
                  let shortfall = runLegacyNetting sku onhand demandQty safetyStock
                  test <@ Quantity.value shortfall = 20.0m @>)

              testCase "should calculate zero shortage when supply is sufficient (legacy)" (fun () ->
                  let sku = SkuId.create "sku-abc" |> getOk

                  let createQty v =
                      Quantity.create v
                      |> Result.defaultWith (fun e -> failwith e.Message)

                  let onhand = createQty 50.0m
                  let demandQty = createQty 25.0m
                  let safetyStock = createQty 5.0m
                  let shortfall = runLegacyNetting sku onhand demandQty safetyStock
                  test <@ Quantity.value shortfall = 0.0m @>)

              // 1. BOM Explosion & Cycle Detection
              testCase "BOM explosion should catch cyclic relationships and raise error" (fun () ->
                  let skuA = SkuId.create "sku-a" |> getOk
                  let skuB = SkuId.create "sku-b" |> getOk

                  // Cyclic BOM: A -> B -> A
                  let mockBomLookup sku _ =
                      if SkuId.value sku = "sku-a" then
                          Some
                              { BomId = "bom-a"
                                ParentSkuId = skuA
                                Components =
                                  [ { ComponentSkuId = skuB
                                      QuantityPer = Quantity.create 1m |> getOk
                                      UnitOfMeasureId = UomId.create "pc" |> getOk
                                      Sequence = 1
                                      IsPhantom = false } ]
                                IsActive = true }
                      elif SkuId.value sku = "sku-b" then
                          Some
                              { BomId = "bom-b"
                                ParentSkuId = skuB
                                Components =
                                  [ { ComponentSkuId = skuA
                                      QuantityPer = Quantity.create 1m |> getOk
                                      UnitOfMeasureId = UomId.create "pc" |> getOk
                                      Sequence = 1
                                      IsPhantom = false } ]
                                IsActive = true }
                      else
                          None

                  let demand =
                      { DemandId = "dem-1"
                        SkuId = skuA
                        NodeId = NodeId.create "node-1" |> getOk
                        StockingPointId = StockingPointId.create "sp-1" |> getOk
                        Quantity = Quantity.create 10m |> getOk
                        RequiredDate = Timestamp.now
                        Source = Manual "test"
                        Priority = None }

                  let result = BomExplosion.explode mockBomLookup DefaultBom demand

                  match result with
                  | Error(CycleDetected path) -> test <@ List.head path = "sku-a" @>
                  | _ -> failwith "Expected cycle detection error")

              // 2. Forecast Consumption
              testCase "Forecast consumption should consume forecast inside window" (fun () ->
                  let sku = SkuId.create "sku-1" |> getOk
                  let node = NodeId.create "node-1" |> getOk
                  let fcId = "fc-1"

                  let forecast =
                      { ForecastId = fcId
                        SkuId = sku
                        NodeId = node
                        Quantity = Quantity.create 100m |> getOk
                        PeriodStart = DateTimeOffset.UtcNow
                        PeriodEnd = DateTimeOffset.UtcNow.AddDays(1.0) }

                  let order =
                      { OrderId = OrderId.create "ord-1" |> getOk
                        LineId = "1"
                        SkuId = sku
                        NodeId = node
                        Quantity = Quantity.create 40m |> getOk
                        DueDate = DateTimeOffset.UtcNow.AddHours(2.0)
                        Priority = 1
                        IsExpedited = false }

                  let policy =
                      { ForecastConsumptionPolicy.Enabled = true
                        ConsumptionWindow = TimeSpan.FromDays(2.0)
                        Strategy = BidirectionalConsumption }

                  let consumed = ForecastConsumption.consumeForecasts policy [ forecast ] [ order ]
                  test <@ List.length consumed = 1 @>
                  test <@ Quantity.value (List.head consumed).Quantity = 60m @>)

              // 3. Lot Sizing (Economic Order Quantity)
              testCase "Lot sizing should calculate EOQ correctly" (fun () ->
                  // annualDemand = 1000, orderingCost = 20, holdingCost = 4
                  // EOQ = sqrt(2 * 1000 * 20 / 4) = sqrt(10000) = 100
                  let annualDemand = Quantity.clampToZero 1000m
                  let orderingCost = PositiveDecimal.createSafe 20m
                  let holdingCost = PositiveDecimal.createSafe 4m
                  let eoqQty = LotSizing.eoq annualDemand orderingCost holdingCost
                  test <@ Quantity.value eoqQty = 100m @>)

              // 4. Capacity CTP checks & alternate routing fallback
              testCaseAsync
                  "Capacity CTP check should fall back onto alternate routing on overload"
                  (async {
                      let spId = StockingPointId.create "sp-1" |> getOk
                      let sku = SkuId.create "sku-x" |> getOk
                      let primaryRouting = RoutingId.create "routing-primary" |> getOk
                      let altRouting = RoutingId.create "routing-alt" |> getOk
                      let targetDate = Timestamp.now
                      let delayedDate = Timestamp.create (DateTimeOffset.UtcNow.AddDays(5.0))

                      // Mock capacity query: primary routing has delay (returns delayedDate), alternate has capacity (returns targetDate)
                      let mockCapacityQuery _ _ routingOpt _ _ _ =
                          task {
                              match routingOpt with
                              | Some rid when RoutingId.value rid = "routing-primary" -> return Ok delayedDate
                              | Some rid when RoutingId.value rid = "routing-alt" -> return Ok targetDate
                              | _ -> return Ok targetDate
                          }

                      // Mock alternate routing lookup
                      let mockAlternateRoutingsQuery _ _ = task { return [ primaryRouting; altRouting ] }

                      let proposal =
                          { Id = SupplyProposalId.create "prop-1" |> getOk
                            ProposalType = PlannedWorkOrder
                            SkuId = sku
                            NodeId = NodeId.create "node-1" |> getOk
                            StockingPointId = spId
                            Quantity = Quantity.clampToZero 10m
                            DueDate = targetDate
                            StartDate = None
                            RoutingId = Some primaryRouting
                            SupplierId = None
                            Priority = 5
                            IsExpedite = false
                            Status = Planned
                            PeggingRefs = []
                            CapacityCheckedDate = None
                            CreatedAt = Timestamp.now }

                      let policy =
                          { MrpPolicy.defaults with
                              CapacityPolicy =
                                  { Finite = true
                                    SafetyBuffer = None
                                    ReliabilityFactor = None
                                    MaxAlternateAttempts = 2
                                    BottleneckProtection = None } }

                      let ctx =
                          MrpContext.create (MrpRunId.create "run-1" |> getOk) targetDate targetDate spId policy

                      let step = CapacityCheckStep.createStep mockCapacityQuery mockAlternateRoutingsQuery
                      let! result = step [ proposal ] ctx |> Async.AwaitTask

                      match result with
                      | Ok(updatedProposals, _) ->
                          test <@ List.length updatedProposals = 1 @>
                          let p = List.head updatedProposals
                          // Should fall back to altRouting and remain on time (targetDate)
                          test <@ p.RoutingId = Some altRouting @>
                          test <@ p.DueDate = targetDate @>
                      | Error e -> failwithf "Expected capacity check to succeed, got %A" e
                  }) ]
