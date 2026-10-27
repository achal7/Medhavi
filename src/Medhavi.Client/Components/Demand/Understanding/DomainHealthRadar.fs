module Medhavi.Client.Components.Demand.Understanding.DomainHealthRadar

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
open Medhavi.Client.ViewComposition.Data

let definitionId = ComponentDefId.literal "domain-health-radar"
let storeId = DataStoreId.literal "demo-domain-health-store"

let store: DataStore =
    {
        Id = storeId
        Fetch =
            fun (ctx: ApplicationContext) ->
                async {
                    match ctx.Resolve<Medhavi.Contracts.Demand.DemandUnderstandingQueries>() with
                    | Some queries ->
                        let! _ = queries.GetAll() |> Async.AwaitTask
                        return NoData
                    | None -> return NoData
                }
    }

let definition: ComponentDefinition =
    {
        Id = definitionId
        DisplayName = "APS Domain Intelligence Radar"
        Icon = "radar"
        Category = ComponentCategory.Visualization
        RequiredSchemaCapabilities = [ "Kpi" ]
        DefaultGridSpan = GridSpan.literal 6 5
        DefaultSchemaId = Some KpiSchema.schemaId
        DefaultStoreId = Some storeId
        IsResizable = true
        IsDraggable = true
        SelectionMode = SelectionMode.NoSelection
    }

type DomainHealthItem =
    {
        DomainName: string
        Icon: string
        ScorePct: float
        StatusText: string
        Health: KpiHealth
        KeyMetric: string
        SubMetric: string
        ActionLabel: string
    }

let domainItems: DomainHealthItem list =
    [
        {
            DomainName = "Demand Intelligence"
            Icon = "show_chart"
            ScorePct = 94.2
            StatusText = "Stable Forecast"
            Health = KpiHealth.Healthy
            KeyMetric = "MAPE: 5.8%"
            SubMetric = "Bias: +1.1% · WMAPE: 4.9%"
            ActionLabel = "View Demand"
        }

        {
            DomainName = "Supply Intelligence"
            Icon = "local_shipping"
            ScorePct = 82.5
            StatusText = "3 POs Delayed"
            Health = Warning
            KeyMetric = "Adherence: 89.5%"
            SubMetric = "3 POs late · Risk: $180k"
            ActionLabel = "Expedite POs"
        }

        {
            DomainName = "Promise Intelligence"
            Icon = "handshake"
            ScorePct = 98.6
            StatusText = "Optimal Service"
            Health = Healthy
            KeyMetric = "ATP Latency: 112ms"
            SubMetric = "Reliability: 99.1%"
            ActionLabel = "View Orders"
        }

        {
            DomainName = "Capacity Intelligence"
            Icon = "precision_manufacturing"
            ScorePct = 78.4
            StatusText = "1 Bottleneck Active"
            Health = Warning
            KeyMetric = "Peak Load: 92.4%"
            SubMetric = "Line 3 Overtime: 6.2%"
            ActionLabel = "Reallocate Shift"
        }

        {
            DomainName = "Scenario Intelligence"
            Icon = "science"
            ScorePct = 91.0
            StatusText = "2 What-Ifs Active"
            Health = Healthy
            KeyMetric = "Scenario Delta: +$42k"
            SubMetric = "Best Plan: Supplier Delay Sim"
            ActionLabel = "Compare Plans"
        }
    ]

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
    {
        Definition = definition
        Runtime = { Render = render }
    }
