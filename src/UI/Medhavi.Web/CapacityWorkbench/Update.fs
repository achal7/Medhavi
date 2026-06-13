namespace Medhavi.Web.CapacityWorkbench

open System
open Elmish
open Medhavi.Web.WorkspaceEngine
open Medhavi.Web.Panels
open Medhavi.Web.Stores
open Medhavi.Contracts.Capacity

module Update =

    let init (workspaceId: WorkspaceId) (context: WorkspaceContext) : Model * Cmd<Msg> =
        { WorkspaceId = workspaceId
          Context = context
          SummaryData = NotRequested
          PendingSearchText = ""
          SearchText = ""
          SelectedOperation = None
          IsLoadingDetails = false
          DetailsText = None }, Cmd.ofMsg LoadSummary

    let update (capacityStore: CapacityStore) (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        | LoadSummary ->
            let loadAsync () =
                async {
                    try
                        let snapshot = capacityStore.GetSnapshot()
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
                    return sprintf "Lazy Loaded Trace for Capacity Operation: %s\n-----------------------------\nRouting Step: %s\nOperation Code: %s\nStatus: %A\nPlanned Runtime: %M mins\nSetup Time: %M mins\nStart Time: %s\nEnd Time: %s\nFirm Status: %b\nFrozen Status: %b\nExpedited Status: %b\nPegged Demand Order ID: %s"
                        row.OperationId row.RoutingStepId row.OperationCode row.Status row.RunMinutes row.SetupMinutes (row.StartTime.ToString("yyyy-MM-dd HH:mm")) (row.EndTime.ToString("yyyy-MM-dd HH:mm")) row.IsFirm row.IsFrozen row.IsExpedited (row.DemandOrderId |> Option.defaultValue "N/A")
                }
            let cmd = Cmd.OfAsync.perform loadDetailsAsync () DetailsLoaded
            { model with SelectedOperation = Some row; IsLoadingDetails = true; DetailsText = None }, cmd

        | DetailsLoaded text ->
            { model with DetailsText = Some text; IsLoadingDetails = false }, Cmd.none

        | CloseDetails ->
            { model with SelectedOperation = None; DetailsText = None }, Cmd.none
