namespace Medhavi.Client.Components.Supply

open System
open System.Collections.Generic
open Microsoft.AspNetCore.Components
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Medhavi.Client
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Data
open Medhavi.Contracts.Core.Demand
open Medhavi.Contracts.Supply

type PeggingItemType =
    | Demand
    | PlannedSupply

type PeggingAppointment =
    {
        Id: string
        Text: string
        Start: DateTime
        End: DateTime
        ItemType: PeggingItemType
        Quantity: decimal
        EntityRef: string
        Status: string
    }

module PeggingScheduler =

    let definitionId = ComponentDefId.literal "comp-pegging-scheduler"
    let storeId = DataStoreId.literal "pegging-scheduler-store"

    let definition: ComponentDefinition =
        {
            Id = definitionId
            DisplayName = "Pegging Horizon Timeline & Year Planner"
            Icon = "calendar_month"
            Category = ComponentCategory.Visualization
            RequiredSchemaCapabilities = [ "TimeSeries" ]
            DefaultGridSpan = GridSpan.literal 12 5
            DefaultSchemaId = Some PeggingSchedulerSchema.schemaId
            DefaultStoreId = Some storeId
            IsResizable = true
            IsDraggable = true
            SelectionMode = SelectionMode.SingleSelection
        }

    let defaultAppointments: PeggingAppointment list =
        [
            {
                Id = "DEM-ORD-1001"
                Text = "Demand: DEM-ORD-1001 (500GB Drive · 250 EA)"
                Start = DateTime(2026, 9, 18, 8, 0, 0)
                End = DateTime(2026, 9, 18, 17, 0, 0)
                ItemType = Demand
                Quantity = 250m
                EntityRef = "SKU-DRIVE-500GB"
                Status = "Committed"
            }

            {
                Id = "PO-ACME-4011"
                Text = "Supply: PO-ACME-4011 (Acme Components · 250 EA)"
                Start = DateTime(2026, 9, 17, 8, 0, 0)
                End = DateTime(2026, 9, 17, 17, 0, 0)
                ItemType = PlannedSupply
                Quantity = 250m
                EntityRef = "SKU-DRIVE-500GB"
                Status = "Confirmed"
            }

            {
                Id = "DEM-ORD-1002"
                Text = "Demand: DEM-ORD-1002 (1TB Drive · 500 EA)"
                Start = DateTime(2026, 9, 25, 8, 0, 0)
                End = DateTime(2026, 9, 25, 17, 0, 0)
                ItemType = Demand
                Quantity = 500m
                EntityRef = "SKU-DRIVE-1TB"
                Status = "Planned"
            }

            {
                Id = "PO-GLOBAL-9012"
                Text = "Supply: PO-GLOBAL-9012 (Global Chips · 500 EA)"
                Start = DateTime(2026, 9, 23, 8, 0, 0)
                End = DateTime(2026, 9, 23, 17, 0, 0)
                ItemType = PlannedSupply
                Quantity = 500m
                EntityRef = "SKU-DRIVE-1TB"
                Status = "InProcurement"
            }

            {
                Id = "DEM-PROD-3001"
                Text = "Demand: DEM-PROD-3001 (850W Power Supply · 75 EA)"
                Start = DateTime(2026, 9, 20, 8, 0, 0)
                End = DateTime(2026, 9, 20, 17, 0, 0)
                ItemType = Demand
                Quantity = 75m
                EntityRef = "SKU-POWER-SUPPLY-850W"
                Status = "Confirmed"
            }

            {
                Id = "TRF-ORD-7712"
                Text = "Supply: TRF-ORD-7712 (Transfer West->East · 75 EA)"
                Start = DateTime(2026, 9, 19, 8, 0, 0)
                End = DateTime(2026, 9, 19, 17, 0, 0)
                ItemType = PlannedSupply
                Quantity = 75m
                EntityRef = "SKU-POWER-SUPPLY-850W"
                Status = "InTransit"
            }

            {
                Id = "DEM-FCST-2001"
                Text = "Demand: DEM-FCST-2001 (Battery Pack Pro · 1200 EA)"
                Start = DateTime(2026, 10, 15, 8, 0, 0)
                End = DateTime(2026, 10, 15, 17, 0, 0)
                ItemType = Demand
                Quantity = 1200m
                EntityRef = "SKU-BATTERY-PACK-PRO"
                Status = "Forecast"
            }

            {
                Id = "WO-2026-W37-01"
                Text = "Supply: WO-2026-W37-01 (Work Order Assembly · 1200 EA)"
                Start = DateTime(2026, 10, 10, 8, 0, 0)
                End = DateTime(2026, 10, 10, 17, 0, 0)
                ItemType = PlannedSupply
                Quantity = 1200m
                EntityRef = "SKU-BATTERY-PACK-PRO"
                Status = "Scheduled"
            }
        ]

    let toUniversal (appt: PeggingAppointment) : UniversalRow =
        { Key = appt.Id
          Cells =
            Map.ofList
                [ "Id", TextValue appt.Id
                  "Text", TextValue appt.Text
                  "Start", DateTimeValue (DateTimeOffset appt.Start)
                  "End", DateTimeValue (DateTimeOffset appt.End)
                  "ItemType", TextValue (match appt.ItemType with Demand -> "Demand" | PlannedSupply -> "PlannedSupply")
                  "Quantity", NumberValue appt.Quantity
                  "EntityRef", TextValue appt.EntityRef
                  "Status", TextValue appt.Status ] }

    let store: DataStore =
        {
            Id = storeId
            Fetch =
                fun (ctx: ApplicationContext) ->
                    async {
                        let! demandsOpt =
                            async {
                                match ctx.Resolve<DemandQueries>() with
                                | Some queries ->
                                    let! dtos = queries.GetAll() |> Async.AwaitTask
                                    return Some dtos
                                | None ->
                                    match ctx.DomainServices with
                                    | Some ds ->
                                        let! dtos = ds.Core.DemandManagement.Queries.GetAll() |> Async.AwaitTask
                                        return Some dtos
                                    | None -> return None
                            }

                        let! suppliesOpt =
                            async {
                                match ctx.Resolve<SupplyPlanQueries>() with
                                | Some queries ->
                                    let! plans = queries.GetAll() |> Async.AwaitTask
                                    let orders =
                                        plans
                                        |> List.choose (fun p -> p.CurrentVersion)
                                        |> List.collect (fun v -> v.PlannedOrders)
                                    return Some orders
                                | None ->
                                    match ctx.DomainServices with
                                    | Some ds ->
                                        let! plans = ds.Supply.PlanSupply.SupplyPlanQueries.GetAll() |> Async.AwaitTask
                                        let orders =
                                            plans
                                            |> List.choose (fun p -> p.CurrentVersion)
                                            |> List.collect (fun v -> v.PlannedOrders)
                                        return Some orders
                                    | None -> return None
                            }

                        let demandAppointments =
                            match demandsOpt with
                            | Some dtos when not (List.isEmpty dtos) ->
                                dtos
                                |> List.map (fun d ->
                                    {
                                        Id = d.Id
                                        Text = $"Demand: {d.Id} ({d.Item} · {d.Quantity} EA)"
                                        Start = d.NeedWindowLatest.LocalDateTime
                                        End = d.NeedWindowLatest.LocalDateTime.AddHours(8.0)
                                        ItemType = PeggingItemType.Demand
                                        Quantity = d.Quantity
                                        EntityRef = d.Item
                                        Status = d.LifecycleState
                                    })
                            | _ -> []

                        let supplyAppointments =
                            match suppliesOpt with
                            | Some orders when not (List.isEmpty orders) ->
                                orders
                                |> List.map (fun o ->
                                    {
                                        Id = o.OrderId
                                        Text = $"Supply: {o.OrderId} ({o.Item} · {o.Quantity} {o.UnitOfMeasure})"
                                        Start = o.ReleaseDate.LocalDateTime
                                        End = o.DueDate.LocalDateTime
                                        ItemType = PeggingItemType.PlannedSupply
                                        Quantity = o.Quantity
                                        EntityRef = o.Item
                                        Status = "Planned"
                                    })
                            | _ -> []

                        let appointments =
                            if List.isEmpty demandAppointments && List.isEmpty supplyAppointments then
                                defaultAppointments
                            else
                                demandAppointments @ supplyAppointments

                        let rows = appointments |> List.map toUniversal
                        return Tabular rows
                    }
        }

    let onAppointmentRender (args: SchedulerAppointmentRenderEventArgs<PeggingAppointment>) =
        match args.Data.ItemType with
        | Demand ->
            args.Attributes.["style"] <-
                "background-color: var(--rz-primary); border-left: 4px solid var(--rz-primary-dark); color: var(--rz-on-primary); border-radius: 4px; font-size: 11px; font-weight: 600; padding: 2px 4px;"
        | PlannedSupply ->
            args.Attributes.["style"] <-
                "background-color: var(--rz-success); border-left: 4px solid var(--rz-success-dark); color: var(--rz-on-success); border-radius: 4px; font-size: 11px; font-weight: 600; padding: 2px 4px;"

    let render (context: ComponentRenderContext) (dispatch: ComponentEvent -> unit) : Node =
        let liveAppointments =
            match context.Data with
            | Tabular rows when not (List.isEmpty rows) ->
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
                        Some {
                            Id = id
                            Text = text
                            Start = startDto.LocalDateTime
                            End = endDto.LocalDateTime
                            ItemType = itemType
                            Quantity = qty
                            EntityRef = entityRef
                            Status = status
                        }
                    | _ -> None)
            | _ -> defaultAppointments

        let appointments =
            (if List.isEmpty liveAppointments then defaultAppointments else liveAppointments)
            :> IEnumerable<PeggingAppointment>

        div {
            attr.style
                "display: flex; flex-direction: column; height: 100%; width: 100%; box-sizing: border-box; background: var(--rz-base-background-color); border: 1px solid var(--rz-border-color); border-radius: 8px; overflow: hidden;"

            // Header Legend Banner
            div {
                attr.style
                    "display: flex; justify-content: space-between; align-items: center; padding: 8px 14px; background: rgba(0,0,0,0.02); border-bottom: 1px solid var(--rz-border-color);"

                div {
                    attr.style "display: flex; align-items: center; gap: 8px;"

                    span {
                        attr.``class`` "rzi"
                        attr.style "font-size: 18px; color: var(--rz-primary);"
                        "calendar_month"
                    }

                    span {
                        attr.style "font-size: 13px; font-weight: 700; color: var(--rz-text-color);"
                        "Demand-Supply Pegging Horizon"
                    }
                }

                // Legend
                div {
                    attr.style "display: flex; align-items: center; gap: 16px; font-size: 11px; font-weight: 600;"

                    div {
                        attr.style "display: flex; align-items: center; gap: 6px;"

                        span {
                            attr.style
                                "display: inline-block; width: 12px; height: 12px; background: var(--rz-primary); border-left: 3px solid var(--rz-primary-dark); border-radius: 2px;"
                        }

                        span {
                            attr.style "color: var(--rz-text-color);"
                            "Demand Commitments (Blue)"
                        }
                    }

                    div {
                        attr.style "display: flex; align-items: center; gap: 6px;"

                        span {
                            attr.style
                                "display: inline-block; width: 12px; height: 12px; background: var(--rz-success); border-left: 3px solid var(--rz-success-dark); border-radius: 2px;"
                        }

                        span {
                            attr.style "color: var(--rz-text-color);"
                            "Planned Supplies (Green)"
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
                    "Date" => DateTime(2026, 9, 1)
                    "SelectedIndex" => 0
                    "Style" => "height: 100%; width: 100%;"

                    "AppointmentRender" => Action<SchedulerAppointmentRenderEventArgs<PeggingAppointment>>(onAppointmentRender)

                    attr.callback
                        "AppointmentSelect"
                        (fun (args: SchedulerAppointmentSelectEventArgs<PeggingAppointment>) ->
                            dispatch(SelectionChanged(Some args.Data.Id)))

                    attr.fragment
                        "ChildContent"
                        (concat {
                            comp<RadzenYearPlannerView> {
                                "Text" => "Year Planner"
                                "StartMonth" => Month.September
                            }

                            comp<RadzenYearTimelineView> { "Text" => "Timeline" }
                            comp<RadzenMonthView> { "Text" => "Month" }
                        })
                }
            }
        }

    let runtime: ComponentRuntime = { Render = render }

    let registration: ComponentRegistration =
        {
            Definition = definition
            Runtime = runtime
        }
