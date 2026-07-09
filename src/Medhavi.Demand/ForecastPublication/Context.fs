module Medhavi.Demand.ForecastPublication.Context

open System
open Medhavi.Contracts.Demand.ForecastPublication
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Observation
open Medhavi.Demand.ForecastPublication.Model
open Medhavi.Demand.ForecastPublication.Projection
open Medhavi.Demand.ForecastPublication.Capabilities
open Medhavi.Demand.ForecastPublication.CommandHandler

// Traceability: Exposes Bounded Context entry point for SE-D-035 (Forecast Publication)
// Wires internal CommandHandler and public Capabilities. Exposes Commands : Api and Queries : QueryService.

type ForecastPublicationContext =
    { Commands: ForecastPublicationApi
      Queries: ForecastPublicationQueries
      Agent: ForecastPublicationAgent
      Dispose: unit -> unit }

let create
    (repo: Repository<ForecastPublication, string, ForecastPublicationEvent>)
    (computationService: ComputationService.Service)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    : ForecastPublicationContext =

    // 1. Internal Command Execution Corridor
    let executeCmd = execute repo publishKnowledge

    // 2. Read-Model Projections & Queries
    let agent = createProjectionAgent()
    let queries = createQueryService agent
    
    // 3. Public Workflow API Capabilities (Command Gateway)
    let api = createCapabilities executeCmd computationService queries

    // 4. Subscriptions for Event Sourcing
    let mutable subscriptions: IDisposable list = []

    task {
        let! publications = repo.GetAll()
        publications |> Result.map(seedProjections agent) |> ignore

        let sub = DomainEventBus.Subscribe<ForecastPublicationEvent>(fun ev -> agent.Post(ev, Guid.NewGuid(), None))
        subscriptions <- [ sub ]
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously

    let dispose () =
        for sub in subscriptions do
            sub.Dispose()

        subscriptions <- []

    { Commands = api
      Queries = queries
      Agent = agent
      Dispose = dispose }
