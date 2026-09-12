namespace Medhavi.Client.Components.Core.Demand

open System
open Medhavi.Contracts.Core.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ViewComposition.Context
open Medhavi.Common.Validation
open Medhavi.Client.Components.Shared

module DemandOrdersActions =

    let lock: ActionDescriptor =
        { Id = ActionDefId.literal "demand.lock"
          DisplayName = "Lock Demand"
          Icon = "lock"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Lock this demand line against automated rescheduling?"
          RequiredRoles = [ "Planner"; "LeadPlanner" ]
          IsPrimary = true }

    let unlock: ActionDescriptor =
        { Id = ActionDefId.literal "demand.unlock"
          DisplayName = "Unlock Demand"
          Icon = "lock_open"
          Scope = SingleEntityScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "LeadPlanner" ]
          IsPrimary = false }

    let overrideQuantity: ActionDescriptor =
        { Id = ActionDefId.literal "demand.override"
          DisplayName = "Override Quantity"
          Icon = "edit_note"
          Scope = SingleEntityScope
          ExecutionMode = Parameterized("demand-override-form", "Override Demand Quantity")
          RequiredRoles = [ "Planner" ]
          IsPrimary = false }

    let explainShortage: ActionDescriptor =
        { Id = ActionDefId.literal "demand.explain"
          DisplayName = "Explain Shortage"
          Icon = "auto_awesome"
          Scope = SingleEntityScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "Viewer" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ lock; unlock; overrideQuantity; explainShortage ]

module DemandOrdersSchema =

    let schemaId = SchemaDefId.literal "demand-grid-schema"

    let definition: SchemaDefinition =
        { Id = schemaId
          DisplayName = "Enterprise Demand Orders"
          Capabilities = [ "Tabular" ]
          SupportedActions = DemandOrdersActions.all |> List.map(fun a -> a.Id)
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
                FilterValueKind = Some DateTimeOffset
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

    let adapter =
        match ProjectionCompiler.compile<DemandDto> definition with
        | Valid a -> a
        | Invalid errs -> failwithf "DemandOrdersSchema compilation failed: %A" errs
