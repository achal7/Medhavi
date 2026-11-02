/// CA-P-006 Establish Commitments Composition Root
module Medhavi.Promise.EstablishCommitments.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Medhavi.Promise.EstablishCommitments.OrderCommitment.Model
open Medhavi.Promise.EstablishCommitments.OrderCommitment.Policies
open Medhavi.Promise.EstablishCommitments.PromiseExplanation.Model
open Medhavi.Promise.EstablishCommitments.Workflows
open Capabilities

/// Public context exposing commands, queries, and lifecycle management for Establish Commitments
type EstablishCommitmentsContext =
    { Commands: EstablishCommitmentsApis
      CommitmentQueries: QueryService<OrderCommitmentDto, OrderCommitmentId>
      ExplanationQueries: QueryService<PromiseExplanationDto, PromiseExplanationId>
      Dispose: unit -> unit }

/// Creates the complete Establish Commitments context with all child aggregates, projections, and workflows wired
let create
    (commitmentRepo: Repository<OrderCommitment, OrderCommitmentId, OrderCommitmentEvent>)
    (commitmentDeps: EnvelopeStoreDependencies<OrderCommitmentEvent>)
    (commitmentQueryService: CreateQueryService<OrderCommitmentEvent, OrderCommitmentDto, OrderCommitmentId>)
    (commitmentPolicy: CommitmentGovernancePolicy)
    (explanationRepo: Repository<PromiseExplanation, PromiseExplanationId, PromiseExplanationEvent>)
    (explanationDeps: EnvelopeStoreDependencies<PromiseExplanationEvent>)
    (explanationQueryService: CreateQueryService<PromiseExplanationEvent, PromiseExplanationDto, PromiseExplanationId>)
    (publishKnowledge: KnowledgeRepresentation)
    (ports: PromisePorts)
    (defaultScopeId: string)
    : TaskResult<EstablishCommitmentsContext, ApplicationError> =
    taskResult {

        // 1. Initialize OrderCommitment projection
        let! commitmentAggregates: OrderCommitment list =
            commitmentRepo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let commitmentInitialState = OrderCommitment.Projections.seedFromAggregates commitmentAggregates

        let commitmentValidEvents =
            [ ArsIdentifiers.EnterpriseEvents.commitmentEstablished.Id
              ArsIdentifiers.EnterpriseEvents.commitmentRevised.Id
              ArsIdentifiers.EnterpriseEvents.commitmentFulfilled.Id
              ArsIdentifiers.EnterpriseEvents.commitmentCancelled.Id ]
            |> EnvelopeFilter.EventTypes

        let! commitmentQueryCtx: ProjectionContext<OrderCommitmentDto, OrderCommitmentId> =
            commitmentQueryService OrderCommitment.Projections.apply commitmentValidEvents commitmentInitialState "OrderCommitment"

        // 2. Initialize PromiseExplanation projection
        let! explanationAggregates: PromiseExplanation list =
            explanationRepo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let explanationInitialState = PromiseExplanation.Projections.seedFromAggregates explanationAggregates

        let explanationValidEvents =
            [ ArsIdentifiers.EnterpriseEvents.promiseExplanationGenerated.Id ]
            |> EnvelopeFilter.EventTypes

        let! explanationQueryCtx: ProjectionContext<PromiseExplanationDto, PromiseExplanationId> =
            explanationQueryService PromiseExplanation.Projections.apply explanationValidEvents explanationInitialState "PromiseExplanation"

        // 3. Instantiate Child Aggregate APIs
        let commitmentAggApi =
            OrderCommitment.Capabilities.create
                commitmentRepo
                publishKnowledge
                commitmentPolicy
                commitmentDeps
                ports

        let explanationAggApi =
            PromiseExplanation.Capabilities.create
                explanationRepo
                publishKnowledge
                explanationDeps
                commitmentRepo

        // 4. Instantiate Parent Capability API
        let apis = Capabilities.create commitmentAggApi explanationAggApi

        // 5. Wire workflow dependencies
        let workflowDeps: EstablishCommitmentsWorkflowDependencies =
            { CommitmentCodec = Medhavi.Foundation.Codec.json
              ExplanationCodec = Medhavi.Foundation.Codec.json
              Subscribe = commitmentDeps.Subscribe
              DispatchEnvelope = commitmentDeps.DispatchEnvelope
              Apis = apis
              DefaultScopeId = defaultScopeId }

        let! sub: IDisposable =
            createCommitmentWorkflow workflowDeps CancellationToken.None

        // 6. Combine lifecycle disposables
        let dispose () =
            commitmentQueryCtx.Dispose ()
            explanationQueryCtx.Dispose ()
            sub.Dispose ()

        return
            { Commands = apis
              CommitmentQueries = commitmentQueryCtx.QueryService
              ExplanationQueries = explanationQueryCtx.QueryService
              Dispose = dispose }
    }

