namespace Medhavi.Client.Components.Supply

open System
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ViewComposition.Context
open Medhavi.Common.Validation
open Medhavi.Client.Components.Shared

module SupplyActions =

    let expedite: ActionDescriptor =
        { Id = ActionDefId.literal "supply.expedite"
          DisplayName = "Expedite PO"
          Icon = "bolt"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Request supplier to expedite delivery by 3 days?"
          RequiredRoles = [ "Planner"; "Buyer" ]
          IsPrimary = true }

    let deExpedite: ActionDescriptor =
        { Id = ActionDefId.literal "supply.de_expedite"
          DisplayName = "De-Expedite PO"
          Icon = "update"
          Scope = SingleEntityScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "Buyer" ]
          IsPrimary = false }

    let split: ActionDescriptor =
        { Id = ActionDefId.literal "supply.split"
          DisplayName = "Split PO"
          Icon = "call_split"
          Scope = SingleEntityScope
          ExecutionMode = Parameterized("supply-split-form", "Split Purchase Order")
          RequiredRoles = [ "Planner"; "Buyer" ]
          IsPrimary = false }

    let changeVendor: ActionDescriptor =
        { Id = ActionDefId.literal "supply.change_vendor"
          DisplayName = "Alternate Vendor"
          Icon = "swap_horiz"
          Scope = SingleEntityScope
          ExecutionMode = Parameterized("vendor-selection-form", "Select Alternate Vendor")
          RequiredRoles = [ "Planner"; "Buyer" ]
          IsPrimary = false }

    let explainDelay: ActionDescriptor =
        { Id = ActionDefId.literal "supply.explain"
          DisplayName = "Explain Delay"
          Icon = "auto_awesome"
          Scope = SingleEntityScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "Viewer" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ expedite; deExpedite; split; changeVendor; explainDelay ]

type SupplyRow =
    { SupplyId: string
      Vendor: string
      Product: string
      Location: string
      PromisedDate: DateTimeOffset
      Quantity: decimal
      Confirmed: decimal
      Status: string
      PeggedDemandId: string }

module SupplySchema =

    let schemaId = SchemaDefId.literal "supply-grid-schema"

    let definition: SchemaDefinition =
        { Id = schemaId
          DisplayName = "Supply Orders Schema"
          Capabilities = [ "Tabular" ]
          SupportedActions = SupplyActions.all |> List.map(fun a -> a.Id)
          Columns =
            [ { ColumnId = "Status"
                PropertyName = Some "Status"
                Title = "Status"
                DefaultOrder = 0
                DefaultWidthInPixels = Some 110
                DefaultVisible = true
                DefaultPinned = true
                Sortable = true
                FilterValueKind = Some Text
                Filterable = true
                Groupable = false
                RenderHint = Some "StatusBadge"
                Alignment = Left
                CellType = TextType
                IsKey = false
                FormatString = None
                UserConfigurable = true }
              { ColumnId = "SupplyId"
                PropertyName = Some "SupplyId"
                Title = "PO Number"
                DefaultOrder = 1
                DefaultWidthInPixels = Some 120
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
              { ColumnId = "Vendor"
                PropertyName = Some "Vendor"
                Title = "Supplier / Vendor"
                DefaultOrder = 2
                DefaultWidthInPixels = Some 170
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
              { ColumnId = "Product"
                PropertyName = Some "Product"
                Title = "Component / Item"
                DefaultOrder = 3
                DefaultWidthInPixels = Some 160
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
                Title = "Destination DC"
                DefaultOrder = 4
                DefaultWidthInPixels = Some 130
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
              { ColumnId = "PromisedDate"
                PropertyName = Some "PromisedDate"
                Title = "Promised Date"
                DefaultOrder = 5
                DefaultWidthInPixels = Some 120
                DefaultVisible = true
                DefaultPinned = false
                Sortable = true
                FilterValueKind = Some DateTimeOffset
                Filterable = true
                Groupable = false
                RenderHint = None
                Alignment = Left
                CellType = DateTimeType
                IsKey = false
                FormatString = Some "yyyy-MM-dd"
                UserConfigurable = true }
              { ColumnId = "Quantity"
                PropertyName = Some "Quantity"
                Title = "Ordered Qty"
                DefaultOrder = 6
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
              { ColumnId = "Confirmed"
                PropertyName = Some "Confirmed"
                Title = "Confirmed Qty"
                DefaultOrder = 7
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
              { ColumnId = "PeggedDemandId"
                PropertyName = Some "PeggedDemandId"
                Title = "Pegged Demand"
                DefaultOrder = 8
                DefaultWidthInPixels = Some 120
                DefaultVisible = true
                DefaultPinned = false
                Sortable = true
                FilterValueKind = Some Text
                Filterable = true
                Groupable = false
                RenderHint = None
                Alignment = Left
                CellType = TextType
                IsKey = false
                FormatString = None
                UserConfigurable = true } ]
          DefaultSort = None
          DefaultFilters = [] }

    let adapter: ProjectionAdapter<SupplyRow> =
        match ProjectionCompiler.compile<SupplyRow> definition with
        | Valid a -> a
        | Invalid errs -> failwithf "SupplySchema compilation failed: %A" errs

    let toUniversal: SupplyRow -> UniversalRow = adapter.ToUniversal
