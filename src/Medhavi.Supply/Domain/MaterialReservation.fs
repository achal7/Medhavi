module Medhavi.Supply.Domain.MaterialReservationAgg

open System
open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate

type MaterialReservation =
    { Id: string
      IdempotencyKey: string
      SkuId: SkuId
      StockingPointId: StockingPointId
      Quantity: Quantity
      State: string // "Tentative", "Confirmed", "Released", "Expired", "Reduced"
      RequiredDate: DateTimeOffset
      ExpiryTime: DateTimeOffset
      Created: Timestamp
      Modified: Timestamp }

type CreateTentativeCmd =
    { Id: string
      IdempotencyKey: string
      SkuId: SkuId
      StockingPointId: StockingPointId
      Quantity: decimal
      RequiredDate: DateTimeOffset
      ExpiryTime: DateTimeOffset }

type ConfirmCmd = { Id: string }
type ReleaseCmd = { Id: string }
type ReduceCmd = { Id: string; NewQuantity: decimal }
type ExpireCmd = { Id: string }

type MaterialReservationCommand =
    | CreateTentative of CreateTentativeCmd
    | Confirm of ConfirmCmd
    | Release of ReleaseCmd
    | Reduce of ReduceCmd
    | Expire of ExpireCmd

// Events
type ReservationCreatedEvt = MaterialReservation
type ReservationConfirmedEvt = { Id: string }
type ReservationReleasedEvt = { Id: string }
type ReservationReducedEvt = { Id: string; NewQuantity: decimal }
type ReservationExpiredEvt = { Id: string }

type MaterialReservationEvent =
    | ReservationCreated of ReservationCreatedEvt
    | ReservationConfirmed of ReservationConfirmedEvt
    | ReservationReleased of ReservationReleasedEvt
    | ReservationReduced of ReservationReducedEvt
    | ReservationExpired of ReservationExpiredEvt

type DecideMaterialReservation = Decide<MaterialReservation, MaterialReservationCommand, MaterialReservationEvent>
type EvolveMaterialReservation = Evolve<MaterialReservation, MaterialReservationEvent>

let validateAndCreateTentative now cmd =
    let makeReservation id qty =
        { Id = id
          IdempotencyKey = cmd.IdempotencyKey
          SkuId = cmd.SkuId
          StockingPointId = cmd.StockingPointId
          Quantity = qty
          State = "Tentative"
          RequiredDate = cmd.RequiredDate
          ExpiryTime = cmd.ExpiryTime
          Created = now
          Modified = Timestamp.minValue }

    makeReservation cmd.Id
    <!> (Quantity.create cmd.Quantity |> fromResult)

let decide: DecideMaterialReservation =
    fun command stateOpt ->
        match command, stateOpt with
        | CreateTentative cmd, None ->
            createAggregate (validateAndCreateTentative Timestamp.now) (fun res -> [ ReservationCreated res ]) cmd
            
        | CreateTentative cmd, Some _ ->
            Error(DomainError.validation $"Material reservation with Id '{cmd.Id}' already exists.")

        | Confirm cmd, Some state ->
            if state.State <> "Tentative" then
                Error(DomainError.validation $"Cannot confirm reservation in state '{state.State}'. Only Tentative reservations can be confirmed.")
            else
                Ok { NewState = { state with State = "Confirmed"; Modified = Timestamp.now }
                     Events = [ ReservationConfirmed { Id = cmd.Id } ] }

        | Release cmd, Some state ->
            if state.State <> "Tentative" && state.State <> "Confirmed" then
                Error(DomainError.validation $"Cannot release reservation in state '{state.State}'. Only Tentative or Confirmed reservations can be released.")
            else
                Ok { NewState = { state with State = "Released"; Modified = Timestamp.now }
                     Events = [ ReservationReleased { Id = cmd.Id } ] }

        | Reduce cmd, Some state ->
            if state.State <> "Confirmed" then
                Error(DomainError.validation $"Cannot reduce reservation in state '{state.State}'. Only Confirmed reservations can be reduced.")
            else
                match Quantity.create cmd.NewQuantity with
                | Error err -> Error err
                | Ok newQty ->
                    if newQty >= state.Quantity then
                        Error(DomainError.validation "New quantity must be strictly less than current quantity to reduce reservation.")
                    else
                        Ok { NewState = { state with Quantity = newQty; State = "Reduced"; Modified = Timestamp.now }
                             Events = [ ReservationReduced { Id = cmd.Id; NewQuantity = cmd.NewQuantity } ] }

        | Expire cmd, Some state ->
            if state.State <> "Tentative" then
                Error(DomainError.validation $"Cannot expire reservation in state '{state.State}'. Only Tentative reservations can be expired.")
            else
                Ok { NewState = { state with State = "Expired"; Modified = Timestamp.now }
                     Events = [ ReservationExpired { Id = cmd.Id } ] }

        | Confirm _, None
        | Release _, None
        | Reduce _, None
        | Expire _, None ->
            Error(DomainError.validation "Reservation not found.")

let evolve (state: MaterialReservation option) (event: MaterialReservationEvent) : MaterialReservation option =
    match event with
    | ReservationCreated r -> Some r
    | ReservationConfirmed e -> state |> Option.map (fun s -> { s with State = "Confirmed"; Modified = Timestamp.now })
    | ReservationReleased e -> state |> Option.map (fun s -> { s with State = "Released"; Modified = Timestamp.now })
    | ReservationReduced e -> state |> Option.map (fun s -> { s with Quantity = Quantity.clampToZero e.NewQuantity; State = "Reduced"; Modified = Timestamp.now })
    | ReservationExpired e -> state |> Option.map (fun s -> { s with State = "Expired"; Modified = Timestamp.now })
