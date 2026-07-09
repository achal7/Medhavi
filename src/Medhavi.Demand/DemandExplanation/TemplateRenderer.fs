module Medhavi.Demand.DemandExplanation.TemplateRenderer

open Medhavi.Demand.DemandExplanation.Model

type TemplateResolver = ExplanationNode -> string option

let private getProp key (node: ExplanationNode) = node.Properties |> Map.tryFind key |> Option.defaultValue ""

/// Default template resolver for EnterpriseDemandPicture context nodes
let defaultEdpResolver: TemplateResolver =
    fun node ->
        let cause = getProp "Cause" node
        let contributions = getProp "Contributions" node
        let derivedFacts = getProp "DerivedFacts" node

        // Initial state (version 1, no revision history)
        if cause = "Initial state established." then
            Some
                "The EnterpriseDemandPicture is the initial published version. No revision history or contributing observations are recorded."
        elif System.String.IsNullOrEmpty cause || cause = "A revision occurred." then
            Some "A revision occurred; the root cause is not recorded."
        else
            let factsStr =
                if System.String.IsNullOrEmpty derivedFacts then
                    ""
                else
                    $" resulting in a {derivedFacts.ToLower().TrimEnd('.')}"

            let contribStr =
                if System.String.IsNullOrEmpty contributions then
                    ""
                else
                    $" driven by the incorporation of {contributions}"

            let sentence = $"A revision occurred to the EnterpriseDemandPicture{factsStr}{contribStr}."
            Some sentence

/// Catalog mapping of (templateVersion, target artifact type) to resolver
let catalog: Map<string * string, TemplateResolver> =
    Map.ofList [ ("v1", "EnterpriseDemandPicture"), defaultEdpResolver ]

/// Resolve template-based explanation for a given template version, artifact type and business context node
let resolve (templateVersion: string) (artifactType: string) (node: ExplanationNode) : string option =
    catalog |> Map.tryFind(templateVersion, artifactType) |> Option.bind(fun resolver -> resolver node)
