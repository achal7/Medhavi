namespace Medhavi.Client.Components.Core.ItemTransition

open System
open Medhavi.Contracts.Core.ItemTransition
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ViewComposition.Context
open Medhavi.Common.Validation
open Medhavi.Client.Components.Shared

module ItemTransitionActions =

    let suspend: ActionDescriptor =
        { Id = ActionDefId.literal "item_transition.suspend"
          DisplayName = "Suspend Transition"
          Icon = "pause"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Suspend this active item transition rule?"
          RequiredRoles = [ "Planner"; "Supervisor" ]
          IsPrimary = true }

    let reinstate: ActionDescriptor =
        { Id = ActionDefId.literal "item_transition.reinstate"
          DisplayName = "Reinstate Transition"
          Icon = "play_arrow"
          Scope = SingleEntityScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "Supervisor" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ suspend; reinstate ]

module ItemTransitionSchema =

    let schemaId = SchemaDefId.literal "item-transition-schema"

    let definition: SchemaDefinition =
        { Id = schemaId
          DisplayName = "Item Transitions & Supersessions"
          Capabilities = [ "Tabular" ]
          SupportedActions = ItemTransitionActions.all |> List.map(fun a -> a.Id)
          Actions = ItemTransitionActions.all
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
              { ColumnId = "TransitionId"
                PropertyName = Some "TransitionId"
                Title = "Transition ID"
                DefaultOrder = 1
                DefaultWidthInPixels = Some 130
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
              { ColumnId = "SupersededItem"
                PropertyName = Some "SupersededItem"
                Title = "Old Item (Superseded)"
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
              { ColumnId = "SupersedingItem"
                PropertyName = Some "SupersedingItem"
                Title = "New Item (Superseding)"
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
              { ColumnId = "TransitionType"
                PropertyName = Some "TransitionType"
                Title = "Type"
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
              { ColumnId = "EffectiveDate"
                PropertyName = Some "EffectiveDate"
                Title = "Effective Date"
                DefaultOrder = 5
                DefaultWidthInPixels = Some 130
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
              { ColumnId = "EndDate"
                PropertyName = Some "EndDate"
                Title = "End Date"
                DefaultOrder = 6
                DefaultWidthInPixels = Some 130
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
                FormatString = Some "yyyy-MM-dd" } ]
          DefaultSort =
            Some
                { Field = "EffectiveDate"
                  Direction = Descending
                  Priority = 1 }
          DefaultFilters = [] }

    let adapter =
        match ProjectionCompiler.compile<ItemTransitionDto> definition with
        | Valid a -> a
        | Invalid errs -> failwithf "ItemTransitionSchema compilation failed: %A" errs
