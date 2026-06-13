namespace Medhavi.Web.DemandWorkbench

open System
open Elmish
open Medhavi.Web.WorkspaceEngine
open Medhavi.Web.Panels
open Medhavi.Web.Stores
open Medhavi.Contracts.Demand
open Medhavi.SharedKernel.ScenarioContracts

module Update =

    let init (workspaceId: WorkspaceId) (context: WorkspaceContext) : Model * Cmd<Msg> =
        { WorkspaceId = workspaceId
          Context = context
          SummaryData = NotRequested
          PendingSearchText = ""
          SearchText = ""
          SelectedDemand = None
          IsLoadingDetails = false
          DetailsText = None
          OverrideQtyInput = ""
          OverrideReasonInput = ""
          IsSubmittingOverride = false
          OverrideError = None }, Cmd.ofMsg LoadSummary

    let update (demandStore: DemandStore) (scenarioStore: ScenarioStore) (msg: Msg) (model: Model) : Model * Cmd<Msg> =
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
            { model with SelectedDemand = Some row; IsLoadingDetails = true; DetailsText = None; OverrideQtyInput = ""; OverrideReasonInput = ""; OverrideError = None }, cmd

        | DetailsLoaded text ->
            { model with DetailsText = Some text; IsLoadingDetails = false }, Cmd.none

        | CloseDetails ->
            { model with SelectedDemand = None; DetailsText = None; OverrideQtyInput = ""; OverrideReasonInput = ""; OverrideError = None }, Cmd.none

        | UpdateOverrideQty text ->
            { model with OverrideQtyInput = text }, Cmd.none

        | UpdateOverrideReason text ->
            { model with OverrideReasonInput = text }, Cmd.none

        | SubmitOverride ->
            match model.SelectedDemand with
            | None -> { model with OverrideError = Some "No demand line selected." }, Cmd.none
            | Some selected ->
                match Decimal.TryParse(model.OverrideQtyInput) with
                | false, _ ->
                    { model with OverrideError = Some "Invalid quantity format." }, Cmd.none
                | true, parsedQty ->
                    let activeScenarioIdOpt = model.Context.CurrentScope.ScenarioId
                    match activeScenarioIdOpt with
                    | None ->
                        { model with OverrideError = Some "Cannot apply overrides on BASELINE. Please select/create a What-If scenario first." }, Cmd.none
                    | Some scenarioId when scenarioId.Equals("baseline", StringComparison.OrdinalIgnoreCase) ->
                        { model with OverrideError = Some "Cannot apply overrides on BASELINE. Please select/create a What-If scenario first." }, Cmd.none
                    | Some scenarioId ->
                        let reason = if String.IsNullOrWhiteSpace(model.OverrideReasonInput) then "No reason provided" else model.OverrideReasonInput
                        let ov = DemandOverride(selected.DemandLineId, parsedQty, reason)
                        let submitAsync () =
                            async {
                                try
                                    let! res = scenarioStore.AddOverride(scenarioId, ov) |> Async.AwaitTask
                                    return res
                                with ex ->
                                    return Error ex.Message
                            }
                        let cmd =
                            Cmd.OfAsync.either
                                submitAsync
                                ()
                                (fun res -> OverrideApplied res)
                                (fun ex -> OverrideApplied (Error ex.Message))
                        { model with IsSubmittingOverride = true; OverrideError = None }, cmd

        | OverrideApplied res ->
            match res with
            | Ok () ->
                { model with
                    IsSubmittingOverride = false
                    OverrideQtyInput = ""
                    OverrideReasonInput = ""
                    OverrideError = None }, Cmd.ofMsg LoadSummary
            | Error err ->
                { model with
                    IsSubmittingOverride = false
                    OverrideError = Some err }, Cmd.none
