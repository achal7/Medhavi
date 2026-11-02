/// CA-P-011 Learn & Calibrate Promising Models — Composition Root
module Medhavi.Promise.LearnFromPromises.Context

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
open Medhavi.Promise.LearnFromPromises.PromiseCalibrationModel.Model
open Medhavi.Promise.LearnFromPromises.PromiseCalibrationModel.Policies
open Medhavi.Promise.LearnFromPromises.PromiseCalibrationModel

type LearnFromPromisesContext =
    { Commands: PromiseLearningApi
      LearningQueries: QueryService<PromiseCalibrationModelDto, PromiseLearningId>
      Dispose: unit -> unit }

let create
    (repo: Repository<PromiseCalibrationModel, PromiseLearningId, PromiseCalibrationEvent>)
    (deps: EnvelopeStoreDependencies<PromiseCalibrationEvent>)
    (queryService: CreateQueryService<PromiseCalibrationEvent, PromiseCalibrationModelDto, PromiseLearningId>)
    (policy: PromisingModelCalibrationPolicy)
    (publishKnowledge: KnowledgeRepresentation)
    : TaskResult<LearnFromPromisesContext, ApplicationError> =
    taskResult {
        let! aggregates =
            repo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun err -> Infrastructure(Database(sprintf "%A" err)))

        let initialState = Projections.seedFromAggregates aggregates

        let validEvents =
            [ ArsIdentifiers.EnterpriseEvents.promiseCalibrationProposed.Id
              ArsIdentifiers.EnterpriseEvents.promiseCalibrationApplied.Id ]
            |> EnvelopeFilter.EventTypes

        let! queryCtx: ProjectionContext<PromiseCalibrationModelDto, PromiseLearningId> =
            queryService Projections.apply validEvents initialState SemanticObjects.promiseCalibrationModel.Id

        let aggApi = PromiseCalibrationModel.Capabilities.create repo publishKnowledge policy deps
        let parentApi = Capabilities.createApi aggApi

        let workflowDeps: Workflows.LearningWorkflowDependencies =
            { Subscribe = deps.Subscribe
              DispatchNotification = fun _ _ _ -> System.Threading.Tasks.Task.FromResult () }

        let workflows = Workflows.create workflowDeps
        let! (sub: IDisposable) = workflows.StartCalibration CancellationToken.None

        let dispose () =
            queryCtx.Dispose ()
            sub.Dispose ()

        return
            { Commands = parentApi
              LearningQueries = queryCtx.QueryService
              Dispose = dispose }
    }
