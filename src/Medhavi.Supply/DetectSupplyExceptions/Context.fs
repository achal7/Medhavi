/// CA-S-011 Detect Supply Exceptions Composition Root
module Medhavi.Supply.DetectSupplyExceptions.Context

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
open Medhavi.Supply.DetectSupplyExceptions.SupplyExceptionAssessment.Model
open Medhavi.Supply.DetectSupplyExceptions.SupplyExceptionAssessment.Policies
open Medhavi.Supply.DetectSupplyExceptions.SupplyExceptionAssessment.Projections
open Medhavi.Supply.DetectSupplyExceptions.Workflows
open Capabilities

/// Public context exposing commands, queries, and lifecycle management for Detect Supply Exceptions
type DetectSupplyExceptionsContext =
    {
        Commands: DetectSupplyExceptionsApis
        Queries: QueryService<SupplyExceptionAssessmentDto, SupplyExceptionId>
        Dispose: unit -> unit
    }

/// Creates the complete Detect Supply Exceptions context with aggregate API, projection, and workflows wired
let create
    (repo: Repository<SupplyExceptionAssessment, SupplyExceptionId, SupplyExceptionEvent>)
    (deps: EnvelopeStoreDependencies<SupplyExceptionEvent>)
    (queryService: CreateQueryService<SupplyExceptionEvent, SupplyExceptionAssessmentDto, SupplyExceptionId>)
    (policy: ExceptionDetectionPolicy)
    (publishKnowledge: KnowledgeRepresentation)
    (ports: SupplyPorts)
    (defaultScopeId: string)
    : TaskResult<DetectSupplyExceptionsContext, ApplicationError> =
    taskResult {

        // 1. Initialize Supply Exception Assessment projection
        let! aggregates: SupplyExceptionAssessment list =
            repo.GetAll()
            |> TaskResult.ofTask
            |> TaskResult.mapError(fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let initialState = seedFromAggregates aggregates

        let validEvents =
            [
                ArsIdentifiers.EnterpriseEvents.supplyExceptionAssessmentUpdated.Id
                ArsIdentifiers.EnterpriseEvents.supplyExceptionDetectionEvidenceProduced.Id
                ArsIdentifiers.EnterpriseEvents.supplyExceptionResolutionEvidenceProduced.Id
            ]
            |> EnvelopeFilter.EventTypes

        let! queryCtx: ProjectionContext<SupplyExceptionAssessmentDto, SupplyExceptionId> =
            queryService apply validEvents initialState "SupplyExceptionAssessment"

        // 2. Instantiate Aggregate API
        let aggApi = SupplyExceptionAssessment.Capabilities.create repo publishKnowledge policy deps ports

        // 3. Instantiate Parent Capability API
        let apis = Capabilities.create aggApi

        // 4. Wire workflow dependencies
        let workflowDeps: DetectSupplyExceptionsWorkflowDependencies =
            {
                Subscribe = deps.Subscribe
                DispatchEnvelope = deps.DispatchEnvelope
                Apis = apis
                DefaultScopeId = defaultScopeId
            }

        let! (changeSub: IDisposable) = createSupplyChangeExceptionWorkflow workflowDeps CancellationToken.None
        let! (qualitySub: IDisposable) = createSupplyQualityExceptionWorkflow workflowDeps CancellationToken.None
        let! (operationalSub: IDisposable) = createOperationalPositionExceptionWorkflow workflowDeps CancellationToken.None
        let! (coreSub: IDisposable) = createEnterpriseCoreExceptionWorkflow workflowDeps CancellationToken.None
        let! (notifSub: IDisposable) = createNotificationNodeWorkflow workflowDeps CancellationToken.None

        // 5. Combine lifecycle disposables
        let dispose () =
            queryCtx.Dispose()
            changeSub.Dispose()
            qualitySub.Dispose()
            operationalSub.Dispose()
            coreSub.Dispose()
            notifSub.Dispose()

        return
            {
                Commands = apis
                Queries = queryCtx.QueryService
                Dispose = dispose
            }
    }
