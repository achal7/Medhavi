/// CA-P-003 Reserve Inventory and Capacity Temporarily — Model
/// Traces to: SE-P-030, CR-P-009, CR-P-010, CR-P-011
module Medhavi.Promise.ReserveInventory.TemporaryReservation.Model

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers

type ReservationLifecycleState =
    | Active
    | Confirmed
    | Released
    | Expired

type CapacityReservationSlot =
    { SlotId: string
      PhysicalResourceId: PhysicalResourceId
      ReservedHours: decimal
      WorkCenterShiftDate: Timestamp }

type InventoryReservationHold =
    { HoldId: string
      Location: LocationId
      Item: ItemId
      ReservedQuantity: Quantity
      BatchOrLotNumber: string option }

type TemporaryReservation =
    { ReservationToken: TemporaryReservationId
      OrderLineId: OrderLineId
      CustomerId: CustomerId
      Item: ItemId
      Location: LocationId
      RequestedQuantity: Quantity
      HeldQuantity: Quantity
      LeaseCreationTime: Timestamp
      LeaseExpirationTime: Timestamp
      CapacitySlots: CapacityReservationSlot list
      InventoryHolds: InventoryReservationHold list
      LifecycleState: ReservationLifecycleState
      DecisionTraceability: string option }

/// AB-P-014 Place Temporary Reservation Command
type CreateTemporaryHoldCmd =
    { ReservationToken: TemporaryReservationId
      OrderLineId: OrderLineId
      CustomerId: CustomerId
      Item: ItemId
      Location: LocationId
      Quantity: Quantity
      LeaseDurationMinutes: int
      CreationTime: Timestamp }

/// AB-P-016 Confirm Reservation Hold Command
type ConfirmHoldToCommitmentCmd =
    { ReservationToken: TemporaryReservationId
      ConfirmationTime: Timestamp }

/// AB-P-015 Release Temporary Reservation Command
type ReleaseTemporaryHoldCmd =
    { ReservationToken: TemporaryReservationId
      ReleaseReason: string
      ReleaseTime: Timestamp }

/// Expiration reaper Command
type ExpireTemporaryHoldCmd =
    { ReservationToken: TemporaryReservationId
      HeartbeatTime: Timestamp }

type TemporaryReservationCmd =
    | Create of CreateTemporaryHoldCmd
    | Confirm of ConfirmHoldToCommitmentCmd
    | Release of ReleaseTemporaryHoldCmd
    | Expire of ExpireTemporaryHoldCmd

    static member GetId(cmd: TemporaryReservationCmd) =
        match cmd with
        | Create c -> c.ReservationToken
        | Confirm c -> c.ReservationToken
        | Release c -> c.ReservationToken
        | Expire c -> c.ReservationToken

/// Domain Events emitted by Temporary Reservation Aggregate
type TemporaryReservationEvent =
    | TemporaryHoldCreated of TemporaryReservation
    | TemporaryHoldConfirmed of ReservationToken: TemporaryReservationId * ConfirmTime: Timestamp * TraceId: string
    | TemporaryHoldReleased of ReservationToken: TemporaryReservationId * Reason: string * ReleaseTime: Timestamp * TraceId: string
    | TemporaryHoldExpired of ReservationToken: TemporaryReservationId * ExpirationTime: Timestamp * TraceId: string

/// Pure state evolution (catamorphism)
let evolve: Evolve<TemporaryReservation, TemporaryReservationEvent> =
    fun (state: TemporaryReservation option) (event: TemporaryReservationEvent) ->
        match event with
        | TemporaryHoldCreated res -> Some res

        | TemporaryHoldConfirmed(token, _, traceId) ->
            state
            |> Option.map (fun r ->
                if r.ReservationToken = token then
                    { r with
                        LifecycleState = Confirmed
                        DecisionTraceability = Some traceId }
                else r)

        | TemporaryHoldReleased(token, _, _, traceId) ->
            state
            |> Option.map (fun r ->
                if r.ReservationToken = token then
                    { r with
                        LifecycleState = Released
                        DecisionTraceability = Some traceId }
                else r)

        | TemporaryHoldExpired(token, _, traceId) ->
            state
            |> Option.map (fun r ->
                if r.ReservationToken = token then
                    { r with
                        LifecycleState = Expired
                        DecisionTraceability = Some traceId }
                else r)

