/// CA-D-002 Forecast Demand Composition Root
module Medhavi.Demand.ForecastDemand.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Medhavi.Demand.ForecastDemand.ForecastPublication.Model
open Medhavi.Demand.ForecastDemand.ForecastPublication.Policies
open Medhavi.Demand.ForecastDemand.ForecastPublication.Projections
open Medhavi.Demand.ForecastDemand.Workflows

/// Public context exposing commands, queries, and lifecycle management
type ForecastDemandContext =
    { Commands: ForecastPublicationApi
      Queries: QueryService<ForecastPublicationDto, ForecastPublicationId>
      Dispose: unit -> unit }

/// Creates the complete Forecast Demand context with all dependencies wired
let create
    (repo: Repository<ForecastPublication, ForecastPublicationId, ForecastPublicationEvent>)
    (deps: EnvelopeStoreDependencies<ForecastPublicationEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (createQueryService: CreateQueryService<ForecastPublicationEvent, ForecastPublicationDto, ForecastPublicationId>)
    (modelGovernancePolicy: ForecastModelGovernancePolicy)
    (unforecastablePolicy: UnforecastableSeriesPolicy)
    (publicationGovernancePolicy: ForecastPublicationGovernancePolicy)
    (overridePolicy: ForecastOverrideAuthorizationPolicy)
    (modelParamsPolicy: ForecastModelParametersPolicy)
    (ports: DemandPorts)
    (defaultScopeId: string)
    : TaskResult<ForecastDemandContext, ApplicationError> =
    taskResult {

        // 1. Seed projection read model from repository
        let! aggregates =
            repo.GetAll()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let state = seedFromAggregates aggregates

        let validEventTypes =
            [ ArsIdentifiers.EnterpriseEvents.forecastCycleEstablished.Id
              ArsIdentifiers.EnterpriseEvents.championModelSelected.Id
              ArsIdentifiers.EnterpriseEvents.forecastProjectionProduced.Id
              ArsIdentifiers.EnterpriseEvents.forecastOverrideRecorded.Id
              ArsIdentifiers.EnterpriseEvents.forecastPublicationPublished.Id ]
            |> EnvelopeFilter.EventTypes

        // 2. Initialize projection query service
        let! queryCtx: ProjectionContext<ForecastPublicationDto, ForecastPublicationId> =
            createQueryService apply validEventTypes state "ForecastPublication"

        // 3. Instantiate child aggregate API
        let aggregateApi =
            ForecastPublication.Capabilities.create
                repo
                publishKnowledge
                modelGovernancePolicy
                unforecastablePolicy
                publicationGovernancePolicy
                overridePolicy
                modelParamsPolicy
                deps
                ports

        // 4. Instantiate parent capability API
        let commands = Capabilities.create aggregateApi ports deps.DispatchEnvelope

        // 5. Wire workflow dependencies
        let workflowDeps: CriticalDemandForecastWorkflowDependencies =
            { Codec = Medhavi.Foundation.Codec.json
              Subscribe = deps.Subscribe
              ForecastApi = commands
              DefaultScopeId = defaultScopeId }

        let! critSub: IDisposable =
            createCriticalDemandForecastWorkflow workflowDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError (fun err ->
                Infrastructure(EventStore($"Failed to create critical forecast workflow: {err}")))

        let! autoPubSub: IDisposable =
            createForecastAutoPublishWorkflow workflowDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError (fun err ->
                Infrastructure(EventStore($"Failed to create forecast auto-publish workflow: {err}")))

        // 6. Combine lifecycle disposables
        let dispose () =
            queryCtx.Dispose()
            critSub.Dispose()
            autoPubSub.Dispose()

        return
            { Commands = commands
              Queries = queryCtx.QueryService
              Dispose = dispose }
    }
