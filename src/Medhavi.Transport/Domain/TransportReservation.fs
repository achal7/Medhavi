namespace Medhavi.Transport.Domain.TransportReservationAgg

open System
open Medhavi.SharedKernel
open Medhavi.Transport

/// State of a transport reservation aggregate
type TransportReservation =
    { Id: TransportReservationId
      IdempotencyKey: string
      ItineraryId: ItineraryId
      SkuId: string
      FromNode: string
      ToNode: string
      Quantity: decimal
      Status: TransportReservationStatus
      EarliestDeparture: DateTimeOffset
      EarliestArrival: DateTimeOffset
      ExpiryTime: DateTimeOffset
      CreatedAt: DateTimeOffset
      ModifiedAt: DateTimeOffset }

// ─── Commands ───────────────────────────────────────────────────────────────

type CreateTransportReservationCmd =
    { Id: TransportReservationId
      IdempotencyKey: string
      ItineraryId: ItineraryId
      SkuId: string
      FromNode: string
      ToNode: string
      Quantity: decimal
      EarliestDeparture: DateTimeOffset
      EarliestArrival: DateTimeOffset
      ExpiryTime: DateTimeOffset }

type ConfirmTransportReservationCmd = { Id: TransportReservationId }

type ReleaseTransportReservationCmd =
    { Id: TransportReservationId
      ReleasedAt: DateTimeOffset }

type ExpireTransportReservationCmd =
    { Id: TransportReservationId
      ExpiredAt: DateTimeOffset }

type TransportReservationCommand =
    | CreateTransportReservation of CreateTransportReservationCmd
    | ConfirmTransportReservation of ConfirmTransportReservationCmd
    | ReleaseTransportReservation of ReleaseTransportReservationCmd
    | ExpireTransportReservation of ExpireTransportReservationCmd

// ─── Events ─────────────────────────────────────────────────────────────────

type TransportReservationCreatedEvt =
    { Id: TransportReservationId
      IdempotencyKey: string
      ItineraryId: ItineraryId
      SkuId: string
      FromNode: string
      ToNode: string
      Quantity: decimal
      EarliestDeparture: DateTimeOffset
      EarliestArrival: DateTimeOffset
      ExpiryTime: DateTimeOffset
      CreatedAt: DateTimeOffset }

type TransportReservationConfirmedEvt =
    { Id: TransportReservationId
      ConfirmedAt: DateTimeOffset }

type TransportReservationReleasedEvt =
    { Id: TransportReservationId
      ReleasedAt: DateTimeOffset }

type TransportReservationExpiredEvt =
    { Id: TransportReservationId
      ExpiredAt: DateTimeOffset }

type TransportReservationEvent =
    | TransportReservationCreated of TransportReservationCreatedEvt
    | TransportReservationConfirmed of TransportReservationConfirmedEvt
    | TransportReservationReleased of TransportReservationReleasedEvt
    | TransportReservationExpired of TransportReservationExpiredEvt

// ─── Aggregate type aliases ─────────────────────────────────────────────────

type DecideTransportReservation = Decide<TransportReservation, TransportReservationCommand, TransportReservationEvent>
type EvolveTransportReservation = Evolve<TransportReservation, TransportReservationEvent>

// ─── Evolve ─────────────────────────────────────────────────────────────────

