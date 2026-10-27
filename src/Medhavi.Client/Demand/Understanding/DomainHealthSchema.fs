module Medhavi.Client.Demand.Understanding.DomainHealthSchema

open System
open Medhavi.Contracts.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ViewComposition.Context
open Medhavi.Common.Validation
open Medhavi.Client.Components.Shared

/// Declarative metadata descriptors for Demand Understanding & Domain Health actions.
module DomainHealthActions =

    let revise: ActionDescriptor =
        {
            Id = ActionDefId.literal "demand_understanding.revise"
            DisplayName = "Revise Understanding"
            Icon = "refresh"
            Scope = SingleEntityScope
            ExecutionMode = Direct
            RequiredRoles = [ "Planner"; "DemandAnalyst" ]
            IsPrimary = true
        }

    let publish: ActionDescriptor =
        {
            Id = ActionDefId.literal "demand_understanding.publish"
            DisplayName = "Publish Understanding"
            Icon = "publish"
            Scope = SingleEntityScope
            ExecutionMode =
                RequiresConfirmation "Publish this demand understanding assessment to the enterprise ledger?"
            RequiredRoles = [ "Planner"; "LeadPlanner" ]
            IsPrimary = false
        }

    let explain: ActionDescriptor =
        {
            Id = ActionDefId.literal "demand_understanding.explain"
            DisplayName = "Explain Understanding"
            Icon = "psychology"
            Scope = SingleEntityScope
            ExecutionMode = Direct
            RequiredRoles = [ "Planner"; "Viewer" ]
            IsPrimary = false
        }

    let all: ActionDescriptor list = [ revise; publish; explain ]

let schemaId = SchemaDefId.literal "demand-understanding-health-schema"

let definition: SchemaDefinition =
    {
        Id = schemaId
        DisplayName = "Demand Understanding Domain Health"
        Capabilities = [ "Radar"; "Scorecard"; "Tabular" ]
        SupportedActions = DomainHealthActions.all |> List.map(fun a -> a.Id)
        Actions = DomainHealthActions.all
        Columns =
            [
                {
                    ColumnId = "PlanningScopeId"
                    PropertyName = Some "PlanningScopeId"
                    Title = "Planning Scope"
                    DefaultOrder = 0
                    DefaultWidthInPixels = Some 160
                    DefaultVisible = true
                    DefaultPinned = true
                    Sortable = true
                    Filterable = true
                    Groupable = true
                    FilterValueKind = Some Text
                    UserConfigurable = true
                    RenderHint = None
                    Alignment = Left
                    CellType = TextType
                    IsKey = true
                    FormatString = None
                }
                {
                    ColumnId = "VersionNumber"
                    PropertyName = Some "VersionNumber"
                    Title = "Version"
                    DefaultOrder = 1
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
                    FormatString = Some "N0"
                }
                {
                    ColumnId = "State"
                    PropertyName = Some "State"
                    Title = "Status"
                    DefaultOrder = 2
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
                    FormatString = None
                }
                {
                    ColumnId = "LastPublishedTime"
                    PropertyName = Some "LastPublishedTime"
                    Title = "Last Published"
                    DefaultOrder = 3
                    DefaultWidthInPixels = Some 160
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
                    FormatString = Some "yyyy-MM-dd HH:mm"
                }
                {
                    ColumnId = "Interpretation"
                    PropertyName = Some "Interpretation"
                    Title = "Interpretation Assessment"
                    DefaultOrder = 4
                    DefaultWidthInPixels = Some 220
                    DefaultVisible = true
                    DefaultPinned = false
                    Sortable = false
                    Filterable = false
                    Groupable = false
                    FilterValueKind = Some Text
                    UserConfigurable = true
                    RenderHint = None
                    Alignment = Left
                    CellType = TextType
                    IsKey = false
                    FormatString = None
                }
            ]
        DefaultSort =
            Some
                {
                    Field = "PlanningScopeId"
                    Direction = Ascending
                    Priority = 1
                }
        DefaultFilters = []
    }

let adapter: ProjectionAdapter<DemandUnderstandingDto> =
    match ProjectionCompiler.compile<DemandUnderstandingDto> definition with
    | Valid a -> a
    | Invalid errs ->
        let msg = errs |> String.concat "; "
        invalidOp $"DomainHealthSchema compilation failed: {msg}"
