module Medhavi.Demand.Projections

open System
open Medhavi.Demand.Domain
open Medhavi.SharedKernel
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.Infrastructure.Projections
open Medhavi.Demand.Domain.DemandLineAgg

type DemandProjectionState = Map<string, Medhavi.Contracts.Demand.DemandLine>

type DemandAgent = ProjectionAgent<DemandProjectionState, DemandLineEvent>

let private formatDate (dt: DateTimeOffset) = dt.ToString("yyyy-MM-dd")

let mapToDtoStatus (dlineStatu: DemandStatus) =
    match dlineStatu with
    | DemandStatus.Cancelled -> Medhavi.Contracts.Demand.DemandLineStatus.Cancelled
    | DemandStatus.Fulfilled -> Medhavi.Contracts.Demand.DemandLineStatus.Fulfilled
    | DemandStatus.Active -> Medhavi.Contracts.Demand.DemandLineStatus.Active

let mapToDtoCategory (category: DemandCategory) : Medhavi.Contracts.Demand.DemandCategory =
    match category with
    | CustomerOrderDemand -> Medhavi.Contracts.Demand.DemandCategory.CustomerOrderDemand
    | SalesOrderForecast -> Medhavi.Contracts.Demand.DemandCategory.SalesOrderForecast
    | InterplantTransfer -> Medhavi.Contracts.Demand.DemandCategory.InterplantTransfer
    | ServicePart -> Medhavi.Contracts.Demand.DemandCategory.ServicePart
    | InternalConsumption -> Medhavi.Contracts.Demand.DemandCategory.InternalConsumption
    | DependentDemand -> Medhavi.Contracts.Demand.DemandCategory.DependentDemand

let calculateLatenessRisk (confirmed: DateOnly option) (latest: DateOnly option) (requested: DateOnly) =
    match confirmed, latest with
    | None, _ -> Medhavi.Contracts.Demand.LatenessRisk.Critical
    | Some conf, Some lateBound when conf > lateBound -> Medhavi.Contracts.Demand.LatenessRisk.Critical
    | Some conf, _ when conf > requested ->
        let diff = conf.DayNumber - requested.DayNumber
        Medhavi.Contracts.Demand.LatenessRisk.AtRisk diff
    | _ -> Medhavi.Contracts.Demand.LatenessRisk.OnTrack

let mapToDTO (d: DemandLine) : Medhavi.Contracts.Demand.DemandLine =
    let skuIdStr = SkuId.value d.SkuId
    let stockingPointIdStr = StockingPointId.value d.StockingPointId
    let statusStr = mapToDtoStatus d.Status

    let earliestDate =
        d.EarliestDeliveryDate |> Option.map(fun ts -> DateOnly.FromDateTime((Timestamp.value ts).DateTime))

    let requestedDate = DateOnly.FromDateTime((Timestamp.value d.RequestedDeliveryDate).DateTime)
    let latestDate = d.LatestDeliveryDate |> Option.map(fun ts -> DateOnly.FromDateTime((Timestamp.value ts).DateTime))

    let confirmedDate =
        d.ConfirmedDeliveryDate |> Option.map(fun ts -> DateOnly.FromDateTime((Timestamp.value ts).DateTime))

    let latenessRisk = calculateLatenessRisk confirmedDate latestDate requestedDate

    let requestedQty = Quantity.value d.Quantity
    let openQty = Quantity.value d.OpenQuantity
    let confirmedQty = Quantity.value d.ConfirmedQty
    let shortfallQty = max 0m (openQty - confirmedQty)

    { DemandLineId = d.DemandLineId
      DemandOrderId = d.DemandOrderId
      SkuId = skuIdStr
      SkuCode = skuIdStr
      SkuName = skuIdStr
      CustomerId = d.CustomerId
      CustomerName = d.CustomerId
      StockingPointId = stockingPointIdStr
      Priority = DemandPriority.weight d.Priority
      DemandCategory = mapToDtoCategory d.DemandCategory
      IsFirm = d.IsFirm
      IsFrozen = d.IsFrozen
      FrozenUntilUtc = d.FrozenUntilUtc |> Option.map Timestamp.value
      IsOnHold = d.IsOnHold
      OnHoldReason = d.OnHoldReason
      CancelReason = d.CancelReason
      EarliestDeliveryDate = earliestDate
      RequestedDeliveryDate = requestedDate
      LatestDeliveryDate = latestDate
      ConfirmedDeliveryDate = confirmedDate
      RequestedQty = requestedQty
      OpenQty = openQty
      FulfilledQty = Quantity.value d.FulfilledQuantity
      ConfirmedQty = confirmedQty
      ShortfallQty = shortfallQty
      LatenessRisk = latenessRisk
      Status = statusStr
      UnitOfMeasure = d.UnitOfMeasure
      PeggedSupply = [] }

