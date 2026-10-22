/// PO-S-130 — Supply Explanation Policy
/// Governs explanation composition, template catalog, confidence computation, and negative explanation rules
module Medhavi.Supply.ExplainSupplyDecisions.SupplyExplanation.Policies

/// PO-S-130: Explanation Policy Configuration
type ExplanationPolicy =
    {
        PolicyId: string
        PolicyVersion: string
        Version: int
        /// Weight assigned to inherited evidence confidence (default 40%)
        InheritedConfidenceWeight: decimal
        /// Weight assigned to evidence completeness (default 30%)
        CompletenessWeight: decimal
        /// Weight assigned to evidence consistency (default 30%)
        ConsistencyWeight: decimal
        /// Minimum threshold for High confidence rating (default 85.0%)
        HighConfidenceThreshold: decimal
        /// Minimum threshold for Medium confidence rating (default 60.0%)
        MediumConfidenceThreshold: decimal
        /// Mandatory requirement for canonical Structured Reasoning Graph
        RequireStructuredReasoningGraph: bool
        /// Mandatory requirement for Multi-Level Renderings (Planner, Waterfall, Full Audit, AI-Consumable)
        RequireMultiLevelRenderings: bool
        /// Mandatory requirement for quantitative Factor Attribution table
        RequireFactorWaterfall: bool
        /// Mandatory requirement for W3C PROV-O JSON-LD representation
        RequireAiConsumableJsonLd: bool
        /// Default template version
        DefaultTemplateVersion: string
    }

/// Ratified baseline enterprise policy parameters for PO-S-130
let defaultExplanationPolicy: ExplanationPolicy =
    {
        PolicyId = "PO-S-130"
        PolicyVersion = "PO-S-130:v1.0"
        Version = 1
        InheritedConfidenceWeight = 0.40m
        CompletenessWeight = 0.30m
        ConsistencyWeight = 0.30m
        HighConfidenceThreshold = 85.0m
        MediumConfidenceThreshold = 60.0m
        RequireStructuredReasoningGraph = true
        RequireMultiLevelRenderings = true
        RequireFactorWaterfall = true
        RequireAiConsumableJsonLd = true
        DefaultTemplateVersion = "v1.0"
    }
