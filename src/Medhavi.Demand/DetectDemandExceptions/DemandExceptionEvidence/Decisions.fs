/// DE-D-012 — Evaluate Demand Exception Evidence Decision
module Medhavi.Demand.DetectDemandExceptions.DemandExceptionEvidence.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Demand
open Rules
open Model

/// Evaluates DE-D-012: Evaluate Demand Exception Evidence against governance rules
let evaluateEvidence
    (rules: Rule<ExceptionRuleInput> list)
    (input: ExceptionRuleInput)
    : Result<DecisionOutcome<LifecycleDetermination>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input

        let allPassed = evaluations |> List.forall(fun e -> e.Passed)

        if not allPassed then
            let failureMsg =
                evaluations
                |> List.filter(fun e -> not e.Passed)
                |> List.map(fun e -> e.Evidence |> String.concat "; ")
                |> String.concat " | "

            return!
                Error(DomainError.rule(failureMsg, rule = ArsIdentifiers.Decisions.evaluateDemandExceptionEvidence.Id))
        else
            let determination =
                Algorithms.assessLifecycle
                    input.IsCurrentlyActive
                    input.ExceptionType
                    input.MetricValue
                    input.HistoricalValues
                    input.Policy

            return
                { Outcome = determination
                  Evaluations = evaluations }
    }
