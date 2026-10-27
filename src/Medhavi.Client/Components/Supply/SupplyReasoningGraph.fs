module Medhavi.Client.Components.Supply.SupplyReasoningGraph

open System
open System.Text
open Bolero
open Bolero.Html
open Medhavi.Contracts.Supply

let private nodeColors (nodeType: string) (isHypothetical: bool) =
    if isHypothetical then
        ("var(--rz-series-1, var(--rz-primary))", "var(--rz-primary-lighter)", "var(--rz-primary-light)")
    else
        match nodeType with
        | "RootCause" -> ("var(--rz-danger)", "var(--rz-danger-lighter)", "var(--rz-danger-light)")
        | "Bottleneck" -> ("var(--rz-warning-dark)", "var(--rz-warning-lighter)", "var(--rz-warning-light)")
        | "Constraint" -> ("var(--rz-secondary)", "var(--rz-secondary-lighter)", "var(--rz-border-color)")
        | "Disruption" -> ("var(--rz-danger)", "var(--rz-danger-lighter)", "var(--rz-danger-light)")
        | "PrescriptiveRecommendation" -> ("var(--rz-success)", "var(--rz-success-lighter)", "var(--rz-success-light)")
        | _ -> ("var(--rz-primary)", "var(--rz-primary-lighter)", "var(--rz-primary-light)")

let private buildSvgMarkup (graph: StructuredReasoningGraphDto) (selectedNodeId: string option) : string =

    let sb = StringBuilder()

    let categorizeStage (node: ExplanationNodeDto) =
        if node.NodeType = "RootCause" || node.NodeType = "Disruption" then 0
        elif node.NodeType = "Constraint" then 1
        elif node.NodeType = "Bottleneck" then 2
        elif node.IsHypothetical || node.NodeType = "PrescriptiveRecommendation" then 3
        else 1

    let nodesWithStage = graph.Nodes |> List.groupBy categorizeStage |> List.sortBy fst

    let colWidth = 230
    let nodeCardWidth = 190
    let nodeCardHeight = 64
    let rowSpacing = 85
    let startX = 30
    let startY = 30

    let nodePositions: Map<string, (int * int)> =
        nodesWithStage
        |> List.collect(fun (stage, nodes) ->
            nodes
            |> List.mapi(fun rowIndex node ->
                let x = startX + stage * colWidth
                let y = startY + rowIndex * rowSpacing
                (node.NodeId, (x, y))))
        |> Map.ofList

    let maxCols = if List.isEmpty nodesWithStage then 1 else List.length nodesWithStage

    let maxRows =
        if List.isEmpty nodesWithStage then
            1
        else
            nodesWithStage |> List.map(fun (_, ns) -> List.length ns) |> List.max

    let svgWidth = max 780 (startX + maxCols * colWidth + 40)
    let svgHeight = max 260 (startY + maxRows * rowSpacing + 40)

    sb.AppendFormat(
        "<svg width=\"{0}\" height=\"{1}\" viewBox=\"0 0 {0} {1}\" xmlns=\"http://www.w3.org/2000/svg\" style=\"display: block;\">",
        svgWidth,
        svgHeight
    )
    |> ignore

    // Defs for Arrowhead markers
    sb.Append("<defs>") |> ignore

    sb.Append(
        "<marker id=\"arrow-norm\" viewBox=\"0 0 10 10\" refX=\"8\" refY=\"5\" markerWidth=\"6\" markerHeight=\"6\" orient=\"auto-start-reverse\"><path d=\"M 0 1 L 9 5 L 0 9 z\" fill=\"var(--rz-border-color)\" /></marker>"
    )
    |> ignore

    sb.Append(
        "<marker id=\"arrow-hypo\" viewBox=\"0 0 10 10\" refX=\"8\" refY=\"5\" markerWidth=\"6\" markerHeight=\"6\" orient=\"auto-start-reverse\"><path d=\"M 0 1 L 9 5 L 0 9 z\" fill=\"var(--rz-series-1, var(--rz-primary))\" /></marker>"
    )
    |> ignore

    sb.Append("</defs>") |> ignore

    // Render Edges
    for edge in graph.Edges do
        match Map.tryFind edge.SourceNode nodePositions, Map.tryFind edge.TargetNode nodePositions with
        | Some(sx, sy), Some(tx, ty) ->
            let x1 = sx + nodeCardWidth
            let y1 = sy + (nodeCardHeight / 2)
            let x2 = tx
            let y2 = ty + (nodeCardHeight / 2)
            let dx = (x2 - x1) / 2
            let strokeColor = if edge.IsHypothetical then "var(--rz-series-1, var(--rz-primary))" else "var(--rz-border-color)"
            let markerId = if edge.IsHypothetical then "url(#arrow-hypo)" else "url(#arrow-norm)"
            let dashArray = if edge.IsHypothetical then " stroke-dasharray=\"4,4\"" else ""

            sb.AppendFormat(
                "<path d=\"M {0} {1} C {2} {1}, {3} {5}, {4} {5}\" fill=\"none\" stroke=\"{6}\" stroke-width=\"2\"{7} marker-end=\"{8}\" />",
                x1,
                y1,
                (x1 + dx),
                (x2 - dx),
                x2,
                y2,
                strokeColor,
                dashArray,
                markerId
            )
            |> ignore
        | _ -> ()

    // Render Nodes
    for node in graph.Nodes do
        match Map.tryFind node.NodeId nodePositions with
        | Some(x, y) ->
            let strokeColor, bgColor, borderColor = nodeColors node.NodeType node.IsHypothetical
            let isSelected = selectedNodeId = Some node.NodeId
            let borderStroke = if isSelected then "var(--rz-primary)" else borderColor
            let strokeWidth = if isSelected then "2.5" else "1.5"
            let dashArray = if node.IsHypothetical then " stroke-dasharray=\"5,4\"" else ""

            sb.AppendFormat(
                "<rect x=\"{0}\" y=\"{1}\" width=\"{2}\" height=\"{3}\" rx=\"8\" ry=\"8\" fill=\"{4}\" stroke=\"{5}\" stroke-width=\"{6}\"{7} />",
                x,
                y,
                nodeCardWidth,
                nodeCardHeight,
                bgColor,
                borderStroke,
                strokeWidth,
                dashArray
            )
            |> ignore

            // Header Pill / Type Badge
            sb.AppendFormat(
                "<text x=\"{0}\" y=\"{1}\" font-size=\"10\" font-weight=\"700\" fill=\"{2}\" font-family=\"Segoe UI, sans-serif\">{3}</text>",
                (x + 12),
                (y + 20),
                strokeColor,
                node.NodeType.ToUpperInvariant()
            )
            |> ignore

            if node.IsHypothetical then
                sb.AppendFormat(
                    "<text x=\"{0}\" y=\"{1}\" font-size=\"9\" font-weight=\"600\" fill=\"var(--rz-series-1, var(--rz-primary))\" font-family=\"Segoe UI, sans-serif\">WHAT-IF</text>",
                    (x + nodeCardWidth - 52),
                    (y + 20)
                )
                |> ignore

            let label =
                if node.Label.Length > 24 then
                    node.Label.Substring(0, 22) + "..."
                else
                    node.Label

            sb.AppendFormat(
                "<text x=\"{0}\" y=\"{1}\" font-size=\"11\" font-weight=\"600\" fill=\"var(--rz-text-color)\" font-family=\"Segoe UI, sans-serif\">{2}</text>",
                (x + 12),
                (y + 44),
                label
            )
            |> ignore
        | None -> ()

    sb.Append("</svg>") |> ignore
    sb.ToString()

