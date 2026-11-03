module Medhavi.Client.Core.Traceability.TraceCanvasRenderer

open System
open System.Text
open Bolero
open Bolero.Html
open Medhavi.Contracts.Traceability

let private cleanAggregateId (raw: string) : string =
    if String.IsNullOrWhiteSpace raw then ""
    else
        let stripped = raw.Replace("\"", "").Trim()
        let prefixes = [ "ExceptionId "; "DemandId "; "SupplyId "; "OrderId "; "PlanId "; "RecordId "; "ScopeId "; "ResourceId " ]
        let mutable result = stripped
        for p in prefixes do
            if result.StartsWith(p, StringComparison.OrdinalIgnoreCase) then
                result <- result.Substring(p.Length).Trim()
        result

let private nodeSeverityStyles (severity: string) =
    match severity.ToLowerInvariant() with
    | "error" | "errors" | "critical" ->
        ("var(--rz-danger)", "rgba(220, 53, 69, 0.08)", "var(--rz-danger)", "error")
    | "warning" | "warn" ->
        ("var(--rz-warning-dark)", "rgba(255, 193, 7, 0.08)", "var(--rz-warning)", "warning")
    | _ ->
        ("var(--rz-primary)", "var(--rz-base-100)", "var(--rz-border-color)", "check_circle")

let private domainThemeColors (bc: string) =
    match bc.ToLowerInvariant() with
    | "core" -> ("#6366f1", "rgba(99, 102, 241, 0.12)", "#4f46e5")
    | "demand" -> ("#f59e0b", "rgba(245, 158, 11, 0.12)", "#d97706")
    | "supply" -> ("#06b6d4", "rgba(6, 182, 212, 0.12)", "#0891b2")
    | "promise" -> ("#10b981", "rgba(16, 185, 129, 0.12)", "#059669")
    | _ -> ("var(--rz-primary)", "rgba(67, 80, 222, 0.12)", "var(--rz-primary-dark)")

type private EdgeGeometry =
    { PathD: string
      MidX: float
      MidY: float
      StrokeColor: string
      DashArray: string
      MarkerId: string }

let private computeEdgeGeometry (src: TraceCanvasNode) (tgt: TraceCanvasNode) (isLoopback: bool) (hasErrors: bool) : EdgeGeometry =
    let colDiff = tgt.X - src.X
    let strokeColor =
        if hasErrors then "#ef4444"
        elif isLoopback then "#f59e0b"
        elif abs colDiff <= 30.0 then "#64748b"
        else "#0284c7"

    let marker =
        if hasErrors then "url(#arrow-err)"
        elif isLoopback then "url(#arrow-loop)"
        elif abs colDiff <= 30.0 then "url(#arrow-step)"
        else "url(#arrow-norm)"

    let dash =
        if isLoopback then " stroke-dasharray=\"6,4\""
        elif hasErrors then " stroke-dasharray=\"5,3\""
        else ""

    if colDiff > 30.0 then
        // CASE 1: Forward Cross-Context (Left -> Right)
        // Exits source right center -> enters target left center
        let x1 = src.X + src.Width
        let y1 = src.Y + (src.Height / 2.0)
        let x2 = tgt.X
        let y2 = tgt.Y + (tgt.Height / 2.0)
        let dx = max 50.0 ((x2 - x1) * 0.45)
        let cp1x, cp1y = x1 + dx, y1
        let cp2x, cp2y = x2 - dx, y2
        let midX = 0.125 * x1 + 0.375 * cp1x + 0.375 * cp2x + 0.125 * x2
        let midY = 0.125 * y1 + 0.375 * cp1y + 0.375 * cp2y + 0.125 * y2
        let d = sprintf "M %.1f %.1f C %.1f %.1f, %.1f %.1f, %.1f %.1f" x1 y1 cp1x cp1y cp2x cp2y x2 y2
        { PathD = d; MidX = midX; MidY = midY; StrokeColor = strokeColor; DashArray = dash; MarkerId = marker }

    elif colDiff < -30.0 then
        // CASE 2: Backward / Return Loop (Right -> Left across columns)
        // Exits source left center -> arches gracefully -> enters target right center
        let x1 = src.X
        let y1 = src.Y + (src.Height / 2.0)
        let x2 = tgt.X + tgt.Width
        let y2 = tgt.Y + (tgt.Height / 2.0)
        let dx = max 55.0 ((x1 - x2) * 0.35)
        let cp1x, cp1y = x1 - dx, y1
        let cp2x, cp2y = x2 + dx, y2
        let midX = 0.125 * x1 + 0.375 * cp1x + 0.375 * cp2x + 0.125 * x2
        let midY = 0.125 * y1 + 0.375 * cp1y + 0.375 * cp2y + 0.125 * y2
        let d = sprintf "M %.1f %.1f C %.1f %.1f, %.1f %.1f, %.1f %.1f" x1 y1 cp1x cp1y cp2x cp2y x2 y2
        { PathD = d; MidX = midX; MidY = midY; StrokeColor = strokeColor; DashArray = dash; MarkerId = marker }

    else
        // CASE 3: Intra-Domain Step (Downward in Same Column)
        // Exits source bottom center -> enters target top center
        let x1 = src.X + (src.Width / 2.0)
        let y1 = src.Y + src.Height
        let x2 = tgt.X + (tgt.Width / 2.0)
        let y2 = tgt.Y
        let dy = max 25.0 ((y2 - y1) * 0.5)
        let cp1x, cp1y = x1, y1 + dy
        let cp2x, cp2y = x2, y2 - dy
        let midX = (x1 + x2) / 2.0
        let midY = (y1 + y2) / 2.0
        let d = sprintf "M %.1f %.1f C %.1f %.1f, %.1f %.1f, %.1f %.1f" x1 y1 cp1x cp1y cp2x cp2y x2 y2
        { PathD = d; MidX = midX; MidY = midY; StrokeColor = strokeColor; DashArray = dash; MarkerId = marker }

