/// CA-D-006 — Prioritize Demand Composition Root
module Medhavi.Demand.PrioritizeDemand.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Medhavi.Demand.PrioritizeDemand.PlanningPriorityAssignment.Model
open Medhavi.Demand.PrioritizeDemand.PlanningPriorityAssignment.Policies
open Medhavi.Demand.PrioritizeDemand.PlanningPriorityAssignment.Projections
open Medhavi.Demand.PrioritizeDemand.Workflows

/// Public context exposing commands, queries, and lifecycle management for Prioritize Demand
type PlanningPriorityContext =
    { Commands: PlanningPriorityApi
      Queries: QueryService<PlanningPriorityDto, PlanningPriorityAssignmentId>
      Dispose: unit -> unit }

/// Creates the complete Prioritize Demand context with all dependencies wired
let create
    (repo: Repository<PlanningPriorityAssignment, PlanningPriorityAssignmentId, PlanningPriorityEvent>)
    (deps: EnvelopeStoreDependencies<PlanningPriorityEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (createQueryService: CreateQueryService<PlanningPriorityEvent, PlanningPriorityDto, PlanningPriorityAssignmentId>)
    (policy: PrioritizationPolicy)
    (overridePolicy: PrioritizationOverridePolicy)
    (ports: DemandPorts)
    : TaskResult<PlanningPriorityContext, ApplicationError> =
    taskResult {

        // 1. Seed projection read model from repository
        let! aggregates =
            repo.GetAll()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let state = seedFromAggregates aggregates

        let validEventTypes =
            [ ArsIdentifiers.EnterpriseEvents.planningPriorityChanged.Id ]
            |> EnvelopeFilter.EventTypes

        // 2. Initialize projection query service
        let! queryCtx: ProjectionContext<PlanningPriorityDto, PlanningPriorityAssignmentId> =
            createQueryService apply validEventTypes state "PlanningPriorityAssignment"

        // 3. Instantiate child aggregate API
        let aggregateApi =
            PlanningPriorityAssignment.Capabilities.create
                repo
                publishKnowledge
                policy
                overridePolicy
                deps
                ports

        // 4. Instantiate parent capability API
        let commands = Capabilities.create aggregateApi ports deps.DispatchEnvelope

        // 5. Wire workflow
        let workflowDeps: PlanningPriorityWorkflowDependencies =
            { Subscribe = deps.Subscribe
              PriorityApi = commands }

        let! workflowSub: IDisposable =
            createPlanningPriorityWorkflow workflowDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError (fun err ->
                Infrastructure(EventStore($"Failed to create planning priority workflow: {err}")))

        // 6. Lifecycle disposables
        let dispose () =
            queryCtx.Dispose()
            workflowSub.Dispose()

        return
            { Commands = commands
              Queries = queryCtx.QueryService
              Dispose = dispose }
    }
