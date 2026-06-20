module Medhavi.Demand.Projections

open System
open Medhavi.Demand.Domain
open Medhavi.SharedKernel
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.Infrastructure.Projections
open Medhavi.Demand.Domain.DemandLineAgg

type DemandAgent = ProjectionAgent<Map<string, Medhavi.Contracts.Demand.DemandLine>, DemandLineEvent>
let mapToDTO (d: DemandLine) : Medhavi.Contracts.Demand.DemandLine =
    let skuIdStr = SkuId.value d.SkuId

    { DemandLineId = d.DemandLineId
      DemandOrderId = d.DemandOrderId
      SkuId = skuIdStr
      SkuCode = skuIdStr
      SkuName = skuIdStr
      CustomerId = d.CustomerId
      CustomerName = d.CustomerId
      StockingPointId = StockingPointId.value d.StockingPointId
      Priority = d.Priority
      DemandCategory = d.DemandCategory.ToString()
      IsFirm = d.IsFirm
      EarliestDeliveryDate = d.EarliestDeliveryDate |> Option.map(fun dt -> DateOnly.FromDateTime(dt.DateTime))
      RequestedDeliveryDate = DateOnly.FromDateTime(d.RequestedDeliveryDate.DateTime)
      LatestDeliveryDate = d.LatestDeliveryDate |> Option.map(fun dt -> DateOnly.FromDateTime(dt.DateTime))
      ConfirmedDeliveryDate = d.ConfirmedDeliveryDate |> Option.map(fun dt -> DateOnly.FromDateTime(dt.DateTime))
      RequestedQty = Quantity.value d.Quantity
      OpenQty = Quantity.value d.OpenQuantity
      FulfilledQty = Quantity.value d.FulfilledQuantity
      ConfirmedQty = Quantity.value d.Quantity - Quantity.value d.OpenQuantity
      ShortfallQty = Quantity.value d.OpenQuantity
      LatenessRisk = Medhavi.Contracts.Demand.LatenessRisk.OnTrack
      Status = d.Status.ToString()
      UnitOfMeasure = d.UnitOfMeasure
      PeggedSupply = [] }

let evolveProjection (state: Map<string, Medhavi.Contracts.Demand.DemandLine>) (evt: DemandLineEvent) =
    match evt with
    | DemandLineCreated dl -> Map.add dl.DemandLineId (mapToDTO dl) state
    | _ -> state
// | DemandLineFulfilled e ->
//     match Map.tryFind e.DemandLineId state with
//     | Some dl ->
//         let updated = applyFulfilled dl e
//         Map.add dl.DemandLineId updated state
//     | None -> state

let createProjectionAgent () =
    DemandAgent(
        evolveProjection,
        Map.empty,
        "DemandLineReadModel"
    )

let createDemandQueryService (agent: DemandAgent) =

    let apiEvents = Event<obj>()
    let queryService = QueryServiceBase.getQueryService agent id

    // Subscribe to agent's EventApplied and trigger corresponding API event notifications
    let _ =
        agent.EventApplied
        |> Observable.subscribe(fun ev ->
            match ev with
            | DemandLineCreated dl ->
                let notif: Medhavi.Contracts.Demand.DemandCreatedNotification = { DemandLineId = dl.DemandLineId }
                DomainEventBus.Publish(notif)
            | DemandLineFulfilled e ->
                let notif: Medhavi.Contracts.Demand.DemandUpdatedNotification = { DemandLineId = e.DemandLineId }
                DomainEventBus.Publish(notif))

    queryService

let seedProjections (demandAgent: DemandAgent) ( list: DemandLine list) =
    let m =
        list
        |> List.map mapToDTO
        |> List.map (fun d -> d.DemandLineId, d)
        |> Map.ofList

    demandAgent.SetState(m)
