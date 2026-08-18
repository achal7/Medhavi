/// BA-D-012 — Advanced Deterministic Structured Reasoning Graph Construction Engine
/// Builds canonical 2-hop graph vertices, causal decision chains, PROV-O provenance, explicit policy nodes, and synthesized BusinessContext
module Medhavi.Demand.ExplainDemand.DemandExplanation.ReasoningGraphAlgorithms

open System
open Medhavi.Foundation.Contracts
open Model
open TemplateCatalog

// =============================================================================
// BUSINESS RELATIONSHIP ONTOLOGY & VOCABULARY
// =============================================================================

type BusinessRelationship =
    | ContributedToRevision
    | IncludedInCalculation
    | PublishedAs
    | ApprovedBy
    | ConstrainedBy
    | DecisionFollowedBy
    | InfluencedBy
    | DeterminedBy
    | GovernedBy
    | EvaluatedBy
    | TriggeredBy
    | ComparedAgainst
    | CorroboratedBy
    | DerivedFrom
    | AdjustedBy
    | Summarizes
    | BaseState
    | Unknown

    member this.AsString =
        match this with
        | ContributedToRevision -> "ContributedToRevision"
        | IncludedInCalculation -> "IncludedInCalculation"
        | PublishedAs -> "PublishedAs"
        | ApprovedBy -> "ApprovedBy"
        | ConstrainedBy -> "ConstrainedBy"
        | DecisionFollowedBy -> "DecisionFollowedBy"
        | InfluencedBy -> "InfluencedBy"
        | DeterminedBy -> "DeterminedBy"
        | GovernedBy -> "GovernedBy"
        | EvaluatedBy -> "EvaluatedBy"
        | TriggeredBy -> "TriggeredBy"
        | ComparedAgainst -> "ComparedAgainst"
        | CorroboratedBy -> "CorroboratedBy"
        | DerivedFrom -> "DerivedFrom"
        | AdjustedBy -> "AdjustedBy"
        | Summarizes -> "Summarizes"
        | BaseState -> "BaseState"
        | Unknown -> "Unknown"

    static member FromString(s: string) : BusinessRelationship =
        match s with
        | "ContributedToRevision" -> ContributedToRevision
        | "IncludedInCalculation" -> IncludedInCalculation
        | "PublishedAs" -> PublishedAs
        | "ApprovedBy" -> ApprovedBy
        | "ConstrainedBy" -> ConstrainedBy
        | "DecisionFollowedBy" -> DecisionFollowedBy
        | "InfluencedBy" -> InfluencedBy
        | "DeterminedBy" -> DeterminedBy
        | "GovernedBy" -> GovernedBy
        | "EvaluatedBy" -> EvaluatedBy
        | "TriggeredBy" -> TriggeredBy
        | "ComparedAgainst" -> ComparedAgainst
        | "CorroboratedBy" -> CorroboratedBy
        | "DerivedFrom" -> DerivedFrom
        | "AdjustedBy" -> AdjustedBy
        | "Summarizes" -> Summarizes
        | "BaseState" -> BaseState
        | _ -> Unknown

// ---------- Deterministic Identifier Builders ----------

let private makeNodeId (artifactType: string) (artifactId: string) (version: int) (nodeType: string) (key: string) =
    $"{artifactType.ToLowerInvariant()}-{artifactId.ToLowerInvariant()}-v{version}-{nodeType.ToLowerInvariant()}-{key.ToLowerInvariant()}"

let private makeEdgeId (sourceNodeId: string) (targetNodeId: string) (rel: string) =
    $"edge-{sourceNodeId}-to-{targetNodeId}-{rel.ToLowerInvariant()}"

let private getProp key (node: ExplanationNode) =
    node.Properties |> Map.tryFind key |> Option.defaultValue ""

// ---------- Node Builders ----------

