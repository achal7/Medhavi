module Medhavi.Client.Shells.UserShell

open System
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Elmish
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Medhavi.Client
open Medhavi.Client.Controls
open Medhavi.Contracts.Client
open Medhavi.Client.Components
open Medhavi.Client.Core.Demand
open Medhavi.Client.Supply.Capacity
open Medhavi.Client.Supply.SupplyOrders

module SupplyActions = SupplyOrderGridSchema
module CapacityActions = CapacityGridSchema
open Medhavi.Client.Components.SettingsDialog
open Medhavi.Client.ViewComposition.View
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.Primitives
open Medhavi.Client.ViewComposition.Session
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ActionExecution

type BreadcrumbItem =
    { Label: string
      Icon: string option
      IsActive: bool }

type SaveViewModalState =
    { ViewName: string
      Category: string
      Icon: string
      IsSaveAs: bool }

type Model =
    { ApplicationBar: ApplicationBar.Model
      Navigation: NavigationBar.Model
      ViewSpace: ViewSpace.Model
      SidePanel: SidePanel.Model
      CommandPalette: CommandPalette.Model
      SavedViews: ViewDefinition list
      SaveViewModal: SaveViewModalState option
      SettingsModal: SettingsDialog.Model option
      Appearance: AppearancePreferences
      Cultural: CulturalPreferences
      Planning: PlanningContext
      Breadcrumbs: BreadcrumbItem list }

type Msg =
    | ApplicationBarMsg of ApplicationBar.Msg
    | NavigationMsg of NavigationBar.Msg
    | ViewSpaceMsg of ViewSpace.Msg
    | SidePanelMsg of SidePanel.Msg
    | CommandPaletteMsg of CommandPalette.Msg

    | NavigateToView of ViewDefId
    | NavigationLoaded of ViewDefinition list
    | NavigationLoadFailed of string

    | BreadcrumbClicked of int

    | SetScenario of ScenarioId
    | SetHorizon of DateRange
    | SetPlant of string option

    | SaveActiveTab
    | SaveActiveTabAs
    | OpenSaveViewModal of isSaveAs: bool
    | UpdateSaveViewModalName of string
    | UpdateSaveViewModalCategory of string
    | UpdateSaveViewModalIcon of string
    | ConfirmSaveViewModal
    | CloseSaveViewModal
    | RevertActiveTab
    | CreateBlankWorkspace
    | ToggleActiveTabEditMode

    | OpenSettingsModal
    | CloseSettingsModal
    | SettingsDialogMsg of SettingsDialog.Msg

    | ToggleCommandPalette

    | ActionExecutionCompleted of Result<ActionExecutionOutcome, string>

[<RequireQualifiedAccess>]
type Output =
    | Notify of Services.NotificationRequest
    | ChangeTheme of VisualTheme
    | ChangeCulture of CulturalPreferences
    | Logout

let private initialBreadcrumbs =
    [ { Label = "Home"
        Icon = Some "home"
        IsActive = true } ]

let private pushBreadcrumb
    (breadcrumbs: BreadcrumbItem list)
    (label: string)
    (icon: string option)
    : BreadcrumbItem list =

    let deactivated = breadcrumbs |> List.map(fun item -> { item with IsActive = false })

    match List.tryLast deactivated with
    | Some last when last.Label = label ->
        let withoutLast = deactivated |> List.take(deactivated.Length - 1)
        withoutLast @ [ { last with IsActive = true } ]
    | _ ->
        deactivated
        @ [ { Label = label
              Icon = icon
              IsActive = true } ]

let init ctx : Model * Cmd<Msg> =
    let planning = ctx.PlanningContextService.Get()
    let initialViews = ViewCatalog.defaultViews

    let applicationBarModel, applicationBarCmd = ApplicationBar.init planning
    let navigationModel, navigationCmd = NavigationBar.init ctx.ComponentRegistry initialViews
    let viewSpaceModel, viewSpaceCmd = ViewSpace.init()
    let sidePanelModel, sidePanelCmd = SidePanel.init()
    let commandPaletteModel, commandPaletteCmd = CommandPalette.init()

    let initialViewCmd =
        Cmd.ofMsg(
            ViewSpaceMsg(ViewSpace.Msg.Request(ViewSpace.OpenViewDefinition ViewCatalog.executiveKpiDashboardView))
        )

    { ApplicationBar = applicationBarModel
      Navigation = navigationModel
      ViewSpace = viewSpaceModel
      SidePanel = sidePanelModel
      CommandPalette = commandPaletteModel
      SavedViews = initialViews
      SaveViewModal = None
      SettingsModal = None
      Appearance = Formatting.defaultAppearancePreferences
      Cultural = Formatting.defaultCulturalPreferences
      Planning = planning
      Breadcrumbs = initialBreadcrumbs },
    Cmd.batch
        [ Cmd.map ApplicationBarMsg applicationBarCmd
          Cmd.map NavigationMsg navigationCmd
          Cmd.map ViewSpaceMsg viewSpaceCmd
          Cmd.map SidePanelMsg sidePanelCmd
          Cmd.map CommandPaletteMsg commandPaletteCmd
          initialViewCmd ]

let private handleNavigationLoaded (views: ViewDefinition list) (model: Model) : Model * Cmd<Msg> =
    model, Cmd.ofMsg(NavigationMsg(NavigationBar.Msg.ReplaceViews views))

let private newViewComponentId () = ViewComponentId.literal $"vc-{Guid.NewGuid():N}"

let routeAddComponent componentDefId componentRegistry schemaRegistry =
    let occurrenceId = newViewComponentId()
    ComponentRequest.prepareAddComponent occurrenceId componentDefId componentRegistry schemaRegistry

