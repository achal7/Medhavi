/// SE-S-120 — Supply Explanation Aggregate Model
/// Traces to: Supply Intelligence Specification (SE-S-120, AB-S-110, BA-S-090, PO-S-130, Chapter 4.3.1, Chapter 5.12)
module Medhavi.Supply.ExplainSupplyDecisions.SupplyExplanation.Model

open System
open Medhavi.SemanticModel
open Medhavi.Supply

// =============================================================================
// GRAPH VERTEX & EDGE DEFINITIONS (CANONICAL REASONING STRUCTURE)
// =============================================================================

/// Single vertex in the canonical Structured Reasoning Graph (ARS §3.1, BA-S-090)
type ExplanationNode =
    { NodeId: string
      NodeType: string // "Policy", "DecisionTrace", "RuleEvaluation", "SourceArtifact", "FactorAttribution", "BusinessContext"
      Label: string
      Properties: Map<string, string>
      IsHypothetical: bool }

/// Single directed relationship edge in the Structured Reasoning Graph
type ExplanationEdge =
    { EdgeId: string
      SourceNode: string
      TargetNode: string
      Relationship: string // "GovernedBy", "DeterminedBy", "InfluencedBy", "EvaluatedBy", "DerivedFrom", "AdjustedBy", "ContributedToRevision", "DecisionFollowedBy", "Summarizes"
      IsHypothetical: bool }

/// Canonical deterministic graph structure
type StructuredReasoningGraph =
    { Nodes: ExplanationNode list
      Edges: ExplanationEdge list }

// =============================================================================
// QUANTITATIVE FACTOR ATTRIBUTION (WATERFALL RECONCILIATION)
// =============================================================================

/// Direction of factor impact on baseline supply/capacity
type FactorDirection =
    | Uplift
    | Reduction
    | Neutral

    member this.AsString =
        match this with
        | Uplift -> "Uplift"
        | Reduction -> "Reduction"
        | Neutral -> "Neutral"

    static member FromImpact(impact: decimal) : FactorDirection =
        if impact > 0.0m then Uplift
        elif impact < 0.0m then Reduction
        else Neutral

/// Quantitative factor attribution driver (Waterfall Reconciliation)
type FactorContribution =
    { FactorName: string
      BaseValue: decimal
      ImpactValue: decimal
      PercentageContribution: decimal
      Direction: FactorDirection
      Confidence: decimal }

// =============================================================================
// PRESERVED HISTORICAL EVIDENCE REFERENCE
// =============================================================================

/// Preserved immutable evidence reference at a specific historical version (BR-S-282)
type ExplanationSourceArtifactRef =
    { ArtifactType: string
      ArtifactId: string
      Version: int64
      Properties: Map<string, string> }

// =============================================================================
// GOVERNED RATIONALE BLOCKS (BR-S-233)
// =============================================================================

/// Structured rationale block mapping outcome dimensions to causal rules and evidence
type RationaleBlock =
    { RationaleBlockId: string
      OutcomeDimension: string
      CausalRuleRef: string
      InputEvidenceRef: string
      GovernedNarrative: string }

// =============================================================================
// NEGATIVE EXPLANATION FORMULATION (BA-S-090 §7.2)
// =============================================================================

/// Category of formal negative explanation reason
type NegativeReasonType =
    | PreconditionViolated
    | GateThresholdUnmet
    | DominantAlternativeSelected
    | PolicyConstraintVeto

    member this.AsString =
        match this with
        | PreconditionViolated -> "PreconditionViolated"
        | GateThresholdUnmet -> "GateThresholdUnmet"
        | DominantAlternativeSelected -> "DominantAlternativeSelected"
        | PolicyConstraintVeto -> "PolicyConstraintVeto"

/// Formal payload for negative reasoning why an alternative was deliberately not chosen
type NegativeExplanationPayload =
    { DiscardedAlternativeId: string
      DiscardedAlternativeType: string
      ReasonCategory: NegativeReasonType
      FormalReason: string
      EvaluationContext: Map<string, string> }

// =============================================================================
// OPAQUE MODEL PROVENANCE (BR-S-234)
// =============================================================================

