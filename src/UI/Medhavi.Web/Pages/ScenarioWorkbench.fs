namespace Medhavi.Web.Pages

open System
open Bolero
open Bolero.Html
open Medhavi.Web
open Medhavi.Web.Components
open Microsoft.AspNetCore.Components.Web
open Medhavi.Web.Stores
open Medhavi.Scenario
open Medhavi.SharedKernel.ScenarioContracts
open Radzen
open Radzen.Blazor

module ScenarioWorkbench =
    type Model = {
        Scenarios: ScenarioReadModel list
        IsLoading: bool
        ErrorMessage: string option
        ActiveScenarioId: string option
        CompareScenarioId: string option
    }

    type Msg =
        | LoadScenarios of ScenarioReadModel list
        | ShowError of string
        | SetLoading of bool
        | SelectActiveScenario of string option
        | SelectCompareScenario of string option

    let init () = 
        { Scenarios = []
          IsLoading = true
          ErrorMessage = None
          ActiveScenarioId = None
          CompareScenarioId = None }

    let update msg model =
        match msg with
        | LoadScenarios scenarios ->
            { model with Scenarios = scenarios; IsLoading = false; ErrorMessage = None }
        | ShowError err ->
            { model with ErrorMessage = Some err; IsLoading = false }
        | SetLoading loading ->
            { model with IsLoading = loading }
        | SelectActiveScenario idOpt ->
            { model with ActiveScenarioId = idOpt }
        | SelectCompareScenario idOpt ->
            { model with CompareScenarioId = idOpt }

    let renderOverride ov =
        match ov with
        | DemandOverride (demandId, overrideQty, reason) ->
            sprintf "Demand Order '%s': Qty override to %M (%s)" demandId overrideQty reason
        | InventoryOverride (skuId, stockingPointId, overrideQty) ->
            sprintf "Inventory SKU '%s' at SP '%s': On-hand adjusted to %M" skuId stockingPointId overrideQty
        | LeadTimeOverride (skuId, overrideDays, reason) ->
            sprintf "Lead Time SKU '%s': %d days (%s)" skuId overrideDays reason
        | CapacityOverride (resourceId, bucket, extraQty) ->
            sprintf "Capacity Resource '%s': Added %M Qty on %s" resourceId extraQty (bucket.ToString("yyyy-MM-dd"))
        | SupplierSuspension (supplierId, bucket) ->
            sprintf "Supplier Suspension '%s': Suspended during '%s'" supplierId bucket
        | BomOverride (parent, comp, qtyPer) ->
            sprintf "BOM parent '%s' -> component '%s': Qty-per override to %M" parent comp qtyPer

    let view (model: Model) dispatch (onRunMrp: unit -> unit) canRun =

        div {
            attr.``class`` "p-4"
            
            // Header / Toolbar
            div {
                attr.``class`` "rz-display-flex rz-justify-content-space-between rz-align-items-center rz-mb-4"
                h3 { attr.``class`` "rz-text-h4 rz-m-0"; "Scenario Workbench" }
                comp<RadzenButton> {
                    "Text" => "Run Baseline MRP"
                    "Icon" => "play_arrow"
                    "ButtonStyle" => ButtonStyle.Success
                    "Disabled" => not canRun
                    if canRun then attr.callback "Click" (fun (e: MouseEventArgs) -> onRunMrp()) else attr.empty()
                }
            }
            
            if model.IsLoading then
                div {
                    Rz.progressBar(50.0, mode = ProgressBarMode.Indeterminate)
                    p { "Loading scenarios..." }
                }
            elif model.ErrorMessage.IsSome then
                div {
                    attr.``class`` "notification is-danger"
                    model.ErrorMessage.Value
                }
            else
                // Scenarios list grid
                div {
                    attr.``class`` "rz-mb-4"
                    Rz.dataGrid<ScenarioReadModel> (
                        data = model.Scenarios,
                        columns = [
                            Rz.dataGridColumn<ScenarioReadModel>("ScenarioId", "Scenario ID")
                            Rz.dataGridColumn<ScenarioReadModel>("Name", "Name")
                            Rz.dataGridColumn<ScenarioReadModel>("BaseScenarioId", "Base ID")
                            Rz.dataGridColumn<ScenarioReadModel>("Version", "Version")
                            Rz.dataGridColumn<ScenarioReadModel>("CreatedAt", "Created At")
                            Rz.dataGridColumn<ScenarioReadModel>("IsActive", "Active?")
                        ],
                        allowFiltering = true,
                        allowSorting = true,
                        allowPaging = true,
                        pageSize = 5
                    )
                }

                // Scenario Comparison setup
                comp<RadzenCard> {
                    "Style" => "margin-top: 24px; padding: 20px; border-radius: 8px;"
                    Rz.stack([
                        h4 { attr.``class`` "rz-text-h6 rz-m-0"; "Compare Scenarios 🔍" }
                        span { attr.``class`` "rz-color-text-secondary"; "Select two scenarios to compare their characteristics and overrides." }
                        
                        div {
                            attr.style "display: flex; gap: 20px; margin-top: 10px; flex-wrap: wrap;"
                            
                            // Active scenario selection
                            div {
                                attr.style "display: flex; flex-direction: column; gap: 4px; min-width: 250px;"
                                label { attr.style "font-size: 12px; font-weight: bold;"; "Active Scenario" }
                                let data = "" :: (model.Scenarios |> List.map (fun s -> s.ScenarioId))
                                let currentVal = model.ActiveScenarioId |> Option.defaultValue ""
                                Rz.dropDown(data, currentVal, onChange = (fun v ->
                                    let s = v :?> string
                                    let opt = if String.IsNullOrEmpty(s) then None else Some s
                                    dispatch (SelectActiveScenario opt)
                                ))
                            }

                            // Comparison scenario selection
                            div {
                                attr.style "display: flex; flex-direction: column; gap: 4px; min-width: 250px;"
                                label { attr.style "font-size: 12px; font-weight: bold;"; "Compare Scenario" }
                                let data = "" :: (model.Scenarios |> List.map (fun s -> s.ScenarioId))
                                let currentVal = model.CompareScenarioId |> Option.defaultValue ""
                                Rz.dropDown(data, currentVal, onChange = (fun v ->
                                    let s = v :?> string
                                    let opt = if String.IsNullOrEmpty(s) then None else Some s
                                    dispatch (SelectCompareScenario opt)
                                ))
                            }
                        }

                        // Side-by-side comparison details
                        match model.ActiveScenarioId, model.CompareScenarioId with
                        | Some activeId, Some compareId when activeId <> compareId ->
                            let activeOpt = model.Scenarios |> List.tryFind (fun s -> s.ScenarioId = activeId)
                            let compareOpt = model.Scenarios |> List.tryFind (fun s -> s.ScenarioId = compareId)
                            
                            match activeOpt, compareOpt with
                            | Some activeSc, Some compareSc ->
                                div {
                                    attr.style "margin-top: 24px; display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 20px;"
                                    
                                    // Base Scenario Details
                                    comp<RadzenCard> {
                                        "Style" => "padding: 16px; border: 1px solid var(--rz-primary-color); background-color: rgba(33, 150, 243, 0.02);"
                                        Rz.stack([
                                            h5 { attr.style "font-weight: bold; margin: 0; color: var(--rz-primary-color);"; sprintf "Active: %s" activeSc.Name }
                                            span { attr.style "font-size: 12px;"; sprintf "Version: %d" activeSc.Version }
                                            span { attr.style "font-size: 12px;"; sprintf "Created: %s" (activeSc.CreatedAt.ToString("g")) }
                                            span { attr.style "font-size: 12px;"; sprintf "Is Active: %b" activeSc.IsActive }
                                            hr { attr.style "margin: 8px 0; border: none; border-top: 1px solid var(--rz-border-color);" }
                                            span { attr.style "font-weight: bold; font-size: 13px;"; sprintf "Overrides (%d)" activeSc.Overrides.Length }
                                            if activeSc.Overrides.IsEmpty then
                                                span { attr.style "font-style: italic; font-size: 12px; color: var(--rz-color-text-secondary);"; "No overrides defined." }
                                            else
                                                ul {
                                                    attr.style "padding-left: 16px; margin: 4px 0; font-size: 12px;"
                                                    for ov in activeSc.Overrides do
                                                        li { renderOverride ov }
                                                }
                                        ], gap = "6px")
                                    }

                                    // Compare Scenario Details
                                    comp<RadzenCard> {
                                        "Style" => "padding: 16px; border: 1px solid var(--rz-secondary-color); background-color: rgba(156, 39, 176, 0.02);"
                                        Rz.stack([
                                            h5 { attr.style "font-weight: bold; margin: 0; color: var(--rz-secondary-color);"; sprintf "Compare: %s" compareSc.Name }
                                            span { attr.style "font-size: 12px;"; sprintf "Version: %d" compareSc.Version }
                                            span { attr.style "font-size: 12px;"; sprintf "Created: %s" (compareSc.CreatedAt.ToString("g")) }
                                            span { attr.style "font-size: 12px;"; sprintf "Is Active: %b" compareSc.IsActive }
                                            hr { attr.style "margin: 8px 0; border: none; border-top: 1px solid var(--rz-border-color);" }
                                            span { attr.style "font-weight: bold; font-size: 13px;"; sprintf "Overrides (%d)" compareSc.Overrides.Length }
                                            if compareSc.Overrides.IsEmpty then
                                                span { attr.style "font-style: italic; font-size: 12px; color: var(--rz-color-text-secondary);"; "No overrides defined." }
                                            else
                                                ul {
                                                    attr.style "padding-left: 16px; margin: 4px 0; font-size: 12px;"
                                                    for ov in compareSc.Overrides do
                                                        li { renderOverride ov }
                                                }
                                        ], gap = "6px")
                                    }
                                }
                            | _ -> empty()
                        | Some activeId, Some compareId when activeId = compareId ->
                            div {
                                attr.style "margin-top: 16px; color: var(--rz-warning-color); font-size: 13px; font-style: italic;"
                                "Active and Compare scenarios are the same. Please choose distinct scenarios to compare."
                            }
                        | _ ->
                            div {
                                attr.style "margin-top: 16px; color: var(--rz-color-text-secondary); font-size: 13px; font-style: italic;"
                                "Select both scenarios above to perform an overlay delta check."
                            }
                    ], gap = "12px")
                }
        }
