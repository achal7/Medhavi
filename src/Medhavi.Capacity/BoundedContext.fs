namespace Medhavi.Capacity

open System
open System.Threading.Tasks
open Medhavi.SharedKernel
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.SharedKernel.InMemRepository
open Medhavi.Capacity.Domain.CalendarAgg
open Medhavi.Capacity.Domain.CapacityAgg
open Medhavi.Capacity.Domain.CapacityReservationAgg
open Medhavi.Capacity.Domain.OperationAgg
open Medhavi.Capacity.Domain.CapacityResourceAgg
open Medhavi.Capacity.Application
open Medhavi.Infrastructure.Projections

type CapacityContext =
    { Calendar: CalendarApp.CalendarCapabilities
      Capacity: CapacityApp.CapacityCapabilities
      Operation: OperationApp.OperationCapabilities
      CapacityResource: CapacityResourceApp.CapacityResourceCapabilities
      CapacityReservation: CapacityReservationApp.CapacityReservationCapabilities
      CalendarAgent: ProjectionAgent<Map<string, Calendar>, CalendarsEvent>
      CapacityAgent: ProjectionAgent<Map<string, CapacityBucket>, CapacityEvent>
      OperationAgent: ProjectionAgent<Map<string, Operation>, OperationEvent>
      CapacityResourceAgent: ProjectionAgent<Map<string, CapacityResource>, CapacityResourceEvent>
      CapacityReservationAgent: ProjectionAgent<Map<string, CapacityReservation>, CapacityReservationEvent>
      Initialize: unit -> Task<unit>
      Dispose: unit -> unit }

