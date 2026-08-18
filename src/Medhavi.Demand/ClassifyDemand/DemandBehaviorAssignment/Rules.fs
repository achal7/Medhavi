/// BR-D-307 & BR-D-308 — Demand Behavior Classification Business Rules
module Medhavi.Demand.ClassifyDemand.DemandBehaviorAssignment.Rules

open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers
open Policies
open Model

/// Typed input context for behavioral classification rules
type ClassificationRuleInput =
    { Item: ItemId
      Location: LocationId
      Dimension: BehaviorDimension
      DemandQuantities: decimal list
      Policy: ClassificationPolicy }

/// Typed input context for manual override rules
type OverrideRuleInput =
    { PlannerId: string
      Justification: string
      Policy: ClassificationOverridePolicy }

/// BR-D-307: Behavior classification must be determined by the rules in Classification Policy
let policyComplianceRule: Rule<ClassificationRuleInput> =
    Rule.create
        ArsIdentifiers.Rules.behaviorClassificationDeterminedByPolicy.Id
        ArsIdentifiers.Rules.behaviorClassificationDeterminedByPolicy.Explanation
        (fun input ->
            not (System.String.IsNullOrWhiteSpace input.Policy.PolicyId)
            && input.Policy.MinimumHistoryDataPoints > 0)
        (fun input ->
            if not (System.String.IsNullOrWhiteSpace input.Policy.PolicyId) then
                $"Classification governed by active policy '{input.Policy.PolicyId}' (v{input.Policy.Version})"
            else
                "No active Classification Policy configured")

/// BR-D-308: Entity shall be classified as Unclassified if minimum evidence is not met
let minimumEvidenceRule: Rule<ClassificationRuleInput> =
    Rule.create
        ArsIdentifiers.Rules.minimumEvidenceForBehaviorClassification.Id
        ArsIdentifiers.Rules.minimumEvidenceForBehaviorClassification.Explanation
        (fun input -> input.DemandQuantities.Length >= input.Policy.MinimumHistoryDataPoints)
        (fun input ->
            if input.DemandQuantities.Length >= input.Policy.MinimumHistoryDataPoints then
                $"Sufficient historical demand points provided (%d{input.DemandQuantities.Length} >= %d{input.Policy.MinimumHistoryDataPoints})"
            else
                $"Insufficient historical demand points (%d{input.DemandQuantities.Length} < %d{input.Policy.MinimumHistoryDataPoints}); assigned Unclassified")

/// Classification rules pipeline
let classificationRules: Rule<ClassificationRuleInput> list =
    [ policyComplianceRule; minimumEvidenceRule ]

/// Mandatory justification rule for manual planner overrides (PO-D-038)
let overrideJustificationRule: Rule<OverrideRuleInput> =
    Rule.create
        "BR-D-308-OVR"
        "Manual classification overrides require non-empty business justification and registered planner"
        (fun input ->
            if input.Policy.RequireJustification then
                not (System.String.IsNullOrWhiteSpace input.Justification)
                && not (System.String.IsNullOrWhiteSpace input.PlannerId)
            else
                true)
        (fun input ->
            if not (System.String.IsNullOrWhiteSpace input.Justification) then
                $"Override submitted by planner '{input.PlannerId}' with justification: '{input.Justification}'"
            else
                "Manual override rejected: justification is missing or empty")

let overrideRules: Rule<OverrideRuleInput> list =
    [ overrideJustificationRule ]
