module Medhavi.Demand.EnterpriseDemandPicture.Context

open System
open System.Threading.Tasks
open Medhavi.Contracts
open Medhavi.Contracts.Demand.Edp
open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Observation
open Medhavi.Demand.EnterpriseDemandPicture.Model
open Medhavi.Demand.EnterpriseDemandPicture.Projection
open Medhavi.Demand.EnterpriseDemandPicture.Capabilities
open Medhavi.Demand.EnterpriseDemandPicture.CommandHandler

// Traceability: Exposes Bounded Context entry point for SE-D-003 (Enterprise Demand Picture)
// Wires internal CommandHandler and public Capabilities. Exposes Commands : Api and Queries : QueryService.

type EdpContext =
    { Commands: EnterpriseDemandPictureApi
      Queries: EnterpriseDemandPictureQueries
      Agent: EdpProjectionAgent
      Dispose: unit -> unit }

let create
    (repo: Repository<EnterpriseDemandPicture, string, EdpEvent>)
    (getAdjustments: PlanningScopeId -> Task<Map<PlanningPeriod, Quantity>>)
    (getOverrides: PlanningScopeId -> Task<Map<PlanningPeriod, Quantity>>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    : EdpContext =

    // 1. Internal Command Execution Corridor
    let executeCmd = execute repo publishKnowledge

    // 2. Read-Model Projections & Queries
    let agent = createProjectionAgent()
    let queries = createQueryService agent

    // 3. Public Workflow API Capabilities (Command Gateway)
    let api = createCapabilities executeCmd getAdjustments getOverrides

    // 4. Subscriptions for Event Sourcing
    let mutable subscriptions: IDisposable list = []

    task {
        let! edps = repo.GetAll()
        edps |> Result.map(seedProjections agent) |> ignore

        let sub = DomainEventBus.Subscribe<EdpEvent>(fun ev -> agent.Post(ev, Guid.NewGuid(), None))
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
