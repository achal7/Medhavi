module Medhavi.Demand.DemandObservationContext

open System
open System.Threading.Tasks
open Medhavi.Contracts
open Medhavi.Contracts.Demand.DemandObservation
open Medhavi.Demand.Domain.DemandObservationAgg
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Execution.ExecutionApiBridge
open Medhavi.SharedKernel.Observation
open Medhavi.Demand.Application.DemandObservationProjection

type ObservationApi = {
    Establish : EstablishObservationReq -> Task<Result<string, ApiError>>
    Evaluate  : EvaluateObservationReq  -> Task<Result<string, ApiError>>
    GetById   : string -> Task<DemandObservation option>
    GetAll    : unit -> Task<DemandObservation list>
}

type ObservationContext = {
    Commands : ObservationApi
    Queries  : DemandObservationQuries
    Agent    : ObservationAgent
    Dispose  : unit -> unit
}

let create (repo: Repository<DemandObservation, string, ObservationEvent>)
           (publishKnowledge: ArchitecturalKnowledge -> unit) =

    // 1. Capabilities
    let capabilities = ObservationCapabilities.create repo publishKnowledge

    // 2. Projection Agent
    let agent = createProjectionAgent()

    // 3. Queries
    let queries = createQueryService agent

    // 4. API
    let api : ObservationApi = {
        Establish = fun req ->
            task {
                let! o = capabilities.Establish req
                return toApiResult o |> Result.map (fun obs -> DemandObservationId.value obs.Id)
            }
        Evaluate = fun req ->
            task {
                let! o = capabilities.Evaluate req
                return toApiResult o |> Result.map (fun obs -> DemandObservationId.value obs.Id)
            }
        GetById = fun id -> queries.GetById id
        GetAll  = fun () -> queries.GetAll()
    }

    // 5. Subscriptions
    let mutable subscriptions : IDisposable list = []

    task {
        let! demands = repo.GetAll()
        demands |> Result.map (seedProjections agent) |> ignore

        let sub = DomainEventBus.Subscribe<ObservationEvent>(fun ev ->
            agent.Post(ev, Guid.NewGuid(), None))
        subscriptions <- [sub]
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously

    let dispose () =
        for sub in subscriptions do
            sub.Dispose()
        subscriptions <- []

    { Commands = api
      Queries  = queries
      Agent    = agent
      Dispose  = dispose }
