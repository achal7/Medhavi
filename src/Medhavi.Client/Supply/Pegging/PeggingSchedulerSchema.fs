module Medhavi.Client.Supply.Pegging.PeggingSchedulerSchema

open System
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ViewComposition.Context

type PeggingItemType =
    | Demand
    | PlannedSupply

type PeggingAppointment =
    {
        Id: string
        Text: string
        Start: DateTimeOffset
        End: DateTimeOffset
        ItemType: PeggingItemType
        Quantity: decimal
        EntityRef: string
        Status: string
    }

let realignPegging: ActionDescriptor =
    {
        Id = ActionDefId.literal "pegging.realign"
        DisplayName = "Realign Pegging Horizon"
        Icon = "sync"
        Scope = GlobalScope
        ExecutionMode = Direct
        RequiredRoles = [ "Planner" ]
        IsPrimary = true
    }

let allActions: ActionDescriptor list = [ realignPegging ]

let schemaId = SchemaDefId.literal "schema-pegging-scheduler"

let definition: SchemaDefinition =
    {
        Id = schemaId
        DisplayName = "Demand-Supply Pegging Horizon"
        Capabilities = [ "TimeSeries"; "Tabular" ]
        SupportedActions = allActions |> List.map(fun a -> a.Id)
        Actions = allActions
        Columns =
            [
                {
                    ColumnId = "Id"
                    PropertyName = Some "Id"
                    Title = "Pegging ID"
                    DefaultOrder = 0
                    DefaultWidthInPixels = Some 140
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
                    UserConfigurable = false
                }
                {
                    ColumnId = "Text"
                    PropertyName = Some "Text"
                    Title = "Description"
                    DefaultOrder = 1
                    DefaultWidthInPixels = Some 260
                    DefaultVisible = true
                    DefaultPinned = false
                    Sortable = true
                    FilterValueKind = Some Text
                    Filterable = true
                    Groupable = false
                    RenderHint = None
                    Alignment = Left
                    CellType = TextType
                    IsKey = false
                    FormatString = None
                    UserConfigurable = true
                }
                {
                    ColumnId = "Start"
                    PropertyName = Some "Start"
                    Title = "Start Date"
                    DefaultOrder = 2
                    DefaultWidthInPixels = Some 130
                    DefaultVisible = true
                    DefaultPinned = false
                    Sortable = true
                    FilterValueKind = Some FilterValueKind.DateTimeOffset
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
                    ColumnId = "End"
                    PropertyName = Some "End"
                    Title = "End Date"
                    DefaultOrder = 3
                    DefaultWidthInPixels = Some 130
                    DefaultVisible = true
                    DefaultPinned = false
                    Sortable = true
                    FilterValueKind = Some FilterValueKind.DateTimeOffset
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
                    ColumnId = "ItemType"
                    PropertyName = Some "ItemType"
                    Title = "Type"
                    DefaultOrder = 4
                    DefaultWidthInPixels = Some 120
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
                    ColumnId = "EntityRef"
                    PropertyName = Some "EntityRef"
                    Title = "Entity Reference"
                    DefaultOrder = 6
                    DefaultWidthInPixels = Some 140
                    DefaultVisible = true
                    DefaultPinned = false
                    Sortable = true
                    FilterValueKind = Some Text
                    Filterable = true
                    Groupable = false
                    RenderHint = None
                    Alignment = Left
                    CellType = TextType
                    IsKey = false
                    FormatString = None
                    UserConfigurable = true
                }
                {
                    ColumnId = "Status"
                    PropertyName = Some "Status"
                    Title = "Status"
                    DefaultOrder = 7
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
            ]
        DefaultSort = None
        DefaultFilters = []
    }

let createRow
    (id: string)
    (text: string)
    (startDt: DateTimeOffset)
    (endDt: DateTimeOffset)
    (itemType: string)
    (quantity: decimal)
    (entityRef: string)
    (status: string)
    : UniversalRow =
    {
        Key = id
        Cells =
            Map.ofList
                [
                    "Id", TextValue id
                    "Text", TextValue text
                    "Start", DateTimeValue startDt
                    "End", DateTimeValue endDt
                    "ItemType", TextValue itemType
                    "Quantity", NumberValue quantity
                    "EntityRef", TextValue entityRef
                    "Status", TextValue status
                ]
    }

let adapter: ProjectionAdapter<PeggingAppointment> =
    {
        ToUniversal =
            fun (appt: PeggingAppointment) ->
                let itemTypeStr =
                    match appt.ItemType with
                    | Demand -> "Demand"
                    | PlannedSupply -> "PlannedSupply"

                createRow appt.Id appt.Text appt.Start appt.End itemTypeStr appt.Quantity appt.EntityRef appt.Status
    }
