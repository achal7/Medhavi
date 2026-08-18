/// CA-D-005 — Classify Demand Composition Root
module Medhavi.Demand.ClassifyDemand.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Medhavi.Demand.ClassifyDemand.DemandBehaviorAssignment.Model
open Medhavi.Demand.ClassifyDemand.DemandBehaviorAssignment.Policies
open Medhavi.Demand.ClassifyDemand.DemandBehaviorAssignment.Projections
open Medhavi.Demand.ClassifyDemand.Workflows

/// Public context exposing commands, queries, and lifecycle management for Classify Demand
type DemandBehaviorClassificationContext =
    { Commands: DemandBehaviorClassificationApi
      Queries: QueryService<DemandBehaviorAssignmentDto, DemandBehaviorAssignmentId>
      Dispose: unit -> unit }

/// Creates the complete Classify Demand context with all dependencies wired
let create
    (repo: Repository<DemandBehaviorAssignment, DemandBehaviorAssignmentId, DemandBehaviorEvent>)
    (deps: EnvelopeStoreDependencies<DemandBehaviorEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (createQueryService: CreateQueryService<DemandBehaviorEvent, DemandBehaviorAssignmentDto, DemandBehaviorAssignmentId>)
    (policy: ClassificationPolicy)
    (overridePolicy: ClassificationOverridePolicy)
    (defaultLocationId: string)
    : TaskResult<DemandBehaviorClassificationContext, ApplicationError> =
    taskResult {

        // 1. Seed projection read model from repository
        let! aggregates =
            repo.GetAll()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let state = seedFromAggregates aggregates

        let validEventTypes =
            [ ArsIdentifiers.EnterpriseEvents.demandBehaviorClassificationChanged.Id ]
            |> EnvelopeFilter.EventTypes

        // 2. Initialize projection query service
        let! queryCtx: ProjectionContext<DemandBehaviorAssignmentDto, DemandBehaviorAssignmentId> =
            createQueryService apply validEventTypes state "DemandBehaviorAssignment"

        // 3. Instantiate child aggregate API
        let aggregateApi =
            DemandBehaviorAssignment.Capabilities.create
                repo
                publishKnowledge
                policy
                overridePolicy
                deps

        // 4. Instantiate parent capability API
        let commands = Medhavi.Demand.ClassifyDemand.Capabilities.create aggregateApi deps.DispatchEnvelope

        // 5. Wire workflow
        let workflowDeps: DemandBehaviorClassificationWorkflowDependencies =
            { Codec = Medhavi.Foundation.Codec.json
              Subscribe = deps.Subscribe
              ClassificationApi = commands
              DefaultLocationId = defaultLocationId }

        let! workflowSub: IDisposable =
            createDemandBehaviorClassificationWorkflow workflowDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError (fun err ->
                Infrastructure(EventStore($"Failed to create demand behavior classification workflow: {err}")))

        // 6. Lifecycle disposables
        let dispose () =
            queryCtx.Dispose()
            workflowSub.Dispose()

        return
            { Commands = commands
              Queries = queryCtx.QueryService
              Dispose = dispose }
    }