/// Builds explicit Policy nodes for governing policies referenced in decision traces
let buildPolicyNodes
    (artifactType: string)
    (artifactId: string)
    (version: int)
    (traces: DecisionTrace list)
    : ExplanationNode list =

    traces
    |> List.choose (fun t ->
        match t.PolicyId with
        | Some pId when not (String.IsNullOrWhiteSpace pId) && pId <> "None" ->
            let pVer = t.PolicyVersion |> Option.map string |> Option.defaultValue "1"
            Some (pId, pVer)
        | _ -> None)
    |> List.distinct
    |> List.map (fun (policyId, policyVersion) ->
        let nodeId = makeNodeId artifactType artifactId version "policy" policyId
        let props =
            [ "PolicyId", policyId
              "PolicyVersion", policyVersion ]
            |> Map.ofList

        { NodeId = nodeId
          NodeType = "Policy"
          Label = $"Policy: {policyId} (v{policyVersion})"
          Properties = props
          IsHypothetical = false })

/// Builds DecisionTrace nodes and child RuleEvaluation nodes with full evidence provenance
let buildDecisionNodes
    (artifactType: string)
    (artifactId: string)
    (version: int)
    (traces: DecisionTrace list)
    : ExplanationNode list * ExplanationEdge list =

    let nodes = ResizeArray<ExplanationNode>()
    let edges = ResizeArray<ExplanationEdge>()

    traces
    |> List.iteri (fun traceIdx (trace: DecisionTrace) ->
        let decisionNodeId = makeNodeId artifactType artifactId version "decision" $"{trace.DecisionId}-{traceIdx}"

        let rulesSummary =
            trace.RulesEvaluated
            |> List.map (fun (r: RuleEvaluation) ->
                let status = if r.Passed then "Passed" else "Failed"
                $"{r.RuleId}: {status}")
            |> String.concat ", "

        let policyStr =
            match trace.PolicyId, trace.PolicyVersion with
            | Some pId, Some pVer -> $"{pId}:v{pVer}"
            | Some pId, None -> pId
            | _ -> "None"

        let decisionProps =
            [ "DecisionId", trace.DecisionId
              "CapabilityId", trace.CapabilityId
              "RationaleSummary", trace.Rationale.Summary
              "RationaleEvidence", trace.Rationale.Evidence |> String.concat "; "
              "PolicyId", policyStr
              "RulesEvaluatedSummary", rulesSummary
              "SemanticObjectIds", String.concat ", " trace.SemanticObjectIds ]
            |> Map.ofList

        nodes.Add
            { NodeId = decisionNodeId
              NodeType = "DecisionTrace"
              Label = $"Decision: {trace.DecisionId}"
              Properties = decisionProps
              IsHypothetical = false }

        // 1. Build child RuleEvaluation nodes linked to parent decision (EvaluatedBy)
        trace.RulesEvaluated
        |> List.iteri (fun ruleIdx (ruleEval: RuleEvaluation) ->
            let ruleNodeId = makeNodeId artifactType artifactId version "rule" $"{ruleEval.RuleId}-{traceIdx}-{ruleIdx}"

            let ruleProps =
                [ "RuleId", ruleEval.RuleId
                  "Passed", string ruleEval.Passed
                  "Evidence", ruleEval.Evidence |> String.concat "; "
                  "ReasonCode", ruleEval.ReasonCode |> Option.defaultValue "None" ]
                |> Map.ofList

            nodes.Add
                { NodeId = ruleNodeId
                  NodeType = "RuleEvaluation"
                  Label = $"Rule: {ruleEval.RuleId}"
                  Properties = ruleProps
                  IsHypothetical = false }

            let edgeId = makeEdgeId decisionNodeId ruleNodeId BusinessRelationship.EvaluatedBy.AsString
            edges.Add
                { EdgeId = edgeId
                  SourceNode = decisionNodeId
                  TargetNode = ruleNodeId
                  Relationship = BusinessRelationship.EvaluatedBy.AsString
                  IsHypothetical = false })

        // 2. Link governing Policy Node to Decision Trace (GovernedBy)
        match trace.PolicyId with
        | Some pId when not (String.IsNullOrWhiteSpace pId) && pId <> "None" ->
            let policyNodeId = makeNodeId artifactType artifactId version "policy" pId
            let edgeId = makeEdgeId policyNodeId decisionNodeId BusinessRelationship.GovernedBy.AsString
            edges.Add
                { EdgeId = edgeId
                  SourceNode = policyNodeId
                  TargetNode = decisionNodeId
                  Relationship = BusinessRelationship.GovernedBy.AsString
                  IsHypothetical = false }
        | _ -> ())

    nodes |> Seq.toList, edges |> Seq.toList

