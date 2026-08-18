/// SE-D-010 — Demand Explanation Read Model Projections
/// Pure Functional Projection Fold (Layer E: Catamorphism)
module Medhavi.Demand.ExplainDemand.DemandExplanation.Projections

open Medhavi.SemanticModel
open Medhavi.Demand
open Medhavi.Contracts.Demand
open Model

let mapNodeToDto (node: ExplanationNode) : ExplanationNodeDto =
    { NodeId = node.NodeId
      NodeType = node.NodeType
      Label = node.Label
      Properties = node.Properties
      IsHypothetical = node.IsHypothetical }

let mapEdgeToDto (edge: ExplanationEdge) : ExplanationEdgeDto =
    { EdgeId = edge.EdgeId
      SourceNode = edge.SourceNode
      TargetNode = edge.TargetNode
      Relationship = edge.Relationship
      IsHypothetical = edge.IsHypothetical }

let mapGraphToDto (graph: StructuredReasoningGraph) : StructuredReasoningGraphDto =
    { Nodes = graph.Nodes |> List.map mapNodeToDto
      Edges = graph.Edges |> List.map mapEdgeToDto }

let mapFactorToDto (f: FactorContribution) : FactorContributionDto =
    { FactorName = f.FactorName
      BaseValue = f.BaseValue
      ImpactValue = f.ImpactValue
      PercentageContribution = f.PercentageContribution
      Direction = f.Direction.AsString
      Confidence = f.Confidence }

let mapEvidenceRefToDto (ref: ExplanationSourceArtifactRef) : PreservedEvidenceRefDto =
    { ArtifactType = ref.ArtifactType
      ArtifactId = ref.ArtifactId
      Version = ref.Version
      Properties = ref.Properties }

let mapToDto (aggregate: DemandExplanation) : DemandExplanationDto =
    { ExplanationId = DemandExplanationId.value aggregate.Id
      ExplainedArtifactType = aggregate.ExplainedArtifactType
      ExplainedArtifactId = aggregate.ExplainedArtifactId
      Version = aggregate.Version
      StructuredReasoningGraph = mapGraphToDto aggregate.StructuredReasoningGraph
      MultiLevelRenderings =
        { PlannerSummary = aggregate.MultiLevelRenderings.PlannerSummary
          FactorWaterfallMarkdown = aggregate.MultiLevelRenderings.FactorWaterfallMarkdown
          FullAuditJson = aggregate.MultiLevelRenderings.FullAuditJson
          AiConsumableJsonLd = aggregate.MultiLevelRenderings.AiConsumableJsonLd }
      FactorContributions = aggregate.FactorContributions |> List.map mapFactorToDto
      PreservedEvidenceRefs = aggregate.PreservedEvidenceRefs |> List.map mapEvidenceRefToDto
      TemplateVersion = aggregate.TemplateVersion
      ExplainabilityScore = aggregate.ExplainabilityScore
      WhatIfAssumption = aggregate.WhatIfAssumption
      Timestamp = Timestamp.value aggregate.CreatedAt }

/// Projection state: Map of DemandExplanationId to DemandExplanationDto
type State = Map<DemandExplanationId, DemandExplanationDto>

let initial: State = Map.empty

/// Pure projection fold (Layer E: Catamorphism)
let apply (state: State) (event: DemandExplanationEvent) : State =
    match event with
    | DemandExplanationEstablished exp ->
        let dto = mapToDto exp
        Map.add exp.Id dto state

/// Seed projection from existing aggregates
let seedFromAggregates (aggregates: DemandExplanation list) : State =
    aggregates
    |> List.fold
        (fun state agg ->
            let dto = mapToDto agg
            Map.add agg.Id dto state)
        initial
