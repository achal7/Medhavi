module Medhavi.Client.Demand.Understanding.DomainHealthRadar

open Bolero
open Bolero.Html
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.Registries
open Medhavi.Client.Rendering
open Medhavi.Client.Controls

let definitionId = ComponentDefId.literal "domain-health-radar"
let storeId = DomainHealthStore.storeId
let schemaId = DomainHealthSchema.schemaId

let definition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "APS Domain Intelligence Radar"
      Icon = "radar"
      Category = ComponentCategory.Visualization
      RequiredSchemaCapabilities = [ "Radar"; "Scorecard" ]
      DefaultGridSpan = GridSpan.literal 6 5
      DefaultSchemaId = Some schemaId
      DefaultStoreId = Some storeId
      IsResizable = true
      IsDraggable = true
      SelectionMode = SelectionMode.SingleSelection }

type DomainHealthItem =
    { DomainName: string
      Icon: string
      ScorePct: float
      StatusText: string
      Health: KpiHealth
      KeyMetric: string
      SubMetric: string
      ActionLabel: string
      EntityKey: string }

let private domainIntelligenceItems: DomainHealthItem list =
    [ { DomainName = "Demand Intelligence"
        Icon = "show_chart"
        ScorePct = 94.2
        StatusText = "Stable Forecast"
        Health = KpiHealth.Healthy
        KeyMetric = "MAPE: 5.8%"
        SubMetric = "Bias: +1.1% · WMAPE: 4.9%"
        ActionLabel = "Revise Assessment"
        EntityKey = "DEMAND-SCOPE-GLOBAL" }

      { DomainName = "Supply Intelligence"
        Icon = "local_shipping"
        ScorePct = 82.5
        StatusText = "3 POs Delayed"
        Health = Warning
        KeyMetric = "Adherence: 89.5%"
        SubMetric = "3 POs late · Risk: $180k"
        ActionLabel = "Expedite POs"
        EntityKey = "SUPPLY-SCOPE-GLOBAL" }

      { DomainName = "Promise Intelligence"
        Icon = "handshake"
        ScorePct = 98.6
        StatusText = "Optimal Service"
        Health = Healthy
        KeyMetric = "ATP Latency: 112ms"
        SubMetric = "Reliability: 99.1%"
        ActionLabel = "View Orders"
        EntityKey = "PROMISE-SCOPE-GLOBAL" }

      { DomainName = "Capacity Intelligence"
        Icon = "precision_manufacturing"
        ScorePct = 78.4
        StatusText = "1 Bottleneck Active"
        Health = Warning
        KeyMetric = "Peak Load: 92.4%"
        SubMetric = "Line 3 Overtime: 6.2%"
        ActionLabel = "Reallocate Shift"
        EntityKey = "CAPACITY-SCOPE-GLOBAL" }

      { DomainName = "Scenario Intelligence"
        Icon = "science"
        ScorePct = 91.0
        StatusText = "2 What-Ifs Active"
        Health = Healthy
        KeyMetric = "Scenario Delta: +$42k"
        SubMetric = "Best Plan: Supplier Delay Sim"
        ActionLabel = "Compare Plans"
        EntityKey = "SCENARIO-SCOPE-GLOBAL" } ]

let private healthBadgeStyle (health: KpiHealth) =
    match health with
    | Healthy ->
        "color: var(--rz-success); background: var(--rz-success-lighter); border: 1px solid var(--rz-success-light);"
    | Warning ->
        "color: var(--rz-warning-dark); background: var(--rz-warning-lighter); border: 1px solid var(--rz-warning-light);"
    | Critical ->
        "color: var(--rz-danger); background: var(--rz-danger-lighter); border: 1px solid var(--rz-danger-light);"
    | Neutral -> "color: var(--rz-info); background: var(--rz-info-lighter); border: 1px solid var(--rz-info-light);"

let private progressBarColor (health: KpiHealth) =
    match health with
    | Healthy -> "var(--rz-success)"
    | Warning -> "var(--rz-warning)"
    | Critical -> "var(--rz-danger)"
    | Neutral -> "var(--rz-info)"

