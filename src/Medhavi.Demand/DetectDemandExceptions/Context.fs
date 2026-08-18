/// CA-D-008 — Detect Demand Exceptions Composition Root
module Medhavi.Demand.DetectDemandExceptions.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Medhavi.Demand.DetectDemandExceptions.DemandExceptionEvidence.Model
open Medhavi.Demand.DetectDemandExceptions.DemandExceptionEvidence.Policies
open Medhavi.Demand.DetectDemandExceptions.DemandExceptionEvidence.Projections
open Medhavi.Demand.DetectDemandExceptions.Workflows

/// Public context exposing commands, queries, and lifecycle management for Detect Demand Exceptions
type DemandExceptionContext =
    { Commands: DemandExceptionApi
      Queries: QueryService<DemandExceptionEvidenceDto, DemandExceptionEvidenceId>
      Dispose: unit -> unit }

/// Creates the complete Detect Demand Exceptions context with all dependencies wired
let create
    (repo: Repository<DemandExceptionEvidenceAggregate, DemandExceptionEvidenceId, DemandExceptionEvent>)
    (deps: EnvelopeStoreDependencies<DemandExceptionEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (createQueryService: CreateQueryService<DemandExceptionEvent, DemandExceptionEvidenceDto, DemandExceptionEvidenceId>)
    (policy: DemandExceptionEvidencePolicy)
    : TaskResult<DemandExceptionContext, ApplicationError> =
    taskResult {

        // 1. Seed projection read model from repository
        let! aggregates =
            repo.GetAll()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let state = seedFromAggregates aggregates

        let validEventTypes =
            [ ArsIdentifiers.EnterpriseEvents.demandExceptionEvidenceEvaluated.Id ]
            |> EnvelopeFilter.EventTypes

        // 2. Initialize projection query service
        let! queryCtx: ProjectionContext<DemandExceptionEvidenceDto, DemandExceptionEvidenceId> =
            createQueryService apply validEventTypes state "DemandExceptionEvidence"

        // 3. Instantiate child aggregate API
        let aggregateApi =
            DemandExceptionEvidence.Capabilities.create
                repo
                publishKnowledge
                policy
                deps

        // 4. Instantiate parent capability API
        let commands = Capabilities.create aggregateApi deps.DispatchEnvelope

        // 5. Wire workflow
        let workflowDeps: DemandExceptionWorkflowDependencies =
            { Subscribe = deps.Subscribe
              ExceptionApi = commands }

        let! workflowSub: IDisposable =
            createDemandExceptionWorkflow workflowDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError (fun err ->
                Infrastructure(EventStore($"Failed to create demand exception workflow: {err}")))

        // 6. Lifecycle disposables
        let dispose () =
            queryCtx.Dispose()
            workflowSub.Dispose()

        return
            { Commands = commands
              Queries = queryCtx.QueryService
              Dispose = dispose }
    }
