namespace Medhavi.Web.Panels

open System
open Microsoft.AspNetCore.Components
open Bolero
open Bolero.Html
open Elmish
open Medhavi.Contracts.Demand
open Medhavi.Web.Components
open Medhavi.Web.Controls

module DemandPanel =

    type Model =
        { Demands: RemoteData<DemandLine list>
          SelectedDemand: DemandLine option
          SearchText: string }

    type Msg =
        | SetDemands of RemoteData<DemandLine list>
        | SelectDemand of DemandLine option
        | SearchTextChanged of string
        | RefreshRequested

    let init () =
        { Demands = RemoteData.NotRequested
          SelectedDemand = None
          SearchText = "" }

    let update msg model =
        match msg with
        | SetDemands demands -> { model with Demands = demands }, Cmd.none
        | SelectDemand demand -> { model with SelectedDemand = demand }, Cmd.none
        | SearchTextChanged txt -> { model with SearchText = txt }, Cmd.none
        | RefreshRequested -> model, Cmd.none

    let view (model: Model) (dispatch: Msg -> unit) : Node =
        let filterDemands (items: DemandLine list) =
            if String.IsNullOrWhiteSpace model.SearchText then
                items
            else
                items
                |> List.filter(fun d ->
                    d.SkuCode.Contains(model.SearchText, StringComparison.OrdinalIgnoreCase)
                    || d.CustomerName.Contains(model.SearchText, StringComparison.OrdinalIgnoreCase)
                    || d.DemandLineId.Contains(model.SearchText, StringComparison.OrdinalIgnoreCase))

        let renderGrid (items: DemandLine list) =
            let filtered = filterDemands items

            let gridConfig =
                { Columns =
                    [ Rz.dataGridColumn<DemandLine>("DemandLineId", "Line ID", width = "110px")
                      Rz.dataGridColumn<DemandLine>("SkuCode", "SKU Code", width = "120px")
                      Rz.dataGridColumn<DemandLine>("SkuName", "Product", width = "180px")
                      Rz.dataGridColumn<DemandLine>("CustomerName", "Customer", width = "180px")
                      Rz.dataGridColumn<DemandLine>("RequestedQty", "Qty", width = "80px")
                      Rz.dataGridColumn<DemandLine>("RequestedDeliveryDate", "Due Date", width = "110px")
                      Rz.dataGridColumn<DemandLine>("Status", "Status", width = "110px") ]
                  Data = filtered
                  IsLoading = false
                  OnRowSelected = fun item -> SelectDemand(Some item) }

            comp<GridPanel<DemandLine, Msg>> {
                "Config" => gridConfig
                "Dispatch" => dispatch
            }

        div {
            attr.``class`` "rz-p-4"
            attr.style "display: flex; flex-direction: column; gap: 16px; height: 100%;"

            div {
                attr.style "display: flex; align-items: center; gap: 12px;"

                Rz.textBox(
                    value = model.SearchText,
                    placeholder = "Search by SKU, Customer, or Line ID...",
                    style = "flex: 1;",
                    valueChanged = (SearchTextChanged >> dispatch)
                )

                Rz.button("Refresh", (fun _ -> dispatch RefreshRequested), Radzen.ButtonStyle.Secondary)

            }

            div {
                attr.style "display: flex; gap: 16px; flex: 1;"

                div {
                    attr.style "flex: 2; min-width: 0;"

                    comp<RemoteState<DemandLine list>> {
                        "Data" => model.Demands
                        "Template" => (fun items -> renderGrid items)
                    }
                }

                match model.SelectedDemand with
                | Some demand ->
                    div {
                        attr.style "flex: 1; min-width: 300px;"

                        let detailConfig =
                            { Title = sprintf "Demand Line details - %s" demand.DemandLineId
                              Items =
                                [ "Order ID", demand.DemandOrderId
                                  "SKU Code", demand.SkuCode
                                  "SKU Name", demand.SkuName
                                  "Customer", demand.CustomerName
                                  "Quantity", sprintf "%M %s" demand.RequestedQty demand.UnitOfMeasure
                                  "Due Date", demand.RequestedDeliveryDate.ToString("yyyy-MM-dd")
                                  "Priority", sprintf "%d" demand.Priority
                                  "Lateness Risk", sprintf "%A" demand.LatenessRisk
                                  "Status", demand.Status ]
                              OnClose = fun () -> dispatch(SelectDemand None) }

                        comp<DetailPanel> { "Config" => detailConfig }
                    }
                | None -> empty()
            }
        }
