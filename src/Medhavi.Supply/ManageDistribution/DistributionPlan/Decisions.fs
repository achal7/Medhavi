/// SE-S-081 — Distribution Plan Decisions
/// Traces to: Supply Intelligence Specification §4.3.1 (DE-S-072)
module Medhavi.Supply.ManageDistribution.DistributionPlan.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Supply
open Rules
open Algorithms

[<RequireQualifiedAccess>]
type PlanPublicationDecisionOutcome =
    | Publish of PlanCompositionResult
    | PublishWithFlags of Flags: string list * Composition: PlanCompositionResult
    | Hold of Reasons: string list

/// DE-S-072 — Evaluate whether the composed distribution plan baseline meets publication criteria
let evaluatePublication
    (rules: Rule<PlanPublicationInput> list)
    (input: PlanPublicationInput)
    : Result<DecisionOutcome<PlanPublicationDecisionOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter(fun e -> not e.Passed)

        if not failed.IsEmpty then
            let failures =
                failed
                |> List.collect(fun e -> e.Evidence)
                |> fun evs -> if evs.IsEmpty then failed |> List.map(fun e -> e.RuleId) else evs

            return
                {
                    Outcome = PlanPublicationDecisionOutcome.Hold failures
                    Evaluations = evaluations
                }
        else
            let comp = input.Composition
            let isLowConfidence = comp.ConfidenceDistribution.Overall = DistributionConfidence.Low
            let hasConditionalFeasibility = comp.FeasibilitySummary.ConditionallyFeasibleCount > 0
            let hasDocumentedInfeasibility = comp.FeasibilitySummary.InfeasibleCount > 0

            let flags =
                [
                    if isLowConfidence then
                        yield $"Confidence score ({comp.ConfidenceDistribution.Overall}) is below governed threshold"
                    if hasConditionalFeasibility then
                        yield
                            $"{comp.FeasibilitySummary.ConditionallyFeasibleCount} movement recommendations have conditional feasibility constraints"
                    if hasDocumentedInfeasibility then
                        yield
                            $"{comp.FeasibilitySummary.InfeasibleCount} movement recommendations have documented unresolved feasibility issues"
                ]

            if not flags.IsEmpty then
                return
                    {
                        Outcome = PlanPublicationDecisionOutcome.PublishWithFlags(flags, comp)
                        Evaluations = evaluations
                    }
            else
                return
                    {
                        Outcome = PlanPublicationDecisionOutcome.Publish comp
                        Evaluations = evaluations
                    }
    }
