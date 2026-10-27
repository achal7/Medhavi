namespace Medhavi.Client.Components.Supply

open System
open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ViewComposition.Context
open Medhavi.Common.Validation
open Medhavi.Client.Components.Shared

module SupplyUnderstandingActions =

    let acknowledge: ActionDescriptor =
        {
            Id = ActionDefId.literal "sensory.acknowledge"
            DisplayName = "Acknowledge Signal"
            Icon = "done"
            Scope = SingleEntityScope
            ExecutionMode = Direct
            RequiredRoles = [ "Planner"; "DataSteward" ]
            IsPrimary = true
        }

    let evaluate: ActionDescriptor =
        {
            Id = ActionDefId.literal "sensory.evaluate"
            DisplayName = "Evaluate Materiality"
            Icon = "analytics"
            Scope = SingleEntityScope
            ExecutionMode = Parameterized("sensory-eval-form", "Evaluate Supply Materiality")
            RequiredRoles = [ "Planner"; "LeadPlanner" ]
            IsPrimary = false
        }

    let all: ActionDescriptor list = [ acknowledge; evaluate ]

module SupplyUnderstandingSchema =

    let schemaId = SchemaDefId.literal "supply-understanding-schema"

    let definition: SchemaDefinition =
        {
            Id = schemaId
            DisplayName = "Supply Sensory Stream"
            Capabilities = [ "Tabular" ]
            SupportedActions = SupplyUnderstandingActions.all |> List.map(fun a -> a.Id)
            Actions = SupplyUnderstandingActions.all
            Columns =
                [
                    {
                        ColumnId = "SupplyType"
                        PropertyName = Some "SupplyType"
                        Title = "Signal Type"
                        DefaultOrder = 0
                        DefaultWidthInPixels = Some 120
                        DefaultVisible = true
                        DefaultPinned = true
                        Sortable = true
                        FilterValueKind = Some Text
                        Filterable = true
                        Groupable = true
                        RenderHint = Some "StatusBadge"
                        Alignment = Left
                        CellType = TextType
                        IsKey = false
                        FormatString = None
                        UserConfigurable = true
                    }
                    {
                        ColumnId = "RecordId"
                        PropertyName = Some "RecordId"
                        Title = "Record ID"
                        DefaultOrder = 1
                        DefaultWidthInPixels = Some 130
                        DefaultVisible = true
                        DefaultPinned = true
                        Sortable = true
                        FilterValueKind = Some Text
                        Filterable = true
                        Groupable = false
                        RenderHint = None
                        Alignment = Left
                        CellType = TextType
                        IsKey = true
                        FormatString = None
                        UserConfigurable = true
                    }
                    {
                        ColumnId = "LifecycleState"
                        PropertyName = Some "LifecycleState"
                        Title = "State"
                        DefaultOrder = 2
                        DefaultWidthInPixels = Some 110
                        DefaultVisible = true
                        DefaultPinned = false
                        Sortable = true
                        FilterValueKind = Some Text
                        Filterable = true
                        Groupable = true
                        RenderHint = Some "StatusBadge"
                        Alignment = Left
                        CellType = TextType
                        IsKey = false
                        FormatString = None
                        UserConfigurable = true
                    }
                    {
                        ColumnId = "Item"
                        PropertyName = Some "Item"
                        Title = "Item / Material"
                        DefaultOrder = 3
                        DefaultWidthInPixels = Some 140
                        DefaultVisible = true
                        DefaultPinned = false
                        Sortable = true
                        FilterValueKind = Some Text
                        Filterable = true
                        Groupable = true
                        RenderHint = None
                        Alignment = Left
                        CellType = TextType
                        IsKey = false
                        FormatString = None
                        UserConfigurable = true
                    }
                    {
                        ColumnId = "Location"
                        PropertyName = Some "Location"
                        Title = "Location"
                        DefaultOrder = 4
                        DefaultWidthInPixels = Some 120
                        DefaultVisible = true
                        DefaultPinned = false
                        Sortable = true
                        FilterValueKind = Some Text
                        Filterable = true
                        Groupable = true
                        RenderHint = None
                        Alignment = Left
                        CellType = TextType
                        IsKey = false
                        FormatString = None
                        UserConfigurable = true
                    }
                    {
                        ColumnId = "Quantity"
                        PropertyName = Some "Quantity"
                        Title = "Quantity"
                        DefaultOrder = 5
                        DefaultWidthInPixels = Some 110
                        DefaultVisible = true
                        DefaultPinned = false
                        Sortable = true
                        FilterValueKind = Some Number
                        Filterable = true
                        Groupable = false
                        RenderHint = None
                        Alignment = Right
                        CellType = NumberType
                        IsKey = false
                        FormatString = Some "N0"
                        UserConfigurable = true
                    }
                    {
                        ColumnId = "UnitOfMeasure"
                        PropertyName = Some "UnitOfMeasure"
                        Title = "UoM"
                        DefaultOrder = 6
                        DefaultWidthInPixels = Some 80
                        DefaultVisible = true
                        DefaultPinned = false
                        Sortable = true
                        FilterValueKind = Some Text
                        Filterable = true
                        Groupable = true
                        RenderHint = None
                        Alignment = Left
                        CellType = TextType
                        IsKey = false
                        FormatString = None
                        UserConfigurable = true
                    }
                    {
                        ColumnId = "ExpectedDeliveryDate"
                        PropertyName = Some "ExpectedDeliveryDate"
                        Title = "Expected Date"
                        DefaultOrder = 7
                        DefaultWidthInPixels = Some 120
                        DefaultVisible = true
                        DefaultPinned = false
                        Sortable = true
                        FilterValueKind = Some DateTimeOffset
                        Filterable = true
                        Groupable = false
                        RenderHint = None
                        Alignment = Left
                        CellType = DateTimeType
                        IsKey = false
                        FormatString = Some "yyyy-MM-dd"
                        UserConfigurable = true
                    }
                    {
                        ColumnId = "ObservationTime"
                        PropertyName = Some "ObservationTime"
                        Title = "Observed At"
                        DefaultOrder = 8
                        DefaultWidthInPixels = Some 140
                        DefaultVisible = true
                        DefaultPinned = false
                        Sortable = true
                        FilterValueKind = Some DateTimeOffset
                        Filterable = true
                        Groupable = false
                        RenderHint = None
                        Alignment = Left
                        CellType = DateTimeType
                        IsKey = false
                        FormatString = Some "yyyy-MM-dd HH:mm"
                        UserConfigurable = true
                    }
                    {
                        ColumnId = "SourceSystemProvenance"
                        PropertyName = Some "SourceSystemProvenance"
                        Title = "Source System"
                        DefaultOrder = 9
                        DefaultWidthInPixels = Some 130
                        DefaultVisible = true
                        DefaultPinned = false
                        Sortable = true
                        FilterValueKind = Some Text
                        Filterable = true
                        Groupable = true
                        RenderHint = None
                        Alignment = Left
                        CellType = TextType
                        IsKey = false
                        FormatString = None
                        UserConfigurable = true
                    }
                    {
                        ColumnId = "SupplierIdentifier"
                        PropertyName = Some "SupplierIdentifier"
                        Title = "Supplier"
                        DefaultOrder = 10
                        DefaultWidthInPixels = Some 130
                        DefaultVisible = false
                        DefaultPinned = false
                        Sortable = true
                        FilterValueKind = Some Text
                        Filterable = true
                        Groupable = true
                        RenderHint = None
                        Alignment = Left
                        CellType = TextType
                        IsKey = false
                        FormatString = None
                        UserConfigurable = true
                    }
                    {
                        ColumnId = "BatchIdentifier"
                        PropertyName = Some "BatchIdentifier"
                        Title = "Batch / Lot"
                        DefaultOrder = 11
                        DefaultWidthInPixels = Some 120
                        DefaultVisible = false
                        DefaultPinned = false
                        Sortable = true
                        FilterValueKind = Some Text
                        Filterable = true
                        Groupable = true
                        RenderHint = None
                        Alignment = Left
                        CellType = TextType
                        IsKey = false
                        FormatString = None
                        UserConfigurable = true
                    }
                    {
                        ColumnId = "OrderIdentifier"
                        PropertyName = Some "OrderIdentifier"
                        Title = "External Order"
                        DefaultOrder = 12
                        DefaultWidthInPixels = Some 120
                        DefaultVisible = false
                        DefaultPinned = false
                        Sortable = true
                        FilterValueKind = Some Text
                        Filterable = true
                        Groupable = true
                        RenderHint = None
                        Alignment = Left
                        CellType = TextType
                        IsKey = false
                        FormatString = None
                        UserConfigurable = true
                    }
                    {
                        ColumnId = "BusinessTime"
                        PropertyName = Some "BusinessTime"
                        Title = "Business Time"
                        DefaultOrder = 13
                        DefaultWidthInPixels = Some 140
                        DefaultVisible = false
                        DefaultPinned = false
                        Sortable = true
                        FilterValueKind = Some DateTimeOffset
                        Filterable = true
                        Groupable = false
                        RenderHint = None
                        Alignment = Left
                        CellType = DateTimeType
                        IsKey = false
                        FormatString = Some "yyyy-MM-dd HH:mm"
                        UserConfigurable = true
                    }
                ]
            DefaultSort =
                Some
                    {
                        Field = "ObservationTime"
                        Direction = Descending
                        Priority = 1
                    }
            DefaultFilters = []
        }

    let adapter =
        match ProjectionCompiler.compile<SupplyDataRecordDto> definition with
        | Valid a -> a
        | Invalid errs -> failwithf "SupplyUnderstandingSchema compilation failed: %A" errs
