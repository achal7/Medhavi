/// DE-D-014 — Approve Demand Explanation Decision
module Medhavi.Demand.ExplainDemand.DemandExplanation.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Demand
open Rules
open Policies
open Model

/// Evaluates DE-D-014: Approve Demand Explanation against governance rules
let evaluateExplanationApproval
    (rules: Rule<ExplanationRuleInput> list)
    (input: ExplanationRuleInput)
    : Result<DecisionOutcome<bool>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input

        let allPassed = evaluations |> List.forall (fun e -> e.Passed)

        if not allPassed then
            let failureMsg =
                evaluations
                |> List.filter (fun e -> not e.Passed)
                |> List.map (fun e -> e.Evidence |> String.concat "; ")
                |> String.concat " | "

            return! Error (DomainError.rule (failureMsg, rule = ArsIdentifiers.Decisions.approveDemandExplanation.Id))
        else
            return
                { Outcome = true
                  Evaluations = evaluations }
    }
