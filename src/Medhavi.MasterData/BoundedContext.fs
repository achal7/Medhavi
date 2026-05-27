namespace Medhavi.MasterData

open System
open System.Threading.Tasks
open Medhavi.Infrastructure.Stores.InMemRepository
open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.SharedKernel.API
open Medhavi.SharedKernel
open Medhavi.MasterData.Application
open Medhavi.MasterData.Domain.UomAgg
open Medhavi.MasterData.Domain.SkuAgg
open Medhavi.MasterData.Domain.StockingPointAgg
open Medhavi.MasterData.Domain.BoMAgg
open Medhavi.MasterData.Domain.RoutingAgg
open Medhavi.MasterData.Domain.TransportAgg
open Medhavi.MasterData.Domain.PlantAgg
open Medhavi.MasterData.Domain.NodeAgg
open Medhavi.MasterData.Domain.UnitConversionAgg

type MasterData =
    { Uom: UomApi
      Sku: SkuApi
      StockingPoint: StockingPointApi
      Bom: BomApi
      Routing: RoutingApi
      TransportLeg: TransportLegApi
      Plant: PlantApi
      UnitConversion: UnitConversionApi
      Node: Node.NodeCapabilities
      Initialize: unit -> Task<unit>
      Dispose: unit -> unit }

