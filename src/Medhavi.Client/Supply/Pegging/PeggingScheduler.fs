module Medhavi.Client.Supply.Pegging.PeggingScheduler

open System
open System.Collections.Generic
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Medhavi.Client
open Medhavi.Client.Registries
open Medhavi.Client.Rendering
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
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

/// Declarative metadata descriptors for Pegging Scheduler actions.
module PeggingSchedulerActions =

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

    let all: ActionDescriptor list = [ realignPegging ]

let realignPegging = PeggingSchedulerActions.realignPegging
let allActions = PeggingSchedulerActions.all

let storeId = DataStoreId.literal "pegging-scheduler-store"
let schemaId = SchemaDefId.literal "schema-pegging-scheduler"
let definitionId = ComponentDefId.literal "comp-pegging-scheduler"

let schema: SchemaDefinition =
    {
        Id = schemaId
        DisplayName = "Demand-Supply Pegging Horizon"
        Capabilities = [ "TimeSeries"; "Tabular" ]
        SupportedActions = PeggingSchedulerActions.all |> List.map(fun a -> a.Id)
        Actions = PeggingSchedulerActions.all
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

let definition: SchemaDefinition = schema

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

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Demand-Supply Pegging Horizon & Timeline"
      Icon = "calendar_month"
      Category = ComponentCategory.Visualization
      RequiredSchemaCapabilities = [ "TimeSeries" ]
      DefaultGridSpan = GridSpan.literal 12 5
      DefaultSchemaId = Some schemaId
      DefaultStoreId = Some storeId
      IsResizable = true
      IsDraggable = true
      SelectionMode = SelectionMode.SingleSelection }

let onAppointmentRender (args: SchedulerAppointmentRenderEventArgs<PeggingAppointment>) =
    match args.Data.ItemType with
    | Demand ->
        args.Attributes.["style"] <-
            "background-color: var(--rz-primary); border-left: 4px solid var(--rz-primary-dark); color: var(--rz-on-primary); border-radius: 4px; font-size: 11px; font-weight: 600; padding: 2px 4px;"
    | PlannedSupply ->
        args.Attributes.["style"] <-
            "background-color: var(--rz-success); border-left: 4px solid var(--rz-success-dark); color: var(--rz-on-success); border-radius: 4px; font-size: 11px; font-weight: 600; padding: 2px 4px;"

let render (context: ComponentRenderContext) (dispatch: ComponentEvent -> unit) : Node =
    let appointmentsList =
        match context.Data with
        | Tabular rows ->
            rows
            |> List.choose (fun row ->
                match Map.tryFind "Id" row.Cells, Map.tryFind "Start" row.Cells, Map.tryFind "End" row.Cells with
                | Some (TextValue id), Some (DateTimeValue startDto), Some (DateTimeValue endDto) ->
                    let text = match Map.tryFind "Text" row.Cells with Some (TextValue t) -> t | _ -> id
                    let itemType =
                        match Map.tryFind "ItemType" row.Cells with
                        | Some (TextValue "Demand") -> Demand
                        | _ -> PlannedSupply
                    let qty = match Map.tryFind "Quantity" row.Cells with Some (NumberValue q) -> q | _ -> 0m
                    let entityRef = match Map.tryFind "EntityRef" row.Cells with Some (TextValue r) -> r | _ -> ""
                    let status = match Map.tryFind "Status" row.Cells with Some (TextValue s) -> s | _ -> ""
                    Some
                        { Id = id
                          Text = text
                          Start = startDto
                          End = endDto
                          ItemType = itemType
                          Quantity = qty
                          EntityRef = entityRef
                          Status = status }
                | _ -> None)
        | _ -> []

    let appointments: IEnumerable<PeggingAppointment> = appointmentsList :> IEnumerable<PeggingAppointment>

    let startDate =
        appointmentsList
        |> List.tryHead
        |> Option.map (fun a -> a.Start.DateTime)
        |> Option.defaultValue DateTime.Today

    let startMonth =
        appointmentsList
        |> List.tryHead
        |> Option.map (fun a -> enum<Month> a.Start.Month)
        |> Option.defaultValue Month.January

    div {
        attr.style "display: flex; flex-direction: column; height: 100%; width: 100%; overflow: hidden; background: var(--rz-base-background-color);"

        // Workbench Control Bar
        div {
            attr.style "display: flex; align-items: center; justify-content: space-between; padding: 8px 12px; border-bottom: 1px solid var(--rz-border-color); background: var(--rz-base-200);"

            div {
                attr.style "display: flex; align-items: center; gap: 8px;"

                span {
                    attr.style "font-weight: 600; font-size: 14px;"
                    "Pegging Horizon & Replenishment Timeline"
                }

                span {
                    attr.style "font-size: 12px; color: var(--rz-text-secondary-color);"
                    $"(Showing {appointmentsList.Length} demand/supply events)"
                }
            }

            div {
                attr.style "display: flex; align-items: center; gap: 8px;"

                // Legend
                div {
                    attr.style "display: flex; align-items: center; gap: 6px; font-size: 11px; margin-right: 8px;"

                    span {
                        attr.style "display: inline-block; width: 12px; height: 12px; background: var(--rz-primary); border-radius: 2px;"
                    }
                    span { "Demand Orders" }

                    span {
                        attr.style "display: inline-block; width: 12px; height: 12px; background: var(--rz-success); border-radius: 2px; margin-left: 6px;"
                    }
                    span { "Planned Supply" }
                }

                button {
                    attr.``class`` "rz-button rz-button-sm rz-secondary rz-shade-default"
                    attr.style "display: flex; align-items: center; gap: 4px;"
                    on.click (fun _ -> dispatch (ActionRequested(PeggingSchedulerActions.realignPegging.Id, None, NoPayload)))

                    span {
                        attr.``class`` "rzi"
                        "sync"
                    }
                    span { "Realign Pegging" }
                }

                button {
                    attr.``class`` "rz-button rz-button-sm rz-base-100 rz-shade-default"
                    attr.style "display: flex; align-items: center;"
                    on.click (fun _ -> dispatch RefreshRequested)

                    span {
                        attr.``class`` "rzi"
                        "refresh"
                    }
                }
            }
        }

        // Radzen Scheduler View
        div {
            attr.style "flex: 1; min-height: 0; width: 100%; overflow: hidden;"

            comp<RadzenScheduler<PeggingAppointment>> {
                "Data" => appointments
                "StartProperty" => "Start"
                "EndProperty" => "End"
                "TextProperty" => "Text"
                "Date" => startDate
                "SelectedIndex" => 0
                "Style" => "height: 100%; width: 100%;"

                "AppointmentRender" => Action<SchedulerAppointmentRenderEventArgs<PeggingAppointment>>(onAppointmentRender)

                attr.callback
                    "AppointmentSelect"
                    (fun (args: SchedulerAppointmentSelectEventArgs<PeggingAppointment>) ->
                        dispatch (SelectionChanged (Some args.Data.Id)))

                attr.fragment
                    "ChildContent"
                    (concat {
                        comp<RadzenYearPlannerView> {
                            "Text" => "Year Planner"
                            "StartMonth" => startMonth
                        }

                        comp<RadzenYearTimelineView> { "Text" => "Timeline" }
                        comp<RadzenMonthView> { "Text" => "Month" }
                    })
            }
        }
    }

let runtime: ComponentRuntime = { Render = render }

let registration: ComponentRegistration =
    { Definition = componentDefinition
      Schema = schema
      Runtime = runtime }
