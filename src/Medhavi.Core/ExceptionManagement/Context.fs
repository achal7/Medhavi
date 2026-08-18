/// CA-C-020 Exception Management Composition Root
module Medhavi.Core.ExceptionManagement.Context

open System.Threading
open System.Threading.Tasks
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

        // 1. Projection
        let! aggregates =
            repo.GetAll()
            |> TaskResult.ofTask
            |> TaskResult.mapError(fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let state = seedFromAggregates aggregates

        let validEventTypes =
            [ ArsIdentifiers.EnterpriseEvents.exceptionActivated.Id
              ArsIdentifiers.EnterpriseEvents.exceptionUpdated.Id
              ArsIdentifiers.EnterpriseEvents.exceptionResolved.Id ]
            |> EnvelopeFilter.EventTypes

        let! queryCtx:ProjectionContext<Exception.ExceptionDto, ExceptionId> =
            createQueryService Projections.apply validEventTypes state "Exception"

        // Helper to query current severity from projection
        let getCurrentSeverity (id: ExceptionId) : Task<VocabularyEntryId option> =
            task {
                let! dtoOpt = queryCtx.QueryService.GetById id
                return
                    dtoOpt
                    |> Option.bind (fun dto -> dto.Severity)
                    |> Option.bind (fun s ->
                        match VocabularyEntryId.create s with
                        | Ok vId -> Some vId
                        | Error _ -> None)
            }

        // 2. Create capabilities
        let commands = Capabilities.create repo publishKnowledge policy deps getCurrentSeverity

        // 3. Wire FS-C-003: Exception evidence ingestion workflow
        let workflowDeps : EvidenceWorkflowDependencies =
            { DemandCodec = Medhavi.Foundation.Codec.json
              SupplyCodec = Medhavi.Foundation.Codec.json
              InventoryCodec = Medhavi.Foundation.Codec.json
              Subscribe = deps.Subscribe
              ExceptionApi = commands }

        let! workflowSubscription:System.IDisposable =
            createEvidenceWorkflow workflowDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError(fun err ->
                Infrastructure(EventStore($"Failed to create exception evidence ingestion workflow: {err}")))

        // 4. Combine disposables
        let dispose () =
            queryCtx.Dispose()
            workflowSubscription.Dispose()

        return
            { Commands = commands
              Queries = queryCtx.QueryService
              Dispose = dispose }
    }
