module Medhavi.Demand.PlanningPriorityAssignment.Context

open System
open Medhavi.Contracts.Demand.PlanningPriorityAssignment
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Observation
open Medhavi.Demand.PlanningPriorityAssignment.Model
open Medhavi.Demand.PlanningPriorityAssignment.Projection
open Medhavi.Demand.PlanningPriorityAssignment.Capabilities
open Medhavi.Demand.PlanningPriorityAssignment.CommandHandler

// Traceability: Exposes Bounded Context entry point for SE-D-038 (Planning Priority Assignment)
// Wires internal CommandHandler and public Capabilities. Exposes Commands : Api and Queries : QueryService.

type PlanningPriorityAssignmentContext =
    { Commands: PlanningPriorityAssignmentApi
      Queries: PlanningPriorityQueries
      Agent: PriorityAgent
      Dispose: unit -> unit }

let create
    (repo: Repository<PlanningPriorityAssignment, string, PlanningPriorityEvent>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    : PlanningPriorityAssignmentContext =

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

        let sub = DomainEventBus.Subscribe<PlanningPriorityEvent>(fun ev -> agent.Post(ev, Guid.NewGuid(), None))
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
