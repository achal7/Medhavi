module Medhavi.Demand.DemandBehaviourAssignment.Context

open System
open Medhavi.Contracts.Demand.DemandBehaviourAssignment
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Observation
open Medhavi.Demand.DemandBehaviourAssignment.Model
open Medhavi.Demand.DemandBehaviourAssignment.Projection
open Medhavi.Demand.DemandBehaviourAssignment.Capabilities
open Medhavi.Demand.DemandBehaviourAssignment.CommandHandler

// Traceability: Exposes Bounded Context entry point for SE-D-037 (Demand Behaviour Assignment)
// Wires internal CommandHandler and public Capabilities. Exposes Commands : Api and Queries : QueryService.

type DemandBehaviourAssignmentContext =
    { Commands: DemandBehaviourAssignmentApi
      Queries: DemandBehaviourAssignmentQueries
      Agent: BehaviourAgent
      Dispose: unit -> unit }

let create
    (repo: Repository<DemandBehaviourAssignment, string, DemandBehaviourAssignmentEvent>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    : DemandBehaviourAssignmentContext =

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

        let sub =
            DomainEventBus.Subscribe<DemandBehaviourAssignmentEvent>(fun ev -> agent.Post(ev, Guid.NewGuid(), None))

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
