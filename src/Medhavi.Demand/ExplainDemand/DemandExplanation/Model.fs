/// SE-D-010 — Demand Explanation Aggregate Model
/// Traces to: Demand Specification (SE-D-010, AB-D-016, BA-D-012, PO-D-047, PI-D-107, Chapter 4.3.1)
module Medhavi.Demand.ExplainDemand.DemandExplanation.Model

open Medhavi.SemanticModel
open Medhavi.Demand

// ---------- Graph Vertex & Edge Definitions ----------

/// Single vertex in the canonical Structured Reasoning Graph
type ExplanationNode =
    { NodeId: string
      NodeType: string // "Evidence", "DecisionTrace", "Policy", "FactorAttribution", "RuleEvaluation", "MissingEvidence", "CompletenessAssessment", "BusinessContext"
      Label: string
      Properties: Map<string, string>
      IsHypothetical: bool }

/// Single directed relationship edge in the Structured Reasoning Graph
type ExplanationEdge =
    { EdgeId: string
      SourceNode: string
      TargetNode: string
      Relationship: string // "GovernedBy", "DeterminedBy", "InfluencedBy", "EvaluatedBy", "DerivedFrom", "AdjustedBy", "ContributedToRevision"
      IsHypothetical: bool }

/// Canonical deterministic graph structure
type StructuredReasoningGraph =
    { Nodes: ExplanationNode list
      Edges: ExplanationEdge list }

// ---------- Quantitative Factor Attribution ----------

/// Direction of factor impact on baseline demand
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

// ---------- Preserved Historical Evidence Reference ----------

/// Preserved immutable evidence reference at a specific historical version
type ExplanationSourceArtifactRef =
    { ArtifactType: string
      ArtifactId: string
      Version: int
      Properties: Map<string, string> }

// ---------- Multi-Level Renderings ----------

/// Multi-tier representations serving Planners, Auditors, and AI Copilots
type MultiLevelRenderings =
    { PlannerSummary: string
      FactorWaterfallMarkdown: string
      FullAuditJson: string
      AiConsumableJsonLd: string }

// ---------- Aggregate Root State ----------

/// SE-D-010 — Demand Explanation Aggregate Root
/// Pattern: Explanation. Immutable once created.
type DemandExplanation =
    { Id: DemandExplanationId
      ExplainedArtifactType: string
      ExplainedArtifactId: string
      Version: int
      StructuredReasoningGraph: StructuredReasoningGraph
      MultiLevelRenderings: MultiLevelRenderings
      FactorContributions: FactorContribution list
      PreservedEvidenceRefs: ExplanationSourceArtifactRef list
      TemplateVersion: string
      ExplainabilityScore: decimal
      WhatIfAssumption: string option
      CreatedAt: Timestamp }

// ---------- Commands ----------

/// AB-D-016 Command: Establish Demand Explanation
type EstablishExplanationCmd =
    { ExplanationId: DemandExplanationId
      ExplainedArtifactType: string
      ExplainedArtifactId: string
      Version: int
      StructuredReasoningGraph: StructuredReasoningGraph
      MultiLevelRenderings: MultiLevelRenderings
      FactorContributions: FactorContribution list
      PreservedEvidenceRefs: ExplanationSourceArtifactRef list
      TemplateVersion: string
      ExplainabilityScore: decimal
      WhatIfAssumption: string option
      CreationTime: Timestamp }

// ---------- Enterprise Events ----------

/// EV-D-024 — Demand Explanation Established
type DemandExplanationEvent = DemandExplanationEstablished of DemandExplanation

// ---------- Pure State Evolution (Layer E: Catamorphism) ----------

let evolve: Medhavi.Foundation.Contracts.Evolve<DemandExplanation, DemandExplanationEvent> =
    fun (_: DemandExplanation option) (event: DemandExplanationEvent) ->
        match event with
        | DemandExplanationEstablished exp -> Some exp

/// Replay event sequence to rehydrate aggregate state
let replay (events: DemandExplanationEvent seq) : DemandExplanation option = Seq.fold evolve None events
