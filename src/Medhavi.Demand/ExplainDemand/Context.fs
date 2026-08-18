/// CA-D-009 — Explain Demand Composition Root
module Medhavi.Demand.ExplainDemand.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Medhavi.Demand.ExplainDemand.DemandExplanation.Model
open Medhavi.Demand.ExplainDemand.DemandExplanation.Policies
open Medhavi.Demand.ExplainDemand.DemandExplanation.Projections
open Medhavi.Demand.ExplainDemand.Workflows

/// Public context exposing commands, queries, and lifecycle management for Explain Demand
type DemandExplanationContext =
    { Commands: DemandExplanationApi
      Queries: QueryService<DemandExplanationDto, DemandExplanationId>
      Dispose: unit -> unit }

/// Creates the complete Explain Demand context with all dependencies wired
let create
    (repo: Repository<DemandExplanation, DemandExplanationId, DemandExplanationEvent>)
    (deps: EnvelopeStoreDependencies<DemandExplanationEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (createQueryService: CreateQueryService<DemandExplanationEvent, DemandExplanationDto, DemandExplanationId>)
    (policy: ExplanationGovernancePolicy)
    : TaskResult<DemandExplanationContext, ApplicationError> =
    taskResult {

        // 1. Seed projection read model from repository
        let! aggregates =
            repo.GetAll()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let state = seedFromAggregates aggregates

        let validEventTypes =
            [ ArsIdentifiers.EnterpriseEvents.demandExplanationEstablished.Id ]
            |> EnvelopeFilter.EventTypes

        // 2. Initialize projection query service
        let! queryCtx: ProjectionContext<DemandExplanationDto, DemandExplanationId> =
            createQueryService apply validEventTypes state "DemandExplanation"

        // 3. Instantiate child aggregate API
        let aggregateApi =
            DemandExplanation.Capabilities.create
                repo
                publishKnowledge
                policy
                deps

        // 4. Instantiate parent capability API
        let commands = Capabilities.create aggregateApi deps.DispatchEnvelope

        // 5. Wire workflow
        let workflowDeps: DemandExplanationWorkflowDependencies =
            { Subscribe = deps.Subscribe
              ExplanationApi = commands }

        let! workflowSub: IDisposable =
            createDemandExplanationWorkflow workflowDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError (fun err ->
                Infrastructure(EventStore($"Failed to create demand explanation workflow: {err}")))

        // 6. Lifecycle disposables
        let dispose () =
            queryCtx.Dispose()
            workflowSub.Dispose()

        return
            { Commands = commands
              Queries = queryCtx.QueryService
              Dispose = dispose }
    }
