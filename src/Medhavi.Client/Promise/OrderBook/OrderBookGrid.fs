module Medhavi.Client.Promise.OrderBook.OrderBookGrid

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

module OrderBookActions =

    let ingestOrder: ActionDescriptor =
        { Id = ActionDefId.literal "order_book.ingest"
          DisplayName = "Ingest Order"
          Icon = "add_shopping_cart"
          Scope = GlobalScope
          ExecutionMode = Parameterized("order-ingest-form", "Ingest Customer Order Intake")
          RequiredRoles = [ "Planner"; "CustomerService"; "SalesLead" ]
          IsPrimary = true }

    let validateLine: ActionDescriptor =
        { Id = ActionDefId.literal "order_book.validate_line"
          DisplayName = "Validate Order Line"
          Icon = "verified"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Run full business rules validation on selected order line?"
          RequiredRoles = [ "Planner"; "CustomerService" ]
          IsPrimary = false }

    let evaluateAtp: ActionDescriptor =
        { Id = ActionDefId.literal "order_book.evaluate_atp"
          DisplayName = "Evaluate Multi-Option ATP"
          Icon = "alt_route"
          Scope = SingleEntityScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "CustomerService" ]
          IsPrimary = false }

    let cancelOrder: ActionDescriptor =
        { Id = ActionDefId.literal "order_book.cancel"
          DisplayName = "Cancel Order"
          Icon = "cancel"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Authorize cancellation of this customer order snapshot?"
          RequiredRoles = [ "Planner"; "CustomerService" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ ingestOrder; validateLine; evaluateAtp; cancelOrder ]

let schemaId = SchemaDefId.literal "order-book-schema"
let storeId = DataStoreId.literal "order-book-store"
let definitionId = ComponentDefId.literal "order-book-grid"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Customer Order Book Snapshots (SE-P-001)"
      Capabilities = [ "Tabular" ]
      SupportedActions = OrderBookActions.all |> List.map (fun a -> a.Id)
      Actions = OrderBookActions.all
      Columns =
        [ { ColumnId = "LifecycleState"
            PropertyName = Some "LifecycleState"
            Title = "Lifecycle State"
            DefaultOrder = 0
            DefaultWidthInPixels = Some 120
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
          { ColumnId = "SnapshotId"
            PropertyName = Some "SnapshotId"
            Title = "Snapshot ID"
            DefaultOrder = 1
            DefaultWidthInPixels = Some 140
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
          { ColumnId = "CustomerOrderIdentifier"
            PropertyName = Some "CustomerOrderIdentifier"
            Title = "Order Ref"
            DefaultOrder = 2
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
          { ColumnId = "OrderChannel"
            PropertyName = Some "OrderChannel"
            Title = "Channel"
            DefaultOrder = 4
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
          { ColumnId = "OrderPriorityClass"
            PropertyName = Some "OrderPriorityClass"
            Title = "Priority Class"
            DefaultOrder = 5
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
          { ColumnId = "ObservationTimestamp"
            PropertyName = Some "ObservationTimestamp"
            Title = "Intake Timestamp"
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
          { ColumnId = "PlanningScopeId"
            PropertyName = Some "PlanningScopeId"
            Title = "Planning Scope"
            DefaultOrder = 7
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
          { ColumnId = "DecisionTraceability"
            PropertyName = Some "DecisionTraceability"
            Title = "Decision Trace"
            DefaultOrder = 8
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
            { Field = "ObservationTimestamp"
              Direction = Descending
              Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<OrderBookSnapshotDto> =
    match ProjectionCompiler.compile<OrderBookSnapshotDto> schema with
    | Valid a -> a
    | Invalid errs ->
        let msg = errs |> String.concat "; "
        invalidOp $"OrderBookGrid compilation failed: {msg}"

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Customer Order Book Snapshots"
      Icon = "receipt_long"
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