/// Builds SourceArtifact nodes from preserved historical evidence references
let buildSourceArtifactNodes
    (artifactType: string)
    (artifactId: string)
    (version: int)
    (sourceRefs: ExplanationSourceArtifactRef list)
    : ExplanationNode list =

    sourceRefs
    |> List.mapi (fun idx ref ->
        let nodeId = makeNodeId artifactType artifactId version "source" $"{ref.ArtifactType}-{ref.ArtifactId}-{idx}"

        let baseProps =
            [ "ArtifactType", ref.ArtifactType
              "ArtifactId", ref.ArtifactId
              "Version", string ref.Version ]
            |> Map.ofList

        let allProps =
            ref.Properties
            |> Map.fold (fun acc k v -> Map.add k v acc) baseProps

        { NodeId = nodeId
          NodeType = "SourceArtifact"
          Label = $"{ref.ArtifactType}: {ref.ArtifactId} (v{ref.Version})"
          Properties = allProps
          IsHypothetical = false })

/// Builds FactorAttribution nodes for quantitative waterfall drivers
let buildFactorNodes
    (artifactType: string)
    (artifactId: string)
    (version: int)
    (factors: FactorContribution list)
    : ExplanationNode list =

    factors
    |> List.mapi (fun idx f ->
        let nodeId = makeNodeId artifactType artifactId version "factor" $"{f.FactorName}-{idx}"

        let props =
            [ "FactorName", f.FactorName
              "BaseValue", f.BaseValue.ToString("N2")
              "ImpactValue", f.ImpactValue.ToString("N2")
              "PercentageContribution", f.PercentageContribution.ToString("N1") + "%"
              "Direction", f.Direction.AsString
              "Confidence", f.Confidence.ToString("N1") + "%" ]
            |> Map.ofList

        { NodeId = nodeId
          NodeType = "FactorAttribution"
          Label = $"Driver: {f.FactorName} ({f.ImpactValue:N2})"
          Properties = props
          IsHypothetical = false })

// ---------- Causal Edge Chaining Builders ----------

/// Links consecutive decision traces into a temporal chronological execution chain (DecisionFollowedBy)
let buildTraceToTraceEdges (decisionNodes: ExplanationNode list) : ExplanationEdge list =
    decisionNodes
    |> List.filter (fun n -> n.NodeType = "DecisionTrace")
    |> List.pairwise
    |> List.map (fun (prevNode, nextNode) ->
        let edgeId = makeEdgeId prevNode.NodeId nextNode.NodeId BusinessRelationship.DecisionFollowedBy.AsString
        { EdgeId = edgeId
          SourceNode = prevNode.NodeId
          TargetNode = nextNode.NodeId
          Relationship = BusinessRelationship.DecisionFollowedBy.AsString
          IsHypothetical = false })

// ---------- Synthesized BusinessContext Node Generator ----------

