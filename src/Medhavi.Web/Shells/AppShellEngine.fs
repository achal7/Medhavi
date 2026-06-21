module Medhavi.Web.AppShell.AppShellEngine

open System
open Bolero
open Elmish
open Microsoft.AspNetCore.Components
open Medhavi.Web
open Medhavi.Web.AppShell
open Medhavi.Web.Workspaces
open Medhavi.Web.Stores

let mapWorkspace (w: Workspace) : Navigation.WorkspaceNavigation =
    match w with
    | Workspace.ResourceScheduling -> Navigation.WorkspaceNavigation.Capacity
    | Workspace.MaterialReservation -> Navigation.WorkspaceNavigation.MaterialReservation
    | Workspace.ScenarioManagement -> Navigation.WorkspaceNavigation.Scenarios
    | Workspace.MasterData -> Navigation.WorkspaceNavigation.MasterData

let mapWorkspaceNavigationToWorkspace (nav: Navigation.WorkspaceNavigation) : Workspace option =
    match nav with
    | Navigation.WorkspaceNavigation.MaterialReservation -> Some Workspace.MaterialReservation
    | Navigation.WorkspaceNavigation.Capacity -> Some Workspace.ResourceScheduling
    | Navigation.WorkspaceNavigation.Scenarios -> Some Workspace.ScenarioManagement
    | Navigation.WorkspaceNavigation.MasterData -> Some Workspace.MasterData
    | _ -> None

let syncAppbarState (model: AppShellModel) : AppShellModel =
    let appbar = model.AppbarState

    let updatedAppbar =
        { appbar with
            User = model.Session.User
            ConnectionStatus = model.Session.ConnectionStatus
            Theme = model.Session.Theme
            Notifications = model.Session.Notifications
            CurrentUser = Some model.Session.User }

    { model with
        AppbarState = updatedAppbar }

let init (session: Session.Model) =
    let appbar, appbarCmd = Appbar.init session.User

    let nav, navCmd =
        Navigation.init
            [ Navigation.WorkspaceNavigation.Dashboard
              Navigation.WorkspaceNavigation.MaterialReservation
              Navigation.WorkspaceNavigation.Supply
              Navigation.WorkspaceNavigation.Capacity
              Navigation.WorkspaceNavigation.MasterData
              Navigation.WorkspaceNavigation.Scenarios ]

    { ActiveWorkspace = None
      NavigationbarExpanded = true
      Session = session
      RightSidebarExpanded = false
      RightSidebarActiveTab = 0
      CommandPaletteOpen = false
      CommandPaletteSearchText = ""
      SettingsDialogOpen = false
      ProfilePopoverOpen = false
      AppbarState = appbar
      NavigationState = nav
      MaterialReservationState = None
      MasterDataState = None }
    |> syncAppbarState,
    Cmd.batch [ Cmd.map AppbarMsg appbarCmd; Cmd.map NavigationMsg navCmd ]

let logSessionTrace
    (origin: CommandOrigin)
    (action: WorkspaceAction)
    (status: CommandStatus)
    (notes: string option)
    (session: Session.Model)
    : Session.Model * Cmd<Session.Msg> =
    let trace =
        { TimestampUtc = DateTime.UtcNow
          Origin = origin
          RawText = ""
          ActionText = string action
          Status = status
          Notes = notes }

    Session.update (Session.LogAction trace) session

let handleNavigationOutput
    (output: Navigation.Output)
    (model: AppShellModel)
    : AppShellModel * Cmd<Message> * Output option =
    match output with
    | Navigation.Output.SelectedMenu menu ->
        let workspaceOpt = mapWorkspaceNavigationToWorkspace menu

        match workspaceOpt with
        | Some workspace ->
            let action = WorkspaceAction.NavigateTo workspace

            let sessionModel, sessionCmd =
                logSessionTrace
                    (CommandOrigin.Human model.Session.User.Name)
                    action
                    CommandStatus.Succeeded
                    None
                    model.Session

            { model with Session = sessionModel },
            Cmd.batch [ Cmd.map SessionMsg sessionCmd; Cmd.ofMsg(ExecuteWorkspaceAction action) ],
            None
        | None -> model, Cmd.none, None
    | Navigation.Output.ToggleSidebar ->
        { model with
            NavigationbarExpanded = not model.NavigationbarExpanded },
        Cmd.none,
        None
    | Navigation.Output.SetSidebar expanded ->
        { model with
            NavigationbarExpanded = expanded },
        Cmd.none,
        None

