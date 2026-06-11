namespace Medhavi.Web.Pages

open System
open Bolero
open Bolero.Html
open Medhavi.Web
open Medhavi.Web.Components
open Medhavi.Web.Stores
open Medhavi.Analytics.PlanningHorizon
open Radzen

module SupplyWorkbench =
    type Model = {
        Supplies: SupplyElementView list
        IsLoading: bool
        ErrorMessage: string option
    }

    type Msg =
        | LoadSupplies of SupplyElementView list
        | ShowError of string
        | SetLoading of bool

    let init () = 
        { Supplies = []
          IsLoading = true
          ErrorMessage = None }

    let update msg model =
        match msg with
        | LoadSupplies supplies ->
            { model with Supplies = supplies; IsLoading = false; ErrorMessage = None }
        | ShowError err ->
            { model with ErrorMessage = Some err; IsLoading = false }
        | SetLoading loading ->
            { model with IsLoading = loading }

    let view (model: Model) dispatch =
        div {
            attr.``class`` "p-4"
            h3 { attr.``class`` "title is-3"; "Supply Workbench" }
            
            if model.IsLoading then
                div {
                    Rz.progressBar(50.0, mode = ProgressBarMode.Indeterminate)
                    p { "Loading supply items..." }
                }
            elif model.ErrorMessage.IsSome then
                div {
                    attr.``class`` "notification is-danger"
                    model.ErrorMessage.Value
                }
            else
                Rz.dataGrid<SupplyElementView> (
                    data = model.Supplies,
                    columns = [
                        Rz.dataGridColumn<SupplyElementView>("SupplyOrderId", "Order ID")
                        Rz.dataGridColumn<SupplyElementView>("SupplyType", "Type")
                        Rz.dataGridColumn<SupplyElementView>("SkuId", "SKU ID")
                        Rz.dataGridColumn<SupplyElementView>("StockingPointId", "Stocking Point")
                        Rz.dataGridColumn<SupplyElementView>("PlannedQty", "Planned Qty")
                        Rz.dataGridColumn<SupplyElementView>("ConfirmedQty", "Confirmed Qty")
                        Rz.dataGridColumn<SupplyElementView>("PlannedDate", "Planned Date")
                        Rz.dataGridColumn<SupplyElementView>("IsFirm", "Firm?")
                        Rz.dataGridColumn<SupplyElementView>("IsLocked", "Locked?")
                    ],
                    allowFiltering = true,
                    allowSorting = true,
                    allowPaging = true,
                    pageSize = 10
                )
        }
