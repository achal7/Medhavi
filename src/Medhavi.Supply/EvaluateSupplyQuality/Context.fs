/// CA-S-010 — Evaluate Supply Quality Composition Root
module Medhavi.Supply.EvaluateSupplyQuality.Context

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
open SupplyQualityAssessment.Model
open SupplyQualityAssessment.Policies
open Capabilities
open Workflows

/// Public context exposing commands, queries, and lifecycle management for Evaluate Supply Quality
type EvaluateSupplyQualityContext =
    { Commands: EvaluateSupplyQualityApis
      Queries: QueryService<SupplyQualityAssessmentDto, SupplyQualityAssessmentId>
      Dispose: unit -> unit }

/// Creates the complete Evaluate Supply Quality context with all child aggregates, projections, and workflows wired
let create
    (repo: Repository<SupplyQualityAssessment, SupplyQualityAssessmentId, SupplyQualityAssessmentEvent>)
    (deps: EnvelopeStoreDependencies<SupplyQualityAssessmentEvent>)
    (queryService: CreateQueryService<SupplyQualityAssessmentEvent, SupplyQualityAssessmentDto, SupplyQualityAssessmentId>)
    (policy: SupplyQualityPolicy)
    (publishKnowledge: KnowledgeRepresentation)
    (ports: SupplyPorts)
    : TaskResult<EvaluateSupplyQualityContext, ApplicationError> =
    taskResult {

        // 1. Initialize Supply Quality Assessment projection
        let! aggregates =
            repo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let initialState = SupplyQualityAssessment.Projections.seedFromAggregates aggregates

        let validEvents =
            [ ArsIdentifiers.EnterpriseEvents.supplyQualityAssessmentPublished.Id ]
            |> EnvelopeFilter.EventTypes

        let! (queryCtx: ProjectionContext<SupplyQualityAssessmentDto, SupplyQualityAssessmentId>) =
            queryService SupplyQualityAssessment.Projections.apply validEvents initialState "SupplyQualityAssessment"

        // 2. Instantiate Child Aggregate API
        let aggApi =
            SupplyQualityAssessment.Capabilities.create
                repo
                publishKnowledge
                policy
                deps
                ports

        let now = DateTimeOffset.UtcNow
        let defaultPeriodId = sprintf "P-%s" (now.ToString("yyyy-MM"))
        let defaultHorizon = sprintf "%s/%s" (now.ToString("yyyy-MM-ddTHH:mm:ssZ")) (now.AddDays(90.0).ToString("yyyy-MM-ddTHH:mm:ssZ"))

        // 3. Instantiate Parent Capability API
        let apis = Capabilities.create aggApi

        // 4. Wire workflow dependencies
        let workflowDeps : EvaluateSupplyQualityWorkflowDependencies =
            { Subscribe = deps.Subscribe
              DispatchEnvelope = deps.DispatchEnvelope
              Apis = apis
              DefaultEvaluationPeriodId = defaultPeriodId
              DefaultPlanningHorizon = defaultHorizon }

        let! (sub1: IDisposable) = createQualityEvaluationWorkflow workflowDeps CancellationToken.None
        let! (sub2: IDisposable) = createNotificationNodeWorkflow workflowDeps CancellationToken.None

        // 5. Combine lifecycle disposables
        let dispose () =
            queryCtx.Dispose ()
            sub1.Dispose ()
            sub2.Dispose ()

        let resultContext: EvaluateSupplyQualityContext =
            { Commands = apis
              Queries = queryCtx.QueryService
              Dispose = dispose }

        return resultContext
    }
