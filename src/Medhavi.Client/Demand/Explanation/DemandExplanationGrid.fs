module Medhavi.Client.Demand.Explanation.DemandExplanationGrid

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

/// Declarative metadata descriptors for Demand Explanation actions.
module DemandExplanationActions =

    let establishExplanation: ActionDescriptor =
        { Id = ActionDefId.literal "demand_explanation.establish"
          DisplayName = "Establish Explanation"
          Icon = "psychology"
          Scope = SingleEntityScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "DemandAnalyst" ]
          IsPrimary = true }

    let all: ActionDescriptor list = [ establishExplanation ]

let schemaId = SchemaDefId.literal "demand-explanation-schema"
let storeId = DataStoreId.literal "demand-explanation-store"
let definitionId = ComponentDefId.literal "comp-demand-explanation"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Demand Decision Explanations & Factor Attribution"
      Capabilities = [ "Tabular" ]
      SupportedActions = DemandExplanationActions.all |> List.map (fun a -> a.Id)
      Actions = DemandExplanationActions.all
      Columns =
        [ { ColumnId = "ExplanationId"
            PropertyName = Some "ExplanationId"
            Title = "Explanation ID"
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
          { ColumnId = "ExplainedArtifactType"
            PropertyName = Some "ExplainedArtifactType"
            Title = "Artifact Type"
            DefaultOrder = 1
            DefaultWidthInPixels = Some 140
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
          { ColumnId = "ExplainedArtifactId"
            PropertyName = Some "ExplainedArtifactId"
            Title = "Artifact ID"
            DefaultOrder = 2
            DefaultWidthInPixels = Some 160
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
          { ColumnId = "Version"
            PropertyName = Some "Version"
            Title = "Version"
            DefaultOrder = 3
            DefaultWidthInPixels = Some 90
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
          { ColumnId = "TemplateVersion"
            PropertyName = Some "TemplateVersion"
            Title = "Template"
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
          { ColumnId = "ExplainabilityScore"
            PropertyName = Some "ExplainabilityScore"
            Title = "Score"
            DefaultOrder = 5
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
            FormatString = Some "P1" }
          { ColumnId = "WhatIfAssumption"
            PropertyName = Some "WhatIfAssumption"
            Title = "What-If Scenario"
            DefaultOrder = 6
            DefaultWidthInPixels = Some 200
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
            FormatString = None }
          { ColumnId = "Timestamp"
            PropertyName = Some "Timestamp"
            Title = "Created"
            DefaultOrder = 7
            DefaultWidthInPixels = Some 140
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
            FormatString = Some "yyyy-MM-dd HH:mm" } ]
      DefaultSort =
        Some
            { Field = "Timestamp"
              Direction = Descending
              Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<DemandExplanationDto> =
    match ProjectionCompiler.compile<DemandExplanationDto> schema with
    | Valid a -> a
    | Invalid errs ->
        let msg = errs |> String.concat "; "
        invalidOp $"DemandExplanationGrid compilation failed: {msg}"

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Demand Decision Explanations"
      Icon = "lightbulb"
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