/// Synthesizes high-level business context by traversing the entire constructed reasoning graph
let buildBusinessContextNode
    (artifactType: string)
    (artifactId: string)
    (version: int)
    (nodes: ExplanationNode list)
    (edges: ExplanationEdge list)
    (targetNodeId: string)
    : ExplanationNode =

    let sourceNodes = nodes |> List.filter (fun n -> n.NodeType = "SourceArtifact")
    let traceNodes = nodes |> List.filter (fun n -> n.NodeType = "DecisionTrace")
    let factorNodes = nodes |> List.filter (fun n -> n.NodeType = "FactorAttribution")
    let policyNodes = nodes |> List.filter (fun n -> n.NodeType = "Policy")

    // 1. Group source contributing paths by artifact type to generate natural language cause clauses
    let clauses =
        sourceNodes
        |> List.groupBy (fun sn -> getProp "ArtifactType" sn)
        |> List.map (fun (artType, items) ->
            let count = items.Length
            $"{count} {artType}(s) incorporated")

    let cause =
        if clauses.IsEmpty && traceNodes.IsEmpty then
            $"Initial {artifactType} state established."
        elif clauses.IsEmpty then
            $"Revised by {traceNodes.Length} governed decision step(s)."
        else
            (String.concat ", " clauses) + $" into the {artifactType}."

    // 2. Aggregate contributing evidence artifact IDs and metadata
    let contributions =
        sourceNodes
        |> List.map (fun sn ->
            let id = getProp "ArtifactId" sn
            let artType = getProp "ArtifactType" sn
            $"{artType}:{id}")
        |> String.concat "; "

    // 3. Aggregate rules and policy identifiers
    let rulesAndPolicies =
        let policiesStr =
            policyNodes
            |> List.map (fun pn -> getProp "PolicyId" pn)
            |> String.concat ", "

        let rulesStr =
            traceNodes
            |> List.choose (fun tn ->
                let r = getProp "RulesEvaluatedSummary" tn
                if String.IsNullOrWhiteSpace r then None else Some r)
            |> String.concat "; "

        [ if not (String.IsNullOrWhiteSpace policiesStr) then $"Governing Policies: [{policiesStr}]"
          if not (String.IsNullOrWhiteSpace rulesStr) then $"Rules: {rulesStr}" ]
        |> String.concat " | "

    // 4. Summarize quantitative factor attribution facts
    let factorFacts =
        factorNodes
        |> List.map (fun fn ->
            let name = getProp "FactorName" fn
            let impact = getProp "ImpactValue" fn
            $"{name}: {impact}")
        |> String.concat "; "

    // 5. Detect diagnostic unknowns or unrecorded historical steps
    let unknowns =
        if version > 1 && traceNodes.IsEmpty then
            "Revision occurred but historical decision trace is not attached."
        else
            "None"

    let props =
        [ "Cause", cause
          "Contributions", (if String.IsNullOrWhiteSpace contributions then "None" else contributions)
          "RulesAndPolicies", (if String.IsNullOrWhiteSpace rulesAndPolicies then "None" else rulesAndPolicies)
          "FactorFacts", (if String.IsNullOrWhiteSpace factorFacts then "None" else factorFacts)
          "Unknowns", unknowns ]
        |> Map.ofList

    let contextNodeId = makeNodeId artifactType artifactId version "context" "business"
    { NodeId = contextNodeId
      NodeType = "BusinessContext"
      Label = "Business Context & Governance Summary"
      Properties = props
      IsHypothetical = false }

// ---------- Full Graph Assembly (BA-D-012) ----------