let handleAppbarOutput (output: Appbar.Output) (model: AppShellModel) : AppShellModel * Cmd<Message> * Output option =
    match output with
    | Appbar.Output.ToggleNavigationbar ->
        { model with
            NavigationbarExpanded = not model.NavigationbarExpanded },
        Cmd.none,
        None
    | Appbar.Output.SelectActiveScenario selected ->
        let updatedAppbar =
            { model.AppbarState with
                CurrentScope = { ScenarioId = selected } }

        { model with
            AppbarState = updatedAppbar },
        Cmd.none,
        None
    | Appbar.Output.ToggleCommandPalette ->
        { model with
            CommandPaletteOpen = not model.CommandPaletteOpen },
        Cmd.none,
        None
    | Appbar.Output.SetTheme theme -> model, Cmd.ofMsg(SessionMsg(Session.Msg.SetTheme theme)), None
    | Appbar.Output.MarkAllNotificationsRead -> model, Cmd.ofMsg(SessionMsg Session.Msg.MarkAllNotificationsRead), None
    | Appbar.Output.ClearNotifications -> model, Cmd.ofMsg(SessionMsg Session.Msg.ClearNotifications), None
    | Appbar.Output.CycleUserRole -> model, Cmd.ofMsg(SessionMsg Session.Msg.CycleUserRole), None
    | Appbar.Output.TriggerLogout -> model, Cmd.none, Some Output.Logout
    | Appbar.Output.ToggleNotifications ->
        let isTabActive = model.RightSidebarExpanded && model.RightSidebarActiveTab = 1

        { model with
            RightSidebarExpanded = not isTabActive
            RightSidebarActiveTab = 1 },
        Cmd.none,
        None
    | Appbar.Output.ToggleActivityFeed ->
        let isTabActive = model.RightSidebarExpanded && model.RightSidebarActiveTab = 0

        { model with
            RightSidebarExpanded = not isTabActive
            RightSidebarActiveTab = 0 },
        Cmd.none,
        None
    | Appbar.Output.ToggleSettingsDialog ->
        { model with
            SettingsDialogOpen = not model.SettingsDialogOpen },
        Cmd.none,
        None
    | Appbar.Output.SetSettingsDialogOpen open' ->
        { model with
            SettingsDialogOpen = open' },
        Cmd.none,
        None

// Handle incoming material reservation output message
let handleMaterialReservationOutput
    (output: MaterialReservation.Output)
    (model: AppShellModel)
    : AppShellModel * Cmd<Message> * Output option =
    model, Cmd.none, None

let rec update (env: AppShellEnv) (msg: Message) (model: AppShellModel) : AppShellModel * Cmd<Message> * Output option =
    let newModel, cmd, out =
        match msg with
        // AI messages
        | ToggleCommandPalette ->
            { model with
                CommandPaletteOpen = not model.CommandPaletteOpen
                CommandPaletteSearchText = "" },
            Cmd.none,
            None
        | SetCommandPaletteOpen open' ->
            { model with
                CommandPaletteOpen = open' },
            Cmd.none,
            None

        // Component messages
        | ToggleSidebar ->
            { model with
                NavigationbarExpanded = not model.NavigationbarExpanded },
            Cmd.none,
            None
        | SetSidebar expanded ->
            { model with
                NavigationbarExpanded = expanded },
            Cmd.none,
            None
        | NavigationMsg msg ->
            updateChildWithOutput
                (fun m -> m.NavigationState)
                (fun child m -> { m with NavigationState = child })
                NavigationMsg
                Navigation.update
                handleNavigationOutput
                msg
                model
        | AppbarMsg msg ->
            updateChildWithOutput
                (fun m -> m.AppbarState)
                (fun child m -> { m with AppbarState = child })
                AppbarMsg
                Appbar.update
                handleAppbarOutput
                msg
                model

        // Right Sidebar / Control Center messages
        | SetRightSidebar(expanded, tab) ->
            { model with
                RightSidebarExpanded = expanded
                RightSidebarActiveTab = defaultArg tab model.RightSidebarActiveTab },
            Cmd.none,
            None
        | SetRightSidebarActiveTab tab ->
            { model with
                RightSidebarActiveTab = tab },
            Cmd.none,
            None
        | SessionMsg msg ->
            let sessionModel, sessionCmd = Session.update msg model.Session
            { model with Session = sessionModel }, Cmd.map SessionMsg sessionCmd, None
        | ExecuteWorkspaceAction action ->
            // let sessionModel, sessionCmd =
            //     Session.logSessionTrace CommandOrigin.Human action CommandStatus.Succeeded None model.Session
            WorkspaceEngine.executeWorkspaceAction env model action
            |> function
                | (newModel, cmd) ->

                    newModel, cmd, None

        // Toggle dialogs/popovers locally or through appbar actions
        | ToggleSettingsDialog ->
            { model with
                SettingsDialogOpen = not model.SettingsDialogOpen },
            Cmd.none,
            None
        | SetSettingsDialogOpen open' ->
            { model with
                SettingsDialogOpen = open' },
            Cmd.none,
            None
        | ToggleProfilePopover ->
            { model with
                ProfilePopoverOpen = not model.ProfilePopoverOpen },
            Cmd.none,
            None
        | SetProfilePopoverOpen open' ->
            { model with
                ProfilePopoverOpen = open' },
            Cmd.none,
            None
        // Incoming message from MaterialReservation workspace
        | ReservationWorkspaceMsg msg ->
            match model.MaterialReservationState with
            | None -> model, Cmd.none, None
            | Some state ->
                let mrenv = WorkspaceEngine.makeMaterialReservationEnv env

                updateChildWithOutput
                    (fun m -> state)
                    (fun child m ->
                        { model with
                            MaterialReservationState = Some child })
                    ReservationWorkspaceMsg
                    (MaterialReservation.update mrenv)
                    handleMaterialReservationOutput
                    msg
                    model

        | MasterDataMsg msg ->
            match model.MasterDataState with
            | None -> model, Cmd.none, None
            | Some state ->
                let mdenv = WorkspaceEngine.makeMasterDataEnv env

                updateChildWithOutput
                    (fun m -> state)
                    (fun child m ->
                        { model with
                            MasterDataState = Some child })
                    MasterDataMsg
                    (MasterData.update mdenv)
                    (fun _ _ -> model, Cmd.none, None)
                    msg
                    model

    syncAppbarState newModel, cmd, out

type Component() =
    inherit ElmishComponent<AppShellModel, Message>()

    override this.View model dispatch = AppShellView.view this.Env model dispatch

    [<Parameter>]
    member val Env: AppShellEnv = Unchecked.defaultof<_> with get, set
