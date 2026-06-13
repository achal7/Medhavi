namespace Medhavi.Transport

open System
open System.Threading.Tasks
open Medhavi.SharedKernel
open Medhavi.Infrastructure.Projections
open Medhavi.Infrastructure.Stores.InMemRepository
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.Transport.Domain.TransportReservationAgg
open Medhavi.Transport.Application
open Medhavi.Transport.Application.TransportAtpApp

/// The composed Transport Bounded Context
type TransportContext =
    { Atp: TransportAtpCapabilities
      Reservation: TransportReservationApp.TransportReservationCapabilities
      ReservationAgent: ProjectionAgent<Map<string, TransportReservation>, TransportReservationEvent>
      Initialize: unit -> Task<unit>
      Dispose: unit -> unit }

module BoundedContext =

    let create (getLegs: GetActiveLegs) =

        // 1. Repositories
        let reservationRepo =
            createInMemoryRepository<TransportReservation, string, TransportReservationEvent> ()

        // 2. Capabilities
        let reservationCaps =
            TransportReservationApp.createCapabilities reservationRepo

        let atpCaps =
            createCapabilities getLegs defaultConfig

        // 3. Projection Agents
        let reservationAgent = TransportReservationApp.createProjectionAgent ()

        // 4. Subscriptions List
        let mutable subscriptions: IDisposable list = []

        // 5. Initialize
        let initialize () =
            task {
                // A. Seed projection from repository
                let! reservations = reservationRepo.GetAll()
                match reservations with
                | Ok list ->
                    let m =
                        list
                        |> List.map (fun r -> TransportReservationId.value r.Id, r)
                        |> Map.ofList
                    reservationAgent.SetState(m)
                | Error _ -> ()

                // B. Subscribe to reservation domain events
                let localSubs =
                    [ DomainEventBus.Subscribe<TransportReservationEvent>(fun ev ->
                          reservationAgent.Post(ev, Guid.NewGuid(), None)) ]

                // C. Invalidate itinerary cache on leg data changes
                // (In Phase 8 we wire this to MasterData TransportLeg events)
                // For Phase 6 MVP: manual invalidation via atpCaps.InvalidateCache()

                subscriptions <- localSubs
            }

        // 6. Dispose
        let dispose () =
            for sub in subscriptions do sub.Dispose()
            subscriptions <- []

        { Atp               = atpCaps
          Reservation        = reservationCaps
          ReservationAgent   = reservationAgent
          Initialize         = initialize
          Dispose            = dispose }
