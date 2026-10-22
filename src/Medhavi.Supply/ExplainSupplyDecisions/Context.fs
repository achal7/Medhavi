/// CA-S-012 Explain Supply Decisions Composition Root
module Medhavi.Supply.ExplainSupplyDecisions.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Medhavi.Supply.ExplainSupplyDecisions.SupplyExplanation.Model
open Medhavi.Supply.ExplainSupplyDecisions.SupplyExplanation.Policies
open Medhavi.Supply.ExplainSupplyDecisions.SupplyExplanation.Projections
open Medhavi.Supply.ExplainSupplyDecisions.Workflows
open Capabilities

/// Public context exposing commands, queries, and lifecycle management for Explain Supply Decisions
type ExplainSupplyDecisionsContext =
    {
        Commands: ExplainSupplyDecisionsApis
        Queries: QueryService<SupplyExplanationDto, SupplyExplanationId>
        Dispose: unit -> unit
    }

/// Creates the complete Explain Supply Decisions context with all dependencies wired
let create
    (repo: Repository<SupplyExplanation, SupplyExplanationId, SupplyExplanationEvent>)
    (deps: EnvelopeStoreDependencies<SupplyExplanationEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (createQueryService: CreateQueryService<SupplyExplanationEvent, SupplyExplanationDto, SupplyExplanationId>)
    (policy: ExplanationPolicy)
    (ports: ExplainSupplyPorts)
    (dispatchNotification: SupplyExplanationNotification -> Tasks.Task<unit>)
    : TaskResult<ExplainSupplyDecisionsContext, ApplicationError> =
    taskResult {

        // 1. Seed projection read model from repository aggregates
        let! aggregates =
            repo.GetAll()
            |> TaskResult.ofTask
            |> TaskResult.mapError(fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let state = seedFromAggregates aggregates

        let validEventTypes =
            [ ArsIdentifiers.EnterpriseEvents.supplyExplanationRecorded.Id ] |> EnvelopeFilter.EventTypes

        // 2. Initialize projection query service
        let! queryCtx: ProjectionContext<SupplyExplanationDto, SupplyExplanationId> =
            createQueryService apply validEventTypes state "SupplyExplanation"

        // 3. Instantiate child aggregate API
        let aggregateApi = SupplyExplanation.Capabilities.create repo publishKnowledge policy deps

        // 4. Instantiate parent capability API
        let commands = Capabilities.create aggregateApi ports

        // 5. Wire automated workflows and notification node
        let workflowDeps: SupplyExplanationWorkflowDependencies =
            {
                Subscribe = deps.Subscribe
                DispatchNotification = dispatchNotification
                Apis = commands
            }

        let! (triggerSub: IDisposable) = createSupplyExplanationWorkflow workflowDeps CancellationToken.None
        let! (notificationSub: IDisposable) = createNotificationNodeWorkflow workflowDeps CancellationToken.None

        // 6. Return context with dispose hook
        let dispose () =
            queryCtx.Dispose()
            triggerSub.Dispose()
            notificationSub.Dispose()

        return
            {
                Commands = commands
                Queries = queryCtx.QueryService
                Dispose = dispose
            }
    }
