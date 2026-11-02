module Medhavi.Client.Promise.Commitments.CommitmentGrid

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

module CommitmentActions =

    let establishCommitment: ActionDescriptor =
        { Id = ActionDefId.literal "commitment.establish"
          DisplayName = "Establish Binding Promise"
          Icon = "handshake"
          Scope = GlobalScope
          ExecutionMode = Parameterized("establish-commitment-form", "Firmly Commit Order Delivery Date & Margin")
          RequiredRoles = [ "Planner"; "CustomerService" ]
          IsPrimary = true }

    let reviseCommitment: ActionDescriptor =
        { Id = ActionDefId.literal "commitment.revise"
          DisplayName = "Revise Commitment Schedule"
          Icon = "edit_calendar"
          Scope = SingleEntityScope
          ExecutionMode = Parameterized("revise-commitment-form", "Adjust Delivery Date / Quantity with Audit Reason")
          RequiredRoles = [ "Planner"; "CustomerService" ]
          IsPrimary = false }

    let confirmFulfillment: ActionDescriptor =
        { Id = ActionDefId.literal "commitment.confirm_pod"
          DisplayName = "Confirm Carrier POD Fulfillment"
          Icon = "local_shipping"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Record proof of delivery for this commitment?"
          RequiredRoles = [ "LogisticsLead"; "CustomerService" ]
          IsPrimary = false }

    let cancelCommitment: ActionDescriptor =
        { Id = ActionDefId.literal "commitment.cancel"
          DisplayName = "De-Commit & Cancel"
          Icon = "cancel"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Authorize cancellation and de-allocation of this commitment?"
          RequiredRoles = [ "Planner"; "CustomerService" ]
          IsPrimary = false }

    let all: ActionDescriptor list =
        [ establishCommitment
          reviseCommitment
          confirmFulfillment
          cancelCommitment ]

let schemaId = SchemaDefId.literal "order-commitment-schema"
let storeId = DataStoreId.literal "order-commitment-store"
let definitionId = ComponentDefId.literal "order-commitment-grid"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Order Commitment Ledgers (SE-P-010)"
      Capabilities = [ "Tabular" ]
      SupportedActions = CommitmentActions.all |> List.map (fun a -> a.Id)
      Actions = CommitmentActions.all
      Columns =
        [ { ColumnId = "CommitmentId"
            PropertyName = Some "CommitmentId"
            Title = "Commitment ID"
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
          { ColumnId = "CustomerOrderIdentifier"
            PropertyName = Some "CustomerOrderIdentifier"
            Title = "Order Ref"
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
          { ColumnId = "OrderLineId"
            PropertyName = Some "OrderLineId"
            Title = "Order Line"
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
          { ColumnId = "Item"
            PropertyName = Some "Item"
            Title = "Item SKU"
            DefaultOrder = 4
            DefaultWidthInPixels = Some 150
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
          { ColumnId = "DestinationLocation"
            PropertyName = Some "DestinationLocation"
            Title = "Destination"
            DefaultOrder = 5
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
          { ColumnId = "CurrentRevisionIndex"
            PropertyName = Some "CurrentRevisionIndex"
            Title = "Revision"
            DefaultOrder = 6
            DefaultWidthInPixels = Some 90
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Number
            UserConfigurable = true
            RenderHint = None
            Alignment = Center
            CellType = NumberType
            IsKey = false
            FormatString = Some "0" }
          { ColumnId = "LifecycleState"
            PropertyName = Some "LifecycleState"
            Title = "State"
            DefaultOrder = 7
            DefaultWidthInPixels = Some 130
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
            { Field = "CommitmentId"
              Direction = Ascending
              Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<OrderCommitmentDto> =
    match ProjectionCompiler.compile<OrderCommitmentDto> schema with
    | Valid a -> a
    | Invalid errs ->
        let msg = errs |> String.concat "; "
        invalidOp $"CommitmentGrid compilation failed: {msg}"

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Order Commitment Ledgers"
      Icon = "handshake"
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

