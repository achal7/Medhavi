namespace Medhavi.Client.Components.Core.EnterprisePicture

open System
open Medhavi.Contracts.Core
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ViewComposition.Context
open Medhavi.Common.Validation
open Medhavi.Client.Components.Shared

/// Tabular representation of an Enterprise Picture Version
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

module EnterprisePictureActions =

    let publish: ActionDescriptor =
        {
            Id = ActionDefId.literal "picture.publish"
            DisplayName = "Publish Picture Version"
            Icon = "publish"
            Scope = SingleEntityScope
            ExecutionMode =
                RequiresConfirmation "Publish this picture version to make it the active authoritative enterprise plan?"
            RequiredRoles = [ "Planner"; "LeadPlanner" ]
            IsPrimary = true
        }

    let compose: ActionDescriptor =
        {
            Id = ActionDefId.literal "picture.compose"
            DisplayName = "Compose Draft Version"
            Icon = "add_circle"
            Scope = GlobalScope
            ExecutionMode =
                RequiresConfirmation "Compose a new Draft Enterprise Picture for the current planning scope?"
            RequiredRoles = [ "Planner"; "LeadPlanner" ]
            IsPrimary = false
        }

    let all: ActionDescriptor list = [ publish; compose ]

module EnterprisePictureSchema =

    let schemaId = SchemaDefId.literal "enterprise-picture-schema"

    let definition: SchemaDefinition =
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
                        FilterValueKind = Some DateTimeOffset
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

    let adapter =
        match ProjectionCompiler.compile<EnterprisePictureVersionRow> definition with
        | Valid a -> a
        | Invalid errs -> failwithf "EnterprisePictureSchema compilation failed: %A" errs
