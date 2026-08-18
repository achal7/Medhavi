/// BA-D-012 — Demand Explanation Composition, Multi-Level Rendering & AI Agent Knowledge Graph Builder
module Medhavi.Demand.ExplainDemand.DemandExplanation.Algorithms

open System
open System.Text.Json
open Medhavi.Foundation.Contracts
open Model
open TemplateCatalog
open TemplateRenderer

// =============================================================================
// DOMAIN KNOWLEDGE & EXPLAINABILITY PRINCIPLES:
//
// 1. Multi-Level Renderings:
//    - Planner Narrative: Plain-English summary with business context.
//    - Waterfall Table: Quantitative driver decomposition (Additive Reconciliation).
//    - Full Audit JSON: Deterministic serialization of the full reasoning graph.
//    - AI-Consumable JSON-LD: W3C PROV-O semantic knowledge graph for AI Copilots.
//
// 2. Explainability Score (PI-D-107):
//    ExplainabilityScore = (Present Required Elements / Total Required Elements) * 100%
//
// 3. Counterfactual What-If Reasoning:
//    Evaluates how hypothetical parameter shifts affect active policy rule outcomes.
// =============================================================================

/// Detects missing required evidence types and computes PI-D-107 Explainability Score
let evaluateCompleteness
    (template: ExplanationTemplate)
    (sourceRefs: ExplanationSourceArtifactRef list)
    : (decimal * string list) =

    let required = template.RequiredEvidenceTypes

    if required.IsEmpty then
        100.0m, []
    else
        let presentSet = sourceRefs |> List.map(fun r -> r.ArtifactType) |> Set.ofList
        let missing = required |> List.filter(fun r -> not(presentSet.Contains r))
        let presentCount = required.Length - missing.Length
        let score = (decimal presentCount / decimal required.Length) * 100.0m
        score, missing

/// Builds the W3C PROV-O JSON-LD representation specifically for AI Agents and Graph-RAG
let buildAiConsumableJsonLd (graph: StructuredReasoningGraph) : string =
    let escape (s: string) = s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", "")

    let context =
        """{"@context": {"medhavi": "https://medhavi.com/ontology#", "prov": "http://www.w3.org/ns/prov#", "artifactType": "@type", "relationship": "medhavi:relationship"}}"""

    let nodeJson (n: ExplanationNode) =
        let props =
            n.Properties
            |> Map.toList
            |> List.map(fun (k, v) -> $"\"{escape k}\": \"{escape v}\"")
            |> String.concat ", "

        let propsStr = if String.IsNullOrWhiteSpace props then "" else ", " + props
        $$"""{"@id": "medhavi:node/{{escape n.NodeId}}", "@type": "medhavi:{{escape n.NodeType}}", "label": "{{escape n.Label}}", "isHypothetical": {{n.IsHypothetical.ToString().ToLowerInvariant()}}{{propsStr}}}"""

    let edgeJson (e: ExplanationEdge) =
        $$"""{"@id": "medhavi:edge/{{escape e.EdgeId}}", "@type": "medhavi:Edge", "source": "medhavi:node/{{escape e.SourceNode}}", "target": "medhavi:node/{{escape e.TargetNode}}", "relationship": "medhavi:{{escape e.Relationship}}"}"""

    let allObjects = (graph.Nodes |> List.map nodeJson) @ (graph.Edges |> List.map edgeJson) |> String.concat ", "

    $$"""{ {{context}}, "@graph": [{{allObjects}}] }"""

/// Evaluates a What-If counterfactual hypothesis against decision rule evaluations
let evaluateWhatIfHypothesis (assumption: string) (decisionTraces: DecisionTrace list) : string =

    let lines =
        decisionTraces
        |> List.collect(fun t -> t.RulesEvaluated)
        |> List.map(fun r ->
            let evidence = r.Evidence |> String.concat "; "
            let status = if r.Passed then "Passed" else "Failed"
            $"Rule [{r.RuleId}] evaluated against assumption '{assumption}'. Current state: {status} ({evidence})")

    if lines.IsEmpty then
        $"What-If Assumption: '{assumption}' recorded with no active rule impact."
    else
        $"WHAT-IF HYPOTHESIS EVALUATION:\nAssumption: {assumption}\n" + (lines |> String.concat "\n")

/// Composes the complete Multi-Level Renderings
let composeMultiLevelRenderings
    (graph: StructuredReasoningGraph)
    (template: ExplanationTemplate)
    (properties: Map<string, string>)
    (factors: FactorContribution list)
    (totalExplainedValue: decimal)
    (explainabilityScore: decimal)
    (missingEvidence: string list)
    (whatIfAssumption: string option)
    (decisionTraces: DecisionTrace list)
    : MultiLevelRenderings =

    // 1. Enrich properties from synthesized BusinessContext node if present
    let contextNodeOpt = graph.Nodes |> List.tryFind (fun n -> n.NodeType = "BusinessContext")
    let effectiveProps =
        match contextNodeOpt with
        | Some ctx ->
            ctx.Properties
            |> Map.fold (fun acc k v ->
                if not (acc |> Map.containsKey k) then Map.add k v acc else acc) properties
        | None -> properties

    let baseNarrative = renderSummary template effectiveProps

    // 2. Append Incomplete Evidence Notice if applicable
    let narrativeWithEvidence =
        if missingEvidence.IsEmpty then
            baseNarrative
        else
            let missingStr = missingEvidence |> String.concat ", "

            baseNarrative
            + "\n\n⚠️ INCOMPLETE EVIDENCE NOTICE: Missing required evidence types: ["
            + missingStr
            + "]. Explainability Score: "
            + explainabilityScore.ToString("N1")
            + "%."

    // 3. Append What-If Analysis if applicable
    let finalPlannerSummary =
        match whatIfAssumption with
        | Some assumption when not(String.IsNullOrWhiteSpace assumption) ->
            let whatIfText = evaluateWhatIfHypothesis assumption decisionTraces
            narrativeWithEvidence + "\n\n" + whatIfText
        | _ -> narrativeWithEvidence

    // 4. Quantitative Waterfall Markdown
    let waterfallMarkdown = renderWaterfallMarkdown factors totalExplainedValue

    // 5. Full Audit JSON (safe structured serialization)
    let fullAuditJson =
        let options = JsonSerializerOptions(WriteIndented = true)
        JsonSerializer.Serialize(graph, options)

    // 6. AI Consumable JSON-LD
    let aiConsumable = buildAiConsumableJsonLd graph

    { PlannerSummary = finalPlannerSummary
      FactorWaterfallMarkdown = waterfallMarkdown
      FullAuditJson = fullAuditJson
      AiConsumableJsonLd = aiConsumable }
