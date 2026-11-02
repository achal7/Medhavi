/// CA-P-006 Establish Commitments Decisions
/// Traces to: DE-P-008 Authorize Binding Commitment
module Medhavi.Promise.EstablishCommitments.OrderCommitment.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Promise.ArsIdentifiers
open Rules

type EstablishCommitmentOutcome =
    | CommitmentAuthorized
    | CommitmentRejected of reasons: string list

/// DE-P-008: Authorize Binding Commitment
let evaluateEstablishCommitment
    (rules: Rule<EstablishCommitmentInput> list)
    (input: EstablishCommitmentInput)
    : Result<DecisionOutcome<EstablishCommitmentOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = CommitmentAuthorized
                  Evaluations = evaluations }
        else
            let reasons = failed |> List.map (fun e -> sprintf "[%s] %s" e.RuleId (e.Evidence |> String.concat ", "))
            return
                { Outcome = CommitmentRejected reasons
                  Evaluations = evaluations }
    }

type ReviseCommitmentOutcome =
    | RevisionAuthorized
    | RevisionRejected of reason: string

let evaluateReviseCommitment
    (rules: Rule<ReviseCommitmentInput> list)
    (input: ReviseCommitmentInput)
    : Result<DecisionOutcome<ReviseCommitmentOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = RevisionAuthorized
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = RevisionRejected $"Schedule revision unauthorized: {reason}"
                  Evaluations = evaluations }
    }

type CancelCommitmentOutcome =
    | CancellationAuthorized
    | CancellationRejected of reason: string

let evaluateCancelCommitment
    (rules: Rule<CancelCommitmentInput> list)
    (input: CancelCommitmentInput)
    : Result<DecisionOutcome<CancelCommitmentOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = CancellationAuthorized
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = CancellationRejected $"De-commitment unauthorized: {reason}"
                  Evaluations = evaluations }
    }

