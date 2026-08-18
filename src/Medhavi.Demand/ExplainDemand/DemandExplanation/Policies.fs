/// PO-D-047 — Explanation Governance Policy
/// Governs explanation completeness, determinism, required template elements, and evidence preservation
module Medhavi.Demand.ExplainDemand.DemandExplanation.Policies

/// PO-D-047: Explanation Governance Policy
type ExplanationGovernancePolicy =
    { PolicyId: string
      PolicyVersion: string
      Version: int
      /// Minimum fraction of required evidence present to consider explanation complete (PI-D-107 threshold)
      MinimumCompletenessThreshold: decimal
      /// Mandatory requirement for canonical Structured Reasoning Graph
      RequireStructuredReasoningGraph: bool
      /// Mandatory requirement for Multi-Level Renderings (Planner, Waterfall, Full Audit, AI-Consumable)
      RequireMultiLevelRenderings: bool
      /// Mandatory requirement for quantitative Factor Attribution
      RequireFactorWaterfall: bool
      /// Mandatory requirement for W3C PROV-O JSON-LD representation
      RequireAiConsumableJsonLd: bool
      /// Default template version
      DefaultTemplateVersion: string }

module ExplanationGovernancePolicy =
    let defaultPolicy: ExplanationGovernancePolicy =
        { PolicyId = "PO-D-047"
          PolicyVersion = "PO-D-047:v1.0"
          Version = 1
          MinimumCompletenessThreshold = 80.0m
          RequireStructuredReasoningGraph = true
          RequireMultiLevelRenderings = true
          RequireFactorWaterfall = true
          RequireAiConsumableJsonLd = true
          DefaultTemplateVersion = "v1.0" }
