/// CA-P-007 Assess Promise Risk and Resilience — Decisions
/// Traces to: DE-P-009 Evaluate Promise Risk Severity
module Medhavi.Promise.AssessPromiseRisk.PromiseRiskAssessment.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Promise
open Rules

type AssessRiskOutcome =
    | RiskAssessmentAuthorized
    | AssessmentRejected of Reason: string

let evaluateAssessRisk
    (rules: Rule<AssessRiskInput> list)
    (input: AssessRiskInput)
    : Result<DecisionOutcome<AssessRiskOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = RiskAssessmentAuthorized
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = AssessmentRejected reason
                  Evaluations = evaluations }
    }

type EscalateRiskOutcome =
    | EscalationAuthorized of TargetExceptionId: PromiseExceptionId
    | EscalationRejected of Reason: string

let evaluateEscalateRisk
    (rules: Rule<EscalateRiskInput> list)
    (input: EscalateRiskInput)
    : Result<DecisionOutcome<EscalateRiskOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = EscalationAuthorized input.Cmd.TargetExceptionId
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = EscalationRejected reason
                  Evaluations = evaluations }
    }