let renderDag
    (graph: StructuredReasoningGraphDto)
    (selectedNodeId: string option)
    (_onSelectNode: string -> unit)
    : Node =

    div {
        attr.style
            "position: relative; width: 100%; height: 280px; overflow: auto; background: var(--rz-base-background-color); border: 1px solid var(--rz-border-color); border-radius: 8px; padding: 12px; box-sizing: border-box;"

        rawHtml(buildSvgMarkup graph selectedNodeId)
    }

let renderWaterfall (factors: FactorContributionDto list) : Node =
    div {
        attr.style
            "background: var(--rz-base-background-color); border: 1px solid var(--rz-border-color); border-radius: 8px; padding: 12px; box-sizing: border-box;"

        div {
            attr.style
                "font-size: 12px; font-weight: 700; color: var(--rz-text-color); margin-bottom: 8px; display: flex; justify-content: space-between; align-items: center;"

            span { "Factor Attribution & Driver Waterfall (BA-S-090)" }

            span {
                attr.style "font-size: 10px; color: var(--rz-text-secondary-color); font-weight: normal;"
                "Variance: 0.00"
            }
        }

        forEach factors (fun factor ->
            let isUnfavorable = factor.Direction.Equals("Unfavorable", StringComparison.OrdinalIgnoreCase)
            let barColor = if isUnfavorable then "var(--rz-danger)" else "var(--rz-success)"

            let signStr =
                if factor.ImpactValue >= 0m then
                    sprintf "+%M" factor.ImpactValue
                else
                    sprintf "%M" factor.ImpactValue

            div {
                attr.style "margin-bottom: 8px; display: flex; flex-direction: column; gap: 3px;"

                div {
                    attr.style "display: flex; justify-content: space-between; font-size: 11px;"

                    span {
                        attr.style "font-weight: 600; color: var(--rz-text-color);"
                        factor.FactorName
                    }

                    span {
                        attr.style(sprintf "font-weight: 700; color: %s;" barColor)
                        sprintf "%s (%M%%)" signStr factor.PercentageContribution
                    }
                }

                // Bar container
                div {
                    attr.style
                        "width: 100%; height: 8px; background: var(--rz-secondary-lighter); border-radius: 4px; overflow: hidden; position: relative;"

                    div {
                        let widthPct = Math.Min(100.0, Math.Abs(float factor.PercentageContribution))

                        attr.style(
                            sprintf
                                "height: 100%%; width: %.1f%%; background: %s; border-radius: 4px;"
                                widthPct
                                barColor
                        )
                    }
                }
            })
    }
