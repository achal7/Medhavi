/// CA-D-001 Understand Demand Composition Root
module Medhavi.Demand.UnderstandDemand.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers
open Medhavi.Demand.UnderstandDemand.DemandObservation.Model
open Medhavi.Demand.UnderstandDemand.DemandObservation.Policies
open Medhavi.Demand.UnderstandDemand.DemandUnderstanding.Model
open Medhavi.Demand.UnderstandDemand.DemandUnderstanding.Policies
open Medhavi.Demand.UnderstandDemand.Workflows
open Capabilities

/// Public context exposing commands, queries, and lifecycle management for Understand Demand
type UnderstandDemandContext =
    { Commands: UnderstandDemandApis
      ObservationQueries: QueryService<DemandObservationDto, DemandObservationId>
      UnderstandingQueries: QueryService<DemandUnderstandingDto, string>
      Dispose: unit -> unit }

/// Creates the complete Understand Demand context with all child aggregates, projections, and workflows wired
let create
    (obsRepo: Repository<DemandObservation, DemandObservationId, ObservationEvent>)
    (obsDeps: EnvelopeStoreDependencies<ObservationEvent>)
    (obsQueryService: CreateQueryService<ObservationEvent, DemandObservationDto, DemandObservationId>)
    (obsPolicy: DemandDataAcceptancePolicy)
    (undRepo: Repository<DemandUnderstanding, PlanningScopeId, DemandUnderstandingEvent>)
    (undDeps: EnvelopeStoreDependencies<DemandUnderstandingEvent>)
    (undQueryService: CreateQueryService<DemandUnderstandingEvent, DemandUnderstandingDto, string>)
    (materialityPolicy: MaterialityPolicy)
    (cadencePolicy: CadencePolicy)
    (publishKnowledge: KnowledgeRepresentation)
    (ports: DemandPorts)
    (defaultScopeId: string)
    : TaskResult<UnderstandDemandContext, ApplicationError> =
    taskResult {

        // 1. Initialize Demand Observation projection
        let! obsAggregates =
            obsRepo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let obsInitialState = DemandObservation.Projections.seedFromAggregates obsAggregates

        let obsValidEvents =
            [ ArsIdentifiers.EnterpriseEvents.demandObservationReceived.Id
              ArsIdentifiers.EnterpriseEvents.demandObservationEvaluated.Id ]
            |> EnvelopeFilter.EventTypes

        let! obsQueryCtx: ProjectionContext<DemandObservationDto, DemandObservationId> =
            obsQueryService DemandObservation.Projections.apply obsValidEvents obsInitialState "DemandObservation"

        // 2. Initialize Demand Understanding projection
        let! undAggregates =
            undRepo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let undInitialState = DemandUnderstanding.Projections.seedFromAggregates undAggregates

        let undValidEvents =
            [ ArsIdentifiers.EnterpriseEvents.demandUnderstandingRevised.Id
              ArsIdentifiers.EnterpriseEvents.demandUnderstandingPublished.Id ]
            |> EnvelopeFilter.EventTypes

        let! undQueryCtx: ProjectionContext<DemandUnderstandingDto, string> =
            undQueryService DemandUnderstanding.Projections.apply undValidEvents undInitialState "DemandUnderstanding"

        // 3. Instantiate Child Aggregate APIs
        let obsAggApi =
            DemandObservation.Capabilities.create
                obsRepo
                publishKnowledge
                obsPolicy
                obsDeps
                ports

        let undAggApi =
            DemandUnderstanding.Capabilities.create
                undRepo
                publishKnowledge
                materialityPolicy
                cadencePolicy
                undDeps
                ports

        // 4. Instantiate Parent Capability API
        let apis = Capabilities.create obsAggApi undAggApi undDeps.DispatchEnvelope

        // 5. Wire workflow dependencies
        let workflowDeps: UnderstandDemandWorkflowDependencies =
            { ObservationCodec = Medhavi.Foundation.Codec.json
              UnderstandingCodec = Medhavi.Foundation.Codec.json
              Subscribe = obsDeps.Subscribe
              Apis = apis
              DefaultScopeId = defaultScopeId }

        let! obsSub: IDisposable =
            createDemandObservationWorkflow workflowDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError (fun err ->
                Infrastructure(EventStore($"Failed to create observation workflow: {err}")))

        let! epSub: IDisposable =
            createEnterprisePictureWorkflow workflowDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError (fun err ->
                Infrastructure(EventStore($"Failed to create enterprise picture workflow: {err}")))

        let! undPubSub: IDisposable =
            createDemandUnderstandingPublishWorkflow workflowDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError (fun err ->
                Infrastructure(EventStore($"Failed to create understanding publish workflow: {err}")))

        // 6. Combine lifecycle disposables
        let dispose () =
            obsQueryCtx.Dispose ()
            undQueryCtx.Dispose ()
            obsSub.Dispose ()
            epSub.Dispose ()
            undPubSub.Dispose ()

        return
            { Commands = apis
              ObservationQueries = obsQueryCtx.QueryService
              UnderstandingQueries = undQueryCtx.QueryService
              Dispose = dispose }
    }
