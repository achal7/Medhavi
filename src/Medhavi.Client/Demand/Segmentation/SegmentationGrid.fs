module Medhavi.Client.Demand.Segmentation.SegmentationGrid

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

/// Declarative metadata descriptors for Demand Segmentation actions.
module SegmentationActions =

    let classifyEntity: ActionDescriptor =
        { Id = ActionDefId.literal "demand_segmentation.classify_entity"
          DisplayName = "Classify Entity"
          Icon = "category"
          Scope = SingleEntityScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "DemandAnalyst" ]
          IsPrimary = true }

    let overrideClassification: ActionDescriptor =
        { Id = ActionDefId.literal "demand_segmentation.override_classification"
          DisplayName = "Override Classification"
          Icon = "edit_note"
          Scope = SingleEntityScope
          ExecutionMode =
            RequiresConfirmation "Apply planner override to this entity's planning classification?"
          RequiredRoles = [ "Planner"; "LeadPlanner" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ classifyEntity; overrideClassification ]

let schemaId = SchemaDefId.literal "demand-segmentation-schema"
let storeId = DataStoreId.literal "demand-segmentation-store"
let definitionId = ComponentDefId.literal "comp-demand-segmentation"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Demand Planning Segmentation (ABC / XYZ)"
      Capabilities = [ "Tabular" ]
      SupportedActions = SegmentationActions.all |> List.map (fun a -> a.Id)
      Actions = SegmentationActions.all
      Columns =
        [ { ColumnId = "AssignmentId"
            PropertyName = Some "AssignmentId"
            Title = "Assignment ID"
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
          { ColumnId = "EntityType"
            PropertyName = Some "EntityType"
            Title = "Entity Type"
            DefaultOrder = 1
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
            DefaultOrder = 2
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
          { ColumnId = "ClassificationType"
            PropertyName = Some "ClassificationType"
            Title = "Segment Scheme"
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
          { ColumnId = "CurrentClassification"
            PropertyName = Some "CurrentClassification"
            Title = "Classification"
            DefaultOrder = 4
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
            FormatString = None }
          { ColumnId = "ClassificationScore"
            PropertyName = Some "ClassificationScore"
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
          { ColumnId = "ClassificationConfidence"
            PropertyName = Some "ClassificationConfidence"
            Title = "Confidence"
            DefaultOrder = 6
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
            FormatString = None }
          { ColumnId = "AssignmentRationale"
            PropertyName = Some "AssignmentRationale"
            Title = "Rationale"
            DefaultOrder = 7
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
            FormatString = None }
          { ColumnId = "PolicyVersion"
            PropertyName = Some "PolicyVersion"
            Title = "Policy"
            DefaultOrder = 8
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
          { ColumnId = "LastUpdated"
            PropertyName = Some "LastUpdated"
            Title = "Updated"
            DefaultOrder = 9
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
            { Field = "CurrentClassification"
              Direction = Ascending
              Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<PlanningClassificationDto> =
    match ProjectionCompiler.compile<PlanningClassificationDto> schema with
    | Valid a -> a
    | Invalid errs ->
        let msg = errs |> String.concat "; "
        invalidOp $"SegmentationGrid compilation failed: {msg}"

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Demand Segmentation Workbench"
      Icon = "category"
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
