module Medhavi.Demand.EnterpriseDemandPicture.Projection

open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.Contracts.Demand.Edp

type EdpProjectionState = Map<string, EnterpriseDemandPicture> // PlanningScopeId -> latest

let mapToContract (edp: Model.EnterpriseDemandPicture) : EnterpriseDemandPicture =
    let periods =
        edp.PlanningDemand
        |> Map.toList
        |> List.map(fun (period, line) ->
            { Period = period
              OperationalDemand = Quantity.value line.OperationalDemand
              Adjustment = Quantity.value line.Adjustment
              Override = Quantity.value line.Override
              FinalQuantity = Quantity.value line.FinalQuantity })

    { PlanningScopeId = PlanningScopeId.value edp.PlanningScopeId
      Version = edp.Version
      Status = edp.Status.ToString()
      Periods = periods
      TransactionTime = Timestamp.value edp.TransactionTime
      PublicationTime = edp.PublicationTime |> Option.map Timestamp.value }

let evolveProjection (state: EdpProjectionState) (evt: Model.EdpEvent) =
    match evt with
    | Model.EdpRevised edp
    | Model.EdpCalculated edp
    | Model.EdpPublished(edp, _) ->
        let contract = mapToContract edp
        Map.add (PlanningScopeId.value edp.PlanningScopeId) contract state

type EdpProjectionAgent = ProjectionAgent<EdpProjectionState, Model.EdpEvent>

let createProjectionAgent () = ProjectionAgent(evolveProjection, Map.empty, "EnterpriseDemandPictureReadModel")

let seedProjections (agent: EdpProjectionAgent) (list: Model.EnterpriseDemandPicture list) =
    let map = list |> List.map(fun p -> PlanningScopeId.value p.PlanningScopeId, mapToContract p) |> Map.ofList
    agent.SetState map

let createQueryService (agent: EdpProjectionAgent) =
    let queryService = QueryServiceBase.getQueryService agent id

    // Subscribe to events for UI notifications
    // let _ =
    //     agent.EventApplied
    //     |> Observable.subscribe(fun ev ->
    //         match ev with
    //         | EnterpriseDemandPictureAgg.EdpRevised edp ->
    //             DomainEventBus.Publish(
    //                 { PlanningScopeId = PlanningScopeId.value edp.PlanningScopeId }: EdpUpdatedNotification
    //             )
    //         | EnterpriseDemandPictureAgg.EdpCalculated edp ->
    //             DomainEventBus.Publish(
    //                 { PlanningScopeId = PlanningScopeId.value edp.PlanningScopeId }: EdpUpdatedNotification
    //             )

    queryService
