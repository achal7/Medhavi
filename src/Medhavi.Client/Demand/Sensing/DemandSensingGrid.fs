module Medhavi.Client.Demand.Sensing.DemandSensingGrid

open System
open Medhavi.Contracts.Demand
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

/// Declarative metadata descriptors for Demand Behavior Sensing actions.
module DemandSensingActions =

    let evaluateSignal: ActionDescriptor =
        { Id = ActionDefId.literal "demand_sensing.evaluate_signal"
          DisplayName = "Evaluate Signal"
          Icon = "sensors"
          Scope = SingleEntityScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "DemandAnalyst" ]
          IsPrimary = true }

    let evaluateRefresh: ActionDescriptor =
        { Id = ActionDefId.literal "demand_sensing.evaluate_refresh"
          DisplayName = "Evaluate Forecast Refresh"
          Icon = "published_with_changes"
          Scope = SingleEntityScope
          ExecutionMode =
            RequiresConfirmation "Evaluate whether demand shift warrants triggering an out-of-cycle forecast refresh?"
          RequiredRoles = [ "Planner"; "DemandAnalyst" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ evaluateSignal; evaluateRefresh ]

let schemaId = SchemaDefId.literal "demand-sensing-schema"
let storeId = DataStoreId.literal "demand-sensing-store"
let definitionId = ComponentDefId.literal "comp-demand-sensing"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Demand Sensing & Behavior Assessment"
      Capabilities = [ "Tabular" ]
      SupportedActions = DemandSensingActions.all |> List.map (fun a -> a.Id)
      Actions = DemandSensingActions.all
      Columns =
        [ { ColumnId = "AssessmentId"
            PropertyName = Some "AssessmentId"
            Title = "Assessment ID"
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
          { ColumnId = "Item"
            PropertyName = Some "Item"
            Title = "Item"
            DefaultOrder = 1
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
            DefaultOrder = 2
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
          { ColumnId = "CurrentState"
            PropertyName = Some "CurrentState"
            Title = "Behavior State"
            DefaultOrder = 3
            DefaultWidthInPixels = Some 130
            DefaultVisible = true
            DefaultPinned = false
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
          { ColumnId = "BaselineMean"
            PropertyName = Some "BaselineMean"
            Title = "Baseline Mean"
            DefaultOrder = 4
            DefaultWidthInPixels = Some 120
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
            FormatString = Some "N2" }
          { ColumnId = "BaselineStdDev"
            PropertyName = Some "BaselineStdDev"
            Title = "Baseline StdDev"
            DefaultOrder = 5
            DefaultWidthInPixels = Some 120
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
            FormatString = Some "N2" }
          { ColumnId = "LastDeviationMagnitude"
            PropertyName = Some "LastDeviationMagnitude"
            Title = "Deviation Mag"
            DefaultOrder = 6
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
            FormatString = Some "N2" }
          { ColumnId = "AssessmentConfidence"
            PropertyName = Some "AssessmentConfidence"
            Title = "Confidence"
            DefaultOrder = 7
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = Some "StatusBadge"
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None } ]
      DefaultSort =
        Some
            { Field = "CurrentState"
              Direction = Descending
              Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<DemandBehaviorAssessmentDto> =
    match ProjectionCompiler.compile<DemandBehaviorAssessmentDto> schema with
    | Valid a -> a
    | Invalid errs ->
        let msg = errs |> String.concat "; "
        invalidOp $"DemandSensingGrid compilation failed: {msg}"

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Demand Sensing Studio"
      Icon = "sensors"
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