let private handleApplicationBarOutput
    (output: ApplicationBar.Output)
    (model: Model)
    : Model * Cmd<Msg> * Output option =

    match output with
    | ApplicationBar.Output.ToggleNavigation ->
        let newNavModel, navCmd, _ = NavigationBar.update NavigationBar.Msg.ToggleCollapsed model.Navigation
        { model with Navigation = newNavModel }, (Cmd.map NavigationMsg navCmd), None
    | ApplicationBar.Output.ScenarioChanged sc -> model, Cmd.ofMsg(SetScenario sc), None
    | ApplicationBar.Output.HorizonChanged hz -> model, Cmd.ofMsg(SetHorizon hz), None
    | ApplicationBar.Output.PlantChanged pl -> model, Cmd.ofMsg(SetPlant pl), None
    | ApplicationBar.Output.OpenCommandPalette -> model, Cmd.ofMsg ToggleCommandPalette, None
    | ApplicationBar.Output.SettingsRequested -> model, Cmd.ofMsg OpenSettingsModal, None
    | ApplicationBar.Output.AboutRequested ->
        let req =
            Services.NotificationRequest.Create(
                "About",
                "Medhavi APS v1.0 Enterprise Edition.",
                Services.NotificationType.Info
            )

        model, Cmd.none, Some(Output.Notify req)
    | ApplicationBar.Output.ToggleSidePanel ->
        let newmodel, cmd, _ =
            SidePanel.update (SidePanel.Msg.SetExpanded(not model.SidePanel.Expanded)) model.SidePanel

        { model with SidePanel = newmodel }, (Cmd.map SidePanelMsg cmd), None
    | ApplicationBar.Output.LogoutRequested -> model, Cmd.none, Some Output.Logout

let private handleNavigationOutput
    componentRegistry
    schemaRegistry
    (output: NavigationBar.Output)
    (model: Model)
    : Model * Cmd<Msg> * Output option =

    match output with
    | NavigationBar.Output.LinkClicked link ->
        match link.Target with
        | NavigationBar.NavigationTarget.OpenView viewId -> model, Cmd.ofMsg(NavigateToView viewId), None
        | NavigationBar.NavigationTarget.AddComponent componentDefId ->
            match routeAddComponent componentDefId componentRegistry schemaRegistry with
            | Ok request -> Cmd.ofMsg(ViewSpaceMsg(ViewSpace.Msg.Request request))
            | Error _ -> Cmd.none
            |> fun cmd -> model, cmd, None
        | NavigationBar.NavigationTarget.Tool _ -> model, Cmd.none, None

