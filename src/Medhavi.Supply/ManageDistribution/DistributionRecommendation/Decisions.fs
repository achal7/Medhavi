/// SE-S-080 — Distribution Recommendation Decisions
/// Traces to: Supply Intelligence Specification §4.3.1 (DE-S-071)
module Medhavi.Supply.ManageDistribution.DistributionRecommendation.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Rules
open Algorithms
open Medhavi.Supply

[<RequireQualifiedAccess>]
type SourcingDecisionOutcome =
    | PreferredSourceSourcing of ConstructionResult
    | AlternateSourceSourcing of ConstructionResult
    | CrossDockSourcing of ConstructionResult
    | DeferMovement of reason: string * ConstructionResult

/// DE-S-071 — Evaluate whether the distribution sourcing determination satisfies all governed rules
let evaluateSourcing
    (rules: Rule<SourcingInput> list)
    (input: SourcingInput)
    : Result<DecisionOutcome<SourcingDecisionOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter(fun e -> not e.Passed)

        if not failed.IsEmpty then
            let failures =
                failed
                |> List.collect(fun e -> e.Evidence)
                |> fun evs -> if evs.IsEmpty then failed |> List.map(fun e -> e.RuleId) else evs
                |> String.concat "; "

            return!
                Error(
                    ruleViolationWithDetail ArsIdentifiers.Rules.distributionRecommendationCreationEligibility failures
                )
        else
            let outcome =
                match input.Result.Strategy with
                | DistributionStrategy.PreferredSource -> SourcingDecisionOutcome.PreferredSourceSourcing input.Result
                | DistributionStrategy.AlternateSource -> SourcingDecisionOutcome.AlternateSourceSourcing input.Result
                | DistributionStrategy.CrossDock -> SourcingDecisionOutcome.CrossDockSourcing input.Result
                | DistributionStrategy.Deferral ->
                    let deferReason =
                        input.Result.AllocationOutcome
                        |> Option.bind(fun a -> a.DeferralReason)
                        |> Option.defaultValue "Movement deferred per algorithm evaluation"

                    SourcingDecisionOutcome.DeferMovement(deferReason, input.Result)

            return
                {
                    Outcome = outcome
                    Evaluations = evaluations
                }
    }
