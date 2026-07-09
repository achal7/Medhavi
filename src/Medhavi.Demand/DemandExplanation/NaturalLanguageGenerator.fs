namespace Medhavi.Demand.DemandExplanation

open System.Threading.Tasks
open System.Net.Http
open System.Text.Json
open Medhavi.Demand.DemandExplanation.Model

type NaturalLanguageGenerator = StructuredReasoningGraph -> string -> string -> string -> Task<string>

[<CLIMutable>]
type OllamaResponse = { response: string }

[<CLIMutable>]
type OllamaRequest =
    { model: string
      prompt: string
      stream: bool
      temperature: float
      stop: string list }

module NaturalLanguageGenerator =
    /// Extract a property from a node, defaulting to an empty string.
    let private getProp key (node: ExplanationNode) = node.Properties |> Map.tryFind key |> Option.defaultValue ""

    /// Build a clean, factual prompt from the pre‑computed BusinessContext node.
    let buildPrompt (graph: StructuredReasoningGraph) (artifactType: string) (artifactId: string) (question: string) : string =
        // 1. Locate the BusinessContext node (produced deterministically by the graph builder).
        let ctxNode = graph.Nodes |> List.tryFind(fun n -> n.NodeType = "BusinessContext")

        // 2. Extract the business‑meaningful facts.
        let cause = ctxNode |> Option.map(getProp "Cause") |> Option.defaultValue ""
        let contributions = ctxNode |> Option.map(getProp "Contributions") |> Option.defaultValue ""
        let derivedFacts = ctxNode |> Option.map(getProp "DerivedFacts") |> Option.defaultValue ""
        let rulesAndPolicies = ctxNode |> Option.map(getProp "RulesAndPolicies") |> Option.defaultValue ""
        let unknowns = ctxNode |> Option.map(getProp "Unknowns") |> Option.defaultValue ""

        // 3. Also list the source artifacts (their identities and measurable properties)
        //    so the LLM can reference individual artifacts if needed.
        let sourceDescriptions =
            graph.Nodes
            |> List.filter(fun n -> n.NodeType = "SourceArtifact")
            |> List.map(fun n ->
                let artType = getProp "ArtifactType" n
                let artId = getProp "ArtifactId" n
                let ver = getProp "Version" n

                let extraProps =
                    n.Properties
                    |> Map.remove "ArtifactType"
                    |> Map.remove "ArtifactId"
                    |> Map.remove "Version"
                    |> Map.toList
                    |> List.map(fun (k, v) -> $"{k}: {v}")
                    |> String.concat ", "

                let extraStr = if extraProps = "" then "" else $", {extraProps}"
                $"- {artType} {artId} (Version {ver}{extraStr})")
            |> String.concat "\n"

        // 4. Assemble the prompt.  No business logic – just formatting.
        "You are a planning assistant. Answer the question using ONLY the facts provided below. Be concise and factual. Do not add any extra information or speculate.\n\n"
        + $"TARGET\n{artifactType} {artifactId}\n\n"
        + "SOURCES\n"
        + sourceDescriptions
        + "\n\n"
        + "CAUSE\n"
        + cause
        + "\n\n"
        + "CONTRIBUTIONS\n"
        + contributions
        + "\n\n"
        + "DERIVED FACTS\n"
        + derivedFacts
        + "\n\n"
        + "RULES & POLICIES\n"
        + rulesAndPolicies
        + "\n\n"
        + "UNKNOWNS\n"
        + unknowns
        + "\n\n"
        + $"Question: {question}\n\n"
        + "Answer:"

    /// In‑memory fake generator for deterministic testing.
    let fakeGenerator (fixedResult: string) : NaturalLanguageGenerator = fun _ _ _ _ -> Task.FromResult fixedResult

    /// Ollama HTTP generator with grounding parameters.
    let createOllamaGenerator (httpClient: HttpClient) (endpoint: string) (model: string) : NaturalLanguageGenerator =
        fun graph artifactType artifactId question ->
            task {
                try
                    let prompt = buildPrompt graph artifactType artifactId question

                    let requestPayload =
                        { model = model
                          prompt = prompt
                          stream = false
                          temperature = 0.0
                          stop = [ "\n"; "." ] }

                    let jsonPayload = JsonSerializer.Serialize requestPayload
                    use content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json")

                    let requestUrl = $"{endpoint.TrimEnd('/')}/api/generate"
                    let! response = httpClient.PostAsync(requestUrl, content)

                    if response.IsSuccessStatusCode then
                        let! responseString = response.Content.ReadAsStringAsync()
                        let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
                        let result = JsonSerializer.Deserialize<OllamaResponse>(responseString, options)
                        return result.response.Trim()
                    else
                        return
                            $"Explanation for {artifactType} '{artifactId}' with {graph.Nodes.Length} reasoning nodes."
                with ex ->
                    printfn $"LLM Error:\n %A{ex.StackTrace}"
                    return $"Explanation for {artifactType} '{artifactId}' with {graph.Nodes.Length} reasoning nodes."
            }

