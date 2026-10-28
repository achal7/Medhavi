module Medhavi.Client.Demand.Classification.DemandBehaviorClassificationGrid

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

/// Declarative metadata descriptors for Demand Behavior Classification actions.
module DemandClassificationActions =

    let classifyBehavior: ActionDescriptor =
        {
            Id = ActionDefId.literal "demand_classification.classify_behavior"
            DisplayName = "Classify Behavior"
            Icon = "analytics"
            Scope = SingleEntityScope
            ExecutionMode = Direct
            RequiredRoles = [ "Planner"; "DemandAnalyst" ]
            IsPrimary = true
        }

    let overrideBehavior: ActionDescriptor =
        {
            Id = ActionDefId.literal "demand_classification.override_behavior"
            DisplayName = "Override Behavior"
            Icon = "edit_attributes"
            Scope = SingleEntityScope
            ExecutionMode =
                RequiresConfirmation "Override demand behavior pattern classification for this item-location?"
            RequiredRoles = [ "Planner"; "LeadPlanner" ]
            IsPrimary = false
        }

    let all: ActionDescriptor list = [ classifyBehavior; overrideBehavior ]

let schemaId = SchemaDefId.literal "demand-classification-schema"
let storeId = DataStoreId.literal "demand-classification-store"
let definitionId = ComponentDefId.literal "comp-demand-classification"

let schema: SchemaDefinition =
    {
        Id = schemaId
        DisplayName = "Demand Behavior Pattern Classification (Smooth / Intermittent / Lumpy / Erratic)"
        Capabilities = [ "Tabular" ]
        SupportedActions = DemandClassificationActions.all |> List.map(fun a -> a.Id)
        Actions = DemandClassificationActions.all
        Columns =
            [
                {
                    ColumnId = "AssignmentId"
                    PropertyName = Some "AssignmentId"
                    Title = "Assignment ID"
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
                    FormatString = None
                }
                {
                    ColumnId = "ItemId"
                    PropertyName = Some "ItemId"
                    Title = "Item"
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
                    FormatString = None
                }
                {
                    ColumnId = "LocationId"
                    PropertyName = Some "LocationId"
                    Title = "Location"
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
                    FormatString = None
                }
                {
                    ColumnId = "Dimension"
                    PropertyName = Some "Dimension"
                    Title = "Dimension"
                    DefaultOrder = 3
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
                    ColumnId = "CurrentClassification"
                    PropertyName = Some "CurrentClassification"
                    Title = "Pattern"
                    DefaultOrder = 4
                    DefaultWidthInPixels = Some 140
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
                    ColumnId = "ClassificationConfidence"
                    PropertyName = Some "ClassificationConfidence"
                    Title = "Confidence"
                    DefaultOrder = 5
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
                    ColumnId = "AssignmentRationale"
                    PropertyName = Some "AssignmentRationale"
                    Title = "Rationale"
                    DefaultOrder = 6
                    DefaultWidthInPixels = Some 220
                    DefaultVisible = true
                    DefaultPinned = false
                    Sortable = false
                    Filterable = true
                    Groupable = false
                    FilterValueKind = Some Text
                    UserConfigurable = true
                    RenderHint = None
                    Alignment = Left
                    CellType = TextType
                    IsKey = false
                    FormatString = None
                }
                {
                    ColumnId = "PolicyVersion"
                    PropertyName = Some "PolicyVersion"
                    Title = "Policy"
                    DefaultOrder = 7
                    DefaultWidthInPixels = Some 100
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
                    FormatString = None
                }
                {
                    ColumnId = "LastUpdated"
                    PropertyName = Some "LastUpdated"
                    Title = "Updated"
                    DefaultOrder = 8
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
                    FormatString = Some "yyyy-MM-dd HH:mm"
                }
            ]
        DefaultSort =
            Some
                {
                    Field = "CurrentClassification"
                    Direction = Ascending
                    Priority = 1
                }
        DefaultFilters = []
    }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<DemandBehaviorAssignmentDto> =
    match ProjectionCompiler.compile<DemandBehaviorAssignmentDto> schema with
    | Valid a -> a
    | Invalid errs ->
        let msg = errs |> String.concat "; "
        invalidOp $"DemandBehaviorClassificationGrid compilation failed: {msg}"

let componentDefinition: ComponentDefinition =
    {
        Id = definitionId
        DisplayName = "Demand Behavior Classification Studio"
        Icon = "auto_graph"
        Category = ComponentCategory.DataDisplay
        RequiredSchemaCapabilities = [ "Tabular" ]
        DefaultGridSpan = GridSpan.literal 12 6
        DefaultSchemaId = Some schemaId
        DefaultStoreId = Some storeId
        IsResizable = true
        IsDraggable = true
        SelectionMode = SingleSelection
    }

let runtime: ComponentRuntime =
    {
        Render = UniversalEnterpriseGrid.render
    }

let registration: ComponentRegistration =
    {
        Definition = componentDefinition
        Schema = schema
        Runtime = runtime
    }
