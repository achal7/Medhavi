namespace Medhavi.Contracts.Demand

open System
open System.Threading.Tasks
open Medhavi.Contracts

// =============================================================================
// CA-D-009 & SE-D-010 — Demand Explanation Public Contracts
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
      Version: int
      Properties: Map<string, string> }

/// Canonical SE-D-010 Demand Explanation DTO
type DemandExplanationDto =
    { ExplanationId: string
      ExplainedArtifactType: string
      ExplainedArtifactId: string
      Version: int
      StructuredReasoningGraph: StructuredReasoningGraphDto
      MultiLevelRenderings: MultiLevelRenderingsDto
      FactorContributions: FactorContributionDto list
      PreservedEvidenceRefs: PreservedEvidenceRefDto list
      TemplateVersion: string
      ExplainabilityScore: decimal
      WhatIfAssumption: string option
      Timestamp: DateTimeOffset }

// ---------- Commands / Requests ----------

/// Request payload to establish or retrieve a Demand Explanation
type EstablishDemandExplanationReq =
    { ExplainedArtifactType: string
      ExplainedArtifactId: string
      Version: int
      TemplateVersion: string option
      WhatIfAssumption: string option
      EvidenceRefs: PreservedEvidenceRefDto list }

// ---------- API Interface ----------

type DemandExplanationApi =
    { EstablishExplanation: EstablishDemandExplanationReq -> Task<Result<DemandExplanationDto, ApiError>> }

/// Query service alias
type DemandExplanationQueries = QueryService<DemandExplanationDto, string>
