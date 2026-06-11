namespace Medhavi.Scheduler.Tests.Mrp.Domain.Algorithms

open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.Errors
open Medhavi.Scheduler.Mrp.Domain.Policies
open Medhavi.Scheduler.Mrp.Domain.Algorithms
open Medhavi.Scheduler.Mrp.Domain.Algorithms.BomExplosion
open Medhavi.Scheduler.Tests.TestCommon

module BomExplosionTests =

    [<Tests>]
    let tests =
        testList
            "MRP Domain - BOM Explosion Tests"
            [

              testCase
                  "Scenario: Single-Level BOM Explosion - should include parent assembly at level-0 and component at level-1"
                  (fun () ->

                      let demand = defaultDemand skuSub

                      let result =
                          BomExplosion.explode mockBomLookup DefaultBom demand
                          |> getOk

                      // Result must contain BOTH the parent (level 0) and the child component (level 1)
                      test <@ List.length result = 2 @>

                      let parentOpt = result |> List.tryFind (fun c -> c.SkuId = skuSub)
                      test <@ Option.isSome parentOpt @>
                      let parent = Option.get parentOpt
                      test <@ parent.BomLevel = 0 @>
                      test <@ parent.RequiredQuantity = createQty 10m @>

                      let childOpt = result |> List.tryFind (fun c -> c.SkuId = skuRM)
                      test <@ Option.isSome childOpt @>
                      let child = Option.get childOpt
                      test <@ child.BomLevel = 1 @>
                      test <@ child.RequiredQuantity = createQty 40m @>) // 10 * 4 = 40

              testCase
                  "Scenario: Multi-Level BOM Explosion - should explode all nested levels with scaled quantities"
                  (fun () ->
                      let demand = defaultDemand skuFG

                      let result =
                          BomExplosion.explode mockBomLookup DefaultBom demand
                          |> getOk

                      test <@ List.length result = 3 @> // Parent (level 0), Sub (level 1), RM (level 2)

                      let fg = result |> List.find (fun c -> c.SkuId = skuFG)
                      test <@ fg.BomLevel = 0 @>
                      test <@ fg.RequiredQuantity = createQty 10m @>

                      let sub = result |> List.find (fun c -> c.SkuId = skuSub)
                      test <@ sub.BomLevel = 1 @>
                      test <@ sub.RequiredQuantity = createQty 20m @> // 10 * 2 = 20

                      let rm = result |> List.find (fun c -> c.SkuId = skuRM)
                      test <@ rm.BomLevel = 2 @>
                      test <@ rm.RequiredQuantity = createQty 80m @>) // 10 * 4 = 40

              testCase
                  "Scenario: Phantom Component Pass-Through - should bypass phantom but explode children"
                  (fun () ->
                      let skuPhantom = SkuId.create "sku-phan-3" |> getOk

                      let mockBomLookup sku _ =
                          if sku = skuFG then
                              Some
                                  { BomId = "bom-fg-3"
                                    ParentSkuId = skuFG
                                    Components =
                                      [ { ComponentSkuId = skuPhantom
                                          QuantityPer = createQty 1m
                                          UnitOfMeasureId = uomPc
                                          Sequence = 1
                                          IsPhantom = true } ]
                                    IsActive = true }
                          elif sku = skuPhantom then
                              Some
                                  { BomId = "bom-phan-3"
                                    ParentSkuId = skuPhantom
                                    Components =
                                      [ { ComponentSkuId = skuRM
                                          QuantityPer = createQty 5m
                                          UnitOfMeasureId = uomPc
                                          Sequence = 1
                                          IsPhantom = false } ]
                                    IsActive = true }
                          else
                              None

                      let demand =
                          { DemandId = "dem-fg-3"
                            SkuId = skuFG
                            NodeId = nodeWarehouse
                            StockingPointId = spWarehouse
                            Quantity = createQty 10m
                            RequiredDate = Timestamp.now
                            Source = Manual "test"
                            Priority = None }

                      let result =
                          BomExplosion.explode mockBomLookup DefaultBom demand
                          |> getOk

                      // Result should contain FG (level 0) and RM (level 2 via phantom), but NOT the phantom component itself
                      test <@ List.length result = 2 @>

                      test
                          <@
                              not (
                                  result
                                  |> List.exists (fun c -> c.SkuId = skuPhantom)
                              )
                          @>

                      let rm = result |> List.find (fun c -> c.SkuId = skuRM)
                      test <@ rm.RequiredQuantity = createQty 50m @>) // 10 * 1 * 5 = 50

              testCase "Scenario: Cyclic Dependency Detection - should throw CycleDetected error" (fun () ->
                  let skuA = SkuId.create "sku-a" |> getOk
                  let skuB = SkuId.create "sku-b" |> getOk

                  // Cyclic BOM: A -> B -> A
                  let mockBomLookup sku _ =
                      if sku = skuA then
                          Some
                              { BomId = "bom-a"
                                ParentSkuId = skuA
                                Components =
                                  [ { ComponentSkuId = skuB
                                      QuantityPer = createQty 1m
                                      UnitOfMeasureId = uomPc
                                      Sequence = 1
                                      IsPhantom = false } ]
                                IsActive = true }
                      elif sku = skuB then
                          Some
                              { BomId = "bom-b"
                                ParentSkuId = skuB
                                Components =
                                  [ { ComponentSkuId = skuA
                                      QuantityPer = createQty 1m
                                      UnitOfMeasureId = uomPc
                                      Sequence = 1
                                      IsPhantom = false } ]
                                IsActive = true }
                      else
                          None

                  let demand =
                      { DemandId = "dem-a"
                        SkuId = skuA
                        NodeId = nodeWarehouse
                        StockingPointId = spWarehouse
                        Quantity = createQty 10m
                        RequiredDate = Timestamp.now
                        Source = Manual "test"
                        Priority = None }

                  let result = BomExplosion.explode mockBomLookup DefaultBom demand

                  match result with
                  | Error(CycleDetected path) ->
                      test <@ List.head path = "sku-a" @>
                      test <@ List.contains "sku-b" path @>
                  | _ -> failwith "Expected cycle detection error")

              testCase "Scenario: Inactive BOM Handling - should raise BomNotActive error" (fun () ->
                  let skuFG = SkuId.create "sku-fg-inactive" |> getOk

                  let mockBomLookup sku _ =
                      if sku = skuFG then
                          Some
                              { BomId = "bom-fg-inactive"
                                ParentSkuId = skuFG
                                Components = []
                                IsActive = false }
                      else
                          None

                  let demand =
                      { DemandId = "dem-inactive"
                        SkuId = skuFG
                        NodeId = nodeWarehouse
                        StockingPointId = spWarehouse
                        Quantity = createQty 10m
                        RequiredDate = Timestamp.now
                        Source = Manual "test"
                        Priority = None }

                  let result = BomExplosion.explode mockBomLookup DefaultBom demand

                  match result with
                  | Error(BomNotActive msg) -> test <@ msg = "sku-fg-inactive" @>
                  | _ -> failwith "Expected BomNotActive error") ]
