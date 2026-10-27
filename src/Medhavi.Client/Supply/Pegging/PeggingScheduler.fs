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
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.Supply.Pegging.PeggingSchedulerSchema

let definitionId = ComponentDefId.literal "comp-pegging-scheduler"
let storeId = PeggingSchedulerStore.storeId
let schemaId = PeggingSchedulerSchema.schemaId

let definition: ComponentDefinition =
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

    let appointments = appointmentsList :> IEnumerable<PeggingAppointment>

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
                        dispatch (SelectionChanged (Some args.Data.Id)))

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
    { Definition = definition
      Runtime = runtime }
