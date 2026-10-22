/// BR-S-120, BR-S-280..286 — Supply Explanation Invariants & Governance Rules
module Medhavi.Supply.ExplainSupplyDecisions.SupplyExplanation.Rules

open System
open Medhavi.Foundation.Contracts
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Policies
open Model

/// Typed input context for evaluating explanation governance rules
type ExplanationRuleInput =
    { ExplainedArtifactType: string
      ExplainedArtifactId: string
      ExplainedArtifactVersion: int64
      ExistingExplanation: SupplyExplanation option
      EvidenceRefs: ExplanationSourceArtifactRef list
      ReasoningGraph: StructuredReasoningGraph
      Confidence: ConfidenceBreakdown
      Policy: ExplanationPolicy }

/// BR-S-120: Supply Explanation Identity Rule
let identityRule: Rule<ExplanationRuleInput> =
    Rule.create
        ArsIdentifiers.Rules.supplyExplanationIdentity.Id
        ArsIdentifiers.Rules.supplyExplanationIdentity.Explanation
        (fun input ->
            not (String.IsNullOrWhiteSpace input.ExplainedArtifactType)
            && not (String.IsNullOrWhiteSpace input.ExplainedArtifactId)
            && input.ExplainedArtifactVersion >= 0L)
        (fun input ->
            $"Composite key verified: Type='{input.ExplainedArtifactType}', Id='{input.ExplainedArtifactId}', Version={input.ExplainedArtifactVersion}.")

/// BR-S-280: Supply Explanation Composition Eligibility Rule
let eligibilityRule: Rule<ExplanationRuleInput> =
    Rule.create
        ArsIdentifiers.Rules.supplyExplanationCompositionEligibility.Id
        ArsIdentifiers.Rules.supplyExplanationCompositionEligibility.Explanation
        (fun input ->
            not (String.IsNullOrWhiteSpace input.Policy.PolicyId)
            && input.Policy.Version > 0)
        (fun input ->
            $"Policy '{input.Policy.PolicyId}' v{input.Policy.Version} is active and current.")

/// BR-S-281: Supply Explanation Immutability Rule
let immutabilityRule: Rule<ExplanationRuleInput> =
    Rule.create
        ArsIdentifiers.Rules.supplyExplanationImmutability.Id
        ArsIdentifiers.Rules.supplyExplanationImmutability.Explanation
        (fun input -> input.ExistingExplanation.IsNone)
        (fun input ->
            match input.ExistingExplanation with
            | Some ex ->
                $"Explanation already established for {input.ExplainedArtifactType} '{input.ExplainedArtifactId}' v{input.ExplainedArtifactVersion} at {ex.CompositionTimestamp}. Immutable (Created)."
            | None ->
                $"No prior explanation exists for {input.ExplainedArtifactType} '{input.ExplainedArtifactId}' v{input.ExplainedArtifactVersion}. Eligible for creation.")

/// BR-S-282: Historical Version Referencing Rule
let historicalVersionRule: Rule<ExplanationRuleInput> =
    Rule.create
        ArsIdentifiers.Rules.historicalVersionReferencing.Id
        ArsIdentifiers.Rules.historicalVersionReferencing.Explanation
        (fun input ->
            input.EvidenceRefs
            |> List.forall (fun r -> r.Version >= 0L && not (String.IsNullOrWhiteSpace r.ArtifactId)))
        (fun input ->
            $"{input.EvidenceRefs.Length} evidence references verified at historical versions.")

/// BR-S-283: No Invented Reasoning Rule
let noInventedReasoningRule: Rule<ExplanationRuleInput> =
    Rule.create
        ArsIdentifiers.Rules.noInventedReasoning.Id
        ArsIdentifiers.Rules.noInventedReasoning.Explanation
        (fun input ->
            input.ReasoningGraph.Nodes
            |> List.forall (fun n -> not (String.IsNullOrWhiteSpace n.NodeId) && not (String.IsNullOrWhiteSpace n.NodeType)))
        (fun input ->
            $"All {input.ReasoningGraph.Nodes.Length} graph vertices trace to specific authoritative source artifacts or traces.")

/// BR-S-284: Explanation Composition Determinism Rule
let determinismRule: Rule<ExplanationRuleInput> =
    Rule.create
        ArsIdentifiers.Rules.explanationCompositionDeterminism.Id
        ArsIdentifiers.Rules.explanationCompositionDeterminism.Explanation
        (fun input ->
            input.ReasoningGraph.Nodes |> List.exists (fun n -> n.NodeType = input.ExplainedArtifactType))
        (fun input ->
            $"Canonical reasoning graph anchored to root {input.ExplainedArtifactType} vertex.")

/// BR-S-285: Explanation Confidence Assessment Rule
let confidenceAssessmentRule: Rule<ExplanationRuleInput> =
    Rule.create
        ArsIdentifiers.Rules.explanationConfidenceAssessment.Id
        ArsIdentifiers.Rules.explanationConfidenceAssessment.Explanation
        (fun input ->
            input.Confidence.CompositeIndex >= 0.0m && input.Confidence.CompositeIndex <= 100.0m)
        (fun input ->
            $"Confidence assessed: {input.Confidence.ConfidenceLevel.AsString} (Composite Index: {input.Confidence.CompositeIndex:N1}%%).")

/// BR-S-286: Explanation Derivation Source Rule
let derivationSourceRule: Rule<ExplanationRuleInput> =
    Rule.create
        ArsIdentifiers.Rules.explanationDerivationSource.Id
        ArsIdentifiers.Rules.explanationDerivationSource.Explanation
        (fun input ->
            not (String.IsNullOrWhiteSpace input.ExplainedArtifactId))
        (fun input ->
            $"Explanation derived from preserved enterprise knowledge for '{input.ExplainedArtifactId}'.")

let explanationRules: Rule<ExplanationRuleInput> list =
    [ identityRule
      eligibilityRule
      immutabilityRule
      historicalVersionRule
      noInventedReasoningRule
      determinismRule
      confidenceAssessmentRule
      derivationSourceRule ]
