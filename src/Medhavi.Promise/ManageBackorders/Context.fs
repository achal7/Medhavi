/// CA-P-008 Manage Backorders — Composition Root
module Medhavi.Promise.ManageBackorders.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Medhavi.Promise.ManageBackorders.BackorderRecord.Model
open Medhavi.Promise.ManageBackorders.BackorderRecord.Policies
open Medhavi.Promise.ManageBackorders.Workflows
open Capabilities

/// Public context exposing commands, queries, and lifecycle management for Manage Backorders
type ManageBackordersContext =
    { Commands: ManageBackordersApis
      BackorderQueries: QueryService<BackorderRecordDto, BackorderId>
      Dispose: unit -> unit }

/// Creates the complete Manage Backorders context with all projections and workflows wired
let create
    (backorderRepo: Repository<BackorderRecord, BackorderId, BackorderEvent>)
    (backorderDeps: EnvelopeStoreDependencies<BackorderEvent>)
    (backorderQueryService: CreateQueryService<BackorderEvent, BackorderRecordDto, BackorderId>)
    (backorderPolicy: BacklogGovernancePolicy)
    (publishKnowledge: KnowledgeRepresentation)
    (ports: PromisePorts)
    (defaultScopeId: PlanningScopeId)
    : TaskResult<ManageBackordersContext, ApplicationError> =
    taskResult {

        // 1. Initialize BackorderRecord projection
        let! backorderAggregates: BackorderRecord list =
            backorderRepo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let backorderInitialState = BackorderRecord.Projections.seedFromAggregates backorderAggregates

        let backorderValidEvents =
            [ ArsIdentifiers.EnterpriseEvents.backorderQueued.Id
              ArsIdentifiers.EnterpriseEvents.backorderResolved.Id ]
            |> EnvelopeFilter.EventTypes

        let! backorderQueryCtx: ProjectionContext<BackorderRecordDto, BackorderId> =
            backorderQueryService BackorderRecord.Projections.apply backorderValidEvents backorderInitialState (SemanticObjects.backorderRecord.Id)

        // 2. Instantiate Child Aggregate API
        let backorderAggApi =
            BackorderRecord.Capabilities.create
                backorderRepo
                publishKnowledge
                backorderPolicy
                backorderDeps
                ports

        // 3. Instantiate Parent Capability API
        let apis = Capabilities.create backorderAggApi

        // 4. Wire workflow dependencies
        let workflowDeps: ManageBackordersWorkflowDependencies =
            { BackorderCodec = Medhavi.Foundation.Codec.json
              Subscribe = backorderDeps.Subscribe
              DispatchEnvelope = backorderDeps.DispatchEnvelope
              Apis = apis
              DefaultScopeId = PlanningScopeId.value defaultScopeId }

        let! sub: IDisposable =
            createBackorderWorkflow workflowDeps CancellationToken.None

        // 5. Combine lifecycle disposables
        let dispose () =
            backorderQueryCtx.Dispose ()
            sub.Dispose ()

        return
            { Commands = apis
              BackorderQueries = backorderQueryCtx.QueryService
              Dispose = dispose }
    }

