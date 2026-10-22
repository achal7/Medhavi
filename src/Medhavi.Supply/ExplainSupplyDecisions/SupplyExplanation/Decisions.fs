/// Deterministic rule evaluation and audit trace recording for Supply Explanation
module Medhavi.Supply.ExplainSupplyDecisions.SupplyExplanation.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Supply
open Rules

/// Evaluates governance rules and builds a verified evaluation outcome
let evaluateExplanationRules
    (rules: Rule<ExplanationRuleInput> list)
    (input: ExplanationRuleInput)
    : Result<DecisionOutcome<bool>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input

        let allPassed = evaluations |> List.forall (fun e -> e.Passed)

        if not allPassed then
            let failedRules =
                evaluations
                |> List.filter (fun e -> not e.Passed)

            let failureMsg =
                failedRules
                |> List.map (fun e -> e.Evidence |> String.concat "; ")
                |> String.concat " | "

            let firstFailedRuleId =
                failedRules
                |> List.tryHead
                |> Option.map (fun e -> e.RuleId)
                |> Option.defaultValue ArsIdentifiers.Rules.supplyExplanationImmutability.Id

            return! Error (DomainError.rule (failureMsg, rule = firstFailedRuleId))
        else
            return
                { Outcome = true
                  Evaluations = evaluations }
    }
