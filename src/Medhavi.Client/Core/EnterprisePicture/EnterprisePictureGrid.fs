module Medhavi.Client.Core.EnterprisePicture.EnterprisePictureGrid

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

/// Tabular row representation of an Enterprise Picture Version.
type EnterprisePictureVersionRow =
    {
        VersionKey: string
        PlanningScopeId: string
        VersionNumber: int
        LifecycleState: string
        DemandCount: int
        SupplyCount: int
        InventoryCount: int
        PublicationTime: DateTimeOffset option
        IsCurrentPublished: bool
    }

/// Declarative metadata descriptors for Enterprise Picture actions.
module EnterprisePictureActions =

    let refreshPicture: ActionDescriptor =
        {
            Id = ActionDefId.literal "picture.refreshPicture"
            DisplayName = "Refresh Picture (Compose Draft)"
            Icon = "refresh"
            Scope = GlobalScope
            ExecutionMode =
                RequiresConfirmation "Compose and refresh the Enterprise Picture draft for the current planning scope?"
            RequiredRoles = [ "Planner"; "LeadPlanner" ]
            IsPrimary = false
        }

    let freezeBaseline: ActionDescriptor =
        {
            Id = ActionDefId.literal "picture.freezeBaseline"
            DisplayName = "Freeze Baseline (Publish Version)"
            Icon = "lock"
            Scope = SingleEntityScope
            ExecutionMode =
                RequiresConfirmation "Publish this picture version as the authoritative frozen enterprise baseline?"
            RequiredRoles = [ "Planner"; "LeadPlanner" ]
            IsPrimary = true
        }

    let publish: ActionDescriptor =
        {
            Id = ActionDefId.literal "picture.publish"
            DisplayName = "Publish Picture Version"
            Icon = "publish"
            Scope = SingleEntityScope
            ExecutionMode =
                RequiresConfirmation "Publish this picture version to make it the active authoritative enterprise plan?"
            RequiredRoles = [ "Planner"; "LeadPlanner" ]
            IsPrimary = false
        }

    let compose: ActionDescriptor =
        {
            Id = ActionDefId.literal "picture.compose"
            DisplayName = "Compose Draft Version"
            Icon = "add_circle"
            Scope = GlobalScope
            ExecutionMode = RequiresConfirmation "Compose a new Draft Enterprise Picture for the current planning scope?"
            RequiredRoles = [ "Planner"; "LeadPlanner" ]
            IsPrimary = false
        }

    let all: ActionDescriptor list = [ refreshPicture; freezeBaseline; publish; compose ]

let refreshPicture = EnterprisePictureActions.refreshPicture
let freezeBaseline = EnterprisePictureActions.freezeBaseline
let publish = EnterprisePictureActions.publish
let compose = EnterprisePictureActions.compose
let allActions = EnterprisePictureActions.all

let schemaId = SchemaDefId.literal "core-enterprise-picture-grid-schema"
let storeId: DataStoreId = DataStoreId.literal "core-enterprise-picture-store"
let definitionId = ComponentDefId.literal "enterprise-picture-grid"

let schema: SchemaDefinition =
    {
        Id = schemaId
        DisplayName = "Enterprise Picture Versions"
        Capabilities = [ "Tabular" ]
        SupportedActions = EnterprisePictureActions.all |> List.map(fun a -> a.Id)
        Actions = EnterprisePictureActions.all
        Columns =
            [
                {
                    ColumnId = "LifecycleState"
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
                    FormatString = None
                }
                {
                    ColumnId = "VersionNumber"
                    PropertyName = Some "VersionNumber"
                    Title = "Version #"
                    DefaultOrder = 1
                    DefaultWidthInPixels = Some 100
                    DefaultVisible = true
                    DefaultPinned = true
                    Sortable = true
                    Filterable = true
                    Groupable = false
                    FilterValueKind = Some Number
                    UserConfigurable = true
                    RenderHint = None
                    Alignment = Right
                    CellType = NumberType
                    IsKey = false
                    FormatString = Some "#0"
                }
                {
                    ColumnId = "PlanningScopeId"
                    PropertyName = Some "PlanningScopeId"
                    Title = "Planning Scope"
                    DefaultOrder = 2
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
                    ColumnId = "DemandCount"
                    PropertyName = Some "DemandCount"
                    Title = "Demands Pegged"
                    DefaultOrder = 3
                    DefaultWidthInPixels = Some 130
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
                    FormatString = Some "#,##0"
                }
                {
                    ColumnId = "SupplyCount"
                    PropertyName = Some "SupplyCount"
                    Title = "Supplies Pegged"
                    DefaultOrder = 4
                    DefaultWidthInPixels = Some 130
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
                    FormatString = Some "#,##0"
                }
                {
                    ColumnId = "InventoryCount"
                    PropertyName = Some "InventoryCount"
                    Title = "Inventory Items"
                    DefaultOrder = 5
                    DefaultWidthInPixels = Some 130
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
                    FormatString = Some "#,##0"
                }
                {
                    ColumnId = "PublicationTime"
                    PropertyName = Some "PublicationTime"
                    Title = "Published At"
                    DefaultOrder = 6
                    DefaultWidthInPixels = Some 150
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
                    ColumnId = "IsCurrentPublished"
                    PropertyName = Some "IsCurrentPublished"
                    Title = "Active Truth"
                    DefaultOrder = 7
                    DefaultWidthInPixels = Some 110
                    DefaultVisible = true
                    DefaultPinned = false
                    Sortable = true
                    Filterable = true
                    Groupable = true
                    FilterValueKind = Some Boolean
                    UserConfigurable = true
                    RenderHint = Some "StatusBadge"
                    Alignment = Center
                    CellType = BooleanType
                    IsKey = false
                    FormatString = None
                }
                {
                    ColumnId = "VersionKey"
                    PropertyName = Some "VersionKey"
                    Title = "Key"
                    DefaultOrder = 8
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
                    FormatString = None
                }
            ]
        DefaultSort =
            Some
                {
                    Field = "VersionNumber"
                    Direction = Descending
                    Priority = 1
                }
        DefaultFilters = []
    }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<EnterprisePictureVersionRow> =
    match ProjectionCompiler.compile<EnterprisePictureVersionRow> schema with
    | Valid a -> a
    | Invalid errs ->
        let msg = String.concat "; " errs
        invalidOp $"EnterprisePictureGrid compilation failed: {msg}"

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Enterprise Picture Workbench"
      Icon = "photo_library"
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
