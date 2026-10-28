module Medhavi.Client.Supply.Quality.SupplyQualityGrid

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

/// Declarative metadata descriptors for Supply Quality actions.
module SupplyQualityActions =

    let evaluateQuality: ActionDescriptor =
        { Id = ActionDefId.literal "supply_quality.evaluate"
          DisplayName = "Evaluate Supply Quality"
          Icon = "verified"
          Scope = GlobalScope
          ExecutionMode = RequiresConfirmation "Evaluate supply chain operational quality and alignment?"
          RequiredRoles = [ "Planner"; "QualityAuditor" ]
          IsPrimary = true }

    let all: ActionDescriptor list = [ evaluateQuality ]

let evaluateQuality = SupplyQualityActions.evaluateQuality
let allActions = SupplyQualityActions.all

let storeId = DataStoreId.literal "supply-quality-store"
let schemaId = SchemaDefId.literal "supply-quality-grid-schema"
let definitionId = ComponentDefId.literal "comp-supply-quality"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Supply Quality Assessments"
      Capabilities = [ "Tabular" ]
      SupportedActions = SupplyQualityActions.all |> List.map (fun a -> a.Id)
      Actions = SupplyQualityActions.all
      Columns =
        [ { ColumnId = "AssessmentId"
            PropertyName = Some "AssessmentId"
            Title = "Assessment ID"
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
          { ColumnId = "PlanningScopeId"
            PropertyName = Some "PlanningScopeId"
            Title = "Planning Scope"
            DefaultOrder = 1
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
          { ColumnId = "PlanningHorizon"
            PropertyName = Some "PlanningHorizon"
            Title = "Horizon"
            DefaultOrder = 2
            DefaultWidthInPixels = Some 110
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
          { ColumnId = "EvaluationPeriodId"
            PropertyName = Some "EvaluationPeriodId"
            Title = "Period"
            DefaultOrder = 3
            DefaultWidthInPixels = Some 100
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
          { ColumnId = "VersionNumber"
            PropertyName = Some "VersionNumber"
            Title = "Version"
            DefaultOrder = 4
            DefaultWidthInPixels = Some 90
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Number
            Filterable = true
            Groupable = true
            RenderHint = None
            Alignment = TextAlignment.Right
            CellType = CellType.NumberType
            IsKey = false
            FormatString = Some "N0"
            UserConfigurable = true }
          { ColumnId = "OverallClassification"
            PropertyName = Some "OverallClassification"
            Title = "Classification"
            DefaultOrder = 5
            DefaultWidthInPixels = Some 130
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
          { ColumnId = "OverallScore"
            PropertyName = Some "OverallScore"
            Title = "Quality Score"
            DefaultOrder = 6
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Number
            Filterable = true
            Groupable = false
            RenderHint = None
            Alignment = TextAlignment.Right
            CellType = CellType.NumberType
            IsKey = false
            FormatString = Some "N1"
            UserConfigurable = true }
          { ColumnId = "OverallConfidence"
            PropertyName = Some "OverallConfidence"
            Title = "Confidence"
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
            UserConfigurable = true } ]
      DefaultSort = Some { Field = "OverallScore"; Direction = Descending; Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<SupplyQualityAssessmentDto> =
    { ToUniversal =
        fun (dto: SupplyQualityAssessmentDto) ->
            { Key = dto.AssessmentId
              Cells =
                Map.ofList
                    [ "AssessmentId", TextValue dto.AssessmentId
                      "PlanningScopeId", TextValue dto.PlanningScopeId
                      "PlanningHorizon", TextValue dto.PlanningHorizon
                      "EvaluationPeriodId", TextValue dto.EvaluationPeriodId
                      "VersionNumber", NumberValue (decimal dto.VersionNumber)
                      "OverallClassification", TextValue dto.OverallQuality.OverallClassification
                      "OverallScore", NumberValue dto.OverallQuality.OverallScore
                      "OverallConfidence", TextValue dto.OverallQuality.OverallConfidence ] } }

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Supply Quality Assessments"
      Icon = "verified"
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
