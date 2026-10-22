/// SE-S-081 — Distribution Plan Rules
/// Traces to: Supply Intelligence Specification §4.3.1 (BR-S-081, BR-S-156..158, BR-S-160..162, BR-S-231)
module Medhavi.Supply.ManageDistribution.DistributionPlan.Rules

open Medhavi.Foundation.Contracts
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Model
open Policies
open Algorithms

type PlanPublicationInput =
    { Composition: PlanCompositionResult
      Policy: DistributionPlanPublicationPolicyParameters
      HasActiveRecommendations: bool
      IsPolicyCurrent: bool }

/// BR-S-231 — Distribution plan publication eligibility
let publicationEligibility: Rule<PlanPublicationInput> =
    Rule.create
        Rules.distributionPlanPublicationEligibility.Id
        Rules.distributionPlanPublicationEligibility.Explanation
        (fun input -> input.HasActiveRecommendations && input.IsPolicyCurrent)
        (fun input -> $"HasActiveRecommendations: %b{input.HasActiveRecommendations}; IsPolicyCurrent: %b{input.IsPolicyCurrent}")

/// BR-S-156 — Completeness of distribution recommendations
let completenessCoverage: Rule<PlanPublicationInput> =
    Rule.create
        Rules.completenessOfDistributionRecommendations.Id
        Rules.completenessOfDistributionRecommendations.Explanation
        (fun input -> input.Composition.TotalMovements > 0 || not input.HasActiveRecommendations)
        (fun input -> $"TotalMovements: %d{input.Composition.TotalMovements}")

/// BR-S-157 — Plan confidence threshold
let confidenceThreshold: Rule<PlanPublicationInput> =
    Rule.create
        Rules.distributionPlanConfidenceThreshold.Id
        Rules.distributionPlanConfidenceThreshold.Explanation
        (fun input ->
            match input.Composition.ConfidenceDistribution.Overall with
            | DistributionConfidence.Low -> input.Policy.AllowPublicationWithFlags
            | _ -> true)
        (fun input -> $"Confidence: %A{input.Composition.ConfidenceDistribution.Overall}; AllowWithFlags: %b{input.Policy.AllowPublicationWithFlags}")

/// BR-S-158 — Unresolved feasibility documentation
let unresolvedFeasibilityDocumentation: Rule<PlanPublicationInput> =
    Rule.create
        Rules.distributionPlanUnresolvedFeasibilityIssues.Id
        Rules.distributionPlanUnresolvedFeasibilityIssues.Explanation
        (fun input ->
            if input.Composition.FeasibilitySummary.InfeasibleCount > 0 then
                input.Composition.RiskSignals |> List.exists (fun r -> r.Severity = RiskSeverity.High)
            else true)
        (fun input ->
            $"InfeasibleCount: %d{input.Composition.FeasibilitySummary.InfeasibleCount}; HighRiskDocumented: %b{input.Composition.RiskSignals |> List.exists (fun r -> r.Severity = RiskSeverity.High)}")

/// BR-S-160 — Distribution plan aggregates approved recommendations
let aggregatesApprovedRecommendations: Rule<PlanPublicationInput> =
    Rule.create
        Rules.distributionPlanAggregatesApprovedRecommendations.Id
        Rules.distributionPlanAggregatesApprovedRecommendations.Explanation
        (fun input -> input.Composition.FeasibilitySummary.OverallFeasibilityPercentage >= input.Policy.MinimumFeasibilityPercentage)
        (fun input ->
            $"FeasibilityPercentage: %.2f{input.Composition.FeasibilitySummary.OverallFeasibilityPercentage}; Minimum: %.2f{input.Policy.MinimumFeasibilityPercentage}")

/// Governance rule set for DE-S-072
let publicationRules: Rule<PlanPublicationInput> list =
    [ publicationEligibility
      completenessCoverage
      confidenceThreshold
      unresolvedFeasibilityDocumentation
      aggregatesApprovedRecommendations ]