let private buildSvgBackground (nodes: TraceCanvasNode list) (edges: TraceCanvasEdge list) (width: float) (height: float) : string =
    let nodeMap = nodes |> List.map (fun n -> n.NodeId, n) |> Map.ofList
    let sb = StringBuilder()

    sb.Append(sprintf "<svg width=\"%.0f\" height=\"%.0f\" viewBox=\"0 0 %.0f %.0f\" xmlns=\"http://www.w3.org/2000/svg\" style=\"position: absolute; top: 0; left: 0; width: %.0fpx; height: %.0fpx; pointer-events: none; font-family: Segoe UI, sans-serif;\">" width height width height width height) |> ignore

    // Defs: Modern stealth chevrons that dock flush with card outer border
    sb.Append("<defs>") |> ignore
    sb.Append("<marker id=\"arrow-norm\" viewBox=\"0 0 10 10\" refX=\"9\" refY=\"5\" markerWidth=\"7\" markerHeight=\"7\" orient=\"auto\"><polygon points=\"0 1, 9 5, 0 9, 2 5\" fill=\"#0284c7\" /></marker>") |> ignore
    sb.Append("<marker id=\"arrow-loop\" viewBox=\"0 0 10 10\" refX=\"9\" refY=\"5\" markerWidth=\"7\" markerHeight=\"7\" orient=\"auto\"><polygon points=\"0 1, 9 5, 0 9, 2 5\" fill=\"#f59e0b\" /></marker>") |> ignore
    sb.Append("<marker id=\"arrow-err\" viewBox=\"0 0 10 10\" refX=\"9\" refY=\"5\" markerWidth=\"7\" markerHeight=\"7\" orient=\"auto\"><polygon points=\"0 1, 9 5, 0 9, 2 5\" fill=\"#ef4444\" /></marker>") |> ignore
    sb.Append("<marker id=\"arrow-step\" viewBox=\"0 0 10 10\" refX=\"9\" refY=\"5\" markerWidth=\"7\" markerHeight=\"7\" orient=\"auto\"><polygon points=\"0 1, 9 5, 0 9, 2 5\" fill=\"#64748b\" /></marker>") |> ignore
    sb.Append("</defs>") |> ignore

    // Swimlane Column Shading, Headers & Vertical Boundary Lines
    let swimlanes = [ "CORE DOMAIN", 50.0; "DEMAND DOMAIN", 370.0; "SUPPLY DOMAIN", 690.0; "PROMISE DOMAIN", 1010.0 ]
    for idx, (title, x) in List.indexed swimlanes do
        let bc = title.Split(' ').[0]
        let strokeCol, fillCol, _ = domainThemeColors bc

        // Subtle alternating lane shading for structural depth
        let laneFill = if idx % 2 = 1 then "rgba(241, 245, 249, 0.45)" else "transparent"
        sb.Append(sprintf "<rect x=\"%.0f\" y=\"0\" width=\"320\" height=\"%.0f\" fill=\"%s\" />" (x - 25.0) height laneFill) |> ignore
        sb.Append(sprintf "<line x1=\"%.0f\" y1=\"0\" x2=\"%.0f\" y2=\"%.0f\" stroke=\"var(--rz-border-color)\" stroke-width=\"1\" opacity=\"0.55\" />" (x - 25.0 + 320.0) (x - 25.0 + 320.0) height) |> ignore

        // Header pill
        sb.Append(sprintf "<rect x=\"%.0f\" y=\"14\" width=\"270\" height=\"34\" rx=\"7\" fill=\"%s\" stroke=\"%s\" stroke-width=\"1.5\" />" x fillCol strokeCol) |> ignore
        sb.Append(sprintf "<text x=\"%.0f\" y=\"36\" font-size=\"12.5\" font-weight=\"700\" fill=\"%s\" text-anchor=\"middle\" letter-spacing=\"0.5px\">%s</text>" (x + 135.0) strokeCol title) |> ignore

    // Edges with Dynamic Geometry and Precise Bezier Midpoint Latency Tags
    for edge in edges do
        match Map.tryFind edge.SourceNodeId nodeMap, Map.tryFind edge.TargetNodeId nodeMap with
        | Some src, Some tgt ->
            let geom = computeEdgeGeometry src tgt edge.IsLoopback edge.HasErrors
            sb.Append(sprintf "<path d=\"%s\" fill=\"none\" stroke=\"%s\" stroke-width=\"2.4\"%s marker-end=\"%s\" />" geom.PathD geom.StrokeColor geom.DashArray geom.MarkerId) |> ignore

            // Latency Badge Pill placed on the true Bezier midpoint
            if edge.LatencyMs > 0.0 then
                let pillW = 58.0
                let pillH = 19.0
                let rx = 9.5
                let px = geom.MidX - (pillW / 2.0)
                let py = geom.MidY - (pillH / 2.0)
                sb.Append(sprintf "<rect x=\"%.1f\" y=\"%.1f\" width=\"%.1f\" height=\"%.1f\" rx=\"%.1f\" fill=\"var(--rz-base-background-color)\" stroke=\"%s\" stroke-width=\"1.2\" />" px py pillW pillH rx geom.StrokeColor) |> ignore
                sb.Append(sprintf "<text x=\"%.1f\" y=\"%.1f\" font-size=\"10.5\" font-weight=\"700\" fill=\"%s\" text-anchor=\"middle\" dominant-baseline=\"central\">+%.0fms</text>" geom.MidX geom.MidY geom.StrokeColor edge.LatencyMs) |> ignore
        | _ -> ()

    sb.Append("</svg>") |> ignore
    sb.ToString()

