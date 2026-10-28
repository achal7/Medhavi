module Medhavi.Client.Demand.Learning.DemandLearningGrid

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

/// Declarative metadata descriptors for Demand Learning actions.
module DemandLearningActions =

    let deriveLearnings: ActionDescriptor =
        { Id = ActionDefId.literal "demand_learning.derive"
          DisplayName = "Derive Learnings"
          Icon = "model_training"
          Scope = GlobalScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "LeadPlanner" ]
          IsPrimary = true }

    let recordLearning: ActionDescriptor =
        { Id = ActionDefId.literal "demand_learning.record"
          DisplayName = "Record Learning"
          Icon = "bookmark_add"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Record this learning insight into the permanent repository?"
          RequiredRoles = [ "LeadPlanner"; "Executive" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ deriveLearnings; recordLearning ]

let schemaId = SchemaDefId.literal "demand-learning-schema"
let storeId = DataStoreId.literal "demand-learning-store"
let definitionId = ComponentDefId.literal "comp-demand-learning"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Demand Learning & Continuous Improvement Insights"
      Capabilities = [ "Tabular" ]
      SupportedActions = DemandLearningActions.all |> List.map (fun a -> a.Id)
      Actions = DemandLearningActions.all
      Columns =
        [ { ColumnId = "LearningId"
            PropertyName = Some "LearningId"
            Title = "Learning ID"
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
          { ColumnId = "Scope"
            PropertyName = Some "Scope"
            Title = "Scope"
            DefaultOrder = 1
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
          { ColumnId = "LearningType"
            PropertyName = Some "LearningType"
            Title = "Type"
            DefaultOrder = 2
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
          { ColumnId = "LearningStatement"
            PropertyName = Some "LearningStatement"
            Title = "Learning Statement"
            DefaultOrder = 3
            DefaultWidthInPixels = Some 240
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
          { ColumnId = "PatternConfidence"
            PropertyName = Some "PatternConfidence"
            Title = "Pattern Conf."
            DefaultOrder = 4
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
          { ColumnId = "InterventionConfidence"
            PropertyName = Some "InterventionConfidence"
            Title = "Action Conf."
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
          { ColumnId = "PolicyVersion"
            PropertyName = Some "PolicyVersion"
            Title = "Policy"
            DefaultOrder = 6
            DefaultWidthInPixels = Some 100
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
          { ColumnId = "Timestamp"
            PropertyName = Some "Timestamp"
            Title = "Discovered"
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

let adapter: ProjectionAdapter<DemandLearningDto> =
    match ProjectionCompiler.compile<DemandLearningDto> schema with
    | Valid a -> a
    | Invalid errs ->
        let msg = errs |> String.concat "; "
        invalidOp $"DemandLearningGrid compilation failed: {msg}"

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Demand Continuous Learning Studio"
      Icon = "school"
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
