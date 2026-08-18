/// Pure Deterministic Template Renderer for NLG and Factor Waterfall Tables
module Medhavi.Demand.ExplainDemand.DemandExplanation.TemplateRenderer

open System
open System.Text.RegularExpressions
open Model
open TemplateCatalog

/// Replaces {Placeholder} tokens with property values deterministically
let renderSummary (template: ExplanationTemplate) (properties: Map<string, string>) : string =
    Regex.Replace(
        template.SummaryTemplate,
        @"\{(\w+)\}",
        fun m ->
            let key = m.Groups.[1].Value
            properties |> Map.tryFind key |> Option.defaultValue "[Not Available]"
    )
    |> fun s -> s.Trim()

/// Renders a Markdown Waterfall Reconciliation Table for human planners
let renderWaterfallMarkdown (factors: FactorContribution list) (totalExplainedValue: decimal) : string =
    if factors.IsEmpty then
        "No quantitative factor decomposition available."
    else
        let header =
            "| Driver / Factor Name | Impact (Units/Score) | Contribution (%) | Direction | Confidence |\n"
            + "| :--- | :--- | :--- | :--- | :--- |\n"

        let rows =
            factors
            |> List.map (fun f ->
                let sign = if f.ImpactValue >= 0.0m then "+" else ""
                let pctSign = if f.PercentageContribution >= 0.0m then "+" else ""
                let impactStr = sign + f.ImpactValue.ToString("N2")
                let contribStr = pctSign + f.PercentageContribution.ToString("N1") + "%"
                let confStr = f.Confidence.ToString("N1") + "%"
                "| " + f.FactorName + " | " + impactStr + " | " + contribStr + " | " + f.Direction.AsString + " | " + confStr + " |")
            |> String.concat "\n"

        let footer =
            "\n| **Total Reconciled Value** | **"
            + totalExplainedValue.ToString("N2")
            + "** | **100.0%** | **Final Plan** | **100.0%** |"

        header + rows + footer

