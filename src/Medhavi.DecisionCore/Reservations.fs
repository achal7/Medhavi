namespace Medhavi.DecisionCore

open System
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Failure

type ReservationScope =
    | Atp
    | Ctp
    | Allocation
    | Planned

type ReservationStatus =
    | Tentative
    | Confirmed
    | Released
    | Expired

type Reservation = {
    ReservationId: Guid
    Scope: ReservationScope
    Status: ReservationStatus
    SkuId: string
    Quantity: PositiveDecimal
    Source: string
    CreatedAt: DateTimeOffset
    ExpiresAt: DateTimeOffset option
}

module Reservation =

    let createTentative (scope: ReservationScope) (skuId: string) (quantity: PositiveDecimal) (source: string) (ttl: TimeSpan option) =
        { ReservationId = Guid.NewGuid()
          Scope = scope
          Status = Tentative
          SkuId = skuId
          Quantity = quantity
          Source = source
          CreatedAt = DateTimeOffset.UtcNow
          ExpiresAt = ttl |> Option.map (fun t -> DateTimeOffset.UtcNow + t) }

    let private transition allowed newStatus reservation =
        if allowed reservation.Status then
            Ok { reservation with Status = newStatus }
        else
            Error (DomainError.invariant $"Cannot transition from {reservation.Status} to {newStatus}")

    let confirm = transition (fun s -> s = Tentative) Confirmed
    let release = transition (fun s -> s = Tentative) Released
    let expire = transition (fun s -> s = Tentative) Expired

    let reduce (reservation: Reservation) (newQty: PositiveDecimal) =
        if newQty <= reservation.Quantity then
            Ok { reservation with Quantity = newQty }
        else
            Error (DomainError.validation "Reduced quantity must be less than or equal to original quantity")

    let validateLifecycle fromStatus toStatus =
        match fromStatus, toStatus with
        | Tentative, Confirmed -> true
        | Tentative, Released  -> true
        | Tentative, Expired   -> true
        | Confirmed, _         -> false
        | Released,  _         -> false
        | Expired,   _         -> false
        | Tentative, Tentative -> false
