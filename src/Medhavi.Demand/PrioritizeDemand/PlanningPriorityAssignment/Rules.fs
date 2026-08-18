/// BR-D-309 & BR-D-310 — Planning Priority Business Rules
module Medhavi.Demand.PrioritizeDemand.PlanningPriorityAssignment.Rules

open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers
open Policies
open Model

/// Typed input context for prioritization rules
type PrioritizationRuleInput =
    { EntityType: EntityType
      EntityId: string
      RevenueContribution: decimal option
      StrategicImportance: decimal option
      RiskExposure: decimal option
      ContractualObligation: decimal option
      Policy: PrioritizationPolicy }

/// Typed input context for manual override rules
type OverrideRuleInput =
    { PlannerId: string
      Justification: string
      Policy: PrioritizationOverridePolicy }

/// BR-D-309: Priority must be determined using the scoring methodology in Prioritization Policy
let policyComplianceRule: Rule<PrioritizationRuleInput> =
    Rule.create
        ArsIdentifiers.Rules.prioritizationDeterminedByPolicy.Id
        ArsIdentifiers.Rules.prioritizationDeterminedByPolicy.Explanation
        (fun input ->
            not (System.String.IsNullOrWhiteSpace input.Policy.PolicyId)
            && (input.Policy.RevenueWeight + input.Policy.StrategyWeight + input.Policy.RiskWeight + input.Policy.ContractualWeight) > 0.0m)
        (fun input ->
            if not (System.String.IsNullOrWhiteSpace input.Policy.PolicyId) then
                $"Prioritization governed by active policy '{input.Policy.PolicyId}' (v{input.Policy.Version})"
            else
                "No active Prioritization Policy configured")

/// BR-D-310: Entity assigned Unclassified if mandatory business evidence is missing
let minimumEvidenceRule: Rule<PrioritizationRuleInput> =
    Rule.create
        ArsIdentifiers.Rules.minimumEvidenceForPrioritization.Id
        ArsIdentifiers.Rules.minimumEvidenceForPrioritization.Explanation
        (fun input ->
            input.RevenueContribution.IsSome
            || input.StrategicImportance.IsSome
            || input.RiskExposure.IsSome
            || input.ContractualObligation.IsSome)
        (fun input ->
            let count =
                [ input.RevenueContribution.IsSome
                  input.StrategicImportance.IsSome
                  input.RiskExposure.IsSome
                  input.ContractualObligation.IsSome ]
                |> List.filter id
                |> List.length

            if count > 0 then
                $"%d{count} business dimension evidence inputs available for scoring"
            else
                "No business dimension evidence provided; assigned Unclassified priority")

/// Prioritization rules pipeline
let prioritizationRules: Rule<PrioritizationRuleInput> list =
    [ policyComplianceRule; minimumEvidenceRule ]

/// Mandatory justification rule for manual planner overrides (PO-D-040)
let overrideJustificationRule: Rule<OverrideRuleInput> =
    Rule.create
        "BR-D-310-OVR"
        "Manual priority overrides require non-empty business justification and registered planner"
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
