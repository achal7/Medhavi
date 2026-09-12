namespace Medhavi.Client.Components.Demand.Understanding

open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Microsoft.AspNetCore.Components.Web
open Medhavi.Client
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.Components

module DomainHealthRadar =

    let definitionId = ComponentDefId.literal "domain-health-radar"
    let storeId = DataStoreId.literal "demo-domain-health-store"

    let definition: ComponentDefinition =
        { Id = definitionId
          DisplayName = "APS Domain Intelligence Radar"
          Icon = "radar"
          Category = ComponentCategory.Visualization
          RequiredSchemaCapabilities = [ "Kpi" ]
          DefaultGridSpan = GridSpan.literal 6 5
          DefaultSchemaId = Some KpiSchema.schemaId
          DefaultStoreId = Some storeId
          IsResizable = true
          IsDraggable = true
          SelectionMode = SelectionMode.NoSelection }

    type DomainHealthItem =
        { DomainName: string
          Icon: string
          ScorePct: float
          StatusText: string
          Health: KpiHealth
          KeyMetric: string
          SubMetric: string
          ActionLabel: string }

    let domainItems: DomainHealthItem list =
        [ { DomainName = "Demand Intelligence"
            Icon = "show_chart"
            ScorePct = 94.2
            StatusText = "Stable Forecast"
            Health = Healthy
            KeyMetric = "MAPE: 5.8%"
            SubMetric = "Bias: +1.1% · WMAPE: 4.9%"
            ActionLabel = "View Demand" }

          { DomainName = "Supply Intelligence"
            Icon = "local_shipping"
            ScorePct = 82.5
            StatusText = "3 POs Delayed"
            Health = Warning
            KeyMetric = "Adherence: 89.5%"
            SubMetric = "3 POs late · Risk: $180k"
            ActionLabel = "Expedite POs" }

          { DomainName = "Promise Intelligence"
            Icon = "handshake"
            ScorePct = 98.6
            StatusText = "Optimal Service"
            Health = Healthy
            KeyMetric = "ATP Latency: 112ms"
            SubMetric = "Reliability: 99.1%"
            ActionLabel = "View Orders" }

          { DomainName = "Capacity Intelligence"
            Icon = "precision_manufacturing"
            ScorePct = 78.4
            StatusText = "1 Bottleneck Active"
            Health = Warning
            KeyMetric = "Peak Load: 92.4%"
            SubMetric = "Line 3 Overtime: 6.2%"
            ActionLabel = "Reallocate Shift" }

          { DomainName = "Scenario Intelligence"
            Icon = "science"
            ScorePct = 91.0
            StatusText = "2 What-Ifs Active"
            Health = Healthy
            KeyMetric = "Scenario Delta: +$42k"
            SubMetric = "Best Plan: Supplier Delay Sim"
            ActionLabel = "Compare Plans" } ]

    let private healthBadgeStyle (health: KpiHealth) =
        match health with
        | Healthy ->
            "color: var(--rz-success); background: rgba(40, 167, 69, 0.12); border: 1px solid rgba(40, 167, 69, 0.25);"
        | Warning -> "color: #b78103; background: rgba(255, 193, 7, 0.15); border: 1px solid rgba(255, 193, 7, 0.35);"
        | Critical ->
            "color: var(--rz-danger); background: rgba(220, 53, 69, 0.12); border: 1px solid rgba(220, 53, 69, 0.25);"
        | Neutral ->
            "color: var(--rz-info); background: rgba(23, 162, 184, 0.12); border: 1px solid rgba(23, 162, 184, 0.25);"

    let private progressBarColor (health: KpiHealth) =
        match health with
        | Healthy -> "var(--rz-success)"
        | Warning -> "var(--rz-warning)"
        | Critical -> "var(--rz-danger)"
        | Neutral -> "var(--rz-info)"

    let render (context: ComponentRenderContext) (_: ComponentEvent -> unit) : Node =

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

            // Header
            div {
                attr.style
                    "display: flex; align-items: center; justify-content: space-between; margin-bottom: 0.75rem; border-bottom: 1px solid var(--rz-border-color); padding-bottom: 0.5rem;"

                div {
                    attr.style "display: flex; align-items: center; gap: 0.5rem;"

                    span {
                        attr.``class`` "rzi"
                        attr.style "font-size: 1.25rem; color: var(--rz-primary);"
                        "radar"
                    }

                    span {
                        attr.style "font-size: 14px; font-weight: 700; color: var(--rz-text-color);"
                        "APS Intelligence Radar (5 Domains)"
                    }
                }

                span {
                    attr.style "font-size: 11px; font-weight: 600; color: var(--rz-text-secondary-color);"
                    "Live Health Indices"
                }
            }

            // Domain Health Rows
            div {
                attr.style "display: flex; flex-direction: column; gap: 0.75rem; flex: 1;"

                forEach domainItems (fun item ->
                    div {
                        attr.style
                            "display: flex; flex-direction: column; gap: 0.25rem; padding: 0.4rem 0.6rem; border-radius: 6px; background: rgba(0,0,0,0.02);"

                        // Row Top: Name + Score + Status Badge
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

                        // Progress Bar
                        div {
                            attr.style
                                "width: 100%; height: 4px; background: var(--rz-border-color); border-radius: 2px; overflow: hidden;"

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

                        // Row Submetrics
                        div {
                            attr.style
                                "display: flex; justify-content: space-between; font-size: 11px; color: var(--rz-text-secondary-color); margin-top: 1px;"

                            span { item.KeyMetric + " · " + item.SubMetric }
                        }
                    })
            }
        }

    let registration: ComponentRegistration =
        { Definition = definition
          Runtime = { Render = render } }
