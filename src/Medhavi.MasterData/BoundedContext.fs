namespace Medhavi.MasterData

open System
open System.Threading.Tasks
open Medhavi.Contracts.MasterData.Bom
open Medhavi.Contracts.MasterData.Network
open Medhavi.Contracts.MasterData.Resource
open Medhavi.Contracts.MasterData.Routing
open Medhavi.Contracts.MasterData.Sku
open Medhavi.Contracts.MasterData.Transport
open Medhavi.Contracts.MasterData.Uom
open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.SharedKernel.InMemRepository
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
open Medhavi.MasterData.Domain.ResourceGroupAgg
open Medhavi.MasterData.Domain.StandardResourceAgg
open Medhavi.MasterData.Domain.PhysicalResourceAgg


type MasterDataQueries =
    { Uom: UomQueryService
      Sku: SkuQueryService
      StockingPoint: StockingPointQueryService
      Bom: BomQueryService
      Routing: RoutingQueryService
      TransportLeg: TransportLegQueryService
      Plant: PlantQueryService
      UnitConversion: UnitConversionQueryService
      ResourceGroup: ResourceGroupQueryService
      StandardResource: StandardResourceQueryService
      PhysicalResource: PhysicalResourceQueryService }

type MasterDataCommands =
    { Uom: UomApi
      Sku: SkuApi
      StockingPoint: StockingPointApi
      Bom: BomApi
      Routing: RoutingApi
      TransportLeg: TransportLegApi
      Plant: PlantApi
      UnitConversion: UnitConversionApi
      Node: Node.NodeCapabilities
      ResourceGroup: ResourceGroupApi
      StandardResource: StandardResourceApi
      PhysicalResource: PhysicalResourceApi }

type MasterDataContext =
    { Commands: MasterDataCommands
      Queries: MasterDataQueries
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
        let groupRepo = createInMemoryRepository<ResourceGroup, string, ResourceGroupEvent> ()
        let standardRepo = createInMemoryRepository<StandardResource, string, StandardResourceEvent> ()
        let physicalRepo = createInMemoryRepository<PhysicalResource, string, PhysicalResourceEvent> ()

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
        let groupCaps = ResourceGroup.createCapabilities groupRepo
        let standardCaps = StandardResource.createCapabilities standardRepo
        let physicalCaps = PhysicalResource.createCapabilities physicalRepo

        // 3. Projection Agents
        let uomAgent = Uom.createProjectionAgent ()
        let skuAgent = Sku.createProjection ()
        let spAgent = StockingPoint.createProjectionAgent ()
        let bomAgent = BillOfMaterials.createProjectionAgent ()
        let routingAgent = Routing.createProjectionAgent ()
        let legAgent = TransportLeg.createProjectionAgent ()
        let plantAgent = Plant.createProjectionAgent ()
        let conversionAgent = UoMConversion.createProjectionAgent ()
        let groupAgent = ResourceGroup.createProjectionAgent ()
        let standardAgent = StandardResource.createProjectionAgent ()
        let physicalAgent = PhysicalResource.createProjectionAgent ()

        // 4. APIs
        let uomApi = Uom.createUomApi uomCaps
        let skuApi = Sku.createSkuApi skuCaps
        let spApi = StockingPoint.createStockingPointApi spCaps
        let bomApi = BillOfMaterials.createBomApi bomCaps
        let routingApi = Routing.createRoutingApi routingCaps
        let legApi = TransportLeg.createTransportLegApi legCaps legAgent
        let plantApi = Plant.createPlantApi plantCaps
        let conversionApi = UoMConversion.createUnitConversionApi conversionCaps
        let groupApi = ResourceGroup.createResourceGroupApi groupCaps
        let standardApi = StandardResource.createStandardResourceApi standardCaps
        let physicalApi = PhysicalResource.createPhysicalResourceApi physicalCaps

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

                let! groups = groupRepo.GetAll()
                match groups with
                | Ok list ->
                    let m = list |> List.map (fun g -> ResourceGroupId.value g.Id, ResourceGroup.mapResourceGroupDto g) |> Map.ofList
                    groupAgent.SetState(m)
                | Error _ -> ()

                let! standards = standardRepo.GetAll()
                match standards with
                | Ok list ->
                    let m = list |> List.map (fun s -> StandardResourceId.value s.Id, StandardResource.mapStandardResourceDto s) |> Map.ofList
                    standardAgent.SetState(m)
                | Error _ -> ()

                let! physicals = physicalRepo.GetAll()
                match physicals with
                | Ok list ->
                    let m = list |> List.map (fun p -> PhysicalResourceId.value p.Id, PhysicalResource.mapPhysicalResourceDto p) |> Map.ofList
                    physicalAgent.SetState(m)
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
                    DomainEventBus.Subscribe<ResourceGroupEvent>(fun ev -> groupAgent.Post(ev, Guid.NewGuid(), None))
                    DomainEventBus.Subscribe<StandardResourceEvent>(fun ev -> standardAgent.Post(ev, Guid.NewGuid(), None))
                    DomainEventBus.Subscribe<PhysicalResourceEvent>(fun ev -> physicalAgent.Post(ev, Guid.NewGuid(), None))
                ]
            }

        // 7. Dispose
        let dispose () =
            for sub in subscriptions do sub.Dispose()
            subscriptions <- []

        let queries : MasterDataQueries =
            { Uom = QueryServiceBase.getQueryService uomAgent id
              Sku = QueryServiceBase.getQueryService skuAgent id
              StockingPoint = QueryServiceBase.getQueryService spAgent id
              Bom = QueryServiceBase.getQueryService bomAgent id
              Routing = QueryServiceBase.getQueryService routingAgent id
              TransportLeg = QueryServiceBase.getQueryService legAgent id
              Plant = QueryServiceBase.getQueryService plantAgent id
              UnitConversion = QueryServiceBase.getQueryService conversionAgent id
              ResourceGroup = QueryServiceBase.getQueryService groupAgent id
              StandardResource = QueryServiceBase.getQueryService standardAgent id
              PhysicalResource = QueryServiceBase.getQueryService physicalAgent id }

        let commands : MasterDataCommands =
            { Uom = uomApi
              Sku = skuApi
              StockingPoint = spApi
              Bom = bomApi
              Routing = routingApi
              TransportLeg = legApi
              Plant = plantApi
              UnitConversion = conversionApi
              Node = nodeCaps
              ResourceGroup = groupApi
              StandardResource = standardApi
              PhysicalResource = physicalApi }

        { Commands = commands
          Queries = queries
          Initialize = initialize
          Dispose = dispose }
