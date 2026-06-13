namespace Medhavi.Web.CapacityWorkbench

open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Medhavi.Web.Components
open Medhavi.Web.Panels
open Microsoft.AspNetCore.Components
open System

open Medhavi.Contracts.Capacity

module View =

    let render (model: Model) (dispatch: Msg -> unit) =
        let renderSummary (items: OperationView list) =
            let filtered =
                if String.IsNullOrWhiteSpace(model.SearchText) then
                    items
                else
                    items
                    |> List.filter (fun o ->
                        o.OperationId.Contains(model.SearchText, StringComparison.OrdinalIgnoreCase) ||
                        o.RoutingStepId.Contains(model.SearchText, StringComparison.OrdinalIgnoreCase))

            div {
                Rz.stack([
                    Rz.icon("search")
                    input {
                        attr.``class`` "rz-textbox"
                        attr.placeholder "Search Operation ID or Step ID..."
                        attr.value model.PendingSearchText
                        on.input (fun (e: ChangeEventArgs) ->
                            let txt = match e.Value with null -> "" | v -> string v
                            dispatch (SearchTextChanged txt))
                        attr.style "width: 300px;"
                    }
                ], orientation = Orientation.Horizontal, alignItems = AlignItems.Center, gap = "8px", class' = "rz-mb-4")

                div {
                    attr.``class`` "rz-mb-4"
                    CapacityGridPanel.render filtered dispatch
                }

                match model.SelectedOperation with
                | Some selected ->
                    comp<RadzenCard> {
                        "Style" => "margin-top: 24px; padding: 20px; border-radius: 8px; border: 1px solid var(--rz-info-color); background-color: rgba(33, 150, 243, 0.02);"
                        Rz.stack([
                            div {
                                attr.style "display: flex; justify-content: space-between; align-items: center;"
                                h4 { attr.style "font-weight: bold; margin: 0; color: var(--rz-info-color); font-family: var(--rz-font-family);"; sprintf "Capacity Operation Details: %s" selected.OperationId }
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
            h3 { attr.``class`` "rz-text-h4 rz-mb-4"; "Capacity Workbench" }

            comp<RemoteState<OperationView list>> {
                "Data" => model.SummaryData
                "Template" => (fun (items: OperationView list) -> renderSummary items)
            }
        }
