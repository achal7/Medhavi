/// BA-S-090 — Supply Explanation Composition, 3-Factor Confidence Assessment & Multi-Level Renderings
module Medhavi.Supply.ExplainSupplyDecisions.SupplyExplanation.Algorithms

open System
open System.Text.Json
open Medhavi.Foundation.Contracts
open Model
open Policies
open TemplateCatalog
open TemplateRenderer

// =============================================================================
// DOMAIN KNOWLEDGE & EXPLAINABILITY PRINCIPLES (ARS §15.7.3, BA-S-090):
//
// 1. Multi-Level Renderings:
//    - Planner Narrative: Plain-English summary with business context.
//    - Waterfall Table: Quantitative driver decomposition (Additive Reconciliation).
//    - Full Audit JSON: Deterministic serialization of the full reasoning graph.
//    - AI-Consumable JSON-LD: W3C PROV-O semantic knowledge graph for AI Copilots.
//
// 2. 3-Factor Explanation Confidence (PO-S-130 & BA-S-090 §7.3):
//    - Inherited Evidence Confidence (Gamma_inh): Mean confidence across source evidence.
//    - Evidence Completeness (Kappa_comp): Present required evidence / Total required evidence.
//    - Evidence Consistency (Sigma_cons): Harmony across sources, penalized by contradiction.
//    - Composite Index: Weighted linear synthesis governed by PO-S-130.
//
// 3. Governed Rationale Blocks (BR-S-233):
//    - Explicit structured blocks mapping dimensions to causal rules and evidence.
// =============================================================================

/// Detects missing required evidence types and computes Evidence Completeness (Kappa_comp)
let evaluateCompleteness
    (template: ExplanationTemplate)
    (sourceRefs: ExplanationSourceArtifactRef list)
    : (decimal * string list) =

    let required = template.RequiredEvidenceTypes

    if required.IsEmpty then
        1.0m, []
    else
        let presentSet = sourceRefs |> List.map(fun r -> r.ArtifactType) |> Set.ofList
        let missing = required |> List.filter(fun r -> not(presentSet.Contains r))
        let presentCount = required.Length - missing.Length
        let score = (decimal presentCount) / (decimal required.Length)
        score, missing

/// Computes Inherited Evidence Confidence (Gamma_inh) across evidence sources
let computeInheritedConfidence (sourceRefs: ExplanationSourceArtifactRef list) : decimal =
    if sourceRefs.IsEmpty then
        1.0m
    else
        let confidences =
            sourceRefs
            |> List.choose(fun r ->
                r.Properties
                |> Map.tryFind "Confidence"
                |> Option.bind(fun s ->
                    match Decimal.TryParse s with
                    | true, v -> Some(Math.Clamp(v, 0.0m, 1.0m))
                    | false, _ -> None))

        if confidences.IsEmpty then
            1.0m
        else
            let sum = confidences |> List.sum
            sum / (decimal confidences.Length)

/// Computes Evidence Consistency (Sigma_cons) based on graph relationships
let computeEvidenceConsistency (graph: StructuredReasoningGraph) : decimal =
    let totalEdges = decimal graph.Edges.Length

    let contradictedCount =
        graph.Edges |> List.filter(fun e -> e.Relationship = "Contradicted") |> List.length |> decimal

    1.0m - (contradictedCount / (totalEdges + 1.0m))

/// Computes full 3-Factor Explanation Confidence Breakdown (BA-S-090 §7.3 & PO-S-130)
let evaluateConfidenceBreakdown
    (policy: ExplanationPolicy)
    (gammaInh: decimal)
    (kappaComp: decimal)
    (sigmaCons: decimal)
    : ConfidenceBreakdown =

    // Weighted composite index per PO-S-130 governed weights
    let composite =
        (policy.InheritedConfidenceWeight * gammaInh)
        + (policy.CompletenessWeight * kappaComp)
        + (policy.ConsistencyWeight * sigmaCons)

    let compositePct = composite * 100.0m
    let kappaPct = kappaComp * 100.0m

    let level =
        if compositePct >= policy.HighConfidenceThreshold && kappaPct >= 80.0m && sigmaCons >= 0.95m then
            ExplanationConfidenceLevel.High
        elif compositePct >= policy.MediumConfidenceThreshold && kappaPct >= 60.0m then
            ExplanationConfidenceLevel.Medium
        else
            ExplanationConfidenceLevel.Low

    {
        InheritedEvidenceConfidence = Math.Round(gammaInh * 100.0m, 2)
        EvidenceCompleteness = Math.Round(kappaComp * 100.0m, 2)
        EvidenceConsistency = Math.Round(sigmaCons * 100.0m, 2)
        CompositeIndex = Math.Round(compositePct, 2)
        ConfidenceLevel = level
    }

