module Medhavi.Demand.DemandExplanation.Context

open System
open Medhavi.Contracts.Demand.DemandLearning
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Contracts.DecisionTrace
open Medhavi.SharedKernel.Observation
open Medhavi.Demand.DemandExplanation.Model
open Medhavi.Demand.DemandExplanation.Projection
open Medhavi.Demand.DemandExplanation.Capabilities
open Medhavi.Demand
open System.Threading.Tasks

type DemandExplanationContext =
    { Commands: DemandExplanationApi
      Queries: DemandExplanationQueries
      Agent: ExplanationAgent
      Dispose: unit -> unit }

let create
    (repo: Repository<DemandExplanation, string, DemandExplanationEvent>)
    (getDecisionTraces: string -> string -> Task<DecisionTrace list>)
    (getSourceArtifactRefs: string -> string -> Task<ExplanationSourceArtifactRef list>)
    (templateVersionRef: string)
    (generateNaturalLanguage: NaturalLanguageGenerator)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    : DemandExplanationContext =

    let executeCmd = CommandHandler.execute repo publishKnowledge
    let agent = createProjectionAgent()
    let queries = createQueryService agent

    let api = createCapabilities executeCmd getDecisionTraces getSourceArtifactRefs templateVersionRef generateNaturalLanguage

    let mutable subscriptions: IDisposable list = []

    task {
        let! all = repo.GetAll()

        match all with
        | Ok explanations ->
            let map = explanations |> List.map(fun e -> DemandExplanationId.value e.Id, mapToContract e) |> Map.ofList
            agent.SetState map
        | _ -> ()

        let sub = DomainEventBus.Subscribe<DemandExplanationEvent>(fun ev -> agent.Post(ev, Guid.NewGuid(), None))
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
