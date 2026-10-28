module Medhavi.Client.Supply.SupplyOrders.SupplyGrid

open System
open Medhavi.Contracts.Supply
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

/// Declarative metadata descriptors for Supply Order actions.
module SupplyOrderActions =

    let generatePlan: ActionDescriptor =
        { Id = ActionDefId.literal "supply.generate_plan"
          DisplayName = "Generate Supply Plan"
          Icon = "auto_mode"
          Scope = GlobalScope
          ExecutionMode = RequiresConfirmation "Generate supply plan balancing supply and demand across active scope?"
          RequiredRoles = [ "Planner" ]
          IsPrimary = true }

    let evaluatePlan: ActionDescriptor =
        { Id = ActionDefId.literal "supply.evaluate_plan"
          DisplayName = "Evaluate Supply Plan"
          Icon = "rule"
          Scope = GlobalScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner" ]
          IsPrimary = false }

    let publishPlan: ActionDescriptor =
        { Id = ActionDefId.literal "supply.publish_plan"
          DisplayName = "Publish Supply Plan"
          Icon = "publish"
          Scope = GlobalScope
          ExecutionMode = RequiresConfirmation "Publish supply plan baseline to enterprise picture?"
          RequiredRoles = [ "Planner"; "SupplyManager" ]
          IsPrimary = false }

    let explainDelay: ActionDescriptor =
        { Id = ActionDefId.literal "supply.explain"
          DisplayName = "Explain Feasibility"
          Icon = "auto_awesome"
          Scope = SingleEntityScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "Viewer" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ generatePlan; evaluatePlan; publishPlan; explainDelay ]

let generatePlan = SupplyOrderActions.generatePlan
let evaluatePlan = SupplyOrderActions.evaluatePlan
let publishPlan = SupplyOrderActions.publishPlan
let explainDelay = SupplyOrderActions.explainDelay
let allActions = SupplyOrderActions.all

let storeId = DataStoreId.literal "supply-store"
let schemaId = SchemaDefId.literal "supply-grid-schema"
let definitionId = ComponentDefId.literal "supply-grid"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Planned Order Schedule"
      Capabilities = [ "Tabular" ]
      SupportedActions = SupplyOrderActions.all |> List.map (fun a -> a.Id)
      Actions = SupplyOrderActions.all
      Columns =
        [ { ColumnId = "SupplyType"
            PropertyName = Some "SupplyType"
            Title = "Type"
            DefaultOrder = 0
            DefaultWidthInPixels = Some 110
            DefaultVisible = true
            DefaultPinned = true
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = Some "StatusBadge"
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "OrderId"
            PropertyName = Some "OrderId"
            Title = "Planned Order ID"
            DefaultOrder = 1
            DefaultWidthInPixels = Some 130
            DefaultVisible = true
            DefaultPinned = true
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = false
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = true
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "Item"
            PropertyName = Some "Item"
            Title = "Item / SKU"
            DefaultOrder = 2
            DefaultWidthInPixels = Some 140
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "Location"
            PropertyName = Some "Location"
            Title = "Location"
            DefaultOrder = 3
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "Quantity"
            PropertyName = Some "Quantity"
            Title = "Quantity"
            DefaultOrder = 4
            DefaultWidthInPixels = Some 110
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Number
            Filterable = true
            Groupable = false
            RenderHint = None
            Alignment = Right
            CellType = NumberType
            IsKey = false
            FormatString = Some "N0"
            UserConfigurable = true }
          { ColumnId = "UnitOfMeasure"
            PropertyName = Some "UnitOfMeasure"
            Title = "UoM"
            DefaultOrder = 5
            DefaultWidthInPixels = Some 80
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "ReleaseDate"
            PropertyName = Some "ReleaseDate"
            Title = "Release Date"
            DefaultOrder = 6
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some FilterValueKind.DateTimeOffset
            Filterable = true
            Groupable = false
            RenderHint = None
            Alignment = Left
            CellType = DateTimeType
            IsKey = false
            FormatString = Some "yyyy-MM-dd"
            UserConfigurable = true }
          { ColumnId = "DueDate"
            PropertyName = Some "DueDate"
            Title = "Due Date"
            DefaultOrder = 7
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some FilterValueKind.DateTimeOffset
            Filterable = true
            Groupable = false
            RenderHint = None
            Alignment = Left
            CellType = DateTimeType
            IsKey = false
            FormatString = Some "yyyy-MM-dd"
            UserConfigurable = true }
          { ColumnId = "AssignedResource"
            PropertyName = Some "AssignedResource"
            Title = "Assigned Resource"
            DefaultOrder = 8
            DefaultWidthInPixels = Some 150
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "AssignedSupplier"
            PropertyName = Some "AssignedSupplier"
            Title = "Assigned Supplier"
            DefaultOrder = 9
            DefaultWidthInPixels = Some 150
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "SourceLocation"
            PropertyName = Some "SourceLocation"
            Title = "Source Location"
            DefaultOrder = 10
            DefaultWidthInPixels = Some 120
            DefaultVisible = false
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true } ]
      DefaultSort =
        Some
            { Field = "DueDate"
              Direction = Ascending
              Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<PlannedOrderDto> =
    match ProjectionCompiler.compile<PlannedOrderDto> schema with
    | Valid a -> a
    | Invalid _ -> { ToUniversal = fun _ -> { Key = ""; Cells = Map.empty } }

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Planned Order Schedule"
      Icon = "inventory_2"
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
