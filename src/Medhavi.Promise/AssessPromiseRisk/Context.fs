/// CA-P-007 Assess Promise Risk and Resilience — Composition Root
module Medhavi.Promise.AssessPromiseRisk.Context

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
open Medhavi.Promise.AssessPromiseRisk.PromiseRiskAssessment.Model
open Medhavi.Promise.AssessPromiseRisk.PromiseRiskAssessment.Policies
open Medhavi.Promise.AssessPromiseRisk.Workflows
open Capabilities

type CommitmentEntity = Medhavi.Promise.EstablishCommitments.OrderCommitment.Model.OrderCommitment
type CommitmentEvent = Medhavi.Promise.EstablishCommitments.OrderCommitment.Model.OrderCommitmentEvent

/// Public context exposing commands, queries, and lifecycle management for Assess Promise Risk
type AssessPromiseRiskContext =
    { Commands: AssessPromiseRiskApis
      RiskQueries: QueryService<PromiseRiskAssessmentDto, PromiseRiskAssessmentId>
      Dispose: unit -> unit }

/// Creates the complete Assess Promise Risk context with all projections and workflows wired
let create
    (riskRepo: Repository<PromiseRiskAssessment, PromiseRiskAssessmentId, PromiseRiskEvent>)
    (riskDeps: EnvelopeStoreDependencies<PromiseRiskEvent>)
    (riskQueryService: CreateQueryService<PromiseRiskEvent, PromiseRiskAssessmentDto, PromiseRiskAssessmentId>)
    (riskPolicy: PromiseRiskAssessmentPolicy)
    (publishKnowledge: KnowledgeRepresentation)
    (ports: PromisePorts)
    (commitmentRepo: Repository<CommitmentEntity, OrderCommitmentId, CommitmentEvent>)
    (defaultScopeId: PlanningScopeId)
    : TaskResult<AssessPromiseRiskContext, ApplicationError> =
    taskResult {

        // 1. Initialize PromiseRiskAssessment projection
        let! riskAggregates: PromiseRiskAssessment list =
            riskRepo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let riskInitialState = PromiseRiskAssessment.Projections.seedFromAggregates riskAggregates

        let riskValidEvents =
            [ ArsIdentifiers.EnterpriseEvents.commitmentRiskAssessed.Id
              ArsIdentifiers.EnterpriseEvents.commitmentRiskEscalated.Id
              ArsIdentifiers.EnterpriseEvents.riskFlagCleared.Id ]
            |> EnvelopeFilter.EventTypes

        let! riskQueryCtx: ProjectionContext<PromiseRiskAssessmentDto, PromiseRiskAssessmentId> =
            riskQueryService PromiseRiskAssessment.Projections.apply riskValidEvents riskInitialState (SemanticObjects.promiseRiskAssessment.Id)

        // 2. Instantiate Child Aggregate API
        let riskAggApi =
            PromiseRiskAssessment.Capabilities.create
                riskRepo
                publishKnowledge
                riskPolicy
                riskDeps
                ports
                commitmentRepo
                defaultScopeId

        // 3. Instantiate Parent Capability API
        let apis = Capabilities.create riskAggApi

        // 4. Wire workflow dependencies
        let workflowDeps: AssessPromiseRiskWorkflowDependencies =
            { RiskCodec = Medhavi.Foundation.Codec.json
              Subscribe = riskDeps.Subscribe
              DispatchEnvelope = riskDeps.DispatchEnvelope
              Apis = apis
              DefaultScopeId = PlanningScopeId.value defaultScopeId }

        let! sub: IDisposable =
            createRiskMonitoringWorkflow workflowDeps CancellationToken.None

        // 5. Combine lifecycle disposables
        let dispose () =
            riskQueryCtx.Dispose ()
            sub.Dispose ()

        return
            { Commands = apis
              RiskQueries = riskQueryCtx.QueryService
              Dispose = dispose }
    }

