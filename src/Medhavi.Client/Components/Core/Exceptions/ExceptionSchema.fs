namespace Medhavi.Client.Components.Core.Exceptions

open System
open Medhavi.Contracts.Core.Exception
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ViewComposition.Context
open Medhavi.Common.Validation
open Medhavi.Client.Components.Shared

module ExceptionActions =

    let resolve: ActionDescriptor =
        { Id = ActionDefId.literal "exception.resolve"
          DisplayName = "Resolve Exception"
          Icon = "check_circle"
          Scope = SingleEntityScope
          ExecutionMode = Parameterized("exception-resolve-form", "Resolve Core Exception")
          RequiredRoles = [ "Planner"; "Supervisor" ]
          IsPrimary = true }

    let investigate: ActionDescriptor =
        { Id = ActionDefId.literal "exception.investigate"
          DisplayName = "Investigate Constraint"
          Icon = "search"
          Scope = SingleEntityScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "Supervisor" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ resolve; investigate ]

module ExceptionSchema =

    let schemaId = SchemaDefId.literal "exception-grid-schema"

    let definition: SchemaDefinition =
        { Id = schemaId
          DisplayName = "Core Constraint Exceptions"
          Capabilities = [ "Tabular" ]
          SupportedActions = ExceptionActions.all |> List.map(fun a -> a.Id)
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
              { ColumnId = "RegistrationTime"
                PropertyName = Some "RegistrationTime"
                Title = "Registered At"
                DefaultOrder = 7
                DefaultWidthInPixels = Some 130
                DefaultVisible = true
                DefaultPinned = false
                Sortable = true
                Filterable = true
                Groupable = false
                FilterValueKind = Some DateTimeOffset
                UserConfigurable = true
                RenderHint = None
                Alignment = Left
                CellType = DateTimeType
                IsKey = false
                FormatString = Some "yyyy-MM-dd HH:mm" }
              { ColumnId = "ResolutionTime"
                PropertyName = Some "ResolutionTime"
                Title = "Resolved At"
                DefaultOrder = 8
                DefaultWidthInPixels = Some 130
                DefaultVisible = false
                DefaultPinned = false
                Sortable = true
                Filterable = true
                Groupable = false
                FilterValueKind = Some DateTimeOffset
                UserConfigurable = true
                RenderHint = None
                Alignment = Left
                CellType = DateTimeType
                IsKey = false
                FormatString = Some "yyyy-MM-dd HH:mm" } ]
          DefaultSort =
            Some
                { Field = "RegistrationTime"
                  Direction = Descending
                  Priority = 1 }
          DefaultFilters = [] }

    let adapter =
        match ProjectionCompiler.compile<ExceptionDto> definition with
        | Valid a -> a
        | Invalid errs -> failwithf "ExceptionSchema compilation failed: %A" errs
