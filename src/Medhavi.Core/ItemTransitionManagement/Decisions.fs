/// CA-C-021 Decisions
module Medhavi.Core.ItemTransitionManagement.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Rules
open Policies
open Model

/// DE-C-005 outcomes
type RecognitionOutcome =
    | RecognizeTransition
    | RejectRecognition of reasons: string list

/// Suspend outcomes
type SuspensionOutcome =
    | SuspendTransition
    | RejectSuspension of reasons: string list

/// Reinstate outcomes
type ReinstatementOutcome =
    | ReinstateTransition
    | RejectReinstatement of reasons: string list

/// Retire outcomes
type RetirementOutcome =
    | RetireTransition
    | RejectRetirement of reasons: string list

/// DE-C-005: Validate Item Transition Recognition.
let evaluateRecognition
    (rules: Rule<RecognizeInput> list)
    (input: RecognizeInput)
    : Result<DecisionOutcome<RecognitionOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter(fun e -> not e.Passed)

        if not failed.IsEmpty then
            let reasons = failed |> List.map(fun e -> sprintf "[%s] %s" e.RuleId (e.Evidence |> String.concat ", "))

            return
                { Outcome = RejectRecognition reasons
                  Evaluations = evaluations }
        else
            return
                { Outcome = RecognizeTransition
                  Evaluations = evaluations }
    }

/// Evaluate suspension eligibility.
let evaluateSuspension
    (rules: Rule<SuspendInput> list)
    (input: SuspendInput)
    : Result<DecisionOutcome<SuspensionOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter(fun e -> not e.Passed)

        if not failed.IsEmpty then
            let reasons = failed |> List.map(fun e -> sprintf "[%s] %s" e.RuleId (e.Evidence |> String.concat ", "))

            return
                { Outcome = RejectSuspension reasons
                  Evaluations = evaluations }
        else
            return
                { Outcome = SuspendTransition
                  Evaluations = evaluations }
    }

/// Evaluate reinstatement eligibility.
let evaluateReinstatement
    (rules: Rule<ReinstateInput> list)
    (input: ReinstateInput)
    : Result<DecisionOutcome<ReinstatementOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter(fun e -> not e.Passed)

        if not failed.IsEmpty then
            let reasons = failed |> List.map(fun e -> sprintf "[%s] %s" e.RuleId (e.Evidence |> String.concat ", "))

            return
                { Outcome = RejectReinstatement reasons
                  Evaluations = evaluations }
        else
            return
                { Outcome = ReinstateTransition
                  Evaluations = evaluations }
    }

/// Evaluate retirement eligibility.
let evaluateRetirement
    (rules: Rule<RetireInput> list)
    (input: RetireInput)
    : Result<DecisionOutcome<RetirementOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter(fun e -> not e.Passed)

        if not failed.IsEmpty then
            let reasons = failed |> List.map(fun e -> sprintf "[%s] %s" e.RuleId (e.Evidence |> String.concat ", "))

            return
                { Outcome = RejectRetirement reasons
                  Evaluations = evaluations }
        else
            return
                { Outcome = RetireTransition
                  Evaluations = evaluations }
    }
