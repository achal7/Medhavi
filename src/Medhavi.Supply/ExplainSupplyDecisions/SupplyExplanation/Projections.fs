/// Read-model projection for SE-S-120 Supply Explanation
module Medhavi.Supply.ExplainSupplyDecisions.SupplyExplanation.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model

/// Maps domain SupplyExplanation entity to public SupplyExplanationDto
let toDto (exp: SupplyExplanation) : SupplyExplanationDto =
    let nodeDto (n: ExplanationNode) : ExplanationNodeDto =
        {
            NodeId = n.NodeId
            NodeType = n.NodeType
            Label = n.Label
            Properties = n.Properties
            IsHypothetical = n.IsHypothetical
        }

    let edgeDto (e: ExplanationEdge) : ExplanationEdgeDto =
        {
            EdgeId = e.EdgeId
            SourceNode = e.SourceNode
            TargetNode = e.TargetNode
            Relationship = e.Relationship
            IsHypothetical = e.IsHypothetical
        }

    let graphDto: StructuredReasoningGraphDto =
        {
            Nodes = exp.StructuredReasoningGraph.Nodes |> List.map nodeDto
            Edges = exp.StructuredReasoningGraph.Edges |> List.map edgeDto
        }

    let factorDto (f: FactorContribution) : FactorContributionDto =
        {
            FactorName = f.FactorName
            BaseValue = f.BaseValue
            ImpactValue = f.ImpactValue
            PercentageContribution = f.PercentageContribution
            Direction = f.Direction.AsString
            Confidence = f.Confidence
        }

    let renderingsDto: MultiLevelRenderingsDto =
        {
            PlannerSummary = exp.MultiLevelRenderings.PlannerSummary
            FactorWaterfallMarkdown = exp.MultiLevelRenderings.FactorWaterfallMarkdown
            FullAuditJson = exp.MultiLevelRenderings.FullAuditJson
            AiConsumableJsonLd = exp.MultiLevelRenderings.AiConsumableJsonLd
        }

    let sourceRefDto (r: ExplanationSourceArtifactRef) : PreservedEvidenceRefDto =
        {
            ArtifactType = r.ArtifactType
            ArtifactId = r.ArtifactId
            Version = r.Version
            Properties = r.Properties
        }

    let rationaleBlockDto (rb: RationaleBlock) : RationaleBlockDto =
        {
            RationaleBlockId = rb.RationaleBlockId
            OutcomeDimension = rb.OutcomeDimension
            CausalRuleRef = rb.CausalRuleRef
            InputEvidenceRef = rb.InputEvidenceRef
            GovernedNarrative = rb.GovernedNarrative
        }

    let negativeDto (n: NegativeExplanationPayload) : NegativeExplanationDto =
        {
            DiscardedAlternativeId = n.DiscardedAlternativeId
            DiscardedAlternativeType = n.DiscardedAlternativeType
            ReasonCategory = n.ReasonCategory.AsString
            FormalReason = n.FormalReason
            EvaluationContext = n.EvaluationContext
        }

    let modelDto (m: OpaqueModelProvenance) : OpaqueModelProvenanceDto =
        {
            OpaqueModelId = m.OpaqueModelId
            ModelVersion = m.ModelVersion
            ModelConfidence = m.ModelConfidence
            GuardrailRuleReferences = m.GuardrailRuleReferences
            KnownLimitations = m.KnownLimitations
        }

    let confidenceDto: ConfidenceBreakdownDto =
        {
            InheritedEvidenceConfidence = exp.Confidence.InheritedEvidenceConfidence
            EvidenceCompleteness = exp.Confidence.EvidenceCompleteness
            EvidenceConsistency = exp.Confidence.EvidenceConsistency
            CompositeIndex = exp.Confidence.CompositeIndex
            ConfidenceLevel = exp.Confidence.ConfidenceLevel.AsString
        }

    {
        ExplanationId = SupplyExplanationId.value exp.Id
        ExplainedArtifactType = exp.ExplainedArtifactType
        ExplainedArtifactId = exp.ExplainedArtifactId
        ExplainedArtifactVersion = exp.ExplainedArtifactVersion
        StructuredReasoningGraph = graphDto
        MultiLevelRenderings = renderingsDto
        FactorContributions = exp.FactorContributions |> List.map factorDto
        PreservedEvidenceRefs = exp.PreservedEvidenceRefs |> List.map sourceRefDto
        RationaleBlocks = exp.RationaleBlocks |> List.map rationaleBlockDto
        IsNegativeExplanation = exp.IsNegativeExplanation
        NegativeExplanation = exp.NegativeExplanation |> Option.map negativeDto
        OpaqueModelProvenance = exp.OpaqueModelProvenance |> Option.map modelDto
        Confidence = confidenceDto
        TemplateVersionReference = exp.TemplateVersionReference
        CompositionTimestamp = Timestamp.value exp.CompositionTimestamp
    }

/// Projection state: map of SupplyExplanationId to SupplyExplanationDto
type State = Map<SupplyExplanationId, SupplyExplanationDto>

let initial: State = Map.empty

/// Pure projection fold (catamorphism)
let apply (state: State) (event: SupplyExplanationEvent) : State =
    match event with
    | SupplyExplanationRecorded exp -> state |> Map.add exp.Id (toDto exp)

/// Seeds initial read model state from loaded repository aggregates
let seedFromAggregates (aggregates: SupplyExplanation list) : State =
    aggregates |> List.fold (fun state agg -> Map.add agg.Id (toDto agg) state) initial
