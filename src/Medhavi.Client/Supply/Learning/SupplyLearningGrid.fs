module Medhavi.Client.Supply.Learning.SupplyLearningGrid

open System
open Medhavi.Contracts.Supply
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

/// Declarative metadata descriptors for Supply Learning actions.
module SupplyLearningActions =

    let recordLearning: ActionDescriptor =
        { Id = ActionDefId.literal "supply_learning.record_learning"
          DisplayName = "Record Learning"
          Icon = "school"
          Scope = GlobalScope
          ExecutionMode = RequiresConfirmation "Record derived multi-period supply learning?"
          RequiredRoles = [ "Planner"; "SupplyChainLead" ]
          IsPrimary = true }

    let recordOutcome: ActionDescriptor =
        { Id = ActionDefId.literal "supply_learning.record_outcome"
          DisplayName = "Record Outcome"
          Icon = "fact_check"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Record governance outcome on selected supply learning?"
          RequiredRoles = [ "Planner"; "SupplyChainLead" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ recordLearning; recordOutcome ]

let recordLearning = SupplyLearningActions.recordLearning
let recordOutcome = SupplyLearningActions.recordOutcome
let allActions = SupplyLearningActions.all

let storeId = DataStoreId.literal "supply-learning-store"
let schemaId = SchemaDefId.literal "supply-learning-grid-schema"
let definitionId = ComponentDefId.literal "comp-supply-learning"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Supply Learning & Retrospective Insights"
      Capabilities = [ "Tabular" ]
      SupportedActions = SupplyLearningActions.all |> List.map (fun a -> a.Id)
      Actions = SupplyLearningActions.all
      Columns =
        [ { ColumnId = "LearningId"
            PropertyName = Some "LearningId"
            Title = "Learning ID"
            DefaultOrder = 0
            DefaultWidthInPixels = Some 140
            DefaultVisible = true
            DefaultPinned = true
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = false
            RenderHint = None
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = true
            FormatString = None
            UserConfigurable = false }
          { ColumnId = "SubjectType"
            PropertyName = Some "SubjectType"
            Title = "Subject Type"
            DefaultOrder = 1
            DefaultWidthInPixels = Some 130
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = None
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "SubjectId"
            PropertyName = Some "SubjectId"
            Title = "Subject Ref"
            DefaultOrder = 2
            DefaultWidthInPixels = Some 160
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = None
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "LearningType"
            PropertyName = Some "LearningType"
            Title = "Learning Type"
            DefaultOrder = 3
            DefaultWidthInPixels = Some 150
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = Some "StatusBadge"
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "Scope"
            PropertyName = Some "Scope"
            Title = "Scope"
            DefaultOrder = 4
            DefaultWidthInPixels = Some 160
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = None
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "PatternConfidence"
            PropertyName = Some "PatternConfidence"
            Title = "Confidence"
            DefaultOrder = 5
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = Some "StatusBadge"
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "LifecycleState"
            PropertyName = Some "LifecycleState"
            Title = "Lifecycle State"
            DefaultOrder = 6
            DefaultWidthInPixels = Some 130
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = Some "StatusBadge"
            Alignment = TextAlignment.Center
            CellType = CellType.TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "Outcome"
            PropertyName = Some "Outcome"
            Title = "Outcome"
            DefaultOrder = 7
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = Some "StatusBadge"
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "GenerationTimestamp"
            PropertyName = Some "GenerationTimestamp"
            Title = "Generated At"
            DefaultOrder = 8
            DefaultWidthInPixels = Some 160
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some FilterValueKind.DateTimeOffset
            Filterable = true
            Groupable = false
            RenderHint = None
            Alignment = TextAlignment.Left
            CellType = CellType.DateTimeType
            IsKey = false
            FormatString = Some "yyyy-MM-dd HH:mm"
            UserConfigurable = true } ]
      DefaultSort = Some { Field = "GenerationTimestamp"; Direction = Descending; Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<SupplyLearningDto> =
    match ProjectionCompiler.compile<SupplyLearningDto> schema with
    | Valid a -> a
    | Invalid _ ->
        { ToUniversal =
            fun (dto: SupplyLearningDto) ->
                { Key = dto.LearningId
                  Cells =
                    Map.ofList
                        [ "LearningId", TextValue dto.LearningId
                          "SubjectType", TextValue dto.SubjectType
                          "SubjectId", TextValue dto.SubjectId
                          "LearningType", TextValue dto.LearningType
                          "Scope", TextValue dto.Scope
                          "PatternConfidence", TextValue dto.PatternConfidence
                          "LifecycleState", TextValue dto.LifecycleState
                          "Outcome", TextValue dto.Outcome
                          "GenerationTimestamp", DateTimeValue dto.GenerationTimestamp ] } }

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Supply Learning & Retrospective Insights"
      Icon = "school"
      Category = ComponentCategory.DataDisplay
      RequiredSchemaCapabilities = [ "Tabular" ]
      DefaultGridSpan = GridSpan.literal 12 6
      DefaultSchemaId = Some schemaId
      DefaultStoreId = Some storeId
      IsResizable = true
      IsDraggable = true
      SelectionMode = SelectionMode.SingleSelection }

let runtime: ComponentRuntime =
    { Render = UniversalEnterpriseGrid.render }

let registration: ComponentRegistration =
    { Definition = componentDefinition
      Schema = schema
      Runtime = runtime }
