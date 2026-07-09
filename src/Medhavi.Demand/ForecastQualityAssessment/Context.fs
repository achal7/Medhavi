module Medhavi.Demand.ForecastQualityAssessment.Context

open System
open Medhavi.Contracts.Demand.ForecastQualityAssessment
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Observation
open Medhavi.Demand.ForecastQualityAssessment.Model
open Medhavi.Demand.ForecastQualityAssessment.Projection
open Medhavi.Demand.ForecastQualityAssessment.CommandHandler
open Medhavi.Demand.ForecastQualityAssessment.Capabilities
open Medhavi.Demand
open System.Threading.Tasks

type ForecastQualityAssessmentContext =
    { Commands: ForecastQualityApi
      Queries: ForecastQualityAssessmentQueries
      Agent: AssessmentAgent
      Dispose: unit -> unit }

let create
    (repo: Repository<ForecastQualityAssessment, string, ForecastQualityAssessmentEvent>)
    (getActuals: PlanningScopeId -> Timestamp -> Timestamp -> Task<decimal list>)
    (getForecasts: PlanningScopeId -> Timestamp -> Timestamp -> Task<decimal list>)
    (getNaiveForecasts: PlanningScopeId -> Timestamp -> Timestamp -> Task<decimal list option>)
    (getOverrideHistory: PlanningScopeId -> Timestamp -> Timestamp -> Task<(decimal * decimal) list>)
    (getHistoricalForecasts: PlanningScopeId -> int -> Task<decimal list list>)
    (policyThreshold: decimal)
    (policyMinPeriod: int)
    (policyWeights: CoreMetrics)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    : ForecastQualityAssessmentContext =

    // internal command executor
    let executeCmd = CommandHandler.execute repo publishKnowledge

    let agent = createProjectionAgent()
    let queries = createQueryService agent

    let api =
        createCapabilities
            executeCmd
            getActuals
            getForecasts
            getNaiveForecasts
            getOverrideHistory
            getHistoricalForecasts
            policyThreshold
            policyMinPeriod
            policyWeights

    // subscriptions & seed
    let mutable subscriptions: IDisposable list = []

    task {
        let! all = repo.GetAll()

        match all with
        | Ok assessments ->
            let map =
                assessments |> List.map(fun a -> ForecastQualityAssessmentId.value a.Id, mapToContract a) |> Map.ofList

            agent.SetState map
        | _ -> ()

        let sub =
            DomainEventBus.Subscribe<ForecastQualityAssessmentEvent>(fun ev -> agent.Post(ev, Guid.NewGuid(), None))

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
