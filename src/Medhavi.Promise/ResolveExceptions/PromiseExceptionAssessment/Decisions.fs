/// CA-P-009 Resolve Exceptions — Decisions
/// Traces to: DE-P-011 Authorize Disruption Mitigation Plan
module Medhavi.Promise.ResolveExceptions.PromiseExceptionAssessment.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Rules

type RaiseOutcome =
    | RaiseAuthorized
    | RaiseRejected of Reason: string

let evaluateRaise
    (rules: Rule<RaiseExceptionInput> list)
    (input: RaiseExceptionInput)
    : Result<DecisionOutcome<RaiseOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = RaiseAuthorized
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = RaiseRejected reason
                  Evaluations = evaluations }
    }

type ResolveOutcome =
    | MitigationPlanAuthorized of SelectedPlanId: string
    | MitigationRejected of Reason: string

let evaluateResolve
    (rules: Rule<ResolveExceptionInput> list)
    (input: ResolveExceptionInput)
    : Result<DecisionOutcome<ResolveOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = MitigationPlanAuthorized input.Cmd.SelectedPlanId
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = MitigationRejected reason
                  Evaluations = evaluations }
    }

