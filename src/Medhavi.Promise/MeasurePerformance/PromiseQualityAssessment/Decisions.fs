/// CA-P-010 Measure Promise Performance & Service Quality — Decisions
/// Architectural Lineage: ARS §3, §14; Table A.7 DE-P-012
module Medhavi.Promise.MeasurePerformance.PromiseQualityAssessment.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Rules

type AuditOutcome =
    | AuditAuthorized
    | AuditRejected of Reason: string

let evaluateAudit
    (rules: Rule<AuditQualityInput> list)
    (input: AuditQualityInput)
    : Result<DecisionOutcome<AuditOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter(fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                {
                    Outcome = AuditAuthorized
                    Evaluations = evaluations
                }
        else
            let reason = failed |> List.map(fun e -> e.RuleId) |> String.concat "; "

            return
                {
                    Outcome = AuditRejected reason
                    Evaluations = evaluations
                }
    }

type DisputeOutcome =
    | DisputeAuthorized
    | DisputeRejected of Reason: string

let evaluateDispute
    (rules: Rule<DisputeInput> list)
    (input: DisputeInput)
    : Result<DecisionOutcome<DisputeOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter(fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                {
                    Outcome = DisputeAuthorized
                    Evaluations = evaluations
                }
        else
            let reason = failed |> List.map(fun e -> e.RuleId) |> String.concat "; "

            return
                {
                    Outcome = DisputeRejected reason
                    Evaluations = evaluations
                }
    }
