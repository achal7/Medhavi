/// DE-D-010 — Determine Planning Priority Decision
module Medhavi.Demand.PrioritizeDemand.PlanningPriorityAssignment.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Demand
open Rules
open Policies
open Model

/// Decision evaluation payload for planning priority
type PriorityEvaluation =
    { Priority: PriorityLevel
      Score: decimal
      Breakdown: DimensionScoreBreakdown
      Rationale: string
      BusinessValidity: string }

/// Evaluates DE-D-010: Determine Planning Priority
let evaluatePlanningPriority
    (rules: Rule<PrioritizationRuleInput> list)
    (input: PrioritizationRuleInput)
    : Result<DecisionOutcome<PriorityEvaluation>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input

        let hasCompliance =
            evaluations
            |> List.exists(fun e -> e.RuleId = ArsIdentifiers.Rules.prioritizationDeterminedByPolicy.Id && e.Passed)

        let hasEvidence =
            evaluations
            |> List.exists(fun e -> e.RuleId = ArsIdentifiers.Rules.minimumEvidenceForPrioritization.Id && e.Passed)

        if not hasCompliance then
            let failureMsg =
                evaluations
                |> List.filter(fun e -> not e.Passed)
                |> List.map(fun e -> e.Evidence |> String.concat "; ")
                |> String.concat " | "

            return! Error(DomainError.rule(failureMsg, rule = ArsIdentifiers.Rules.prioritizationDeterminedByPolicy.Id))
        elif not hasEvidence then
            let level, score, breakdown, rationale, validity =
                Algorithms.computePriority None None None None input.Policy

            return
                { Outcome =
                    { Priority = level
                      Score = score
                      Breakdown = breakdown
                      Rationale = rationale
                      BusinessValidity = validity }
                  Evaluations = evaluations }
        else
            let level, score, breakdown, rationale, validity =
                Algorithms.computePriority
                    input.RevenueContribution
                    input.StrategicImportance
                    input.RiskExposure
                    input.ContractualObligation
                    input.Policy

            return
                { Outcome =
                    { Priority = level
                      Score = score
                      Breakdown = breakdown
                      Rationale = rationale
                      BusinessValidity = validity }
                  Evaluations = evaluations }
    }

/// Evaluates manual planner priority override against governance rules
let evaluateOverride
    (rules: Rule<OverrideRuleInput> list)
    (input: OverrideRuleInput)
    (newPriority: PriorityLevel)
    : Result<DecisionOutcome<PriorityLevel>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input

        let allPassed = evaluations |> List.forall(fun e -> e.Passed)

        if not allPassed then
            let failureMsg =
                evaluations
                |> List.filter(fun e -> not e.Passed)
                |> List.map(fun e -> e.Evidence |> String.concat "; ")
                |> String.concat " | "

            return! Error(DomainError.rule(failureMsg, rule = "BR-D-310-OVR"))
        else
            return
                { Outcome = newPriority
                  Evaluations = evaluations }
    }
