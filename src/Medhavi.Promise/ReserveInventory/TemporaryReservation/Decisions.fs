/// CA-P-003 Reserve Inventory and Capacity Temporarily Decisions
/// Traces to: DE-P-005 Authorize Temporary Reservation Lease
module Medhavi.Promise.ReserveInventory.TemporaryReservation.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Promise.ArsIdentifiers
open Rules

type CreateHoldOutcome =
    | HoldAuthorized
    | HoldRejected of reasons: string list

/// DE-P-005: Authorize Temporary Reservation Lease
let evaluateCreateHold
    (rules: Rule<CreateHoldInput> list)
    (input: CreateHoldInput)
    : Result<DecisionOutcome<CreateHoldOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = HoldAuthorized
                  Evaluations = evaluations }
        else
            let reasons = failed |> List.map (fun e -> sprintf "[%s] %s" e.RuleId (e.Evidence |> String.concat ", "))
            return
                { Outcome = HoldRejected reasons
                  Evaluations = evaluations }
    }

type ConfirmHoldOutcome =
    | ConfirmAuthorized
    | ConfirmRejected of reason: string

let evaluateConfirmHold
    (rules: Rule<ConfirmHoldInput> list)
    (input: ConfirmHoldInput)
    : Result<DecisionOutcome<ConfirmHoldOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = ConfirmAuthorized
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = ConfirmRejected $"Confirmation unauthorized: {reason}"
                  Evaluations = evaluations }
    }

type ReleaseHoldOutcome =
    | ReleaseAuthorized
    | ReleaseRejected of reason: string

let evaluateReleaseHold
    (rules: Rule<ReleaseHoldInput> list)
    (input: ReleaseHoldInput)
    : Result<DecisionOutcome<ReleaseHoldOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = ReleaseAuthorized
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = ReleaseRejected $"Release unauthorized: {reason}"
                  Evaluations = evaluations }
    }

type ExpireHoldOutcome =
    | ExpireAuthorized
    | ExpireRejected of reason: string

let evaluateExpireHold
    (rules: Rule<ExpireHoldInput> list)
    (input: ExpireHoldInput)
    : Result<DecisionOutcome<ExpireHoldOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = ExpireAuthorized
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = ExpireRejected $"Expiration unauthorized: {reason}"
                  Evaluations = evaluations }
    }