/// Assembles the complete deterministic Structured Reasoning Graph with 2-hop provenance, Policy nodes, and BusinessContext
let buildReasoningGraph
    (artifactType: string)
    (artifactId: string)
    (version: int)
    (decisionTraces: DecisionTrace list)
    (sourceRefs: ExplanationSourceArtifactRef list)
    (factors: FactorContribution list)
    (template: ExplanationTemplate)
    : StructuredReasoningGraph =

    // 1. Root Target Node
    let targetNodeId = makeNodeId artifactType artifactId version "target" "root"
    let targetNode =
        { NodeId = targetNodeId
          NodeType = artifactType
          Label = $"{artifactType}: {artifactId} (v{version})"
          Properties =
            [ "ArtifactType", artifactType
              "ArtifactId", artifactId
              "Version", string version ]
            |> Map.ofList
          IsHypothetical = false }

    // 2. Build Policy, Decision, Source, and Factor Nodes
    let policyNodes = buildPolicyNodes artifactType artifactId version decisionTraces
    let decisionNodes, ruleEdgesAndPolicyEdges = buildDecisionNodes artifactType artifactId version decisionTraces
    let sourceNodes = buildSourceArtifactNodes artifactType artifactId version sourceRefs
    let factorNodes = buildFactorNodes artifactType artifactId version factors

    // 3. Chain Decision Traces together (DecisionFollowedBy)
    let traceToTraceEdges = buildTraceToTraceEdges decisionNodes

    // 4. Build Causal Edges from Source Evidence to Decision Trace (or Target Node)
    let connectingEdges = ResizeArray<ExplanationEdge>()

    let firstDecisionTraceOpt = decisionNodes |> List.tryFind (fun n -> n.NodeType = "DecisionTrace")
    let lastDecisionTraceOpt = decisionNodes |> List.filter (fun n -> n.NodeType = "DecisionTrace") |> List.tryLast

    sourceNodes
    |> List.iter (fun sn ->
        let srcType = sn.Properties |> Map.tryFind "ArtifactType" |> Option.defaultValue "Unknown"
        let rel =
            template.RelationshipMapping
            |> Map.tryFind (srcType, artifactType)
            |> Option.defaultValue BusinessRelationship.DerivedFrom.AsString

        // If decision traces exist, connect source evidence to the decision that consumed it (2-hop provenance)
        let destinationNodeId =
            match firstDecisionTraceOpt with
            | Some firstDecision -> firstDecision.NodeId
            | None -> targetNodeId

        let edgeId = makeEdgeId sn.NodeId destinationNodeId rel
        connectingEdges.Add
            { EdgeId = edgeId
              SourceNode = sn.NodeId
              TargetNode = destinationNodeId
              Relationship = rel
              IsHypothetical = false })

    // 5. Connect Last Decision Trace to Target Node (PublishedAs / DeterminedBy)
    match lastDecisionTraceOpt with
    | Some lastDecision ->
        let edgeId = makeEdgeId lastDecision.NodeId targetNodeId BusinessRelationship.PublishedAs.AsString
        connectingEdges.Add
            { EdgeId = edgeId
              SourceNode = lastDecision.NodeId
              TargetNode = targetNodeId
              Relationship = BusinessRelationship.PublishedAs.AsString
              IsHypothetical = false }
    | None -> ()

    // 6. Connect Factor Attribution Nodes to Target Node (InfluencedBy)
    factorNodes
    |> List.iter (fun fn ->
        let edgeId = makeEdgeId fn.NodeId targetNodeId BusinessRelationship.InfluencedBy.AsString
        connectingEdges.Add
            { EdgeId = edgeId
              SourceNode = fn.NodeId
              TargetNode = targetNodeId
              Relationship = BusinessRelationship.InfluencedBy.AsString
              IsHypothetical = false })

    let preliminaryNodes = targetNode :: (policyNodes @ decisionNodes @ sourceNodes @ factorNodes)
    let preliminaryEdges = ruleEdgesAndPolicyEdges @ traceToTraceEdges @ (connectingEdges |> Seq.toList)

    // 7. Synthesize the BusinessContext node from the complete preliminary graph
    let contextNode = buildBusinessContextNode artifactType artifactId version preliminaryNodes preliminaryEdges targetNodeId

    // Connect BusinessContext node to target node
    let contextEdge =
        { EdgeId = makeEdgeId contextNode.NodeId targetNodeId BusinessRelationship.Summarizes.AsString
          SourceNode = contextNode.NodeId
          TargetNode = targetNodeId
          Relationship = BusinessRelationship.Summarizes.AsString
          IsHypothetical = false }

    let allNodes =
        (contextNode :: preliminaryNodes)
        |> List.sortBy (fun n -> n.NodeId)

    let allEdges =
        (contextEdge :: preliminaryEdges)
        |> List.sortBy (fun e -> e.EdgeId)

    { Nodes = allNodes
      Edges = allEdges }
