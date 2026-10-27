module Medhavi.Client.Core.Traceability.TraceabilityGridSchema

open System
open Medhavi.Contracts.Traceability
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ViewComposition.Context
open Medhavi.Common.Validation
open Medhavi.Client.Components.Shared

/// Tabular representation of an end-to-end Telemetry Trace Event.
type TraceabilityRow =
    { EventId: string
      Timestamp: DateTimeOffset
      Severity: string
      BoundedContext: string
      AggregateType: string
      AggregateId: string
      CommandType: string
      Message: string
      CorrelationId: string
      CausationId: string
      HasDecision: bool
      HasErrors: bool }

/// Declarative metadata descriptors for Traceability telemetry actions.
let inspectDossier: ActionDescriptor =
    { Id = ActionDefId.literal "traceability.inspectDossier"
      DisplayName = "Inspect Causal Dossier"
      Icon = "policy"
      Scope = SingleEntityScope
      ExecutionMode = Direct
      RequiredRoles = [ "Planner"; "Auditor"; "LeadPlanner"; "Administrator" ]
      IsPrimary = true }

let exportTrace: ActionDescriptor =
    { Id = ActionDefId.literal "traceability.exportTrace"
      DisplayName = "Export Trace Log"
      Icon = "file_download"
      Scope = GlobalScope
      ExecutionMode = Direct
      RequiredRoles = [ "Planner"; "Auditor"; "LeadPlanner"; "Administrator" ]
      IsPrimary = false }

let allActions: ActionDescriptor list = [ inspectDossier; exportTrace ]

let schemaId = SchemaDefId.literal "traceability-workbench-schema"

let definition: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Traceability & Investigation Telemetry"
      Capabilities = [ "Tabular"; "Detail" ]
      SupportedActions = allActions |> List.map (fun a -> a.Id)
      Actions = allActions
      Columns =
        [ { ColumnId = "Timestamp"
            PropertyName = Some "Timestamp"
            Title = "Timestamp"
            DefaultOrder = 0
            DefaultWidthInPixels = Some 160
            DefaultVisible = true
            DefaultPinned = true
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some FilterValueKind.DateTimeOffset
            UserConfigurable = true
            RenderHint = None
            Alignment = Left
            CellType = DateTimeType
            IsKey = false
            FormatString = Some "HH:mm:ss.fff" }
          { ColumnId = "Severity"
            PropertyName = Some "Severity"
            Title = "Severity"
            DefaultOrder = 1
            DefaultWidthInPixels = Some 90
            DefaultVisible = true
            DefaultPinned = true
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = Some "StatusBadge"
            Alignment = Center
            CellType = TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "BoundedContext"
            PropertyName = Some "BoundedContext"
            Title = "Domain"
            DefaultOrder = 2
            DefaultWidthInPixels = Some 110
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
          { ColumnId = "AggregateType"
            PropertyName = Some "AggregateType"
            Title = "Aggregate"
            DefaultOrder = 3
            DefaultWidthInPixels = Some 130
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
          { ColumnId = "AggregateId"
            PropertyName = Some "AggregateId"
            Title = "Aggregate ID"
            DefaultOrder = 4
            DefaultWidthInPixels = Some 150
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
          { ColumnId = "CommandType"
            PropertyName = Some "CommandType"
            Title = "Action / Command"
            DefaultOrder = 5
            DefaultWidthInPixels = Some 180
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
          { ColumnId = "Message"
            PropertyName = Some "Message"
            Title = "Lifecycle Stage / Message"
            DefaultOrder = 6
            DefaultWidthInPixels = Some 180
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
          { ColumnId = "CorrelationId"
            PropertyName = Some "CorrelationId"
            Title = "Correlation ID"
            DefaultOrder = 7
            DefaultWidthInPixels = Some 170
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
          { ColumnId = "CausationId"
            PropertyName = Some "CausationId"
            Title = "Causation ID"
            DefaultOrder = 8
            DefaultWidthInPixels = Some 150
            DefaultVisible = false
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
          { ColumnId = "HasDecision"
            PropertyName = Some "HasDecision"
            Title = "Has Decision"
            DefaultOrder = 9
            DefaultWidthInPixels = Some 90
            DefaultVisible = false
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some Boolean
            UserConfigurable = true
            RenderHint = None
            Alignment = Center
            CellType = BooleanType
            IsKey = false
            FormatString = None }
          { ColumnId = "HasErrors"
            PropertyName = Some "HasErrors"
            Title = "Has Errors"
            DefaultOrder = 10
            DefaultWidthInPixels = Some 90
            DefaultVisible = false
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some Boolean
            UserConfigurable = true
            RenderHint = None
            Alignment = Center
            CellType = BooleanType
            IsKey = false
            FormatString = None }
          { ColumnId = "EventId"
            PropertyName = Some "EventId"
            Title = "Event ID"
            DefaultOrder = 11
            DefaultWidthInPixels = Some 100
            DefaultVisible = false
            DefaultPinned = false
            Sortable = false
            Filterable = false
            Groupable = false
            FilterValueKind = Some Text
            UserConfigurable = false
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = true
            FormatString = None } ]
      DefaultSort =
        Some
            { Field = "Timestamp"
              Direction = Descending
              Priority = 1 }
      DefaultFilters = [] }

let adapter: ProjectionAdapter<TraceabilityRow> =
    match ProjectionCompiler.compile<TraceabilityRow> definition with
    | Valid a -> a
    | Invalid errs ->
        let msg = String.concat "; " errs
        invalidOp $"TraceabilityGridSchema compilation failed: {msg}"
