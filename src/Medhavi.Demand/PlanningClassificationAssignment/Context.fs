module Medhavi.Demand.PlanningClassificationAssignment.Context

open System
open Medhavi.Contracts.Demand.PlanningClassificationAssignment
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Observation
open Medhavi.Demand.PlanningClassificationAssignment.Model
open Medhavi.Demand.PlanningClassificationAssignment.Projection
open Medhavi.Demand.PlanningClassificationAssignment.Capabilities
open Medhavi.Demand.PlanningClassificationAssignment.CommandHandler

// Traceability: Exposes Bounded Context entry point for SE-D-036 (Planning Classification Assignment)
// Wires internal CommandHandler and public Capabilities. Exposes Commands : Api and Queries : QueryService.

type PlanningClassificationContext =
    { Commands: PlanningClassificationApi
      Queries: PlanningClassificationQueries
      Agent: ClassificationAgent
      Dispose: unit -> unit }

let create
    (repo: Repository<PlanningClassificationAssignment, string, PlanningClassificationEvent>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    : PlanningClassificationContext =

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
        let! all = repo.GetAll()
        all |> Result.map(seedProjections agent) |> ignore

        let sub = DomainEventBus.Subscribe<PlanningClassificationEvent>(fun ev -> agent.Post(ev, Guid.NewGuid(), None))
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
