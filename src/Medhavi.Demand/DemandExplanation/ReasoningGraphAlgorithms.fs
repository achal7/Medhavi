module Medhavi.Demand.ReasoningGraphAlgorithms

open Medhavi.Demand.DemandExplanation.Model
open Medhavi.SharedKernel.Contracts.DecisionTrace

// ------------------------------------------------------------------
// Typed business relationships – the controlled vocabulary for edges
// ------------------------------------------------------------------
type BusinessRelationship =
    | ContributedToRevision
    | IncludedInCalculation
    | PublishedAs
    | ApprovedBy
    | ConstrainedBy
    | DecisionFollowedBy
    | BaseState
    | ForecastInput
    | Unknown

/// Safely parse a string into a BusinessRelationship, defaulting to Unknown.
let private parseRelationship (s: string) =
    match s with
    | "ContributedToRevision" -> ContributedToRevision
    | "IncludedInCalculation" -> IncludedInCalculation
    | "PublishedAs"           -> PublishedAs
    | "ApprovedBy"            -> ApprovedBy
    | "ConstrainedBy"         -> ConstrainedBy
    | "DecisionFollowedBy"    -> DecisionFollowedBy
    | "BaseState"             -> BaseState
    | "ForecastInput"         -> ForecastInput
    | _                       -> Unknown

/// Convert a BusinessRelationship to its string representation for edges.
let private relationshipToString (rel: BusinessRelationship) = rel.ToString()

// Helper to extract a property from a node, returning "" if missing.
let private getProp key (node: ExplanationNode) =
    node.Properties |> Map.tryFind key |> Option.defaultValue ""

// ------------------------------------------------------------------
// 1. Build trace nodes from decision traces
// ------------------------------------------------------------------
let private buildTraceNodes (traces: DecisionTrace list) : ExplanationNode list =
    traces |> List.mapi (fun i trace ->
        let label =
            if System.String.IsNullOrEmpty trace.DecisionId then "Decision: EDP Revised"
            else $"Decision: {trace.DecisionId}"
        let rulesStr =
            trace.RulesEvaluated
            |> List.map (fun (r, v) -> $"{r} v{v}: {Medhavi.SharedKernel.ArsIdentifiers.Demand.Rules.describe r}")
            |> String.concat ", "
        let policyStr =
            match trace.PolicyId, trace.PolicyVersion with
            | Some pId, Some pVer -> $"{pId} v{pVer}"
            | Some pId, None     -> pId
            | _                  -> ""
        let evidenceStr = String.concat ", " trace.Rationale.Evidence
        let semanticStr = String.concat ", " trace.SemanticObjectIds
        let props =
            [ "DecisionId", trace.DecisionId
              "CapabilityId", trace.CapabilityId
              "Rationale", trace.Rationale.Summary
              "RulesEvaluated", rulesStr
              "Policy", policyStr
              "Evidence", evidenceStr
              "SemanticObjectIds", semanticStr ]
            |> List.filter (fun (_, v) -> not (System.String.IsNullOrEmpty v))
            |> Map.ofList
        { NodeId   = $"node-trace-{i}"
          NodeType = "DecisionTrace"
          Label    = label
          Properties = props }
    )

// ------------------------------------------------------------------
// 2. Build source artifact nodes (with all their properties)
// ------------------------------------------------------------------
let private buildSourceNodes (sourceRefs: ExplanationSourceArtifactRef list) : ExplanationNode list =
    sourceRefs |> List.mapi (fun j ref ->
        let props =
            [ "ArtifactType", ref.ArtifactType
              "ArtifactId",   ref.ArtifactId
              "Version",      string ref.Version ]
            @ (ref.Properties |> Map.toList)
            |> Map.ofList
        { NodeId   = $"node-source-{j}"
          NodeType = "SourceArtifact"
          Label    = $"{ref.ArtifactType}: {ref.ArtifactId}"
          Properties = props }
    )

