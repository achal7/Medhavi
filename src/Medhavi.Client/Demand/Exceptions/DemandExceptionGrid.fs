module Medhavi.Client.Demand.Exceptions.DemandExceptionGrid

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

/// Declarative metadata descriptors for Demand Exception actions.
module DemandExceptionActions =

    let evaluateException: ActionDescriptor =
        { Id = ActionDefId.literal "demand_exception.evaluate"
          DisplayName = "Evaluate Exception"
          Icon = "gavel"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Evaluate exception evidence against detection rules?"
          RequiredRoles = [ "Planner"; "DemandAnalyst" ]
          IsPrimary = true }

    let all: ActionDescriptor list = [ evaluateException ]

let schemaId = SchemaDefId.literal "demand-exception-schema"
let storeId = DataStoreId.literal "demand-exception-store"
let definitionId = ComponentDefId.literal "comp-demand-exceptions"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Demand Exceptions & Evidence"
      Capabilities = [ "Tabular" ]
      SupportedActions = DemandExceptionActions.all |> List.map (fun a -> a.Id)
      Actions = DemandExceptionActions.all
      Columns =
        [ { ColumnId = "EvidenceId"
            PropertyName = Some "EvidenceId"
            Title = "Evidence ID"
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
          { ColumnId = "ExceptionType"
            PropertyName = Some "ExceptionType"
            Title = "Exception Type"
            DefaultOrder = 1
            DefaultWidthInPixels = Some 150
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
          { ColumnId = "EntityType"
            PropertyName = Some "EntityType"
            Title = "Entity Type"
            DefaultOrder = 2
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
          { ColumnId = "EntityId"
            PropertyName = Some "EntityId"
            Title = "Entity ID"
            DefaultOrder = 3
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
          { ColumnId = "ScopeId"
            PropertyName = Some "ScopeId"
            Title = "Planning Scope"
            DefaultOrder = 4
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
          { ColumnId = "Severity"
            PropertyName = Some "Severity"
            Title = "Severity"
            DefaultOrder = 5
            DefaultWidthInPixels = Some 110
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
          { ColumnId = "TriggeringMetric"
            PropertyName = Some "TriggeringMetric"
            Title = "Trigger Metric"
            DefaultOrder = 6
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
          { ColumnId = "MetricValue"
            PropertyName = Some "MetricValue"
            Title = "Metric Value"
            DefaultOrder = 7
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
          { ColumnId = "ThresholdValue"
            PropertyName = Some "ThresholdValue"
            Title = "Threshold"
            DefaultOrder = 8
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
          { ColumnId = "Rationale"
            PropertyName = Some "Rationale"
            Title = "Rationale"
            DefaultOrder = 9
            DefaultWidthInPixels = Some 220
            DefaultVisible = true
            DefaultPinned = false
            Sortable = false
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
            { Field = "Severity"
              Direction = Descending
              Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<DemandExceptionEvidenceDto> =
    match ProjectionCompiler.compile<DemandExceptionEvidenceDto> schema with
    | Valid a -> a
    | Invalid errs ->
        let msg = errs |> String.concat "; "
        invalidOp $"DemandExceptionGrid compilation failed: {msg}"

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Demand Exceptions Workbench"
      Icon = "warning_amber"
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
