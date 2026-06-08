namespace Medhavi.Nexus

open System
open System.Threading.Tasks
open Medhavi.SharedKernel
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.Analytics.PlanningHorizon
open Medhavi.Analytics.KPI
open Medhavi.Analytics
open Medhavi.Common

module DigitalTwin =

    /// Evaluates if an anomaly is detected based on raw telemetry values
    let detectAnomaly (sensorId: string) (reading: float) (threshold: float) =
        if reading > threshold then
            Some(sprintf "Sensor %s exceeded threshold of %f with value %f" sensorId threshold reading)
        else
            None

module AnalyticsWiring =

    open Medhavi.Demand
    open Medhavi.Supply
    open Medhavi.Capacity
    open Medhavi.Transport
    open Medhavi.Scenario

    // Shared global reference to the latest MRP run for peggings/proposals lookup
    let mutable latestMrpRunRef: Medhavi.Scheduler.Mrp.Domain.MrpRunAggregate.MrpRunResult option = None

    let getRealDataSources
        (demandQuery: DemandQueryService)
        (supplyQueries: SupplyQueries)
        (capacityContext: CapacityContext)
        (transportContext: TransportContext)
        (scenarioQueries: ScenarioQueries)
        : ProjectionDataSources =

        // Helper to retrieve scenario overlay
        let getOverlay (context: PlanContext) : Task<ScenarioOverlay option> =
            task {
                match context with
                | Live -> return None
                | Scenario sid ->
                    let! scenOpt = scenarioQueries.GetById(sid)
                    return scenOpt |> Option.map (fun s -> ScenarioAdapter.toScenarioOverlay sid s.Overrides)
            }

        let demandSource : DemandDataSource =
            { GetDemandLines = fun plantId startDate endDate context ->
                task {
                    let! lines = demandQuery.GetDemandLines plantId startDate endDate
                    let! overlayOpt = getOverlay context
                    return
                        lines
                        |> List.map (fun line ->
                            let mapped =
                                { DemandLineView.DemandLineId = line.DemandLineId
                                  DemandOrderId = line.DemandOrderId
                                  SkuId = SkuId.value line.SkuId
                                  SkuCode = SkuId.value line.SkuId
                                  SkuName = SkuId.value line.SkuId
                                  CustomerId = line.CustomerId
                                  CustomerName = line.CustomerId
                                  Priority = line.Priority
                                  DemandCategory = sprintf "%A" line.DemandCategory
                                  IsFirm = line.IsFirm
                                  EarliestDeliveryDate = line.EarliestDeliveryDate |> Option.map (fun d -> DateOnly.FromDateTime(d.DateTime))
                                  RequestedDeliveryDate = DateOnly.FromDateTime(line.RequestedDeliveryDate.DateTime)
                                  LatestDeliveryDate = line.LatestDeliveryDate |> Option.map (fun d -> DateOnly.FromDateTime(d.DateTime))
                                  ConfirmedDeliveryDate = line.ConfirmedDeliveryDate |> Option.map (fun d -> DateOnly.FromDateTime(d.DateTime))
                                  RequestedQty = Quantity.value line.Quantity
                                  OpenQty = Quantity.value line.OpenQuantity
                                  FulfilledQty = Quantity.value line.FulfilledQuantity
                                  ConfirmedQty = line.ConfirmedDeliveryDate |> Option.map (fun _ -> Quantity.value line.Quantity) |> Option.defaultValue 0.0m
                                  ShortfallQty = max 0.0m (Quantity.value line.OpenQuantity - (line.ConfirmedDeliveryDate |> Option.map (fun _ -> Quantity.value line.Quantity) |> Option.defaultValue 0.0m))
                                  LatenessRisk =
                                      match line.ConfirmedDeliveryDate with
                                      | None -> LatenessRisk.Critical
                                      | Some cdd ->
                                          if cdd <= line.RequestedDeliveryDate then LatenessRisk.OnTrack
                                          else
                                              match line.LatestDeliveryDate with
                                              | None -> LatenessRisk.AtRisk (int (cdd - line.RequestedDeliveryDate).TotalDays)
                                              | Some ldd ->
                                                  if cdd <= ldd then LatenessRisk.AtRisk (int (cdd - line.RequestedDeliveryDate).TotalDays)
                                                  else LatenessRisk.Critical
                                  PeggedSupply = [] }
                            match overlayOpt with
                            | None -> mapped
                            | Some overlay -> ScenarioAdapter.applyDemandOverlay overlay mapped
                        )
                }
              GetPeggedSupply = fun demandLineId context ->
                task {
                    match latestMrpRunRef with
                    | None -> return []
                    | Some run ->
                        return
                            run.Peggings
                            |> List.filter (fun p -> p.Demand.DemandId = demandLineId)
                            |> List.map (fun (p: Medhavi.Scheduler.Mrp.Domain.PeggingLink) ->
                                let supplyId, supplyType, deliveryDate =
                                    match p.Target with
                                    | Medhavi.Scheduler.Mrp.Domain.PegTarget.Supply s ->
                                        s.SupplyId, sprintf "%A" s.ProposalType, DateOnly.FromDateTime((Timestamp.value s.DeliveryDate).DateTime)
                                    | Medhavi.Scheduler.Mrp.Domain.PegTarget.Reservation r ->
                                        let deliveryDate = DateOnly.FromDateTime((Timestamp.value p.Demand.NeedDate).DateTime)
                                        match r with
                                        | Medhavi.Scheduler.Mrp.Domain.Material id -> id, "MaterialReservation", deliveryDate
                                        | Medhavi.Scheduler.Mrp.Domain.Capacity id -> CapacityReservationId.value id, "CapacityReservation", deliveryDate
                                        | Medhavi.Scheduler.Mrp.Domain.Transport id -> id, "TransportReservation", deliveryDate
                                { SupplyOrderId = supplyId
                                  SupplyType = supplyType
                                  Quantity = Quantity.value p.PeggedQty
                                  PlannedDate = deliveryDate })
                }
            }

        let materialSource : MaterialDataSource =
            { GetSupplyElements = fun skuId plantId startDate endDate context ->
                task {
                    let! orders = supplyQueries.SupplyOrder.GetAll()
                    let filtered =
                        orders
                        |> List.filter (fun o ->
                            o.SkuId = skuId &&
                            (o.RequiredDeliveryDate
                             |> Option.map (fun d -> DateOnly.FromDateTime(d.DateTime) >= startDate && DateOnly.FromDateTime(d.DateTime) <= endDate)
                             |> Option.defaultValue false))
                    return
                        filtered
                        |> List.map (fun o ->
                            { SupplyElementView.SupplyOrderId = o.Id
                              SupplyType =
                                  match o.OrderType.ToLower() with
                                  | "workorder" | "plannedworkorder" -> PlannedProductionOrder
                                  | "purchaseorder" | "plannedpurchaseorder" -> PlannedPurchaseOrder
                                  | _ -> PlannedProductionOrder
                              SkuId = o.SkuId
                              SkuCode = o.SkuId
                              StockingPointId = o.StockingPointId
                              PlannedQty = o.Quantity
                              ConfirmedQty = o.CompletedQuantity
                              PlannedDate = o.RequiredDeliveryDate |> Option.map (fun d -> DateOnly.FromDateTime(d.DateTime)) |> Option.defaultValue startDate
                              IsFirm = o.IsFirm
                              IsLocked = o.IsLocked
                              IsExpedited = o.IsExpedited
                              RoutingId = o.RoutingId
                              SupplierId = o.SupplierId
                              LeadTimeDays = None })
                }
              GetInventorySnapshot = fun skuId stockingPointId asOf context ->
                task {
                    let! invs = supplyQueries.Inventory.GetAll()
                    let! targets = supplyQueries.InventoryTarget.GetAll()
                    let invOpt = invs |> List.tryFind (fun i -> i.SkuId = skuId && i.StockingPointId = stockingPointId)
                    let targetOpt = targets |> List.tryFind (fun t -> t.SkuId = skuId && t.StockingPointId = stockingPointId)
                    let! overlayOpt = getOverlay context
                    let baseSnapshot =
                        { InventorySnapshot.SkuId = skuId
                          StockingPointId = stockingPointId
                          OnHandQty = invOpt |> Option.map (fun i -> i.Quantity) |> Option.defaultValue 0.0m
                          AvailableToPromise = invOpt |> Option.map (fun i -> i.Quantity) |> Option.defaultValue 0.0m
                          QualityHoldQty = 0m
                          DamagedQty = 0m
                          InTransitInboundQty = 0m
                          InTransitOutboundQty = 0m
                          SafetyStockQty = targetOpt |> Option.bind (fun t -> t.SafetyStockQty) |> Option.defaultValue 0.0m
                          MaxStockQty = targetOpt |> Option.bind (fun t -> t.MaxQty)
                          DaysOfSupply = 0m
                          SnapshotDate = asOf }
                    match overlayOpt with
                    | None -> return baseSnapshot
                    | Some overlay -> return ScenarioAdapter.applyInventoryOverlay overlay baseSnapshot
                }
            }

        let capacitySource : CapacityDataSource =
            { GetOperations = fun plantId startDate endDate context ->
                task {
                    let! ops = capacityContext.OperationAgent.GetStateAsync()
                    let filtered =
                        ops.Values
                        |> Seq.filter (fun o ->
                            let start = DateOnly.FromDateTime((Timestamp.value o.Window.Start).DateTime)
                            start >= startDate && start <= endDate)
                        |> Seq.toList
                    return
                        filtered
                        |> List.map (fun o ->
                            { OperationView.OperationId = OperationId.value o.Id
                              WorkOrderId = None
                              SkuId = ""
                              SkuCode = ""
                              RoutingStepId = RoutingStepId.value o.RoutingStepId
                              OperationCode = RoutingStepId.value o.RoutingStepId
                              Quantity = 0m
                              SetupMinutes = 0m
                              RunMinutes = o.Duration |> Option.map (fun d -> decimal d.TotalMinutes) |> Option.defaultValue 0m
                              StartTime = Timestamp.value o.Window.Start
                              EndTime = o.Window.End |> Option.map Timestamp.value |> Option.defaultValue (Timestamp.value o.Window.Start)
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
                              IsExpedited = false })
                }
              GetCapacityBuckets = fun resourceGroupId startDate endDate ->
                task {
                    let! buckets = capacityContext.CapacityAgent.GetStateAsync()
                    let matched =
                        buckets.Values
                        |> Seq.filter (fun b ->
                            PhysicalResourceId.value b.ResourceId = resourceGroupId &&
                            let start = DateOnly.FromDateTime((Timestamp.value b.Window.Start).DateTime)
                            start >= startDate && start <= endDate)
                        |> Seq.toList
                    return
                        matched
                        |> List.map (fun b ->
                            { AvailableHours = (DurationMinutes.value b.AvailableMinutes) / 60.0m
                              CalendarHours = (DurationMinutes.value b.AvailableMinutes) / 60.0m
                              MaintenanceHours = 0.0m } : Medhavi.Analytics.PlanningHorizon.CapacityBucketView)
                }
              GetMaintenanceWindows = fun plantId startDate endDate ->
                task {
                    return []
                }
            }

        let transportSource : TransportDataSource =
            { GetShipments = fun legId startDate endDate context ->
                task {
                    let! resvs = transportContext.ReservationAgent.GetStateAsync()
                    let filtered =
                        resvs.Values
                        |> Seq.filter (fun r ->
                            let matchesLeg =
                                match legId with
                                | None -> true
                                | Some lid -> ItineraryId.value r.ItineraryId = lid
                            let start = DateOnly.FromDateTime(r.EarliestDeparture.DateTime)
                            matchesLeg && start >= startDate && start <= endDate)
                        |> Seq.toList
                    return
                        filtered
                        |> List.map (fun r ->
                            { ShipmentView.ShipmentId = TransportReservationId.value r.Id
                              TransportLegId = ItineraryId.value r.ItineraryId
                              SkuId = r.SkuId
                              SkuCode = r.SkuId
                              FromPlantId = r.FromNode
                              ToPlantId = r.ToNode
                              Quantity = r.Quantity
                              DepartureDate = DateOnly.FromDateTime(r.EarliestDeparture.DateTime)
                              ArrivalDate = DateOnly.FromDateTime(r.EarliestArrival.DateTime)
                              TransitDays = decimal (r.EarliestArrival - r.EarliestDeparture).TotalDays
                              Mode = ""
                              CarrierId = None
                              Status = ShipmentStatus.PlannedShipment
                              IsFirm = false })
                }
            }

        { Demand = demandSource
          Material = materialSource
          Capacity = capacitySource
          Transport = transportSource }

    let mutable private isBootstrapped = false
    let mutable private subscription: IDisposable option = None

    let bootstrapAnalytics
        (demandQuery: DemandQueryService)
        (supplyQueries: SupplyQueries)
        (capacityContext: CapacityContext)
        (transportContext: TransportContext)
        (scenarioQueries: ScenarioQueries)
        : KpiQueryService =

        let sources = getRealDataSources demandQuery supplyQueries capacityContext transportContext scenarioQueries
        let configReader () = Task.FromResult([])
        let kpiService = KpiQueryService.createKpiQueryService sources configReader

        // Wire Event-Driven Invalidations
        if not isBootstrapped then
            let sub =
                DomainEventBus.Subscribe<obj>(fun evObj ->
                    match evObj with
                    | :? Medhavi.Scheduler.Mrp.Pipeline.MrpEvent as mrpEvt ->
                        match mrpEvt with
                        | Medhavi.Scheduler.Mrp.Pipeline.MrpEvent.MrpRunCompleted(runId, timestamp) ->
                            let dates = [ DateOnly.FromDateTime(DateTime.UtcNow) ]
                            let invEvent = Medhavi.Contracts.Integration.KpiInvalidationEvent.MrpRunCompleted("PLANT-DEFAULT", "SP-DEFAULT", runId, dates)
                            let keys = KpiInvalidation.keysAffectedBy invEvent
                            KpiRefreshEngine.invalidateKeys keys
                        | _ -> ()
                    | :? Medhavi.Supply.Domain.InventoryAgg.InventoryEvent as invEvt ->
                        match invEvt with
                        | Medhavi.Supply.Domain.InventoryAgg.InventoryCreated e ->
                            let date = DateOnly.FromDateTime(DateTime.UtcNow)
                            let invEvent = Medhavi.Contracts.Integration.KpiInvalidationEvent.InventoryAdjusted(StockingPointId.value e.StockingPointId, SkuId.value e.SkuId, date)
                            let keys = KpiInvalidation.keysAffectedBy invEvent
                            KpiRefreshEngine.invalidateKeys keys
                        | _ -> ()
                    | _ -> ())

            subscription <- Some sub
            isBootstrapped <- true
            printfn "Analytics and KPI engine successfully bootstrapped in Nexus."

        kpiService
