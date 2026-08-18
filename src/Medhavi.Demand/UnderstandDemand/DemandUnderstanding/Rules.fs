/// SE-D-002 — Demand Understanding Rules
/// Traces to: BR-D-204 (Material Change Required), BR-D-205 (Interpretation Completeness)
module Medhavi.Demand.UnderstandDemand.DemandUnderstanding.Rules

open Medhavi.Foundation.Contracts
open Medhavi.Demand.ArsIdentifiers
open Model
open Algorithms

/// DE-D-002 — input context for the publication eligibility rules.
type PublicationInput =
    { Assessment: MaterialityAssessment
      PeriodicRefreshDue: bool
      Interpretation: Interpretation
      CompletenessThreshold: decimal }

/// BR-D-204 — Publication requires material change in at least one interpretation dimension,
/// or a Periodic Refresh due per PO-D-012.
let materialChangeRequired: Rule<PublicationInput> =
    Rule.create
        Rules.materialChangeRequiredForPublication.Id
        Rules.materialChangeRequiredForPublication.Explanation
        (fun input -> input.Assessment.HasMaterialChange || input.PeriodicRefreshDue)
        (fun input ->
            sprintf
                "HasMaterialChange: %b; PeriodicRefreshDue: %b"
                input.Assessment.HasMaterialChange
                input.PeriodicRefreshDue)

/// BR-D-205 — Interpretation completeness must meet the threshold defined in PO-D-011.
let interpretationComplete: Rule<PublicationInput> =
    Rule.create
        Rules.interpretationCompletenessThreshold.Id
        Rules.interpretationCompletenessThreshold.Explanation
        (fun input -> Interpretation.completenessRatio input.Interpretation >= input.CompletenessThreshold)
        (fun input ->
            sprintf
                "Completeness: %.2f; Threshold: %.2f"
                (Interpretation.completenessRatio input.Interpretation)
                input.CompletenessThreshold)

/// DE-D-002 — the publication eligibility rule set.
let publicationRules: Rule<PublicationInput> list = [ materialChangeRequired; interpretationComplete ]
