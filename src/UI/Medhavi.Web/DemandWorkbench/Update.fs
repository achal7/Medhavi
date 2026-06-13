namespace Medhavi.Web.DemandWorkbench

open System
open Elmish
open Medhavi.Web.WorkspaceEngine
open Medhavi.Web.Panels
open Medhavi.Web.Stores
open Medhavi.Contracts.Demand

module Update =

    let init (workspaceId: WorkspaceId) (context: WorkspaceContext) : Model * Cmd<Msg> =
        { WorkspaceId = workspaceId
          Context = context
          SummaryData = NotRequested
          PendingSearchText = ""
          SearchText = ""
          SelectedDemand = None
          IsLoadingDetails = false
          DetailsText = None }, Cmd.ofMsg LoadSummary

    let update (demandStore: DemandStore) (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        | LoadSummary ->
            let loadAsync () =
                async {
                    try
                        let snapshot = demandStore.GetSnapshot()
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
                    return sprintf "Lazy Loaded Trace for Order: %s\n-----------------------------\nSKU Code: %s\nStocking Location: %s\nQuantity: %M %s\nDelivery Promise Date: %s\nPriority Class: %d\nMRP Pegging Status: %s\nException: None" 
                        row.DemandOrderId row.SkuId row.StockingPointId row.RequestedQty row.UnitOfMeasure (row.RequestedDeliveryDate.ToString("yyyy-MM-dd")) row.Priority row.Status
                }
            let cmd = Cmd.OfAsync.perform loadDetailsAsync () DetailsLoaded
            { model with SelectedDemand = Some row; IsLoadingDetails = true; DetailsText = None }, cmd

        | DetailsLoaded text ->
            { model with DetailsText = Some text; IsLoadingDetails = false }, Cmd.none

        | CloseDetails ->
            { model with SelectedDemand = None; DetailsText = None }, Cmd.none
