namespace Medhavi.Web.Pages

open System
open Bolero
open Bolero.Html
open Medhavi.Web
open Medhavi.Web.Components
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Medhavi.Web.Stores
open Medhavi.Nexus
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
        
        // Form state
        CreateFormOpen: bool
        NewScenarioName: string
        NewScenarioType: ScenarioType
        NewScenarioParentId: string option
    }

    type Msg =
        | LoadScenarios of ScenarioReadModel list
        | ShowError of string
        | SetLoading of bool
        | SelectActiveScenario of string option
        | SelectCompareScenario of string option
        | OpenCreateForm of string option
        | CloseCreateForm
        | UpdateNewName of string
        | UpdateNewType of ScenarioType
        | SubmitCreateScenario

    let init () = 
        { Scenarios = []
          IsLoading = true
          ErrorMessage = None
          ActiveScenarioId = None
          CompareScenarioId = None
          CreateFormOpen = false
          NewScenarioName = ""
          NewScenarioType = ScenarioType.WhatIf
          NewScenarioParentId = None }

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
        | OpenCreateForm parentIdOpt ->
            { model with CreateFormOpen = true; NewScenarioName = ""; NewScenarioType = ScenarioType.WhatIf; NewScenarioParentId = parentIdOpt }
        | CloseCreateForm ->
            { model with CreateFormOpen = false; NewScenarioName = ""; NewScenarioParentId = None }
        | UpdateNewName name ->
            { model with NewScenarioName = name }
        | UpdateNewType t ->
            { model with NewScenarioType = t }
        | SubmitCreateScenario ->
            model // Handled at AppShell level for stores/async actions

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

    let rec renderScenarioTreeItem (node: ScenarioReadModel) (allScenarios: ScenarioReadModel list) (activeScenarioId: string option) (compareScenarioId: string option) dispatch : Node =
        let children = allScenarios |> List.filter (fun s -> s.BaseScenarioId = Some node.ScenarioId)
        let isActive = activeScenarioId = Some node.ScenarioId
        let isCompare = compareScenarioId = Some node.ScenarioId
        
        li {
            attr.style "list-style-type: none; margin: 4px 0; padding-left: 12px; border-left: 1px dashed var(--rz-border-color);"
            
            div {
                attr.style "display: flex; align-items: center; justify-content: space-between; padding: 6px 12px; border-radius: 4px; background-color: rgba(255,255,255,0.02); margin-bottom: 4px; border: 1px solid var(--rz-border-color);"
                
                div {
                    attr.style "display: flex; align-items: center; gap: 8px;"
                    Rz.icon((if children.IsEmpty then "description" else "folder"), style = "font-size: 16px; color: var(--rz-text-secondary-color);")
                    span {
                        attr.style (if isActive then "font-weight: bold; color: var(--rz-primary-light, #3498db);" elif isCompare then "font-weight: bold; color: var(--rz-secondary-color);" else "")
                        node.Name
                    }
                    span {
                        attr.style "font-size: 11px; color: var(--rz-text-secondary-color); font-family: monospace; background-color: rgba(0,0,0,0.2); padding: 2px 4px; border-radius: 2px;"
                        node.ScenarioId
                    }
                    if isActive then
                        span {
                            attr.style "font-size: 11px; color: var(--rz-success-color); font-weight: bold;"
                            "(Active)"
                        }
                    if isCompare then
                        span {
                            attr.style "font-size: 11px; color: var(--rz-secondary-color); font-weight: bold;"
                            "(Comparing)"
                        }
                }
                
                div {
                    attr.style "display: flex; gap: 8px; align-items: center;"
                    
                    // Branch option
                    button {
                        attr.``class`` "theme-trigger-btn"
                        attr.title "Branch from this scenario"
                        attr.style "padding: 4px 8px; border-radius: 4px; font-size: 11px; display: flex; align-items: center; gap: 4px; background-color: rgba(255,255,255,0.05); border: 1px solid var(--rz-border-color); color: var(--rz-text-color); cursor: pointer;"
                        on.click (fun (e: MouseEventArgs) -> dispatch (OpenCreateForm (Some node.ScenarioId)))
                        Rz.icon("call_split", style = "font-size: 14px;")
                        span { "Branch" }
                    }
                    
                    // Activate button
                    if not isActive then
                        button {
                            attr.``class`` "theme-trigger-btn"
                            attr.title "Make active"
                            attr.style "padding: 4px 8px; border-radius: 4px; font-size: 11px; display: flex; align-items: center; gap: 4px; background-color: rgba(255,255,255,0.05); border: 1px solid var(--rz-border-color); color: var(--rz-text-color); cursor: pointer;"
                            on.click (fun (e: MouseEventArgs) -> dispatch (SelectActiveScenario (Some node.ScenarioId)))
                            Rz.icon("check", style = "font-size: 14px;")
                            span { "Activate" }
                        }
                        
                    // Compare button
                    if not isCompare then
                        button {
                            attr.``class`` "theme-trigger-btn"
                            attr.title "Compare scenario"
                            attr.style "padding: 4px 8px; border-radius: 4px; font-size: 11px; display: flex; align-items: center; gap: 4px; background-color: rgba(255,255,255,0.05); border: 1px solid var(--rz-border-color); color: var(--rz-text-color); cursor: pointer;"
                            on.click (fun (e: MouseEventArgs) -> dispatch (SelectCompareScenario (Some node.ScenarioId)))
                            Rz.icon("compare", style = "font-size: 14px;")
                            span { "Compare" }
                        }
                }
            }
            
            if not children.IsEmpty then
                ul {
                    attr.style "padding-left: 12px; margin: 4px 0;"
                    for child in children do
                        renderScenarioTreeItem child allScenarios activeScenarioId compareScenarioId dispatch
                }
        }

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
                div {
                    attr.style "display: grid; grid-template-columns: 350px 1fr; gap: 24px; min-height: 500px;"
                    
                    // Left Panel: Scenario Tree
                    comp<RadzenCard> {
                        "Style" => "padding: 20px; border-radius: 8px; display: flex; flex-direction: column; gap: 16px;"
                        Rz.stack([
                            div {
                                attr.style "display: flex; justify-content: space-between; align-items: center;"
                                h4 { attr.``class`` "rz-text-h6 rz-m-0"; "Scenarios Tree 🌳" }
                                comp<RadzenButton> {
                                    "Icon" => "add"
                                    "Text" => "Create"
                                    "ButtonStyle" => ButtonStyle.Primary
                                    "Size" => ButtonSize.Small
                                    attr.callback "Click" (fun (e: MouseEventArgs) -> dispatch (OpenCreateForm None))
                                }
                            }
                            
                            ul {
                                attr.style "padding: 0; margin: 0; overflow-y: auto; max-height: 450px;"
                                let roots =
                                    model.Scenarios
                                    |> List.filter (fun s ->
                                        match s.BaseScenarioId with
                                        | None -> true
                                        | Some bid -> not (model.Scenarios |> List.exists (fun x -> x.ScenarioId = bid))
                                    )
                                if roots.IsEmpty then
                                    li { attr.style "list-style-type: none; font-style: italic; color: var(--rz-text-secondary-color);"; "No scenarios available." }
                                else
                                    for r in roots do
                                        renderScenarioTreeItem r model.Scenarios model.ActiveScenarioId model.CompareScenarioId dispatch
                            }
                        ], gap = "12px")
                    }
                    
                    // Right Panel: Catalog & Comparison
                    div {
                        attr.style "display: flex; flex-direction: column; gap: 24px;"
                        
                        // Scenarios list grid
                        comp<RadzenCard> {
                            "Style" => "padding: 20px; border-radius: 8px;"
                            Rz.stack([
                                h4 { attr.``class`` "rz-text-h6 rz-m-0"; "Scenarios Catalog 📋" }
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
                            ], gap = "12px")
                        }

                        // Scenario Comparison setup
                        comp<RadzenCard> {
                            "Style" => "padding: 20px; border-radius: 8px;"
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
                }
            
            // Create Scenario Form Overlay
            if model.CreateFormOpen then
                div {
                    attr.``class`` "rz-dialog-mask"
                    attr.style "position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; z-index: 10000; background-color: rgba(0,0,0,0.6); display: flex; align-items: center; justify-content: center;"
                    on.click (fun (e: MouseEventArgs) -> dispatch CloseCreateForm)
                    
                    div {
                        attr.style "width: 450px; background-color: var(--rz-dialog-background-color, #202b38); border: 1px solid var(--rz-border-color); border-radius: 8px; box-shadow: 0 10px 25px rgba(0,0,0,0.5); padding: 20px; display: flex; flex-direction: column; gap: 16px;"
                        on.stopPropagation "click" true
                        
                        div {
                            attr.style "display: flex; align-items: center; justify-content: space-between; border-bottom: 1px solid var(--rz-border-color); padding-bottom: 12px;"
                            h3 { attr.style "margin: 0; font-size: 16px; font-weight: bold; font-family: var(--rz-font-family); color: var(--rz-header-color, #ffffff);"; 
                                 match model.NewScenarioParentId with
                                 | Some pid -> sprintf "Branch Scenario from %s" pid
                                 | None -> "Create New Scenario"
                            }
                            button {
                                attr.style "background: transparent; border: none; cursor: pointer; color: var(--rz-text-secondary-color);"
                                on.click (fun (e: MouseEventArgs) -> dispatch CloseCreateForm)
                                Rz.icon("close")
                            }
                        }
                        
                        // Name input
                        div {
                            attr.style "display: flex; flex-direction: column; gap: 6px;"
                            label { attr.style "font-size: 13px; font-weight: 500; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"; "Scenario Name" }
                            input {
                                attr.``class`` "rz-textbox"
                                attr.style "width: 100%; padding: 8px; background-color: var(--rz-header-background-color, #2b3a4a); border: 1px solid var(--rz-border-color); color: var(--rz-text-color); border-radius: 4px;"
                                attr.value model.NewScenarioName
                                on.input (fun (e: ChangeEventArgs) -> dispatch (UpdateNewName (string e.Value)))
                            }
                        }
                        
                        // Type selector
                        div {
                            attr.style "display: flex; flex-direction: column; gap: 6px;"
                            label { attr.style "font-size: 13px; font-weight: 500; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"; "Scenario Type" }
                            select {
                                attr.``class`` "rz-dropdown"
                                attr.style "width: 100%; padding: 8px; background-color: var(--rz-header-background-color, #2b3a4a); border: 1px solid var(--rz-border-color); color: var(--rz-text-color); border-radius: 4px;"
                                on.change (fun (e: ChangeEventArgs) ->
                                    let t =
                                        match string e.Value with
                                        | "WhatIf" -> ScenarioType.WhatIf
                                        | "Sandbox" -> ScenarioType.Sandbox
                                        | _ -> ScenarioType.WhatIf
                                    dispatch (UpdateNewType t))
                                
                                option { attr.value "WhatIf"; "WhatIf" }
                                option { attr.value "Sandbox"; "Sandbox" }
                            }
                        }
                        
                        // Buttons
                        div {
                            attr.style "display: flex; justify-content: end; gap: 12px; border-top: 1px solid var(--rz-border-color); padding-top: 12px; margin-top: 8px;"
                            comp<RadzenButton> {
                                "Text" => "Cancel"
                                "ButtonStyle" => ButtonStyle.Secondary
                                attr.callback "Click" (fun (e: MouseEventArgs) -> dispatch CloseCreateForm)
                            }
                            comp<RadzenButton> {
                                "Text" => "Create"
                                "ButtonStyle" => ButtonStyle.Primary
                                "Disabled" => String.IsNullOrWhiteSpace(model.NewScenarioName)
                                attr.callback "Click" (fun (e: MouseEventArgs) -> dispatch SubmitCreateScenario)
                            }
                        }
                    }
                }
        }
