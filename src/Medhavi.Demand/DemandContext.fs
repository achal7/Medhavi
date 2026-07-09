namespace Medhavi.Demand

open System
open System.Threading.Tasks
open Medhavi.Contracts
open Medhavi.Infrastructure.Projections
open Medhavi.Demand.Domain.DemandAgg
open Medhavi.Demand.Domain.ForecastAgg
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Observation
open Medhavi.Demand.Application
open Medhavi.Demand.Application.DemandProjection

type DemandContext =
    { Commands: Demand.DemandApi
      Queries: Demand.DemandQueries
      DemandAgent: ProjectionAgent<Map<string, Demand.Demand>, DemandEvent>
      Initialize: unit -> Task<unit>
      Dispose: unit -> unit }

type ForecastContext =
    { Commands: Demand.ForecastApi
      Queries: Demand.ForecastQueries
      DemandAgent: ProjectionAgent<Map<string, Demand.Forecast>, ForecastEvent>
      Initialize: unit -> Task<unit>
      Dispose: unit -> unit }

module DemandBoundedContext =

    let create (repo: Repository<Demand, string, DemandEvent>) (publishKnowledge: ArchitecturalKnowledge -> unit) =

        // 1. Capabilities
        let capabilities = DemandCapabilities.create repo publishKnowledge //createCapabilities demandLineRepo

        // 3. Projection Agents
        let demandAgent = createProjectionAgent()

        // 4. APIs
        let demandApi = DemandCapabilities.createDemandApi capabilities

        // 5. Subscriptions List
        let mutable subscriptions: IDisposable list = []

        // 6. Initialize
        // let initialize () =
        task {
            // A. Seed projection from repository
            let! demands = repo.GetAll()
            demands |> Result.map(seedProjections demandAgent) |> ignore

            // B. Subscribe to domain events
            let localSubs =
                [ DomainEventBus.Subscribe<DemandEvent>(fun ev -> demandAgent.Post(ev, Guid.NewGuid(), None)) ]

            subscriptions <- localSubs
        }
        |> Async.AwaitTask
        |> Async.RunSynchronously

        // 7. Dispose
        let dispose () =
            for sub in subscriptions do
                sub.Dispose()

            subscriptions <- []

        { Commands = demandApi
          Queries = createDemandQueryService demandAgent
          DemandAgent = demandAgent
          Initialize = fun () -> task { return () } // no‑op
          Dispose = dispose }
