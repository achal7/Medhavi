module Medhavi.Client.Demand.Prioritization.PrioritizationGrid

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

/// Declarative metadata descriptors for Demand Planning Priority actions.
module PrioritizationActions =

    let prioritizeEntity: ActionDescriptor =
        { Id = ActionDefId.literal "demand_prioritization.prioritize_entity"
          DisplayName = "Prioritize Entity"
          Icon = "low_priority"
          Scope = SingleEntityScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "DemandAnalyst" ]
          IsPrimary = true }

    let overridePriority: ActionDescriptor =
        { Id = ActionDefId.literal "demand_prioritization.override_priority"
          DisplayName = "Override Priority"
          Icon = "flag"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Apply priority override to this planning entity?"
          RequiredRoles = [ "Planner"; "LeadPlanner" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ prioritizeEntity; overridePriority ]

let schemaId = SchemaDefId.literal "demand-prioritization-schema"
let storeId = DataStoreId.literal "demand-prioritization-store"
let definitionId = ComponentDefId.literal "comp-demand-prioritization"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Demand Planning Priority Ranking"
      Capabilities = [ "Tabular" ]
      SupportedActions = PrioritizationActions.all |> List.map (fun a -> a.Id)
      Actions = PrioritizationActions.all
      Columns =
        [ { ColumnId = "AssignmentId"
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
            FormatString = None }
          { ColumnId = "EntityType"
            PropertyName = Some "EntityType"
            Title = "Entity Type"
            DefaultOrder = 1
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
          { ColumnId = "EntityId"
            PropertyName = Some "EntityId"
            Title = "Entity ID"
            DefaultOrder = 2
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
          { ColumnId = "CurrentPriority"
            PropertyName = Some "CurrentPriority"
            Title = "Priority Tier"
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
            FormatString = None }
          { ColumnId = "PriorityScore"
            PropertyName = Some "PriorityScore"
            Title = "Score"
            DefaultOrder = 4
            DefaultWidthInPixels = Some 110
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
            FormatString = Some "N2" }
          { ColumnId = "DecisionRationale"
            PropertyName = Some "DecisionRationale"
            Title = "Rationale"
            DefaultOrder = 5
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
            FormatString = None }
          { ColumnId = "BusinessValidity"
            PropertyName = Some "BusinessValidity"
            Title = "Validity"
            DefaultOrder = 6
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
          { ColumnId = "PolicyVersion"
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
            FormatString = None }
          { ColumnId = "LastUpdated"
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
            FormatString = Some "yyyy-MM-dd HH:mm" } ]
      DefaultSort =
        Some
            { Field = "PriorityScore"
              Direction = Descending
              Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<PlanningPriorityDto> =
    match ProjectionCompiler.compile<PlanningPriorityDto> schema with
    | Valid a -> a
    | Invalid errs ->
        let msg = errs |> String.concat "; "
        invalidOp $"PrioritizationGrid compilation failed: {msg}"

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Demand Prioritization Studio"
      Icon = "rule"
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
