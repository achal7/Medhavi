namespace Medhavi.Demand

open System
open System.Threading.Tasks
open Medhavi.Contracts
open Medhavi.SharedKernel
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.Infrastructure.Projections
open Medhavi.Demand.Domain
open Medhavi.Demand.Domain.DemandLineAgg
open Medhavi.Demand.Projections
open Medhavi.Demand.Application

type DemandContext =
    { Commands: Demand.DemandLineApi
      Queries: Demand.DemandLineQueries
      DemandAgent: ProjectionAgent<Map<string, Demand.DemandLine>, DemandLineEvent>
      Initialize: unit -> Task<unit>
      Dispose: unit -> unit }

module BoundedContext =

    let create () =
        // 1. Repositories
        let demandLineRepo = InMemRepository.createInMemoryRepository<DemandLine, string, DemandLineEvent> ()

        // 2. Capabilities
        let capabilities = createCapabilities demandLineRepo

        // 3. Projection Agents
        let demandAgent = createProjectionAgent ()

        // 4. APIs
        let demandApi = createDemandLineApi capabilities

        // 5. Subscriptions List
        let mutable subscriptions: IDisposable list = []

        // 6. Initialize
        let initialize () =
            task {
                // A. Seed projection from repository
                let! demands = demandLineRepo.GetAll()
                demands
                |> Result.map (seedProjections demandAgent)
                |> ignore

                // B. Subscribe to domain events
                let localSubs =
                    [ DomainEventBus.Subscribe<DemandLineEvent>(fun ev -> demandAgent.Post(ev, Guid.NewGuid(), None)) ]

                subscriptions <- localSubs
            }

        // 7. Dispose
        let dispose () =
            for sub in subscriptions do
                sub.Dispose()

            subscriptions <- []

        { Commands = demandApi
          Queries = createDemandQueryService demandAgent
          DemandAgent = demandAgent
          Initialize = initialize
          Dispose = dispose }