namespace Medhavi.Client.Core.Traceability

open System
open System.Text.Json
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Medhavi.Client.ViewComposition.Context
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.Rendering
open Medhavi.Client.Controls
open Medhavi.Contracts.Traceability

module TraceabilityStudio =

    let private getStringCell (colId: string) (row: UniversalRow) : string =
        match Map.tryFind colId row.Cells with
        | Some (TextValue s) -> s
        | Some (EnumValue s) -> s
        | Some (ReferenceValue (k, _)) -> k
        | _ -> ""

    let private getDateCell (colId: string) (row: UniversalRow) : DateTimeOffset =
        match Map.tryFind colId row.Cells with
        | Some (DateTimeValue dt) -> dt
        | _ -> DateTimeOffset.UtcNow

    let render (ctx: ComponentRenderContext) (dispatch: ComponentEvent -> unit) : Node =
        let rows =
            match ctx.Data with
            | Tabular r -> r
            | _ -> []

        // Parameters
        let activeTab =
            match Map.tryFind "ActiveStudioTab" ctx.Configuration.Parameters with
            | Some (StringValue "Waterfall") -> TraceDetailTab.WaterfallTab
            | Some (StringValue "Rules") -> TraceDetailTab.RuleInspectorTab
            | Some (StringValue "Raw") -> TraceDetailTab.RawDossierTab
            | _ -> TraceDetailTab.FlowCanvasTab

        let domainFilter =
            match Map.tryFind "FilterDomain" ctx.Configuration.Parameters with
            | Some (StringValue d) when not (String.IsNullOrWhiteSpace d) -> d
            | _ -> "All"

        let timeFilter =
            match Map.tryFind "FilterTime" ctx.Configuration.Parameters with
            | Some (StringValue t) when not (String.IsNullOrWhiteSpace t) -> t
            | _ -> "All"

        let severityFilter =
            match Map.tryFind "FilterSeverity" ctx.Configuration.Parameters with
            | Some (StringValue s) when not (String.IsNullOrWhiteSpace s) -> s
            | _ -> "All"

        let searchFilter =
            match Map.tryFind "FilterSearch" ctx.Configuration.Parameters with
            | Some (StringValue q) -> q.Trim()
            | _ -> ""

        let zoomLevel =
            match Map.tryFind "StudioZoomLevel" ctx.Configuration.Parameters with
            | Some (StringValue z) ->
                match Double.TryParse z with
                | true, v when v >= 0.5 && v <= 2.5 -> v
                | _ -> 1.0
            | _ -> 1.0

        let selectedNodeId =
            match Map.tryFind "SelectedCanvasNodeId" ctx.Configuration.Parameters with
            | Some (StringValue n) when not (String.IsNullOrWhiteSpace n) -> Some n
            | _ -> None

        let setParam (key: string) (value: string) =
            let updatedParams = ctx.Configuration.Parameters |> Map.add key (StringValue value)
            dispatch (ConfigurationChangeRequested { ctx.Configuration with Parameters = updatedParams })

        let setParams (pairs: (string * string) list) =
            let updatedParams = pairs |> List.fold (fun acc (k, v) -> Map.add k (StringValue v) acc) ctx.Configuration.Parameters
            dispatch (ConfigurationChangeRequested { ctx.Configuration with Parameters = updatedParams })

        let setTab (tab: TraceDetailTab) =
            let tabStr =
                match tab with
                | TraceDetailTab.WaterfallTab -> "Waterfall"
                | TraceDetailTab.RuleInspectorTab -> "Rules"
                | TraceDetailTab.RawDossierTab -> "Raw"
                | _ -> "Flow"
            setParam "ActiveStudioTab" tabStr

        // Apply In-Memory Multi-Dimensional Filtering for Master Grid
        let now = DateTimeOffset.UtcNow
        let today = now.Date

        let filteredRows =
            rows
            |> List.filter (fun r ->
                // 1. Domain Filter
                let bc = getStringCell "BoundedContext" r
                let matchesDomain =
                    if domainFilter = "All" then true
                    else bc.Equals(domainFilter, StringComparison.OrdinalIgnoreCase)

                // 2. Time Filter
                let rowDate = (getDateCell "Timestamp" r).Date
                let matchesTime =
                    match timeFilter with
                    | "Today" -> rowDate = today
                    | "Yesterday" -> rowDate = today.AddDays(-1.0)
                    | "7d" -> rowDate >= today.AddDays(-7.0)
                    | _ -> true

                // 3. Severity Filter
                let sev = getStringCell "Severity" r
                let isErr = sev.Equals("Error", StringComparison.OrdinalIgnoreCase) || sev.Equals("Errors", StringComparison.OrdinalIgnoreCase) || sev.Equals("Critical", StringComparison.OrdinalIgnoreCase)
                let isWarn = sev.Equals("Warning", StringComparison.OrdinalIgnoreCase) || sev.Equals("Warn", StringComparison.OrdinalIgnoreCase)
                let matchesSeverity =
                    match severityFilter with
                    | "Errors" -> isErr
                    | "Warnings" -> isErr || isWarn
                    | _ -> true

                // 4. Search Filter
                let matchesSearch =
                    if String.IsNullOrWhiteSpace searchFilter then true
                    else
                        let cid = getStringCell "CorrelationId" r
                        let agg = getStringCell "AggregateId" r
                        let cmd = getStringCell "CommandType" r
                        let msg = getStringCell "Message" r
                        cid.Contains(searchFilter, StringComparison.OrdinalIgnoreCase)
                        || agg.Contains(searchFilter, StringComparison.OrdinalIgnoreCase)
                        || cmd.Contains(searchFilter, StringComparison.OrdinalIgnoreCase)
                        || msg.Contains(searchFilter, StringComparison.OrdinalIgnoreCase)

                matchesDomain && matchesTime && matchesSeverity && matchesSearch)

        // Count Badges per Domain
        let countForBc (bc: string) =
            rows |> List.filter (fun r -> (getStringCell "BoundedContext" r).Equals(bc, StringComparison.OrdinalIgnoreCase)) |> List.length
        let totalCount = rows.Length
        let coreCount = countForBc "Core"
        let demandCount = countForBc "Demand"
        let supplyCount = countForBc "Supply"
        let promiseCount = countForBc "Promise"

        // Row Selection
        let selectedRow =
            ctx.Selection
            |> Option.bind (fun selKey -> rows |> List.tryFind (fun r -> r.Key = selKey))

        let correlationIdOpt =
            selectedRow
            |> Option.bind (fun r ->
                let cid = getStringCell "CorrelationId" r
                if not (String.IsNullOrWhiteSpace cid) then Some cid else None)
            |> Option.orElse ctx.Selection

        // Grid Context with filtered rows
        let gridCtx = { ctx with Data = Tabular filteredRows }

        comp<RadzenSplitter> {
            "Orientation" => Orientation.Horizontal
            "Style" => "width: 100%; height: 100%; min-height: 0;"

            // PANE 1: MASTER TELEMETRY GRID & FILTER TOOLSTRIP (42%)
            comp<RadzenSplitterPane> {
                "Size" => "42%"
                "Min" => "30%"
                "Resizable" => true

                div {
                    attr.style "display: flex; flex-direction: column; width: 100%; height: 100%; min-height: 0; box-sizing: border-box;"

                    // MULTI-DIMENSIONAL FILTER BAR
                    div {
                        attr.style "display: flex; flex-direction: column; gap: 6px; padding: 8px 10px; background: var(--rz-base-100); border-bottom: 1px solid var(--rz-border-color); flex-shrink: 0;"

                        // Row 1: Bounded Context Selector Pills
                        div {
                            attr.style "display: flex; align-items: center; gap: 4px; flex-wrap: wrap;"

                            span {
                                attr.style "font-size: 11px; font-weight: 700; color: var(--rz-text-secondary-color); margin-right: 4px;"
                                "Context:"
                            }

                            let domainPill (label: string) (code: string) (count: int) =
                                let isSelected = domainFilter.Equals(code, StringComparison.OrdinalIgnoreCase)
                                button {
                                    attr.``type`` "button"
                                    attr.style (
                                        if isSelected then
                                            "border: none; border-radius: 12px; background: var(--rz-primary); color: white; padding: 2px 9px; font-size: 11px; font-weight: 700; cursor: pointer;"
                                        else
                                            "border: 1px solid var(--rz-border-color); border-radius: 12px; background: var(--rz-base-background-color); color: var(--rz-text-color); padding: 2px 9px; font-size: 11px; font-weight: 500; cursor: pointer;"
                                    )
                                    attr.callback "onclick" (fun _ -> setParam "FilterDomain" code)
                                    sprintf "%s (%d)" label count
                                }

                            domainPill "All" "All" totalCount
                            domainPill "Demand" "Demand" demandCount
                            domainPill "Supply" "Supply" supplyCount
                            domainPill "Promise" "Promise" promiseCount
                            domainPill "Core" "Core" coreCount
                        }

                        // Row 2: Time Window, Severity & Live Search
                        div {
                            attr.style "display: flex; align-items: center; gap: 6px; flex-wrap: wrap;"

                            span {
                                attr.style "font-size: 11px; font-weight: 700; color: var(--rz-text-secondary-color); margin-right: 2px;"
                                "Time:"
                            }

                            let timePill (label: string) (code: string) =
                                let isSelected = timeFilter.Equals(code, StringComparison.OrdinalIgnoreCase)
                                button {
                                    attr.``type`` "button"
                                    attr.style (
                                        if isSelected then
                                            "border: none; border-radius: 10px; background: var(--rz-secondary); color: white; padding: 1px 7px; font-size: 10px; font-weight: 700; cursor: pointer;"
                                        else
                                            "border: 1px solid var(--rz-border-color); border-radius: 10px; background: transparent; color: var(--rz-text-secondary-color); padding: 1px 7px; font-size: 10px; font-weight: 500; cursor: pointer;"
                                    )
                                    attr.callback "onclick" (fun _ -> setParam "FilterTime" code)
                                    label
                                }

                            timePill "All Time" "All"
                            timePill "Today" "Today"
                            timePill "Yesterday" "Yesterday"
                            timePill "7 Days" "7d"

                            span { attr.style "width: 1px; height: 16px; background: var(--rz-border-color); margin: 0 4px;" }

                            let sevPill (label: string) (code: string) =
                                let isSelected = severityFilter.Equals(code, StringComparison.OrdinalIgnoreCase)
                                button {
                                    attr.``type`` "button"
                                    attr.style (
                                        if isSelected then
                                            "border: none; border-radius: 10px; background: var(--rz-danger); color: white; padding: 1px 7px; font-size: 10px; font-weight: 700; cursor: pointer;"
                                        else
                                            "border: 1px solid var(--rz-border-color); border-radius: 10px; background: transparent; color: var(--rz-text-secondary-color); padding: 1px 7px; font-size: 10px; font-weight: 500; cursor: pointer;"
                                    )
                                    attr.callback "onclick" (fun _ -> setParam "FilterSeverity" code)
                                    label
                                }

                            sevPill "All" "All"
                            sevPill "Errors Only" "Errors"

                            div {
                                attr.style "margin-left: auto; display: flex; align-items: center; gap: 4px;"

                                input {
                                    attr.``type`` "text"
                                    attr.placeholder "Filter text..."
                                    attr.value searchFilter
                                    attr.style "border: 1px solid var(--rz-border-color); border-radius: 4px; padding: 2px 6px; font-size: 11px; width: 110px; background: var(--rz-base-background-color); color: var(--rz-text-color);"
                                    on.input (fun (e: ChangeEventArgs) -> setParam "FilterSearch" (string e.Value))
                                }

                                if domainFilter <> "All" || timeFilter <> "All" || severityFilter <> "All" || not (String.IsNullOrWhiteSpace searchFilter) then
                                    button {
                                        attr.``type`` "button"
                                        attr.style "background: transparent; border: none; color: var(--rz-primary); font-size: 11px; cursor: pointer; text-decoration: underline; padding: 0 2px;"
                                        attr.callback "onclick" (fun _ -> setParams [ ("FilterDomain", "All"); ("FilterTime", "All"); ("FilterSeverity", "All"); ("FilterSearch", "") ])
                                        "Reset"
                                    }
                            }
                        }
                    }

                    // MASTER GRID (Receiving filtered dataset)
                    div {
                        attr.style "flex: 1; min-height: 0; width: 100%; height: 100%; overflow: hidden;"
                        UniversalEnterpriseGrid.render gridCtx dispatch
                    }
                }
            }

            // PANE 2: GRAPHICAL DETAIL STUDIO (58%)
            comp<RadzenSplitterPane> {
                "Size" => "58%"
                "Min" => "35%"
                "Resizable" => true

                match correlationIdOpt with
                | None ->
                    div {
                        attr.style "display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; color: var(--rz-text-secondary-color); gap: 1rem; padding: 2rem; text-align: center;"
                        span {
                            attr.``class`` "rzi"
                            attr.style "font-size: 3.5rem; color: var(--rz-secondary-light);"
                            "account_tree"
                        }
                        div {
                            attr.style "font-size: 16px; font-weight: 700; color: var(--rz-text-color);"
                            "Traceability & Investigation Studio"
                        }
                        div {
                            attr.style "font-size: 13px; max-width: 360px; line-height: 1.5;"
                            "Select any telemetry row on the left to inspect its cross-context message flow, stage latency waterfall, and policy evaluation evidence."
                        }
                    }

                | Some corrId ->
                    let relatedRows =
                        let matching =
                            rows
                            |> List.filter (fun r ->
                                let cid = getStringCell "CorrelationId" r
                                cid = corrId)
                        if List.isEmpty matching then
                            match selectedRow with
                            | Some r -> [ r ]
                            | None -> []
                        else
                            matching

                    let events: TelemetryItemDto list =
                        relatedRows
                        |> List.map (fun r ->
                            {
                                EventId = r.Key
                                Timestamp = getDateCell "Timestamp" r
                                Severity = getStringCell "Severity" r
                                BoundedContext = getStringCell "BoundedContext" r
                                AggregateType = getStringCell "AggregateType" r
                                AggregateId = getStringCell "AggregateId" r
                                CommandType = getStringCell "CommandType" r
                                Message = getStringCell "Message" r
                                CorrelationId = corrId
                                CausationId = getStringCell "CausationId" r
                            })
                        |> List.sortBy (fun e -> e.Timestamp)

                    let minTime = if List.isEmpty events then DateTimeOffset.UtcNow else events.Head.Timestamp
                    let maxTime = if List.isEmpty events then DateTimeOffset.UtcNow else (List.last events).Timestamp
                    let totalDuration = max 1.0 (maxTime - minTime).TotalMilliseconds

                    let contextsInvolved =
                        events
                        |> List.map (fun e -> e.BoundedContext)
                        |> List.filter (not << String.IsNullOrWhiteSpace)
                        |> List.distinct

                    let hasErrors =
                        events |> List.exists (fun e ->
                            e.Severity.Equals("Error", StringComparison.OrdinalIgnoreCase)
                            || e.Severity.Equals("Errors", StringComparison.OrdinalIgnoreCase)
                            || e.Severity.Equals("Critical", StringComparison.OrdinalIgnoreCase))

                    let sampleDecisions: DecisionDossierDto list =
                        [
                            {
                                DecisionId = sprintf "DEC-%s" corrId
                                CapabilityId = if List.isEmpty contextsInvolved then "Promise" else contextsInvolved.Head
                                PolicyId = "PO-P-005-ALLOCATION"
                                PolicyVersion = "2.1"
                                Outcome = if hasErrors then "Quarantined" else "Accepted"
                                RulesEvaluated =
                                    [
                                        { RuleId = "BR-P-021-CUSTOMER-ELIGIBILITY"; Passed = true; ReasonCode = "CREDIT_VALID"; Evidence = [ "Credit limit $500,000"; "Active customer standing" ] }
                                        { RuleId = "BR-P-024-CHANNEL-QUOTA"; Passed = not hasErrors; ReasonCode = (if hasErrors then "QUOTA_EXHAUSTED" else "QUOTA_AVAILABLE"); Evidence = [ "Allocated: 2,500 EA"; "Requested: 100 EA" ] }
                                        { RuleId = "BR-P-048-REV0-LOCK"; Passed = true; ReasonCode = "PERMANENCE_LOCKED"; Evidence = [ "Revision 0 baseline established" ] }
                                    ]
                                RationaleSummary =
                                    if hasErrors then
                                        "Channel quota ceiling reached for current planning horizon. Demand routed to overflow review queue."
                                    else
                                        "Automated multi-option allocation satisfied all operational invariants and customer SLA target."
                                Alternatives = [ "Option B (Consolidated Delivery)", "Feasible at Day+14"; "Option C (Product Substitution)", "Feasible (+3% cost)" ]
                            }
                        ]

                    let dossier: TraceDossierDto =
                        {
                            CorrelationId = corrId
                            RootCommandType =
                                events
                                |> List.tryHead
                                |> Option.map (fun e -> if String.IsNullOrWhiteSpace e.CommandType then e.Message else e.CommandType)
                                |> Option.defaultValue "ProcessTransaction"
                            BoundedContextsInvolved = contextsInvolved
                            TotalDurationMs = totalDuration
                            EventCount = events.Length
                            Events = events
                            Decisions = sampleDecisions
                            Transitions = []
                            StageTimings =
                                [
                                    { StageName = "Ingest"; DurationMs = Math.Max(2.0, totalDuration * 0.15) }
                                    { StageName = "ValidateAndDecide"; DurationMs = Math.Max(5.0, totalDuration * 0.45) }
                                    { StageName = "PersistAndPublish"; DurationMs = Math.Max(3.0, totalDuration * 0.40) }
                                ]
                        }

                    let canvasNodes, _, _ = TraceCanvasLayout.computeLayout dossier
                    let activeNode =
                        selectedNodeId
                        |> Option.bind (fun nid -> canvasNodes |> List.tryFind (fun n -> n.NodeId = nid))
                        |> Option.orElse (canvasNodes |> List.tryHead)

                    div {
                        attr.style "display: flex; flex-direction: column; width: 100%; height: 100%; min-height: 0; background: var(--rz-base-background-color);"

                        // Studio Header Bar
                        div {
                            attr.style "display: flex; align-items: center; justify-content: space-between; padding: 0.6rem 1rem; border-bottom: 1px solid var(--rz-border-color); background: var(--rz-base-100); flex-shrink: 0;"

                            div {
                                attr.style "display: flex; align-items: center; gap: 0.75rem;"
                                span {
                                    attr.``class`` "rzi"
                                    attr.style "color: var(--rz-primary); font-size: 1.4rem;"
                                    "timeline"
                                }
                                div {
                                    div {
                                        attr.style "font-weight: 700; font-size: 13px; color: var(--rz-text-color);"
                                        sprintf "Trace Dossier: %s" corrId
                                    }
                                    div {
                                        attr.style "font-size: 11px; color: var(--rz-text-secondary-color);"
                                        sprintf "%d events · %.1fms total · Contexts: %s" events.Length totalDuration (String.concat ", " contextsInvolved)
                                    }
                                }
                            }

                            div {
                                attr.style "display: flex; align-items: center; gap: 0.5rem;"

                                // Zoom Controls (Active in Flow Tab)
                                if activeTab = TraceDetailTab.FlowCanvasTab then
                                    div {
                                        attr.style "display: flex; align-items: center; border: 1px solid var(--rz-border-color); border-radius: 4px; overflow: hidden; margin-right: 6px;"

                                        button {
                                            attr.``type`` "button"
                                            attr.title "Zoom Out"
                                            attr.style "background: var(--rz-base-background-color); border: none; padding: 2px 7px; cursor: pointer; font-size: 12px; font-weight: 700;"
                                            attr.callback "onclick" (fun _ -> setParam "StudioZoomLevel" (sprintf "%.2f" (max 0.6 (zoomLevel - 0.15))))
                                            "−"
                                        }

                                        span {
                                            attr.style "padding: 2px 8px; font-size: 11px; font-weight: 600; color: var(--rz-text-secondary-color); background: var(--rz-base-100);"
                                            sprintf "%.0f%%" (zoomLevel * 100.0)
                                        }

                                        button {
                                            attr.``type`` "button"
                                            attr.title "Zoom In"
                                            attr.style "background: var(--rz-base-background-color); border: none; padding: 2px 7px; cursor: pointer; font-size: 12px; font-weight: 700;"
                                            attr.callback "onclick" (fun _ -> setParam "StudioZoomLevel" (sprintf "%.2f" (min 2.0 (zoomLevel + 0.15))))
                                            "+"
                                        }

                                        button {
                                            attr.``type`` "button"
                                            attr.title "Reset Zoom"
                                            attr.style "background: var(--rz-base-background-color); border-left: 1px solid var(--rz-border-color); border-top: none; border-bottom: none; border-right: none; padding: 2px 6px; cursor: pointer; font-size: 10px; font-weight: 600; color: var(--rz-primary);"
                                            attr.callback "onclick" (fun _ -> setParam "StudioZoomLevel" "1.0")
                                            "100%"
                                        }
                                    }

                                // Tab Selector Buttons
                                Rz.button("Flow", onClick = (fun _ -> setTab TraceDetailTab.FlowCanvasTab), style = (if activeTab = TraceDetailTab.FlowCanvasTab then ButtonStyle.Primary else ButtonStyle.Light), icon = "alt_route")
                                Rz.button("Waterfall", onClick = (fun _ -> setTab TraceDetailTab.WaterfallTab), style = (if activeTab = TraceDetailTab.WaterfallTab then ButtonStyle.Primary else ButtonStyle.Light), icon = "view_timeline")
                                Rz.button("Rules", onClick = (fun _ -> setTab TraceDetailTab.RuleInspectorTab), style = (if activeTab = TraceDetailTab.RuleInspectorTab then ButtonStyle.Primary else ButtonStyle.Light), icon = "gavel")
                                Rz.button("Raw", onClick = (fun _ -> setTab TraceDetailTab.RawDossierTab), style = (if activeTab = TraceDetailTab.RawDossierTab then ButtonStyle.Primary else ButtonStyle.Light), icon = "code")

                                Rz.iconButton("file_download", onClick = (fun _ -> dispatch (ActionRequested(ActionDefId.literal "traceability.exportTrace", Some corrId, NoPayload))), tooltip = "Export Trace Log")
                            }
                        }

                        // Perspective Body
                        div {
                            attr.style "flex: 1; min-height: 0; display: flex; flex-direction: column; overflow: hidden;"

                            match activeTab with
                            | TraceDetailTab.FlowCanvasTab ->
                                div {
                                    attr.style "flex: 1; min-height: 0; display: flex; flex-direction: column; width: 100%; height: 100%; overflow: hidden;"

                                    // Canvas Upper Area
                                    div {
                                        attr.style "flex: 1; min-height: 280px; overflow: hidden;"
                                        TraceCanvasRenderer.renderSvgCanvas dossier zoomLevel selectedNodeId (fun nid -> setParam "SelectedCanvasNodeId" nid)
                                    }

                                    // Interactive Dataflow & I/O Bottom Inspector
                                    match activeNode with
                                    | Some node ->
                                        TraceNodeInspectorRenderer.renderNodeInspector node dossier (fun () -> setParam "SelectedCanvasNodeId" "")
                                    | None ->
                                        empty ()
                                }
                            | TraceDetailTab.WaterfallTab ->
                                TraceWaterfallRenderer.renderWaterfall dossier
                            | TraceDetailTab.RuleInspectorTab ->
                                TraceRuleInspectorRenderer.renderRuleInspector dossier
                            | TraceDetailTab.RawDossierTab ->
                                div {
                                    attr.style "padding: 1rem; height: 100%; overflow: auto; box-sizing: border-box;"
                                    comp<RadzenCard> {
                                        attr.style "padding: 1rem; border-radius: 8px; font-family: Consolas, monospace; font-size: 12px; background: var(--rz-base-100);"
                                        let jsonStr = JsonSerializer.Serialize(dossier, JsonSerializerOptions(WriteIndented = true))
                                        pre { attr.style "margin: 0; white-space: pre-wrap;"; jsonStr }
                                    }
                                }
                            | _ ->
                                TraceCanvasRenderer.renderSvgCanvas dossier zoomLevel selectedNodeId (fun nid -> setParam "SelectedCanvasNodeId" nid)
                        }
                    }
            }
        }
