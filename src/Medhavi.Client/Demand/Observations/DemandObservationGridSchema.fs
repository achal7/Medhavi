module Medhavi.Client.Demand.Observations.DemandObservationGridSchema

open System
open Medhavi.Contracts.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ViewComposition.Context
open Medhavi.Common.Validation
open Medhavi.Client.Components.Shared

/// Declarative metadata descriptors for Demand Observation actions.
module DemandObservationActions =

    let confirmObservation: ActionDescriptor =
        { Id = ActionDefId.literal "demand_observation.confirm"
          DisplayName = "Confirm Observation"
          Icon = "check_circle"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Confirm and evaluate this demand observation into the enterprise sensory stream?"
          RequiredRoles = [ "Planner"; "DemandAnalyst" ]
          IsPrimary = true }

    let dismissNoise: ActionDescriptor =
        { Id = ActionDefId.literal "demand_observation.dismiss_noise"
          DisplayName = "Dismiss Noise"
          Icon = "delete_sweep"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Dismiss this demand observation as sensory noise?"
          RequiredRoles = [ "Planner"; "DemandAnalyst" ]
          IsPrimary = false }

    let classifyPattern: ActionDescriptor =
        { Id = ActionDefId.literal "demand_observation.classify_pattern"
          DisplayName = "Classify Pattern"
          Icon = "category"
          Scope = SingleEntityScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "DemandAnalyst" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ confirmObservation; dismissNoise; classifyPattern ]

let schemaId = SchemaDefId.literal "demand-observation-schema"

let definition: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Demand Observations (Sensory Stream)"
      Capabilities = [ "Tabular" ]
      SupportedActions = DemandObservationActions.all |> List.map (fun a -> a.Id)
      Actions = DemandObservationActions.all
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
          { ColumnId = "ObservationId"
            PropertyName = Some "ObservationId"
            Title = "Observation ID"
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
          { ColumnId = "Item"
            PropertyName = Some "Item"
            Title = "Item"
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
          { ColumnId = "Location"
            PropertyName = Some "Location"
            Title = "Location"
            DefaultOrder = 3
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
          { ColumnId = "Quantity"
            PropertyName = Some "Quantity"
            Title = "Observed Qty"
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
            FormatString = Some "N0" }
          { ColumnId = "UnitOfMeasure"
            PropertyName = Some "UnitOfMeasure"
            Title = "UoM"
            DefaultOrder = 5
            DefaultWidthInPixels = Some 80
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = None
            Alignment = Center
            CellType = TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "ObservationType"
            PropertyName = Some "ObservationType"
            Title = "Signal Type"
            DefaultOrder = 6
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
          { ColumnId = "BusinessTime"
            PropertyName = Some "BusinessTime"
            Title = "Signal Timestamp"
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
            FormatString = Some "yyyy-MM-dd HH:mm" }
          { ColumnId = "ObservationTime"
            PropertyName = Some "ObservationTime"
            Title = "Ingestion Timestamp"
            DefaultOrder = 8
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
          { ColumnId = "SourceSystemProvenance"
            PropertyName = Some "SourceSystemProvenance"
            Title = "Channel Provenance"
            DefaultOrder = 9
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
            DefaultOrder = 10
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
            { Field = "BusinessTime"
              Direction = Descending
              Priority = 1 }
      DefaultFilters = [] }

let adapter: ProjectionAdapter<DemandObservationDto> =
    match ProjectionCompiler.compile<DemandObservationDto> definition with
    | Valid a -> a
    | Invalid errs ->
        let msg = errs |> String.concat "; "
        invalidOp $"DemandObservationGridSchema compilation failed: {msg}"
