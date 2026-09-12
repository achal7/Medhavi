module Medhavi.Nexus.Composition.ComposeClient

open System
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection
open Medhavi.Contracts
open Medhavi.Contracts.Client
open Medhavi.Contracts.Core.Demand
open Medhavi.Contracts.Core.Exception
open Medhavi.Contracts.Core.ItemTransition
open Medhavi.Contracts.Demand
open Medhavi.Client
open Medhavi.SemanticModel
open Medhavi.Demand

let private adaptDemandQueries (q: Medhavi.Foundation.Contracts.QueryService<DemandDto, DemandId>) : DemandQueries =
    { GetAll = q.GetAll
      GetById =
        fun (id: string) ->
            match DemandId.create id with
            | Ok dId -> q.GetById dId
            | Error _ -> Task.FromResult None
      Exists =
        fun (id: string) ->
            match DemandId.create id with
            | Ok dId -> q.Exists dId
            | Error _ -> Task.FromResult false
      Filter = q.Filter
      SubscribeApiEvents = q.SubscribeApiEvents }

let private adaptExceptionQueries
    (q: Medhavi.Foundation.Contracts.QueryService<ExceptionDto, ExceptionId>)
    : ExceptionQueries =
    { GetAll = q.GetAll
      GetById =
        fun (id: string) ->
            match ExceptionId.create id with
            | Ok eId -> q.GetById eId
            | Error _ -> Task.FromResult None
      Exists =
        fun (id: string) ->
            match ExceptionId.create id with
            | Ok eId -> q.Exists eId
            | Error _ -> Task.FromResult false
      Filter = q.Filter
      SubscribeApiEvents = q.SubscribeApiEvents }

let private adaptItemTransitionQueries
    (q: Medhavi.Foundation.Contracts.QueryService<ItemTransitionDto, TransitionId>)
    : ItemTransitionQueries =
    { GetAll = q.GetAll
      GetById =
        fun (id: string) ->
            match TransitionId.create id with
            | Ok tId -> q.GetById tId
            | Error _ -> Task.FromResult None
      Exists =
        fun (id: string) ->
            match TransitionId.create id with
            | Ok tId -> q.Exists tId
            | Error _ -> Task.FromResult false
      Filter = q.Filter
      SubscribeApiEvents = q.SubscribeApiEvents }

let private adaptObservationQueries
    (q: Medhavi.Foundation.Contracts.QueryService<DemandObservationDto, DemandObservationId>)
    : DemandObservationQueries =
    { GetAll = q.GetAll
      GetById =
        fun (id: string) ->
            match DemandObservationId.create id with
            | Ok oId -> q.GetById oId
            | Error _ -> Task.FromResult None
      Exists =
        fun (id: string) ->
            match DemandObservationId.create id with
            | Ok oId -> q.Exists oId
            | Error _ -> Task.FromResult false
      Filter = q.Filter
      SubscribeApiEvents = q.SubscribeApiEvents }

let private adaptUnderstandingQueries
    (q: Medhavi.Foundation.Contracts.QueryService<DemandUnderstandingDto, PlanningScopeId>)
    : DemandUnderstandingQueries =
    { GetAll = q.GetAll
      GetById =
        fun (id: string) ->
            match PlanningScopeId.create id with
            | Ok sId -> q.GetById sId
            | Error _ -> Task.FromResult None
      Exists =
        fun (id: string) ->
            match PlanningScopeId.create id with
            | Ok sId -> q.Exists sId
            | Error _ -> Task.FromResult false
      Filter = q.Filter
      SubscribeApiEvents = q.SubscribeApiEvents }

let addApplicationServices
    (configuration: ClientConfiguration)
    (nexusCtx: ComposeNexus.NexusContext)
    (services: IServiceCollection)
    : IServiceCollection =

    // 1. Register Clock & ClientConfiguration
    services.AddSingleton<Clock>(system) |> ignore
    services.AddSingleton<ClientConfiguration>(configuration) |> ignore

    // 2. Core Domain Services (CA-C-019 .. CA-C-022)
    let demandApi: DemandApi = nexusCtx.Core.DemandManagement.Commands
    let demandQueries: DemandQueries = adaptDemandQueries nexusCtx.Core.DemandManagement.Queries

    let exceptionApi: ExceptionApi = nexusCtx.Core.ExceptionManagement.Commands
    let exceptionQueries: ExceptionQueries = adaptExceptionQueries nexusCtx.Core.ExceptionManagement.Queries

    let itemTransitionApi: ItemTransitionApi = nexusCtx.Core.ItemTransitionManagement.Commands

    let itemTransitionQueries: ItemTransitionQueries =
        adaptItemTransitionQueries nexusCtx.Core.ItemTransitionManagement.Queries

    let coreServices: CoreDomainServices =
        { DemandManagement =
            { Commands = demandApi
              Queries = demandQueries }
          ExceptionManagement =
            { Commands = exceptionApi
              Queries = exceptionQueries }
          ItemTransitionManagement =
            { Commands = itemTransitionApi
              Queries = itemTransitionQueries } }

    services.AddSingleton<CoreDomainServices>(coreServices) |> ignore
    services.AddSingleton<DemandApi>(demandApi) |> ignore
    services.AddSingleton<DemandQueries>(demandQueries) |> ignore
    services.AddSingleton<ExceptionApi>(exceptionApi) |> ignore
    services.AddSingleton<ExceptionQueries>(exceptionQueries) |> ignore
    services.AddSingleton<ItemTransitionApi>(itemTransitionApi) |> ignore
    services.AddSingleton<ItemTransitionQueries>(itemTransitionQueries) |> ignore

    // 3. Demand Intelligence Domain Services (CA-D-001)
    let demandObsApi: DemandObservationApi = nexusCtx.Demand.UnderstandDemandContext.Commands.Observations

    let demandObsQueries: DemandObservationQueries =
        adaptObservationQueries nexusCtx.Demand.UnderstandDemandContext.ObservationQueries

    let demandUndApi: DemandUnderstandingApi = nexusCtx.Demand.UnderstandDemandContext.Commands.Understanding

    let demandUndQueries: DemandUnderstandingQueries =
        adaptUnderstandingQueries nexusCtx.Demand.UnderstandDemandContext.UnderstandingQueries

    let demandServices: DemandDomainServices =
        { UnderstandDemand =
            { Observation =
                { Commands = demandObsApi
                  Queries = demandObsQueries }
              Understanding =
                { Commands = demandUndApi
                  Queries = demandUndQueries } } }

    services.AddSingleton<DemandDomainServices>(demandServices) |> ignore
    services.AddSingleton<DemandObservationApi>(demandObsApi) |> ignore
    services.AddSingleton<DemandObservationQueries>(demandObsQueries) |> ignore
    services.AddSingleton<DemandUnderstandingApi>(demandUndApi) |> ignore
    services.AddSingleton<DemandUnderstandingQueries>(demandUndQueries) |> ignore

    // 4. Platform Domain Services Composite
    let domainServices: DomainServices =
        { Core = coreServices
          Demand = demandServices }

    services.AddSingleton<DomainServices>(domainServices) |> ignore

    services
