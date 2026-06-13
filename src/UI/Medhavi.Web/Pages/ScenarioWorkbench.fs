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
        
        // Lifecycle form state
        PublishFormOpen: bool
        PublishReason: string
        RejectFormOpen: bool
        RejectReason: string
        RejectingScenarioId: string option
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
        | RemoveOverride of ScenarioDataOverride
        | SubmitForApproval
        | ApproveScenario
        | OpenRejectForm of string
        | CloseRejectForm
        | UpdateRejectReason of string
        | SubmitRejectScenario
        | OpenPublishForm
        | ClosePublishForm
        | UpdatePublishReason of string
        | SubmitPublishScenario
        | RollbackScenario of string

    let init () = 
        { Scenarios = []
          IsLoading = true
          ErrorMessage = None
          ActiveScenarioId = None
          CompareScenarioId = None
          CreateFormOpen = false
          NewScenarioName = ""
          NewScenarioType = ScenarioType.WhatIf
          NewScenarioParentId = None
          PublishFormOpen = false
          PublishReason = ""
          RejectFormOpen = false
          RejectReason = ""
          RejectingScenarioId = None }

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
        | SubmitCreateScenario
        | RemoveOverride _
        | SubmitForApproval
        | ApproveScenario
        | SubmitRejectScenario
        | SubmitPublishScenario
        | RollbackScenario _ ->
            model // Handled at AppShell level for stores/async actions
        | OpenRejectForm scenId ->
            { model with RejectFormOpen = true; RejectReason = ""; RejectingScenarioId = Some scenId }
        | CloseRejectForm ->
            { model with RejectFormOpen = false; RejectReason = ""; RejectingScenarioId = None }
        | UpdateRejectReason reason ->
            { model with RejectReason = reason }
        | OpenPublishForm ->
            { model with PublishFormOpen = true; PublishReason = "" }
        | ClosePublishForm ->
            { model with PublishFormOpen = false; PublishReason = "" }
        | UpdatePublishReason reason ->
            { model with PublishReason = reason }

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

        // Supplier Deltas
        | SupplierReactivation(supplierId, reason) ->
            sprintf "Supplier Reactivation '%s': Reactivated (%s)" supplierId reason
        | SupplierLeadTimeOverride(supplierId, skuIdOpt, overrideLeadTimeDays, reason) ->
            let skuStr = match skuIdOpt with Some s -> sprintf " for SKU '%s'" s | None -> ""
            sprintf "Supplier '%s' Lead Time%s: Override to %d days (%s)" supplierId skuStr overrideLeadTimeDays reason
        | SupplierCapacityOverride(supplierId, skuIdOpt, overrideQty, reason) ->
            let skuStr = match skuIdOpt with Some s -> sprintf " for SKU '%s'" s | None -> ""
            sprintf "Supplier '%s' Capacity%s: Override to %M (%s)" supplierId skuStr overrideQty reason
        | SupplierPriceOverride(supplierId, skuIdOpt, overridePrice, reason) ->
            let skuStr = match skuIdOpt with Some s -> sprintf " for SKU '%s'" s | None -> ""
            sprintf "Supplier '%s' Price%s: Override to %M (%s)" supplierId skuStr overridePrice reason

        // BOM Deltas
        | BomAlternateSelection(parentProductId, alternateBomId, reason) ->
            sprintf "BOM parent '%s': Selected alternate BOM '%s' (%s)" parentProductId alternateBomId reason
        | BomComponentAddition(parentProductId, componentProductId, qtyPer, reason) ->
            sprintf "BOM parent '%s': Added component '%s' with Qty-per %M (%s)" parentProductId componentProductId qtyPer reason
        | BomComponentRemoval(parentProductId, componentProductId, reason) ->
            sprintf "BOM parent '%s': Removed component '%s' (%s)" parentProductId componentProductId reason

        // Policy Deltas
        | KpiWeightOverride(kpiId, overrideWeight) ->
            sprintf "KPI Weight Policy '%s': Override weight to %M" kpiId overrideWeight
        | ServiceLevelTargetOverride(targetId, overrideValue) ->
            sprintf "Service Level Target Policy '%s': Override target to %M" targetId overrideValue
        | CostRiskTradeoffOverride(policyId, overrideValue) ->
            sprintf "Cost Risk Tradeoff Policy '%s': Override value to %M" policyId overrideValue
        | CarbonWeightOverride(policyId, overrideWeight) ->
            sprintf "Carbon Weight Policy '%s': Override weight to %M" policyId overrideWeight
        | FreezePolicyOverride(policyId, isEnabled) ->
            sprintf "Freeze Policy '%s': Override state to %s" policyId (if isEnabled then "Enabled" else "Disabled")
        | ApprovalThresholdOverride(policyId, overrideThreshold) ->
            sprintf "Approval Threshold Policy '%s': Override threshold to %M" policyId overrideThreshold

        // Knowledge Deltas
        | TagAddedOverride(conceptId, tag, reason) ->
            sprintf "Knowledge concept '%s': Added tag '%s' (%s)" conceptId tag reason
        | AnnotationAddedOverride(conceptId, note, reason) ->
            sprintf "Knowledge concept '%s': Added annotation '%s' (%s)" conceptId note reason
        | RelationHintAddedOverride(subjectId, relation, objectId, reason) ->
            sprintf "Knowledge relation: '%s' %s '%s' (%s)" subjectId relation objectId reason

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
                    
                    // Right Panel: Catalog & Active Details & Comparison
                    div {
                        attr.style "display: flex; flex-direction: column; gap: 24px;"
                        
                        // Active Scenario Workbench & Inspector
                        match model.ActiveScenarioId with
                        | Some activeId when not (activeId.Equals("baseline", StringComparison.OrdinalIgnoreCase)) ->
                            let activeScOpt = model.Scenarios |> List.tryFind (fun s -> s.ScenarioId = activeId)
                            match activeScOpt with
                            | Some activeSc ->
                                let isPublished =
                                    match activeSc.Status with
                                    | ScenarioStatus.Published _ -> true
                                    | _ -> false
                                comp<RadzenCard> {
                                    "Style" => "padding: 20px; border-radius: 8px; border: 1px solid var(--rz-info-color);"
                                    Rz.stack([
                                        // Header Row with Status Badge
                                        div {
                                            attr.style "display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid var(--rz-border-color); padding-bottom: 12px;"
                                            Rz.stack([
                                                h4 { attr.style "font-weight: bold; margin: 0; color: var(--rz-info-color); font-family: var(--rz-font-family);"; sprintf "Active Scenario: %s" activeSc.Name }
                                                span { attr.style "font-size: 11px; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"; sprintf "ID: %s | Version: %d" activeSc.ScenarioId activeSc.Version }
                                            ], gap = "4px")
                                            
                                            let badgeClass, badgeText =
                                                match activeSc.Status with
                                                | ScenarioStatus.Draft -> "rz-badge rz-badge-secondary", "Draft"
                                                | ScenarioStatus.UnderReview -> "rz-badge rz-badge-warning", "Under Review"
                                                | ScenarioStatus.Approved -> "rz-badge rz-badge-success", "Approved"
                                                | ScenarioStatus.Published(pubDate, _, _) -> "rz-badge rz-badge-info", sprintf "Published (%s)" (pubDate.ToString("yyyy-MM-dd"))
                                                | ScenarioStatus.Archived -> "rz-badge rz-badge-info", "Archived"
                                                | ScenarioStatus.PlanningRunning -> "rz-badge rz-badge-warning", "Running Solver"
                                                | ScenarioStatus.PlanningComplete -> "rz-badge rz-badge-success", "Plan Complete"
                                                | ScenarioStatus.PlanningFailed -> "rz-badge rz-badge-danger", "Plan Failed"
                                                | _ -> "rz-badge rz-badge-light", sprintf "%A" activeSc.Status
                                            span { attr.``class`` badgeClass; attr.style "font-size: 12px; padding: 4px 8px; border-radius: 4px; font-family: var(--rz-font-family); font-weight: bold;"; badgeText }
                                        }
                                        
                                        // Workflow Controls Panel
                                        div {
                                            attr.style "display: flex; gap: 12px; flex-wrap: wrap; align-items: center; padding: 12px; background-color: rgba(255,255,255,0.02); border-radius: 4px; border: 1px solid var(--rz-border-color);"
                                            
                                            if activeSc.Status = ScenarioStatus.Draft then
                                                comp<RadzenButton> {
                                                    "Text" => "Submit for Approval"
                                                    "Icon" => "send"
                                                    "ButtonStyle" => ButtonStyle.Secondary
                                                    "Size" => ButtonSize.Small
                                                    attr.callback "Click" (fun (e: MouseEventArgs) -> dispatch SubmitForApproval)
                                                }
                                            
                                            if activeSc.Status = ScenarioStatus.UnderReview then
                                                comp<RadzenButton> {
                                                    "Text" => "Approve"
                                                    "Icon" => "done"
                                                    "ButtonStyle" => ButtonStyle.Success
                                                    "Size" => ButtonSize.Small
                                                    "Disabled" => not canRun
                                                    attr.callback "Click" (fun (e: MouseEventArgs) -> dispatch ApproveScenario)
                                                }
                                                comp<RadzenButton> {
                                                    "Text" => "Reject"
                                                    "Icon" => "close"
                                                    "ButtonStyle" => ButtonStyle.Danger
                                                    "Size" => ButtonSize.Small
                                                    "Disabled" => not canRun
                                                    attr.callback "Click" (fun (e: MouseEventArgs) -> dispatch (OpenRejectForm activeSc.ScenarioId))
                                                }
                                                
                                            if activeSc.Status = ScenarioStatus.Approved then
                                                comp<RadzenButton> {
                                                    "Text" => "Publish Merge to Baseline"
                                                    "Icon" => "publish"
                                                    "ButtonStyle" => ButtonStyle.Info
                                                    "Size" => ButtonSize.Small
                                                    "Disabled" => not canRun
                                                    attr.callback "Click" (fun (e: MouseEventArgs) -> dispatch OpenPublishForm)
                                                }
                                                
                                            if (activeSc.Status = ScenarioStatus.Archived || isPublished) && activeSc.PublishId.IsSome then
                                                comp<RadzenButton> {
                                                    "Text" => "Rollback Merge"
                                                    "Icon" => "restore"
                                                    "ButtonStyle" => ButtonStyle.Danger
                                                    "Size" => ButtonSize.Small
                                                    "Disabled" => not canRun
                                                    attr.callback "Click" (fun (e: MouseEventArgs) -> dispatch (RollbackScenario activeSc.PublishId.Value))
                                                }
                                                span {
                                                    attr.style "font-size: 11px; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"
                                                    sprintf "Merged into baseline via Publish ID: %s" activeSc.PublishId.Value
                                                }
                                                
                                            if not canRun && (activeSc.Status = ScenarioStatus.UnderReview || activeSc.Status = ScenarioStatus.Approved || ((activeSc.Status = ScenarioStatus.Archived || isPublished) && activeSc.PublishId.IsSome)) then
                                                span {
                                                    attr.style "font-size: 11px; color: var(--rz-warning-color); font-style: italic; font-family: var(--rz-font-family);"
                                                    "ℹ️ Only Supervisor, Manager, or Admin can execute these workflow actions."
                                                }
                                        }
                                        
                                        // KPI Metrics Summary Dashboard
                                        div {
                                            attr.style "display: flex; flex-direction: column; gap: 8px;"
                                            h5 { attr.style "font-weight: bold; margin: 0; color: var(--rz-primary-color); font-family: var(--rz-font-family);"; "KPI Metrics Dashboard 📊" }
                                            match activeSc.KpiSummary with
                                            | None ->
                                                span { attr.style "font-style: italic; font-size: 12px; color: var(--rz-color-text-secondary); font-family: var(--rz-font-family);"; "No plan summary available. Run MRP solver on this scenario to generate KPIs." }
                                            | Some kpi ->
                                                div {
                                                    attr.style "display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 12px;"
                                                    
                                                    comp<RadzenCard> {
                                                        "Style" => "padding: 12px; text-align: center; border-left: 4px solid var(--rz-success-color);"
                                                        span { attr.style "font-size: 11px; color: var(--rz-text-secondary-color); font-weight: bold; text-transform: uppercase; font-family: var(--rz-font-family);"; "Service Level" }
                                                        h4 { attr.style "margin: 8px 0 0 0; font-weight: bold; color: var(--rz-success-color); font-family: var(--rz-font-family);"; sprintf "%.1f%%" (kpi.ServiceLevel * 100.0) }
                                                    }
                                                    
                                                    comp<RadzenCard> {
                                                        "Style" => "padding: 12px; text-align: center; border-left: 4px solid var(--rz-danger-color);"
                                                        span { attr.style "font-size: 11px; color: var(--rz-text-secondary-color); font-weight: bold; text-transform: uppercase; font-family: var(--rz-font-family);"; "Total Cost" }
                                                        h4 { attr.style "margin: 8px 0 0 0; font-weight: bold; color: var(--rz-danger-color); font-family: var(--rz-font-family);"; sprintf "$%M" kpi.TotalCost }
                                                    }
                                                    
                                                    comp<RadzenCard> {
                                                        "Style" => "padding: 12px; text-align: center; border-left: 4px solid var(--rz-warning-color);"
                                                        span { attr.style "font-size: 11px; color: var(--rz-text-secondary-color); font-weight: bold; text-transform: uppercase; font-family: var(--rz-font-family);"; "Plan Churn" }
                                                        h4 { attr.style "margin: 8px 0 0 0; font-weight: bold; color: var(--rz-warning-color); font-family: var(--rz-font-family);"; sprintf "%.1f%%" (kpi.PlanChurn * 100.0) }
                                                    }
                                                    
                                                    comp<RadzenCard> {
                                                        "Style" => "padding: 12px; text-align: center; border-left: 4px solid var(--rz-warning-color);"
                                                        span { attr.style "font-size: 11px; color: var(--rz-text-secondary-color); font-weight: bold; text-transform: uppercase; font-family: var(--rz-font-family);"; "Shortages" }
                                                        h4 { attr.style "margin: 8px 0 0 0; font-weight: bold; color: var(--rz-warning-color); font-family: var(--rz-font-family);"; sprintf "%d" kpi.ShortageCount }
                                                    }
                                                }
                                        }
                                        
                                        // Delta Overrides Inspector
                                        div {
                                            attr.style "display: flex; flex-direction: column; gap: 8px; margin-top: 8px;"
                                            h5 { attr.style "font-weight: bold; margin: 0; color: var(--rz-primary-color); font-family: var(--rz-font-family);"; sprintf "Delta Overrides Inspector (%d)" activeSc.Overrides.Length }
                                            if activeSc.Overrides.IsEmpty then
                                                span { attr.style "font-style: italic; font-size: 12px; color: var(--rz-color-text-secondary); font-family: var(--rz-font-family);"; "No sandbox overrides defined. Apply overrides in the Demand Workbench or other sheets." }
                                            else
                                                div {
                                                    attr.style "max-height: 200px; overflow-y: auto; border: 1px solid var(--rz-border-color); border-radius: 4px;"
                                                    table {
                                                        attr.style "width: 100%; border-collapse: collapse;"
                                                        thead {
                                                            tr {
                                                                attr.style "background-color: rgba(255,255,255,0.05); text-align: left; border-bottom: 1px solid var(--rz-border-color);"
                                                                th { attr.style "padding: 8px 12px; font-size: 11px; font-weight: bold; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"; "Override Details" }
                                                                th { attr.style "padding: 8px 12px; font-size: 11px; font-weight: bold; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family); width: 60px; text-align: center;"; "Action" }
                                                            }
                                                        }
                                                        tbody {
                                                            for ov in activeSc.Overrides do
                                                                tr {
                                                                    attr.style "border-bottom: 1px solid var(--rz-border-color);"
                                                                    td { attr.style "padding: 8px 12px; font-size: 12px; color: var(--rz-text-color); font-family: var(--rz-font-family);"; renderOverride ov }
                                                                    td {
                                                                        attr.style "padding: 8px 12px; text-align: center;"
                                                                        comp<RadzenButton> {
                                                                            "Icon" => "delete"
                                                                            "ButtonStyle" => ButtonStyle.Danger
                                                                            "Size" => ButtonSize.Small
                                                                            "Disabled" => (activeSc.Status = ScenarioStatus.Archived || isPublished)
                                                                            attr.callback "Click" (fun (e: MouseEventArgs) -> dispatch (RemoveOverride ov))
                                                                        }
                                                                    }
                                                                }
                                                        }
                                                    }
                                                }
                                        }
                                    ], gap = "16px")
                                }
                            | _ -> empty()
                        | _ -> empty()
 
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
                                        Rz.stack([
                                            div {
                                                attr.style "margin-top: 24px; display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 20px;"
                                                
                                                // Base Scenario Details
                                                comp<RadzenCard> {
                                                    "Style" => "padding: 16px; border: 1px solid var(--rz-primary-color); background-color: rgba(33, 150, 243, 0.02);"
                                                    Rz.stack([
                                                        h5 { attr.style "font-weight: bold; margin: 0; color: var(--rz-primary-color);"; sprintf "Active: %s" activeSc.Name }
                                                        span { attr.style "font-size: 12px;"; sprintf "Version: %d" activeSc.Version }
                                                        span { attr.style "font-size: 12px;"; sprintf "Created: %s" (activeSc.CreatedAt.ToString("g")) }
                                                        span { attr.style "font-size: 12px;"; sprintf "Status: %A" activeSc.Status }
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
                                                        span { attr.style "font-size: 12px;"; sprintf "Status: %A" compareSc.Status }
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

                                            // Side-by-side KPI comparison grid
                                            div {
                                                attr.style "margin-top: 24px; border: 1px solid var(--rz-border-color); border-radius: 8px; padding: 16px; background-color: rgba(255,255,255,0.01);"
                                                h5 { attr.style "font-weight: bold; margin: 0 0 12px 0; color: var(--rz-primary-color); font-family: var(--rz-font-family);"; "KPI Metrics Comparison Grid 📊" }
                                                
                                                match activeSc.KpiSummary, compareSc.KpiSummary with
                                                | Some actKpi, Some compKpi ->
                                                    table {
                                                        attr.style "width: 100%; border-collapse: collapse;"
                                                        thead {
                                                            tr {
                                                                attr.style "background-color: rgba(255,255,255,0.05); text-align: left; border-bottom: 1px solid var(--rz-border-color);"
                                                                th { attr.style "padding: 10px 12px; font-size: 12px; font-weight: bold; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"; "Metric" }
                                                                th { attr.style "padding: 10px 12px; font-size: 12px; font-weight: bold; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"; activeSc.Name }
                                                                th { attr.style "padding: 10px 12px; font-size: 12px; font-weight: bold; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"; compareSc.Name }
                                                                th { attr.style "padding: 10px 12px; font-size: 12px; font-weight: bold; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"; "Delta" }
                                                            }
                                                        }
                                                        tbody {
                                                            let rows = [
                                                                ("Service Level", sprintf "%.1f%%" (actKpi.ServiceLevel * 100.0), sprintf "%.1f%%" (compKpi.ServiceLevel * 100.0), sprintf "%.1f%%" ((compKpi.ServiceLevel - actKpi.ServiceLevel) * 100.0))
                                                                ("On-Time Delivery", sprintf "%.1f%%" (actKpi.OnTimeDelivery * 100.0), sprintf "%.1f%%" (compKpi.OnTimeDelivery * 100.0), sprintf "%.1f%%" ((compKpi.OnTimeDelivery - actKpi.OnTimeDelivery) * 100.0))
                                                                ("Total Cost", sprintf "$%M" actKpi.TotalCost, sprintf "$%M" compKpi.TotalCost, sprintf "$%M" (compKpi.TotalCost - actKpi.TotalCost))
                                                                ("Inventory Carrying Cost", sprintf "$%M" actKpi.InventoryCarryingCost, sprintf "$%M" compKpi.InventoryCarryingCost, sprintf "$%M" (compKpi.InventoryCarryingCost - actKpi.InventoryCarryingCost))
                                                                ("Plan Churn", sprintf "%.1f%%" (actKpi.PlanChurn * 100.0), sprintf "%.1f%%" (compKpi.PlanChurn * 100.0), sprintf "%.1f%%" ((compKpi.PlanChurn - actKpi.PlanChurn) * 100.0))
                                                                ("Average Tardiness", sprintf "%.1f days" actKpi.AverageTardiness, sprintf "%.1f days" compKpi.AverageTardiness, sprintf "%.1f days" (compKpi.AverageTardiness - actKpi.AverageTardiness))
                                                                ("Shortages Count", sprintf "%d" actKpi.ShortageCount, sprintf "%d" compKpi.ShortageCount, sprintf "%d" (compKpi.ShortageCount - actKpi.ShortageCount))
                                                                ("CO2 Emissions", sprintf "%M kg" actKpi.CO2Emissions, sprintf "%M kg" compKpi.CO2Emissions, sprintf "%M kg" (compKpi.CO2Emissions - actKpi.CO2Emissions))
                                                            ]
                                                            for (label, actVal, compVal, deltaVal) in rows do
                                                                tr {
                                                                    attr.style "border-bottom: 1px solid var(--rz-border-color);"
                                                                    td { attr.style "padding: 10px 12px; font-size: 12px; font-weight: 500; color: var(--rz-text-color); font-family: var(--rz-font-family);"; label }
                                                                    td { attr.style "padding: 10px 12px; font-size: 12px; color: var(--rz-text-color); font-family: var(--rz-font-family);"; actVal }
                                                                    td { attr.style "padding: 10px 12px; font-size: 12px; color: var(--rz-text-color); font-family: var(--rz-font-family);"; compVal }
                                                                    td { attr.style "padding: 10px 12px; font-size: 12px; font-weight: bold; color: var(--rz-info-color); font-family: var(--rz-font-family);"; deltaVal }
                                                                }
                                                        }
                                                    }
                                                | _ ->
                                                    span { attr.style "font-style: italic; font-size: 12px; color: var(--rz-color-text-secondary); font-family: var(--rz-font-family);"; "Both scenarios must have Plan KPI summaries to show the side-by-side comparison grid." }
                                            }
                                        ], gap = "12px")
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
            
            // Create Scenario Form Overlay (Opaque solid background)
            if model.CreateFormOpen then
                div {
                    attr.``class`` "rz-dialog-mask"
                    attr.style "position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; z-index: 10000; background-color: #111a24; display: flex; align-items: center; justify-content: center;"
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

            // Publish Scenario Form Overlay (Opaque solid background)
            if model.PublishFormOpen then
                div {
                    attr.``class`` "rz-dialog-mask"
                    attr.style "position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; z-index: 10000; background-color: #111a24; display: flex; align-items: center; justify-content: center;"
                    on.click (fun _ -> dispatch ClosePublishForm)
                    
                    div {
                        attr.style "width: 450px; background-color: var(--rz-dialog-background-color, #202b38); border: 1px solid var(--rz-border-color); border-radius: 8px; box-shadow: 0 10px 25px rgba(0,0,0,0.5); padding: 20px; display: flex; flex-direction: column; gap: 16px;"
                        on.stopPropagation "click" true
                        
                        div {
                            attr.style "display: flex; align-items: center; justify-content: space-between; border-bottom: 1px solid var(--rz-border-color); padding-bottom: 12px;"
                            h3 { attr.style "margin: 0; font-size: 16px; font-weight: bold; font-family: var(--rz-font-family); color: var(--rz-header-color, #ffffff);"; "Publish Scenario to Baseline" }
                            button {
                                attr.style "background: transparent; border: none; cursor: pointer; color: var(--rz-text-secondary-color);"
                                on.click (fun _ -> dispatch ClosePublishForm)
                                Rz.icon("close")
                            }
                        }
                        
                        div {
                            attr.style "font-size: 13px; color: var(--rz-text-secondary-color); line-height: 1.4; font-family: var(--rz-font-family);"
                            "Publishing this scenario will permanently merge its sandbox overrides into the baseline live data. An inverse rollback patch will be logged to the immutable ledger."
                        }
                        
                        // Reason input
                        div {
                            attr.style "display: flex; flex-direction: column; gap: 6px;"
                            label { attr.style "font-size: 13px; font-weight: 500; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"; "Publish Reason / Note" }
                            input {
                                attr.``class`` "rz-textbox"
                                attr.style "width: 100%; padding: 8px; background-color: var(--rz-header-background-color, #2b3a4a); border: 1px solid var(--rz-border-color); color: var(--rz-text-color); border-radius: 4px;"
                                attr.value model.PublishReason
                                on.input (fun (e: ChangeEventArgs) -> dispatch (UpdatePublishReason (string e.Value)))
                            }
                        }
                        
                        // Buttons
                        div {
                            attr.style "display: flex; justify-content: end; gap: 12px; border-top: 1px solid var(--rz-border-color); padding-top: 12px; margin-top: 8px;"
                            comp<RadzenButton> {
                                "Text" => "Cancel"
                                "ButtonStyle" => ButtonStyle.Secondary
                                attr.callback "Click" (fun (e: MouseEventArgs) -> dispatch ClosePublishForm)
                            }
                            comp<RadzenButton> {
                                "Text" => "Confirm & Publish"
                                "ButtonStyle" => ButtonStyle.Info
                                attr.callback "Click" (fun (e: MouseEventArgs) -> dispatch SubmitPublishScenario)
                            }
                        }
                    }
                }

            // Reject Scenario Form Overlay (Opaque solid background)
            if model.RejectFormOpen then
                div {
                    attr.``class`` "rz-dialog-mask"
                    attr.style "position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; z-index: 10000; background-color: #111a24; display: flex; align-items: center; justify-content: center;"
                    on.click (fun _ -> dispatch CloseRejectForm)
                    
                    div {
                        attr.style "width: 450px; background-color: var(--rz-dialog-background-color, #202b38); border: 1px solid var(--rz-border-color); border-radius: 8px; box-shadow: 0 10px 25px rgba(0,0,0,0.5); padding: 20px; display: flex; flex-direction: column; gap: 16px;"
                        on.stopPropagation "click" true
                        
                        div {
                            attr.style "display: flex; align-items: center; justify-content: space-between; border-bottom: 1px solid var(--rz-border-color); padding-bottom: 12px;"
                            h3 { attr.style "margin: 0; font-size: 16px; font-weight: bold; font-family: var(--rz-font-family); color: var(--rz-header-color, #ffffff);"; "Reject Scenario" }
                            button {
                                attr.style "background: transparent; border: none; cursor: pointer; color: var(--rz-text-secondary-color);"
                                on.click (fun _ -> dispatch CloseRejectForm)
                                Rz.icon("close")
                            }
                        }
                        
                        // Reason input
                        div {
                            attr.style "display: flex; flex-direction: column; gap: 6px;"
                            label { attr.style "font-size: 13px; font-weight: 500; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"; "Rejection Reason (Required)" }
                            input {
                                attr.``class`` "rz-textbox"
                                attr.style "width: 100%; padding: 8px; background-color: var(--rz-header-background-color, #2b3a4a); border: 1px solid var(--rz-border-color); color: var(--rz-text-color); border-radius: 4px;"
                                attr.value model.RejectReason
                                on.input (fun (e: ChangeEventArgs) -> dispatch (UpdateRejectReason (string e.Value)))
                            }
                        }
                        
                        // Buttons
                        div {
                            attr.style "display: flex; justify-content: end; gap: 12px; border-top: 1px solid var(--rz-border-color); padding-top: 12px; margin-top: 8px;"
                            comp<RadzenButton> {
                                "Text" => "Cancel"
                                "ButtonStyle" => ButtonStyle.Secondary
                                attr.callback "Click" (fun (e: MouseEventArgs) -> dispatch CloseRejectForm)
                            }
                            comp<RadzenButton> {
                                "Text" => "Reject Scenario"
                                "ButtonStyle" => ButtonStyle.Danger
                                "Disabled" => String.IsNullOrWhiteSpace(model.RejectReason)
                                attr.callback "Click" (fun (e: MouseEventArgs) -> dispatch SubmitRejectScenario)
                            }
                        }
                    }
                }
        }
