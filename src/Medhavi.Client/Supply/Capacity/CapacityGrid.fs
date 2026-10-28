module Medhavi.Client.Supply.Capacity.CapacityGrid

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

/// Declarative metadata descriptors for Capacity actions.
module CapacityActions =

    let addShift: ActionDescriptor =
        { Id = ActionDefId.literal "capacity.add_shift"
          DisplayName = "Authorize Shift"
          Icon = "more_time"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Authorize +8h weekend overtime shift for Work Center?"
          RequiredRoles = [ "Planner"; "PlantManager" ]
          IsPrimary = true }

    let maintenance: ActionDescriptor =
        { Id = ActionDefId.literal "capacity.maintenance_window"
          DisplayName = "Schedule Maintenance"
          Icon = "build"
          Scope = SingleEntityScope
          ExecutionMode = Parameterized("maintenance-form", "Schedule Maintenance Window")
          RequiredRoles = [ "Planner"; "PlantManager" ]
          IsPrimary = false }

    let reallocate: ActionDescriptor =
        { Id = ActionDefId.literal "capacity.reallocate"
          DisplayName = "Reallocate Workload"
          Icon = "alt_route"
          Scope = SingleEntityScope
          ExecutionMode = Parameterized("capacity-reallocate-form", "Reallocate Workload to Alternate Line")
          RequiredRoles = [ "Planner"; "PlantManager" ]
          IsPrimary = false }

    let explainBottleneck: ActionDescriptor =
        { Id = ActionDefId.literal "capacity.explain"
          DisplayName = "Explain Bottleneck"
          Icon = "auto_awesome"
          Scope = SingleEntityScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "PlantManager" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ addShift; maintenance; reallocate; explainBottleneck ]

let addShift = CapacityActions.addShift
let maintenance = CapacityActions.maintenance
let reallocate = CapacityActions.reallocate
let explainBottleneck = CapacityActions.explainBottleneck
let allActions = CapacityActions.all

let storeId = DataStoreId.literal "capacity-store"
let schemaId = SchemaDefId.literal "capacity-grid-schema"
let definitionId = ComponentDefId.literal "capacity-grid"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Work Center Capacity Schema"
      Capabilities = [ "Tabular" ]
      SupportedActions = CapacityActions.all |> List.map (fun a -> a.Id)
      Actions = CapacityActions.all
      Columns =
        [ { ColumnId = "BottleneckAssessment"
            PropertyName = Some "BottleneckAssessment"
            Title = "Bottleneck Risk"
            DefaultOrder = 0
            DefaultWidthInPixels = Some 140
            DefaultVisible = true
            DefaultPinned = true
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = Some "StatusBadge"
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "PhysicalResourceId"
            PropertyName = Some "PhysicalResourceId"
            Title = "Work Center / Line"
            DefaultOrder = 1
            DefaultWidthInPixels = Some 150
            DefaultVisible = true
            DefaultPinned = true
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = false
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = true
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "UtilizationStatus"
            PropertyName = Some "UtilizationStatus"
            Title = "Utilization Status"
            DefaultOrder = 2
            DefaultWidthInPixels = Some 130
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = Some "StatusBadge"
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "CurrentUtilizationPercentage"
            PropertyName = Some "CurrentUtilizationPercentage"
            Title = "Current Utilization"
            DefaultOrder = 3
            DefaultWidthInPixels = Some 180
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Number
            Filterable = true
            Groupable = false
            RenderHint = Some "OperatingEnvelopeBar"
            Alignment = Right
            CellType = NumberType
            IsKey = false
            FormatString = Some "N1"
            UserConfigurable = true }
          { ColumnId = "CurrentLoadHours"
            PropertyName = Some "CurrentLoadHours"
            Title = "Current Load (h)"
            DefaultOrder = 4
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Number
            Filterable = true
            Groupable = false
            RenderHint = None
            Alignment = Right
            CellType = NumberType
            IsKey = false
            FormatString = Some "N1"
            UserConfigurable = true }
          { ColumnId = "AvailableCapacityHours"
            PropertyName = Some "AvailableCapacityHours"
            Title = "Capacity (h)"
            DefaultOrder = 5
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Number
            Filterable = true
            Groupable = false
            RenderHint = None
            Alignment = Right
            CellType = NumberType
            IsKey = false
            FormatString = Some "N1"
            UserConfigurable = true }
          { ColumnId = "ProjectedLoadHours"
            PropertyName = Some "ProjectedLoadHours"
            Title = "Projected Load (h)"
            DefaultOrder = 6
            DefaultWidthInPixels = Some 130
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Number
            Filterable = true
            Groupable = false
            RenderHint = None
            Alignment = Right
            CellType = NumberType
            IsKey = false
            FormatString = Some "N1"
            UserConfigurable = true }
          { ColumnId = "ProjectedUtilizationPercentage"
            PropertyName = Some "ProjectedUtilizationPercentage"
            Title = "Projected Util %"
            DefaultOrder = 7
            DefaultWidthInPixels = Some 120
            DefaultVisible = false
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Number
            Filterable = true
            Groupable = false
            RenderHint = None
            Alignment = Right
            CellType = NumberType
            IsKey = false
            FormatString = Some "N1"
            UserConfigurable = true }
          { ColumnId = "ConstraintStatus"
            PropertyName = Some "ConstraintStatus"
            Title = "Constraint State"
            DefaultOrder = 8
            DefaultWidthInPixels = Some 130
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = Some "StatusBadge"
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "OverloadRisk"
            PropertyName = Some "OverloadRisk"
            Title = "Overload Risk"
            DefaultOrder = 9
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = Some "StatusBadge"
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "SinglePointOfFailureRisk"
            PropertyName = Some "SinglePointOfFailureRisk"
            Title = "SPOF Risk"
            DefaultOrder = 10
            DefaultWidthInPixels = Some 110
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Boolean
            Filterable = true
            Groupable = true
            RenderHint = Some "LockIcon"
            Alignment = Center
            CellType = BooleanType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "AssessmentConfidence"
            PropertyName = Some "AssessmentConfidence"
            Title = "Confidence"
            DefaultOrder = 11
            DefaultWidthInPixels = Some 110
            DefaultVisible = false
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "LifecycleState"
            PropertyName = Some "LifecycleState"
            Title = "State"
            DefaultOrder = 12
            DefaultWidthInPixels = Some 100
            DefaultVisible = false
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = Some "StatusBadge"
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "AssessmentTimestamp"
            PropertyName = Some "AssessmentTimestamp"
            Title = "Assessed At"
            DefaultOrder = 13
            DefaultWidthInPixels = Some 150
            DefaultVisible = false
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some FilterValueKind.DateTimeOffset
            Filterable = true
            Groupable = false
            RenderHint = None
            Alignment = Left
            CellType = DateTimeType
            IsKey = false
            FormatString = Some "yyyy-MM-dd HH:mm"
            UserConfigurable = true } ]
      DefaultSort =
        Some
            { Field = "CurrentUtilizationPercentage"
              Direction = Descending
              Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<CapacityPositionAssessmentDto> =
    match ProjectionCompiler.compile<CapacityPositionAssessmentDto> schema with
    | Valid a -> a
    | Invalid _ -> { ToUniversal = fun _ -> { Key = ""; Cells = Map.empty } }

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Work Center Capacity Workbench"
      Icon = "precision_manufacturing"
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

