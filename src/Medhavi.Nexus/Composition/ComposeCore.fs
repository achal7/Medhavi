module Medhavi.Nexus.Composition.ComposeCore

open Microsoft.Extensions.DependencyInjection
open Medhavi.Common
open Medhavi.Foundation
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Infrastructure
open Medhavi.Infrastructure.Projections
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.SemanticModel
open Medhavi.Core
open Medhavi.Core.DemandManagement.Context
open Medhavi.Core.DemandManagement.Model
open Medhavi.Core.EnterprisePictureManagement
open Medhavi.Core.EnterprisePictureManagement.Model
open Medhavi.Core.EnterprisePictureManagement.Context
open Medhavi.Core.ExceptionManagement.Context
open Medhavi.Core.ExceptionManagement.Model
open Medhavi.Core.ItemTransitionManagement.Context
open Medhavi.Core.ItemTransitionManagement.Model
open Medhavi.Core.ItemTransitionManagement.Policies

type CoreContext =
    { DemandManagement: DemandManagementContext
      ExceptionManagement: ExceptionManagementContext
      EnterprisePictureManagement: EnterprisePictureContext
      ItemTransitionManagement: ItemTransitionManagementContext
      Dispose: unit -> unit }

let traceCodec: Codec<DecisionTrace> = Codec.json<DecisionTrace>

let composeException
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (pconfig: ProjectionConfig) =
    let exceptionRepo = InMemRepository.create<Medhavi.SemanticModel.Exception, ExceptionId, ExceptionEvent>()
    let exceptionCodec = Codec.json<ExceptionEvent>
    let serializeEvent = exceptionCodec.Encode >> Result.mapError(fun c -> c.ToString())
    let exceptionPolicy = ExceptionManagement.Policies.ExceptionManagementPolicy.defaultPolicy

    let exceptionDeps = EnvelopeStoreDeps.create store ExceptionManagement.ACL.toArsEventId serializeEvent traceCodec

    let exceptionGetCurrentSeverity (_: ExceptionId) = task { return None }

    let projectionDeps: ProjectionDependencies<ExceptionEvent> =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = exceptionCodec
          TelemetryPublisher = telemetryPublisher }

    let projectionQry =
        createProjectionAgent<ExceptionEvent, Medhavi.Contracts.Core.Exception.ExceptionDto, ExceptionId> projectionDeps

    ExceptionManagement.Context.create exceptionRepo exceptionDeps publishKnowledge projectionQry exceptionPolicy

let composeDemandManagement
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (pconfig: ProjectionConfig) =

    let demandRepo = InMemRepository.create<Demand, DemandId, DemandEvent>()
    let demandCodec = Codec.json<DemandEvent>
    let demandPolicy = DemandManagement.Policies.DemandManagementPolicy.defaultPolicy
    let serializeEvent = demandCodec.Encode >> Result.mapError(fun c -> c.ToString())
    let demandDeps = EnvelopeStoreDeps.create store DemandManagement.ACL.toArsEventId serializeEvent traceCodec

    let demandProjectionDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = demandCodec
          TelemetryPublisher = telemetryPublisher }

    let demandProjection =
        createProjectionAgent<DemandEvent, Medhavi.Contracts.Core.Demand.DemandDto, DemandId> demandProjectionDeps

    DemandManagement.Context.create demandRepo demandDeps publishKnowledge demandProjection demandPolicy

let composeEnterprisePictureManagement
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (pconfig: ProjectionConfig) =


    let epicRepo = InMemRepository.create<EnterprisePicture, PlanningScopeId, EnterprisePictureEvent>()
    let epicCodec = Codec.json<EnterprisePictureEvent>
    let serializeEvent = epicCodec.Encode >> Result.mapError(fun c -> c.ToString())
    let epicDeps = EnvelopeStoreDeps.create store EnterprisePictureManagement.ACL.toArsEventId serializeEvent traceCodec

    let epicProjectionDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = epicCodec
          TelemetryPublisher = telemetryPublisher }

    let epicProjection =
        createProjectionAgent<EnterprisePictureEvent, Medhavi.Contracts.Core.EnterprisePicture, PlanningScopeId> epicProjectionDeps

    EnterprisePictureManagement.Context.create epicRepo epicDeps epicProjection publishKnowledge EnterprisePicturePolicy.defaultPolicy

let composeItemTransition
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (pconfig: ProjectionConfig) =


    let itemTransitionRepo = InMemRepository.create<ItemTransition, TransitionId, ItemTransitionEvent>()
    let itemTransitionCodec = Codec.json<ItemTransitionEvent>
    let serializeEvent = itemTransitionCodec.Encode >> Result.mapError(fun c -> c.ToString())
    let itemTransitionDeps = EnvelopeStoreDeps.create store ItemTransitionManagement.ACL.toArsEventId serializeEvent traceCodec

    let itemTransitionProjectionDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = itemTransitionCodec
          TelemetryPublisher = telemetryPublisher }

    let itemTransitionProjection =
        createProjectionAgent<ItemTransitionEvent, Medhavi.Contracts.Core.ItemTransition.ItemTransitionDto, TransitionId> itemTransitionProjectionDeps

    let getItemState: (ItemId -> System.Threading.Tasks.Task<option<ReferenceLifecycleState>>) =
        fun _ -> task { return Some ReferenceLifecycleState.Active }

    ItemTransitionManagement.Context.create itemTransitionRepo itemTransitionDeps publishKnowledge itemTransitionProjection ItemTransitionPolicy.defaultPolicy getItemState



let compose
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (pconfig: ProjectionConfig)
    (services: IServiceCollection)
    : TaskResult<CoreContext, ApplicationError> =
    taskResult{

    // Exception Management (CA-C-020)
    let! exceptionCtx:ExceptionManagementContext = composeException store publishKnowledge telemetryPublisher pconfig

    // Enterprise Demand (CA-C-022)
    let! demandCtx:DemandManagementContext = composeDemandManagement store publishKnowledge telemetryPublisher pconfig

    // Enterprise Picture Management (CA-C-019)
    let! epictureCtx: EnterprisePictureContext = composeEnterprisePictureManagement store publishKnowledge telemetryPublisher pconfig

    // CA-C-021 Item Transition Management Capabilities
    let! itemTransitionCtx: ItemTransitionManagementContext = composeItemTransition store publishKnowledge telemetryPublisher pconfig

    services.AddSingleton<ExceptionManagementContext>(exceptionCtx) |> ignore
    services.AddSingleton<DemandManagementContext>(demandCtx) |> ignore
    services.AddSingleton<EnterprisePictureContext>(epictureCtx) |> ignore
    services.AddSingleton<ItemTransitionManagementContext>(itemTransitionCtx) |> ignore

    let dispose () = () // nothing to dispose in memory

    return
        {   DemandManagement = demandCtx
            ExceptionManagement = exceptionCtx
            EnterprisePictureManagement = epictureCtx
            ItemTransitionManagement = itemTransitionCtx
            Dispose = dispose }
    }
