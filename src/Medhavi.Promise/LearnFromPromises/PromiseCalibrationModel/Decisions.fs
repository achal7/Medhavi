/// CA-P-011 Learn & Calibrate Promising Models — Decisions
/// Architectural Lineage: ARS §3, §14; Table A.7 DE-P-013
module Medhavi.Promise.LearnFromPromises.PromiseCalibrationModel.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Rules

type ProposeOutcome =
    | ProposeAuthorized
    | ProposeRejected of Reason: string

let evaluatePropose
    (rules: Rule<ProposeInput> list)
    (input: ProposeInput)
    : Result<DecisionOutcome<ProposeOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = ProposeAuthorized
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = ProposeRejected reason
                  Evaluations = evaluations }
    }

type ApplyOutcome =
    | ApplyAuthorized
    | ApplyRejected of Reason: string

let evaluateApply
    (rules: Rule<ApplyInput> list)
    (input: ApplyInput)
    : Result<DecisionOutcome<ApplyOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = ApplyAuthorized
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = ApplyRejected reason
                  Evaluations = evaluations }
    }
