namespace Medhavi.Web.SupplyWorkbench

open System
open Elmish
open Medhavi.Web.WorkspaceEngine
open Medhavi.Web.Panels
open Medhavi.Web.Stores
open Medhavi.Contracts.Supply

module Update =

    let init (workspaceId: WorkspaceId) (context: WorkspaceContext) : Model * Cmd<Msg> =
        { WorkspaceId = workspaceId
          Context = context
          SummaryData = NotRequested
          PendingSearchText = ""
          SearchText = ""
          SelectedSupply = None
          IsLoadingDetails = false
          DetailsText = None }, Cmd.ofMsg LoadSummary

    let update (supplyStore: SupplyStore) (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        | LoadSummary ->
            let loadAsync () =
                async {
                    try
                        let snapshot = supplyStore.GetSnapshot()
                        return Ok snapshot
                    with ex ->
                        return Error ex.Message
                }
            let cmd =
                Cmd.OfAsync.either
                    loadAsync
                    ()
                    (function
                        | Ok items -> LoadedSummary items
                        | Error err -> LoadFailed err)
                    (fun ex -> LoadFailed ex.Message)
            { model with SummaryData = Loading }, cmd

        | LoadedSummary items ->
            { model with SummaryData = Loaded items }, Cmd.none

        | LoadFailed err ->
            { model with SummaryData = Failed err }, Cmd.none

        | SearchTextChanged text ->
            let searchCmd =
                Cmd.OfAsync.either
                    (fun () -> Async.Sleep 300)
                    ()
                    (fun () -> TriggerSearch text)
                    (fun ex -> LoadFailed ex.Message)
            { model with PendingSearchText = text }, searchCmd

        | TriggerSearch text ->
            if text = model.PendingSearchText then
                { model with SearchText = text }, Cmd.none
            else
                model, Cmd.none

        | RowSelected row ->
            let loadDetailsAsync () =
                async {
                    do! Async.Sleep 300
                    let plannedDateStr =
                        row.RequiredDeliveryDate
                        |> Option.map (fun d -> d.ToString("yyyy-MM-dd"))
                        |> Option.defaultValue "N/A"
                    return sprintf "Lazy Loaded Trace for Supply Order: %s\n-----------------------------\nSKU ID: %s\nStocking Point: %s\nType: %s\nPlanned Qty: %M\nConfirmed Qty: %M\nPlanned Date: %s\nFirm Status: %b\nLocked Status: %b\nExpedited Status: %b\nRouting ID: %s\nSupplier ID: %s"
                        row.Id row.SkuId row.StockingPointId row.OrderType row.Quantity row.CompletedQuantity plannedDateStr row.IsFirm row.IsLocked row.IsExpedited (row.RoutingId |> Option.defaultValue "N/A") (row.SupplierId |> Option.defaultValue "N/A")
                }
            let cmd = Cmd.OfAsync.perform loadDetailsAsync () DetailsLoaded
            { model with SelectedSupply = Some row; IsLoadingDetails = true; DetailsText = None }, cmd

        | DetailsLoaded text ->
            { model with DetailsText = Some text; IsLoadingDetails = false }, Cmd.none

        | CloseDetails ->
            { model with SelectedSupply = None; DetailsText = None }, Cmd.none
