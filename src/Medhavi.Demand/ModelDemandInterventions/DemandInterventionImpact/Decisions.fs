/// Model Demand Interventions Business Decisions
/// Traces to: DE-D-014 Approve Intervention Impact Publication (Specification Chapter 6)
module Medhavi.Demand.ModelDemandInterventions.DemandInterventionImpact.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Demand
open Model
open Rules

/// DE-D-014: Decision Alternatives for Intervention Impact Publication
type InterventionImpactPublicationAlternative =
    | Publish
    | DoNotPublish

/// DE-D-014 Decision Result payload
type InterventionImpactPublicationDecision =
    { SelectedAlternative: InterventionImpactPublicationAlternative
      ImpactId: DemandInterventionImpactId
      Rationale: string }

/// DE-D-014 — Approve Intervention Impact Publication
let evaluatePublicationApproval
    (rules: Rule<InterventionImpactRuleInput> list)
    (input: InterventionImpactRuleInput)
    : Result<DecisionOutcome<InterventionImpactPublicationDecision>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let passed = evaluations |> List.forall(fun e -> e.Passed)

        let alternative = if passed then Publish else DoNotPublish

        let rationale =
            if passed then
                "Demand Intervention Impact '"
                + (DemandInterventionImpactId.value input.Impact.ImpactId)
                + "' meets all publication criteria (confidence = "
                + (input.Impact.LiftConfidence * 100.0m).ToString("N1")
                + "%, active intervention reference) per PO-D-050."
            else
                let failureReasons =
                    evaluations
                    |> List.filter(fun e -> not e.Passed)
                    |> List.map(fun e -> e.Evidence |> String.concat "; ")
                    |> String.concat " | "

                "Demand Intervention Impact '"
                + (DemandInterventionImpactId.value input.Impact.ImpactId)
                + "' publication blocked: "
                + failureReasons

        if not passed then
            return!
                Error(
                    DomainError.rule(rationale, rule = ArsIdentifiers.Decisions.approveInterventionImpactPublication.Id)
                )
        else
            let decision =
                { SelectedAlternative = alternative
                  ImpactId = input.Impact.ImpactId
                  Rationale = rationale }

            return
                { Outcome = decision
                  Evaluations = evaluations }
    }
