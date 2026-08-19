module Medhavi.Core.DemandManagement.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Core.Demand
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.Core
open Medhavi.Core.DemandManagement.Workflows
open Model
open Policies
open Projections

type DemandManagementContext =
    { Commands: DemandApi
      Queries: QueryService<DemandDto, DemandId>
      Dispose: unit -> unit }

let create
    (repo: Repository<Demand, DemandId, DemandEvent>)
    (deps: EnvelopeStoreDependencies<DemandEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (createQueryService: CreateQueryService<DemandEvent, DemandDto, DemandId>)
    (policy: DemandManagementPolicy)
    : TaskResult<DemandManagementContext, ApplicationError> =
    taskResult {
        // 1. Projection
        let! aggregates =
            repo.GetAll()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun e -> Infrastructure(Database(sprintf "%A" e)))

        let state = seedFromAggregates aggregates

        let validEventTypes =
            [ ArsIdentifiers.EnterpriseEvents.demandRecorded.Id
              ArsIdentifiers.EnterpriseEvents.demandSatisfied.Id
              ArsIdentifiers.EnterpriseEvents.demandCancelled.Id ]
            |> EnvelopeFilter.EventTypes

        let! (queryCtx: ProjectionContext<DemandDto, DemandId>) =
            createQueryService Projections.apply validEventTypes state "Demand"

        // 2. Capabilities
        let commands = Capabilities.create repo publishKnowledge policy deps

        // 3. Wire FS-C-009: Demand Recording Workflow (consumes BN-D-006)
        let workflowDeps: DemandRecordingWorkflowDependencies =
            { Codec = Medhavi.Foundation.Codec.json<DemandObservationAcceptedNotification>
              DemandApi = commands
              Subscribe = deps.Subscribe }

        let! (workflowSubscription: IDisposable) =
            createDemandRecordingWorkflow workflowDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError (fun err ->
                Infrastructure(Database($"Failed to create demand recording workflow: {err}")))

        // 4. Dispose
        let dispose () =
            queryCtx.Dispose()
            workflowSubscription.Dispose()

        return
            { Commands = commands
              Queries = queryCtx.QueryService
              Dispose = dispose }
    }
