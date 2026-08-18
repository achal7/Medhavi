/// BR-D-124 — Explanation Immutability & Completeness Rules
module Medhavi.Demand.ExplainDemand.DemandExplanation.Rules

open Medhavi.Foundation.Contracts
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers
open Policies
open Model

/// Typed input context for evaluating explanation governance rules
type ExplanationRuleInput =
    { ExplainedArtifactType: string
      ExplainedArtifactId: string
      Version: int
      ExistingExplanation: DemandExplanation option
      ExplainabilityScore: decimal
      Policy: ExplanationGovernancePolicy }

/// BR-D-124: Explanation Immutability Rule
let immutabilityRule: Rule<ExplanationRuleInput> =
    Rule.create
        ArsIdentifiers.Rules.explanationImmutability.Id
        ArsIdentifiers.Rules.explanationImmutability.Explanation
        (fun input -> input.ExistingExplanation.IsNone)
        (fun input ->
            match input.ExistingExplanation with
            | Some ex -> $"Explanation already established for {input.ExplainedArtifactType} '{input.ExplainedArtifactId}' v{input.Version} at {ex.CreatedAt}. Immutable."
            | None -> $"No prior explanation exists for {input.ExplainedArtifactType} '{input.ExplainedArtifactId}' v{input.Version}. Eligible for creation.")

let explanationRules: Rule<ExplanationRuleInput> list =
    [ immutabilityRule ]
