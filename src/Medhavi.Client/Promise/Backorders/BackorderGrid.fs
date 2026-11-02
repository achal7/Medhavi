module Medhavi.Client.Promise.Backorders.BackorderGrid

open System
open Medhavi.Contracts.Promise
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

module BackorderActions =

    let enqueue: ActionDescriptor =
        { Id = ActionDefId.literal "backorder.enqueue"
          DisplayName = "Enqueue Backorder Line"
          Icon = "queue"
          Scope = GlobalScope
          ExecutionMode = Parameterized("enqueue-backorder-form", "Enqueue Unfulfilled Line into Backorder Queue")
          RequiredRoles = [ "Planner"; "CustomerService" ]
          IsPrimary = true }

    let resolve: ActionDescriptor =
        { Id = ActionDefId.literal "backorder.resolve"
          DisplayName = "Match Replenishment Supply"
          Icon = "done_all"
          Scope = SingleEntityScope
          ExecutionMode = Parameterized("resolve-backorder-form", "Link Inbound Supply & Resolve Backorder")
          RequiredRoles = [ "Planner"; "InventoryManager" ]
          IsPrimary = false }

    let repromise: ActionDescriptor =
        { Id = ActionDefId.literal "backorder.repromise"
          DisplayName = "Autonomous Re-Promise"
          Icon = "published_with_changes"
          Scope = SingleEntityScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "CustomerService" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ enqueue; resolve; repromise ]

let schemaId = SchemaDefId.literal "backorder-schema"
let storeId = DataStoreId.literal "backorder-store"
let definitionId = ComponentDefId.literal "backorder-grid"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Backorders & Recovery Queue (SE-P-040)"
      Capabilities = [ "Tabular" ]
      SupportedActions = BackorderActions.all |> List.map (fun a -> a.Id)
      Actions = BackorderActions.all
      Columns =
        [ { ColumnId = "BackorderId"
            PropertyName = Some "BackorderId"
            Title = "Backorder ID"
            DefaultOrder = 0
            DefaultWidthInPixels = Some 160
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
          { ColumnId = "OrderLineId"
            PropertyName = Some "OrderLineId"
            Title = "Order Line"
            DefaultOrder = 1
            DefaultWidthInPixels = Some 140
            DefaultVisible = true
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
            FormatString = None }
          { ColumnId = "CustomerId"
            PropertyName = Some "CustomerId"
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
            Title = "Item SKU"
            DefaultOrder = 3
            DefaultWidthInPixels = Some 140
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
          { ColumnId = "BackorderedQuantity"
            PropertyName = Some "BackorderedQuantity"
            Title = "Backorder Qty"
            DefaultOrder = 5
            DefaultWidthInPixels = Some 130
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
            FormatString = Some "#,##0.00" }
          { ColumnId = "EnqueuedTime"
            PropertyName = Some "EnqueuedTime"
            Title = "Enqueued At"
            DefaultOrder = 6
            DefaultWidthInPixels = Some 150
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
            FormatString = Some "yyyy-MM-dd HH:mm" }
          { ColumnId = "EstimatedReplenishmentDate"
            PropertyName = Some "EstimatedReplenishmentDate"
            Title = "Replenishment ETA"
            DefaultOrder = 7
            DefaultWidthInPixels = Some 150
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
          { ColumnId = "LifecycleState"
            PropertyName = Some "LifecycleState"
            Title = "State"
            DefaultOrder = 8
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = Some "StatusBadge"
            Alignment = Center
            CellType = TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "DecisionTraceability"
            PropertyName = Some "DecisionTraceability"
            Title = "Decision Trace"
            DefaultOrder = 9
            DefaultWidthInPixels = Some 140
            DefaultVisible = true
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
            { Field = "EnqueuedTime"
              Direction = Ascending
              Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<BackorderRecordDto> =
    match ProjectionCompiler.compile<BackorderRecordDto> schema with
    | Valid a -> a
    | Invalid errs ->
        let msg = errs |> String.concat "; "
        invalidOp $"BackorderGrid compilation failed: {msg}"

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Backorders & Recovery Queue"
      Icon = "queue"
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

