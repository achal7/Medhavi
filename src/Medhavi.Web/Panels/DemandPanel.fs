namespace Medhavi.Web.Panels

open System
open Bolero
open Bolero.Html
open Elmish
open Medhavi.Contracts.Demand
open Medhavi.Web.Controls

module DemandPanel =

    let inline callback f = Some(fun arg -> f arg) // for one-arg functions
    let inline callback0 f = Some(fun () -> f()) // for zero-arg

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
            let totalQty = filtered |> List.sumBy(fun d -> d.RequestedQty)
            let rowCount = filtered |> List.length

            let gridConfig =
                { Columns =
                    [ Rz.dataGridColumn<DemandLine>(
                          "DemandLineId",
                          "Line ID",
                          width = "110px",
                          footer = text(sprintf "Rows: %d" rowCount),
                          frozenPosition = Radzen.FrozenColumnPosition.Left,
                          isFrozen = true
                      )
                      Rz.dataGridColumn<DemandLine>(
                          "Type",
                          "Type",
                          width = "40px",
                          sortable = false,
                          filterable = false,
                          template =
                              fun d ->
                                  let icon = if d.DemandCategory = "CustomerOrder" then "person" else "trending_up"
                                  comp<Radzen.Blazor.RadzenIcon> { "Icon" => icon }
                      )
                      Rz.dataGridColumn<DemandLine>("SkuCode", "SKU Code", width = "120px")
                      Rz.dataGridColumn<DemandLine>("SkuName", "Product", width = "180px")
                      Rz.dataGridColumn<DemandLine>("CustomerName", "Customer", width = "180px")
                      Rz.dataGridColumn<DemandLine>(
                          "RequestedQty",
                          "Qty",
                          width = "80px",
                          footer = text(sprintf "Total: %M" totalQty)
                      )
                      Rz.dataGridColumn<DemandLine>("RequestedDeliveryDate", "Due Date", width = "110px")
                      Rz.dataGridColumn<DemandLine>(
                          "Status",
                          "Status",
                          width = "110px",
                          filterMode = Radzen.FilterMode.CheckBoxList,
                          sortable = true,
                          filterable = true
                      ) ]
                  Data = filtered
                  IsLoading = false
                  OnRowSelected = fun item -> SelectDemand(Some item) }

            comp<GridPanel<DemandLine, Msg>> {
                "Config" => gridConfig
                "Dispatch" => dispatch
                "SearchText" => model.SearchText
                "SearchPlaceholder" => "Search by SKU, Customer, or Line ID.."
                "OnSearchChanged" => callback(fun s -> dispatch(SearchTextChanged s))
                "OnRefresh" => callback0(fun () -> dispatch RefreshRequested)
                "ShowGroupingToggle" => true
            }

        div {

            div {
                attr.style "display: flex; flex: 1;"

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

                        comp<DetailPanel> {
                            "Config" => detailConfig
                            "OnRefresh" => (fun () -> dispatch RefreshRequested)
                            "ShowGroupingToggle" => true
                        }
                    }
                | None -> empty()
            }
        }
