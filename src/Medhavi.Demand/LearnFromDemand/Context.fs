/// CA-D-010 Learn From Demand Composition Root
module Medhavi.Demand.LearnFromDemand.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Medhavi.Demand.LearnFromDemand.DemandLearning.Model
open Medhavi.Demand.LearnFromDemand.DemandLearning.Policies
open Medhavi.Demand.LearnFromDemand.DemandLearning.Projections
open Medhavi.Demand.LearnFromDemand.Workflows

/// Public context exposing commands, queries, and lifecycle management
type DemandLearningContext =
    { Commands: DemandLearningApi
      Queries: QueryService<DemandLearningDto, DemandLearningId>
      Dispose: unit -> unit }

/// Creates the complete Learn From Demand context with all dependencies wired
let create
    (repo: Repository<DemandLearning, DemandLearningId, DemandLearningEvent>)
    (deps: EnvelopeStoreDependencies<DemandLearningEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (createQueryService: CreateQueryService<DemandLearningEvent, DemandLearningDto, DemandLearningId>)
    (policy: LearningAnalysisPolicy)
    (ports: DemandPorts)
    : TaskResult<DemandLearningContext, ApplicationError> =
    taskResult {

        // 1. Seed projection read model from repository
        let! aggregates =
            repo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let state = seedFromAggregates aggregates

        let validEventTypes =
            [ ArsIdentifiers.EnterpriseEvents.demandLearningEstablished.Id ]
            |> EnvelopeFilter.EventTypes

        // 2. Initialize projection query service
        let! queryCtx: ProjectionContext<DemandLearningDto, DemandLearningId> =
            createQueryService apply validEventTypes state "DemandLearning"

        // 3. Instantiate child aggregate API
        let aggregateApi =
            DemandLearning.Capabilities.create
                repo
                publishKnowledge
                policy
                deps
                ports

        // 4. Instantiate parent capability API
        let commands = Capabilities.create aggregateApi deps.DispatchEnvelope

        // 5. Wire FS-D-017: Establish Demand Learning workflow
        let workflowDeps: DemandLearningWorkflowDependencies =
            { Subscribe = deps.Subscribe
              LearningApi = commands }

        let! workflowSubscription: IDisposable =
            createDemandLearningWorkflow workflowDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError (fun err ->
                Infrastructure(EventStore($"Failed to create demand learning workflow: {err}")))

        // 6. Combine lifecycle disposables
        let dispose () =
            queryCtx.Dispose ()
            workflowSubscription.Dispose ()

        return
            { Commands = commands
              Queries = queryCtx.QueryService
              Dispose = dispose }
    }
