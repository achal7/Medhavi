module Medhavi.Client.Supply.ChangeSensing.SupplyChangeSensingGrid

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

/// Declarative metadata descriptors for Supply Change Sensing actions.
module SupplyChangeSensingActions =

    let corroborate: ActionDescriptor =
        { Id = ActionDefId.literal "act-corroborate-signal"
          DisplayName = "Corroborate Signal"
          Icon = "fact_check"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Corroborate this change signal with secondary telemetry?"
          RequiredRoles = [ "Planner"; "Buyer" ]
          IsPrimary = true }

    let confirm: ActionDescriptor =
        { Id = ActionDefId.literal "act-confirm-change"
          DisplayName = "Confirm Change"
          Icon = "verified"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Confirm this change as verified reality and propagate to planning?"
          RequiredRoles = [ "Planner" ]
          IsPrimary = false }

    let dismiss: ActionDescriptor =
        { Id = ActionDefId.literal "act-dismiss-noise"
          DisplayName = "Dismiss Noise"
          Icon = "close"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Dismiss this signal as unverified telemetry noise?"
          RequiredRoles = [ "Planner" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ corroborate; confirm; dismiss ]

let corroborate = SupplyChangeSensingActions.corroborate
let confirm = SupplyChangeSensingActions.confirm
let dismiss = SupplyChangeSensingActions.dismiss
let allActions = SupplyChangeSensingActions.all

let storeId = DataStoreId.literal "supply-change-sensing-store"
let schemaId = SchemaDefId.literal "schema-supply-changes"
let definitionId = ComponentDefId.literal "comp-supply-changes-grid"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Supply Change Sensing (CA-S-009)"
      Capabilities = [ "Tabular"; "Sensing" ]
      SupportedActions = SupplyChangeSensingActions.all |> List.map (fun a -> a.Id)
      Actions = SupplyChangeSensingActions.all
      Columns =
        [ { ColumnId = "AssessmentId"
            PropertyName = Some "AssessmentId"
            Title = "Signal ID"
            DefaultOrder = 1
            DefaultWidthInPixels = Some 140
            DefaultVisible = true
            DefaultPinned = true
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Text
            UserConfigurable = false
            RenderHint = None
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = true
            FormatString = None }
          { ColumnId = "CurrentEpistemicState"
            PropertyName = Some "CurrentEpistemicState"
            Title = "Epistemic Status"
            DefaultOrder = 2
            DefaultWidthInPixels = Some 150
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = Some "EpistemicBadge"
            Alignment = TextAlignment.Center
            CellType = CellType.TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "ChangeType"
            PropertyName = Some "ChangeType"
            Title = "Change Classification"
            DefaultOrder = 3
            DefaultWidthInPixels = Some 150
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = Some "StatusBadge"
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "MonitoredEntityId"
            PropertyName = Some "MonitoredEntityId"
            Title = "Entity Identifier"
            DefaultOrder = 4
            DefaultWidthInPixels = Some 170
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = None
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "MonitoredEntityType"
            PropertyName = Some "MonitoredEntityType"
            Title = "Entity Type"
            DefaultOrder = 5
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = None
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "Materiality"
            PropertyName = Some "Materiality"
            Title = "Planning Materiality"
            DefaultOrder = 6
            DefaultWidthInPixels = Some 150
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = Some "StatusBadge"
            Alignment = TextAlignment.Center
            CellType = CellType.TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "DaysVariance"
            PropertyName = Some "DaysVariance"
            Title = "Schedule Slip (Days)"
            DefaultOrder = 7
            DefaultWidthInPixels = Some 130
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Number
            UserConfigurable = true
            RenderHint = None
            Alignment = TextAlignment.Right
            CellType = CellType.NumberType
            IsKey = false
            FormatString = Some "+0.0;-0.0;0.0" }
          { ColumnId = "QuantityVariance"
            PropertyName = Some "QuantityVariance"
            Title = "Quantity Delta"
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
            Alignment = TextAlignment.Right
            CellType = CellType.NumberType
            IsKey = false
            FormatString = Some "+0.0;-0.0;0.0" }
          { ColumnId = "AssessmentConfidence"
            PropertyName = Some "AssessmentConfidence"
            Title = "Confidence"
            DefaultOrder = 9
            DefaultWidthInPixels = Some 110
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = None
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "DetectionTime"
            PropertyName = Some "DetectionTime"
            Title = "Detection Time"
            DefaultOrder = 10
            DefaultWidthInPixels = Some 150
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some FilterValueKind.DateTimeOffset
            UserConfigurable = true
            RenderHint = None
            Alignment = TextAlignment.Left
            CellType = CellType.DateTimeType
            IsKey = false
            FormatString = Some "yyyy-MM-dd HH:mm" } ]
      DefaultSort = Some { Field = "DetectionTime"; Direction = Descending; Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<SupplyChangeAssessmentDto> =
    { ToUniversal =
        fun (dto: SupplyChangeAssessmentDto) ->
            { Key = dto.AssessmentId
              Cells =
                Map.ofList
                    [ "AssessmentId", TextValue dto.AssessmentId
                      "CurrentEpistemicState", TextValue dto.CurrentEpistemicState
                      "ChangeType", TextValue dto.ChangeType
                      "MonitoredEntityId", TextValue dto.MonitoredEntityId
                      "MonitoredEntityType", TextValue dto.MonitoredEntityType
                      "Materiality", TextValue dto.Materiality
                      "DaysVariance", NumberValue (decimal dto.DaysVariance)
                      "QuantityVariance", NumberValue (decimal dto.QuantityVariance)
                      "AssessmentConfidence", TextValue dto.AssessmentConfidence
                      "DetectionTime", DateTimeValue dto.DetectionTime ] } }

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Supply Change Sensing Stream"
      Icon = "sensors"
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

