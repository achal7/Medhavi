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

module DemandOrdersSchema =

    let schemaId = SchemaDefId.literal "demand-grid-schema"

    let definition: SchemaDefinition =
        { Id = schemaId
          DisplayName = "Enterprise Demand Orders"
          Capabilities = [ "Tabular" ]
          SupportedActions = DemandOrdersActions.all |> List.map(fun a -> a.Id)
          Actions = DemandOrdersActions.all
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