module BoundedContext =

    let create () =
        // 1. Repositories
        let calendarRepo = createInMemoryRepository<Calendar, string, CalendarsEvent>()

        let capacityRepo = createInMemoryRepository<CapacityBucket, string, CapacityEvent>()

        let operationRepo = createInMemoryRepository<Operation, string, OperationEvent>()

        let capacityResourceRepo = createInMemoryRepository<CapacityResource, string, CapacityResourceEvent>()

        let capacityReservationRepo = createInMemoryRepository<CapacityReservation, string, CapacityReservationEvent>()

        // 2. Capabilities
        let calendarCaps = CalendarApp.createCapabilities calendarRepo
        let capacityCaps = CapacityApp.createCapabilities capacityRepo
        let operationCaps = OperationApp.createCapabilities operationRepo

        let capacityResourceCaps = CapacityResourceApp.createCapabilities capacityResourceRepo

        let capacityReservationCaps = CapacityReservationApp.createCapabilities capacityReservationRepo

        // 3. Projection Agents
        let calendarAgent = CalendarApp.createProjectionAgent()
        let capacityAgent = CapacityApp.createProjectionAgent()
        let operationAgent = OperationApp.createProjectionAgent()
        let capacityResourceAgent = CapacityResourceApp.createProjectionAgent()
        let capacityReservationAgent = CapacityReservationApp.createProjectionAgent()

        // 4. Subscriptions List
        let mutable subscriptions: IDisposable list = []

        // 5. Initialize (Bootstrap & Subscriptions)
        let initialize () =
            task {
                // A. Seeding from Repositories
                let! calendars = calendarRepo.GetAll()

                match calendars with
                | Ok list ->
                    let m =
                        list
                        |> List.map(fun c ->
                            match c.Id with
                            | CalendarId cid -> cid, c)
                        |> Map.ofList

                    calendarAgent.SetState(m)
                | Error _ -> ()

                let! buckets = capacityRepo.GetAll()

                match buckets with
                | Ok list ->
                    let m = list |> List.map(fun b -> CapacityBucketId.value b.Id, b) |> Map.ofList

                    capacityAgent.SetState(m)
                | Error _ -> ()

                let! operations = operationRepo.GetAll()

                match operations with
                | Ok list ->
                    let m = list |> List.map(fun o -> OperationId.value o.Id, o) |> Map.ofList

                    operationAgent.SetState(m)
                | Error _ -> ()

                let! capacityResources = capacityResourceRepo.GetAll()

                match capacityResources with
                | Ok list ->
                    let m = list |> List.map(fun r -> PhysicalResourceId.value r.Id, r) |> Map.ofList

                    capacityResourceAgent.SetState(m)
                | Error _ -> ()

                let! reservations = capacityReservationRepo.GetAll()

                match reservations with
                | Ok list ->
                    let m = list |> List.map(fun r -> CapacityReservationId.value r.Id, r) |> Map.ofList

                    capacityReservationAgent.SetState(m)
                | Error _ -> ()

                // B. Subscriptions (Internal)
                let localSubs =
                    [ DomainEventBus.Subscribe<CalendarsEvent>(fun ev -> calendarAgent.Post(ev, Guid.NewGuid(), None))
                      DomainEventBus.Subscribe<CapacityEvent>(fun ev -> capacityAgent.Post(ev, Guid.NewGuid(), None))
                      DomainEventBus.Subscribe<OperationEvent>(fun ev -> operationAgent.Post(ev, Guid.NewGuid(), None))
                      DomainEventBus.Subscribe<CapacityResourceEvent>(fun ev ->
                          capacityResourceAgent.Post(ev, Guid.NewGuid(), None))
                      DomainEventBus.Subscribe<CapacityReservationEvent>(fun ev ->
                          capacityReservationAgent.Post(ev, Guid.NewGuid(), None)) ]

                // D. Auto-Generation of Capacity Buckets from CapacityResourceRegistered events
                let bucketGenSub =
                    DomainEventBus.Subscribe<CapacityResourceEvent>(fun ev ->
                        match ev with
                        | CapacityResourceRegistered e ->
                            let now = Timestamp.now
                            let windowDays = 30

                            for i in 0 .. windowDays - 1 do
                                let startDt =
                                    Timestamp.value now
                                    |> fun t ->
                                        DateTimeOffset(t.Year, t.Month, t.Day, 0, 0, 0, t.Offset).AddDays(float i)

                                let endDt = startDt.AddDays(1.0)

                                match TimeWindow.createFromTime startDt endDt with
                                | Error _ -> ()
                                | Ok win ->
                                    match DurationMinutes.create 480.0m with // Default 8 hours per day capacity
                                    | Error _ -> ()
                                    | Ok cap ->
                                        task {
                                            let! _ = capacityCaps.DefineBucket(e.Id, win, cap)
                                            return ()
                                        }
                                        |> ignore
                        | _ -> ())

                // E. Forward Reservation Events to Buckets to update Planned/Free Minutes
                let reservationForwardSub =
                    DomainEventBus.Subscribe<CapacityReservationEvent>(fun ev ->
                        match ev with
                        | CapacityReservationCreated e ->
                            task {
                                let! _ =
                                    capacityCaps.Reserve(
                                        e.Id,
                                        e.RequirementId,
                                        e.Minutes,
                                        e.Start,
                                        e.End,
                                        e.Source,
                                        e.BucketId
                                    )

                                return ()
                            }
                            |> ignore
                        | CapacityReservationReleased e ->
                            task {
                                let! resOpt = capacityReservationRepo.Get(CapacityReservationId.value e.Id)

                                match resOpt with
                                | Ok(Some res) ->
                                    let! _ = capacityCaps.Cancel(e.Id, res.BucketId)
                                    ()
                                | _ -> ()
                            }
                            |> ignore)

                subscriptions <- localSubs @ [ bucketGenSub; reservationForwardSub ]
            }

        // 6. Dispose
        let dispose () =
            for sub in subscriptions do
                sub.Dispose()

            subscriptions <- []

        { Calendar = calendarCaps
          Capacity = capacityCaps
          Operation = operationCaps
          CapacityResource = capacityResourceCaps
          CapacityReservation = capacityReservationCaps
          CalendarAgent = calendarAgent
          CapacityAgent = capacityAgent
          OperationAgent = operationAgent
          CapacityResourceAgent = capacityResourceAgent
          CapacityReservationAgent = capacityReservationAgent
          Initialize = initialize
          Dispose = dispose }
