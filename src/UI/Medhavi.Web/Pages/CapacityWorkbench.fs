namespace Medhavi.Web.Pages

open System
open Bolero
open Bolero.Html
open Medhavi.Web
open Medhavi.Web.Components
open Medhavi.Web.Stores
open Medhavi.Analytics.PlanningHorizon
open Radzen

module CapacityWorkbench =
    type Model = {
        Operations: OperationView list
        IsLoading: bool
        ErrorMessage: string option
    }

    type Msg =
        | LoadOperations of OperationView list
        | ShowError of string
        | SetLoading of bool

    let init () = 
        { Operations = []
          IsLoading = true
          ErrorMessage = None }

    let update msg model =
        match msg with
        | LoadOperations ops ->
            { model with Operations = ops; IsLoading = false; ErrorMessage = None }
        | ShowError err ->
            { model with ErrorMessage = Some err; IsLoading = false }
        | SetLoading loading ->
            { model with IsLoading = loading }

    let view (model: Model) dispatch =
        div {
            attr.``class`` "p-4"
            h3 { attr.``class`` "title is-3"; "Capacity Workbench" }
            
            if model.IsLoading then
                div {
                    Rz.progressBar(50.0, mode = ProgressBarMode.Indeterminate)
                    p { "Loading capacity operations..." }
                }
            elif model.ErrorMessage.IsSome then
                div {
                    attr.``class`` "notification is-danger"
                    model.ErrorMessage.Value
                }
            else
                Rz.dataGrid<OperationView> (
                    data = model.Operations,
                    columns = [
                        Rz.dataGridColumn<OperationView>("OperationId", "Operation ID")
                        Rz.dataGridColumn<OperationView>("RoutingStepId", "Step ID")
                        Rz.dataGridColumn<OperationView>("RunMinutes", "Run (min)")
                        Rz.dataGridColumn<OperationView>("StartTime", "Start Time")
                        Rz.dataGridColumn<OperationView>("EndTime", "End Time")
                        Rz.dataGridColumn<OperationView>("Status", "Status")
                        Rz.dataGridColumn<OperationView>("IsFirm", "Firm?")
                    ],
                    allowFiltering = true,
                    allowSorting = true,
                    allowPaging = true,
                    pageSize = 10
                )
        }