let evolveProjection (state: DemandProjectionState) (evt: DemandLineEvent) : DemandProjectionState =
    match evt with
    | DemandLineIngested dl -> Map.add dl.DemandLineId (mapToDTO dl) state

    | DemandLineRevised evt ->
        Map.tryFind evt.DemandLineId state
        |> Option.map(fun dto ->
            let updatedDto =
                { dto with
                    EarliestDeliveryDate =
                        evt.EarliestDeliveryDate
                        |> Option.map(fun ts -> DateOnly.FromDateTime((Timestamp.value ts).DateTime))
                        |> Option.orElse dto.EarliestDeliveryDate
                    IsFirm = evt.IsFirm |> Option.defaultValue dto.IsFirm
                    IsFrozen = evt.IsFrozen |> Option.defaultValue dto.IsFrozen
                    LatestDeliveryDate =
                        evt.LatestDeliveryDate
                        |> Option.map(fun ts -> DateOnly.FromDateTime((Timestamp.value ts).DateTime))
                        |> Option.orElse dto.LatestDeliveryDate
                    RequestedDeliveryDate =
                        evt.RequestedDeliveryDate
                        |> Option.map(fun ts -> DateOnly.FromDateTime((Timestamp.value ts).DateTime))
                        |> Option.defaultValue dto.RequestedDeliveryDate
                    Priority = evt.Priority |> Option.map DemandPriority.weight |> Option.defaultValue dto.Priority
                    RequestedQty = evt.Quantity |> Option.map Quantity.value |> Option.defaultValue dto.RequestedQty
                    OpenQty =
                        evt.Quantity
                        |> Option.map(fun q -> (Quantity.value q) - dto.FulfilledQty)
                        |> Option.defaultValue dto.OpenQty }

            let shortfall = max 0m (updatedDto.OpenQty - updatedDto.ConfirmedQty)

            let risk =
                calculateLatenessRisk
                    updatedDto.ConfirmedDeliveryDate
                    updatedDto.LatestDeliveryDate
                    updatedDto.RequestedDeliveryDate

            let finalDto =
                { updatedDto with
                    ShortfallQty = shortfall
                    LatenessRisk = risk }

            Map.add evt.DemandLineId finalDto state)
        |> Option.defaultValue state

    | DemandLinePromised evt ->
        Map.tryFind evt.DemandLineId state
        |> Option.map(fun dto ->
            let confirmedDate = Some(DateOnly.FromDateTime((Timestamp.value evt.PromisedDate).DateTime))
            let confirmedQty = Quantity.value evt.ConfirmedQty
            let shortfallQty = max 0m (dto.OpenQty - confirmedQty)
            let risk = calculateLatenessRisk confirmedDate dto.LatestDeliveryDate dto.RequestedDeliveryDate

            let updatedDto =
                { dto with
                    ConfirmedDeliveryDate = confirmedDate
                    ConfirmedQty = confirmedQty
                    ShortfallQty = shortfallQty
                    LatenessRisk = risk }

            Map.add evt.DemandLineId updatedDto state)
        |> Option.defaultValue state

    | DemandLineConfirmed evt ->
        Map.tryFind evt.DemandLineId state
        |> Option.map(fun dto ->
            let confirmedDate = Some(DateOnly.FromDateTime((Timestamp.value evt.ConfirmedDate).DateTime))
            let confirmedQty = Quantity.value evt.ConfirmedQty
            let shortfallQty = max 0m (dto.OpenQty - confirmedQty)
            let risk = calculateLatenessRisk confirmedDate dto.LatestDeliveryDate dto.RequestedDeliveryDate

            let updatedDto =
                { dto with
                    ConfirmedDeliveryDate = confirmedDate
                    ConfirmedQty = confirmedQty
                    ShortfallQty = shortfallQty
                    LatenessRisk = risk }

            Map.add evt.DemandLineId updatedDto state)
        |> Option.defaultValue state

    | DemandLineFrozen evt ->
        Map.tryFind evt.DemandLineId state
        |> Option.map(fun dto ->
            let updatedDto =
                { dto with
                    IsFrozen = true
                    FrozenUntilUtc = Some(Timestamp.value evt.FrozenUntilUtc) }

            Map.add evt.DemandLineId updatedDto state)
        |> Option.defaultValue state

    | DemandPlacedOnHold evt ->
        Map.tryFind evt.DemandLineId state
        |> Option.map(fun dto ->
            let updatedDto =
                { dto with
                    IsOnHold = true
                    OnHoldReason = Some evt.Reason }

            Map.add evt.DemandLineId updatedDto state)
        |> Option.defaultValue state

    | DemandLineReleased evt ->
        Map.tryFind evt.DemandLineId state
        |> Option.map(fun dto ->
            let nextFrozen = if evt.Unfreeze then false else dto.IsFrozen
            let nextFrozenUntil = if evt.Unfreeze then None else dto.FrozenUntilUtc
            let nextIsOnHold = if evt.ReleaseFromHold then false else dto.IsOnHold
            let nextOnHoldReason = if evt.ReleaseFromHold then None else dto.OnHoldReason

            let updatedDto =
                { dto with
                    IsFrozen = nextFrozen
                    FrozenUntilUtc = nextFrozenUntil
                    IsOnHold = nextIsOnHold
                    OnHoldReason = nextOnHoldReason }

            Map.add evt.DemandLineId updatedDto state)
        |> Option.defaultValue state

    | DemandLineCancelled evt ->
        Map.tryFind evt.DemandLineId state
        |> Option.map(fun dto ->
            let updatedDto =
                { dto with
                    OpenQty = 0m
                    ShortfallQty = 0m
                    CancelReason = Some evt.Reason
                    Status = Medhavi.Contracts.Demand.DemandLineStatus.Cancelled }

            Map.add evt.DemandLineId updatedDto state)
        |> Option.defaultValue state

    | DemandLineFulfillmentRecorded evt ->
        Map.tryFind evt.DemandLineId state
        |> Option.map(fun dto ->
            let recordQty = Quantity.value evt.Quantity
            let nextFulfilled = dto.FulfilledQty + recordQty
            let nextOpen = max 0m (dto.OpenQty - recordQty)
            let nextShortfall = max 0m (nextOpen - dto.ConfirmedQty)

            let nextStatus =
                if nextOpen <= 0m then
                    Medhavi.Contracts.Demand.DemandLineStatus.Fulfilled
                else
                    Medhavi.Contracts.Demand.DemandLineStatus.Active

            let updatedDto =
                { dto with
                    FulfilledQty = nextFulfilled
                    OpenQty = nextOpen
                    ShortfallQty = nextShortfall
                    Status = nextStatus }

            Map.add evt.DemandLineId updatedDto state)
        |> Option.defaultValue state