/// Mandatory disclosure recorded when an AI/ML opaque model contributed to the outcome
type OpaqueModelProvenance =
    { OpaqueModelId: string
      ModelVersion: string
      ModelConfidence: decimal
      GuardrailRuleReferences: string list
      KnownLimitations: string }

// =============================================================================
// 3-FACTOR EXPLANATION CONFIDENCE (BA-S-090 §7.3 & PO-S-130)
// =============================================================================

/// Categorical explanation confidence level
type ExplanationConfidenceLevel =
    | High
    | Medium
    | Low

    member this.AsString =
        match this with
        | High -> "High"
        | Medium -> "Medium"
        | Low -> "Low"

/// Full 3-factor breakdown of explanation trustworthiness
type ConfidenceBreakdown =
    { InheritedEvidenceConfidence: decimal // Gamma_inh
      EvidenceCompleteness: decimal        // Kappa_comp
      EvidenceConsistency: decimal         // Sigma_cons
      CompositeIndex: decimal              // Lambda_exp
      ConfidenceLevel: ExplanationConfidenceLevel }

// =============================================================================
// MULTI-LEVEL RENDERINGS
// =============================================================================

/// Multi-tier representations serving Planners, Auditors, and AI Copilots
type MultiLevelRenderings =
    { PlannerSummary: string
      FactorWaterfallMarkdown: string
      FullAuditJson: string
      AiConsumableJsonLd: string }

// =============================================================================
// AGGREGATE ROOT STATE
// =============================================================================

/// Single-Version Record lifecycle state: terminal and immutable once created (BR-S-281)
type SupplyExplanationState =
    | Created

/// SE-S-120 — Supply Explanation Aggregate Root
/// Pattern: Preserved Reasoning / Single-Version Record (ARS §3.1). Immutable once created.
type SupplyExplanation =
    { Id: SupplyExplanationId
      ExplainedArtifactType: string
      ExplainedArtifactId: string
      ExplainedArtifactVersion: int64
      State: SupplyExplanationState
      StructuredReasoningGraph: StructuredReasoningGraph
      MultiLevelRenderings: MultiLevelRenderings
      FactorContributions: FactorContribution list
      PreservedEvidenceRefs: ExplanationSourceArtifactRef list
      RationaleBlocks: RationaleBlock list
      IsNegativeExplanation: bool
      NegativeExplanation: NegativeExplanationPayload option
      OpaqueModelProvenance: OpaqueModelProvenance option
      Confidence: ConfidenceBreakdown
      TemplateVersionReference: string
      CompositionTimestamp: Timestamp }

// =============================================================================
// COMMANDS
// =============================================================================

/// AB-S-110 Command: Record Supply Explanation
type ComposeSupplyExplanationCmd =
    { ExplanationId: SupplyExplanationId
      ExplainedArtifactType: string
      ExplainedArtifactId: string
      ExplainedArtifactVersion: int64
      StructuredReasoningGraph: StructuredReasoningGraph
      MultiLevelRenderings: MultiLevelRenderings
      FactorContributions: FactorContribution list
      PreservedEvidenceRefs: ExplanationSourceArtifactRef list
      RationaleBlocks: RationaleBlock list
      IsNegativeExplanation: bool
      NegativeExplanation: NegativeExplanationPayload option
      OpaqueModelProvenance: OpaqueModelProvenance option
      Confidence: ConfidenceBreakdown
      TemplateVersionReference: string
      CompositionTimestamp: Timestamp }

// =============================================================================
// ENTERPRISE EVENTS
// =============================================================================

/// EV-S-110 — Supply Explanation Recorded
type SupplyExplanationEvent =
    | SupplyExplanationRecorded of SupplyExplanation

// =============================================================================
// PURE STATE EVOLUTION (LAYER E: CATAMORPHISM)
// =============================================================================

let evolve: Medhavi.Foundation.Contracts.Evolve<SupplyExplanation, SupplyExplanationEvent> =
    fun (_: SupplyExplanation option) (event: SupplyExplanationEvent) ->
        match event with
        | SupplyExplanationRecorded exp -> Some exp

/// Replay event sequence to rehydrate aggregate state
let replay (events: SupplyExplanationEvent seq) : SupplyExplanation option =
    Seq.fold evolve None events
