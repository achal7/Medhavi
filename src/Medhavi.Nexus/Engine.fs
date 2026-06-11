namespace Medhavi.Nexus

open System
open System.Threading.Tasks
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Infrastructure.Stores.EnvelopeStoreMem
open Medhavi.Infrastructure.Stores.InMemRepository
open Medhavi.Integration
open Medhavi.Capacity
open Medhavi.MasterData
open Medhavi.SharedKernel
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.Supply
open Medhavi.Transport
open Medhavi.Scenario.Domain

type DemandViewItem =
    { DemandLineId: string
      DemandOrderId: string
      SkuId: string
      StockingPointId: string
      Quantity: decimal
      UnitOfMeasure: string
      OrderDate: DateTimeOffset
      RequestedDeliveryDate: DateTimeOffset
      Priority: int
      DemandCategory: string
      OpenQuantity: decimal
      FulfilledQuantity: decimal
      Status: string }

type UIEventLogItem =
    { EventId: string
      EventType: string
      Stream: string
      Timestamp: DateTimeOffset }

type MedhaviEngine() =
    // Initialize Bounded Contexts via modular composition roots
    let masterDataContext = Medhavi.MasterData.BoundedContext.create ()
    let supplyContext = Medhavi.Supply.BoundedContext.create ()
    let capacityContext = Medhavi.Capacity.BoundedContext.create ()
    let demandContext = Medhavi.Demand.BoundedContext.create ()

    let scenarioRepo = createInMemoryRepository<Scenario, string, ScenarioEvent> ()
    let configRepo = createInMemoryRepository<ScenarioConfiguration, string, ScenarioConfigurationEvent> ()
    let overlayRepo = createInMemoryRepository<ScenarioOverlaySet, string, ScenarioOverlayEvent> ()

    let scenarioContext = Medhavi.Scenario.BoundedContext.create scenarioRepo configRepo overlayRepo
    let transportContext = Medhavi.Transport.BoundedContext.create (Medhavi.Nexus.MasterData.getTransportLegs masterDataContext)
    
    // Real EnvelopeStore instance (Mailbox-agent based)
    let envelopeStore = createEnvelopeStoreMem ()
    let integrationCaps = IntegrationService.createCapabilities envelopeStore

    let mrpDep = SchedulerWiring.buildMrpDependencies masterDataContext supplyContext capacityContext demandContext

    let kpiQueryService =
        AnalyticsWiring.bootstrapAnalytics
            demandContext.Queries.DemandLine
            supplyContext.Queries
            capacityContext
            transportContext
            scenarioContext.Queries

    let mutable subscriptionHandle: SubscriptionHandle option = None
    let mutable initialized = false

    member this.MasterData = masterDataContext
    member this.Supply = supplyContext
    member this.Capacity = capacityContext
    member this.Demand = demandContext
    member this.Scenario = scenarioContext
    member this.Transport = transportContext
    member this.EnvelopeStore = envelopeStore
    member this.IntegrationCaps = integrationCaps
    member this.MrpDependencies = mrpDep
    member this.KpiQueryService = kpiQueryService

    member this.Initialize() =
        task {
            if not initialized then
                // Bootstrap projections
                do! masterDataContext.Initialize()
                do! supplyContext.Initialize()
                do! capacityContext.Initialize()
                do! transportContext.Initialize()
                do! scenarioContext.Initialize()
                do! demandContext.Initialize()

                // Subscribe contexts
                let handler (envelopedEvent: EnvelopedEvent) : Task<unit> =
                    task {
                        let envelope = envelopedEvent.Envelope
                        match IntegrationEventEnvelope.tryGetPayload envelope with
                        | Error err ->
                            printfn "   [ ERR ] Failed to deserialize envelope: %A" err
                        | Ok event ->
                            match event with
                            | UomImported uoms ->
                                let! _ = masterDataContext.Commands.Uom.DefineBulk(uoms)
                                ()
                            | UnitConversionsImported unitConversions ->
                                let! _ = masterDataContext.Commands.UnitConversion.DefineBulk(unitConversions)
                                ()
                            | TransportLegsImported transportLegs ->
                                let! _ = masterDataContext.Commands.TransportLeg.DefineBulk(transportLegs)
                                ()
                            | RoutingsImported routings ->
                                let! _ = masterDataContext.Commands.Routing.DefineBulk(routings)
                                ()
                            | BomImported boms ->
                                let! _ = masterDataContext.Commands.Bom.DefineBulk(boms)
                                ()
                            | SkusImported skus ->
                                let! _ = masterDataContext.Commands.Sku.DefineBulk(skus)
                                ()
                            | StockingPointsImported stockingPoints ->
                                let! _ = masterDataContext.Commands.StockingPoint.DefineBulk(stockingPoints)
                                ()
                            | PlantsImported plants ->
                                let! _ = masterDataContext.Commands.Plant.DefineBulk(plants)
                                ()
                            | ResourceGroupsImported groups ->
                                let! _ = masterDataContext.Commands.ResourceGroup.DefineBulk(groups)
                                ()
                            | StandardResourcesImported reqs ->
                                let! _ = masterDataContext.Commands.StandardResource.DefineBulk(reqs)
                                ()
                            | PhysicalResourcesImported reqs ->
                                let! _ = masterDataContext.Commands.PhysicalResource.DefineBulk(reqs)
                                ()
                            | MaterialsReceived _
                            | SupplyOffersImported _
                            | InventoryPositionsImported _
                            | InventoryTargetsImported _
                            | SupplyOrdersImported _
                            | MaterialReservationsImported _
                            | ResourceCalendarsImported _
                            | WorkOrdersCompleted _ ->
                                 let dummyLogger = 
                                     { LogInfo = fun m -> printfn "[Supply] %s" m
                                       LogSuccess = fun m -> printfn "[Supply OK] %s" m
                                       LogError = fun m -> printfn "[Supply ERR] %s" m }

                                 Supply.handleRequest supplyContext masterDataContext dummyLogger event
                                |> ignore
                            | ResourceDowntimes _
                            | TransportDelays _ ->
                                let mrpLogger = 
                                    { LogInfo = fun m -> printfn "[MRP Ingest] %s" m
                                      LogWarning = fun m -> printfn "[MRP Ingest WARN] %s" m
                                      LogError = fun m -> printfn "[MRP Ingest ERR] %s" m }

                                Mrp.handleRequest mrpDep masterDataContext mrpLogger event
                                |> ignore
                    }

                let! subscribeTask =
                    envelopeStore.Subscribe SubscriptionMode.All None handler System.Threading.CancellationToken.None

                match subscribeTask with
                | Error err -> printfn "   [ ERR ] Failed to subscribe: %A" err
                | Ok handle ->
                    subscriptionHandle <- Some handle
                    printfn "   [ OK ] Web engine Bounded Context subscriptions established."

                // Seed initial demands
                do! Task.Run(fun () -> Demand.seedDemands demandContext)
                initialized <- true
        }

    // --- CLEAN FACADE APIs FOR THE UI ---

    member this.GetDemands() : Task<DemandViewItem list> =
        task {
            let! stateMap = demandContext.DemandAgent.GetStateAsync()
            return 
                stateMap 
                |> Map.values 
                |> Seq.toList 
                |> List.map (fun d ->
                    { DemandLineId = d.DemandLineId
                      DemandOrderId = d.DemandOrderId
                      SkuId = SkuId.value d.SkuId
                      StockingPointId = StockingPointId.value d.StockingPointId
                      Quantity = Quantity.value d.Quantity
                      UnitOfMeasure = d.UnitOfMeasure
                      OrderDate = d.OrderDate
                      RequestedDeliveryDate = d.RequestedDeliveryDate
                      Priority = d.Priority
                      DemandCategory = d.DemandCategory.ToString()
                      OpenQuantity = Quantity.value d.OpenQuantity
                      FulfilledQuantity = Quantity.value d.FulfilledQuantity
                      Status = d.Status.ToString() })
        }

    member this.RunMrp() : Task<Result<unit, string>> =
        task {
            let mrpLogger = 
                { LogInfo = printfn "[MRP] %s"
                  LogWarning = printfn "[MRP WARN] %s"
                  LogError = printfn "[MRP ERR] %s" }
            try
                do! Mrp.runBaselineMrp mrpDep demandContext mrpLogger
                return Ok ()
            with ex ->
                return Error ex.Message
        }

    member this.GetEvents() : Task<UIEventLogItem list> =
        task {
            let! res = envelopeStore.ReadAll None None System.Threading.CancellationToken.None
            match res with
            | Error _ -> return []
            | Ok envelopes ->
                return
                    envelopes
                    |> Seq.map (fun e ->
                        let (Medhavi.Infrastructure.EventId id) = e.Envelope.EventId
                        { EventId = id.ToString()
                          EventType = e.Envelope.EventType
                          Stream = e.ReadFrom |> Option.defaultValue "Unknown"
                          Timestamp = e.Envelope.CreatedUtc })
                    |> Seq.toList
                    |> List.sortByDescending (fun e -> e.Timestamp)
        }

    member this.TriggerImport() : Task<Result<unit, string>> =
        task {
            let! res = integrationCaps.IngestAndPublishMasterData()
            match res with
            | Ok _ -> return Ok ()
            | Error err -> return Error (sprintf "%A" err)
        }
