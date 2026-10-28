module Medhavi.Client.Core.ItemTransition.ItemTransitionGrid

open System
open Medhavi.Contracts.Core.ItemTransition
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

/// Declarative metadata descriptors for Item Transition actions.
module ItemTransitionActions =

    let recordSupersession: ActionDescriptor =
        { Id = ActionDefId.literal "item_transition.recordSupersession"
          DisplayName = "Record Supersession"
          Icon = "swap_horiz"
          Scope = GlobalScope
          ExecutionMode = Parameterized("record-supersession-form", "Record Item Supersession")
          RequiredRoles = [ "Planner"; "Supervisor" ]
          IsPrimary = true }

    let activatePhaseOut: ActionDescriptor =
        { Id = ActionDefId.literal "item_transition.activatePhaseOut"
          DisplayName = "Activate Phase-Out"
          Icon = "schedule"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Activate phase-out transition lifecycle for this item?"
          RequiredRoles = [ "Planner"; "Supervisor" ]
          IsPrimary = false }

    let modifyEffectiveDate: ActionDescriptor =
        { Id = ActionDefId.literal "item_transition.modifyEffectiveDate"
          DisplayName = "Modify Effective Date"
          Icon = "edit_calendar"
          Scope = SingleEntityScope
          ExecutionMode = Parameterized("modify-effective-date-form", "Modify Transition Effective Date")
          RequiredRoles = [ "Planner"; "Supervisor" ]
          IsPrimary = false }

    let suspend: ActionDescriptor =
        { Id = ActionDefId.literal "item_transition.suspend"
          DisplayName = "Suspend Transition"
          Icon = "pause"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Suspend this active item transition rule?"
          RequiredRoles = [ "Planner"; "Supervisor" ]
          IsPrimary = false }

    let reinstate: ActionDescriptor =
        { Id = ActionDefId.literal "item_transition.reinstate"
          DisplayName = "Reinstate Transition"
          Icon = "play_arrow"
          Scope = SingleEntityScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "Supervisor" ]
          IsPrimary = false }

    let retire: ActionDescriptor =
        { Id = ActionDefId.literal "item_transition.retire"
          DisplayName = "Retire Transition"
          Icon = "archive"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Permanently retire this item transition rule?"
          RequiredRoles = [ "Planner"; "Supervisor" ]
          IsPrimary = false }

    let all: ActionDescriptor list =
        [ recordSupersession
          activatePhaseOut
          modifyEffectiveDate
          suspend
          reinstate
          retire ]

let recordSupersession = ItemTransitionActions.recordSupersession
let activatePhaseOut = ItemTransitionActions.activatePhaseOut
let modifyEffectiveDate = ItemTransitionActions.modifyEffectiveDate
let suspend = ItemTransitionActions.suspend
let reinstate = ItemTransitionActions.reinstate
let retire = ItemTransitionActions.retire
let allActions = ItemTransitionActions.all

let schemaId = SchemaDefId.literal "core-item-transition-grid-schema"
let storeId: DataStoreId = DataStoreId.literal "core-item-transition-store"
let definitionId = ComponentDefId.literal "item-transition-grid"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Item Transitions & Supersessions"
      Capabilities = [ "Tabular" ]
      SupportedActions = ItemTransitionActions.all |> List.map (fun a -> a.Id)
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
            FilterValueKind = Some FilterValueKind.DateTimeOffset
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
            FilterValueKind = Some FilterValueKind.DateTimeOffset
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

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<ItemTransitionDto> =
    match ProjectionCompiler.compile<ItemTransitionDto> schema with
    | Valid a -> a
    | Invalid errs ->
        let msg = String.concat "; " errs
        invalidOp $"ItemTransitionGrid compilation failed: {msg}"

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Item Transitions Workbench"
      Icon = "swap_calls"
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