let private mapRowToItem (row: UniversalRow) : DomainHealthItem =
    let getCellText colId fallback =
        match Map.tryFind colId row.Cells with
        | Some (TextValue s) -> s
        | Some (NumberValue n) -> string n
        | _ -> fallback

    let scopeId = getCellText "PlanningScopeId" row.Key
    let state = getCellText "State" "Active"
    let version = getCellText "VersionNumber" "1"

    let health =
        match state.ToLowerInvariant() with
        | "published" -> Healthy
        | "draft" -> Warning
        | "superseded" -> Neutral
        | _ -> Healthy

    { DomainName = $"Demand Scope: {scopeId}"
      Icon = "insights"
      ScorePct = 92.0
      StatusText = state
      Health = health
      KeyMetric = $"Version: v{version}"
      SubMetric = $"Scope: {scopeId}"
      ActionLabel = "Explain Scope"
      EntityKey = row.Key }

let render (context: ComponentRenderContext) (dispatch: ComponentEvent -> unit) : Node =
    let items =
        match context.Data with
        | Tabular rows when not rows.IsEmpty ->
            rows |> List.map mapRowToItem
        | _ ->
            domainIntelligenceItems

    div {
        attr.style
            """
            background: var(--rz-base-background-color);
            border: 1px solid var(--rz-border-color);
            border-radius: 8px;
            padding: 1rem 1.25rem;
            display: flex;
            flex-direction: column;
            height: 100%;
            box-sizing: border-box;
            overflow-y: auto;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
            """

        div {
            attr.style "display: flex; flex-direction: column; gap: 0.75rem; flex: 1;"

            forEach items (fun item ->
                let isSelected = context.Selection = Some item.EntityKey

                div {
                    attr.style
                        (if isSelected then
                            "display: flex; flex-direction: column; gap: 0.25rem; padding: 0.5rem 0.6rem; border-radius: 6px; background: var(--rz-primary-lighter); border: 1px solid var(--rz-primary-light); cursor: pointer;"
                         else
                            "display: flex; flex-direction: column; gap: 0.25rem; padding: 0.5rem 0.6rem; border-radius: 6px; background: rgba(0,0,0,0.02); cursor: pointer;")
                    on.click (fun _ -> dispatch (SelectionChanged (Some item.EntityKey)))

                    div {
                        attr.style "display: flex; align-items: center; justify-content: space-between;"

                        div {
                            attr.style "display: flex; align-items: center; gap: 0.4rem;"

                            span {
                                attr.``class`` "rzi"
                                attr.style "font-size: 1rem; color: var(--rz-text-secondary-color);"
                                item.Icon
                            }

                            span {
                                attr.style "font-size: 13px; font-weight: 600; color: var(--rz-text-color);"
                                item.DomainName
                            }
                        }

                        div {
                            attr.style "display: flex; align-items: center; gap: 0.5rem;"

                            span {
                                attr.style "font-size: 12px; font-weight: 700; color: var(--rz-text-color);"
                                sprintf "%.1f%%" item.ScorePct
                            }

                            span {
                                attr.style
                                    $"font-size: 11px; font-weight: 600; padding: 2px 6px; border-radius: 4px; {healthBadgeStyle item.Health}"
                                item.StatusText
                            }
                        }
                    }

                    div {
                        attr.style "width: 100%; height: 4px; background: var(--rz-border-color); border-radius: 2px; overflow: hidden;"

                        div {
                            attr.style
                                $"""
                                width: {item.ScorePct}%%;
                                height: 100%%;
                                background: {progressBarColor item.Health};
                                border-radius: 2px;
                                """
                        }
                    }

                    div {
                        attr.style "display: flex; justify-content: space-between; font-size: 11px; color: var(--rz-text-secondary-color); margin-top: 1px;"

                        span { item.KeyMetric + " · " + item.SubMetric }

                        span {
                            attr.style "font-weight: 600; color: var(--rz-primary); cursor: pointer;"
                            item.ActionLabel
                        }
                    }
                })
        }
    }

let runtime: ComponentRuntime =
    { Render = render }

let registration: ComponentRegistration =
    { Definition = definition
      Runtime = runtime }
