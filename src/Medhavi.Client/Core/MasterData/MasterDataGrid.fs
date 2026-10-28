module Medhavi.Client.Core.MasterData.MasterDataGrid

open System
open Medhavi.Common.Validation
open Medhavi.Contracts.Core
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ViewComposition.Context
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering
open Medhavi.Client.Components.Shared

[<CLIMutable>]
type MasterDataRow =
    { EntityType: string
      Id: string
      Name: string
      SubType: string
      State: string
      RoleOrTimeZone: string }

/// Declarative metadata descriptors for Core Master Data actions.
module MasterDataActions =

    let activateItem: ActionDescriptor =
        { Id = ActionDefId.literal "master_data.activateItem"
          DisplayName = "Activate Item"
          Icon = "check_circle"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Activate this master data item for planning?"
          RequiredRoles = [ "Planner"; "Supervisor"; "Administrator" ]
          IsPrimary = true }

    let retireItem: ActionDescriptor =
        { Id = ActionDefId.literal "master_data.retireItem"
          DisplayName = "Retire Item"
          Icon = "archive"
          Scope = SingleEntityScope
          ExecutionMode = Parameterized("retire-item-form", "Retire Master Data Item")
          RequiredRoles = [ "Planner"; "Supervisor"; "Administrator" ]
          IsPrimary = false }

    let activateLocation: ActionDescriptor =
        { Id = ActionDefId.literal "master_data.activateLocation"
          DisplayName = "Activate Location"
          Icon = "add_location_alt"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Activate this master data location for network logistics?"
          RequiredRoles = [ "Planner"; "Supervisor"; "Administrator" ]
          IsPrimary = false }

    let all: ActionDescriptor list =
        [ activateItem
          retireItem
          activateLocation ]

let activateItem = MasterDataActions.activateItem
let retireItem = MasterDataActions.retireItem
let activateLocation = MasterDataActions.activateLocation
let allActions = MasterDataActions.all

let schemaId = SchemaDefId.literal "core-master-data-grid-schema"
let storeId: DataStoreId = DataStoreId.literal "core-master-data-store"
let definitionId = ComponentDefId.literal "comp-core-master-data"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Core Master Data Catalog"
      Capabilities = [ "Tabular" ]
      SupportedActions = MasterDataActions.all |> List.map (fun a -> a.Id)
      Actions = MasterDataActions.all
      Columns =
        [ { ColumnId = "EntityType"
            PropertyName = Some "EntityType"
            Title = "Entity Type"
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
          { ColumnId = "Id"
            PropertyName = Some "Id"
            Title = "Identifier"
            DefaultOrder = 1
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
          { ColumnId = "Name"
            PropertyName = Some "Name"
            Title = "Entity Name"
            DefaultOrder = 2
            DefaultWidthInPixels = Some 220
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
          { ColumnId = "SubType"
            PropertyName = Some "SubType"
            Title = "Sub-Type / Classification"
            DefaultOrder = 3
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
          { ColumnId = "State"
            PropertyName = Some "State"
            Title = "Lifecycle State"
            DefaultOrder = 4
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
          { ColumnId = "RoleOrTimeZone"
            PropertyName = Some "RoleOrTimeZone"
            Title = "Roles / TimeZone"
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
            FormatString = None } ]
      DefaultSort =
        Some
            { Field = "EntityType"
              Direction = Ascending
              Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<MasterDataRow> =
    match ProjectionCompiler.compile<MasterDataRow> schema with
    | Valid a -> a
    | Invalid errs ->
        let msg = String.concat "; " errs
        invalidOp $"MasterDataGrid compilation failed: {msg}"

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Core Master Data Catalog"
      Icon = "inventory_2"
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
