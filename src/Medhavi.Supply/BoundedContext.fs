namespace Medhavi.Supply

open System
open System.Threading.Tasks
open Medhavi.Infrastructure.Stores.InMemRepository
open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.SharedKernel.API
open Medhavi.SharedKernel
open Medhavi.Supply.Application
open Medhavi.Supply.Domain.InventoryAgg
open Medhavi.Supply.Domain.InventoryTargetAgg
open Medhavi.Supply.Domain.SupplierOfferAgg
open Medhavi.Supply.Domain.SupplyOrderAgg
open Medhavi.Supply.Domain

open Medhavi.SharedKernel.Projections

type SupplyQueries =
    { Inventory: InventoryQueryService
      InventoryTarget: InventoryTargetQueryService
      SupplierOffer: SupplierOfferQueryService
      SupplyOrder: SupplyOrderQueryService
      MaterialReservation: MaterialReservationQueryService }

type SupplyCommands =
    { Inventory: InventoryApi
      InventoryTarget: InventoryTargetApi
      SupplierOffer: SupplierOfferApi
      SupplyOrder: SupplyOrderApi
      MaterialReservation: MaterialReservationApi }

type SupplyContext =
    { Commands: SupplyCommands
      Queries: SupplyQueries
      Initialize: unit -> Task<unit>
      Dispose: unit -> unit }

module BoundedContext =

    let create () =
        // 1. Repositories
        let invRepo = createInMemoryRepository<Inventory, string, InventoryEvent> ()

        let targetRepo =
            createInMemoryRepository<InventoryTarget, string, InventoryTargetEvent> ()

        let offerRepo =
            createInMemoryRepository<SupplierOffer, string, SupplierOfferEvent> ()

        let orderRepo = createInMemoryRepository<SupplyOrder, string, SupplyOrderEvent> ()

        let reservationRepo =
            createInMemoryRepository<
                MaterialReservationAgg.MaterialReservation,
                string,
                MaterialReservationAgg.MaterialReservationEvent
             > ()

        // 2. Capabilities
        let invCaps = Inventory.createCapabilities invRepo
        let targetCaps = InventoryTarget.createCapabilities targetRepo
        let offerCaps = SupplierOffer.createCapabilities offerRepo
        let orderCaps = SupplyOrder.createCapabilities orderRepo
        let reservationCaps = MaterialReservation.createCapabilities reservationRepo

        // 3. Projection Agents
        let invAgent = Inventory.createProjectionAgent ()
        let targetAgent = InventoryTarget.createProjectionAgent ()
        let offerAgent = SupplierOffer.createProjectionAgent ()
        let orderAgent = SupplyOrder.createProjectionAgent ()
        let reservationAgent = MaterialReservation.createProjectionAgent ()

        // 4. APIs
        let invApi = Inventory.createInventoryApi invCaps invAgent
        let targetApi = InventoryTarget.createInventoryTargetApi targetCaps targetAgent
        let offerApi = SupplierOffer.createSupplierOfferApi offerCaps offerAgent
        let orderApi = SupplyOrder.createSupplyOrderApi orderCaps orderAgent

        let reservationApi =
            MaterialReservation.createMaterialReservationApi reservationCaps reservationAgent

        // 5. Subscriptions List
        let mutable subscriptions: IDisposable list = []
        let mutable sweeperRunning = true

        // 6. Initialize (Bootstrap & Subscriptions)
        let initialize () =
            task {
                // A. Seeding from Repositories
                let! invs = invRepo.GetAll()

                match invs with
                | Ok list ->
                    let m =
                        list
                        |> List.map (fun i -> InventoryId.value i.Id, Inventory.ACL.toContract i)
                        |> Map.ofList

                    invAgent.SetState(m)
                | Error _ -> ()

                let! targets = targetRepo.GetAll()

                match targets with
                | Ok list ->
                    let m =
                        list
                        |> List.map (fun t -> InventoryTargetId.value t.Id, InventoryTarget.ACL.toContract t)
                        |> Map.ofList

                    targetAgent.SetState(m)
                | Error _ -> ()

                let! offers = offerRepo.GetAll()

                match offers with
                | Ok list ->
                    let m =
                        list
                        |> List.map (fun o -> SupplierOfferId.value o.Id, SupplierOffer.ACL.toContract o)
                        |> Map.ofList

                    offerAgent.SetState(m)
                | Error _ -> ()

                let! orders = orderRepo.GetAll()

                match orders with
                | Ok list ->
                    let m =
                        list
                        |> List.map (fun o -> SupplyOrderId.value o.Id, SupplyOrder.ACL.toContract o)
                        |> Map.ofList

                    orderAgent.SetState(m)
                | Error _ -> ()

                let! reservations = reservationRepo.GetAll()

                match reservations with
                | Ok list ->
                    let m =
                        list
                        |> List.map (fun r -> r.Id, MaterialReservation.ACL.toContract r)
                        |> Map.ofList

                    reservationAgent.SetState(m)
                | Error _ -> ()

                // B. Subscriptions
                subscriptions <-
                    [ DomainEventBus.Subscribe<InventoryEvent>(fun ev -> invAgent.Post(ev, Guid.NewGuid(), None))
                      DomainEventBus.Subscribe<InventoryTargetEvent>(fun ev ->
                          targetAgent.Post(ev, Guid.NewGuid(), None))
                      DomainEventBus.Subscribe<SupplierOfferEvent>(fun ev -> offerAgent.Post(ev, Guid.NewGuid(), None))
                      DomainEventBus.Subscribe<SupplyOrderEvent>(fun ev -> orderAgent.Post(ev, Guid.NewGuid(), None))
                      DomainEventBus.Subscribe<MaterialReservationAgg.MaterialReservationEvent>(fun ev ->
                          reservationAgent.Post(ev, Guid.NewGuid(), None)) ]

                // C. TTL Sweeper Loop
                let rec sweeper () =
                    task {
                        if sweeperRunning then
                            do! Task.Delay(5000)
                            let! allRes = reservationRepo.GetAll()

                            match allRes with
                            | Error _ -> ()
                            | Ok list ->
                                let now = DateTimeOffset.UtcNow

                                let expired =
                                    list
                                    |> List.filter (fun r -> r.State = "Tentative" && r.ExpiryTime < now)

                                for r in expired do
                                    let! _ = reservationCaps.Expire { Id = r.Id }
                                    ()

                            return! sweeper ()
                    }
                // Run in background without awaiting the loop task
                let _ = sweeper ()
                ()
            }

        // 7. Dispose
        let dispose () =
            sweeperRunning <- false

            for sub in subscriptions do
                sub.Dispose()

            subscriptions <- []

        let queries: SupplyQueries =
            { Inventory = QueryServiceBase.getQueryService invAgent id
              InventoryTarget = QueryServiceBase.getQueryService targetAgent id
              SupplierOffer = QueryServiceBase.getQueryService offerAgent id
              SupplyOrder = QueryServiceBase.getQueryService orderAgent id
              MaterialReservation = QueryServiceBase.getQueryService reservationAgent id }

        let commands: SupplyCommands =
            { Inventory = invApi
              InventoryTarget = targetApi
              SupplierOffer = offerApi
              SupplyOrder = orderApi
              MaterialReservation = reservationApi }

        { Commands = commands
          Queries = queries
          Initialize = initialize
          Dispose = dispose }
