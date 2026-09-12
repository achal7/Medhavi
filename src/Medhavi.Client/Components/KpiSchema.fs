module Medhavi.Client.Components.KpiSchema

open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Context

let drillDown: ActionDescriptor =
    { Id = ActionDefId.literal "kpi.drill_down"
      DisplayName = "Drill Down"
      Icon = "arrow_forward"
      Scope = SingleEntityScope
      ExecutionMode = Direct
      RequiredRoles = [ "Planner"; "Executive" ]
      IsPrimary = true }

let investigate: ActionDescriptor =
    { Id = ActionDefId.literal "kpi.investigate"
      DisplayName = "Investigate Root Cause"
      Icon = "troubleshoot"
      Scope = SingleEntityScope
      ExecutionMode = Direct
      RequiredRoles = [ "Planner"; "Executive" ]
      IsPrimary = false }

let export: ActionDescriptor =
    { Id = ActionDefId.literal "kpi.export"
      DisplayName = "Export Report"
      Icon = "download"
      Scope = GlobalScope
      ExecutionMode = Direct
      RequiredRoles = [ "Planner"; "Executive" ]
      IsPrimary = false }

let all: ActionDescriptor list = [ drillDown; investigate; export ]

let schemaId = SchemaDefId.literal "kpi-metrics-schema"

let definition: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Executive KPI Metrics Schema"
      Capabilities = [ "Kpi" ]
      SupportedActions = all |> List.map(fun a -> a.Id)
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
            UserConfigurable = true }
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
            UserConfigurable = true }
          { ColumnId = "Value"
            PropertyName = Some "Value"
            Title = "Current Value"
            DefaultOrder = 2
            DefaultWidthInPixels = Some 110
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
            UserConfigurable = true }
          { ColumnId = "Target"
            PropertyName = Some "Target"
            Title = "Benchmark / Target"
            DefaultOrder = 3
            DefaultWidthInPixels = Some 120
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
            UserConfigurable = true }
          { ColumnId = "Trend"
            PropertyName = Some "TrendValue"
            Title = "Trend vs Prior"
            DefaultOrder = 4
            DefaultWidthInPixels = Some 110
            DefaultVisible = true
            DefaultPinned = false
            Sortable = false
            FilterValueKind = Some Text
            Filterable = true
            Groupable = false
            RenderHint = None
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "Health"
            PropertyName = Some "Health"
            Title = "Status"
            DefaultOrder = 5
            DefaultWidthInPixels = Some 100
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
            UserConfigurable = true } ]
      DefaultSort =
        Some(
            { Field = "Domain"
              Direction = Ascending
              Priority = 1 }
            : SortCriterion
        )
      DefaultFilters = [] }