module BoundedContext =

    let create () =
        // 1. Repositories
        let uomRepo = createInMemoryRepository<UnitOfMeasure, string, UnitOfMeasureEvent> ()
        let skuRepo = createInMemoryRepository<Sku, string, SkuEvent> ()
        let spRepo = createInMemoryRepository<StockingPoint, string, StockingPointEvent> ()
        let bomRepo = createInMemoryRepository<BillOfMaterial, string, BomEvent> ()
        let routingRepo = createInMemoryRepository<Routing, string, RoutingEvent> ()
        let legRepo = createInMemoryRepository<TransportLeg, string, TransportLegEvent> ()
        
        let plantRepo = createInMemoryRepository<Plant, string, PlantEvent> ()
        let nodeRepo = createInMemoryRepository<Node, string, NodeEvent> ()
        let conversionRepo = createInMemoryRepository<UnitConversion, string, UnitConversionEvent> ()

        // 2. Capabilities
        let uomCaps = Uom.createCapabilities uomRepo
        let skuCaps = Sku.createCapabilities skuRepo
        let spCaps = StockingPoint.createCapabilities spRepo
        let bomCaps = BillOfMaterials.createCapabilities bomRepo
        let routingCaps = Routing.createCapabilities routingRepo
        let legCaps = TransportLeg.createCapabilities legRepo
        
        let plantCaps = Plant.createCapabilities plantRepo
        let nodeCaps = Node.createCapabilities nodeRepo
        let conversionCaps = UoMConversion.createCapabilities conversionRepo

        // 3. Projection Agents
        let uomAgent = Uom.createProjectionAgent ()
        let skuAgent = Sku.createProjection ()
        let spAgent = StockingPoint.createProjectionAgent ()
        let bomAgent = BillOfMaterials.createProjectionAgent ()
        let routingAgent = Routing.createProjectionAgent ()
        let legAgent = TransportLeg.createProjectionAgent ()
        let plantAgent = Plant.createProjectionAgent ()
        let conversionAgent = UoMConversion.createProjectionAgent ()

        // 4. APIs
        let uomApi = Uom.createUomApi uomCaps uomAgent
        let skuApi = Sku.createSkuApi skuCaps skuAgent
        let spApi = StockingPoint.createStockingPointApi spCaps spAgent
        let bomApi = BillOfMaterials.createBomApi bomCaps bomAgent
        let routingApi = Routing.createRoutingApi routingCaps routingAgent
        let legApi = TransportLeg.createTransportLegApi legCaps legAgent
        let plantApi = Plant.createPlantApi plantCaps plantAgent
        let conversionApi = UoMConversion.createUnitConversionApi conversionCaps conversionAgent

        // 5. Subscriptions List
        let mutable subscriptions : IDisposable list = []

        // 6. Initialize (Bootstrap & Subscriptions)
        let initialize () =
            task {
                // A. Seeding from Repositories
                let! uoms = uomRepo.GetAll()
                match uoms with
                | Ok list ->
                    let m = list |> List.map (fun u -> UomId.value u.Id, Uom.mapToUomDto u) |> Map.ofList
                    uomAgent.SetState(m)
                | Error _ -> ()

                let! skus = skuRepo.GetAll()
                match skus with
                | Ok list ->
                    let m = list |> List.map (fun s -> SkuId.value s.Id, Sku.mapSkuDto s) |> Map.ofList
                    skuAgent.SetState(m)
                | Error _ -> ()

                let! sps = spRepo.GetAll()
                match sps with
                | Ok list ->
                    let m = list |> List.map (fun s -> StockingPointId.value s.Id, StockingPoint.mapStockingPointDto s) |> Map.ofList
                    spAgent.SetState(m)
                | Error _ -> ()

                let! boms = bomRepo.GetAll()
                match boms with
                | Ok list ->
                    let m = list |> List.map (fun b -> BillOfMaterialId.value b.Id, BillOfMaterials.mapBomDto b) |> Map.ofList
                    bomAgent.SetState(m)
                | Error _ -> ()

                let! routings = routingRepo.GetAll()
                match routings with
                | Ok list ->
                    let m = list |> List.map (fun r -> RoutingId.value r.Id, Routing.mapRoutingDto r) |> Map.ofList
                    routingAgent.SetState(m)
                | Error _ -> ()

                let! legs = legRepo.GetAll()
                match legs with
                | Ok list ->
                    let m = list |> List.map (fun l -> TransportLegId.value l.Id, TransportLeg.mapTransportLegDto l) |> Map.ofList
                    legAgent.SetState(m)
                | Error _ -> ()

                let! plants = plantRepo.GetAll()
                match plants with
                | Ok list ->
                    let m = list |> List.map (fun p -> PlantId.value p.Id, Plant.mapPlantDto p) |> Map.ofList
                    plantAgent.SetState(m)
                | Error _ -> ()

                let! conversions = conversionRepo.GetAll()
                match conversions with
                | Ok list ->
                    let m = list |> List.map (fun c -> UnitConversionId.value c.Id, UoMConversion.mapUnitConversionDto c) |> Map.ofList
                    conversionAgent.SetState(m)
                | Error _ -> ()

                // B. Subscriptions
                subscriptions <- [
                    DomainEventBus.Subscribe<UnitOfMeasureEvent>(fun ev -> uomAgent.Post(ev, Guid.NewGuid(), None))
                    DomainEventBus.Subscribe<SkuEvent>(fun ev -> skuAgent.Post(ev, Guid.NewGuid(), None))
                    DomainEventBus.Subscribe<StockingPointEvent>(fun ev -> spAgent.Post(ev, Guid.NewGuid(), None))
                    DomainEventBus.Subscribe<BomEvent>(fun ev -> bomAgent.Post(ev, Guid.NewGuid(), None))
                    DomainEventBus.Subscribe<RoutingEvent>(fun ev -> routingAgent.Post(ev, Guid.NewGuid(), None))
                    DomainEventBus.Subscribe<TransportLegEvent>(fun ev -> legAgent.Post(ev, Guid.NewGuid(), None))
                    DomainEventBus.Subscribe<PlantEvent>(fun ev -> plantAgent.Post(ev, Guid.NewGuid(), None))
                    DomainEventBus.Subscribe<UnitConversionEvent>(fun ev -> conversionAgent.Post(ev, Guid.NewGuid(), None))
                ]
            }

        // 7. Dispose
        let dispose () =
            for sub in subscriptions do sub.Dispose()
            subscriptions <- []

        { Uom = uomApi
          Sku = skuApi
          StockingPoint = spApi
          Bom = bomApi
          Routing = routingApi
          TransportLeg = legApi
          Plant = plantApi
          UnitConversion = conversionApi
          Node = nodeCaps
          Initialize = initialize
          Dispose = dispose }
