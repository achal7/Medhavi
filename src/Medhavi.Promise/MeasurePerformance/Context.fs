/// CA-P-010 Measure Promise Performance & Service Quality — Composition Root
module Medhavi.Promise.MeasurePerformance.Context

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
open Medhavi.Promise.MeasurePerformance.PromiseQualityAssessment.Model
open Medhavi.Promise.MeasurePerformance.PromiseQualityAssessment.Policies
open Medhavi.Promise.MeasurePerformance.PromiseQualityAssessment

type MeasurePerformanceContext =
    { Commands: PromiseQualityApi
      QualityQueries: QueryService<PromiseQualityAssessmentDto, PromiseQualityAssessmentId>
      Dispose: unit -> unit }

let create
    (repo: Repository<PromiseQualityAssessment, PromiseQualityAssessmentId, CommitmentQualityEvent>)
    (deps: EnvelopeStoreDependencies<CommitmentQualityEvent>)
    (queryService: CreateQueryService<CommitmentQualityEvent, PromiseQualityAssessmentDto, PromiseQualityAssessmentId>)
    (policy: SlaMeasurementPolicy)
    (publishKnowledge: KnowledgeRepresentation)
    : TaskResult<MeasurePerformanceContext, ApplicationError> =
    taskResult {
        let! aggregates =
            repo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun err -> Infrastructure(Database(sprintf "%A" err)))

        let initialState = Projections.seedFromAggregates aggregates

        let validEvents =
            [ ArsIdentifiers.EnterpriseEvents.commitmentQualityAudited.Id
              ArsIdentifiers.EnterpriseEvents.commitmentDisputeRecorded.Id ]
            |> EnvelopeFilter.EventTypes

        let! queryCtx: ProjectionContext<PromiseQualityAssessmentDto, PromiseQualityAssessmentId> =
            queryService Projections.apply validEvents initialState SemanticObjects.promiseQualityAssessment.Id

        let aggApi = PromiseQualityAssessment.Capabilities.create repo publishKnowledge policy deps
        let parentApi = Capabilities.createApi aggApi

        let workflowDeps: Workflows.QualityWorkflowDependencies =
            { Subscribe = deps.Subscribe
              DispatchNotification = fun _ _ _ -> System.Threading.Tasks.Task.FromResult () }

        let workflows = Workflows.create workflowDeps
        let! (sub: IDisposable) = workflows.StartAuditing CancellationToken.None

        let dispose () =
            queryCtx.Dispose ()
            sub.Dispose ()

        return
            { Commands = parentApi
              QualityQueries = queryCtx.QueryService
              Dispose = dispose }
    }
