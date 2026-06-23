namespace Medhavi.Scheduler.Mrp.Application

open System
open System.Threading.Tasks
open Medhavi.Contracts.Scenario
open Medhavi.SharedKernel
open Medhavi.Scheduler.Planning.Domain
open Medhavi.Scheduler.Planning.Application
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.Policies
open Medhavi.Scheduler.Mrp.Domain.MrpRunAggregate
open Medhavi.Scheduler.Mrp.Steps
open Medhavi.Scheduler.Mrp.Pipeline
open Medhavi.Scheduler.Mrp.Domain.Algorithms

module MrpSolverStrategy =

    let solve: SolvePlan =
        fun scenarioId mode snapshot horizon ->
            async {
                // 1. Build queries reading from in-memory snapshot lists
                let onHandQuery (skuId: SkuId) (spId: StockingPointId) : Task<Quantity> =
                    task {
                        let qty =
                            snapshot.SupplyBuckets
                            |> List.filter (fun s ->
                                s.SkuId = skuId
                                && s.StockingPointId = spId
                                && s.Origin = Inventory)
                            |> List.map (fun s -> s.Quantity)
                            |> Quantity.sum

                        return qty
                    }

                let inboundQuery
                    (skuId: SkuId)
                    (spId: StockingPointId)
                    (start: Timestamp)
                    (endT: Timestamp)
                    : Task<(Timestamp * Quantity * bool * string) list> =
                    task {
                        let list =
                            snapshot.SupplyBuckets
                            |> List.filter (fun s ->
                                s.SkuId = skuId
                                && s.StockingPointId = spId
                                && s.Origin <> Inventory
                                && s.Period >= start
                                && s.Period <= endT)
                            |> List.map (fun s -> (s.Period, s.Quantity, s.IsFirm, s.SupplyId))

                        return list
                    }

                let reservationsQuery
                    (skuId: SkuId)
                    (spId: StockingPointId)
                    (start: Timestamp)
                    (endT: Timestamp)
                    : Task<(Timestamp * Quantity * string) list> =
                    task { return [] }

                let safetyStockQuery (skuId: SkuId) (spId: StockingPointId) : Task<Quantity> =
                    task { return Quantity.Zero }

                let bomLookup (skuId: SkuId) (policy: BomSelectionPolicy) : BomExplosion.BomRecord option =
                    let edges =
                        snapshot.BomEdges
                        |> List.filter (fun e -> e.ParentProduct = skuId)

                    if List.isEmpty edges then
                        None
                    else
                        let components: BomExplosion.BomComponent list =
                            edges
                            |> List.mapi (fun idx e ->
                                { ComponentSkuId = e.Component
                                  QuantityPer = Quantity.clampToZero e.QtyPer
                                  UnitOfMeasureId =
                                    UomId.create "PCS"
                                    |> Result.defaultWith (fun _ -> failwith "Invalid Uom")
                                  Sequence = idx + 1
                                  IsPhantom = false })

                        let record: BomExplosion.BomRecord =
                            { BomId = $"BOM-{SkuId.value skuId}"
                              ParentSkuId = skuId
                              Components = components
                              IsActive = true }

                        Some record

                let productTypeQuery (skuId: SkuId) : Task<SupplyGenerationStep.ProductType> =
                    task {
                        let hasRoutings =
                            snapshot.Routings
                            |> List.exists (fun r -> r.Product = skuId)

                        if hasRoutings then
                            return SupplyGenerationStep.Manufactured
                        else
                            return SupplyGenerationStep.Purchased
                    }

                let supplierQuery (skuId: SkuId) (spId: StockingPointId) : Task<SupplierId option> =
                    task {
                        match SupplierId.create "DEFAULT_SUPPLIER" with
                        | Ok id -> return Some id
                        | Error _ -> return None
                    }

                let routingQuery (skuId: SkuId) (spId: StockingPointId) : Task<RoutingId option> =
                    task {
                        let routingOpt =
                            snapshot.Routings
                            |> List.tryFind (fun r -> r.Product = skuId)
                            |> Option.map (fun r ->
                                RoutingId.create r.ResourceId
                                |> Result.defaultWith (fun _ -> failwith "Invalid RoutingId"))

                        return routingOpt
                    }

                let transferSourceQuery (skuId: SkuId) (spId: StockingPointId) : Task<StockingPointId option> =
                    task { return None }

                let capacityPromiseQuery (rgId: ResourceGroupId) (desiredBucket: BucketIndex) (neededDuration: DurationMinutes) (tentativeLoad: TentativeLoad) : Task<CapacityPromiseResult> =
                    task {
                        return { EarliestFeasibleBucket = desiredBucket; IsFeasible = true }
                    }

                let capacityRoutingQuery (skuId: SkuId) (spId: StockingPointId) (routingIdOpt: RoutingId option) (qty: Quantity) : Task<CapacityRoutingInfo option> =
                    task {
                        let hasRoutings =
                            snapshot.Routings
                            |> List.exists (fun r -> r.Product = skuId)
                        if hasRoutings then
                            let routingId =
                                routingIdOpt
                                |> Option.defaultValue (
                                    RoutingId.create "DEFAULT_ROUTING"
                                    |> Result.defaultWith (fun _ -> failwith "Invalid RoutingId"))
                            let rgId =
                                ResourceGroupId.create "DEFAULT_RESOURCE"
                                |> Result.defaultWith (fun _ -> failwith "Invalid ResourceGroupId")
                            return Some {
                                RoutingId = routingId
                                ResourceGroupId = rgId
                                NeededDuration = DurationMinutes.zero
                            }
                        else
                            return None
                    }

                let alternateRoutingsQuery (skuId: SkuId) (spId: StockingPointId) : Task<RoutingId list> =
                    task { return [] }

                // 2. Build MRP Dependencies
                let deps: MrpDependencies =
                    { BomLookup = bomLookup
                      OnHandQuery = onHandQuery
                      InboundQuery = inboundQuery
                      ReservationsQuery = reservationsQuery
                      SafetyStockQuery = safetyStockQuery
                      ProductTypeQuery = productTypeQuery
                      SupplierQuery = supplierQuery
                      RoutingQuery = routingQuery
                      TransferSourceQuery = transferSourceQuery
                      CapacityPromiseQuery = capacityPromiseQuery
                      CapacityRoutingQuery = capacityRoutingQuery
                      AlternateRoutingsQuery = alternateRoutingsQuery
                      PeggingCreator = None
                      ReservationCreator = None
                      CreateSupplyOrders = fun _ _ -> async { return Ok() } }

                // 3. Translate inputs to MRP demands
                let demands =
                    snapshot.Demands
                    |> List.map (fun d ->
                        let source =
                            match d.DemandType.ToLowerInvariant() with
                            | "forecast" -> Forecast d.DemandId
                            | "salesorder"
                            | "customerorder" -> CustomerOrder(d.DemandId, "1")
                            | _ -> Manual d.DemandId

                        { DemandId = d.DemandId
                          SkuId = d.SkuId
                          NodeId =
                            NodeId.create "N1"
                            |> Result.defaultWith (fun _ -> failwith "Invalid NodeId")
                          StockingPointId = d.StockingPointId
                          Quantity = d.Quantity
                          RequiredDate = d.Period
                          Source = source
                          Priority = Some d.Priority })

                // 4. Create and execute pipeline
                let pipeline = Orchestrator.createPipeline deps

                let defaultSpId =
                    snapshot.Demands
                    |> List.tryHead
                    |> Option.map (fun d -> d.StockingPointId)
                    |> Option.defaultValue (
                        StockingPointId.create "SP-DEFAULT"
                        |> Result.defaultWith (fun _ -> failwith "Invalid StockingPointId")
                    )

                let startTimestamp = Timestamp.create horizon.StartDate
                let endTimestamp = Timestamp.create horizon.EndDate

                let! result =
                    Orchestrator.execute
                        pipeline
                        (Guid.NewGuid().ToString("N"))
                        startTimestamp
                        endTimestamp
                        defaultSpId
                        MrpPolicy.defaults
                        demands
                        []
                    |> Async.AwaitTask

                match result with
                | Error appErr -> return Error [ DomainError.invariant (sprintf "MRP execution failed: %A" appErr) ]
                | Ok mrpResult ->
                    let plannedOrders =
                        mrpResult.Proposals
                        |> List.map (fun p ->
                            let pType =
                                match p.ProposalType with
                                | PlannedWorkOrder -> PlannedProduction
                                | PlannedPurchaseOrder -> PlannedPurchase
                                | PlannedTransferOrder -> PlannedTransfer

                            { OrderId = SupplyProposalId.value p.Id
                              SkuId = p.SkuId
                              StockingPointId = p.StockingPointId
                              Period = p.DueDate
                              Quantity = p.Quantity
                              Type = pType
                              Zone = Medhavi.Scheduler.Planning.Domain.HorizonZone.Free
                              IsFirm = (p.Status = Firmed)
                              IsFrozen = (p.Status = Firmed)
                              SupplierOrResourceId = p.SupplierId |> Option.map SupplierId.value
                              DecisionRationale = None
                              EarliestStartDate = p.StartDate |> Option.map Timestamp.value
                              LatestEndDate = Some(Timestamp.value p.DueDate)
                              SetupGroup = None })

                    let capacityViolations =
                        mrpResult.Warnings
                        |> List.choose (fun warning ->
                            if warning.StartsWith("Capacity overload") then
                                Some
                                    { ResourceId = "RES-1"
                                      Period = mrpResult.StartTime
                                      Overload = Quantity.clampToZero 1.0m }
                            else
                                None)

                    let shortages =
                        mrpResult.NetRequirements
                        |> List.filter (fun nr -> Quantity.isPositive nr.NetRequirement)
                        |> List.map (fun nr ->
                            { SkuId = nr.SkuId
                              StockingPointId = nr.StockingPointId
                              Period = nr.RequiredDate
                              Quantity = nr.NetRequirement })

                    let kpis =
                        { PlanKpiSummary.ServiceLevel = 1.0
                          OnTimeDelivery = 1.0
                          InventoryCarryingCost = 0.0m
                          TotalCost = 0.0m
                          CO2Emissions = 0.0m
                          PlanChurn = 0.0
                          AverageTardiness = 0.0
                          ObjectiveValue = 0.0m
                          HardConstraintViolations = List.length capacityViolations
                          SoftConstraintViolations = 0
                          PlanHorizonDays = (horizon.EndDate - horizon.StartDate).Days
                          PlannedOrderCount = List.length mrpResult.Proposals
                          ShortageCount = List.length shortages }

                    let fingerprintHash =
                        InputFingerprint.computeHash
                            { ScenarioVersion = Version.initial
                              HorizonHash = sprintf "%A" horizon
                              DemandVersion = Version.initial
                              InventoryVersion = Version.initial
                              CapacityVersion = Version.initial
                              BomVersion = Version.initial
                              RoutingVersion = Version.initial
                              PolicyVersion = Version.initial
                              SolverVersion = "1.0.0"
                              Mode = mode
                              AsOf = DateTimeOffset.UtcNow }

                    let finalResult =
                        { RunId = Guid.NewGuid()
                          ScenarioId = scenarioId
                          InputFingerprintHash = fingerprintHash
                          PlannedOrders = plannedOrders
                          Pegging = mrpResult.Peggings
                          CapacityViolations = capacityViolations
                          Shortages = shortages
                          LimiterCatalog = []
                          DecisionRationales = Map.empty
                          GeneratedAt = DateTimeOffset.UtcNow
                          ObjectiveValue = 0.0m
                          KpiSummary = kpis }

                    return Ok finalResult
            }
