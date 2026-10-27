module Medhavi.Client.Supply.Exceptions.SupplyExceptionGridSchema

open System
open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ViewComposition.Context

let explainRootCause: ActionDescriptor =
    { Id = ActionDefId.literal "act-explain-root-cause"
      DisplayName = "Explain Root Cause"
      Icon = "auto_awesome"
      Scope = SingleEntityScope
      ExecutionMode = Direct
      RequiredRoles = [ "Planner"; "Executive" ]
      IsPrimary = true }

let confirm: ActionDescriptor =
    { Id = ActionDefId.literal "act-confirm-exception"
      DisplayName = "Confirm Exception"
      Icon = "check_circle"
      Scope = SingleEntityScope
      ExecutionMode = RequiresConfirmation "Confirm this supply exception for mitigation?"
      RequiredRoles = [ "Planner" ]
      IsPrimary = false }

let resolve: ActionDescriptor =
    { Id = ActionDefId.literal "act-resolve-exception"
      DisplayName = "Resolve Exception"
      Icon = "task_alt"
      Scope = SingleEntityScope
      ExecutionMode = RequiresConfirmation "Mark this supply exception as resolved?"
      RequiredRoles = [ "Planner" ]
      IsPrimary = false }

let escalate: ActionDescriptor =
    { Id = ActionDefId.literal "act-escalate-exception"
      DisplayName = "Escalate to Core"
      Icon = "arrow_upward"
      Scope = SingleEntityScope
      ExecutionMode = RequiresConfirmation "Escalate this supply exception to enterprise core governance?"
      RequiredRoles = [ "Planner"; "PlantManager" ]
      IsPrimary = false }

let allActions: ActionDescriptor list = [ explainRootCause; confirm; resolve; escalate ]

let schemaId = SchemaDefId.literal "schema-supply-exceptions"

let definition: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Supply Exceptions (CA-S-011)"
      Capabilities = [ "Tabular"; "Exceptions" ]
      SupportedActions = allActions |> List.map (fun a -> a.Id)
      Actions = allActions
      Columns =
        [ { ColumnId = "AssessmentId"
            PropertyName = Some "AssessmentId"
            Title = "Exception ID"
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
          { ColumnId = "Severity"
            PropertyName = Some "Severity"
            Title = "Severity (Impact × Urgency)"
            DefaultOrder = 2
            DefaultWidthInPixels = Some 170
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = Some "SeverityVectorBadge"
            Alignment = TextAlignment.Center
            CellType = CellType.TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "ExceptionType"
            PropertyName = Some "ExceptionType"
            Title = "Classification"
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
          { ColumnId = "ScopeEntityId"
            PropertyName = Some "ScopeEntityId"
            Title = "Affected Entity"
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
          { ColumnId = "ScopeEntityType"
            PropertyName = Some "ScopeEntityType"
            Title = "Scope Type"
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
          { ColumnId = "PlanningScopeId"
            PropertyName = Some "PlanningScopeId"
            Title = "Planning Scope"
            DefaultOrder = 6
            DefaultWidthInPixels = Some 150
            DefaultVisible = false
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
          { ColumnId = "ExceptionHorizon"
            PropertyName = Some "ExceptionHorizon"
            Title = "Horizon"
            DefaultOrder = 7
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
          { ColumnId = "LifecycleState"
            PropertyName = Some "LifecycleState"
            Title = "State"
            DefaultOrder = 8
            DefaultWidthInPixels = Some 110
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
          { ColumnId = "EvaluationConfidence"
            PropertyName = Some "EvaluationConfidence"
            Title = "Confidence"
            DefaultOrder = 9
            DefaultWidthInPixels = Some 100
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
          { ColumnId = "DetectionTimestamp"
            PropertyName = Some "DetectionTimestamp"
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
      DefaultSort = Some { Field = "DetectionTimestamp"; Direction = Descending; Priority = 1 }
      DefaultFilters = [] }

let adapter: ProjectionAdapter<SupplyExceptionAssessmentDto> =
    { ToUniversal =
        fun (dto: SupplyExceptionAssessmentDto) ->
            let severityDisplay = $"{dto.BusinessImpact} / {dto.Urgency}"
            { Key = dto.AssessmentId
              Cells =
                Map.ofList
                    [ "AssessmentId", TextValue dto.AssessmentId
                      "Severity", TextValue severityDisplay
                      "ExceptionType", TextValue dto.ExceptionType
                      "ScopeEntityId", TextValue dto.ScopeEntityId
                      "ScopeEntityType", TextValue dto.ScopeEntityType
                      "PlanningScopeId", TextValue dto.PlanningScopeId
                      "ExceptionHorizon", TextValue dto.ExceptionHorizon
                      "LifecycleState", TextValue dto.LifecycleState
                      "EvaluationConfidence", TextValue dto.EvaluationConfidence
                      "DetectionTimestamp", DateTimeValue dto.DetectionTimestamp ] } }
