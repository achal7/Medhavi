module Medhavi.Core.DemandManagement.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Core.ArsIdentifiers
open Rules

type RecordingOutcome =
    | RecordDemand
    | RejectRecording of reasons: string list

type SatisfactionOutcome =
    | SatisfyDemand
    | RejectSatisfaction of reasons: string list

type CancellationOutcome =
    | CancelDemand
    | RejectCancellation of reasons: string list

/// DE-C-006: Evaluate Demand Recording
let evaluateRecording
    (rules: Rule<RecordInput> list)
    (input: RecordInput)
    : Result<DecisionOutcome<RecordingOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter(fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = RecordDemand
                  Evaluations = evaluations }
        else
            let reasons = failed |> List.map(fun e -> sprintf "[%s] %s" e.RuleId (e.Evidence |> String.concat ", "))

            return
                { Outcome = RejectRecording reasons
                  Evaluations = evaluations }
    }

/// DE-C-007: Evaluate Demand Satisfaction
let evaluateSatisfaction
    (rules: Rule<SatisfyInput> list)
    (input: SatisfyInput)
    : Result<DecisionOutcome<SatisfactionOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter(fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = SatisfyDemand
                  Evaluations = evaluations }
        else
            let reasons = failed |> List.map(fun e -> sprintf "[%s] %s" e.RuleId (e.Evidence |> String.concat ", "))

            return
                { Outcome = RejectSatisfaction reasons
                  Evaluations = evaluations }
    }

/// DE-C-008: Evaluate Demand Cancellation
let evaluateCancellation
    (rules: Rule<CancelInput> list)
    (input: CancelInput)
    : Result<DecisionOutcome<CancellationOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter(fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = CancelDemand
                  Evaluations = evaluations }
        else
            let reasons = failed |> List.map(fun e -> sprintf "[%s] %s" e.RuleId (e.Evidence |> String.concat ", "))

            return
                { Outcome = RejectCancellation reasons
                  Evaluations = evaluations }
    }
