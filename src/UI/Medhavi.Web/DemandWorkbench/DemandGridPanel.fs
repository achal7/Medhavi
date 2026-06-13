namespace Medhavi.Web.DemandWorkbench

open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Medhavi.Web.Panels
open Medhavi.Web.Components
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open System

open Medhavi.Contracts.Demand

module DemandGridPanel =

    let render (rows: DemandLine list) (dispatch: Msg -> unit) =
        let columns = [
            Rz.dataGridColumn<DemandLine>("DemandOrderId", "Order ID")
            Rz.dataGridColumn<DemandLine>("SkuId", "SKU ID")
            Rz.dataGridColumn<DemandLine>("StockingPointId", "Stocking Point")
            Rz.dataGridColumn<DemandLine>("RequestedQty", "Quantity")
            Rz.dataGridColumn<DemandLine>("UnitOfMeasure", "UOM")
            Rz.dataGridColumn<DemandLine>("RequestedDeliveryDate", "Requested Date")
            Rz.dataGridColumn<DemandLine>("Priority", "Priority")
            Rz.dataGridColumn<DemandLine>("Status", "Status")
            
            comp<RadzenDataGridColumn<DemandLine>> {
                "Title" => "Details"
                "Width" => "100px"
                "TextAlign" => TextAlign.Center
                "Template" => RenderFragment<DemandLine>(fun r ->
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

        comp<GridPanel<DemandLine, Msg>> {
            "Config" => config
            "Dispatch" => dispatch
        }