// ------------------------------------------------------------------
// 3. Trace-to-trace edges – all are DecisionFollowedBy
// ------------------------------------------------------------------
let private buildTraceToTraceEdges (traces: DecisionTrace list) : ExplanationEdge list =
    traces
    |> List.pairwise
    |> List.mapi (fun i _ ->
        { EdgeId       = $"edge-trace-{i}"
          SourceNode   = $"node-trace-{i}"
          TargetNode   = $"node-trace-{i+1}"
          Relationship = relationshipToString BusinessRelationship.DecisionFollowedBy }
    )

// ------------------------------------------------------------------
// 4. Source-to-trace edges – assign typed relationships
// ------------------------------------------------------------------
let private buildSourceToTraceEdges
    (sourceRefs: ExplanationSourceArtifactRef list)
    (traces: DecisionTrace list)
    : ExplanationEdge list =

    sourceRefs |> List.mapi (fun j ref ->
        let targetNodeId, relationship =
            match ref.ArtifactType with
            | "DemandObservation" ->
                // link to the first revision step (empty DecisionId)
                let idx = traces |> List.tryFindIndex (fun t -> System.String.IsNullOrEmpty t.DecisionId)
                let nodeId = match idx with Some i -> $"node-trace-{i}" | None -> "node-trace-0"
                nodeId, BusinessRelationship.ContributedToRevision
            | "ForecastLine" | "ForecastPublication" ->
                // link to the calculation step (DE-D-011)
                let idx = traces |> List.tryFindIndex (fun t -> t.DecisionId.Contains("DE-D-011"))
                let nodeId = match idx with Some i -> $"node-trace-{i}" | None -> "node-trace-0"
                nodeId, BusinessRelationship.IncludedInCalculation
            | _ ->
                // For the target artifact itself, or unknown types, use BaseState
                "node-trace-0", BusinessRelationship.BaseState

        { EdgeId       = $"edge-source-{j}"
          SourceNode   = $"node-source-{j}"
          TargetNode   = targetNodeId
          Relationship = relationshipToString relationship }
    )

// ------------------------------------------------------------------
// 5. Link from the last decision back to the target artifact
// ------------------------------------------------------------------
let private buildTargetLinkEdge
    (sourceRefs: ExplanationSourceArtifactRef list)
    (traceCount: int)
    : ExplanationEdge list =

    let targetRef = sourceRefs |> List.tryFind (fun r -> r.ArtifactType = "EnterpriseDemandPicture" || r.ArtifactType = "ForecastPublication")
    match targetRef, traceCount with
    | Some _, count when count > 0 ->
        let lastTraceIdx = count - 1
        let targetNodeId =
            sourceRefs
            |> List.findIndex (fun r -> r = targetRef.Value)   // safe because we already found it
            |> fun idx -> $"node-source-{idx}"
        [ { EdgeId       = "edge-target-link"
            SourceNode   = $"node-trace-{lastTraceIdx}"
            TargetNode   = targetNodeId
            Relationship = relationshipToString BusinessRelationship.PublishedAs } ]
    | _ -> []