module Reservation =

    let private applyCreated (evt: TransportReservationCreatedEvt) : TransportReservation =
        { Id            = evt.Id
          IdempotencyKey = evt.IdempotencyKey
          ItineraryId   = evt.ItineraryId
          SkuId         = evt.SkuId
          FromNode      = evt.FromNode
          ToNode        = evt.ToNode
          Quantity      = evt.Quantity
          Status        = Tentative
          EarliestDeparture = evt.EarliestDeparture
          EarliestArrival   = evt.EarliestArrival
          ExpiryTime    = evt.ExpiryTime
          CreatedAt     = evt.CreatedAt
          ModifiedAt    = evt.CreatedAt }

    let private applyConfirmed (evt: TransportReservationConfirmedEvt) (s: TransportReservation) =
        { s with Status = Confirmed; ModifiedAt = evt.ConfirmedAt }

    let private applyReleased (evt: TransportReservationReleasedEvt) (s: TransportReservation) =
        { s with Status = Released; ModifiedAt = evt.ReleasedAt }

    let private applyExpired (evt: TransportReservationExpiredEvt) (s: TransportReservation) =
        { s with Status = Expired; ModifiedAt = evt.ExpiredAt }

    let evolve: EvolveTransportReservation =
        fun event stateOpt ->
            match event, stateOpt with
            | TransportReservationCreated e, None    -> Some(applyCreated e)
            | TransportReservationConfirmed e, Some s -> Some(applyConfirmed e s)
            | TransportReservationReleased e, Some s  -> Some(applyReleased e s)
            | TransportReservationExpired e, Some s   -> Some(applyExpired e s)
            | TransportReservationCreated _, Some s   -> Some s  // idempotent
            | _, _ -> stateOpt

    let decide: DecideTransportReservation =
        fun command stateOpt ->
            match command, stateOpt with

            // ─── Create ────────────────────────────────────────────────
            | CreateTransportReservation cmd, None ->
                if cmd.Quantity <= 0.0m then
                    Error(DomainError.validation "Reservation quantity must be greater than zero")
                elif cmd.ExpiryTime <= DateTimeOffset.UtcNow then
                    Error(DomainError.validation "Reservation expiry must be in the future")
                else
                    let now = DateTimeOffset.UtcNow
                    let evt =
                        { Id              = cmd.Id
                          IdempotencyKey   = cmd.IdempotencyKey
                          ItineraryId      = cmd.ItineraryId
                          SkuId            = cmd.SkuId
                          FromNode         = cmd.FromNode
                          ToNode           = cmd.ToNode
                          Quantity         = cmd.Quantity
                          EarliestDeparture = cmd.EarliestDeparture
                          EarliestArrival   = cmd.EarliestArrival
                          ExpiryTime       = cmd.ExpiryTime
                          CreatedAt        = now }
                    Ok { NewState = applyCreated evt; Events = [ TransportReservationCreated evt ] }

            | CreateTransportReservation _, Some _ ->
                Error(DomainError.invariant "Transport reservation already exists")

            // ─── Confirm ───────────────────────────────────────────────
            | ConfirmTransportReservation cmd, Some state ->
                match state.Status with
                | Tentative ->
                    let evt = { Id = cmd.Id; ConfirmedAt = DateTimeOffset.UtcNow }
                    Ok { NewState = applyConfirmed evt state; Events = [ TransportReservationConfirmed evt ] }
                | Confirmed -> Error(DomainError.invariant "Reservation is already confirmed")
                | Released  -> Error(DomainError.invariant "Cannot confirm a released reservation")
                | Expired   -> Error(DomainError.invariant "Cannot confirm an expired reservation")

            | ConfirmTransportReservation _, None ->
                Error(DomainError.validation "Transport reservation not found")

            // ─── Release ───────────────────────────────────────────────
            | ReleaseTransportReservation cmd, Some state ->
                match state.Status with
                | Released -> Error(DomainError.invariant "Reservation is already released")
                | Expired  -> Error(DomainError.invariant "Cannot release an expired reservation")
                | _ ->
                    let evt = { Id = cmd.Id; ReleasedAt = cmd.ReleasedAt }
                    Ok { NewState = applyReleased evt state; Events = [ TransportReservationReleased evt ] }

            | ReleaseTransportReservation _, None ->
                Error(DomainError.validation "Transport reservation not found")

            // ─── Expire ────────────────────────────────────────────────
            | ExpireTransportReservation cmd, Some state ->
                match state.Status with
                | Tentative ->
                    let evt = { Id = cmd.Id; ExpiredAt = cmd.ExpiredAt }
                    Ok { NewState = applyExpired evt state; Events = [ TransportReservationExpired evt ] }
                | Expired   -> Error(DomainError.invariant "Reservation is already expired")
                | _         -> Error(DomainError.invariant "Only tentative reservations can expire")

            | ExpireTransportReservation _, None ->
                Error(DomainError.validation "Transport reservation not found")
