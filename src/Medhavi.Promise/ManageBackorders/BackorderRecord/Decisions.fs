/// CA-P-008 Manage Backorders — Decisions
/// Traces to: DE-P-010 Authorize Backlog Re-Promise
module Medhavi.Promise.ManageBackorders.BackorderRecord.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Rules

type EnqueueOutcome =
    | EnqueueAuthorized
    | EnqueueRejected of Reason: string

let evaluateEnqueue
    (rules: Rule<EnqueueBackorderInput> list)
    (input: EnqueueBackorderInput)
    : Result<DecisionOutcome<EnqueueOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = EnqueueAuthorized
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = EnqueueRejected reason
                  Evaluations = evaluations }
    }

type ResolveOutcome =
    | ResolveAuthorized
    | ResolveRejected of Reason: string

let evaluateResolve
    (rules: Rule<ResolveBackorderInput> list)
    (input: ResolveBackorderInput)
    : Result<DecisionOutcome<ResolveOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = ResolveAuthorized
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = ResolveRejected reason
                  Evaluations = evaluations }
    }

