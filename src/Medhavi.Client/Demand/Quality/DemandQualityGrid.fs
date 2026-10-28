module Medhavi.Client.Demand.Quality.DemandQualityGrid

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

/// Declarative metadata descriptors for Forecast Quality actions.
module DemandQualityActions =

    let evaluateQuality: ActionDescriptor =
        { Id = ActionDefId.literal "demand_quality.evaluate"
          DisplayName = "Evaluate Quality"
          Icon = "query_stats"
          Scope = SingleEntityScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "DemandAnalyst" ]
          IsPrimary = true }

    let publishAssessment: ActionDescriptor =
        { Id = ActionDefId.literal "demand_quality.publish"
          DisplayName = "Publish Assessment"
          Icon = "publish"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Publish authoritative forecast quality assessment?"
          RequiredRoles = [ "LeadPlanner"; "Executive" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ evaluateQuality; publishAssessment ]

let schemaId = SchemaDefId.literal "demand-quality-schema"
let storeId = DataStoreId.literal "demand-quality-store"
let definitionId = ComponentDefId.literal "comp-demand-quality"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Forecast Quality Assessments (WAPE / Bias / FVA)"
      Capabilities = [ "Tabular" ]
      SupportedActions = DemandQualityActions.all |> List.map (fun a -> a.Id)
      Actions = DemandQualityActions.all
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
          { ColumnId = "ScopeId"
            PropertyName = Some "ScopeId"
            Title = "Scope ID"
            DefaultOrder = 1
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
          { ColumnId = "EvaluationPeriodStart"
            PropertyName = Some "EvaluationPeriodStart"
            Title = "Period Start"
            DefaultOrder = 2
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
          { ColumnId = "EvaluationPeriodEnd"
            PropertyName = Some "EvaluationPeriodEnd"
            Title = "Period End"
            DefaultOrder = 3
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
          { ColumnId = "CurrentPublishedVersion"
            PropertyName = Some "CurrentPublishedVersion"
            Title = "Version"
            DefaultOrder = 4
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
          { ColumnId = "LatestQualityState"
            PropertyName = Some "LatestQualityState"
            Title = "Quality State"
            DefaultOrder = 5
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
          { ColumnId = "LastUpdated"
            PropertyName = Some "LastUpdated"
            Title = "Updated"
            DefaultOrder = 6
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
            { Field = "LastUpdated"
              Direction = Descending
              Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<ForecastQualityAssessmentDto> =
    match ProjectionCompiler.compile<ForecastQualityAssessmentDto> schema with
    | Valid a -> a
    | Invalid errs ->
        let msg = errs |> String.concat "; "
        invalidOp $"DemandQualityGrid compilation failed: {msg}"

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Forecast Quality Studio"
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
