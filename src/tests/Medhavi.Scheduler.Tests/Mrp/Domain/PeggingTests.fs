namespace Medhavi.Scheduler.Tests.Mrp.Domain

open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Steps
open Medhavi.Scheduler.Tests.TestCommon

module PeggingTests =

    [<Tests>]
    let tests =
        testList "MRP Domain - Pegging & Traceability Tests" [
            
            testCase "Scenario: Traceability Queries - should filter bi-directionally" (fun () ->
                let demand1 = { defaultDemandRef skuFG with DemandId = "dem-1"; NeedDate = createTimestampYmd 2026 6 1; Quantity = createQty 10m }
                let demand2 = { defaultDemandRef skuFG with DemandId = "dem-2"; NeedDate = createTimestampYmd 2026 6 2; Quantity = createQty 20m }
                
                let supply1 = { defaultSupplyRef skuFG with SupplyId = "sup-1"; DeliveryDate = createTimestampYmd 2026 6 1; Quantity = createQty 30m }

                let peg1 = { defaultPeggingLink demand1 supply1 with PeggedQty = createQty 10m }
                let peg2 = { defaultPeggingLink demand2 supply1 with PeggedQty = createQty 20m }
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

            testCase "Scenario: Pegging Engine Allocations - should allocate in FIFO order" (fun () ->
                // FIFO: Sort demands by date (ascending)
                let demand1 = { defaultDemand skuFG with DemandId = "dem-1"; Quantity = createQty 10m; RequiredDate = createTimestampYmd 2026 6 2 }
                let demand2 = { defaultDemand skuFG with DemandId = "dem-2"; Quantity = createQty 15m; RequiredDate = createTimestampYmd 2026 6 1 }
                
                let proposal1 = { defaultProposal skuFG with Id = SupplyProposalId.create "prop-1" |> getOk; Quantity = createQty 25m; DueDate = createTimestampYmd 2026 6 1 }

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

            testCase "Scenario: Pegging Engine Allocations - should allocate in Priority order" (fun () ->
                // Priority: High priority first, then Date
                let demand1 = { defaultDemand skuFG with DemandId = "dem-1"; Quantity = createQty 10m; RequiredDate = createTimestampYmd 2026 6 1; Priority = Some 5 }
                let demand2 = { defaultDemand skuFG with DemandId = "dem-2"; Quantity = createQty 15m; RequiredDate = createTimestampYmd 2026 6 2; Priority = Some 10 } // Higher priority
                
                let proposal1 = { defaultProposal skuFG with Id = SupplyProposalId.create "prop-1" |> getOk; Quantity = createQty 25m; DueDate = createTimestampYmd 2026 6 1 }

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
        ]