// ------------------------------------------------------------------
// 6. Business Context Node – deterministic semantic summary
// ------------------------------------------------------------------
let private buildBusinessContextNode (graph: StructuredReasoningGraph) : ExplanationNode option =
    let traceNodes  = graph.Nodes |> List.filter (fun n -> n.NodeType = "DecisionTrace")
    let sourceNodes = graph.Nodes |> List.filter (fun n -> n.NodeType = "SourceArtifact")

    // Identify the target artifact (the one being explained)
    let targetNode =
        sourceNodes
        |> List.tryFind (fun n ->
            let at = getProp "ArtifactType" n
            at = "EnterpriseDemandPicture" || at = "ForecastPublication"
        )
        |> Option.orElse (sourceNodes |> List.tryHead)

    // Build causal paths from edges
    let paths =
        graph.Edges
        |> List.choose (fun e ->
            let sourceNode = sourceNodes |> List.tryFind (fun n -> n.NodeId = e.SourceNode)
            let rel = parseRelationship e.Relationship
            match sourceNode with
            | Some sn when rel <> Unknown -> Some (sn, rel, e.TargetNode)
            | _ -> None
        )

    // Exclude the target artifact from contributing paths
    let targetNodeId = targetNode |> Option.map (fun n -> n.NodeId)
    let contributingPaths =
        paths
        |> List.filter (fun (sn, _, _) -> Some sn.NodeId <> targetNodeId)

    // Enriched, domain-agnostic cause
    let targetVersion = targetNode |> Option.map (fun n -> getProp "Version" n) |> Option.defaultValue "1"
    let cause =
        let grouped =
            contributingPaths
            |> List.groupBy (fun (sn, _, _) -> getProp "ArtifactType" sn)
            |> List.filter (fun (at, _) -> Some at <> (targetNode |> Option.map (fun n -> getProp "ArtifactType" n)))
        let clauses =
            grouped |> List.map (fun (artType, items) ->
                let action =
                    match items |> List.tryHead |> Option.map (fun (_, rel, _) -> rel) with
                    | Some ContributedToRevision -> "incorporated"
                    | Some IncludedInCalculation -> "included in the calculation"
                    | Some PublishedAs           -> "published"
                    | Some ConstrainedBy         -> "constrained by"
                    | _ -> "applied"
                let count = items.Length
                $"{count} {artType}(s) {action}"
            )
        let targetArtifactType = targetNode |> Option.map (fun n -> getProp "ArtifactType" n) |> Option.defaultValue "artifact"
        match clauses with
        | [] ->
            if targetVersion = "1" && traceNodes.IsEmpty then
                "Initial state established."
            else
                "A revision occurred."
        | _  -> $"""{String.concat ", " clauses} into the {targetArtifactType}."""

    // Contributions string
    let contributions =
        contributingPaths
        |> List.map (fun (sn, _, _) ->
            let id = getProp "ArtifactId" sn
            let extraProps =
                sn.Properties
                |> Map.remove "ArtifactType"
                |> Map.remove "ArtifactId"
                |> Map.remove "Version"
                |> Map.toList
                |> List.map (fun (k, v) -> $"{k}: {v}")
                |> String.concat ", "
            if extraProps = "" then id else $"{id} ({extraProps})"
        )
        |> String.concat "; "

    // Derived facts – total quantity from contributing source properties
    let totalQuantity =
        contributingPaths
        |> List.choose (fun (sn, _, _) ->
            sn.Properties |> Map.tryFind "Quantity")
        |> List.choose (fun s ->
            match System.Decimal.TryParse s with
            | true, v -> Some v
            | _ -> None)
        |> List.sum
    let derivedFacts =
        if totalQuantity > 0m then [ $"Total quantity changed by {totalQuantity} units." ]
        else []

    // Rules & policies – deduplicated
    let rulesAndPolicies =
        traceNodes
        |> List.collect (fun n ->
            let ruleDesc = n.Properties |> Map.tryFind "RulesEvaluated" |> Option.defaultValue ""
            let policy   = n.Properties |> Map.tryFind "Policy"         |> Option.defaultValue ""
            [ if ruleDesc <> "" then ruleDesc
              if policy   <> "" then policy ]
        )
        |> List.distinct
        |> String.concat "; "

    let unknowns =
        if targetVersion <> "1" && traceNodes.IsEmpty then
            "The root cause that initiated the revision is not recorded."
        else
            ""

    let props =
        [ "Cause",           cause
          "Contributions",   contributions
          "DerivedFacts",    String.concat "; " derivedFacts
          "RulesAndPolicies", rulesAndPolicies
          "Unknowns",        unknowns ]
        |> Map.ofList

    Some { NodeId   = "node-business-context"
           NodeType = "BusinessContext"
           Label    = "Business Context"
           Properties = props }

