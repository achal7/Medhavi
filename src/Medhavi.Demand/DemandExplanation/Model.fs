module Medhavi.Demand.DemandExplanation.Model

open Medhavi.SharedKernel
open Medhavi.Demand

// =============================================================================
// SE‑D‑041 — Demand Explanation
// =============================================================================

// Nodes and edges of the Structured Reasoning Graph.
type ExplanationNode =
    { NodeId: string
      NodeType: string // Rule‑Based, Statistical, ML, Planner‑Judgment, etc.
      Label: string
      Properties: Map<string, string> }

type ExplanationEdge =
    { EdgeId: string
      SourceNode: string
      TargetNode: string
      Relationship: string } // Influenced, Determined, Overrode, etc.

type StructuredReasoningGraph =
    { Nodes: ExplanationNode list
      Edges: ExplanationEdge list }

type ExplanationSourceArtifactRef =
    { ArtifactType: string
      ArtifactId: string
      Version: int
      Properties: Map<string, string> }

type DemandExplanation =
    { Id: DemandExplanationId
      ExplainedArtifactType: string
      ExplainedArtifactId: string
      StructuredReasoningGraph: StructuredReasoningGraph
      NaturalLanguageExplanation: string
      SourceArtifactRefs: ExplanationSourceArtifactRef list
      ExplanationGenerationTimestamp: Timestamp
      TemplateVersionRef: string
      BusinessTime: Timestamp
      TransactionTime: Timestamp }

// ---------- Commands ----------
type RecordDemandExplanationCmd =
    { ExplanationId: DemandExplanationId
      ExplainedArtifactType: string
      ExplainedArtifactId: string
      StructuredReasoningGraph: StructuredReasoningGraph
      NaturalLanguageExplanation: string
      SourceArtifactRefs: ExplanationSourceArtifactRef list
      TemplateVersionRef: string
      BusinessTime: Timestamp
      TransactionTime: Timestamp }

type DemandExplanationCommand =
    | RecordDemandExplanation of RecordDemandExplanationCmd

    member this.ExplanationId =
        match this with
        | RecordDemandExplanation c -> c.ExplanationId
        |> DemandExplanationId.value

// ---------- Events ----------
type DemandExplanationEvent = DemandExplanationRecorded of DemandExplanation

// ---------- Evolve ----------
let evolve (evt: DemandExplanationEvent) (_: DemandExplanation option) =
    match evt with
    | DemandExplanationRecorded exp -> Some exp
