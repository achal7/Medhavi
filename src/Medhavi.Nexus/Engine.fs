namespace Medhavi.Nexus

open System
open System.Threading.Tasks
open Medhavi.Common
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
open Medhavi.Contracts.Promise
open Medhavi.Contracts.Domain
open Medhavi.Contracts.Demand
open Medhavi.Contracts.Capacity
open Medhavi.Contracts.Supply
open Medhavi.SharedKernel.ScenarioContracts

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

    let configRepo =
        createInMemoryRepository<ScenarioConfiguration, string, ScenarioConfigurationEvent> ()

    let overlayRepo =
        createInMemoryRepository<ScenarioOverlaySet, string, ScenarioOverlayEvent> ()

    let scenarioContext =
        Medhavi.Scenario.BoundedContext.create scenarioRepo configRepo overlayRepo

    let transportContext =
        BoundedContext.create (MasterData.getTransportLegs masterDataContext)

    // Real EnvelopeStore instance (Mailbox-agent based)
    let envelopeStore = createEnvelopeStoreMem ()
    let integrationCaps = IntegrationService.createCapabilities envelopeStore

    let mrpDep =
        SchedulerWiring.buildMrpDependencies masterDataContext supplyContext capacityContext demandContext

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
                        | Error err -> printfn "   [ ERR ] Failed to deserialize envelope: %A" err
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
                            | DemandsImported demands ->
                                for d in demands do
                                    let req: DemandDefineReq =
                                        { DemandLineId = d.DemandLineId
                                          DemandOrderId = d.DemandOrderId
                                          SkuId = d.SkuId
                                          StockingPointId = d.StockingPointId
                                          CustomerId = d.CustomerId
                                          Quantity = d.Quantity
                                          UnitOfMeasure = d.UnitOfMeasure
                                          OrderDate = d.OrderDate
                                          EarliestDeliveryDate = d.EarliestDeliveryDate
                                          RequestedDeliveryDate = d.RequestedDeliveryDate
                                          LatestDeliveryDate = d.LatestDeliveryDate
                                          ConfirmedDeliveryDate = d.ConfirmedDeliveryDate
                                          ActualDeliveryDate = d.ActualDeliveryDate
                                          Priority = d.Priority
                                          DemandCategory = d.DemandCategory.ToLower()
                                          IsFirm = d.IsFirm
                                          IsFrozen = d.IsFrozen }

                                    let! _ = demandContext.Commands.DemandLine.Define(req)
                                    ()

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

                // Ingest and publish CSV master data to bootstrap the system with realistic data
                let! bootstrapRes = integrationCaps.IngestAndPublishMasterData()

                match bootstrapRes with
                | Error err -> printfn "   [ ERR ] Failed to bootstrap master data from CSV: %A" err
                | Ok _ -> printfn "   [ OK ] System successfully bootstrapped with CSV master data."

                initialized <- true
        }

    // --- CLEAN FACADE APIs FOR THE UI ---

    member this.GetDemands() : Task<DemandLine list> =
        task {
            let! stateMap = demandContext.DemandAgent.GetStateAsync()
            let! skus = masterDataContext.Queries.Sku.GetAll()
            let skuMap = skus |> Seq.map (fun s -> s.Id, s) |> Map.ofSeq

            return
                stateMap.Values
                |> Seq.toList
                |> List.map (fun d ->
                    let skuIdStr = SkuId.value d.SkuId
                    let skuOpt = Map.tryFind skuIdStr skuMap

                    let skuCode =
                        skuOpt
                        |> Option.map (fun s -> s.Code)
                        |> Option.defaultValue skuIdStr

                    let skuName =
                        skuOpt
                        |> Option.map (fun s -> s.Name)
                        |> Option.defaultValue skuIdStr

                    { DemandLineId = d.DemandLineId
                      DemandOrderId = d.DemandOrderId
                      SkuId = skuIdStr
                      SkuCode = skuCode
                      SkuName = skuName
                      CustomerId = d.CustomerId
                      CustomerName = d.CustomerId
                      StockingPointId = StockingPointId.value d.StockingPointId
                      Priority = d.Priority
                      DemandCategory = d.DemandCategory.ToString()
                      IsFirm = d.IsFirm
                      EarliestDeliveryDate =
                        d.EarliestDeliveryDate
                        |> Option.map (fun dt -> DateOnly.FromDateTime(dt.DateTime))
                      RequestedDeliveryDate = DateOnly.FromDateTime(d.RequestedDeliveryDate.DateTime)
                      LatestDeliveryDate =
                        d.LatestDeliveryDate
                        |> Option.map (fun dt -> DateOnly.FromDateTime(dt.DateTime))
                      ConfirmedDeliveryDate =
                        d.ConfirmedDeliveryDate
                        |> Option.map (fun dt -> DateOnly.FromDateTime(dt.DateTime))
                      RequestedQty = Quantity.value d.Quantity
                      OpenQty = Quantity.value d.OpenQuantity
                      FulfilledQty = Quantity.value d.FulfilledQuantity
                      ConfirmedQty =
                        Quantity.value d.Quantity
                        - Quantity.value d.OpenQuantity
                      ShortfallQty = Quantity.value d.OpenQuantity
                      LatenessRisk = LatenessRisk.OnTrack
                      Status = d.Status.ToString()
                      UnitOfMeasure = d.UnitOfMeasure
                      PeggedSupply = [] }
                    : DemandLine)
        }

    member this.RunMrp() : Task<Result<unit, string>> =
        task {
            let mrpLogger =
                { LogInfo = printfn "[MRP] %s"
                  LogWarning = printfn "[MRP WARN] %s"
                  LogError = printfn "[MRP ERR] %s" }

            try
                do! Mrp.runBaselineMrp mrpDep demandContext mrpLogger
                return Ok()
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
            | Ok _ -> return Ok()
            | Error err -> return Error(sprintf "%A" err)
        }

    // --- CLEAN FACADE APIs FOR THE UI ---

    member this.GetSkus() : Task<Sku list> = task { return! masterDataContext.Queries.Sku.GetAll() }

    member this.GetPlants() : Task<Plant list> = task { return! masterDataContext.Queries.Plant.GetAll() }

    member this.GetStockingPoints() : Task<StockingPoint list> =
        task { return! masterDataContext.Queries.StockingPoint.GetAll() }

    member this.GetResources() : Task<StandardResource list> =
        task { return! masterDataContext.Queries.StandardResource.GetAll() }

    member this.GetSupplyOrders() : Task<SupplyOrder list> = task { return! supplyContext.Queries.SupplyOrder.GetAll() }

    member this.GetCapacityOperations() : Task<OperationView list> =
        task {
            let! ops = capacityContext.OperationAgent.GetStateAsync()

            return
                ops.Values
                |> Seq.map (fun o ->
                    { OperationId = OperationId.value o.Id
                      WorkOrderId = None
                      SkuId = ""
                      SkuCode = ""
                      RoutingStepId = RoutingStepId.value o.RoutingStepId
                      OperationCode = RoutingStepId.value o.RoutingStepId
                      Quantity = 0m
                      SetupMinutes = 0m
                      RunMinutes =
                        o.Duration
                        |> Option.map (fun d -> decimal d.TotalMinutes)
                        |> Option.defaultValue 0m
                      StartTime = Timestamp.value o.Window.Start
                      EndTime =
                        o.Window.End
                        |> Option.map Timestamp.value
                        |> Option.defaultValue (Timestamp.value o.Window.Start)
                      Status =
                        match o.State with
                        | Medhavi.Capacity.Domain.OperationAgg.Scheduled -> OperationStatus.Planned
                        | Medhavi.Capacity.Domain.OperationAgg.InProgress -> OperationStatus.InProgress
                        | Medhavi.Capacity.Domain.OperationAgg.Completed -> OperationStatus.Completed
                        | Medhavi.Capacity.Domain.OperationAgg.Cancelled -> OperationStatus.Cancelled
                      DemandOrderId = None
                      PeggedDemandQty = None
                      IsFirm = o.IsFixed
                      IsFrozen = false
                      IsExpedited = false }
                    : OperationView)
                |> Seq.toList
        }

    member this.GetScenarios() : Task<ScenarioReadModel list> = task { return! scenarioContext.Queries.GetAll() }

    member this.CreateScenario(name: string, scenarioType: ScenarioType, parentId: string option) : Task<Result<unit, string>> =
        task {
            let scenarioId = $"SCENARIO-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
            let! res = scenarioContext.Commands.Create(scenarioId, name, scenarioType, parentId)
            match res with
            | Ok () -> return Ok ()
            | Error err -> return Error (sprintf "%A" err)
        }

    member this.EvaluatePromise
        (req: Medhavi.Contracts.Promise.PromiseRequest)
        : Task<Result<PromiseEvaluationResponse, string>> =
        task {
            let getSkuIdFromRouting (r: Routing) =
                match r.Details with
                | RoutingDetails.Work w -> w.ProductId
                | RoutingDetails.Transport t -> t.SkuId
                | RoutingDetails.Purchase p -> p.SkuId

            let materialProvider: Medhavi.Promise.MaterialProvider =
                { GetSnapshot =
                    fun (skuId, stockingPointId, asOf) ->
                        async {
                            let! res =
                                Medhavi.Supply.Application.MaterialProvider.getSnapshot
                                    supplyContext
                                    (SkuId.value skuId)
                                    (StockingPointId.value stockingPointId)
                                    asOf

                            match res with
                            | Ok snap ->
                                let pSnap: Medhavi.Promise.PromiseTypes.MaterialSnapshot =
                                    { SkuId = skuId
                                      StockingPointId = stockingPointId
                                      OnHand = snap.OnHand
                                      Inbound = snap.Inbound
                                      Reservations = snap.Reservations |> List.sumBy snd
                                      Safety = snap.Safety }

                                return Ok pSnap
                            | Error e -> return Error Medhavi.Promise.PromiseTypes.ProviderError.Unavailable
                        }
                  GetSupplierOptions =
                    fun (skuId, stockingPointId, qty, asOf) ->
                        async {
                            let! res =
                                Medhavi.Supply.Application.MaterialProvider.getSupplierOptions
                                    supplyContext
                                    (SkuId.value skuId)
                                    (Some(StockingPointId.value stockingPointId))
                                    qty
                                    asOf

                            match res with
                            | Ok offers ->
                                let pOffers =
                                    offers
                                    |> List.map (fun o ->
                                        let leadTimeP50Min = o.LeadTimeP50Minutes |> Option.defaultValue 0.0
                                        let earliest = asOf.AddMinutes(leadTimeP50Min)

                                        let availableQty =
                                            if List.isEmpty o.CapacityWindows then
                                                qty
                                            else
                                                o.CapacityWindows
                                                |> List.sumBy (fun w -> w.AvailableQuantity)

                                        let cost =
                                            o.PriceTiers
                                            |> List.tryFind (fun t ->
                                                qty >= t.MinQuantity
                                                && (match t.MaxQuantity with
                                                    | None -> true
                                                    | Some max -> qty <= max))
                                            |> Option.map (fun t -> t.PricePerUnit)
                                            |> Option.defaultValue (
                                                if List.isEmpty o.PriceTiers then
                                                    0m
                                                else
                                                    (List.head o.PriceTiers).PricePerUnit
                                            )

                                        ({ SupplierId = o.SupplierId
                                           Earliest = earliest
                                           Qty = availableQty
                                           Cost = cost
                                           Reliability = o.Reliability
                                           Moq = o.Moq
                                           LeadTimeP50 =
                                             o.LeadTimeP50Minutes
                                             |> Option.map TimeSpan.FromMinutes
                                           LeadTimeP95 =
                                             o.LeadTimeP95Minutes
                                             |> Option.map TimeSpan.FromMinutes
                                           Incoterm = o.Incoterm }
                                        : Medhavi.Promise.PromiseTypes.SupplierOption))

                                return Ok pOffers
                            | Error _ -> return Error Medhavi.Promise.PromiseTypes.ProviderError.Unavailable
                        } }

            let capacityProvider: Medhavi.Promise.CapacityProvider =
                { CheckCapacity =
                    fun (skuId, qty, asOf) ->
                        async {
                            let! resources =
                                capacityContext.CapacityResourceAgent.GetStateAsync()
                                |> Async.AwaitTask

                            let! calendars =
                                capacityContext.CalendarAgent.GetStateAsync()
                                |> Async.AwaitTask

                            let! buckets =
                                capacityContext.CapacityAgent.GetStateAsync()
                                |> Async.AwaitTask

                            let getRoutings (sku: string) =
                                task {
                                    let! all = masterDataContext.Queries.Routing.GetAll()

                                    let filtered =
                                        all
                                        |> List.filter (fun r -> getSkuIdFromRouting r = sku)

                                    return Ok filtered
                                }

                            let! res =
                                Medhavi.Capacity.Application.SchedulerApp.checkCapacity
                                    (SkuId.value skuId)
                                    qty
                                    asOf
                                    Medhavi.Capacity.Domain.CapacityAgg.CapacityPlanningMode.Finite
                                    resources
                                    calendars
                                    buckets
                                    getRoutings
                                |> Async.AwaitTask

                            match res with
                            | Ok checkRes ->
                                let cRes: Medhavi.Promise.PromiseTypes.CapacityCheckResult =
                                    { IsFeasible = checkRes.IsFeasible
                                      SuggestedDate = checkRes.SuggestedDate
                                      RequiredLoads =
                                        checkRes.RequiredLoads
                                        |> Map.map (fun _ v -> DurationMinutes.value v)
                                      BottleneckResourceId = checkRes.BottleneckResourceId
                                      LatenessReason = checkRes.LatenessReason
                                      EarliestAvailable = checkRes.SuggestedDate }

                                return Ok cRes
                            | Error e -> return Error Medhavi.Promise.PromiseTypes.ProviderError.Unavailable
                        } }

            let transportProvider: Medhavi.Promise.TransportProvider =
                { GetOptions =
                    fun (origin, dest, asOf) ->
                        async {
                            let req: GetTransportOptionsReq =
                                { FromNode = origin
                                  ToNode = dest
                                  SkuId = None
                                  RequiredQuantity = None
                                  NeedByDate = asOf
                                  MaxHops = None
                                  MaxItineraries = None }

                            let! res = transportContext.Atp.GetOptions req

                            match res with
                            | Ok options ->
                                let itineraries = options |> List.map (fun o -> o.Itinerary)
                                return Ok itineraries
                            | Error e -> return Error Medhavi.Promise.PromiseTypes.ProviderError.Unavailable
                        } }

            let routingProvider: Medhavi.Promise.RoutingProvider =
                { Select =
                    fun (skuId, stockingPointId) ->
                        async {
                            let! all =
                                masterDataContext.Queries.Routing.GetAll()
                                |> Async.AwaitTask

                            let filtered =
                                all
                                |> List.filter (fun r -> getSkuIdFromRouting r = SkuId.value skuId)

                            if List.isEmpty filtered then
                                return Error Medhavi.Promise.PromiseTypes.ProviderError.Unavailable
                            else
                                let first = List.head filtered

                                let primaryChoice: Medhavi.Promise.PromiseTypes.RoutingChoice =
                                    { RoutingId = RoutingId.create first.Id |> Result.get
                                      AlternateUsed = false
                                      EstimatedDuration = Some(TimeSpan.FromHours(24.0))
                                      Reliability = Some 0.95m }

                                let selection: Medhavi.Promise.PromiseTypes.RoutingSelection =
                                    { Primary = primaryChoice
                                      Alternates = [] }

                                return Ok selection
                        } }

            let reservationProvider: Medhavi.Promise.ReservationProvider =
                { CreateTentative =
                    fun reqs ->
                        async {
                            let ids =
                                reqs
                                |> List.map (fun r -> $"res-{r.Scope.ToString().ToLower()}-{Guid.NewGuid()}")

                            return Ok ids
                        }
                  Confirm = fun _ -> async { return Ok() }
                  Release = fun _ -> async { return Ok() } }

            let tenantProvider: Medhavi.Promise.TenantProvider =
                { GetTenant = fun () -> "tenant-default", TimeZoneInfo.Utc, Some "USD" }

            let translateOrderLine (l: PromiseOrderLine) : Medhavi.Promise.PromiseTypes.OrderLine =
                let skuId = SkuId.create l.SkuId |> Result.get

                let spId =
                    StockingPointId.create l.StockingPointId
                    |> Result.get

                let qty = Quantity.create l.Quantity |> Result.get

                let origin =
                    l.Origin
                    |> Option.map (fun o -> StockingPointId.create o |> Result.get)

                let dest =
                    l.Destination
                    |> Option.map (fun d -> StockingPointId.create d |> Result.get)

                { LineId = l.LineId
                  SkuId = skuId
                  StockingPointId = spId
                  Quantity = qty
                  DueDate = l.DueDate
                  Priority = l.Priority
                  IsExpedited = l.IsExpedited
                  Origin = origin
                  Destination = dest }

            let translateOrder (o: PromiseOrder) : Medhavi.Promise.PromiseTypes.Order =
                let orderId = OrderId.create o.OrderId |> Result.get

                { OrderId = orderId
                  Lines = o.Lines |> List.map translateOrderLine
                  CustomerId = o.CustomerId
                  RequestDate = o.RequestDate }

            let cmd: Medhavi.Promise.PromiseTypes.PromiseRequestCmd =
                { Order = translateOrder req.Order
                  AsOfDate = req.AsOfDate
                  CustomerTier = req.CustomerTier
                  SkuTier = req.SkuTier
                  Currency = req.Currency }

            let! res =
                Medhavi.Promise.PromiseService.tryPromiseOrder
                    materialProvider
                    capacityProvider
                    transportProvider
                    routingProvider
                    reservationProvider
                    tenantProvider
                    cmd
                |> Async.StartAsTask

            match res with
            | Ok resp ->
                let mapDecision (d: PromiseDecisionStatus) = d

                let mapRouting (r: Medhavi.Promise.PromiseTypes.RoutingChoice option) =
                    r
                    |> Option.map (fun rc ->
                        { RoutingId = string rc.RoutingId
                          AlternateUsed = rc.AlternateUsed
                          EstimatedDuration = rc.EstimatedDuration
                          Reliability = rc.Reliability }
                        : PromiseRoutingChoice)

                let mapCost (c: Medhavi.Promise.PromiseTypes.CostBreakdown option) =
                    c
                    |> Option.map (fun cb ->
                        { MaterialCost = cb.MaterialCost
                          ProductionCost = cb.ProductionCost
                          TransportCost = cb.TransportCost
                          HoldingCost = cb.HoldingCost
                          LatenessPenalty = cb.LatenessPenalty }
                        : PromiseCostBreakdown)

                let response: PromiseEvaluationResponse =
                    { Decision = mapDecision resp.Decision
                      Routing = mapRouting resp.Routing
                      Cost = mapCost resp.Cost
                      Confidence = resp.Confidence
                      Reservations = resp.Reservations |> List.map string }

                return Ok response
            | Error e -> return Error(sprintf "Order promising failed: %A" e)
        }
