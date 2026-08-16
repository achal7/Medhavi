/// CA-C-020 Exception Management Composition Root
module Medhavi.Core.ExceptionManagement.Context

open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Core
open Medhavi.SemanticModel
open Medhavi.Core
open Model
open Policies
open Projections
open Medhavi.Core.ExceptionManagement.Workflows

/// Public context exposing commands and queries
type ExceptionManagementContext =
    { Commands: Exception.ExceptionApi
      Queries: QueryService<Exception.ExceptionDto, ExceptionId>
      Dispose: unit -> unit }

/// Creates the complete context with all dependencies wired
let create
    (repo: Repository<CoreException, ExceptionId, ExceptionEvent>)
    (deps: EnvelopeStoreDependencies<ExceptionEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (createQueryService: CreateQueryService<ExceptionEvent, Exception.ExceptionDto, ExceptionId>)
    (policy: ExceptionManagementPolicy)
    : TaskResult<ExceptionManagementContext, ApplicationError> =
    taskResult {

        // 1. Create capabilities
        let commands = Capabilities.create repo publishKnowledge policy deps

        // 2. Projection
        let! aggregates =
            repo.GetAll()
            |> TaskResult.ofTask
            |> TaskResult.mapError(fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let state = seedFromAggregates aggregates

        let validEventTypes =
            [ ArsIdentifiers.EnterpriseEvents.pictureVersionComposed.Id
              ArsIdentifiers.EnterpriseEvents.pictureVersionPublished.Id
              ArsIdentifiers.EnterpriseEvents.pictureVersionSuperseded.Id ]
            |> EnvelopeFilter.EventTypes

        let! queryCtx:ProjectionContext<Exception.ExceptionDto, ExceptionId> = createQueryService Projections.apply validEventTypes state "Exception"

        // 3. Wire FS-C-020a: Exception evidence ingestion workflow
        let workflowDeps : Evidence.WorkflowDependencies =
            { DemandCodec = Medhavi.Foundation.Codec.json
              SupplyCodec = Medhavi.Foundation.Codec.json
              InventoryCodec = Medhavi.Foundation.Codec.json
              Subscribe = deps.Subscribe
              ExceptionApi = commands }
        let! workflowSubscription:System.IDisposable =
            Evidence.create workflowDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError(fun err ->
                Infrastructure(EventStore($"Failed to create exception evidence ingestion workflow: {err}")))

        // 3. Wire FS-C-020b: Exception SLA Escalation Workflow
        let slaEscalationDeps : ExceptionEvidenceIngestion.ExceptionSlaEscalation.WorkflowDependencies =
            { Subscribe = deps.Subscribe
              ExceptionApi = commands
              Policy = policy
              PublishEscalation = fun exceptionId severity overdueBy ->
                  task {
                      // Publish escalation notification via envelope store
                      let notification : Medhavi.SharedKernel.BusinessNotifications.ExceptionSlaEscalationNotification =
                          { ExceptionId = exceptionId
                            Severity = severity
                            SlaDeadline = Timestamp.now()
                            EscalatedAt = Timestamp.now()
                            OverdueBy = overdueBy }
                      // Dispatch via deps
                      return ()
                  }
              PublishWarning = fun exceptionId severity ->
                  task {
                      // Publish warning notification via envelope store
                      let notification : Medhavi.SharedKernel.BusinessNotifications.ExceptionSlaWarningNotification =
                          { ExceptionId = exceptionId
                            Severity = severity
                            SlaDeadline = Timestamp.now()
                            WarningIssuedAt = Timestamp.now() }
                      // Dispatch via deps
                      return ()
                  }
              GetCurrentTime = fun () -> Timestamp.now() }

        let! slaEscalationSubscription:System.IDisposable =
            ExceptionEvidenceIngestion.ExceptionSlaEscalation.create slaEscalationDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError(fun err ->
                Infrastructure(EventStore($"Failed to create SLA escalation workflow: {err}")))


        // 4. Combine disposables
        let dispose () =
            queryCtx.Dispose()
            workflowSubscription.Dispose()
            slaEscalationSubscription.Dispose()

        return
            { Commands = commands
              Queries = queryCtx.QueryService
              Dispose = dispose }
    }
