namespace Medhavi.Scheduler.Tests

open System
open System.Threading.Tasks
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Steps.SupplyGenerationStep
open Medhavi.Scheduler.Mrp.Application

module TestCommon =

    let getOk =
        function
        | Ok x -> x
        | Error e -> failwithf "Expected Ok, got Error: %A" e

    let createQty v =
        Quantity.create v
        |> Result.defaultWith (fun e -> failwith e.Message)

    let createTimestamp (dto: DateTimeOffset) = Timestamp.create dto

    let createTimestampYmd y m d = Timestamp.create (DateTimeOffset(y, m, d, 0, 0, 0, TimeSpan.Zero))

    let createPercent v =
        Percent.create v
        |> Result.defaultWith (fun e -> failwith e.Message)

    // Common master data ids
    let skuFG = SkuId.create "SKU-FG" |> getOk
    let skuRM = SkuId.create "SKU-RM" |> getOk
    let skuSub = SkuId.create "sku-SUB" |> getOk

    let nodeWarehouse = NodeId.create "NODE-WAREHOUSE" |> getOk
    let spWarehouse = StockingPointId.create "SP-WAREHOUSE" |> getOk
    let nodeFactory = NodeId.create "NODE-FACTORY" |> getOk
    let spFactory = StockingPointId.create "SP-FACTORY" |> getOk
    let uomPc = UomId.create "pc" |> getOk

    // Factory builders for test data
    let defaultDemand sku : MrpDemand =
        { DemandId = "dem-" + Guid.NewGuid().ToString().Substring(0, 8)
          SkuId = sku
          NodeId = nodeWarehouse
          StockingPointId = spWarehouse
          Quantity = createQty 10m
          RequiredDate = Timestamp.now
          Source = Manual "test"
          Priority = None }

    let mockBomLookup sku _ =
        if sku = skuFG then
            Some
                { BomId = "bom-fg-1"
                  ParentSkuId = skuFG
                  Components =
                    [ { ComponentSkuId = skuSub
                        QuantityPer = createQty 2m
                        UnitOfMeasureId = uomPc
                        Sequence = 1
                        IsPhantom = false } ]
                  IsActive = true }
            : Algorithms.BomExplosion.BomRecord option
        elif sku = skuSub then
            Some
                { BomId = "bom-sub-2"
                  ParentSkuId = skuSub
                  Components =
                    [ { ComponentSkuId = skuRM
                        QuantityPer = createQty 4m
                        UnitOfMeasureId = uomPc
                        Sequence = 1
                        IsPhantom = false } ]
                  IsActive = true }
            : Algorithms.BomExplosion.BomRecord option
        else
            None

    let defaultProposal sku : SupplyProposal =
        { Id =
            SupplyProposalId.create (
                "prop-"
                + Guid.NewGuid().ToString().Substring(0, 8)
            )
            |> getOk
          ProposalType = PlannedPurchaseOrder
          SkuId = sku
          NodeId = nodeWarehouse
          StockingPointId = spWarehouse
          Quantity = createQty 10m
          DueDate = Timestamp.now
          StartDate = None
          RoutingId = None
          SupplierId = None
          Priority = 5
          IsExpedite = false
          Status = Planned
          PeggingRefs = []
          CapacityCheckedDate = None
          CreatedAt = Timestamp.now }

    let defaultDemandRef sku : DemandRef =
        { DemandId = "dem-" + Guid.NewGuid().ToString().Substring(0, 8)
          SkuId = sku
          NodeId = nodeWarehouse
          StockingPointId = spWarehouse
          NeedDate = Timestamp.now
          Quantity = createQty 10m }

    let defaultSupplyRef sku : SupplyRef =
        { SupplyId = "sup-" + Guid.NewGuid().ToString().Substring(0, 8)
          ProposalType = PlannedPurchaseOrder
          SkuId = sku
          NodeId = nodeWarehouse
          StockingPointId = spWarehouse
          DeliveryDate = Timestamp.now
          Quantity = createQty 10m }

    let defaultPeggingLink (demandRef: DemandRef) (targetSupplyRef: SupplyRef) : PeggingLink =
        { Id = PeggingId.createDeterministic demandRef.DemandId targetSupplyRef.SupplyId
          Demand = demandRef
          Target = PegTarget.Supply targetSupplyRef
          PeggedQty = demandRef.Quantity
          Status = PegStatus.Active
          IsLocked = false
          Created = DateTimeOffset.UtcNow
          Modified = DateTimeOffset.UtcNow }

    // Reusable stubs for MrpDependencies queries
    let defaultDeps: MrpDependencies =
        { BomLookup = fun _ _ -> None
          OnHandQuery = fun _ _ -> Task.FromResult(createQty 0m)
          InboundQuery = fun _ _ _ _ -> Task.FromResult([])
          ReservationsQuery = fun _ _ _ _ -> Task.FromResult([])
          SafetyStockQuery = fun _ _ -> Task.FromResult(createQty 0m)
          ProductTypeQuery = fun _ -> Task.FromResult(Purchased)
          SupplierQuery = fun _ _ -> Task.FromResult(None)
          RoutingQuery = fun _ _ -> Task.FromResult(None)
          TransferSourceQuery = fun _ _ -> Task.FromResult(None)
          CapacityPromiseQuery = fun _ desiredBucket _ _ -> Task.FromResult({ EarliestFeasibleBucket = desiredBucket; IsFeasible = true })
          CapacityRoutingQuery = fun _ _ routingIdOpt _ -> Task.FromResult(
              Some {
                  RoutingId = routingIdOpt |> Option.defaultValue (RoutingId.create "DEFAULT_ROUTING" |> getOk)
                  ResourceGroupId = ResourceGroupId.create "DEFAULT_RG" |> getOk
                  NeededDuration = DurationMinutes.zero
              })
          AlternateRoutingsQuery = fun _ _ -> Task.FromResult([])
          PeggingCreator = None
          ReservationCreator = None
          CreateSupplyOrders = fun _ _ -> async { return Ok() } }

    // ============================================================================
    // BDD Given/When/Then DSL (Functional State Composition)
    // ============================================================================
    module Bdd =
        type Scenario<'State> = { State: 'State }

        let Given (state: 'State) = { State = state }

        let When (action: 'State -> 'NextState) (scenario: Scenario<'State>) = { State = action scenario.State }

        let WhenAsync (action: 'State -> Async<'NextState>) (scenario: Scenario<'State>) =
            async {
                let! next = action scenario.State
                return { State = next }
            }

        let Then (assertion: 'State -> unit) (scenario: Scenario<'State>) =
            assertion scenario.State
            scenario

        let ThenAsync (assertion: 'State -> unit) (scenario: Async<Scenario<'State>>) =
            async {
                let! s = scenario
                assertion s.State
                return s
            }
