module Medhavi.Demand.DemandPlanningCondition.Context

open System
open Medhavi.Contracts.Demand.DemandLearning
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Observation
open Medhavi.Demand.DemandPlanningCondition.Model
open Medhavi.Demand.DemandPlanningCondition.Projection
open Medhavi.Demand.DemandPlanningCondition.Capabilities
open Medhavi.Demand

type DemandPlanningConditionContext =
    { Commands: DemandPlanningConditionApi
      Queries: DemandPlanningConditionQueries
      Agent: ConditionAgent
      Dispose: unit -> unit }

let create
    (repo: Repository<DemandPlanningCondition, string, DemandPlanningConditionEvent>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    : DemandPlanningConditionContext =

    let executeCmd = CommandHandler.execute repo publishKnowledge
    let agent = createProjectionAgent()
    let queries = createQueryService agent
    let api = createCapabilities executeCmd

    let mutable subscriptions: IDisposable list = []

    task {
        let! all = repo.GetAll()

        match all with
        | Ok conditions ->
            let map =
                conditions |> List.map(fun c -> DemandPlanningConditionId.value c.Id, mapToContract c) |> Map.ofList

            agent.SetState map
        | _ -> ()

        let sub = DomainEventBus.Subscribe<DemandPlanningConditionEvent>(fun ev -> agent.Post(ev, Guid.NewGuid(), None))
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
