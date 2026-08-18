/// CA-D-003 Sense Demand Composition Root
module Medhavi.Demand.SenseDemand.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Medhavi.Demand.SenseDemand.DemandBehaviorAssessment.Model
open Medhavi.Demand.SenseDemand.DemandBehaviorAssessment.Policies
open Medhavi.Demand.SenseDemand.DemandBehaviorAssessment.Projections
open Medhavi.Demand.SenseDemand.Workflows

/// Public context exposing commands, queries, and lifecycle management
type SenseDemandContext =
    { Commands: DemandBehaviorAssessmentApi
      Queries: QueryService<DemandBehaviorAssessmentDto, DemandBehaviorAssessmentId>
      Dispose: unit -> unit }

/// Creates the complete Sense Demand context with all dependencies wired
let create
    (repo: Repository<DemandBehaviorAssessment, DemandBehaviorAssessmentId, DemandBehaviorEvent>)
    (deps: EnvelopeStoreDependencies<DemandBehaviorEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (createQueryService: CreateQueryService<DemandBehaviorEvent, DemandBehaviorAssessmentDto, DemandBehaviorAssessmentId>)
    (sensingPolicy: DemandSensingPolicy)
    (triggerPolicy: ForecastRefreshTriggerPolicy)
    (ports: DemandPorts)
    : TaskResult<SenseDemandContext, ApplicationError> =
    taskResult {

        // 1. Seed projection read model from repository
        let! aggregates =
            repo.GetAll()
            |> TaskResult.ofTask
            |> TaskResult.mapError(fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let state = seedFromAggregates aggregates

        let validEventTypes =
            [ ArsIdentifiers.EnterpriseEvents.demandBehaviorChanged.Id
              ArsIdentifiers.EnterpriseEvents.criticalDemandBehaviorDetected.Id ]
            |> EnvelopeFilter.EventTypes

        // 2. Initialize projection query service
        let! queryCtx: ProjectionContext<DemandBehaviorAssessmentDto, DemandBehaviorAssessmentId> =
            createQueryService apply validEventTypes state "DemandBehaviorAssessment"

        // 3. Instantiate child aggregate API
        let aggregateApi =
            DemandBehaviorAssessment.Capabilities.create
                repo
                publishKnowledge
                sensingPolicy
                triggerPolicy
                deps
                ports

        // 4. Instantiate parent capability API
        let commands = Capabilities.create aggregateApi deps.DispatchEnvelope

        // 5. Wire workflows
        let workflowDeps: CriticalBehaviorWorkflowDependencies =
            { Codec = Medhavi.Foundation.Codec.json
              ObservationCodec = Medhavi.Foundation.Codec.json
              Subscribe = deps.Subscribe
              CapabilityApi = commands }

        let! critSub: IDisposable =
            createCriticalBehaviorWorkflow workflowDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError(fun err ->
                Infrastructure(EventStore($"Failed to create critical demand behavior workflow: {err}")))

        let! obsSub: IDisposable =
            createDemandObservationSensingWorkflow workflowDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError(fun err ->
                Infrastructure(EventStore($"Failed to create observation sensing workflow: {err}")))

        // 6. Combine lifecycle disposables
        let dispose () =
            queryCtx.Dispose()
            critSub.Dispose()
            obsSub.Dispose()

        return
            { Commands = commands
              Queries = queryCtx.QueryService
              Dispose = dispose }
    }
