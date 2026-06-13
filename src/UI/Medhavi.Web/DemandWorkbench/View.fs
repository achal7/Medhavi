namespace Medhavi.Web.DemandWorkbench

open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Medhavi.Web.Components
open Medhavi.Web.Panels
open Microsoft.AspNetCore.Components
open System

open Medhavi.Contracts.Demand

module View =

    let render (model: Model) (dispatch: Msg -> unit) =
        let renderSummary (items: DemandLine list) =
            let filtered =
                if String.IsNullOrWhiteSpace(model.SearchText) then
                    items
                else
                    items
                    |> List.filter (fun d ->
                        d.SkuId.Contains(model.SearchText, StringComparison.OrdinalIgnoreCase) ||
                        d.DemandOrderId.Contains(model.SearchText, StringComparison.OrdinalIgnoreCase) ||
                        d.StockingPointId.Contains(model.SearchText, StringComparison.OrdinalIgnoreCase))

            div {
                Rz.stack([
                    Rz.icon("search")
                    input {
                        attr.``class`` "rz-textbox"
                        attr.placeholder "Search SKU or Order ID..."
                        attr.value model.PendingSearchText
                        on.input (fun (e: ChangeEventArgs) ->
                            let txt = match e.Value with null -> "" | v -> string v
                            dispatch (SearchTextChanged txt))
                        attr.style "width: 300px;"
                    }
                ], orientation = Orientation.Horizontal, alignItems = AlignItems.Center, gap = "8px", class' = "rz-mb-4")

                div {
                    attr.``class`` "rz-mb-4"
                    DemandGridPanel.render filtered dispatch
                }

                match model.SelectedDemand with
                | Some selected ->
                    comp<RadzenCard> {
                        "Style" => "margin-top: 24px; padding: 20px; border-radius: 8px; border: 1px solid var(--rz-info-color); background-color: rgba(33, 150, 243, 0.02);"
                        Rz.stack([
                            div {
                                attr.style "display: flex; justify-content: space-between; align-items: center;"
                                h4 { attr.style "font-weight: bold; margin: 0; color: var(--rz-info-color); font-family: var(--rz-font-family);"; sprintf "Order Details: %s" selected.DemandOrderId }
                                button {
                                    attr.style "background: transparent; border: none; cursor: pointer; color: var(--rz-text-secondary-color); padding: 4px;"
                                    on.click (fun _ -> dispatch CloseDetails)
                                    Rz.icon("close")
                                }
                            }
                            if model.IsLoadingDetails then
                                Rz.progressBar(50.0, mode = ProgressBarMode.Indeterminate)
                            else
                                match model.DetailsText with
                                | Some text ->
                                    pre {
                                        attr.style "margin: 0; font-family: monospace; font-size: 12px; white-space: pre-wrap; color: var(--rz-text-color);"
                                        text
                                    }
                                | None -> empty()
                        ], gap = "12px")
                    }
                | None -> empty()
            }

        div {
            attr.``class`` "p-4"
            h3 { attr.``class`` "rz-text-h4 rz-mb-4"; "Demand Workbench" }

            comp<RemoteState<DemandLine list>> {
                "Data" => model.SummaryData
                "Template" => (fun (items: DemandLine list) -> renderSummary items)
            }
        }
