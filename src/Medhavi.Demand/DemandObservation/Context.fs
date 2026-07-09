module Medhavi.Demand.DemandObservation.Context

open System
open Medhavi.Contracts.Demand.DemandObservation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Observation
open Medhavi.Demand.DemandObservation.Model
open Medhavi.Demand.DemandObservation.Projection
open Medhavi.Demand.DemandObservation.Capabilities
open Medhavi.Demand.DemandObservation.CommandHandler

// Traceability: Exposes the Bounded Context entry point for SE-D-001 (Demand Observation)
// Wires the internal CommandHandler and public Capabilities. Exposes Commands : Api and Queries : QueryService.

type ObservationContext =
    { Commands: DemandObservationApi
      Queries: DemandObservationQuries
      Agent: ObservationAgent
      Dispose: unit -> unit }

let create
    (repo: Repository<DemandObservation, string, ObservationEvent>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    : ObservationContext =

    // 1. Internal Command Execution Corridor
    let executeCmd = execute repo publishKnowledge

    // 2. Read-Model Projections & Queries
    let agent = createProjectionAgent()
    let queries = createQueryService agent
    
    // 3. Public Workflow API Capabilities (Command Gateway)
    let api = createCapabilities executeCmd repo publishKnowledge

    // 4. Subscriptions for Event Sourcing
    let mutable subscriptions: IDisposable list = []

    task {
        let! demands = repo.GetAll()
        demands |> Result.map(seedProjections agent) |> ignore

        let sub = DomainEventBus.Subscribe<ObservationEvent>(fun ev -> agent.Post(ev, Guid.NewGuid(), None))
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
