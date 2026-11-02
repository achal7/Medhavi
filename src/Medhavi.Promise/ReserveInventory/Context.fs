/// CA-P-003 Reserve Inventory and Capacity Temporarily Composition Root
module Medhavi.Promise.ReserveInventory.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Medhavi.Promise.ReserveInventory.TemporaryReservation.Model
open Medhavi.Promise.ReserveInventory.TemporaryReservation.Policies
open Medhavi.Promise.ReserveInventory.Workflows
open Capabilities

type ReserveInventoryContext =
    { Commands: ReserveInventoryApis
      ReservationQueries: QueryService<TemporaryReservationDto, TemporaryReservationId>
      Dispose: unit -> unit }

let create
    (repo: Repository<TemporaryReservation, TemporaryReservationId, TemporaryReservationEvent>)
    (deps: EnvelopeStoreDependencies<TemporaryReservationEvent>)
    (queryService: CreateQueryService<TemporaryReservationEvent, TemporaryReservationDto, TemporaryReservationId>)
    (policy: TemporaryReservationPolicy)
    (publishKnowledge: KnowledgeRepresentation)
    (ports: PromisePorts)
    (defaultScopeId: string)
    : TaskResult<ReserveInventoryContext, ApplicationError> =
    taskResult {

        // 1. Initialize Temporary Reservation projection
        let! resAggregates: TemporaryReservation list =
            repo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let resInitialState = TemporaryReservation.Projections.seedFromAggregates resAggregates

        let resValidEvents =
            [ ArsIdentifiers.EnterpriseEvents.temporaryHoldCreated.Id
              ArsIdentifiers.EnterpriseEvents.temporaryHoldConfirmed.Id
              ArsIdentifiers.EnterpriseEvents.temporaryHoldReleased.Id
              ArsIdentifiers.EnterpriseEvents.temporaryHoldExpired.Id ]
            |> EnvelopeFilter.EventTypes

        let! queryCtx: ProjectionContext<TemporaryReservationDto, TemporaryReservationId> =
            queryService TemporaryReservation.Projections.apply resValidEvents resInitialState "TemporaryReservation"

        // 2. Instantiate Aggregate APIs
        let aggApi =
            TemporaryReservation.Capabilities.create
                repo
                publishKnowledge
                policy
                deps
                ports

        // 3. Instantiate Capability Parent API
        let apis = Capabilities.create aggApi

        // 4. Wire workflow dependencies
        let workflowDeps: ReserveInventoryWorkflowDependencies =
            { ReservationCodec = Medhavi.Foundation.Codec.json
              Subscribe = deps.Subscribe
              DispatchEnvelope = deps.DispatchEnvelope
              Apis = apis
              DefaultScopeId = defaultScopeId }

        let! workflowSub: IDisposable =
            createReservationWorkflow workflowDeps CancellationToken.None

        // 5. Lifecycle management
        let dispose () =
            queryCtx.Dispose ()
            workflowSub.Dispose ()

        return
            { Commands = apis
              ReservationQueries = queryCtx.QueryService
              Dispose = dispose }
    }

