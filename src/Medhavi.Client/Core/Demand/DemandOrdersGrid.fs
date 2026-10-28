module Medhavi.Client.Core.Demand.DemandOrdersGrid

open System
open Medhavi.Contracts.Core.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ViewComposition.Context
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering
open Medhavi.Common.Validation
open Medhavi.Client.Components.Shared

/// Declarative metadata descriptors for demand order actions.
module DemandGridActions =

    let satisfy: ActionDescriptor =
        { Id = ActionDefId.literal "demand.satisfy"
          DisplayName = "Satisfy Demand"
          Icon = "check_circle"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Mark this demand order as fulfilled and satisfied?"
          RequiredRoles = [ "Planner"; "LeadPlanner" ]
          IsPrimary = true }

    let cancel: ActionDescriptor =
        { Id = ActionDefId.literal "demand.cancel"
          DisplayName = "Cancel Demand"
          Icon = "cancel"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Cancel this demand order in the enterprise ledger?"
          RequiredRoles = [ "Planner"; "LeadPlanner" ]
          IsPrimary = false }

    let explainShortage: ActionDescriptor =
        { Id = ActionDefId.literal "demand.explain"
          DisplayName = "Explain Shortage"
          Icon = "auto_awesome"
          Scope = SingleEntityScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "Viewer" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ satisfy; cancel; explainShortage ]

let satisfy = DemandGridActions.satisfy
let cancel = DemandGridActions.cancel
let explainShortage = DemandGridActions.explainShortage
let allActions = DemandGridActions.all

let schemaId = SchemaDefId.literal "demand-grid-schema"
let storeId = DataStoreId.literal "demo-demand-store"
let definitionId = ComponentDefId.literal "demand-grid"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Enterprise Demand Orders"
      Capabilities = [ "Tabular" ]
      SupportedActions = DemandGridActions.all |> List.map (fun a -> a.Id)
      Actions = DemandGridActions.all
      Columns =
        [ { ColumnId = "LifecycleState"
            PropertyName = Some "LifecycleState"
            Title = "Status"
            DefaultOrder = 0
            DefaultWidthInPixels = Some 110
            DefaultVisible = true
            DefaultPinned = true
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = Some "StatusBadge"
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "Id"
            PropertyName = Some "Id"
            Title = "Demand ID"
            DefaultOrder = 1
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = true
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = true
            FormatString = None }
          { ColumnId = "Customer"
            PropertyName = Some "Customer"
            Title = "Customer"
            DefaultOrder = 2
            DefaultWidthInPixels = Some 160
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "Item"
            PropertyName = Some "Item"
            Title = "Item / Product"
            DefaultOrder = 3
            DefaultWidthInPixels = Some 160
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "Location"
            PropertyName = Some "Location"
            Title = "Location"
            DefaultOrder = 4
            DefaultWidthInPixels = Some 130
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "NeedWindowLatest"
            PropertyName = Some "NeedWindowLatest"
            Title = "Need Date"
            DefaultOrder = 5
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some FilterValueKind.DateTimeOffset
            UserConfigurable = true
            RenderHint = None
            Alignment = Left
            CellType = DateTimeType
            IsKey = false
            FormatString = Some "yyyy-MM-dd" }
          { ColumnId = "Quantity"
            PropertyName = Some "Quantity"
            Title = "Quantity"
            DefaultOrder = 6
            DefaultWidthInPixels = Some 110
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Number
            UserConfigurable = true
            RenderHint = None
            Alignment = Right
            CellType = NumberType
            IsKey = false
            FormatString = Some "N0" }
          { ColumnId = "DemandOrigin"
            PropertyName = Some "DemandOrigin"
            Title = "Origin"
            DefaultOrder = 7
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "ParentDemand"
            PropertyName = Some "ParentDemand"
            Title = "Parent Demand"
            DefaultOrder = 8
            DefaultWidthInPixels = Some 120
            DefaultVisible = false
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None } ]
      DefaultSort =
        Some
            { Field = "NeedWindowLatest"
              Direction = Ascending
              Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<DemandDto> =
    match ProjectionCompiler.compile<DemandDto> schema with
    | Valid a -> a
    | Invalid errs ->
        let msg = String.concat "; " errs
        invalidOp $"DemandOrdersGrid compilation failed: {msg}"

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Demands"
      Icon = "view_list"
      Category = ComponentCategory.DataDisplay
      RequiredSchemaCapabilities = [ "Tabular" ]
      DefaultGridSpan = GridSpan.literal 12 6
      DefaultSchemaId = Some schemaId
      DefaultStoreId = Some storeId
      IsResizable = true
      IsDraggable = true
      SelectionMode = SingleSelection }

let runtime: ComponentRuntime =
    { Render = UniversalEnterpriseGrid.render }

let registration: ComponentRegistration =
    { Definition = componentDefinition
      Schema = schema
      Runtime = runtime }

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
