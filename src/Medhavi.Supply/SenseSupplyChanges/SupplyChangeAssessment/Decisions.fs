/// Decisions governing SE-S-090 Supply Change Assessment
/// Traces to: AB-S-080
module Medhavi.Supply.SenseSupplyChanges.SupplyChangeAssessment.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Model
open Rules

[<RequireQualifiedAccess>]
type SupplyChangeAssessmentDecisionOutcome =
    | AssessmentCreated of EpistemicState: ChangeEpistemicState * Materiality: PlanningMateriality * Confidence: AssessmentConfidence
    | AssessmentUpdated of EpistemicState: ChangeEpistemicState * Materiality: PlanningMateriality * Confidence: AssessmentConfidence
    | AssessmentArchived of Reason: string

/// Evaluate rules and construct typed decision outcome
let evaluateAssessment
    (rules: Rule<AssessmentRuleInput> list)
    (input: AssessmentRuleInput)
    : Result<DecisionOutcome<SupplyChangeAssessmentDecisionOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let outcome =
            match input.ExistingAggregate with
            | None ->
                SupplyChangeAssessmentDecisionOutcome.AssessmentCreated(
                    input.Result.SuggestedEpistemicState,
                    input.Result.Materiality,
                    input.Result.Confidence
                )
            | Some _ ->
                SupplyChangeAssessmentDecisionOutcome.AssessmentUpdated(
                    input.Result.SuggestedEpistemicState,
                    input.Result.Materiality,
                    input.Result.Confidence
                )

        return { Outcome = outcome; Evaluations = evaluations }
    }
