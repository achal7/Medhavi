/// Model Demand Interventions Business Rules
/// Traces to: BR-D-414, BR-D-415 (Specification Chapter 7)
module Medhavi.Demand.ModelDemandInterventions.DemandInterventionImpact.Rules

open Medhavi.SemanticModel
open Medhavi.Foundation.Contracts
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers
open Model
open Policies

/// Input context for intervention impact assessment and publication rules
type InterventionImpactRuleInput =
    { Impact: DemandInterventionImpact
      IsInterventionActive: bool
      Policy: InterventionModelingGovernancePolicy }

/// BR-D-414 — Assessed Demand Lift must be non-negative
let liftNonNegativity: Rule<InterventionImpactRuleInput> =
    Rule.create
        Rules.interventionImpactNonNegativity.Id
        Rules.interventionImpactNonNegativity.Explanation
        (fun input -> Quantity.value input.Impact.AssessedDemandLift >= 0.0m)
        (fun input ->
            let liftVal = Quantity.value input.Impact.AssessedDemandLift
            "Assessed Demand Lift: " + liftVal.ToString("N2") + " units (non-negative required per BR-D-414)")

/// BR-D-415 — Intervention Reference must point to an active Scenario Adjustment
let interventionReferenceValidity: Rule<InterventionImpactRuleInput> =
    Rule.create
        Rules.interventionReferenceValidity.Id
        Rules.interventionReferenceValidity.Explanation
        (fun input -> input.IsInterventionActive)
        (fun input ->
            let activeStr = if input.IsInterventionActive then "Active" else "Inactive/Retired"
            "Intervention Reference '" + (ScenarioAdjustmentId.value input.Impact.InterventionReference) + "' status: " + activeStr + " (must be active per BR-D-415)")

/// Publication confidence threshold rule (DE-D-014 / PO-D-050)
let publicationConfidenceThreshold: Rule<InterventionImpactRuleInput> =
    Rule.create
        "BR-D-416"
        "Lift confidence must meet the minimum publication threshold governed by PO-D-050"
        (fun input -> input.Impact.LiftConfidence >= input.Policy.PublicationConfidenceThreshold)
        (fun input ->
            let confStr = (input.Impact.LiftConfidence * 100.0m).ToString("N1") + "%"
            let reqStr = (input.Policy.PublicationConfidenceThreshold * 100.0m).ToString("N1") + "%"
            "Lift confidence: " + confStr + ", Governed publication threshold: " + reqStr)

/// Rule set for DE-D-014 publication approval
let publicationRules: Rule<InterventionImpactRuleInput> list =
    [ liftNonNegativity
      interventionReferenceValidity
      publicationConfidenceThreshold ]
