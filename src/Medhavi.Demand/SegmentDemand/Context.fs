/// CA-D-004 — Segment Demand Composition Root
module Medhavi.Demand.SegmentDemand.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Medhavi.Demand.SegmentDemand.PlanningClassificationAssignment.Model
open Medhavi.Demand.SegmentDemand.PlanningClassificationAssignment.Policies
open Medhavi.Demand.SegmentDemand.PlanningClassificationAssignment.Projections
open Medhavi.Demand.SegmentDemand.Workflows

/// Public context exposing commands, queries, and lifecycle management for Segment Demand
type PlanningClassificationContext =
    { Commands: PlanningClassificationApi
      Queries: QueryService<PlanningClassificationDto, PlanningClassificationAssignmentId>
      Dispose: unit -> unit }

/// Creates the complete Segment Demand context with all dependencies wired
let create
    (repo: Repository<PlanningClassificationAssignment, PlanningClassificationAssignmentId, PlanningClassificationEvent>)
    (deps: EnvelopeStoreDependencies<PlanningClassificationEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (createQueryService: CreateQueryService<PlanningClassificationEvent, PlanningClassificationDto, PlanningClassificationAssignmentId>)
    (policy: SegmentationPolicy)
    (overridePolicy: SegmentationOverridePolicy)
    (ports: DemandPorts)
    : TaskResult<PlanningClassificationContext, ApplicationError> =
    taskResult {

        // 1. Seed projection read model from repository
        let! aggregates =
            repo.GetAll()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let state = seedFromAggregates aggregates

        let validEventTypes =
            [ ArsIdentifiers.EnterpriseEvents.planningClassificationChanged.Id ]
            |> EnvelopeFilter.EventTypes

        // 2. Initialize projection query service
        let! queryCtx: ProjectionContext<PlanningClassificationDto, PlanningClassificationAssignmentId> =
            createQueryService apply validEventTypes state "PlanningClassification"

        // 3. Instantiate child aggregate API
        let aggregateApi =
            PlanningClassificationAssignment.Capabilities.create
                repo
                publishKnowledge
                policy
                overridePolicy
                deps
                ports

        // 4. Instantiate parent capability API
        let commands = Capabilities.create aggregateApi deps.DispatchEnvelope

        // 5. Wire workflow
        let workflowDeps: PlanningClassificationWorkflowDependencies =
            { Codec = Medhavi.Foundation.Codec.json
              Subscribe = deps.Subscribe
              ClassificationApi = commands }

        let! workflowSub: IDisposable =
            createPlanningClassificationWorkflow workflowDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError (fun err ->
                Infrastructure(EventStore($"Failed to create planning classification workflow: {err}")))

        // 6. Lifecycle disposables
        let dispose () =
            queryCtx.Dispose()
            workflowSub.Dispose()

        return
            { Commands = commands
              Queries = queryCtx.QueryService
              Dispose = dispose }
    }
