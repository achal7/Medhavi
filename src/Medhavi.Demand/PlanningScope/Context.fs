module Medhavi.Demand.PlanningScope.Context

open System
open Medhavi.Contracts.Demand.PlanningScope
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Observation
open Medhavi.Demand.PlanningScope.Model
open Medhavi.Demand.PlanningScope.Projection
open Medhavi.Demand.PlanningScope.Capabilities
open Medhavi.Demand.PlanningScope.CommandHandler

// Traceability: Exposes the Bounded Context entry point for SE-D-002 (Planning Scope)
// Wires the internal CommandHandler and public Capabilities. Exposes Commands : Api and Queries : QueryService.

type PlanningScopeContext =
    { Commands: PlanningScopeApi
      Queries: PlanningScopeQueries
      Agent: PlanningScopeAgent
      Dispose: unit -> unit }

let create
    (repo: Repository<PlanningScope, string, PlanningScopeEvent>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    : PlanningScopeContext =

    // 1. Internal Command Execution Corridor
    let executeCmd = execute repo publishKnowledge

    // 2. Read-Model Projections & Queries
    let agent = createProjectionAgent()
    let queries = createQueryService agent
    
    // 3. Public Workflow API Capabilities (Command Gateway)
    let api = createCapabilities executeCmd

    // 4. Subscriptions for Event Sourcing
    let mutable subscriptions: IDisposable list = []

    task {
        let! scopes = repo.GetAll()
        scopes |> Result.map(seedProjections agent) |> ignore

        let sub = DomainEventBus.Subscribe<PlanningScopeEvent>(fun ev -> agent.Post(ev, Guid.NewGuid(), None))
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