let createProjectionAgent () = DemandAgent(evolveProjection, Map.empty, "DemandLineReadModel")

let createDemandQueryService (agent: DemandAgent) =
    let _ = Event<obj>()
    let queryService = QueryServiceBase.getQueryService agent id

    // Subscribe to agent's EventApplied and trigger corresponding API event notifications
    let _ =
        agent.EventApplied
        |> Observable.subscribe(fun ev ->
            match ev with
            | DemandLineIngested dl ->
                let notif: Medhavi.Contracts.Demand.DemandCreatedNotification = { DemandLineId = dl.DemandLineId }
                DomainEventBus.Publish(notif)
            | DemandLineRevised e ->
                let notif: Medhavi.Contracts.Demand.DemandUpdatedNotification = { DemandLineId = e.DemandLineId }
                DomainEventBus.Publish(notif)
            | DemandLinePromised e ->
                let notif: Medhavi.Contracts.Demand.DemandUpdatedNotification = { DemandLineId = e.DemandLineId }
                DomainEventBus.Publish(notif)
            | DemandLineFrozen e ->
                let notif: Medhavi.Contracts.Demand.DemandUpdatedNotification = { DemandLineId = e.DemandLineId }
                DomainEventBus.Publish(notif)
            | DemandLineReleased e ->
                let notif: Medhavi.Contracts.Demand.DemandUpdatedNotification = { DemandLineId = e.DemandLineId }
                DomainEventBus.Publish(notif)
            | DemandLineCancelled e ->
                let notif: Medhavi.Contracts.Demand.DemandUpdatedNotification = { DemandLineId = e.DemandLineId }
                DomainEventBus.Publish(notif)
            | DemandLineFulfillmentRecorded e ->
                let notif: Medhavi.Contracts.Demand.DemandUpdatedNotification = { DemandLineId = e.DemandLineId }
                DomainEventBus.Publish(notif)
            | DemandLineConfirmed(_) -> failwith "Not Implemented"
            | DemandPlacedOnHold(_) -> failwith "Not Implemented")

    queryService

let seedProjections (demandAgent: DemandAgent) (list: DemandLine list) =
    let m = list |> List.map mapToDTO |> List.map(fun d -> d.DemandLineId, d) |> Map.ofList

    demandAgent.SetState(m)
