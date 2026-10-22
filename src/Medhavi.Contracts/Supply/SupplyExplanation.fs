// Medhavi.Contracts.Supply.SupplyExplanation
// Traceability: SE-S-120 Supply Explanation, CR-S-024, EV-S-110, BN-S-110
namespace Medhavi.Contracts.Supply

open System
open System.Threading.Tasks
open Medhavi.Contracts

// =============================================================================
// CA-S-012 & SE-S-120 — Supply Explanation Public Contracts
// =============================================================================

/// Single vertex in the Structured Reasoning Graph DTO
type ExplanationNodeDto =
    { NodeId: string
      NodeType: string
      Label: string
      Properties: Map<string, string>
      IsHypothetical: bool }

/// Single directed relationship edge in the Structured Reasoning Graph DTO
type ExplanationEdgeDto =
    { EdgeId: string
      SourceNode: string
      TargetNode: string
      Relationship: string
      IsHypothetical: bool }

/// Canonical graph structure DTO
type StructuredReasoningGraphDto =
    { Nodes: ExplanationNodeDto list
      Edges: ExplanationEdgeDto list }

/// Quantitative factor attribution driver DTO (Waterfall Reconciliation)
type FactorContributionDto =
    { FactorName: string
      BaseValue: decimal
      ImpactValue: decimal
      PercentageContribution: decimal
      Direction: string
      Confidence: decimal }

/// Multi-tier representations DTO serving Planners, Auditors, and AI Agents
type MultiLevelRenderingsDto =
    { PlannerSummary: string
      FactorWaterfallMarkdown: string
      FullAuditJson: string
      AiConsumableJsonLd: string }

/// Preserved historical evidence reference DTO
type PreservedEvidenceRefDto =
    { ArtifactType: string
      ArtifactId: string
      Version: int64
      Properties: Map<string, string> }

/// Governed Rationale Block DTO (BR-S-233)
type RationaleBlockDto =
    { RationaleBlockId: string
      OutcomeDimension: string
      CausalRuleRef: string
      InputEvidenceRef: string
      GovernedNarrative: string }

/// Negative explanation details DTO (BA-S-090 §7.2)
type NegativeExplanationDto =
    { DiscardedAlternativeId: string
      DiscardedAlternativeType: string
      ReasonCategory: string
      FormalReason: string
      EvaluationContext: Map<string, string> }

/// Opaque model provenance disclosure DTO (BR-S-234)
type OpaqueModelProvenanceDto =
    { OpaqueModelId: string
      ModelVersion: string
      ModelConfidence: decimal
      GuardrailRuleReferences: string list
      KnownLimitations: string }

/// 3-factor explanation confidence breakdown DTO (BA-S-090 §7.3 & PO-S-130)
type ConfidenceBreakdownDto =
    { InheritedEvidenceConfidence: decimal
      EvidenceCompleteness: decimal
      EvidenceConsistency: decimal
      CompositeIndex: decimal
      ConfidenceLevel: string }

/// Canonical SE-S-120 Supply Explanation DTO
type SupplyExplanationDto =
    { ExplanationId: string
      ExplainedArtifactType: string
      ExplainedArtifactId: string
      ExplainedArtifactVersion: int64
      StructuredReasoningGraph: StructuredReasoningGraphDto
      MultiLevelRenderings: MultiLevelRenderingsDto
      FactorContributions: FactorContributionDto list
      PreservedEvidenceRefs: PreservedEvidenceRefDto list
      RationaleBlocks: RationaleBlockDto list
      IsNegativeExplanation: bool
      NegativeExplanation: NegativeExplanationDto option
      OpaqueModelProvenance: OpaqueModelProvenanceDto option
      Confidence: ConfidenceBreakdownDto
      TemplateVersionReference: string
      CompositionTimestamp: DateTimeOffset }

// ---------- Commands / Requests ----------

/// Request payload to record or retrieve a Supply Explanation
type RecordSupplyExplanationReq =
    { ExplainedArtifactType: string
      ExplainedArtifactId: string
      ExplainedArtifactVersion: int64
      TemplateVersion: string option
      IsNegativeExplanation: bool
      NegativeAlternativeId: string option
      NegativeAlternativeType: string option
      NegativeReasonCategory: string option
      WhatIfAssumption: string option
      EvidenceRefs: PreservedEvidenceRefDto list }

// ---------- Business Notifications ----------

/// BN-S-110 Supply Explanation Recorded Business Notification
type SupplyExplanationNotification =
    { ExplanationId: string
      ExplainedArtifactType: string
      ExplainedArtifactId: string
      ExplainedArtifactVersion: int64
      IsNegativeExplanation: bool
      ExplanationConfidence: string
      PublishedAt: DateTimeOffset }

// ---------- API Interface ----------

/// Capability public command API for CA-S-012 Explain Supply Decisions
type SupplyExplanationApi =
    { RecordExplanation: RecordSupplyExplanationReq -> Task<Result<SupplyExplanationDto, ApiError>> }

/// Query service alias for SE-S-120 Supply Explanation
type SupplyExplanationQueries = QueryService<SupplyExplanationDto, string>
