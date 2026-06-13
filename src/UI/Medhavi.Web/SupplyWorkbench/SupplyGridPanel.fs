namespace Medhavi.Web.SupplyWorkbench

open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Medhavi.Web.Panels
open Medhavi.Web.Components
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open System

open Medhavi.Contracts.Supply

module SupplyGridPanel =

    let render (rows: SupplyOrder list) (dispatch: Msg -> unit) =
        let columns = [
            Rz.dataGridColumn<SupplyOrder>("Id", "Order ID")
            Rz.dataGridColumn<SupplyOrder>("OrderType", "Type")
            Rz.dataGridColumn<SupplyOrder>("SkuId", "SKU ID")
            Rz.dataGridColumn<SupplyOrder>("StockingPointId", "Stocking Point")
            Rz.dataGridColumn<SupplyOrder>("Quantity", "Planned Qty")
            Rz.dataGridColumn<SupplyOrder>("CompletedQuantity", "Confirmed Qty")
            comp<RadzenDataGridColumn<SupplyOrder>> {
                "Title" => "Planned Date"
                "Property" => "RequiredDeliveryDate"
                "Template" => RenderFragment<SupplyOrder>(fun r ->
                    RenderFragment(fun b ->
                        let dateText = 
                            match r.RequiredDeliveryDate with
                            | Some dt -> dt.ToString("yyyy-MM-dd")
                            | None -> "N/A"
                        b.AddContent(0, dateText)
                    )
                )
            }
            Rz.dataGridColumn<SupplyOrder>("IsFirm", "Firm?")
            Rz.dataGridColumn<SupplyOrder>("IsLocked", "Locked?")

            comp<RadzenDataGridColumn<SupplyOrder>> {
                "Title" => "Details"
                "Width" => "100px"
                "TextAlign" => TextAlign.Center
                "Template" => RenderFragment<SupplyOrder>(fun r ->
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

        comp<GridPanel<SupplyOrder, Msg>> {
            "Config" => config
            "Dispatch" => dispatch
        }
