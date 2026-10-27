namespace Medhavi.Client.Core.Demand

open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Context

/// Declarative metadata descriptors for Demand KPI operations.
module DemandKpiActions =

    let drillDown: ActionDescriptor =
        { Id = ActionDefId.literal "demand.kpi.drill_down"
          DisplayName = "Open Demand Orders Workbench"
          Icon = "table_view"
          Scope = GlobalScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "Executive"; "Viewer" ]
          IsPrimary = true }

    let all: ActionDescriptor list = [ drillDown ]

/// Authoritative KPI Metric Schema for Enterprise Demand Performance & Fulfillment Risk.
module DemandKpiSchema =

    let schemaId = SchemaDefId.literal "demand-kpi-schema"

    let definition: SchemaDefinition =
        { Id = schemaId
          DisplayName = "Executive Demand Fulfillment & Risk Scorecard"
          Capabilities = [ "Kpi" ]
          SupportedActions = DemandKpiActions.all |> List.map(fun a -> a.Id)
          Actions = DemandKpiActions.all
          Columns =
            [ { ColumnId = "Domain"
                PropertyName = Some "Domain"
                Title = "Domain"
                DefaultOrder = 0
                DefaultWidthInPixels = Some 120
                DefaultVisible = true
                DefaultPinned = true
                Sortable = true
                Filterable = true
                Groupable = false
                RenderHint = None
                Alignment = TextAlignment.Left
                CellType = CellType.TextType
                IsKey = false
                FormatString = None
                FilterValueKind = Some Text
                UserConfigurable = false }
              { ColumnId = "Title"
                PropertyName = Some "Title"
                Title = "Metric Name"
                DefaultOrder = 1
                DefaultWidthInPixels = Some 180
                DefaultVisible = true
                DefaultPinned = false
                Sortable = true
                FilterValueKind = Some Text
                Filterable = true
                Groupable = false
                RenderHint = None
                Alignment = TextAlignment.Left
                CellType = CellType.TextType
                IsKey = false
                FormatString = None
                UserConfigurable = false }
              { ColumnId = "Value"
                PropertyName = Some "Value"
                Title = "Current Value"
                DefaultOrder = 2
                DefaultWidthInPixels = Some 100
                DefaultVisible = true
                DefaultPinned = false
                Sortable = true
                FilterValueKind = Some Text
                Filterable = false
                Groupable = false
                RenderHint = None
                Alignment = TextAlignment.Right
                CellType = CellType.TextType
                IsKey = false
                FormatString = None
                UserConfigurable = false }
              { ColumnId = "Unit"
                PropertyName = Some "Unit"
                Title = "Unit"
                DefaultOrder = 3
                DefaultWidthInPixels = Some 60
                DefaultVisible = true
                DefaultPinned = false
                Sortable = false
                FilterValueKind = Some Text
                Filterable = false
                Groupable = false
                RenderHint = None
                Alignment = TextAlignment.Left
                CellType = CellType.TextType
                IsKey = false
                FormatString = None
                UserConfigurable = false }
              { ColumnId = "Health"
                PropertyName = Some "Health"
                Title = "Health"
                DefaultOrder = 4
                DefaultWidthInPixels = Some 90
                DefaultVisible = true
                DefaultPinned = false
                Sortable = true
                FilterValueKind = Some Text
                Filterable = true
                Groupable = true
                RenderHint = Some "HealthBadge"
                Alignment = TextAlignment.Center
                CellType = CellType.TextType
                IsKey = false
                FormatString = None
                UserConfigurable = false } ]
          DefaultSort = None
          DefaultFilters = [] }

