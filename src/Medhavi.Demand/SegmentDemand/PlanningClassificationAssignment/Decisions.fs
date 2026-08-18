/// Segment Demand Business Decisions
/// Traces to: DE-D-008 (Determine Planning Classification - Specification Chapter 6)
module Medhavi.Demand.SegmentDemand.PlanningClassificationAssignment.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Model
open Policies
open Algorithms
open Rules

/// DE-D-008: Decision Alternatives for Planning Classification
type PlanningClassificationAlternative =
    | Assigned of PlanningClassification
    | FallbackUnclassified

/// DE-D-008 Decision Result payload
type PlanningClassificationDecision =
    { SelectedAlternative: PlanningClassificationAlternative
      Computation: ClassificationComputation }

/// DE-D-008 — Determine Planning Classification
let evaluatePlanningClassification
    (rules: Rule<ClassificationRuleInput> list)
    (input: ClassificationRuleInput)
    : Result<DecisionOutcome<PlanningClassificationDecision>, DomainError> =
    result {
        // 1. Evaluate all business rules
        let! evaluations = Rule.evaluateAll rules input
        let allRulesPassed = evaluations |> List.forall(fun e -> e.Passed)

        // 2. Perform algorithmic computation
        let computation =
            if allRulesPassed then
                match input.ClassificationType with
                | ABC -> computeAbcClassification input.Policy input.VolumeOrRevenuePercentage
                | XYZ -> computeXyzClassification input.Policy input.HistoricalDemandValues
            elif input.AnalogItemId.IsSome then
                // NPI fallback via analog item reference
                { Classification = Unclassified
                  Score = 0.0m
                  Confidence = "Medium"
                  Rationale = $"Classification derived via Analog Product reference {input.AnalogItemId.Value}" }
            else
                { Classification = Unclassified
                  Score = 0.0m
                  Confidence = "Low"
                  Rationale =
                    evaluations
                    |> List.filter(fun e -> not e.Passed)
                    |> List.map(fun e -> e.Evidence |> String.concat "; ")
                    |> String.concat " | " }

        let alternative =
            match computation.Classification with
            | Unclassified -> FallbackUnclassified
            | assignedClass -> Assigned assignedClass

        let decision =
            { SelectedAlternative = alternative
              Computation = computation }

        return
            { Outcome = decision
              Evaluations = evaluations }
    }

/// Decision evaluation for manual planner override (PO-D-036)
let evaluateOverride
    (rules: Rule<OverrideRuleInput> list)
    (input: OverrideRuleInput)
    (newClassification: PlanningClassification)
    : Result<DecisionOutcome<PlanningClassification>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let allPassed = evaluations |> List.forall(fun e -> e.Passed)

        if not allPassed then
            let failureMsg =
                evaluations
                |> List.filter(fun e -> not e.Passed)
                |> List.map(fun e -> e.Evidence |> String.concat "; ")
                |> String.concat " | "

            return! Error(DomainError.rule (failureMsg, rule = "BR-D-306-OVR"))
        else
            return
                { Outcome = newClassification
                  Evaluations = evaluations }
    }
