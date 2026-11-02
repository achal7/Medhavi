/// CA-P-003 Reserve Inventory and Capacity Temporarily Parent API
module Medhavi.Promise.ReserveInventory.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts
open Medhavi.Contracts.Promise
open Medhavi.Promise.AclHelpers

type ReserveInventoryApis =
    { Reservations: ReservationApi }

let create (reservationAggApi: TemporaryReservation.Capabilities.ReservationAggregateApi) : ReserveInventoryApis =

    let createHold (req: CreateTemporaryHoldReq) : Task<Result<TemporaryReservationDto, ApiError>> =
        reservationAggApi.CreateHold req
        |> TaskResult.map TemporaryReservation.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let confirmHold (req: ConfirmHoldToCommitmentReq) : Task<Result<TemporaryReservationDto, ApiError>> =
        reservationAggApi.ConfirmHold req
        |> TaskResult.map TemporaryReservation.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let releaseHold (req: ReleaseTemporaryHoldReq) : Task<Result<TemporaryReservationDto, ApiError>> =
        reservationAggApi.ReleaseHold req
        |> TaskResult.map TemporaryReservation.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let expireHold (req: ExpireTemporaryHoldReq) : Task<Result<TemporaryReservationDto, ApiError>> =
        reservationAggApi.ExpireHold req
        |> TaskResult.map TemporaryReservation.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let api: ReservationApi =
        { CreateHold = createHold
          ConfirmHold = confirmHold
          ReleaseHold = releaseHold
          ExpireHold = expireHold }

    { Reservations = api }

