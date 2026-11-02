/// CA-P-009 Resolve Exceptions — Composition Root
module Medhavi.Promise.ResolveExceptions.Context

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
open Medhavi.Promise.ResolveExceptions.PromiseExceptionAssessment.Model
open Medhavi.Promise.ResolveExceptions.PromiseExceptionAssessment.Policies
open Medhavi.Promise.ResolveExceptions.Workflows
open Capabilities

type CommitmentEntity = Medhavi.Promise.EstablishCommitments.OrderCommitment.Model.OrderCommitment
type CommitmentEvent = Medhavi.Promise.EstablishCommitments.OrderCommitment.Model.OrderCommitmentEvent

/// Public context exposing commands, queries, and lifecycle management for Resolve Exceptions
type ResolveExceptionsContext =
    { Commands: ResolveExceptionsApis
      ExceptionQueries: QueryService<PromiseExceptionAssessmentDto, PromiseExceptionId>
      Dispose: unit -> unit }

/// Creates the complete Resolve Exceptions context with all projections and workflows wired
let create
    (exceptionRepo: Repository<PromiseExceptionAssessment, PromiseExceptionId, PromiseExceptionEvent>)
    (exceptionDeps: EnvelopeStoreDependencies<PromiseExceptionEvent>)
    (exceptionQueryService: CreateQueryService<PromiseExceptionEvent, PromiseExceptionAssessmentDto, PromiseExceptionId>)
    (exceptionPolicy: DisruptionMitigationPolicy)
    (publishKnowledge: KnowledgeRepresentation)
    (commitmentRepo: Repository<CommitmentEntity, OrderCommitmentId, CommitmentEvent>)
    (defaultScopeId: PlanningScopeId)
    : TaskResult<ResolveExceptionsContext, ApplicationError> =
    taskResult {

        // 1. Initialize PromiseExceptionAssessment projection
        let! exceptionAggregates: PromiseExceptionAssessment list =
            exceptionRepo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let exceptionInitialState = PromiseExceptionAssessment.Projections.seedFromAggregates exceptionAggregates

        let exceptionValidEvents =
            [ ArsIdentifiers.EnterpriseEvents.disruptionCaseOpened.Id
              ArsIdentifiers.EnterpriseEvents.disruptionRecoveryExecuted.Id ]
            |> EnvelopeFilter.EventTypes

        let! exceptionQueryCtx: ProjectionContext<PromiseExceptionAssessmentDto, PromiseExceptionId> =
            exceptionQueryService PromiseExceptionAssessment.Projections.apply exceptionValidEvents exceptionInitialState (SemanticObjects.promiseExceptionAssessment.Id)

        // 2. Instantiate Child Aggregate API
        let exceptionAggApi =
            PromiseExceptionAssessment.Capabilities.create
                exceptionRepo
                publishKnowledge
                exceptionPolicy
                exceptionDeps
                commitmentRepo

        // 3. Instantiate Parent Capability API
        let apis = Capabilities.create exceptionAggApi

        // 4. Wire workflow dependencies
        let workflowDeps: ResolveExceptionsWorkflowDependencies =
            { ExceptionCodec = Medhavi.Foundation.Codec.json
              Subscribe = exceptionDeps.Subscribe
              DispatchEnvelope = exceptionDeps.DispatchEnvelope
              Apis = apis
              DefaultScopeId = PlanningScopeId.value defaultScopeId }

        let! sub: IDisposable =
            createExceptionWorkflow workflowDeps CancellationToken.None

        // 5. Combine lifecycle disposables
        let dispose () =
            exceptionQueryCtx.Dispose ()
            sub.Dispose ()

        return
            { Commands = apis
              ExceptionQueries = exceptionQueryCtx.QueryService
              Dispose = dispose }
    }

