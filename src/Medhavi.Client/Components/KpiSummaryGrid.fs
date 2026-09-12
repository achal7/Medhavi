module Medhavi.Client.Components.KpiSummaryGrid

open Bolero
open Bolero.Html
open Medhavi.Client
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Geometry

let definitionId = ComponentDefId.literal "kpi-summary-grid"
let storeId = DataStoreId.literal "demo-kpi-store"

let definition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Executive KPI Summary"
      Icon = "speed"
      Category = ComponentCategory.DataDisplay
      RequiredSchemaCapabilities = [ "Kpi" ]
      DefaultGridSpan = GridSpan.literal 12 3
      DefaultSchemaId = Some KpiSchema.schemaId
      DefaultStoreId = Some storeId
      IsResizable = true
      IsDraggable = true
      SelectionMode = SelectionMode.NoSelection }

let demoMetrics: KpiMetric list =
    [ { Id = "kpi-otif"
        Title = "On-Time In-Full (OTIF)"
        Domain = "Demand & Promise"
        Value = "96.2"
        Unit = Some "%"
        Target = Some "95.0%"
        ProgressPct = Some 96.2
        TrendDirection = Up
        TrendValue = "+1.8%"
        IsTrendFavorable = true
        Health = Healthy
        Icon = "task_alt"
        Description = "184 of 191 orders on schedule"
        DrillDownViewId = Some "view-demand-workbench" }

      { Id = "kpi-var"
        Title = "Revenue at Risk"
        Domain = "Risk & Exceptions"
        Value = "$342.5"
        Unit = Some "k"
        Target = Some "$0.0k"
        ProgressPct = Some 68.5
        TrendDirection = Up
        TrendValue = "+$85k"
        IsTrendFavorable = false
        Health = Critical
        Icon = "warning"
        Description = "4 customer shortages active"
        DrillDownViewId = Some "view-demand-supply-pegging-split" }

      { Id = "kpi-dos"
        Title = "Days of Supply (DOS)"
        Domain = "Supply & Stock"
        Value = "24.5"
        Unit = Some "Days"
        Target = Some "25.0 Days"
        ProgressPct = Some 98.0
        TrendDirection = Down
        TrendValue = "-2.1d"
        IsTrendFavorable = true
        Health = Healthy
        Icon = "inventory_2"
        Description = "Turnover 14.8x annual run-rate"
        DrillDownViewId = Some "view-supply-workbench" }

      { Id = "kpi-cap"
        Title = "Bottleneck Resource Load"
        Domain = "Capacity & Mfg"
        Value = "92.4"
        Unit = Some "%"
        Target = Some "85.0%"
        ProgressPct = Some 92.4
        TrendDirection = Up
        TrendValue = "+4.2%"
        IsTrendFavorable = false
        Health = Warning
        Icon = "precision_manufacturing"
        Description = "Berlin DC Line 3 near capacity"
        DrillDownViewId = None } ]

let render (context: ComponentRenderContext) (dispatch: ComponentEvent -> unit) : Node =
    let onCardClick (metricId: string) = dispatch(SelectionChanged(Some metricId))

    div {
        attr.style "width: 100%; height: 100%; box-sizing: border-box;"
        KpiCard.renderGrid demoMetrics (Some onCardClick)
    }

let registration: ComponentRegistration =
    { Definition = definition
      Runtime = { Render = render } }
