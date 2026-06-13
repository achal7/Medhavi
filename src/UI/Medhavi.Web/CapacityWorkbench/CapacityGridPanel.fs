namespace Medhavi.Web.CapacityWorkbench

open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Medhavi.Web.Panels
open Medhavi.Web.Components
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open System

open Medhavi.Contracts.Capacity

module CapacityGridPanel =

    let render (rows: OperationView list) (dispatch: Msg -> unit) =
        let columns = [
            Rz.dataGridColumn<OperationView>("OperationId", "Operation ID")
            Rz.dataGridColumn<OperationView>("RoutingStepId", "Step ID")
            Rz.dataGridColumn<OperationView>("RunMinutes", "Run (min)")
            Rz.dataGridColumn<OperationView>("StartTime", "Start Time")
            Rz.dataGridColumn<OperationView>("EndTime", "End Time")
            Rz.dataGridColumn<OperationView>("Status", "Status")
            Rz.dataGridColumn<OperationView>("IsFirm", "Firm?")

            comp<RadzenDataGridColumn<OperationView>> {
                "Title" => "Details"
                "Width" => "100px"
                "TextAlign" => TextAlign.Center
                "Template" => RenderFragment<OperationView>(fun r ->
                    RenderFragment(fun b ->
                        b.OpenComponent<RadzenButton>(1)
                        b.AddAttribute(2, "Icon", "info")
                        b.AddAttribute(3, "ButtonStyle", ButtonStyle.Info)
                        b.AddAttribute(4, "Size", ButtonSize.Small)
                        let onClick = EventCallback.Factory.Create<MouseEventArgs>(
                            (dispatch :> obj),
                            Action<MouseEventArgs>(fun _ -> dispatch (RowSelected r))
                        )
                        b.AddAttribute(5, "Click", onClick)
                        b.CloseComponent()
                    )
                )
            }
        ]

        let config =
            { Columns = columns
              Data = rows
              IsLoading = false
              OnRowSelected = RowSelected }

        comp<GridPanel<OperationView, Msg>> {
            "Config" => config
            "Dispatch" => dispatch
        }
