/// CA-S-001 Understand Supply Composition Root
module Medhavi.Supply.UnderstandSupply.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Medhavi.Supply.UnderstandSupply.SupplyDataRecord.Model
open Medhavi.Supply.UnderstandSupply.SupplyDataRecord.Policies
open Medhavi.Supply.UnderstandSupply.SupplyUnderstanding.Model
open Medhavi.Supply.UnderstandSupply.SupplyUnderstanding.Policies
open Medhavi.Supply.UnderstandSupply.Workflows
open Capabilities

/// Public context exposing commands, queries, and lifecycle management for Understand Supply
type UnderstandSupplyContext =
    { Commands: UnderstandSupplyApis
      RecordQueries: QueryService<SupplyDataRecordDto, SupplyDataRecordId>
      UnderstandingQueries: QueryService<SupplyUnderstandingDto, PlanningScopeId>
      Dispose: unit -> unit }

/// Creates the complete Understand Supply context with all child aggregates, projections, and workflows wired
let create
    (recRepo: Repository<SupplyDataRecord, SupplyDataRecordId, RecordEvent>)
    (recDeps: EnvelopeStoreDependencies<RecordEvent>)
    (recQueryService: CreateQueryService<RecordEvent, SupplyDataRecordDto, SupplyDataRecordId>)
    (recPolicy: SupplyDataAcceptancePolicy)
    (undRepo: Repository<SupplyUnderstanding, PlanningScopeId, SupplyUnderstandingEvent>)
    (undDeps: EnvelopeStoreDependencies<SupplyUnderstandingEvent>)
    (undQueryService: CreateQueryService<SupplyUnderstandingEvent, SupplyUnderstandingDto, PlanningScopeId>)
    (materialityPolicy: MaterialityPolicy)
    (cadencePolicy: CadencePolicy)
    (publishKnowledge: KnowledgeRepresentation)
    (ports: SupplyPorts)
    (defaultScopeId: string)
    : TaskResult<UnderstandSupplyContext, ApplicationError> =
    taskResult {

        // 1. Initialize Supply Data Record projection
        let! recAggregates: SupplyDataRecord list =
            recRepo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let recInitialState = SupplyDataRecord.Projections.seedFromAggregates recAggregates

        let recValidEvents =
            [ ArsIdentifiers.EnterpriseEvents.supplyDataRecordReceived.Id
              ArsIdentifiers.EnterpriseEvents.supplyDataRecordEvaluated.Id ]
            |> EnvelopeFilter.EventTypes

        let! recQueryCtx: ProjectionContext<SupplyDataRecordDto, SupplyDataRecordId> =
            recQueryService SupplyDataRecord.Projections.apply recValidEvents recInitialState "SupplyDataRecord"

        // 2. Initialize Supply Understanding projection
        let! undAggregates: SupplyUnderstanding list =
            undRepo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let undInitialState = SupplyUnderstanding.Projections.seedFromAggregates undAggregates

        let undValidEvents =
            [ ArsIdentifiers.EnterpriseEvents.supplyUnderstandingRevised.Id
              ArsIdentifiers.EnterpriseEvents.supplyUnderstandingPublished.Id ]
            |> EnvelopeFilter.EventTypes

        let! undQueryCtx: ProjectionContext<SupplyUnderstandingDto, PlanningScopeId> =
            undQueryService SupplyUnderstanding.Projections.apply undValidEvents undInitialState "SupplyUnderstanding"

        // 3. Instantiate Child Aggregate APIs
        let recAggApi =
            SupplyDataRecord.Capabilities.create
                recRepo
                publishKnowledge
                recPolicy
                recDeps
                ports

        let undAggApi =
            SupplyUnderstanding.Capabilities.create
                undRepo
                publishKnowledge
                materialityPolicy
                cadencePolicy
                undDeps
                ports

        // 4. Instantiate Parent Capability API
        let apis = Capabilities.create recAggApi undAggApi

        // 5. Wire workflow dependencies
        let workflowDeps: UnderstandSupplyWorkflowDependencies =
            { RecordCodec = Medhavi.Foundation.Codec.json
              UnderstandingCodec = Medhavi.Foundation.Codec.json
              Subscribe = recDeps.Subscribe
              DispatchEnvelope = undDeps.DispatchEnvelope
              Apis = apis
              DefaultScopeId = defaultScopeId }

        let! (recSub: IDisposable) = createSupplyDataWorkflow workflowDeps CancellationToken.None
        let! (epSub: IDisposable) = createEnterprisePictureWorkflow workflowDeps CancellationToken.None
        let! (undPubSub: IDisposable) = createSupplyUnderstandingPublishWorkflow workflowDeps CancellationToken.None
        let! (notifSub: IDisposable) = createNotificationNodeWorkflow workflowDeps CancellationToken.None

        // 6. Combine lifecycle disposables
        let dispose () =
            recQueryCtx.Dispose ()
            undQueryCtx.Dispose ()
            recSub.Dispose ()
            epSub.Dispose ()
            undPubSub.Dispose ()
            notifSub.Dispose ()

        return
            { Commands = apis
              RecordQueries = recQueryCtx.QueryService
              UnderstandingQueries = undQueryCtx.QueryService
              Dispose = dispose }
    }
