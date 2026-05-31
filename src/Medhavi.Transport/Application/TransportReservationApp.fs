module Medhavi.Transport.Application.TransportReservationApp

open System
open Medhavi.Common.Patterns
open Medhavi.Infrastructure
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate
open Medhavi.Transport
open Medhavi.Transport.Domain.TransportReservationAgg
open Medhavi.Infrastructure.Projections

type Decision = Decision<TransportReservation, TransportReservationEvent>

type TransportReservationCapabilities =
    { Create:  CreateTransportReservationCmd -> TaskResult<Decision, ApplicationError>
      Confirm: ConfirmTransportReservationCmd -> TaskResult<Decision, ApplicationError>
      Release: ReleaseTransportReservationCmd -> TaskResult<Decision, ApplicationError>
      Expire:  ExpireTransportReservationCmd  -> TaskResult<Decision, ApplicationError> }

let createCapabilities
    (repo: Repository<TransportReservation, string, TransportReservationEvent>)
    : TransportReservationCapabilities =
    { Create  = handleCommand (fun c -> TransportReservationId.value c.Id) repo CreateTransportReservation Reservation.decide
      Confirm = handleCommand (fun c -> TransportReservationId.value c.Id) repo ConfirmTransportReservation Reservation.decide
      Release = handleCommand (fun c -> TransportReservationId.value c.Id) repo ReleaseTransportReservation Reservation.decide
      Expire  = handleCommand (fun c -> TransportReservationId.value c.Id) repo ExpireTransportReservation  Reservation.decide }

let evolveProjection
    (state: Map<string, TransportReservation>)
    (evt: TransportReservationEvent)
    : Map<string, TransportReservation> =
    match evt with
    | TransportReservationCreated e ->
        let id = TransportReservationId.value e.Id
        let newRes = Reservation.evolve (TransportReservationCreated e) None
        match newRes with
        | Some r -> Map.add id r state
        | None   -> state
    | TransportReservationConfirmed e ->
        let id = TransportReservationId.value e.Id
        match Map.tryFind id state with
        | Some existing ->
            match Reservation.evolve (TransportReservationConfirmed e) (Some existing) with
            | Some updated -> Map.add id updated state
            | None         -> state
        | None -> state
    | TransportReservationReleased e ->
        let id = TransportReservationId.value e.Id
        match Map.tryFind id state with
        | Some existing ->
            match Reservation.evolve (TransportReservationReleased e) (Some existing) with
            | Some updated -> Map.add id updated state
            | None         -> state
        | None -> state
    | TransportReservationExpired e ->
        let id = TransportReservationId.value e.Id
        match Map.tryFind id state with
        | Some existing ->
            match Reservation.evolve (TransportReservationExpired e) (Some existing) with
            | Some updated -> Map.add id updated state
            | None         -> state
        | None -> state

let createProjectionAgent () =
    ProjectionAgent<Map<string, TransportReservation>, TransportReservationEvent>(
        evolveProjection,
        Map.empty,
        "TransportReservationReadModel"
    )
