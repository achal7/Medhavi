module Medhavi.Demand.DemandBehaviourAssessment.Context

open System
open System.Threading.Tasks
open Medhavi.Contracts.Demand.SenseDemand
open Medhavi.Contracts.Demand.ForecastPublication
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Observation
open Medhavi.Demand.DemandBehaviourAssessment.Model
open Medhavi.Demand.DemandBehaviourAssessment.Projection
open Medhavi.Demand.DemandBehaviourAssessment.Capabilities
open Medhavi.Demand.DemandBehaviourAssessment.CommandHandler

// Traceability: Exposes Bounded Context entry point for SE-D-037 (Demand Behaviour Assessment aggregate)
// Wires internal CommandHandler and public Capabilities. Exposes Commands : Api and Queries : QueryService.

type DemandBehaviourAssessmentContext =
    { Commands: SenseDemandApi
      Queries: SenseDemandQueries
      Agent: AssessmentAgent
      Dispose: unit -> unit }

let create
    (repo: Repository<DemandBehaviourAssessment, string, DemandBehaviourAssessmentEvent>)
    (isHighPriority: SkuId -> Task<bool>)
    (forecastQueries: ForecastPublicationQueries)
    (forecastApi: ForecastPublicationApi)
    (getScopeId: SkuId -> StockingPointId -> Task<string option>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    : DemandBehaviourAssessmentContext =

    // 1. Internal Command Execution Corridor
    let executeCmd = execute repo publishKnowledge

    // 2. Read-Model Projections & Queries
    let agent = createProjectionAgent()
    let queries = createQueryService agent
    
    // 3. Public Workflow API Capabilities (Command Gateway)
    let api = createCapabilities executeCmd isHighPriority forecastQueries forecastApi getScopeId

    // 4. Subscriptions for Event Sourcing
    let mutable subscriptions: IDisposable list = []

    task {
        let! assessments = repo.GetAll()
        assessments |> Result.map(seedProjections agent) |> ignore

        let sub =
            DomainEventBus.Subscribe<DemandBehaviourAssessmentEvent>(fun ev -> agent.Post(ev, Guid.NewGuid(), None))

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
