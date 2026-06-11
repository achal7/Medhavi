namespace Medhavi.Web.Pages

open System
open Bolero
open Bolero.Html
open Elmish
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Medhavi.Web
open Medhavi.Web.Components
open Medhavi.Web.Stores
open Medhavi.Nexus
open Radzen
open Radzen.Blazor

module DemandWorkbench =
    type Model = {
        Demands: DemandViewItem list
        IsLoading: bool
        ErrorMessage: string option
        PendingSearchText: string
        SearchText: string
        SelectedDemandId: string option
        IsLoadingDetails: bool
        Details: string option
    }

    type Msg =
        | LoadDemands of DemandViewItem list
        | ShowError of string
        | SetLoading of bool
        | SearchTextChanged of string
        | TriggerSearch of string
        | SelectDemand of string option
        | LoadDetails of string

    let init () = 
        { Demands = []
          IsLoading = true
          ErrorMessage = None
          PendingSearchText = ""
          SearchText = ""
          SelectedDemandId = None
          IsLoadingDetails = false
          Details = None }

    let update msg model =
        match msg with
        | LoadDemands demands ->
            { model with Demands = demands; IsLoading = false; ErrorMessage = None }, Cmd.none
        | ShowError err ->
            { model with ErrorMessage = Some err; IsLoading = false }, Cmd.none
        | SetLoading loading ->
            { model with IsLoading = loading }, Cmd.none
        | SearchTextChanged text ->
            let searchCmd =
                Cmd.OfAsync.either
                    (fun () -> Async.Sleep 300)
                    ()
                    (fun () -> TriggerSearch text)
                    (fun ex -> ShowError ex.Message)
            { model with PendingSearchText = text }, searchCmd
        | TriggerSearch text ->
            if text = model.PendingSearchText then
                { model with SearchText = text }, Cmd.none
            else
                model, Cmd.none
        | SelectDemand idOpt ->
            match idOpt with
            | Some id ->
                let activeDemand = model.Demands |> List.tryFind (fun d -> d.DemandOrderId = id)
                let detailCmd =
                    Cmd.OfAsync.either
                        (fun () -> Async.Sleep 500)
                        ()
                        (fun () -> 
                            let detailsText = 
                                match activeDemand with
                                | Some d ->
                                    sprintf "Lazy Loaded Trace for Order: %s\n-----------------------------\nSKU Code: %s\nStocking Location: %s\nRequested Quantity: %M %s\nDelivery Promise Date: %s\nPriority Class: %d\nMRP Pegging Status: %s\nException: None" 
                                        d.DemandOrderId d.SkuId d.StockingPointId d.Quantity d.UnitOfMeasure (d.RequestedDeliveryDate.ToString("yyyy-MM-dd")) d.Priority d.Status
                                | None -> sprintf "Order %s details retrieved." id
                            LoadDetails detailsText)
                        (fun ex -> ShowError ex.Message)
                { model with SelectedDemandId = Some id; IsLoadingDetails = true; Details = None }, detailCmd
            | None ->
                { model with SelectedDemandId = None; IsLoadingDetails = false; Details = None }, Cmd.none
        | LoadDetails details ->
            { model with Details = Some details; IsLoadingDetails = false }, Cmd.none

    let view (model: Model) dispatch =
        let filteredDemands =
            if String.IsNullOrWhiteSpace(model.SearchText) then
                model.Demands
            else
                model.Demands 
                |> List.filter (fun d -> 
                    d.SkuId.Contains(model.SearchText, StringComparison.OrdinalIgnoreCase) ||
                    d.DemandOrderId.Contains(model.SearchText, StringComparison.OrdinalIgnoreCase))

        div {
            attr.``class`` "p-4"
            h3 { attr.``class`` "rz-text-h4 rz-mb-4"; "Demand Workbench" }
            
            if model.IsLoading then
                div {
                    Rz.progressBar(50.0, mode = ProgressBarMode.Indeterminate)
                    p { "Loading demand lines..." }
                }
            elif model.ErrorMessage.IsSome then
                div {
                    attr.``class`` "notification is-danger"
                    model.ErrorMessage.Value
                }
            else
                // Search box
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

                // Virtualized data grid
                div {
                    attr.``class`` "rz-mb-4"
                    Rz.dataGrid<DemandViewItem> (
                        data = filteredDemands,
                        columns = [
                            Rz.dataGridColumn<DemandViewItem>("DemandOrderId", "Order ID")
                            Rz.dataGridColumn<DemandViewItem>("SkuId", "SKU ID")
                            Rz.dataGridColumn<DemandViewItem>("StockingPointId", "Stocking Point")
                            Rz.dataGridColumn<DemandViewItem>("Quantity", "Quantity")
                            Rz.dataGridColumn<DemandViewItem>("UnitOfMeasure", "UOM")
                            Rz.dataGridColumn<DemandViewItem>("RequestedDeliveryDate", "Requested Date")
                            Rz.dataGridColumn<DemandViewItem>("Priority", "Priority")
                            Rz.dataGridColumn<DemandViewItem>("Status", "Status")
                            
                            // Selection Action Button
                            comp<RadzenDataGridColumn<DemandViewItem>> {
                                "Title" => "Details"
                                "Width" => "100px"
                                "TextAlign" => TextAlign.Center
                                "Template" => RenderFragment<DemandViewItem>(fun d ->
                                    RenderFragment(fun b ->
                                        b.OpenComponent<RadzenButton>(1)
                                        b.AddAttribute(2, "Icon", "info")
                                        b.AddAttribute(3, "ButtonStyle", ButtonStyle.Info)
                                        b.AddAttribute(4, "Size", ButtonSize.Small)
                                        let onClick = EventCallback.Factory.Create<MouseEventArgs>((dispatch :> obj), Action<MouseEventArgs>(fun _ -> dispatch (SelectDemand (Some d.DemandOrderId))))
                                        b.AddAttribute(5, "Click", onClick)
                                        b.CloseComponent()
                                    )
                                )
                            }
                        ],
                        allowFiltering = true,
                        allowSorting = true,
                        allowPaging = false, // Virtualization works best with paging disabled
                        allowVirtualization = true,
                        height = "400px"
                    )
                }

                // Lazy loaded Selected details drawer
                match model.SelectedDemandId with
                | Some selectedId ->
                    comp<RadzenCard> {
                        "Style" => "margin-top: 24px; padding: 20px; border-radius: 8px; border: 1px solid var(--rz-info-color); background-color: rgba(33, 150, 243, 0.02);"
                        Rz.stack([
                            div {
                                attr.style "display: flex; justify-content: space-between; align-items: center;"
                                h4 { attr.style "font-weight: bold; margin: 0; color: var(--rz-info-color); font-family: var(--rz-font-family);"; sprintf "Order Details: %s" selectedId }
                                button {
                                    attr.style "background: transparent; border: none; cursor: pointer; color: var(--rz-text-secondary-color); padding: 4px;"
                                    on.click (fun _ -> dispatch (SelectDemand None))
                                    Rz.icon("close")
                                }
                            }
                            if model.IsLoadingDetails then
                                Rz.progressBar(50.0, mode = ProgressBarMode.Indeterminate)
                            else
                                match model.Details with
                                | Some details ->
                                    pre {
                                        attr.style "margin: 0; font-family: monospace; font-size: 12px; white-space: pre-wrap; color: var(--rz-text-color);"
                                        details
                                    }
                                | None -> empty()
                        ], gap = "12px")
                    }
                | None -> empty()
        }
