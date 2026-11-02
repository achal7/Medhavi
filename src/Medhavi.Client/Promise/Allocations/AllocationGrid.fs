module Medhavi.Client.Promise.Allocations.AllocationGrid

open System
open Medhavi.Contracts.Promise
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

module AllocationActions =

    let initializePool: ActionDescriptor =
        { Id = ActionDefId.literal "allocation.initialize"
          DisplayName = "Initialize Allocation Pool"
          Icon = "pie_chart"
          Scope = GlobalScope
          ExecutionMode = Parameterized("init-allocation-pool-form", "Partition Supply into Channel/Tier Buckets")
          RequiredRoles = [ "Planner"; "InventoryManager" ]
          IsPrimary = true }

    let rebalance: ActionDescriptor =
        { Id = ActionDefId.literal "allocation.rebalance"
          DisplayName = "Rebalance Allocation Quotas"
          Icon = "balance"
          Scope = SingleEntityScope
          ExecutionMode = Parameterized("rebalance-pool-form", "Transfer Quota Across Channels or Tiers")
          RequiredRoles = [ "Planner"; "CommercialLead" ]
          IsPrimary = false }

    let calculatePriority: ActionDescriptor =
        { Id = ActionDefId.literal "allocation.calculate_priority"
          DisplayName = "Score Dynamic Order Priority"
          Icon = "trending_up"
          Scope = SingleEntityScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "CustomerService" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ initializePool; rebalance; calculatePriority ]

let schemaId = SchemaDefId.literal "allocation-pool-schema"
let storeId = DataStoreId.literal "allocation-pool-store"
let definitionId = ComponentDefId.literal "allocation-pool-grid"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Supply Allocation Pools & Buckets (SE-P-020)"
      Capabilities = [ "Tabular" ]
      SupportedActions = AllocationActions.all |> List.map (fun a -> a.Id)
      Actions = AllocationActions.all
      Columns =
        [ { ColumnId = "PoolId"
            PropertyName = Some "PoolId"
            Title = "Pool ID"
            DefaultOrder = 0
            DefaultWidthInPixels = Some 180
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
          { ColumnId = "Item"
            PropertyName = Some "Item"
            Title = "Product SKU"
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
            FormatString = None }
          { ColumnId = "Location"
            PropertyName = Some "Location"
            Title = "Location"
            DefaultOrder = 2
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
          { ColumnId = "PlanningPeriod"
            PropertyName = Some "PlanningPeriod"
            Title = "Period"
            DefaultOrder = 3
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = None
            Alignment = Center
            CellType = TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "TotalPlannedSupply"
            PropertyName = Some "TotalPlannedSupply"
            Title = "Planned Supply"
            DefaultOrder = 4
            DefaultWidthInPixels = Some 140
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
            FormatString = Some "#,##0.00" }
          { ColumnId = "TotalAllocatedQuantity"
            PropertyName = Some "TotalAllocatedQuantity"
            Title = "Allocated Quota"
            DefaultOrder = 5
            DefaultWidthInPixels = Some 140
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
            FormatString = Some "#,##0.00" }
          { ColumnId = "PlanningScopeId"
            PropertyName = Some "PlanningScopeId"
            Title = "Planning Scope"
            DefaultOrder = 6
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
          { ColumnId = "LifecycleState"
            PropertyName = Some "LifecycleState"
            Title = "Lifecycle State"
            DefaultOrder = 7
            DefaultWidthInPixels = Some 120
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
          { ColumnId = "DecisionTraceability"
            PropertyName = Some "DecisionTraceability"
            Title = "Decision Trace"
            DefaultOrder = 8
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
            FormatString = None } ]
      DefaultSort =
        Some
            { Field = "PoolId"
              Direction = Ascending
              Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<AllocationPoolDto> =
    match ProjectionCompiler.compile<AllocationPoolDto> schema with
    | Valid a -> a
    | Invalid errs ->
        let msg = errs |> String.concat "; "
        invalidOp $"AllocationGrid compilation failed: {msg}"

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Supply Allocation Pools"
      Icon = "pie_chart"
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

