namespace Medhavi.Scheduler.Tests

open System
open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Steps

module PeggingTests =

    let getOk =
        function
        | Ok x -> x
        | Error e -> failwithf "Expected Ok, got Error: %A" e

    let skuId = SkuId.create "sku-1" |> getOk
    let nodeId = NodeId.create "node-1" |> getOk
    let spId = StockingPointId.create "sp-1" |> getOk

    let createQty v =
        Quantity.create v |> getOk

    let createTimestamp y m d =
        Timestamp.create (DateTimeOffset(y, m, d, 0, 0, 0, TimeSpan.Zero))

    [<Tests>]
    let tests =
        testList "MRP Pegging & Traceability Tests" [
            
            testCase "Traceability queries should filter bi-directionally" (fun () ->
                let demand1: DemandRef =
                    { DemandId = "dem-1"; SkuId = skuId; NodeId = nodeId; StockingPointId = spId; NeedDate = createTimestamp 2026 6 1; Quantity = createQty 10m }
                let demand2: DemandRef =
                    { DemandId = "dem-2"; SkuId = skuId; NodeId = nodeId; StockingPointId = spId; NeedDate = createTimestamp 2026 6 2; Quantity = createQty 20m }
                
                let supply1: SupplyRef =
                    { SupplyId = "sup-1"; ProposalType = PlannedPurchaseOrder; SkuId = skuId; NodeId = nodeId; StockingPointId = spId; DeliveryDate = createTimestamp 2026 6 1; Quantity = createQty 30m }

                let peg1 =
                    { Id = PeggingId.createDeterministic "dem-1" "sup-1"
                      Demand = demand1
                      Target = PegTarget.Supply supply1
                      PeggedQty = createQty 10m
                      Status = PegStatus.Active
                      IsLocked = false
                      Created = DateTimeOffset.UtcNow
                      Modified = DateTimeOffset.UtcNow }

                let peg2 =
                    { Id = PeggingId.createDeterministic "dem-2" "sup-1"
                      Demand = demand2
                      Target = PegTarget.Supply supply1
                      PeggedQty = createQty 20m
                      Status = PegStatus.Active
                      IsLocked = false
                      Created = DateTimeOffset.UtcNow
                      Modified = DateTimeOffset.UtcNow }

                let peggings = [ peg1; peg2 ]

                // Get upstream supplies for demand 1
                let upstream = Traceability.getUpstreamSupplies "dem-1" peggings
                test <@ List.length upstream = 1 @>
                match List.head upstream with
                | Supply s -> test <@ s.SupplyId = "sup-1" @>
                | _ -> failwith "Expected supply"

                // Get downstream demands for supply 1
                let downstream = Traceability.getDownstreamDemands "sup-1" peggings
                test <@ List.length downstream = 2 @>
                test <@ (List.head downstream).DemandId = "dem-1" @>
                test <@ (List.last downstream).DemandId = "dem-2" @>
            )

            testCase "PeggingEngine should allocate in FIFO order" (fun () ->
                // FIFO: Sort demands by date (ascending)
                let demand1 =
                    { DemandId = "dem-1"; SkuId = skuId; NodeId = nodeId; StockingPointId = spId; Quantity = createQty 10m; RequiredDate = createTimestamp 2026 6 2; Source = DemandSource.Manual "test"; Priority = None }
                let demand2 =
                    { DemandId = "dem-2"; SkuId = skuId; NodeId = nodeId; StockingPointId = spId; Quantity = createQty 15m; RequiredDate = createTimestamp 2026 6 1; Source = DemandSource.Manual "test"; Priority = None }
                
                let proposal1 =
                    { Id = SupplyProposalId.create "prop-1" |> getOk
                      ProposalType = PlannedPurchaseOrder; SkuId = skuId; NodeId = nodeId; StockingPointId = spId; Quantity = createQty 25m; DueDate = createTimestamp 2026 6 1; StartDate = None; RoutingId = None; SupplierId = None; Priority = 1; IsExpedite = false; Status = Planned; PeggingRefs = []; CapacityCheckedDate = None; CreatedAt = Timestamp.now }

                let policy = { PeggingPolicy.defaultPolicy with AllocationOrder = AllocationOrderPolicy.FIFO }
                let peggings = PeggingEngine.pegSuppliesToDemands policy [ demand1; demand2 ] [ proposal1 ]
                
                // dem-2 (RequiredDate 6/1) is earlier than dem-1 (RequiredDate 6/2). In FIFO, dem-2 is satisfied first.
                test <@ List.length peggings = 2 @>
                
                // Peg for dem-2
                let pegForD2 = peggings |> List.find (fun p -> p.Demand.DemandId = "dem-2")
                test <@ Quantity.value pegForD2.PeggedQty = 15m @>
                
                // Peg for dem-1
                let pegForD1 = peggings |> List.find (fun p -> p.Demand.DemandId = "dem-1")
                test <@ Quantity.value pegForD1.PeggedQty = 10m @>
            )

            testCase "PeggingEngine should allocate in Priority order" (fun () ->
                // Priority: High priority first, then Date
                let demand1 =
                    { DemandId = "dem-1"; SkuId = skuId; NodeId = nodeId; StockingPointId = spId; Quantity = createQty 10m; RequiredDate = createTimestamp 2026 6 1; Source = DemandSource.Manual "test"; Priority = Some 5 }
                let demand2 =
                    { DemandId = "dem-2"; SkuId = skuId; NodeId = nodeId; StockingPointId = spId; Quantity = createQty 15m; RequiredDate = createTimestamp 2026 6 2; Source = DemandSource.Manual "test"; Priority = Some 10 } // Higher priority
                
                let proposal1 =
                    { Id = SupplyProposalId.create "prop-1" |> getOk
                      ProposalType = PlannedPurchaseOrder; SkuId = skuId; NodeId = nodeId; StockingPointId = spId; Quantity = createQty 25m; DueDate = createTimestamp 2026 6 1; StartDate = None; RoutingId = None; SupplierId = None; Priority = 1; IsExpedite = false; Status = Planned; PeggingRefs = []; CapacityCheckedDate = None; CreatedAt = Timestamp.now }

                let policy = { PeggingPolicy.defaultPolicy with AllocationOrder = AllocationOrderPolicy.Priority }
                let peggings = PeggingEngine.pegSuppliesToDemands policy [ demand1; demand2 ] [ proposal1 ]
                
                test <@ List.length peggings = 2 @>
                
                // dem-2 (Priority 10) is allocated first, getting 15m
                let pegForD2 = peggings |> List.find (fun p -> p.Demand.DemandId = "dem-2")
                test <@ Quantity.value pegForD2.PeggedQty = 15m @>

                // dem-1 (Priority 5) gets the remaining 10m
                let pegForD1 = peggings |> List.find (fun p -> p.Demand.DemandId = "dem-1")
                test <@ Quantity.value pegForD1.PeggedQty = 10m @>
            )

            testCase "Pre-netting step should adjust demands and inbound quantities based on firmed pegs" (fun () ->
                let demand1 =
                    { DemandId = "dem-1"; SkuId = skuId; NodeId = nodeId; StockingPointId = spId; Quantity = createQty 20m; RequiredDate = createTimestamp 2026 6 1; Source = DemandSource.Manual "test"; Priority = None }
                let demand2 =
                    { DemandId = "dem-2"; SkuId = skuId; NodeId = nodeId; StockingPointId = spId; Quantity = createQty 10m; RequiredDate = createTimestamp 2026 6 2; Source = DemandSource.Manual "test"; Priority = None }

                let inbound = [ (createTimestamp 2026 6 1, createQty 30m, true, "sup-1") ]
                let reservations = [ (createTimestamp 2026 6 1, createQty 5m, "res-1") ]

                // Firmed peg: demand 1 pegged to supply 1 for 15m
                let demand1Ref: DemandRef =
                    { DemandId = "dem-1"; SkuId = skuId; NodeId = nodeId; StockingPointId = spId; NeedDate = createTimestamp 2026 6 1; Quantity = createQty 20m }
                let supply1Ref: SupplyRef =
                    { SupplyId = "sup-1"; ProposalType = PlannedPurchaseOrder; SkuId = skuId; NodeId = nodeId; StockingPointId = spId; DeliveryDate = createTimestamp 2026 6 1; Quantity = createQty 30m }

                let firmedPeg =
                    { Id = PeggingId.createDeterministic "dem-1" "sup-1"
                      Demand = demand1Ref
                      Target = PegTarget.Supply supply1Ref
                      PeggedQty = createQty 15m
                      Status = PegStatus.Active
                      IsLocked = true // LOCKED/FIRMED
                      Created = DateTimeOffset.UtcNow
                      Modified = DateTimeOffset.UtcNow }

                let adjustedDemands, adjustedInbound, adjustedReservations =
                    NettingStep.adjustForFirmedPegs skuId spId [ demand1; demand2 ] inbound reservations [ firmedPeg ]

                // demand 1 was 20m, pegged 15m -> remaining adjusted demand should be 5m
                let adjD1 = adjustedDemands |> List.find (fun d -> d.DemandId = "dem-1")
                test <@ Quantity.value adjD1.Quantity = 5m @>

                // demand 2 had no peg -> remaining should be 10m
                let adjD2 = adjustedDemands |> List.find (fun d -> d.DemandId = "dem-2")
                test <@ Quantity.value adjD2.Quantity = 10m @>

                // inbound supply 1 was 30m, pegged 15m -> remaining adjusted supply should be 15m
                test <@ List.length adjustedInbound = 1 @>
                let (_, adjSQty, _, adjSId) = List.head adjustedInbound
                test <@ adjSId = "sup-1" @>
                test <@ Quantity.value adjSQty = 15m @>

                // reservation had no peg -> remaining should be 5m
                test <@ List.length adjustedReservations = 1 @>
                let (_, adjRQty, adjRId) = List.head adjustedReservations
                test <@ adjRId = "res-1" @>
                test <@ Quantity.value adjRQty = 5m @>
            )
        ]
