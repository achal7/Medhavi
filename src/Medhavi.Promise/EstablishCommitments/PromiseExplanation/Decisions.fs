/// CA-P-006 Establish Commitments — Promise Explanation Decisions
module Medhavi.Promise.EstablishCommitments.PromiseExplanation.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Rules

type GenerateExplanationOutcome =
    | ExplanationAuthorized
    | ExplanationRejected of reason: string

let evaluateGenerateExplanation
    (rules: Rule<GenerateExplanationInput> list)
    (input: GenerateExplanationInput)
    : Result<DecisionOutcome<GenerateExplanationOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = ExplanationAuthorized
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = ExplanationRejected $"Explanation generation rejected: {reason}"
                  Evaluations = evaluations }
    }

