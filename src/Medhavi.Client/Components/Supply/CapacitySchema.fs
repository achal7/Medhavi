namespace Medhavi.Client.Components.Supply

open System
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ViewComposition.Context
open Medhavi.Common.Validation
open Medhavi.Client.Components.Shared

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

type CapacityRow =
    { WorkCenterId: string
      PlantId: string
      LineName: string
      AvailableHours: decimal
      RequiredHours: decimal
      UtilizationPct: decimal
      ShiftPattern: string
      BottleneckStatus: string
      IsOvertimeAuthorized: bool }

module CapacitySchema =

    let schemaId = SchemaDefId.literal "capacity-grid-schema"

    let definition: SchemaDefinition =
        { Id = schemaId
          DisplayName = "Work Center Capacity Schema"
          Capabilities = [ "Tabular" ]
          SupportedActions = CapacityActions.all |> List.map(fun a -> a.Id)
          Columns =
            [ { ColumnId = "BottleneckStatus"
                PropertyName = Some "BottleneckStatus"
                Title = "Bottleneck Risk"
                DefaultOrder = 0
                DefaultWidthInPixels = Some 130
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
              { ColumnId = "WorkCenterId"
                PropertyName = Some "WorkCenterId"
                Title = "Work Center"
                DefaultOrder = 1
                DefaultWidthInPixels = Some 130
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
              { ColumnId = "PlantId"
                PropertyName = Some "PlantId"
                Title = "Plant"
                DefaultOrder = 2
                DefaultWidthInPixels = Some 120
                DefaultVisible = true
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
              { ColumnId = "LineName"
                PropertyName = Some "LineName"
                Title = "Line Description"
                DefaultOrder = 3
                DefaultWidthInPixels = Some 180
                DefaultVisible = true
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
              { ColumnId = "AvailableHours"
                PropertyName = Some "AvailableHours"
                Title = "Avail Hours"
                DefaultOrder = 4
                DefaultWidthInPixels = Some 110
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
              { ColumnId = "RequiredHours"
                PropertyName = Some "RequiredHours"
                Title = "Req Hours"
                DefaultOrder = 5
                DefaultWidthInPixels = Some 110
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
              { ColumnId = "UtilizationPct"
                PropertyName = Some "UtilizationPct"
                Title = "Utilization %"
                DefaultOrder = 6
                DefaultWidthInPixels = Some 110
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
              { ColumnId = "ShiftPattern"
                PropertyName = Some "ShiftPattern"
                Title = "Shift Pattern"
                DefaultOrder = 7
                DefaultWidthInPixels = Some 140
                DefaultVisible = true
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
              { ColumnId = "IsOvertimeAuthorized"
                PropertyName = Some "IsOvertimeAuthorized"
                Title = "Overtime"
                DefaultOrder = 8
                DefaultWidthInPixels = Some 90
                DefaultVisible = true
                DefaultPinned = false
                Sortable = true
                FilterValueKind = Some Boolean
                Filterable = true
                Groupable = false
                RenderHint = Some "LockIcon"
                Alignment = Center
                CellType = BooleanType
                IsKey = false
                FormatString = None
                UserConfigurable = true } ]
          DefaultSort = None
          DefaultFilters = [] }

    let adapter: ProjectionAdapter<CapacityRow> =
        match ProjectionCompiler.compile<CapacityRow> definition with
        | Valid a -> a
        | Invalid errs -> failwithf "CapacitySchema compilation failed: %A" errs

    let toUniversal: CapacityRow -> UniversalRow = adapter.ToUniversal
