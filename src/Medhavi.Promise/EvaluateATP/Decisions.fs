/// CA-P-004 Evaluate Multi-Option ATP Decisions
/// Traces to: DE-P-006 Select Optimal Promise Option
module Medhavi.Promise.EvaluateATP.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Promise.ArsIdentifiers
open Model
open Rules

type SelectPromiseOptionOutcome =
    | OptionsSelected of options: CandidatePromiseOption list
    | OptionsRejected of reason: string

/// DE-P-006: Select Optimal Promise Option
let evaluateCandidateOptions
    (rules: Rule<EvaluateATPInput> list)
    (input: EvaluateATPInput)
    (options: CandidatePromiseOption list)
    : Result<DecisionOutcome<SelectPromiseOptionOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            let sortedOptions = options |> List.sortByDescending (fun o -> o.CompositeRankScore)
            return
                { Outcome = OptionsSelected sortedOptions
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = OptionsRejected $"ATP evaluation rejected: {reason}"
                  Evaluations = evaluations }
    }

