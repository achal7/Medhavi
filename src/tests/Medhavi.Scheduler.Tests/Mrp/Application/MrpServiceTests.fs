namespace Medhavi.Scheduler.Tests.Mrp.Application

open System
open System.Threading.Tasks
open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.Scheduler.Mrp.Domain
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.Errors
open Medhavi.Scheduler.Mrp.Domain.Policies
open Medhavi.Scheduler.Mrp.Domain.Algorithms
open Medhavi.Scheduler.Mrp.Domain.Algorithms.BomExplosion
open Medhavi.Scheduler.Mrp.Domain.MrpRunAggregate
open Medhavi.Scheduler.Mrp.Steps.SupplyGenerationStep
open Medhavi.Scheduler.Mrp.Application
open Medhavi.Scheduler.Mrp.MrpService
open Medhavi.Scheduler.Tests.TestCommon
open Medhavi.Scheduler.Tests.TestCommon.Bdd

module MrpServiceTests =

    let sp = spWarehouse
    let noBom = fun _ _ -> None

    let baseDeps bom =
        { defaultDeps with
            BomLookup = bom
            ProductTypeQuery = fun sku -> task { return if sku = skuFG then Manufactured else Purchased }
            SupplierQuery = fun sku _ -> task { return if sku = skuRM then Some(SupplierId.create "SUP-1" |> getOk) else None }
            RoutingQuery = fun sku _ -> task { return if sku = skuFG then Some(RoutingId.create "ROUTING-FG" |> getOk) else None } }

    let runDryRun srv dem pegs =
        task {
            let! r = srv.ExecuteDryRun "test-run" (createTimestampYmd 2026 6 1) (createTimestampYmd 2026 6 20) sp MrpPolicy.defaults dem pegs
            return r |> getOk
        } |> Async.AwaitTask

    let runExec srv dem =
        task {
            let! r = srv.ExecuteRun "test-run" (createTimestampYmd 2026 6 1) (createTimestampYmd 2026 6 20) sp MrpPolicy.defaults dem []
            return r |> getOk
        } |> Async.AwaitTask

    [<Tests>]
    let tests =
        testList "MRP Application - E2E Integration Tests (MrpService)" [

            testCaseAsync "Scenario: Multi-Level E2E Dry-Run Planning - should plan FG work orders and RM purchase orders" (async {
                let mockBomLookup sku _ =
                    if sku = skuFG then
                        Some { BomId = "bom-bike"
                               ParentSkuId = skuFG
                               Components =
                                 [ { ComponentSkuId = skuRM
                                     QuantityPer = createQty 1m
                                     UnitOfMeasureId = uomPc
                                     Sequence = 1
                                     IsPhantom = false } ]
                               IsActive = true }
                    else None

                let service = create (baseDeps mockBomLookup)
                let demands = [ { defaultDemand skuFG with DemandId = "co-bike-1"; Quantity = createQty 10m; RequiredDate = createTimestampYmd 2026 6 10; Source = CustomerOrder("CO-1", "1") } ]

                let! _ =
                    Given(service, demands)
                    |> WhenAsync(fun (srv, dem) -> runDryRun srv dem [])
                    |> ThenAsync(fun runResult ->
                        test <@ runResult.Status = MrpRunStatus.Completed @>
                        test <@ List.length runResult.Proposals = 2 @>

                        let wo = runResult.Proposals |> List.find (fun p -> p.SkuId = skuFG)
                        test <@ wo.ProposalType = PlannedWorkOrder @>
                        test <@ Quantity.value wo.Quantity = 10m @>
                        test <@ wo.RoutingId = Some(RoutingId.create "ROUTING-FG" |> getOk) @>

                        let po = runResult.Proposals |> List.find (fun p -> p.SkuId = skuRM)
                        test <@ po.ProposalType = PlannedPurchaseOrder @>
                        test <@ Quantity.value po.Quantity = 10m @>
                        test <@ po.SupplierId = Some(SupplierId.create "SUP-1" |> getOk) @>

                        test <@ List.length runResult.Peggings = 1 @>
                    )
                ()
            })

            testCaseAsync "Scenario: MRP Netting - should subtract on-hand inventory and only plan net shortfall" (async {
                let deps = { baseDeps noBom with
                               OnHandQuery = fun _ _ -> Task.FromResult(createQty 5m)
                               SafetyStockQuery = fun _ _ -> Task.FromResult(createQty 2m) }

                let service = create deps
                let demands = [ { defaultDemand skuFG with DemandId = "co-1"; Quantity = createQty 10m; RequiredDate = createTimestampYmd 2026 6 10; Source = CustomerOrder("CO-1", "1") } ]

                let! _ =
                    Given(service, demands)
                    |> WhenAsync(fun (srv, dem) -> runDryRun srv dem [])
                    |> ThenAsync(fun runResult ->
                        test <@ runResult.Status = MrpRunStatus.Completed @>
                        test <@ List.length runResult.Proposals = 1 @>
                        let proposal = List.head runResult.Proposals
                        test <@ Quantity.value proposal.Quantity = 7m @>
                        test <@ proposal.SkuId = skuFG @>
                    )
                ()
            })

            testCaseAsync "Scenario: MRP ExecuteRun - should succeed and invoke CreateSupplyOrders on dependencies" (async {
                let mutable committedProposals = []
                let deps = { baseDeps noBom with
                               CreateSupplyOrders = fun _ proposals -> async { committedProposals <- proposals; return Ok() } }

                let service = create deps
                let demands = [ { defaultDemand skuFG with DemandId = "co-exec-run-1"; Quantity = createQty 10m; RequiredDate = createTimestampYmd 2026 6 10; Source = CustomerOrder("CO-1", "1") } ]

                let! _ =
                    Given(service, demands)
                    |> WhenAsync(fun (srv, dem) -> runExec srv dem)
                    |> ThenAsync(fun runResult ->
                        test <@ runResult.Status = MrpRunStatus.Completed @>
                        test <@ List.length runResult.Proposals = 1 @>
                        test <@ List.length committedProposals = 1 @>
                        test <@ (List.head committedProposals).SkuId = skuFG @>
                    )
                ()
            })

            testCaseAsync "Scenario: MRP ExecuteRun - should fail and return error when CreateSupplyOrders fails" (async {
                let deps = { baseDeps noBom with
                               CreateSupplyOrders = fun _ _ -> async { return Error "DB write failed" } }

                let service = create deps
                let demands = [ { defaultDemand skuFG with DemandId = "co-exec-run-2"; Quantity = createQty 10m; RequiredDate = createTimestampYmd 2026 6 10; Source = CustomerOrder("CO-1", "1") } ]

                let! result =
                    task {
                        let! r = service.ExecuteRun "test-run" (createTimestampYmd 2026 6 1) (createTimestampYmd 2026 6 20) sp MrpPolicy.defaults demands []
                        return r
                    } |> Async.AwaitTask

                match result with
                | Error(MrpApplicationError.UnexpectedError ex) ->
                    test <@ ex.Message.Contains("Failed to persist generated supply orders: DB write failed") @>
                | _ -> failwith "Expected UnexpectedError on persist failure"
            })

            testCaseAsync "Scenario: MRP Netting - should subtract reservations from available stock" (async {
                let deps = { baseDeps noBom with
                               OnHandQuery = fun _ _ -> Task.FromResult(createQty 10m)
                               ReservationsQuery = fun _ _ _ _ -> Task.FromResult([ (createTimestampYmd 2026 6 5, createQty 4m, "res-1") ]) }

                let service = create deps
                let demands = [ { defaultDemand skuFG with DemandId = "co-res-1"; Quantity = createQty 10m; RequiredDate = createTimestampYmd 2026 6 10; Source = CustomerOrder("CO-1", "1") } ]

                let! _ =
                    Given(service, demands)
                    |> WhenAsync(fun (srv, dem) -> runDryRun srv dem [])
                    |> ThenAsync(fun runResult ->
                        test <@ runResult.Status = MrpRunStatus.Completed @>
                        test <@ List.length runResult.Proposals = 1 @>
                        let proposal = List.head runResult.Proposals
                        test <@ Quantity.value proposal.Quantity = 4m @>
                        test <@ proposal.SkuId = skuFG @>
                    )
                ()
            })

            testCaseAsync "Scenario: MRP Netting - should exclude firmed peggings from netting calculations" (async {
                let deps = { baseDeps noBom with
                               OnHandQuery = fun _ _ -> Task.FromResult(createQty 0m)
                               InboundQuery = fun _ _ _ _ -> Task.FromResult([ (createTimestampYmd 2026 6 10, createQty 10m, true, "WO-1") ]) }

                let service = create deps
                let demandObj = { defaultDemand skuFG with DemandId = "co-peg-1"; Quantity = createQty 10m; RequiredDate = createTimestampYmd 2026 6 10; Source = CustomerOrder("CO-1", "1") }

                let demandRef : DemandRef = { DemandId = "co-peg-1"; SkuId = skuFG; NodeId = nodeWarehouse; StockingPointId = sp; NeedDate = createTimestampYmd 2026 6 10; Quantity = createQty 10m }
                let supplyRef : SupplyRef = { SupplyId = "WO-1"; ProposalType = PlannedPurchaseOrder; SkuId = skuFG; NodeId = nodeWarehouse; StockingPointId = sp; DeliveryDate = createTimestampYmd 2026 6 10; Quantity = createQty 10m }

                let firmedPeg : PeggingLink = { defaultPeggingLink demandRef supplyRef with IsLocked = true; Status = PegStatus.Active }

                let! _ =
                    Given(service, [ demandObj ])
                    |> WhenAsync(fun (srv, dem) ->
                        task {
                            let! result = srv.ExecuteDryRun "run-test-peg-1" (createTimestampYmd 2026 6 1) (createTimestampYmd 2026 6 20) sp MrpPolicy.defaults dem [ firmedPeg ]
                            return result |> getOk
                        } |> Async.AwaitTask)
                    |> ThenAsync(fun runResult ->
                        test <@ runResult.Status = MrpRunStatus.Completed @>
                        test <@ List.isEmpty runResult.Proposals @>
                    )
                ()
            })

            testCaseAsync "Scenario: Multi-Site Manufactured Product Fallback to Transfer - should plan Transfer Order from Factory and Work Order at Factory" (async {
                let mockBomLookup sku _ =
                    if sku = skuFG then
                        Some { BomId = "bom-bike"
                               ParentSkuId = skuFG
                               Components =
                                 [ { ComponentSkuId = skuRM
                                     QuantityPer = createQty 1m
                                     UnitOfMeasureId = uomPc
                                     Sequence = 1
                                     IsPhantom = false } ]
                               IsActive = true }: Algorithms.BomExplosion.BomRecord option
                    else None

                let customDeps =
                    { defaultDeps with
                        BomLookup = mockBomLookup
                        ProductTypeQuery = fun sku -> task { return if sku = skuFG then Manufactured else Purchased }
                        SupplierQuery = fun sku spId -> task { return if sku = skuRM && spId = spFactory then Some(SupplierId.create "SUP-1" |> getOk) else None }
                        RoutingQuery = fun sku spId -> task { return if sku = skuFG && spId = spFactory then Some(RoutingId.create "ROUTING-FACTORY" |> getOk) else None }
                        TransferSourceQuery = fun sku spId -> task { return if sku = skuFG && spId = spWarehouse then Some(spFactory) else None } }

                let service = create customDeps
                let demands = [ { defaultDemand skuFG with DemandId = "co-bike-1"; StockingPointId = spWarehouse; NodeId = nodeWarehouse; Quantity = createQty 10m; RequiredDate = createTimestampYmd 2026 6 10; Source = CustomerOrder("CO-1", "1") } ]

                let! _ =
                    Given(service, demands)
                    |> WhenAsync(fun (srv, dem) -> runDryRun srv dem [])
                    |> ThenAsync(fun runResult ->
                        test <@ runResult.Status = MrpRunStatus.Completed @>
                        
                        let transferProp = runResult.Proposals |> List.find (fun p -> p.SkuId = skuFG && p.StockingPointId = spWarehouse)
                        test <@ transferProp.ProposalType = PlannedTransferOrder @>
                        test <@ transferProp.SupplierId = Some(SupplierId.create (StockingPointId.value spFactory) |> getOk) @>
                        test <@ Quantity.value transferProp.Quantity = 10m @>

                        let workProp = runResult.Proposals |> List.find (fun p -> p.SkuId = skuFG && p.StockingPointId = spFactory)
                        test <@ workProp.ProposalType = PlannedWorkOrder @>
                        test <@ workProp.RoutingId = Some(RoutingId.create "ROUTING-FACTORY" |> getOk) @>
                        test <@ Quantity.value workProp.Quantity = 10m @>

                        let purchaseProp = runResult.Proposals |> List.find (fun p -> p.SkuId = skuRM && p.StockingPointId = spFactory)
                        test <@ purchaseProp.ProposalType = PlannedPurchaseOrder @>
                        test <@ purchaseProp.SupplierId = Some(SupplierId.create "SUP-1" |> getOk) @>
                        test <@ Quantity.value purchaseProp.Quantity = 10m @>
                    )
                ()
            })

            testCaseAsync "Scenario: Capacity-Aware MRP - should place WO on due date when there is enough capacity" (async {
                let rgId = ResourceGroupId.create "RG-FG" |> getOk
                let routingId = RoutingId.create "ROUTING-FG" |> getOk

                let deps = 
                    { baseDeps noBom with
                        CapacityPromiseQuery = fun _ desiredBucket _ _ -> Task.FromResult({ EarliestFeasibleBucket = desiredBucket; IsFeasible = true })
                        CapacityRoutingQuery = fun _ _ _ _ -> Task.FromResult(Some { RoutingId = routingId; ResourceGroupId = rgId; NeededDuration = DurationMinutes.zero }) }

                let service = create deps
                let demands = [ { defaultDemand skuFG with DemandId = "co-cap-1"; Quantity = createQty 10m; RequiredDate = createTimestampYmd 2026 6 10; Source = CustomerOrder("CO-1", "1") } ]
                let policy = { MrpPolicy.defaults with CapacityPolicy = CapacityPolicy.finiteCapacity }

                let! _ =
                    Given(service, demands)
                    |> WhenAsync(fun (srv, dem) ->
                        task {
                            let! r = srv.ExecuteDryRun "test-run" (createTimestampYmd 2026 6 1) (createTimestampYmd 2026 6 20) sp policy dem []
                            return r |> getOk
                        } |> Async.AwaitTask)
                    |> ThenAsync(fun runResult ->
                        test <@ runResult.Status = MrpRunStatus.Completed @>
                        test <@ List.length runResult.Proposals = 1 @>
                        let proposal = List.head runResult.Proposals
                        test <@ proposal.SkuId = skuFG @>
                        test <@ proposal.DueDate = createTimestampYmd 2026 6 10 @>
                        test <@ proposal.CapacityCheckedDate = Some(createTimestampYmd 2026 6 10) @>
                    )
                ()
            })

            testCaseAsync "Scenario: Capacity-Aware MRP - should fallback to alternate routing when primary resource is overloaded" (async {
                let primaryRg = ResourceGroupId.create "RG-FG-PRIMARY" |> getOk
                let altRg = ResourceGroupId.create "RG-FG-ALT" |> getOk
                let primaryRouting = RoutingId.create "ROUTING-FG-PRIMARY" |> getOk
                let altRouting = RoutingId.create "ROUTING-FG-ALT" |> getOk

                let deps = 
                    { defaultDeps with
                        ProductTypeQuery = fun _ -> Task.FromResult(Manufactured)
                        RoutingQuery = fun _ _ -> Task.FromResult(Some primaryRouting)
                        AlternateRoutingsQuery = fun _ _ -> Task.FromResult([ primaryRouting; altRouting ])
                        CapacityRoutingQuery = fun sku spId rIdOpt qty -> task {
                            match rIdOpt with
                            | Some rId when rId = altRouting -> 
                                return Some { RoutingId = altRouting; ResourceGroupId = altRg; NeededDuration = DurationMinutes.zero }
                            | _ -> 
                                return Some { RoutingId = primaryRouting; ResourceGroupId = primaryRg; NeededDuration = DurationMinutes.zero }
                        }
                        CapacityPromiseQuery = fun rgId desiredBucket _ _ -> task {
                            if rgId = primaryRg then
                                return { EarliestFeasibleBucket = desiredBucket + 5; IsFeasible = false }
                            else
                                return { EarliestFeasibleBucket = desiredBucket; IsFeasible = true }
                        } }

                let service = create deps
                let demands = [ { defaultDemand skuFG with DemandId = "co-cap-2"; Quantity = createQty 10m; RequiredDate = createTimestampYmd 2026 6 10; Source = CustomerOrder("CO-1", "1") } ]
                let policy = { MrpPolicy.defaults with CapacityPolicy = CapacityPolicy.finiteCapacity }

                let! _ =
                    Given(service, demands)
                    |> WhenAsync(fun (srv, dem) ->
                        task {
                            let! r = srv.ExecuteDryRun "test-run" (createTimestampYmd 2026 6 1) (createTimestampYmd 2026 6 20) sp policy dem []
                            return r |> getOk
                        } |> Async.AwaitTask)
                    |> ThenAsync(fun runResult ->
                        test <@ runResult.Status = MrpRunStatus.Completed @>
                        test <@ List.length runResult.Proposals = 1 @>
                        let proposal = List.head runResult.Proposals
                        test <@ proposal.SkuId = skuFG @>
                        test <@ proposal.RoutingId = Some altRouting @>
                        test <@ proposal.DueDate = createTimestampYmd 2026 6 10 @>
                        test <@ proposal.CapacityCheckedDate = Some(createTimestampYmd 2026 6 10) @>
                    )
                ()
            })

            testCaseAsync "Scenario: Capacity-Aware MRP - should shift due date forward when both primary and alternate lack capacity" (async {
                let primaryRg = ResourceGroupId.create "RG-FG-PRIMARY" |> getOk
                let primaryRouting = RoutingId.create "ROUTING-FG-PRIMARY" |> getOk

                let deps = 
                    { defaultDeps with
                        ProductTypeQuery = fun _ -> Task.FromResult(Manufactured)
                        RoutingQuery = fun _ _ -> Task.FromResult(Some primaryRouting)
                        AlternateRoutingsQuery = fun _ _ -> Task.FromResult([ primaryRouting ])
                        CapacityRoutingQuery = fun _ _ _ _ -> Task.FromResult(Some { RoutingId = primaryRouting; ResourceGroupId = primaryRg; NeededDuration = DurationMinutes.zero })
                        CapacityPromiseQuery = fun rgId desiredBucket _ _ -> task {
                            return { EarliestFeasibleBucket = desiredBucket + 2; IsFeasible = false }
                        } }

                let service = create deps
                let demands = [ { defaultDemand skuFG with DemandId = "co-cap-3"; Quantity = createQty 10m; RequiredDate = createTimestampYmd 2026 6 10; Source = CustomerOrder("CO-1", "1") } ]
                let policy = { MrpPolicy.defaults with CapacityPolicy = CapacityPolicy.finiteCapacity }

                let! _ =
                    Given(service, demands)
                    |> WhenAsync(fun (srv, dem) ->
                        task {
                            let! r = srv.ExecuteDryRun "test-run" (createTimestampYmd 2026 6 1) (createTimestampYmd 2026 6 20) sp policy dem []
                            return r |> getOk
                        } |> Async.AwaitTask)
                    |> ThenAsync(fun runResult ->
                        test <@ runResult.Status = MrpRunStatus.Completed @>
                        test <@ List.length runResult.Proposals = 1 @>
                        let proposal = List.head runResult.Proposals
                        test <@ proposal.SkuId = skuFG @>
                        test <@ proposal.DueDate = createTimestampYmd 2026 6 12 @> // Shifted by 2 days
                        test <@ proposal.CapacityCheckedDate = Some(createTimestampYmd 2026 6 12) @>
                    )
                ()
            })

            testCaseAsync "Scenario: Capacity-Aware MRP - should fail with CapacityInfeasible error when shifted date exceeds horizon" (async {
                let primaryRg = ResourceGroupId.create "RG-FG-PRIMARY" |> getOk
                let primaryRouting = RoutingId.create "ROUTING-FG-PRIMARY" |> getOk

                let deps = 
                    { defaultDeps with
                        ProductTypeQuery = fun _ -> Task.FromResult(Manufactured)
                        RoutingQuery = fun _ _ -> Task.FromResult(Some primaryRouting)
                        AlternateRoutingsQuery = fun _ _ -> Task.FromResult([ primaryRouting ])
                        CapacityRoutingQuery = fun _ _ _ _ -> Task.FromResult(Some { RoutingId = primaryRouting; ResourceGroupId = primaryRg; NeededDuration = DurationMinutes.zero })
                        CapacityPromiseQuery = fun rgId desiredBucket _ _ -> task {
                            return { EarliestFeasibleBucket = desiredBucket + 50; IsFeasible = false } // Shift beyond the June 20th horizon
                        } }

                let service = create deps
                let demands = [ { defaultDemand skuFG with DemandId = "co-cap-4"; Quantity = createQty 10m; RequiredDate = createTimestampYmd 2026 6 10; Source = CustomerOrder("CO-1", "1") } ]
                let policy = { MrpPolicy.defaults with CapacityPolicy = CapacityPolicy.finiteCapacity }

                let! result =
                    task {
                        let! r = service.ExecuteDryRun "test-run-fail" (createTimestampYmd 2026 6 1) (createTimestampYmd 2026 6 20) sp policy demands []
                        return r
                    } |> Async.AwaitTask

                match result with
                | Ok runResult ->
                    test <@ List.isEmpty runResult.Proposals @>
                    test <@ runResult.Warnings |> List.exists (fun w -> w.Contains("Capacity infeasible for SKU SKU-FG")) @>
                | _ -> failwith "Expected Ok runResult with warnings"
                ()
            })
        ]