// ------------------------------------------------------------------
// 7. Main function – assemble the complete reasoning graph
// ------------------------------------------------------------------
let buildReasoningGraph
    (decisionTraces: DecisionTrace list)
    (sourceRefs: ExplanationSourceArtifactRef list)
    : StructuredReasoningGraph =

    let traceNodes  = buildTraceNodes decisionTraces
    let sourceNodes = buildSourceNodes sourceRefs
    let traceEdges  = buildTraceToTraceEdges decisionTraces
    let sourceEdges = buildSourceToTraceEdges sourceRefs decisionTraces
    let targetLink  = buildTargetLinkEdge sourceRefs (List.length decisionTraces)

    let graph =
        { Nodes = traceNodes @ sourceNodes
          Edges = traceEdges @ sourceEdges @ targetLink }

    // Attach the business context node (deterministic summary)
    match buildBusinessContextNode graph with
    | Some ctxNode -> { graph with Nodes = graph.Nodes @ [ctxNode] }
    | None         -> graph

(*
// Build a summary node if source refs contain quantitative data
let explanationContextNode decisionTraces traceNodes graph =
    // 1. Find the “causal” relationship: source artifacts linked to a revision/action step
    let linkedSourceNodes =
        graph.Edges
        |> List.filter (fun e -> e.Relationship = "RevisedInto" || e.Relationship = "ContributedTo")  // ← generic edge types
        |> List.choose (fun e ->
            graph.Nodes |> List.tryFind (fun n -> n.NodeId = e.SourceNode && n.NodeType = "SourceArtifact"))

    // 2. Extract a generic cause from the decision that has empty DecisionId (i.e., a “revision” step)
    let cause =
        traceNodes
        |> List.tryFind (fun n -> n.Properties |> Map.tryFind "DecisionId" = Some "")
        |> Option.map (fun n -> n.Properties |> Map.tryFind "Rationale" |> Option.defaultValue "")
        |> Option.defaultValue "Recorded event triggered a change."

    // 3. Summarise contributing artifacts with all their measurable properties
    let contributions =
        linkedSourceNodes
        |> List.map (fun n ->
            let id = n.Properties |> Map.tryFind "ArtifactId" |> Option.defaultValue ""
            let props =
                n.Properties
                |> Map.remove "ArtifactType" |> Map.remove "ArtifactId" |> Map.remove "Version"
                |> Map.toList
                |> List.map (fun (k, v) -> $"{k}: {v}")
                |> String.concat ", "
            if props = "" then id else $"{id} ({props})")
        |> String.concat "; "

    // 4. Compute a result if quantities are present
    let totalQuantity =
        linkedSourceNodes
        |> List.choose (fun n -> n.Properties |> Map.tryFind "Quantity")
        |> List.choose (fun s -> match System.Decimal.TryParse s with true, v -> Some v | _ -> None)
        |> List.sum
    let result = if totalQuantity > 0m then Some $"Total quantity changed by {totalQuantity} units." else None

    // 5. Build the node
    let props =
            [   "Cause", cause
                "Contributions", contributions ]
            @ (result |> Option.map (fun r -> "Result", r) |> Option.toList)
            |> Map.ofList

    Some {  NodeId = "explanation-context"
            NodeType = "ExplanationContext"
            Label = "Business Context"
            Properties = props }

/// BA‑D‑009 – Builds a Structured Reasoning Graph from decision traces and source artifact references.
let buildReasoningGraph
    (decisionTraces: DecisionTrace list)
    (sourceRefs: ExplanationSourceArtifactRef list)
    : StructuredReasoningGraph =

    // 1. Map Decision Traces to nodes
    let traceNodes =
        decisionTraces
        |> List.mapi(fun i trace ->
            let label =
                if System.String.IsNullOrEmpty trace.DecisionId then
                    "Decision: EDP Revised"
                else
                    $"Decision: {trace.DecisionId}"

            let rulesStr =
                trace.RulesEvaluated
                |> List.map(fun (r, v) -> $"{r} v{v}: {Medhavi.SharedKernel.ArsIdentifiers.Demand.Rules.describe r}")
                |> String.concat ", "

            let policyStr =
                match trace.PolicyId, trace.PolicyVersion with
                | Some pId, Some pVer -> $"{pId} v{pVer}"
                | Some pId, None -> pId
                | _ -> ""

            let evidenceStr = String.concat ", " trace.Rationale.Evidence
            let semanticStr = String.concat ", " trace.SemanticObjectIds

            let props =
                [ "DecisionId", trace.DecisionId
                  "CapabilityId", trace.CapabilityId
                  "Rationale", trace.Rationale.Summary
                  "RulesEvaluated", rulesStr
                  "Policy", policyStr
                  "Evidence", evidenceStr
                  "SemanticObjectIds", semanticStr ]
                |> List.filter(fun (_, v) -> not(System.String.IsNullOrEmpty v))
                |> Map.ofList

            { NodeId = $"node-trace-{i}"
              NodeType = "DecisionTrace"
              Label = label
              Properties = props })

    // 2. Map Source Artifacts to nodes
    let sourceNodes =
        sourceRefs
        |> List.mapi(fun j ref ->
            { NodeId = $"node-source-{j}"
              NodeType = "SourceArtifact"
              Label = $"{ref.ArtifactType}: {ref.ArtifactId}"
              Properties =
                [ "ArtifactType", ref.ArtifactType
                  "ArtifactId", ref.ArtifactId
                  "Version", string ref.Version ]
                @ (ref.Properties |> Map.toList)
                |> Map.ofList })

    // 3. Create trace-to-trace edges (pairwise influence chain)
    let traceEdges =
        decisionTraces
        |> List.pairwise
        |> List.mapi(fun i _ ->
            { EdgeId = $"edge-trace-{i}"
              SourceNode = $"node-trace-{i}"
              TargetNode = $"node-trace-{i + 1}"
              Relationship = "Influenced" })

    // 4. Create source-to-trace edges
    let sourceEdges =
        sourceRefs
        |> List.mapi(fun j ref ->
            let targetNodeId =
                match ref.ArtifactType with
                | "DemandObservation" ->
                    let revisionIdxOpt =
                        decisionTraces
                        |> List.tryFindIndex(fun t ->
                            System.String.IsNullOrEmpty t.DecisionId
                            || t.Rationale.Summary.Contains("revised")
                            || t.Rationale.Summary.Contains("observation"))

                    match revisionIdxOpt with
                    | Some idx -> $"node-trace-{idx}"
                    | None -> "node-trace-0"
                | "ForecastPublication"
                | "ForecastLine" ->
                    let calcIdxOpt =
                        decisionTraces
                        |> List.tryFindIndex(fun t ->
                            t.DecisionId.Contains("DE-D-011")
                            || t.Rationale.Summary.Contains("calculated")
                            || t.Rationale.Summary.Contains("Forecast"))

                    match calcIdxOpt with
                    | Some idx -> $"node-trace-{idx}"
                    | None -> "node-trace-0"
                | _ -> "node-trace-0"

            let relationship =
                match ref.ArtifactType with
                | "DemandObservation" -> "RevisedInto"
                | "ForecastPublication"
                | "ForecastLine" -> "ForecastInput"
                | "EnterpriseDemandPicture" -> "BaseState"
                | _ -> "Input"

            { EdgeId = $"edge-source-{j}"
              SourceNode = $"node-source-{j}"
              TargetNode = targetNodeId
              Relationship = relationship })



    // 5. Connect last decision to the target aggregate itself if present
    let targetLinkEdge =
        if not sourceRefs.IsEmpty && not decisionTraces.IsEmpty then
            let targetIdxOpt =
                sourceRefs
                |> List.tryFindIndex(fun r ->
                    r.ArtifactType = "EnterpriseDemandPicture" || r.ArtifactType = "ForecastPublication")

            match targetIdxOpt with
            | Some targetIdx ->
                let lastTraceIdx = decisionTraces.Length - 1

                [ { EdgeId = "edge-target-link"
                    SourceNode = $"node-trace-{lastTraceIdx}"
                    TargetNode = $"node-source-{targetIdx}"
                    Relationship = "PublishedAs" } ]
            | None -> []
        else
            []

    { Nodes = traceNodes @ sourceNodes //@ (explanationContextNode |> Option.toList)
      Edges = traceEdges @ sourceEdges @ targetLinkEdge }
    |> (fun graph ->
            let node = explanationContextNode decisionTraces sourceRefs graph
            { graph with Nodes = graph.Nodes @ (node |> Option.toList) })
*)
