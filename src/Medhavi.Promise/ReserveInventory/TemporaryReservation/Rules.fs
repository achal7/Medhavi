/// CA-P-003 Reserve Inventory and Capacity Temporarily Rules
/// Traces to: BR-P-006, BR-P-026, BR-P-027, BR-P-054
module Medhavi.Promise.ReserveInventory.TemporaryReservation.Rules

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Policies

type CreateHoldInput =
    { Cmd: CreateTemporaryHoldCmd
      CustomerExists: bool
      ItemExists: bool
      LocationExists: bool
      CreditHoldActive: bool
      Policy: TemporaryReservationPolicy }

type ConfirmHoldInput =
    { Cmd: ConfirmHoldToCommitmentCmd
      CurrentReservation: TemporaryReservation option }

type ReleaseHoldInput =
    { Cmd: ReleaseTemporaryHoldCmd
      CurrentReservation: TemporaryReservation option }

type ExpireHoldInput =
    { Cmd: ExpireTemporaryHoldCmd
      CurrentReservation: TemporaryReservation option }

/// BR-P-006: Temporary reservation prerequisites
let reservationPrerequisitesRule: Rule<CreateHoldInput> =
    Rule.create
        Rules.temporaryReservationLeaseIdentity.Id
        Rules.temporaryReservationLeaseIdentity.Explanation
        (fun input -> input.CustomerExists && input.ItemExists && input.LocationExists && not input.CreditHoldActive)
        (fun input -> $"CustExists: %b{input.CustomerExists}, ItemExists: %b{input.ItemExists}, LocExists: %b{input.LocationExists}, CreditHold: %b{input.CreditHoldActive}")

/// BR-P-047: Lease duration bound
let leaseDurationBoundRule: Rule<CreateHoldInput> =
    Rule.create
        Rules.reservationTimeoutLimit.Id
        Rules.reservationTimeoutLimit.Explanation
        (fun input ->
            input.Cmd.LeaseDurationMinutes > 0
            && input.Cmd.LeaseDurationMinutes <= input.Policy.MaxLeaseDurationMinutes)
        (fun input -> $"DurationMinutes: %d{input.Cmd.LeaseDurationMinutes}, Max: %d{input.Policy.MaxLeaseDurationMinutes}")

let createHoldRules: Rule<CreateHoldInput> list =
    [ reservationPrerequisitesRule; leaseDurationBoundRule ]

/// BR-P-046: Active state requirement for confirmation
let confirmStateRule: Rule<ConfirmHoldInput> =
    Rule.create
        Rules.temporaryReservationConservation.Id
        Rules.temporaryReservationConservation.Explanation
        (fun input ->
            match input.CurrentReservation with
            | Some r -> r.LifecycleState = Active
            | None -> false)
        (fun input ->
            match input.CurrentReservation with
            | Some r -> $"LifecycleState: %A{r.LifecycleState}"
            | None -> "ReservationNotFound")

let confirmHoldRules: Rule<ConfirmHoldInput> list =
    [ confirmStateRule ]

/// BR-P-006: Active state requirement for release
let releaseStateRule: Rule<ReleaseHoldInput> =
    Rule.create
        Rules.temporaryReservationLeaseIdentity.Id
        Rules.temporaryReservationLeaseIdentity.Explanation
        (fun input ->
            match input.CurrentReservation with
            | Some r -> r.LifecycleState = Active
            | None -> false)
        (fun input ->
            match input.CurrentReservation with
            | Some r -> $"LifecycleState: %A{r.LifecycleState}"
            | None -> "ReservationNotFound")

let releaseHoldRules: Rule<ReleaseHoldInput> list =
    [ releaseStateRule ]

/// BR-P-047: Heartbeat expiration requirement
let heartbeatExpirationRule: Rule<ExpireHoldInput> =
    Rule.create
        Rules.reservationTimeoutLimit.Id
        Rules.reservationTimeoutLimit.Explanation
        (fun input ->
            match input.CurrentReservation with
            | Some r ->
                r.LifecycleState = Active
                && Timestamp.value input.Cmd.HeartbeatTime >= Timestamp.value r.LeaseExpirationTime
            | None -> false)
        (fun input ->
            match input.CurrentReservation with
            | Some r -> sprintf "Heartbeat: %s, Expiration: %s" ((Timestamp.value input.Cmd.HeartbeatTime).ToString("o")) ((Timestamp.value r.LeaseExpirationTime).ToString("o"))
            | None -> "ReservationNotFound")

let expireHoldRules: Rule<ExpireHoldInput> list =
    [ heartbeatExpirationRule ]
