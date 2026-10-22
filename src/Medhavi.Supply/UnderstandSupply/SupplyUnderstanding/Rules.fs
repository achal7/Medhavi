/// SE-S-001 — Supply Understanding Rules
/// Traces to: BR-S-014 (Material Change Required), BR-S-015 (Data Completeness), BR-S-015a, BR-S-015b
module Medhavi.Supply.UnderstandSupply.SupplyUnderstanding.Rules

open Medhavi.Foundation.Contracts
open Medhavi.Supply.ArsIdentifiers
open Model
open Algorithms

/// DE-S-002 — input context for publication eligibility rules
type PublicationInput =
    { Assessment: MaterialityAssessment
      PeriodicRefreshDue: bool
      Interpretation: Interpretation
      CompletenessThreshold: decimal }

/// BR-S-014 — Publication requires material change in at least one interpretation dimension,
/// or a Periodic Refresh due per PO-S-012.
let materialChangeRequired: Rule<PublicationInput> =
    Rule.create
        Rules.materialChangeRequiredForPublication.Id
        Rules.materialChangeRequiredForPublication.Explanation
        (fun input -> input.Assessment.HasMaterialChange || input.PeriodicRefreshDue)
        (fun input ->
            $"HasMaterialChange: %b{input.Assessment.HasMaterialChange}; PeriodicRefreshDue: %b{input.PeriodicRefreshDue}")

/// BR-S-015 — Interpretation completeness must meet the threshold defined in PO-S-011.
let interpretationComplete: Rule<PublicationInput> =
    Rule.create
        Rules.dataCompletenessThreshold.Id
        Rules.dataCompletenessThreshold.Explanation
        (fun input -> Interpretation.completenessRatio input.Interpretation >= input.CompletenessThreshold)
        (fun input ->
            $"Completeness: %.2f{Interpretation.completenessRatio input.Interpretation}; Threshold: %.2f{input.CompletenessThreshold}")

/// DE-S-002 — publication eligibility rule set
let publicationRules: Rule<PublicationInput> list = [ materialChangeRequired; interpretationComplete ]
