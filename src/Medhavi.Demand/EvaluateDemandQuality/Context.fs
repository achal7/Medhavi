/// CA-D-007 — Evaluate Demand Quality Composition Root
module Medhavi.Demand.EvaluateDemandQuality.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Medhavi.Demand.EvaluateDemandQuality.ForecastQualityAssessment.Model
open Medhavi.Demand.EvaluateDemandQuality.ForecastQualityAssessment.Policies
open Medhavi.Demand.EvaluateDemandQuality.ForecastQualityAssessment.Projections
open Medhavi.Demand.EvaluateDemandQuality.Workflows

/// Public context exposing commands, queries, and lifecycle management for Evaluate Demand Quality
type ForecastQualityContext =
    { Commands: ForecastQualityApi
      Queries: QueryService<ForecastQualityAssessmentDto, ForecastQualityAssessmentId>
      Dispose: unit -> unit }

/// Creates the complete Evaluate Demand Quality context with all dependencies wired
let create
    (repo: Repository<ForecastQualityAssessment, ForecastQualityAssessmentId, ForecastQualityEvent>)
    (deps: EnvelopeStoreDependencies<ForecastQualityEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (createQueryService: CreateQueryService<ForecastQualityEvent, ForecastQualityAssessmentDto, ForecastQualityAssessmentId>)
    (policy: ForecastMeasurementPolicy)
    : TaskResult<ForecastQualityContext, ApplicationError> =
    taskResult {

        // 1. Seed projection read model from repository
        let! aggregates =
            repo.GetAll()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let state = seedFromAggregates aggregates

        let validEventTypes =
            [ ArsIdentifiers.EnterpriseEvents.forecastQualityAssessmentPublished.Id ]
            |> EnvelopeFilter.EventTypes

        // 2. Initialize projection query service
        let! queryCtx: ProjectionContext<ForecastQualityAssessmentDto, ForecastQualityAssessmentId> =
            createQueryService apply validEventTypes state "ForecastQualityAssessment"

        // 3. Instantiate child aggregate API
        let aggregateApi =
            ForecastQualityAssessment.Capabilities.create
                repo
                publishKnowledge
                policy
                deps

        // 4. Instantiate parent capability API
        let commands = Medhavi.Demand.EvaluateDemandQuality.Capabilities.create aggregateApi deps.DispatchEnvelope

        // 5. Wire workflow
        let workflowDeps: ForecastQualityWorkflowDependencies =
            { Subscribe = deps.Subscribe
              QualityApi = commands }

        let! workflowSub: IDisposable =
            createForecastQualityWorkflow workflowDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError (fun err ->
                Infrastructure(EventStore($"Failed to create forecast quality workflow: {err}")))

        // 6. Lifecycle disposables
        let dispose () =
            queryCtx.Dispose()
            workflowSub.Dispose()

        return
            { Commands = commands
              Queries = queryCtx.QueryService
              Dispose = dispose }
    }
