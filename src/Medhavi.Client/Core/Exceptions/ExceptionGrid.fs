module Medhavi.Client.Core.Exceptions.ExceptionGrid

open System
open Medhavi.Contracts.Core.Exception
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

/// Declarative metadata descriptors for Core Exception actions.
module ExceptionActions =

    let acknowledge: ActionDescriptor =
        { Id = ActionDefId.literal "exception.acknowledge"
          DisplayName = "Acknowledge Exception"
          Icon = "done"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Acknowledge this active constraint exception?"
          RequiredRoles = [ "Planner"; "Supervisor" ]
          IsPrimary = false }

    let resolve: ActionDescriptor =
        { Id = ActionDefId.literal "exception.resolve"
          DisplayName = "Resolve Exception"
          Icon = "check_circle"
          Scope = SingleEntityScope
          ExecutionMode = Parameterized("exception-resolve-form", "Resolve Core Exception")
          RequiredRoles = [ "Planner"; "Supervisor" ]
          IsPrimary = true }

    let overrideRule: ActionDescriptor =
        { Id = ActionDefId.literal "exception.overrideRule"
          DisplayName = "Override Rule"
          Icon = "rule"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Override this constraint rule and mark exception resolved?"
          RequiredRoles = [ "Supervisor"; "LeadPlanner" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ acknowledge; resolve; overrideRule ]

let acknowledge = ExceptionActions.acknowledge
let resolve = ExceptionActions.resolve
let overrideRule = ExceptionActions.overrideRule
let allActions = ExceptionActions.all

let schemaId = SchemaDefId.literal "core-exception-grid-schema"
let storeId: DataStoreId = DataStoreId.literal "core-exception-store"
let definitionId = ComponentDefId.literal "exception-grid"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Core Constraint Exceptions"
      Capabilities = [ "Tabular" ]
      SupportedActions = ExceptionActions.all |> List.map (fun a -> a.Id)
      Actions = ExceptionActions.all
      Columns =
        [ { ColumnId = "LifecycleState"
            PropertyName = Some "LifecycleState"
            Title = "Status"
            DefaultOrder = 0
            DefaultWidthInPixels = Some 110
            DefaultVisible = true
            DefaultPinned = true
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
          { ColumnId = "ExceptionId"
            PropertyName = Some "ExceptionId"
            Title = "Exception ID"
            DefaultOrder = 1
            DefaultWidthInPixels = Some 130
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
          { ColumnId = "Classification"
            PropertyName = Some "Classification"
            Title = "Classification"
            DefaultOrder = 2
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
          { ColumnId = "ConstraintReference"
            PropertyName = Some "ConstraintReference"
            Title = "Constraint"
            DefaultOrder = 3
            DefaultWidthInPixels = Some 160
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
          { ColumnId = "AffectedScopeType"
            PropertyName = Some "AffectedScopeType"
            Title = "Scope Type"
            DefaultOrder = 4
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
          { ColumnId = "AffectedScopeIdentifier"
            PropertyName = Some "AffectedScopeIdentifier"
            Title = "Scope Identifier"
            DefaultOrder = 5
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
          { ColumnId = "Severity"
            PropertyName = Some "Severity"
            Title = "Severity"
            DefaultOrder = 6
            DefaultWidthInPixels = Some 100
            DefaultVisible = true
            DefaultPinned = false
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
          { ColumnId = "EvidenceReference"
            PropertyName = Some "EvidenceReference"
            Title = "Evidence"
            DefaultOrder = 7
            DefaultWidthInPixels = Some 200
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
          { ColumnId = "RegistrationTime"
            PropertyName = Some "RegistrationTime"
            Title = "Registered At"
            DefaultOrder = 8
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
            FormatString = Some "yyyy-MM-dd HH:mm" }
          { ColumnId = "ResolutionTime"
            PropertyName = Some "ResolutionTime"
            Title = "Resolved At"
            DefaultOrder = 9
            DefaultWidthInPixels = Some 130
            DefaultVisible = false
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
            FormatString = Some "yyyy-MM-dd HH:mm" }
          { ColumnId = "ResolutionEvidence"
            PropertyName = Some "ResolutionEvidence"
            Title = "Resolution Evidence"
            DefaultOrder = 10
            DefaultWidthInPixels = Some 180
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
            FormatString = None } ]
      DefaultSort =
        Some
            { Field = "RegistrationTime"
              Direction = Descending
              Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<ExceptionDto> =
    match ProjectionCompiler.compile<ExceptionDto> schema with
    | Valid a -> a
    | Invalid errs ->
        let msg = String.concat "; " errs
        invalidOp $"ExceptionGrid compilation failed: {msg}"

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Core Exceptions Workbench"
      Icon = "warning"
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