let renderSvgCanvas
    (dossier: TraceDossierDto)
    (zoomLevel: float)
    (selectedNodeId: string option)
    (onSelectNode: string -> unit)
    : Node =

    let nodes, edges, (width, height) = TraceCanvasLayout.computeLayout dossier

    div {
        attr.style "position: relative; width: 100%; height: 100%; overflow: auto; background: var(--rz-base-background-color); box-sizing: border-box;"

        // Canvas Scale Wrapper
        div {
            attr.style (sprintf "position: relative; min-width: %.0fpx; min-height: %.0fpx; width: %.0fpx; height: %.0fpx; transform: scale(%.2f); transform-origin: top left; transition: transform 0.2s ease-out;" width height width height zoomLevel)

            // 1. SVG Layer (Background connections, column shading, & swimlane pillars)
            rawHtml (buildSvgBackground nodes edges width height)

            // 2. HTML Layer (Interactive Bolero Node Cards with Crisp Typography & Domain Accent Strips)
            for node in nodes do
                let strokeColor, bgColor, borderColor, iconName = nodeSeverityStyles node.Severity
                let domainStroke, domainBg, _ = domainThemeColors node.BoundedContext
                let isSelected = selectedNodeId = Some node.NodeId
                let outlineStyle =
                    if isSelected then
                        "outline: 3px solid var(--rz-primary); box-shadow: 0 0 0 5px rgba(67, 80, 222, 0.25), 0 8px 18px rgba(0,0,0,0.12);"
                    else
                        "box-shadow: 0 2px 7px rgba(0,0,0,0.06);"

                let cardStyle =
                    sprintf "position: absolute; left: %.0fpx; top: %.0fpx; width: %.0fpx; height: %.0fpx; background: %s; border: 1.5px solid %s; border-radius: 8px; padding: 10px 12px 9px 12px; box-sizing: border-box; cursor: pointer; display: flex; flex-direction: column; justify-content: space-between; user-select: none; transition: transform 0.15s ease, box-shadow 0.15s ease; %s overflow: hidden;"
                        node.X node.Y node.Width node.Height bgColor (if isSelected then "var(--rz-primary)" else borderColor) outlineStyle

                let displayAggId = cleanAggregateId node.AggregateId
                let displayAggShort = if displayAggId.Length > 18 then displayAggId.Substring(0, 16) + "..." else displayAggId

                let stageSummary =
                    if node.Stages.Length > 0 then
                        node.Stages
                        |> List.map (fun s ->
                            if s.Contains("AggregateLoaded") || s.Contains("Load") then "Loaded"
                            elif s.Contains("DecisionEvaluated") || s.Contains("Decide") then "Decided"
                            elif s.Contains("EventsPersisted") || s.Contains("Persist") then "Persisted"
                            elif s.Contains("CommandCompleted") || s.Contains("Completed") then "Done"
                            else s)
                        |> String.concat " → "
                    else node.Message

                div {
                    attr.style cardStyle
                    attr.callback "onclick" (fun _ -> onSelectNode node.NodeId)

                    // Domain Colored Top Accent Strip (3.5px)
                    div {
                        attr.style (sprintf "position: absolute; top: 0; left: 0; right: 0; height: 3.5px; background: %s;" domainStroke)
                    }

                    // Card Header: Context Pill + Timestamp + Status Icon
                    div {
                        attr.style "display: flex; justify-content: space-between; align-items: center; margin-top: 1px;"

                        div {
                            attr.style "display: flex; align-items: center; gap: 0.35rem;"

                            span {
                                attr.style (sprintf "font-size: 10.5px; font-weight: 700; color: %s; background: %s; padding: 1px 6px; border-radius: 4px;" domainStroke domainBg)
                                node.BoundedContext.ToUpperInvariant()
                            }

                            span {
                                attr.``class`` "rzi"
                                attr.style (sprintf "font-size: 13px; color: %s;" strokeColor)
                                iconName
                            }
                        }

                        span {
                            attr.style "font-size: 11px; font-weight: 600; color: var(--rz-text-secondary-color);"
                            node.Timestamp.ToString("HH:mm:ss.fff")
                        }
                    }

                    // Card Body: Action / Command Title (Bold 13.5px Crisp Text)
                    div {
                        attr.style "font-size: 13.5px; font-weight: 700; color: var(--rz-text-color); line-height: 1.25; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;"
                        attr.title node.CommandType
                        node.CommandType
                    }

                    // Card Middle: Clean Aggregate ID & Stage Progress Pill
                    div {
                        attr.style "display: flex; align-items: center; justify-content: space-between; gap: 4px; font-size: 11px;"

                        span {
                            attr.style "font-family: 'JetBrains Mono', Consolas, monospace; color: var(--rz-text-secondary-color); overflow: hidden; text-overflow: ellipsis; white-space: nowrap;"
                            attr.title (sprintf "%s: %s" node.AggregateType displayAggId)
                            sprintf "%s: %s" node.AggregateType displayAggShort
                        }

                        if node.StageCount > 1 then
                            span {
                                attr.style "font-size: 10px; font-weight: 600; color: var(--rz-info-dark); background: rgba(0, 180, 216, 0.12); padding: 1px 6px; border-radius: 4px; flex-shrink: 0;"
                                sprintf "%d stages (%.1fms)" node.StageCount node.TotalDurationMs
                            }
                    }

                    // Card Footer: Pipeline Stages Flow & Decision Pill
                    div {
                        attr.style "display: flex; justify-content: space-between; align-items: center; font-size: 10px;"

                        span {
                            attr.style "color: var(--rz-text-secondary-color); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; max-width: 195px;"
                            attr.title node.Message
                            stageSummary
                        }

                        if node.HasDecision then
                            span {
                                attr.style "font-size: 10px; font-weight: 700; color: var(--rz-warning-dark); background: rgba(255, 193, 7, 0.15); padding: 1px 6px; border-radius: 4px; flex-shrink: 0;"
                                "DECISION"
                            }
                    }
                }
        }
    }
