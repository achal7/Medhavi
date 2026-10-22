/// CA-S-009 — Sense Supply Changes Composition Root
module Medhavi.Supply.SenseSupplyChanges.Context

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
open SupplyChangeAssessment.Model
open SupplyChangeAssessment.Policies
open Capabilities
open Workflows

/// Public context exposing commands, queries, and lifecycle management for Sense Supply Changes
type SenseSupplyChangesContext =
    { Commands: SenseSupplyChangesApis
      Queries: QueryService<SupplyChangeAssessmentDto, SupplyChangeAssessmentId>
      Dispose: unit -> unit }

/// Creates the complete Sense Supply Changes context with all child aggregates, projections, and workflows wired
let create
    (repo: Repository<SupplyChangeAssessmentAggregate, SupplyChangeAssessmentId, SupplyChangeEvent>)
    (deps: EnvelopeStoreDependencies<SupplyChangeEvent>)
    (queryService: CreateQueryService<SupplyChangeEvent, SupplyChangeAssessmentDto, SupplyChangeAssessmentId>)
    (policy: SupplyChangePolicyConfiguration)
    (publishKnowledge: KnowledgeRepresentation)
    (ports: SupplyPorts)
    : TaskResult<SenseSupplyChangesContext, ApplicationError> =
    taskResult {

        // 1. Initialize Supply Change Assessment projection
        let! aggregates: SupplyChangeAssessmentAggregate list =
            repo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let initialState = SupplyChangeAssessment.Projections.seedFromAggregates aggregates

        let validEvents =
            [ ArsIdentifiers.EnterpriseEvents.supplyChangeAssessmentUpdated.Id ]
            |> EnvelopeFilter.EventTypes

        let! queryCtx: ProjectionContext<SupplyChangeAssessmentDto, SupplyChangeAssessmentId> =
            queryService SupplyChangeAssessment.Projections.apply validEvents initialState "SupplyChangeAssessment"

        // 2. Instantiate Child Aggregate API
        let aggApi =
            SupplyChangeAssessment.Capabilities.create
                repo
                publishKnowledge
                policy
                deps
                ports

        // 3. Instantiate Parent Capability API
        let apis = Capabilities.create aggApi

        // 4. Wire workflow dependencies
        let workflowDeps: SenseSupplyChangesWorkflowDependencies =
            { Subscribe = deps.Subscribe
              DispatchEnvelope = deps.DispatchEnvelope
              Apis = apis }

        let! (sub1: IDisposable) = createSupplyChangeSensingWorkflow workflowDeps CancellationToken.None
        let! (sub2: IDisposable) = createNotificationNodeWorkflow workflowDeps CancellationToken.None

        // 5. Combine lifecycle disposables
        let dispose () =
            queryCtx.Dispose ()
            sub1.Dispose ()
            sub2.Dispose ()

        return
            { Commands = apis
              Queries = queryCtx.QueryService
              Dispose = dispose }
    }
