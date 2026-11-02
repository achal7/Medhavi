/// CA-P-003 Reserve Inventory and Capacity Temporarily Behaviors
/// Traces to: AB-P-014 Place Temporary Reservation, AB-P-015 Release Temporary Reservation, AB-P-016 Confirm Reservation Hold
module Medhavi.Promise.ReserveInventory.TemporaryReservation.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Rules
open Decisions
open Policies

let buildTrace policy decisionId (decisionOutcome: DecisionOutcome<'T>) state events summary =
    let policyId = policy |> Option.map (fun _ -> Policies.temporaryReservationPolicy.Id)
    let policyVersion = policy |> Option.map (fun _ -> 1)

    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        Capabilities.reserveInventoryAndCapacityTemporarily.Id
        decisionOutcome
        policyId
        policyVersion
        [ SemanticObjects.temporaryReservation.Id ]
        (Some summary)

/// AB-P-014: Place Temporary Reservation
let createHold
    (policy: TemporaryReservationPolicy)
    (customerExists: bool)
    (itemExists: bool)
    (locationExists: bool)
    (creditHoldActive: bool)
    : Decide<TemporaryReservation, CreateTemporaryHoldCmd, TemporaryReservationEvent> =
    fun (cmd: CreateTemporaryHoldCmd) (state: TemporaryReservation option) ->
        result {
            match state with
            | Some _ ->
                return! Error(DomainError.conflict ($"TemporaryReservation already exists: {TemporaryReservationId.value cmd.ReservationToken}", Capabilities.reserveInventoryAndCapacityTemporarily.Id))
            | None ->
                let input: CreateHoldInput =
                    { Cmd = cmd
                      CustomerExists = customerExists
                      ItemExists = itemExists
                      LocationExists = locationExists
                      CreditHoldActive = creditHoldActive
                      Policy = policy }

                let! decision = evaluateCreateHold createHoldRules input
                let traceId = Guid.NewGuid().ToString()

                match decision.Outcome with
                | HoldAuthorized ->
                    let expirationTime =
                        let expDto = (Timestamp.value cmd.CreationTime).AddMinutes(float cmd.LeaseDurationMinutes)
                        Timestamp.create expDto |> Result.defaultWith (fun _ -> cmd.CreationTime)

                    let reservation: TemporaryReservation =
                        { ReservationToken = cmd.ReservationToken
                          OrderLineId = cmd.OrderLineId
                          CustomerId = cmd.CustomerId
                          Item = cmd.Item
                          Location = cmd.Location
                          RequestedQuantity = cmd.Quantity
                          HeldQuantity = cmd.Quantity
                          LeaseCreationTime = cmd.CreationTime
                          LeaseExpirationTime = expirationTime
                          CapacitySlots = []
                          InventoryHolds = []
                          LifecycleState = Active
                          DecisionTraceability = Some traceId }

                    let events = [ TemporaryHoldCreated reservation ]
                    return buildTrace (Some policy) traceId decision state events $"Temporary reservation lease granted until {Timestamp.value expirationTime}"

                | HoldRejected reasons ->
                    return! Error(DomainError.validation (String.concat "; " reasons))
        }

/// AB-P-016: Confirm Reservation Hold into Binding Commitment
let confirmHold
    : Decide<TemporaryReservation, ConfirmHoldToCommitmentCmd, TemporaryReservationEvent> =
    fun (cmd: ConfirmHoldToCommitmentCmd) (state: TemporaryReservation option) ->
        result {
            match state with
            | None ->
                return! Error(DomainError.notFound (SemanticObjects.temporaryReservation.Id, TemporaryReservationId.value cmd.ReservationToken))
            | Some _ ->
                let input: ConfirmHoldInput =
                    { Cmd = cmd
                      CurrentReservation = state }

                let! decision = evaluateConfirmHold confirmHoldRules input
                let traceId = Guid.NewGuid().ToString()

                match decision.Outcome with
                | ConfirmAuthorized ->
                    let events = [ TemporaryHoldConfirmed(cmd.ReservationToken, cmd.ConfirmationTime, traceId) ]
                    return buildTrace None traceId decision state events "Temporary hold converted to permanent commitment"

                | ConfirmRejected reason ->
                    return! Error(DomainError.rule (reason, rule = Rules.temporaryReservationLeaseIdentity.Id))
        }

/// AB-P-015: Release Temporary Reservation Hold
let releaseHold
    : Decide<TemporaryReservation, ReleaseTemporaryHoldCmd, TemporaryReservationEvent> =
    fun (cmd: ReleaseTemporaryHoldCmd) (state: TemporaryReservation option) ->
        result {
            match state with
            | None ->
                return! Error(DomainError.notFound (SemanticObjects.temporaryReservation.Id, TemporaryReservationId.value cmd.ReservationToken))
            | Some _ ->
                let input: ReleaseHoldInput =
                    { Cmd = cmd
                      CurrentReservation = state }

                let! decision = evaluateReleaseHold releaseHoldRules input
                let traceId = Guid.NewGuid().ToString()

                match decision.Outcome with
                | ReleaseAuthorized ->
                    let events = [ TemporaryHoldReleased(cmd.ReservationToken, cmd.ReleaseReason, cmd.ReleaseTime, traceId) ]
                    return buildTrace None traceId decision state events $"Temporary hold released: {cmd.ReleaseReason}"

                | ReleaseRejected reason ->
                    return! Error(DomainError.rule (reason, rule = Rules.temporaryReservationLeaseIdentity.Id))
        }

/// Expiration reaper behavior
let expireHold
    : Decide<TemporaryReservation, ExpireTemporaryHoldCmd, TemporaryReservationEvent> =
    fun (cmd: ExpireTemporaryHoldCmd) (state: TemporaryReservation option) ->
        result {
            match state with
            | None ->
                return! Error(DomainError.notFound (SemanticObjects.temporaryReservation.Id, TemporaryReservationId.value cmd.ReservationToken))
            | Some _ ->
                let input: ExpireHoldInput =
                    { Cmd = cmd
                      CurrentReservation = state }

                let! decision = evaluateExpireHold expireHoldRules input
                let traceId = Guid.NewGuid().ToString()

                match decision.Outcome with
                | ExpireAuthorized ->
                    let events = [ TemporaryHoldExpired(cmd.ReservationToken, cmd.HeartbeatTime, traceId) ]
                    return buildTrace None traceId decision state events "Temporary hold expired via TTL heartbeat"

                | ExpireRejected reason ->
                    return! Error(DomainError.rule (reason, rule = Rules.temporaryReservationLeaseIdentity.Id))
        }