let private handleViewSpaceOutput (ctx: ApplicationContext) (output: ViewSpace.Output) (model: Model) : Model * Cmd<Msg> * Output option =
    match output with
    | ViewSpace.Output.ActionRequested(viewComponentId, actionId, targetKey, payload) ->
        let updatedModel =
            { model with
                SidePanel =
                    { model.SidePanel with
                        SelectedDetail = targetKey } }

        if ActionRegistry.contains actionId ctx.ActionRegistry then
            let actionCtx : ActionExecutionContext =
                { Services = ctx.Services
                  OriginComponentId = viewComponentId
                  TargetKey = targetKey
                  Payload = payload
                  PlanningContext = model.Planning }

            let executeTask () =
                ActionRouter.execute ctx.ActionRegistry actionCtx actionId

            updatedModel,
            Cmd.OfTask.either
                executeTask
                ()
                ActionExecutionCompleted
                (fun ex -> ActionExecutionCompleted (Error ex.Message)),
            None
        else
            let keyDesc = targetKey |> Option.defaultValue "Selected item"

            if actionId = DemandGridActions.explainShortage.Id then
                let target = targetKey |> Option.defaultValue "D-1002"

                let sidePanelModel, sideCmd, _ =
                    SidePanel.update (SidePanel.Msg.ExplainEntity(target, "Demand")) model.SidePanel

                let req =
                    Services.NotificationRequest.Create(
                        "AI Copilot Analysis",
                        $"Generating causal graph trace for Demand '{target}'...",
                        Services.NotificationType.Info
                    )

                { model with
                    SidePanel = sidePanelModel },
                Cmd.map SidePanelMsg sideCmd,
                Some(Output.Notify req)

            elif actionId = SupplyActions.explainDelay.Id then
                let target = targetKey |> Option.defaultValue "PO-8802"

                let sidePanelModel, sideCmd, _ =
                    SidePanel.update (SidePanel.Msg.ExplainEntity(target, "Supply")) model.SidePanel

                let req =
                    Services.NotificationRequest.Create(
                        "AI Copilot Analysis",
                        $"Generating supplier delay trace for PO '{target}'...",
                        Services.NotificationType.Info
                    )

                { model with
                    SidePanel = sidePanelModel },
                Cmd.map SidePanelMsg sideCmd,
                Some(Output.Notify req)

            elif actionId = CapacityActions.explainBottleneck.Id then
                let target = targetKey |> Option.defaultValue "WC-BER-03"

                let sidePanelModel, sideCmd, _ =
                    SidePanel.update (SidePanel.Msg.ExplainEntity(target, "Capacity")) model.SidePanel

                let req =
                    Services.NotificationRequest.Create(
                        "AI Copilot Analysis",
                        $"Generating machine bottleneck & load trace for '{target}'...",
                        Services.NotificationType.Info
                    )

                { model with
                    SidePanel = sidePanelModel },
                Cmd.map SidePanelMsg sideCmd,
                Some(Output.Notify req)

            else
                let updatedModel =
                    { model with
                        SidePanel =
                            { model.SidePanel with
                                SelectedDetail = targetKey } }

                match ComponentCatalog.tryFindAction actionId with
                | Some descriptor ->
                    match descriptor.ExecutionMode with
                    | Direct ->
                        let req =
                            Services.NotificationRequest.Create(
                                descriptor.DisplayName,
                                $"Action '{descriptor.DisplayName}' executed for '{keyDesc}'.",
                                Services.NotificationType.Info
                            )

                        updatedModel, Cmd.none, Some(Output.Notify req)
                    | RequiresConfirmation prompt ->
                        let req =
                            Services.NotificationRequest.Create(
                                descriptor.DisplayName,
                                $"Confirmed: {prompt} on target '{keyDesc}'.",
                                Services.NotificationType.Success
                            )

                        updatedModel, Cmd.none, Some(Output.Notify req)
                    | Parameterized(schemaId, formTitle) ->
                        let req =
                            Services.NotificationRequest.Create(
                                formTitle,
                                $"Parameter form '{schemaId}' initiated for '{keyDesc}'.",
                                Services.NotificationType.Info
                            )

                        updatedModel, Cmd.none, Some(Output.Notify req)
                | None ->
                    let req =
                        Services.NotificationRequest.Create(
                            "Unknown Action",
                            $"Action '{ActionDefId.value actionId}' is not registered in the catalog.",
                            Services.NotificationType.Warning
                        )

                    updatedModel, Cmd.none, Some(Output.Notify req)

    | ViewSpace.Output.ParameterizedActionRequested(_viewComponentId, actionId, targetKey) ->
        let keyDesc = targetKey |> Option.defaultValue "Selected item"
        let req = Services.NotificationRequest.Create("Parameterized Action", $"Initiating parameterized action '{ActionDefId.value actionId}' for '{keyDesc}'", Services.NotificationType.Info)
        model, Cmd.none, Some(Output.Notify req)

    | ViewSpace.Output.SelectionChanged(viewComponentId, targetKey) ->
        let updatedModel = { model with SidePanel = { model.SidePanel with SelectedDetail = targetKey } }
        updatedModel, Cmd.none, None

    | ViewSpace.Output.RefreshRequested viewComponentId ->
        let req =
            Services.NotificationRequest.Create(
                "Data Refreshed",
                $"Refreshed data for component '{ViewComponentId.value viewComponentId}'.",
                Services.NotificationType.Info
            )

        model, Cmd.none, Some(Output.Notify req)
    | ViewSpace.Output.ViewOperationRejected error ->
        let req =
            Services.NotificationRequest.Create(
                "Operation Rejected",
                sprintf "%A" error,
                Services.NotificationType.Error'
            )

        model, Cmd.none, Some(Output.Notify req)
    | ViewSpace.Output.LayoutAdapterError error ->
        let req =
            Services.NotificationRequest.Create("Layout Error", sprintf "%A" error, Services.NotificationType.Error')

        model, Cmd.none, Some(Output.Notify req)

let rec update (ctx: ApplicationContext) (msg: Msg) (model: Model) : Model * Cmd<Msg> * Output option =
    match msg with
    | ApplicationBarMsg childMsg ->
        updateChildWithOutput
            (fun m -> m.ApplicationBar)
            (fun child m -> { m with ApplicationBar = child })
            ApplicationBarMsg
            ApplicationBar.update
            handleApplicationBarOutput
            childMsg
            model

    | NavigationMsg childMsg ->
        updateChildWithOutput
            (fun m -> m.Navigation)
            (fun child m -> { m with Navigation = child })
            NavigationMsg
            NavigationBar.update
            (handleNavigationOutput ctx.ComponentRegistry ctx.SchemaRegistry)
            childMsg
            model

    | ViewSpaceMsg childMsg ->
        updateChildWithOutput
            (fun m -> m.ViewSpace)
            (fun child m -> { m with ViewSpace = child })
            ViewSpaceMsg
            (ViewSpace.update ctx)
            (handleViewSpaceOutput ctx)
            childMsg
            model

    | SidePanelMsg childMsg ->
        updateChildWithOutput
            (fun m -> m.SidePanel)
            (fun child m -> { m with SidePanel = child })
            SidePanelMsg
            SidePanel.update
            (fun output model ->
                match output with
                | SidePanel.Output.ResolutionApplied(resId, actionCode) ->
                    let req =
                        Services.NotificationRequest.Create(
                            "Autonomous Resolution Executed",
                            $"Applied AI proposal '{resId}' ({actionCode}). Active plan schedule synchronized.",
                            Services.NotificationType.Success
                        )

                    model, Cmd.none, Some(Output.Notify req))
            childMsg
            model

    | CommandPaletteMsg childMsg ->
        updateChildWithOutput
            (fun m -> m.CommandPalette)
            (fun child m -> { m with CommandPalette = child })
            CommandPaletteMsg
            CommandPalette.update
            (fun output model ->
                match output with
                | CommandPalette.Output.ExecuteCommand command ->
                    if command.Action.StartsWith("view-") then
                        match ViewDefId.create command.Action with
                        | Ok idv -> model, Cmd.ofMsg(NavigateToView idv), None
                        | Error _ -> model, Cmd.none, None
                    elif command.Action = "action:toggle-edit" then
                        model, Cmd.ofMsg(ViewSpaceMsg ViewSpace.Msg.ToggleEditMode), None
                    elif command.Action = "scenario:baseline" then
                        model, Cmd.ofMsg(SetScenario(ScenarioId.literal "baseline")), None
                    elif command.Action = "scenario:what-if-1" then
                        model, Cmd.ofMsg(SetScenario(ScenarioId.literal "what-if-1")), None
                    elif command.Action = "scenario:what-if-2" then
                        model, Cmd.ofMsg(SetScenario(ScenarioId.literal "what-if-2")), None
                    elif command.Action = "plant:ALL" then
                        model, Cmd.ofMsg(SetPlant None), None
                    elif command.Action.StartsWith("plant:") then
                        let pl = command.Action.Substring("plant:".Length)
                        model, Cmd.ofMsg(SetPlant(Some pl)), None
                    else
                        model, Cmd.none, None)
            childMsg
            model

    | NavigateToView viewId ->
        match model.SavedViews |> List.tryFind(fun v -> v.Id = viewId) with
        | Some viewDef ->
            let req = ViewSpace.OpenViewDefinition viewDef
            let breadcrumbs = pushBreadcrumb model.Breadcrumbs viewDef.Name (Some viewDef.Metadata.Icon)
            { model with Breadcrumbs = breadcrumbs }, Cmd.ofMsg(ViewSpaceMsg(ViewSpace.Msg.Request req)), None
        | None -> model, Cmd.none, None

    | NavigationLoaded views -> handleNavigationLoaded views model |> fun (newModel, cmd) -> newModel, cmd, None

    | NavigationLoadFailed error ->
        let req = Services.NotificationRequest.Create("Navigation Load Failed", error, Services.NotificationType.Error')

        model, Cmd.none, Some(Output.Notify req)

    | BreadcrumbClicked index ->
        let newBreadcrumbs =
            model.Breadcrumbs
            |> List.mapi(fun i breadcrumb -> { breadcrumb with IsActive = i = index })
            |> List.truncate(index + 1)

        { model with
            Breadcrumbs = newBreadcrumbs },
        Cmd.none,
        None

    | SetScenario scenario ->
        ctx.PlanningContextService.Update(fun p -> { p with Scenario = scenario })

        let req =
            Services.NotificationRequest.Create(
                "Scenario Switched",
                $"Active scenario changed to '{ScenarioId.value scenario}'.",
                Services.NotificationType.Info
            )

        let newPlanning =
            { model.Planning with
                Scenario = scenario }

        { model with
            Planning = newPlanning
            ApplicationBar =
                { model.ApplicationBar with
                    Planning = newPlanning } },
        Cmd.none,
        Some(Output.Notify req)

    | SetHorizon horizon ->
        ctx.PlanningContextService.Update(fun p -> { p with Horizon = horizon })
        let startStr = horizon.Start.ToString("yyyy-MM-dd")
        let endStr = horizon.End.ToString("yyyy-MM-dd")

        let req =
            Services.NotificationRequest.Create(
                "Horizon Presets",
                $"Planning horizon set to '{startStr} -> {endStr}'.",
                Services.NotificationType.Info
            )

        let newPlanning =
            { model.Planning with
                Horizon = horizon }

        { model with
            Planning = newPlanning
            ApplicationBar =
                { model.ApplicationBar with
                    Planning = newPlanning } },
        Cmd.none,
        Some(Output.Notify req)

    | SetPlant plantOpt ->
        ctx.PlanningContextService.Update(fun p -> { p with PlantId = plantOpt })
        let plantDesc = plantOpt |> Option.defaultValue "All Plants"

        let req =
            Services.NotificationRequest.Create(
                "Plant Scope",
                $"Active plant scope set to '{plantDesc}'.",
                Services.NotificationType.Info
            )

        let newPlanning =
            { model.Planning with
                PlantId = plantOpt }

        { model with
            Planning = newPlanning
            ApplicationBar =
                { model.ApplicationBar with
                    Planning = newPlanning } },
        Cmd.none,
        Some(Output.Notify req)

    // ------------------------------------------------------------
    // View Lifecycle & Persistence (Save / Save As / Revert)
    // ------------------------------------------------------------

    | SaveActiveTab ->
        match ViewSpace.activeTab model.ViewSpace with
        | Some tab ->
            match tab.Session.SourceViewDefId with
            | Some defId ->
                match model.SavedViews |> List.tryFind(fun v -> v.Id = defId) with
                | Some existingView ->
                    let newVersion = (tab.Session.SourceVersion |> Option.defaultValue existingView.Version) + 1

                    let updatedView =
                        { existingView with
                            Version = newVersion
                            State = tab.Session.State
                            Metadata =
                                { existingView.Metadata with
                                    ModifiedAt = DateTimeOffset.UtcNow } }

                    let updatedSavedViews =
                        model.SavedViews |> List.map(fun v -> if v.Id = defId then updatedView else v)

                    let updatedSession =
                        { tab.Session with
                            SourceVersion = Some newVersion }

                    let updatedViewSpace =
                        { model.ViewSpace with
                            Tabs =
                                ViewSpace.updateTab
                                    tab.Id
                                    (fun t -> { t with Session = updatedSession })
                                    model.ViewSpace.Tabs }

                    let req =
                        Services.NotificationRequest.Create(
                            "View Saved",
                            $"View '{updatedView.Name}' (v{newVersion}) saved successfully.",
                            Services.NotificationType.Success
                        )

                    { model with
                        SavedViews = updatedSavedViews
                        ViewSpace = updatedViewSpace },
                    Cmd.ofMsg(NavigationMsg(NavigationBar.Msg.ReplaceViews updatedSavedViews)),
                    Some(Output.Notify req)

                | None ->
                    // View definition not found in saved list; prompt modal
                    { model with
                        SaveViewModal =
                            Some
                                { ViewName = tab.Session.State.Title
                                  Category = "Custom Workspaces"
                                  Icon = "dashboard"
                                  IsSaveAs = false } },
                    Cmd.none,
                    None

            | None ->
                // Transient/blank workspace without a source view def; prompt modal
                { model with
                    SaveViewModal =
                        Some
                            { ViewName = tab.Session.State.Title
                              Category = "Custom Workspaces"
                              Icon = "dashboard"
                              IsSaveAs = false } },
                Cmd.none,
                None

        | None -> model, Cmd.none, None

    | SaveActiveTabAs ->
        let defaultName =
            ViewSpace.activeTab model.ViewSpace
            |> Option.map(fun t -> t.Session.State.Title + " (Custom)")
            |> Option.defaultValue "New Custom Workspace"

        { model with
            SaveViewModal =
                Some
                    { ViewName = defaultName
                      Category = "Custom Workspaces"
                      Icon = "dashboard"
                      IsSaveAs = true } },
        Cmd.none,
        None

    | OpenSaveViewModal isSaveAs ->
        let defaultName =
            ViewSpace.activeTab model.ViewSpace
            |> Option.map(fun t -> if isSaveAs then t.Session.State.Title + " (Custom)" else t.Session.State.Title)
            |> Option.defaultValue "New Custom Workspace"

        { model with
            SaveViewModal =
                Some
                    { ViewName = defaultName
                      Category = "Custom Workspaces"
                      Icon = "dashboard"
                      IsSaveAs = isSaveAs } },
        Cmd.none,
        None

    | UpdateSaveViewModalName name ->
        { model with
            SaveViewModal = model.SaveViewModal |> Option.map(fun s -> { s with ViewName = name }) },
        Cmd.none,
        None

    | UpdateSaveViewModalCategory cat ->
        { model with
            SaveViewModal = model.SaveViewModal |> Option.map(fun s -> { s with Category = cat }) },
        Cmd.none,
        None

    | UpdateSaveViewModalIcon icon ->
        { model with
            SaveViewModal = model.SaveViewModal |> Option.map(fun s -> { s with Icon = icon }) },
        Cmd.none,
        None

    | CloseSaveViewModal -> { model with SaveViewModal = None }, Cmd.none, None

    | OpenSettingsModal ->
        let settingsModel = SettingsDialog.init model.Appearance model.Cultural

        { model with
            SettingsModal = Some settingsModel },
        Cmd.none,
        None

    | CloseSettingsModal -> { model with SettingsModal = None }, Cmd.none, None

    | SettingsDialogMsg sMsg ->
        match model.SettingsModal with
        | Some sModel ->
            let updatedSettingsModel, outputOpt = SettingsDialog.update sMsg sModel

            match outputOpt with
            | Some(SettingsDialog.Output.ThemeUpdated theme) ->
                let newAppearance = { model.Appearance with Theme = theme }

                { model with
                    SettingsModal = Some updatedSettingsModel
                    Appearance = newAppearance },
                Cmd.none,
                Some(Output.ChangeTheme theme)
            | Some(SettingsDialog.Output.CulturalUpdated cultural) ->
                { model with
                    SettingsModal = Some updatedSettingsModel
                    Cultural = cultural },
                Cmd.none,
                Some(Output.ChangeCulture cultural)
            | Some(SettingsDialog.Output.AppearanceUpdated app) ->
                { model with
                    SettingsModal = Some updatedSettingsModel
                    Appearance = app },
                Cmd.none,
                None
            | Some SettingsDialog.Output.CloseModal -> { model with SettingsModal = None }, Cmd.none, None
            | None ->
                { model with
                    SettingsModal = Some updatedSettingsModel },
                Cmd.none,
                None
        | None -> model, Cmd.none, None

    | ConfirmSaveViewModal ->
        match model.SaveViewModal, ViewSpace.activeTab model.ViewSpace with
        | Some modalState, Some tab ->
            let newDefId = ViewDefId.literal $"view-{Guid.NewGuid():N}"

            let newMetadata =
                { Category = modalState.Category
                  Icon = modalState.Icon
                  Roles = [ "Planner"; "Buyer"; "PlantManager"; "Executive" ]
                  IsDefault = false
                  IsTemplate = false
                  CreatedAt = DateTimeOffset.UtcNow
                  CreatedBy = "Planner"
                  ModifiedAt = DateTimeOffset.UtcNow
                  ModifiedBy = "Planner"
                  Permissions =
                    { Owner = "Planner"
                      Visibility = ViewVisibility.Public
                      EditableBy = [ "Planner" ]
                      Shareable = true } }

            let updatedState =
                { tab.Session.State with
                    Title = modalState.ViewName }

            let newView: ViewDefinition =
                { Id = newDefId
                  Name = modalState.ViewName
                  Version = 1
                  State = updatedState
                  Metadata = newMetadata }

            let updatedSavedViews = model.SavedViews @ [ newView ]

            let updatedSession =
                { tab.Session with
                    SourceViewDefId = Some newDefId
                    SourceVersion = Some 1
                    State = updatedState }

            let updatedViewSpace =
                { model.ViewSpace with
                    Tabs =
                        ViewSpace.updateTab tab.Id (fun t -> { t with Session = updatedSession }) model.ViewSpace.Tabs }

            let req =
                Services.NotificationRequest.Create(
                    "View Created",
                    $"Custom view '{modalState.ViewName}' saved and registered in Navigation.",
                    Services.NotificationType.Success
                )

            { model with
                SavedViews = updatedSavedViews
                ViewSpace = updatedViewSpace
                SaveViewModal = None },
            Cmd.ofMsg(NavigationMsg(NavigationBar.Msg.ReplaceViews updatedSavedViews)),
            Some(Output.Notify req)

        | _ -> { model with SaveViewModal = None }, Cmd.none, None

    | RevertActiveTab ->
        match ViewSpace.activeTab model.ViewSpace with
        | Some tab ->
            match tab.Session.SourceViewDefId with
            | Some defId ->
                match model.SavedViews |> List.tryFind(fun v -> v.Id = defId) with
                | Some originalView ->
                    let revertedSession = ViewLifecycle.openSession tab.Session.SessionId originalView

                    let updatedViewSpace =
                        { model.ViewSpace with
                            Tabs =
                                ViewSpace.updateTab
                                    tab.Id
                                    (fun t ->
                                        { t with
                                            Session = revertedSession
                                            IsEditMode = false })
                                    model.ViewSpace.Tabs }

                    let req =
                        Services.NotificationRequest.Create(
                            "View Reverted",
                            $"Reverted all changes to '{originalView.Name}'.",
                            Services.NotificationType.Info
                        )

                    { model with
                        ViewSpace = updatedViewSpace },
                    Cmd.none,
                    Some(Output.Notify req)
                | None -> model, Cmd.none, None
            | None -> model, Cmd.none, None
        | None -> model, Cmd.none, None

    | CreateBlankWorkspace ->
        let newSessionId = ViewSessionId.literal $"session-{Guid.NewGuid():N}"

        let blankSession =
            TransientViewPolicy.createComponentWorkspace newSessionId "New Workspace" GridDimensions.defaultDimensions

        let req =
            Services.NotificationRequest.Create(
                "New Blank Workspace",
                "Created empty 12x12 canvas. Click components in the sidebar to populate your custom view.",
                Services.NotificationType.Info
            )

        model,
        Cmd.ofMsg(ViewSpaceMsg(ViewSpace.Msg.Request(ViewSpace.OpenViewSession blankSession))),
        Some(Output.Notify req)

    | ToggleActiveTabEditMode -> model, Cmd.ofMsg(ViewSpaceMsg ViewSpace.Msg.ToggleEditMode), None

    // ------------------------------------------------------------
    // Command palette
    // ------------------------------------------------------------

    | ToggleCommandPalette ->
        let command =
            if model.CommandPalette.IsOpen then
                CommandPalette.Msg.Close
            else
                CommandPalette.Msg.Open

        let childModel, childCmd, _ = CommandPalette.update command model.CommandPalette

        { model with
            CommandPalette = childModel },
        Cmd.map CommandPaletteMsg childCmd,
        None

    // ------------------------------------------------------------
    // Action execution outcome handling
    // ------------------------------------------------------------

    | ActionExecutionCompleted result ->
        match result with
        | Ok (ActionExecutionOutcome.Completed (successMsg, refreshedStores)) ->
            let notify = Services.NotificationRequest.Create("Success", successMsg, Services.NotificationType.Success)
            let refreshCmd =
                if List.isEmpty refreshedStores then Cmd.none
                else Cmd.ofMsg (ViewSpaceMsg (ViewSpace.Msg.RefreshStores refreshedStores))
            model, refreshCmd, Some (Output.Notify notify)
        | Ok (ActionExecutionOutcome.ExplainRequested (entityId, entityKind)) ->
            let sidePanelModel, sideCmd, _ =
                SidePanel.update (SidePanel.Msg.ExplainEntity(entityId, entityKind)) model.SidePanel
            { model with SidePanel = sidePanelModel }, Cmd.map SidePanelMsg sideCmd, None
        | Ok (ActionExecutionOutcome.NavigationRequested targetView) ->
            model, Cmd.ofMsg (NavigateToView targetView), None
        | Ok (ActionExecutionOutcome.JobInitiated (jobId, desc, _)) ->
            let notify = Services.NotificationRequest.Create("Engine Job Started", $"{desc} (Job ID: {jobId})", Services.NotificationType.Info)
            model, Cmd.none, Some (Output.Notify notify)
        | Ok (ActionExecutionOutcome.ClientSideDialogRequested (title, msg)) ->
            let notify = Services.NotificationRequest.Create(title, msg, Services.NotificationType.Info)
            model, Cmd.none, Some (Output.Notify notify)
        | Error errorMsg ->
            let notify = Services.NotificationRequest.Create("Action Failed", errorMsg, Services.NotificationType.Error')
            model, Cmd.none, Some (Output.Notify notify)

let private renderBreadcrumbs (model: Model) (_: Msg -> unit) : Node =
    comp<RadzenBreadCrumb> {
        attr.fragment
            "Items"
            (forEach model.Breadcrumbs (fun breadcrumb ->
                comp<RadzenBreadCrumbItem> {
                    "Text" => breadcrumb.Label
                    if breadcrumb.Icon.IsSome then "Icon" => breadcrumb.Icon.Value else attr.empty()
                }))
    }

let private renderBreadcrumbsBar (model: Model) (dispatch: Msg -> unit) : Node =
    div {
        attr.style
            "padding: 0.35rem 1rem; border-bottom: 1px solid var(--rz-border-color); background: var(--rz-base-background-color); display: flex; align-items: center;"

        renderBreadcrumbs model dispatch
    }

let private renderFooter (model: Model) (dispatch: Msg -> unit) : Node =
    let activeTabOpt = ViewSpace.activeTab model.ViewSpace

    let activeViewTitle =
        activeTabOpt |> Option.map(fun t -> t.Session.State.Title) |> Option.defaultValue "No Active View"

    let isDirty =
        activeTabOpt
        |> Option.map(fun t ->
            t.Session.SourceViewDefId
            |> Option.bind(fun defId ->
                model.SavedViews |> List.tryFind(fun v -> v.Id = defId) |> Option.map(fun v -> v.State))
            |> Option.map(fun baseState -> ViewLifecycle.isDirty t.Session (Some baseState))
            |> Option.defaultValue true) // Transient sessions are always dirty
        |> Option.defaultValue false

    let isEditMode = activeTabOpt |> Option.map(fun t -> t.IsEditMode) |> Option.defaultValue false

    let canSave = Option.isSome activeTabOpt
    let plantSummary = model.Planning.PlantId |> Option.defaultValue "All Plants"

    comp<RadzenFooter> {
        attr.style
            "display: flex; align-items: center; justify-content: space-between; padding: 0.35rem 1.25rem; background: var(--rz-base-background-color); border-top: 1px solid var(--rz-border-color); font-size: 12px; gap: 1rem; flex-wrap: wrap;"

        // Left: Platform connection & Planning Context Scope
        div {
            attr.style "display: flex; align-items: center; gap: 0.75rem;"

            div {
                attr.style "display: flex; align-items: center; gap: 0.35rem; font-weight: 600;"

                span {
                    attr.style
                        "display: inline-block; width: 8px; height: 8px; border-radius: 50%; background: var(--rz-success);"
                }

                span {
                    attr.style "color: var(--rz-text-color); font-size: 11px;"
                    "Connected · Medhavi APS"
                }
            }

            div { attr.style "width: 1px; height: 14px; background: var(--rz-border-color);" }

            div {
                attr.style
                    "display: flex; align-items: center; gap: 0.4rem; color: var(--rz-text-secondary-color); font-size: 11px;"

                span { $"Scenario: {ScenarioId.value model.Planning.Scenario}" }
                span { "·" }
                span { $"Plant: {plantSummary}" }
            }
        }

        // Center: Active View Name & Dirty State
        div {
            attr.style "display: flex; align-items: center; gap: 0.5rem;"

            span {
                attr.``class`` "rzi"
                attr.style "font-size: 14px; color: var(--rz-primary);"
                "auto_awesome_mosaic"
            }

            span {
                attr.style "font-weight: 600; color: var(--rz-text-color); font-size: 12px;"
                activeViewTitle
            }

            if isDirty then
                span {
                    attr.style
                        "background: var(--rz-warning-lighter); color: var(--rz-warning-dark); border: 1px solid var(--rz-warning); border-radius: 10px; padding: 1px 8px; font-size: 10px; font-weight: 700;"

                    "● Modified"
                }

            if isEditMode then
                span {
                    attr.style
                        "background: var(--rz-info-lighter); color: var(--rz-info); border: 1px solid var(--rz-info); border-radius: 10px; padding: 1px 8px; font-size: 10px; font-weight: 700;"

                    "⚙️ Layout Mode"
                }
        }

        // Right: Space-efficient SplitButton for View Lifecycle & Layout
        div {
            attr.style "display: flex; align-items: center; gap: 0.75rem;"

            comp<RadzenSplitButton> {
                "Text" => (if isDirty then "Save View *" else "Save View")
                "Icon" => "save"
                "ButtonStyle" => (if isDirty then ButtonStyle.Warning else ButtonStyle.Primary)
                "Size" => ButtonSize.Small
                "Disabled" => not canSave

                attr.callback "Click" (fun (args: RadzenSplitButtonItem) ->
                    if isNull(box args) || isNull(box args.Value) then
                        dispatch SaveActiveTab
                    else
                        match string args.Value with
                        | "save_as" -> dispatch SaveActiveTabAs
                        | "revert" -> dispatch RevertActiveTab
                        | "new_blank" -> dispatch CreateBlankWorkspace
                        | "toggle_edit" -> dispatch ToggleActiveTabEditMode
                        | _ -> dispatch SaveActiveTab)

                attr.fragment
                    "ChildContent"
                    (concat {
                        comp<RadzenSplitButtonItem> {
                            "Text" => "Save View As..."
                            "Value" => "save_as"
                            "Icon" => "save_as"
                        }

                        comp<RadzenSplitButtonItem> {
                            "Text" => "Revert Unsaved Changes"
                            "Value" => "revert"
                            "Icon" => "undo"
                            "Disabled" => not isDirty
                        }

                        comp<RadzenSplitButtonItem> {
                            "Text" => "New Blank Workspace"
                            "Value" => "new_blank"
                            "Icon" => "add_box"
                        }

                        comp<RadzenSplitButtonItem> {
                            "Text" => (if isEditMode then "Exit Layout Edit Mode" else "Edit Tile Layout")
                            "Value" => "toggle_edit"
                            "Icon" => "dashboard_customize"
                        }
                    })
            }

            Rz.text("v1.0.0", textStyle = TextStyle.Caption)
        }
    }

let private renderSaveViewModal (modalState: SaveViewModalState) (dispatch: Msg -> unit) : Node =
    div {
        // Backdrop Overlay
        attr.style
            """
                position: fixed;
                top: 0;
                left: 0;
                width: 100vw;
                height: 100vh;
                background: rgba(0, 0, 0, 0.6);
                backdrop-filter: blur(4px);
                display: flex;
                align-items: center;
                justify-content: center;
                z-index: 100000;
                """

        // Modal Card
        div {
            attr.style
                """
                    background: var(--rz-base-background-color);
                    border: 1px solid var(--rz-border-color);
                    border-radius: 12px;
                    box-shadow: 0 16px 36px rgba(0, 0, 0, 0.4);
                    width: 440px;
                    max-width: 90vw;
                    padding: 1.5rem;
                    display: flex;
                    flex-direction: column;
                    gap: 1rem;
                    """

            // Modal Header
            div {
                attr.style
                    "display: flex; align-items: center; justify-content: space-between; border-bottom: 1px solid var(--rz-border-color); padding-bottom: 0.75rem;"

                div {
                    attr.style "display: flex; align-items: center; gap: 0.5rem;"

                    span {
                        attr.``class`` "rzi"
                        attr.style "font-size: 1.35rem; color: var(--rz-primary);"
                        "save_as"
                    }

                    span {
                        attr.style "font-size: 16px; font-weight: 700; color: var(--rz-text-color);"
                        if modalState.IsSaveAs then "Save View As..." else "Save Custom View"
                    }
                }

                button {
                    attr.``type`` "button"

                    attr.style
                        "border: none; background: transparent; color: var(--rz-text-secondary-color); font-size: 16px; cursor: pointer;"

                    attr.callback "onclick" (fun (_: MouseEventArgs) -> dispatch CloseSaveViewModal)
                    "✕"
                }
            }

            // Form Fields
            div {
                attr.style "display: flex; flex-direction: column; gap: 0.85rem;"

                // View Name Input
                div {
                    attr.style "display: flex; flex-direction: column; gap: 0.25rem;"

                    span {
                        attr.style "font-size: 12px; font-weight: 600; color: var(--rz-text-secondary-color);"
                        "View Name"
                    }

                    comp<RadzenTextBox> {
                        "Value" => modalState.ViewName
                        "Placeholder" => "Enter unique view title..."
                        "Style" => "width: 100%;"
                        attr.callback "Change" (fun (v: string) -> dispatch(UpdateSaveViewModalName v))
                    }
                }

                // Category Selection
                div {
                    attr.style "display: flex; flex-direction: column; gap: 0.25rem;"

                    span {
                        attr.style "font-size: 12px; font-weight: 600; color: var(--rz-text-secondary-color);"
                        "Navigation Category"
                    }

                    comp<RadzenDropDown<string>> {
                        "Data"
                        => [ "Custom Workspaces"
                             "Demand Planning"
                             "Capacity Planning"
                             "Supply Planning"
                             "Connected Planning"
                             "Executive Overview" ]

                        "Value" => modalState.Category
                        "Style" => "width: 100%;"
                        "PopupStyle" => "z-index: 100005;"
                        attr.callback "Change" (fun (v: obj) -> dispatch(UpdateSaveViewModalCategory(string v)))
                    }
                }

                // Icon Selection
                div {
                    attr.style "display: flex; flex-direction: column; gap: 0.25rem;"

                    span {
                        attr.style "font-size: 12px; font-weight: 600; color: var(--rz-text-secondary-color);"
                        "Sidebar Icon"
                    }

                    comp<RadzenDropDown<string>> {
                        "Data"
                        => [ "dashboard"
                             "hub"
                             "analytics"
                             "precision_manufacturing"
                             "view_list"
                             "splitscreen"
                             "auto_awesome_mosaic" ]

                        "Value" => modalState.Icon
                        "Style" => "width: 100%;"
                        "PopupStyle" => "z-index: 100005;"
                        attr.callback "Change" (fun (v: obj) -> dispatch(UpdateSaveViewModalIcon(string v)))
                    }
                }
            }

            // Modal Footer Actions
            div {
                attr.style
                    "display: flex; align-items: center; justify-content: flex-end; gap: 0.5rem; margin-top: 0.5rem;"

                comp<RadzenButton> {
                    "Text" => "Cancel"
                    "ButtonStyle" => ButtonStyle.Light
                    "Size" => ButtonSize.Small
                    attr.callback "Click" (fun (_: MouseEventArgs) -> dispatch CloseSaveViewModal)
                }

                comp<RadzenButton> {
                    "Text" => "Save View"
                    "Icon" => "save"
                    "ButtonStyle" => ButtonStyle.Primary
                    "Size" => ButtonSize.Small
                    "Disabled" => String.IsNullOrWhiteSpace modalState.ViewName
                    attr.callback "Click" (fun (_: MouseEventArgs) -> dispatch ConfirmSaveViewModal)
                }
            }
        }
    }

let view (ctx: ApplicationContext) (model: Model) (dispatch: Msg -> unit) : Node =
    let dep =
        { Components = ctx.ComponentRegistry
          Schemas = ctx.SchemaRegistry
          Culture = Formatting.toCultureInfo model.Cultural
          DashboardHints = { RowHeight = 90; Gap = 8 } }
        : ViewSpace.ViewSpaceDependencies

    div {
        comp<RadzenLayout> {

            ecomp<ApplicationBar.Component, _, _> model.ApplicationBar (ApplicationBarMsg >> dispatch) { attr.empty() }

            ecomp<NavigationBar.Component, _, _> model.Navigation (NavigationMsg >> dispatch) { attr.empty() }

            comp<RadzenBody> {
                //renderBreadcrumbsBar model dispatch

                ecomp<ViewSpace.Component, _, _> model.ViewSpace (ViewSpaceMsg >> dispatch) {
                    "Dependencies" => Some dep
                }
            }

            ecomp<SidePanel.Component, _, _> model.SidePanel (SidePanelMsg >> dispatch) { attr.empty() }

            renderFooter model dispatch

        }

        ecomp<CommandPalette.Component, _, _> model.CommandPalette (CommandPaletteMsg >> dispatch) { attr.empty() }

        match model.SaveViewModal with
        | Some modalState -> renderSaveViewModal modalState dispatch
        | None -> empty()

        match model.SettingsModal with
        | Some settingsModel -> SettingsDialog.view settingsModel (SettingsDialogMsg >> dispatch)
        | None -> empty()
    }

type Component() =
    inherit ElmishComponent<Model, Msg>()

    [<Parameter>]
    member val Context: ApplicationContext = Unchecked.defaultof<ApplicationContext> with get, set

    override this.View model dispatch = view this.Context model dispatch