/// Builds the W3C PROV-O JSON-LD representation specifically for AI Agents and Graph-RAG
let buildAiConsumableJsonLd (graph: StructuredReasoningGraph) : string =
    let escape (s: string) = s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", "")

    let context =
        """{"@context": {"medhavi": "https://medhavi.com/ontology/supply#", "prov": "http://www.w3.org/ns/prov#", "artifactType": "@type", "relationship": "medhavi:relationship"}}"""

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

/// Generates governed Rationale Blocks mapping outcome dimensions to causal rules (BR-S-233)
let generateRationaleBlocks
    (artifactType: string)
    (artifactId: string)
    (decisionTraces: DecisionTrace list)
    (sourceRefs: ExplanationSourceArtifactRef list)
    : RationaleBlock list =

    if decisionTraces.IsEmpty then
        [
            {
                RationaleBlockId = $"rb-{artifactType.ToLowerInvariant()}-baseline"
                OutcomeDimension = "BaselineState"
                CausalRuleRef = "BR-S-286"
                InputEvidenceRef = sourceRefs |> List.map(fun r -> r.ArtifactId) |> String.concat ", "
                GovernedNarrative =
                    $"Authoritative {artifactType} state '{artifactId}' composed from historical enterprise facts."
            }
        ]
    else
        decisionTraces
        |> List.mapi(fun idx trace ->
            let ruleSummary = trace.RulesEvaluated |> List.map(fun r -> r.RuleId) |> String.concat ", "

            let evidenceSummary = trace.Rationale.Evidence |> String.concat "; "

            {
                RationaleBlockId = $"rb-{artifactType.ToLowerInvariant()}-{idx}"
                OutcomeDimension = trace.DecisionId
                CausalRuleRef =
                    if String.IsNullOrWhiteSpace ruleSummary then
                        "GovernedPrecedence"
                    else
                        ruleSummary
                InputEvidenceRef =
                    if String.IsNullOrWhiteSpace evidenceSummary then
                        "HistoricalPreservedEvidence"
                    else
                        evidenceSummary
                GovernedNarrative = trace.Rationale.Summary
            })

/// Composes the complete Multi-Level Renderings
let composeMultiLevelRenderings
    (graph: StructuredReasoningGraph)
    (template: ExplanationTemplate)
    (properties: Map<string, string>)
    (factors: FactorContribution list)
    (totalExplainedValue: decimal)
    (confidence: ConfidenceBreakdown)
    (missingEvidence: string list)
    (whatIfAssumption: string option)
    (decisionTraces: DecisionTrace list)
    : MultiLevelRenderings =

    // 1. Enrich properties from synthesized BusinessContext node if present
    let contextNodeOpt = graph.Nodes |> List.tryFind(fun n -> n.NodeType = "BusinessContext")

    let effectiveProps =
        match contextNodeOpt with
        | Some ctx ->
            ctx.Properties
            |> Map.fold (fun acc k v -> if not(acc |> Map.containsKey k) then Map.add k v acc else acc) properties
        | None -> properties

    let baseNarrative = renderSummary template effectiveProps

    // 2. Append Incomplete Evidence Notice if applicable
    let narrativeWithEvidence =
        if missingEvidence.IsEmpty then
            baseNarrative
        else
            let missingStr = missingEvidence |> String.concat ", "

            baseNarrative
            + "\n\n⚠️ INCOMPLETE EVIDENCE NOTICE: Missing template-required evidence types: ["
            + missingStr
            + $"]. Completeness Score: {confidence.EvidenceCompleteness:N1}%%. Overall Confidence: {confidence.ConfidenceLevel.AsString} ({confidence.CompositeIndex:N1}%%)."

    // 3. Append What-If Analysis if applicable
    let finalPlannerSummary =
        match whatIfAssumption with
        | Some assumption when not(String.IsNullOrWhiteSpace assumption) ->
            let whatIfText = evaluateWhatIfHypothesis assumption decisionTraces
            narrativeWithEvidence + "\n\n" + whatIfText
        | _ -> narrativeWithEvidence

    // 4. Quantitative Waterfall Markdown Table
    let waterfallMarkdown = renderWaterfallMarkdown factors totalExplainedValue

    // 5. Full Audit JSON (safe structured serialization)
    let fullAuditJson =
        let options = JsonSerializerOptions(WriteIndented = true)
        JsonSerializer.Serialize(graph, options)

    // 6. AI Consumable JSON-LD (W3C PROV-O standard)
    let aiConsumable = buildAiConsumableJsonLd graph

    {
        PlannerSummary = finalPlannerSummary
        FactorWaterfallMarkdown = waterfallMarkdown
        FullAuditJson = fullAuditJson
        AiConsumableJsonLd = aiConsumable
    }