(*
type NaturalLanguageGenerator = StructuredReasoningGraph -> string -> string -> Task<string>

[<CLIMutable>]
type OllamaResponse = { response: string }

[<CLIMutable>]
type OllamaRequest =
    { model: string
      prompt: string
      stream: bool
      temperature: float
      stop: string list }

module NaturalLanguageGenerator =

    /// Compact text‑only prompt builder that forces factual grounding.
    let buildPrompt (graph: StructuredReasoningGraph) (artifactType: string) (artifactId: string) : string =

        // Build decision history from trace nodes, in order.
        // We assume trace nodes are already sorted by their creation order (index in the list).
        let traceNodes = graph.Nodes |> List.filter(fun n -> n.NodeType = "DecisionTrace")

        // Build a map from revision trace node IDs to the observations that link to them
        let revisionLinks =
            graph.Edges
            |> List.filter (fun e -> e.Relationship = "RevisedInto")
            |> List.groupBy (fun e -> e.TargetNode)
            |> List.map (fun (targetNode, edges) ->
                let observations =
                    edges
                    |> List.choose (fun e ->
                        graph.Nodes
                        |> List.tryFind (fun n -> n.NodeId = e.SourceNode && n.NodeType = "SourceArtifact")
                        |> Option.bind (fun n -> n.Properties |> Map.tryFind "ArtifactId"))
                targetNode, observations)
            |> Map.ofList

        let decisionLines =
            traceNodes
            |> List.mapi(fun i node ->
                let decisionId = node.Properties |> Map.tryFind "DecisionId" |> Option.defaultValue ""
                let policy = node.Properties |> Map.tryFind "Policy" |> Option.defaultValue ""
                let rules = node.Properties |> Map.tryFind "RulesEvaluated" |> Option.defaultValue ""
                let label = if decisionId = "" then "EDP Revised" else $"Decision {decisionId}"
                let policyStr = if policy = "" then "" else $", policy {policy}"
                let rulesStr = if rules = "" then "" else $", rules {rules}"
                // Annotate revision steps with linked observations
                let annotation =
                    match revisionLinks |> Map.tryFind node.NodeId with
                    | Some obs when not obs.IsEmpty -> $""" (linked to {String.concat ", " obs})"""
                    | _ -> ""
                $"{i + 1}. {label}{policyStr}{rulesStr}{annotation}")
            |> String.concat "\n"

        // Extract source artifact descriptions
        let sourceDescriptions =
            graph.Nodes
            |> List.filter(fun n -> n.NodeType = "SourceArtifact")
            |> List.map(fun n ->
                let artType = n.Properties |> Map.tryFind "ArtifactType" |> Option.defaultValue ""
                let artId = n.Properties |> Map.tryFind "ArtifactId" |> Option.defaultValue ""
                let ver = n.Properties |> Map.tryFind "Version" |> Option.defaultValue ""
                let extraProps =
                    n.Properties
                    |> Map.remove "ArtifactType"
                    |> Map.remove "ArtifactId"
                    |> Map.remove "Version"
                    |> Map.toList
                    |> List.map (fun (k, v) -> $"{k}: {v}")
                    |> String.concat ", "

                let extraStr = if extraProps = "" then "" else $", {extraProps}"
                $"- {artType} {artId} (Version {ver}{extraStr})")
            |> String.concat "\n"

        // Determine which source artifacts are linked to which revision steps (optional context).
        // The graph already contains "RevisedInto" edges; we can mention the observations linked to revisions.
        let observationLinks =
            graph.Edges
            |> List.filter(fun e -> e.Relationship = "RevisedInto")
            |> List.choose(fun e ->
                let sourceNode = graph.Nodes |> List.tryFind(fun n -> n.NodeId = e.SourceNode)

                match sourceNode with
                | Some n when n.NodeType = "SourceArtifact" ->
                    let artId = n.Properties |> Map.tryFind "ArtifactId" |> Option.defaultValue ""
                    if artId <> "" then Some $"{artId}" else None
                | _ -> None)

        let observationCtx =
            if observationLinks.IsEmpty then
                ""
            else
                " (linked to " + String.concat ", " observationLinks + ")"

        let contextText =
            graph.Nodes
            |> List.tryFind (fun n -> n.NodeType = "ExplanationContext")
            |> Option.map (fun n ->
                let cause = n.Properties |> Map.tryFind "Cause" |> Option.defaultValue ""
                let contrib = n.Properties |> Map.tryFind "Contributions" |> Option.defaultValue ""
                let result = n.Properties |> Map.tryFind "Result" |> Option.defaultValue ""
                $"- Cause: {cause}\n- Contributing artifacts: {contrib}\n- Result: {result}")
            |> Option.defaultValue ""

        "System: You are a demand planning explanation assistant. You MUST use ONLY the facts provided below. Do NOT add any extra information or speculate. Write exactly 2–3 factual sentences. If you cannot answer completely from the provided facts, state what is missing.\n\n"
        + $"Target: {artifactType} {artifactId}\n\n"
        + "Source:\n"
        + sourceDescriptions
        + "\n\n"
        + "Decision history (in order):\n"
        + decisionLines
        + "\n\n"
        + (if observationCtx <> "" then
               "Note: Observations linked to revisions: " + observationCtx + "\n\n"
           else
               "")
        + "Context:\n"
        + contextText
        + "\n\n\n"
        + $"Question: Why did the {artifactType} change?\n\n"
        + "Answer:"

    /// In‑memory fake generator for deterministic testing.
    let fakeGenerator (fixedResult: string) : NaturalLanguageGenerator = fun _ _ _ -> Task.FromResult fixedResult

    /// Ollama HTTP generator with grounding parameters.
    let createOllamaGenerator (httpClient: HttpClient) (endpoint: string) (model: string) : NaturalLanguageGenerator =
        fun graph artifactType artifactId ->
            task {
                try
                    let prompt = buildPrompt graph artifactType artifactId

                    let requestPayload =
                        { model = model
                          prompt = prompt
                          stream = false
                          temperature = 0.0
                          stop = [ "\n"; "." ] } // stop at first full stop to prevent rambling

                    let jsonPayload = JsonSerializer.Serialize requestPayload
                    use content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json")

                    let requestUrl = $"{endpoint.TrimEnd('/')}/api/generate"
                    let! response = httpClient.PostAsync(requestUrl, content)

                    if response.IsSuccessStatusCode then
                        let! responseString = response.Content.ReadAsStringAsync()
                        let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
                        let result = JsonSerializer.Deserialize<OllamaResponse>(responseString, options)
                        return result.response.Trim()
                    else
                        return
                            $"Explanation for {artifactType} '{artifactId}' with {graph.Nodes.Length} reasoning nodes."
                with _ ->
                    return $"Explanation for {artifactType} '{artifactId}' with {graph.Nodes.Length} reasoning nodes."
            }

*)
(*namespace Medhavi.Demand.DemandExplanation

open System.Threading.Tasks
open System.Net.Http
open System.Text.Json
open Medhavi.Demand.DemandExplanation.Model

/// Pure functional generator type signature
type NaturalLanguageGenerator = StructuredReasoningGraph -> string -> string -> Task<string>

[<CLIMutable>]
type OllamaResponse =
    { response: string }

[<CLIMutable>]
type OllamaRequest =
    { model: string
      prompt: string
      stream: bool }

module NaturalLanguageGenerator =

    /// Prompt builder defined as a named helper function
    let buildPrompt (graph: StructuredReasoningGraph) (artifactType: string) (artifactId: string) : string =
        let sourceArtifacts =
            graph.Nodes
            |> List.filter (fun n -> n.NodeType = "SourceArtifact")
            |> List.map (fun n ->
                let artType = n.Properties |> Map.tryFind "ArtifactType" |> Option.defaultValue ""
                let artId = n.Properties |> Map.tryFind "ArtifactId" |> Option.defaultValue ""
                let ver = n.Properties |> Map.tryFind "Version" |> Option.defaultValue ""
                $"- {artType} '{artId}' (Version: {ver})")
            |> String.concat "\n"

        let graphJson = JsonSerializer.Serialize graph

        "System: You are Medhavi's planning explanation system. Translate this structured decision reasoning graph into a clear, concise natural language explanation for a demand planner (2-3 sentences max). Maintain a professional tone. Do not mention technical code types or code structures. Explaining the reasoning graph nodes is key.\n\n" +
        "CRITICAL INSTRUCTIONS FOR GROUNDING:\n" +
        "1. Rely ONLY on the explicit facts, nodes, and relationships in the reasoning graph.\n" +
        "2. Do NOT extrapolate, assume, or invent details about the supply chain (such as warehouses, stocking locations, capacity limits, customer demands, or market scenarios) that are not explicitly present as properties in the graph.\n" +
        "3. Explicitly reference the concrete source artifact IDs (e.g. OBS-001, OBS-002), quantities, versions, and rules evaluated (e.g. BR-D-010, BR-D-011) when they are present in the graph.\n" +
        "4. Keep the explanation strictly factual and concise.\n\n" +
        $"Input Source Artifacts:\n{sourceArtifacts}\n\n" +
        $"Reasoning Graph (JSON):\n{graphJson}\n\n" +
        $"Explained Target Artifact Type: {artifactType}\n" +
        $"Explained Target Artifact ID: {artifactId}\n\n" +
        "Explanation:"

    /// In-memory fake generator for deterministic testing
    let fakeGenerator (fixedResult: string) : NaturalLanguageGenerator =
        fun _ _ _ -> Task.FromResult fixedResult

    /// Ollama HTTP generator with configuration parameters and safety fallbacks
    let createOllamaGenerator (httpClient: HttpClient) (endpoint: string) (model: string) : NaturalLanguageGenerator =
        fun graph artifactType artifactId ->
            task {
                try
                    let prompt = buildPrompt graph artifactType artifactId

                    let requestPayload = { model = model; prompt = prompt; stream = false }
                    let jsonPayload = JsonSerializer.Serialize requestPayload
                    use content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json")

                    let requestUrl = $"{endpoint.TrimEnd('/')}/api/generate"
                    let! response = httpClient.PostAsync(requestUrl, content)

                    if response.IsSuccessStatusCode then
                        let! responseString = response.Content.ReadAsStringAsync()
                        let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
                        let result = JsonSerializer.Deserialize<OllamaResponse>(responseString, options)
                        return result.response.Trim()
                    else
                        // Fallback on HTTP failure
                        return $"Explanation for {artifactType} '{artifactId}' with {graph.Nodes.Length} reasoning nodes."
                with
                | _ ->
                    // Fallback on timeout or exception (e.g. Ollama not running)
                    return $"Explanation for {artifactType} '{artifactId}' with {graph.Nodes.Length} reasoning nodes."
            }
*)
