/// CA-P-003 Reserve Inventory and Capacity Temporarily Projections
module Medhavi.Promise.ReserveInventory.TemporaryReservation.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let mapCapacitySlotToDto (s: CapacityReservationSlot) : CapacityReservationSlotDto =
    { SlotId = s.SlotId
      PhysicalResourceId = PhysicalResourceId.value s.PhysicalResourceId
      ReservedHours = s.ReservedHours
      WorkCenterShiftDate = Timestamp.value s.WorkCenterShiftDate }

let mapInventoryHoldToDto (h: InventoryReservationHold) : InventoryReservationHoldDto =
    { HoldId = h.HoldId
      Location = LocationId.value h.Location
      Item = ItemId.value h.Item
      ReservedQuantity = Quantity.value h.ReservedQuantity
      BatchOrLotNumber = h.BatchOrLotNumber }

let mapToDto (res: TemporaryReservation) : TemporaryReservationDto =
    { ReservationToken = TemporaryReservationId.value res.ReservationToken
      OrderLineId = OrderLineId.value res.OrderLineId
      CustomerId = CustomerId.value res.CustomerId
      Item = ItemId.value res.Item
      Location = LocationId.value res.Location
      RequestedQuantity = Quantity.value res.RequestedQuantity
      HeldQuantity = Quantity.value res.HeldQuantity
      LeaseCreationTime = Timestamp.value res.LeaseCreationTime
      LeaseExpirationTime = Timestamp.value res.LeaseExpirationTime
      CapacitySlots = res.CapacitySlots |> List.map mapCapacitySlotToDto
      InventoryHolds = res.InventoryHolds |> List.map mapInventoryHoldToDto
      LifecycleState =
        match res.LifecycleState with
        | Active -> "Active"
        | Confirmed -> "Confirmed"
        | Released -> "Released"
        | Expired -> "Expired"
      DecisionTraceability = res.DecisionTraceability }

type State = Map<TemporaryReservationId, TemporaryReservationDto>

let initial: State = Map.empty

let apply (state: State) (event: TemporaryReservationEvent) : State =
    match event with
    | TemporaryHoldCreated res ->
        state |> Map.add res.ReservationToken (mapToDto res)

    | TemporaryHoldConfirmed(token, _, traceId) ->
        state
        |> Map.change token (Option.map (fun dto ->
            { dto with
                LifecycleState = "Confirmed"
                DecisionTraceability = Some traceId }))

    | TemporaryHoldReleased(token, _, _, traceId) ->
        state
        |> Map.change token (Option.map (fun dto ->
            { dto with
                LifecycleState = "Released"
                DecisionTraceability = Some traceId }))

    | TemporaryHoldExpired(token, _, traceId) ->
        state
        |> Map.change token (Option.map (fun dto ->
            { dto with
                LifecycleState = "Expired"
                DecisionTraceability = Some traceId }))

let seedFromAggregates (aggregates: TemporaryReservation list) : State =
    aggregates
    |> List.fold (fun state res -> Map.add res.ReservationToken (mapToDto res) state) initial

