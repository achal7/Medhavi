/// DE-D-009 — Determine Demand Behavior Classification Decision
module Medhavi.Demand.ClassifyDemand.DemandBehaviorAssignment.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Demand
open Rules
open Policies
open Model

/// Decision evaluation payload for behavior classification
type ClassificationEvaluation =
    { Classification: BehaviorClass
      Features: StatisticalFeatures option
      Confidence: string
      Rationale: string }

/// Evaluates DE-D-009: Determine Demand Behavior Classification
let evaluateBehaviorClassification
    (rules: Rule<ClassificationRuleInput> list)
    (input: ClassificationRuleInput)
    : Result<DecisionOutcome<ClassificationEvaluation>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input

        let hasCompliance =
            evaluations
            |> List.exists(fun e ->
                e.RuleId = ArsIdentifiers.Rules.behaviorClassificationDeterminedByPolicy.Id && e.Passed)

        let hasSufficientEvidence =
            evaluations
            |> List.exists(fun e ->
                e.RuleId = ArsIdentifiers.Rules.minimumEvidenceForBehaviorClassification.Id && e.Passed)

        if not hasCompliance then
            let failureMsg =
                evaluations
                |> List.filter(fun e -> not e.Passed)
                |> List.map(fun e -> e.Evidence |> String.concat "; ")
                |> String.concat " | "

            return!
                Error(
                    DomainError.rule(
                        failureMsg,
                        rule = ArsIdentifiers.Rules.behaviorClassificationDeterminedByPolicy.Id
                    )
                )
        elif not hasSufficientEvidence then
            let features =
                Algorithms.computeStatisticalFeatures input.DemandQuantities input.Policy.SeasonalAutocorrelationLag

            let rationale, conf = Algorithms.classificationRationale Unclassified features

            return
                { Outcome =
                    { Classification = Unclassified
                      Features = Some features
                      Confidence = conf
                      Rationale = rationale }
                  Evaluations = evaluations }
        else
            let features =
                Algorithms.computeStatisticalFeatures input.DemandQuantities input.Policy.SeasonalAutocorrelationLag

            let behaviorClass = Algorithms.determineClass features input.Policy
            let rationale, conf = Algorithms.classificationRationale behaviorClass features

            return
                { Outcome =
                    { Classification = behaviorClass
                      Features = Some features
                      Confidence = conf
                      Rationale = rationale }
                  Evaluations = evaluations }
    }

/// Evaluates manual planner classification override against governance rules
let evaluateOverride
    (rules: Rule<OverrideRuleInput> list)
    (input: OverrideRuleInput)
    (newClassification: BehaviorClass)
    : Result<DecisionOutcome<BehaviorClass>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input

        let allPassed = evaluations |> List.forall(fun e -> e.Passed)

        if not allPassed then
            let failureMsg =
                evaluations
                |> List.filter(fun e -> not e.Passed)
                |> List.map(fun e -> e.Evidence |> String.concat "; ")
                |> String.concat " | "

            return! Error(DomainError.rule(failureMsg, rule = "BR-D-308-OVR"))
        else
            return
                { Outcome = newClassification
                  Evaluations = evaluations }
    }
