/// CA-P-005 Evaluate Capable-to-Promise Decisions
/// Traces to: DE-P-007 Authorize CTP Production Slot
module Medhavi.Promise.EvaluateCTP.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Promise.ArsIdentifiers
open Model
open Rules

type CTPDecisionOutcome =
    | CTPSlotAuthorized of assessment: CTPFeasibilityAssessment
    | CTPRejected of reason: string

/// DE-P-007: Authorize CTP Production Slot
let evaluateCTPDecision
    (rules: Rule<EvaluateCTPInput> list)
    (input: EvaluateCTPInput)
    (assessment: CTPFeasibilityAssessment)
    : Result<DecisionOutcome<CTPDecisionOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = CTPSlotAuthorized assessment
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = CTPRejected $"CTP evaluation rejected: {reason}"
                  Evaluations = evaluations }
    }

