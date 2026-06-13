module Medhavi.Web.AppShell

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Microsoft.JSInterop
open Elmish
open Bolero
open Bolero.Html
open Medhavi.Web
open Medhavi.Web.Components
open Medhavi.Web.Stores
open Medhavi.Web.Services
open Medhavi.Web.Services.PlanningService
open Medhavi.Nexus
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.Contracts.Demand
open Medhavi.Contracts.Domain
open Radzen
open Radzen.Blazor
open Medhavi.Web.WorkspaceEngine

type Model =
    { ActivePage: Page
      SidebarExpanded: bool
      ThemePopoverOpen: bool
      Theme: UITheme
      CurrentUser: User option
      ConnectionStatus: ConnectionStatus
      Notifications: Notification list
      NotificationsOpen: bool
      ActiveOperations: ActiveOperation list
      ActivityFeedOpen: bool
      ActivityFeed: UIEventLogItem list

      // Command Palette
      CommandPaletteOpen: bool
      CommandPaletteSearchText: string
      CommandPaletteResults: Services.GlobalSearchResult list

      // Sub-workbench models
      DemandWorkbench: DemandWorkbench.Model
      SupplyWorkbench: SupplyWorkbench.Model
      CapacityWorkbench: CapacityWorkbench.Model
      PromiseWorkbench: PromiseWorkbench.Model
      ScenarioWorkbench: Pages.ScenarioWorkbench.Model

      // Scope & settings config
      CurrentScope: QueryScope
      PlantsList: Plant list
      StockingPointsList: StockingPoint list
      SettingsDialogOpen: bool
      ActiveSettingsTab: int
      ProfilePopoverOpen: bool }

type Message =
    | SetPage of Page
    | ToggleSidebar
    | SetSidebar of bool
    | ToggleThemePopover
    | SetTheme of UITheme
    | ToggleNotifications
    | MarkAllNotificationsRead
    | ClearNotifications
    | SetConnectionStatus of ConnectionStatus
    | ReceiveNotification of Notification
    | CycleUserRole
    | TriggerLogout
    | ToggleActivityFeed
    | LoadActivityFeed of UIEventLogItem list
    | ReloadAllData

    // Command Palette Messages
    | ToggleCommandPalette
    | SetCommandPaletteOpen of bool
    | CommandPaletteSearchChanged of string
    | CommandPaletteResultsLoaded of Services.GlobalSearchResult list
    | ExecuteCommandResult of Services.GlobalSearchResult

    // Operations
    | StartOperation of id: Guid * name: string
    | UpdateOperationProgress of id: Guid * progressPercentage: int * currentStage: string
    | CompleteOperation of id: Guid
    | FailOperation of id: Guid * error: string
    | DismissOperation of id: Guid
    | TriggerRunMrp
    | TriggerImportData

    // Workbench Sub-messages
    | DemandMsg of DemandWorkbench.Msg
    | SupplyMsg of SupplyWorkbench.Msg
    | CapacityMsg of CapacityWorkbench.Msg
    | PromiseMsg of PromiseWorkbench.Msg
    | ScenarioMsg of Pages.ScenarioWorkbench.Msg

    // Scope & settings messages
    | ScopeChanged of QueryScope
    | LoadedPlantsAndSps of Plant list * StockingPoint list
    | ToggleSettingsDialog
    | SetSettingsDialogOpen of bool
    | SetSettingsTab of int
    | SetScopePlant of string option
    | SetScopeStockingPoint of string option
    | ToggleProfilePopover
    | SetProfilePopoverOpen of bool
    | SetScopeHorizonStart of DateTime
    | SetScopeHorizonEnd of DateTime

let defaultScope =
    { ScenarioId = Some "BASELINE"
      PlantId = None
      StockingPointId = None
      HorizonStart = DateTime.Today.Date
      HorizonEnd = DateTime.Today.AddDays(90.0).Date }

let defaultContext =
    { CurrentScope = defaultScope
      SelectedCustomerId = None
      SelectedProductId = None
      SelectedOrderId = None
      SelectedResourceId = None
      SelectedLocationId = None }

let initModel =
    let dModel, _ =
        DemandWorkbench.Update.init (WorkspaceId(Guid.NewGuid())) defaultContext

    let sModel, _ =
        SupplyWorkbench.Update.init (WorkspaceId(Guid.NewGuid())) defaultContext

    let cModel, _ =
        CapacityWorkbench.Update.init (WorkspaceId(Guid.NewGuid())) defaultContext

    let pModel, _ =
        PromiseWorkbench.Update.init (WorkspaceId(Guid.NewGuid())) defaultContext

    let scModel = Pages.ScenarioWorkbench.init ()

    { ActivePage = Page.Dashboard
      SidebarExpanded = false
      ThemePopoverOpen = false
      Theme = UITheme.Dark
      CurrentUser =
        Some
            { Username = "Planner1"
              Email = "planner1@medhavi.com"
              Role = Role.Planner }
      ConnectionStatus = Connected
      Notifications = []
      NotificationsOpen = false
      ActiveOperations = []
      ActivityFeedOpen = false
      ActivityFeed = []

      CommandPaletteOpen = false
      CommandPaletteSearchText = ""
      CommandPaletteResults = []

      DemandWorkbench = dModel
      SupplyWorkbench = sModel
      CapacityWorkbench = cModel
      PromiseWorkbench = pModel
      ScenarioWorkbench = scModel

      CurrentScope = defaultScope
      PlantsList = []
      StockingPointsList = []
      SettingsDialogOpen = false
      ActiveSettingsTab = 0
      ProfilePopoverOpen = false }

// Layout Chrome: Connection Status Badge
let connectionBadge status =
    let titleText, badgeColor =
        match status with
        | Connected -> "Online", "var(--rz-success-color, #4caf50)"
        | Reconnecting -> "Reconnecting", "var(--rz-warning-color, #ffeb3b)"
        | Disconnected -> "Offline", "var(--rz-danger-color, #f44336)"

    div {
        attr.title titleText
        attr.style (sprintf "width: 10px; height: 10px; border-radius: 50%%; background-color: %s; display: inline-block; cursor: pointer; border: 1px solid rgba(255,255,255,0.2);" badgeColor)
    }

// Layout Chrome: Header Panel
let headerView (model: Model) (dispatch: Message -> unit) =
    Rz.stack (
        items =
            [ Rz.stack (
                  items =
                      [ Rz.sidebarToggle (click = fun _ -> dispatch ToggleSidebar)
                        Rz.label ("APS Planning", class' = "rz-text-weight-bold rz-pl-2") ],
                  orientation = Orientation.Horizontal,
                  alignItems = AlignItems.Center,
                  gap = "0"
              )

              Rz.stack (
                  items =
                      [ connectionBadge model.ConnectionStatus

                        // Scenario selector dropdown
                        div {
                            attr.style "display: flex; align-items: center; gap: 6px;"
                            label {
                                attr.style "font-size: 11px; font-weight: 500; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family); white-space: nowrap;"
                                "Scenario:"
                            }
                            select {
                                attr.``class`` "rz-dropdown"
                                attr.style "padding: 4px 8px; font-size: 12px; background-color: var(--rz-header-background-color, #2b3a4a); border: 1px solid var(--rz-border-color); color: var(--rz-text-color); border-radius: 4px; max-width: 150px; cursor: pointer;"
                                on.change (fun (args: ChangeEventArgs) ->
                                    let selected =
                                        match args.Value with
                                        | null -> None
                                        | v ->
                                            let s = string v
                                            if System.String.IsNullOrEmpty(s) then None else Some s
                                    dispatch (ScenarioMsg (Pages.ScenarioWorkbench.Msg.SelectActiveScenario selected))
                                )

                                for sc in model.ScenarioWorkbench.Scenarios do
                                    option {
                                        attr.value sc.ScenarioId
                                        attr.selected (model.CurrentScope.ScenarioId = Some sc.ScenarioId)
                                        sc.Name
                                    }
                            }
                        }

                        // Search / Command Palette toggle button
                        button {
                            attr.``class`` "theme-trigger-btn"
                            attr.title "Search workbenches and entities (Ctrl+K)"
                            on.click (fun _ -> dispatch ToggleCommandPalette)
                            Rz.icon ("search")
                        }

                        // Theme popover selector
                        div {
                            attr.``class`` "theme-container"

                            button {
                                attr.``class`` "theme-trigger-btn"
                                on.click (fun _ -> dispatch ToggleThemePopover)
                                Rz.icon ("palette")
                            }

                            if model.ThemePopoverOpen then
                                div {
                                    attr.``class`` "theme-popover"

                                    ul {
                                        attr.``class`` "theme-list"

                                        li {
                                            attr.``class`` (
                                                if model.Theme = UITheme.Standard then
                                                    "theme-item active"
                                                else
                                                    "theme-item"
                                            )

                                            on.click (fun _ -> dispatch (SetTheme UITheme.Standard))
                                            Rz.icon ("light_mode")
                                            span { "Standard" }

                                            if model.Theme = UITheme.Standard then
                                                Rz.icon ("check")
                                        }

                                        li {
                                            attr.``class`` (
                                                if model.Theme = UITheme.Dark then
                                                    "theme-item active"
                                                else
                                                    "theme-item"
                                            )

                                            on.click (fun _ -> dispatch (SetTheme UITheme.Dark))
                                            Rz.icon ("dark_mode")
                                            span { "Dark" }

                                            if model.Theme = UITheme.Dark then
                                                Rz.icon ("check")
                                        }

                                        li {
                                            attr.``class`` (
                                                if model.Theme = UITheme.StandardDark then
                                                    "theme-item active"
                                                else
                                                    "theme-item"
                                            )

                                            on.click (fun _ -> dispatch (SetTheme UITheme.StandardDark))
                                            Rz.icon ("nights_stay")
                                            span { "Standard Dark" }

                                            if model.Theme = UITheme.StandardDark then
                                                Rz.icon ("check")
                                        }
                                    }
                                }
                        }

                        // Notifications center badge
                        div {
                            attr.``class`` "notification-container"

                            let unreadCount =
                                model.Notifications
                                |> List.filter (fun n -> not n.IsRead)
                                |> List.length

                            button {
                                attr.``class`` "notification-trigger-btn"
                                on.click (fun _ -> dispatch ToggleNotifications)
                                Rz.icon ("notifications")

                                if unreadCount > 0 then
                                    span {
                                        attr.``class`` "notification-badge"
                                        string unreadCount
                                    }
                            }

                            if model.NotificationsOpen then
                                div {
                                    attr.``class`` "notification-popover"

                                    div {
                                        attr.``class`` "notification-popover-header"

                                        span {
                                            attr.``class`` "popover-title"
                                            "Notifications"
                                        }

                                        if not model.Notifications.IsEmpty then
                                            a {
                                                attr.``class`` "mark-read-link"
                                                on.click (fun _ -> dispatch MarkAllNotificationsRead)
                                                "Mark all as read"
                                            }
                                    }

                                    ul {
                                        attr.``class`` "notification-list"

                                        if model.Notifications.IsEmpty then
                                            li {
                                                attr.``class`` "notification-item empty"
                                                "No notifications"
                                            }
                                        else
                                            for n in model.Notifications do
                                                li {
                                                    attr.``class`` (
                                                        if n.IsRead then
                                                            "notification-item read"
                                                        else
                                                            "notification-item unread"
                                                    )

                                                    span {
                                                        attr.``class`` "item-title"
                                                        n.Title
                                                    }

                                                    span {
                                                        attr.``class`` "item-message"
                                                        n.Message
                                                    }

                                                    span {
                                                        attr.``class`` "item-time"
                                                        n.Timestamp.ToString("HH:mm")
                                                    }
                                                }
                                    }

                                    if not model.Notifications.IsEmpty then
                                        div {
                                            attr.``class`` "notification-popover-footer"

                                            a {
                                                attr.``class`` "clear-all-link"
                                                on.click (fun _ -> dispatch ClearNotifications)
                                                "Clear all"
                                            }
                                        }
                                }
                        }

                        // Activity log drawer button
                        button {
                            attr.``class`` "notification-trigger-btn"
                            attr.title "System Activity Log"
                            on.click (fun _ -> dispatch ToggleActivityFeed)
                            Rz.icon ("history")
                        }

                        // User Profile Dropdown Menu
                        div {
                            attr.``class`` "theme-container"

                            button {
                                attr.``class`` "theme-trigger-btn"

                                attr.style
                                    "border-radius: 4px; padding: 6px 12px; display: flex; align-items: center; gap: 8px; font-size: 13px; font-weight: 500; font-family: var(--rz-font-family); cursor: pointer;"

                                on.click (fun _ -> dispatch ToggleProfilePopover)
                                Rz.icon ("account_circle", style = "font-size: 20px;")

                                span {
                                    match model.CurrentUser with
                                    | Some u -> sprintf "%s (%A)" u.Username u.Role
                                    | None -> "Guest"
                                }

                                Rz.icon ("arrow_drop_down", style = "font-size: 16px;")
                            }

                            if model.ProfilePopoverOpen then
                                div {
                                    attr.``class`` "theme-popover"

                                    ul {
                                        attr.``class`` "theme-list"

                                        li {
                                            attr.``class`` "theme-item"

                                            on.click (fun _ ->
                                                dispatch CycleUserRole
                                                dispatch ToggleProfilePopover)

                                            Rz.icon ("swap_horiz", style = "font-size: 18px;")
                                            span { "Cycle User Role" }
                                        }

                                        li {
                                            attr.``class`` "theme-item"

                                            on.click (fun _ ->
                                                dispatch TriggerLogout
                                                dispatch ToggleProfilePopover)

                                            Rz.icon ("logout", style = "font-size: 18px;")
                                            span { "Sign Out" }
                                        }
                                    }
                                }
                        }

                        // Settings Dialog Trigger Cog
                        button {
                            attr.``class`` "theme-trigger-btn"
                            attr.title "Configuration settings"
                            on.click (fun _ -> dispatch ToggleSettingsDialog)
                            Rz.icon ("settings")
                        } ],
                  orientation = Orientation.Horizontal,
                  alignItems = AlignItems.Center,
                  gap = "10px",
                  style = "margin-left: auto;"
              ) ],
        orientation = Orientation.Horizontal,
        alignItems = AlignItems.Center,
        style = "width: 100%; padding: 0 16px; height: 50px;"
    )

// Layout Chrome: Sidebar Panel
let sidebarView (model: Model) (dispatch: Message -> unit) =
    div {
        attr.style "height: 100%; display: flex; flex-direction: column;"

        Rz.stack (
            items =
                [ Rz.button (
                      "",
                      style = ButtonStyle.Secondary,
                      icon = "west",
                      onClick = fun _ -> dispatch (SetSidebar false)
                  ) ],
            orientation = Orientation.Horizontal,
            justifyContent = JustifyContent.End,
            class' = "rz-p-2"
        )

        Rz.panelMenu (
            [ Rz.panelMenuItem ("Dashboard", icon = "home", path = "/")
              Rz.panelMenuItem ("Demand Workbench", icon = "trending_up", path = "/demand")
              Rz.panelMenuItem ("Supply Workbench", icon = "local_shipping", path = "/supply")
              Rz.panelMenuItem ("Capacity Workbench", icon = "schedule", path = "/capacity")
              Rz.panelMenuItem ("Promise Workbench", icon = "flash_on", path = "/promise")
              Rz.panelMenuItem ("Scenario Workbench", icon = "schema", path = "/scenarios") ],
            style = "flex: 1;"
        )
    }

// Layout Chrome: Breadcrumb Indicator
let breadcrumbView (model: Model) (dispatch: Message -> unit) =
    Rz.breadCrumb (
        items =
            [ Rz.breadCrumbItem ("Home", icon = "home")
              match model.ActivePage with
              | Page.Dashboard -> Rz.breadCrumbItem ("Dashboard", icon = "home")
              | Page.Demand -> Rz.breadCrumbItem ("Demand", icon = "trending_up")
              | Page.Supply -> Rz.breadCrumbItem ("Supply", icon = "local_shipping")
              | Page.Capacity -> Rz.breadCrumbItem ("Capacity", icon = "schedule")
              | Page.Promise -> Rz.breadCrumbItem ("Promise", icon = "flash_on")
              | Page.Scenarios -> Rz.breadCrumbItem ("Scenarios", icon = "schema") ]
    )

// Layout Chrome: Active Operations Overlay Card
let operationsPanel (model: Model) (dispatch: Message -> unit) =
    if List.isEmpty model.ActiveOperations then
        empty ()
    else
        div {
            attr.style
                "position: fixed; bottom: 20px; right: 20px; z-index: 1000; width: 350px; max-height: 500px; overflow-y: auto; display: flex; flex-direction: column; gap: 10px;"

            for op in model.ActiveOperations do
                comp<RadzenCard> {
                    "Style"
                    => "padding: 16px; box-shadow: 0 10px 15px -3px rgba(0,0,0,0.3), 0 4px 6px -2px rgba(0,0,0,0.05); border-radius: 8px; border: 1px solid var(--rz-border-color);"

                    Rz.stack (
                        [ div {
                              attr.style "display: flex; justify-content: space-between; align-items: center;"

                              Rz.stack (
                                  [ let icon, iconStyle =
                                        match op.State with
                                        | OperationState.Pending ->
                                            "hourglass_empty", "color: var(--rz-text-secondary-color); font-size: 16px;"
                                        | OperationState.Running _ ->
                                            "sync", "color: var(--rz-info-color); font-size: 16px;"
                                        | OperationState.Completed() ->
                                            "check_circle", "color: var(--rz-success-color); font-size: 16px;"
                                        | OperationState.Failed _ ->
                                            "error", "color: var(--rz-danger-color); font-size: 16px;"
                                        | OperationState.Cancelled ->
                                            "cancel", "color: var(--rz-text-secondary-color); font-size: 16px;"

                                    comp<RadzenIcon> {
                                        "Icon" => icon
                                        "Style" => iconStyle

                                        match op.State with
                                        | Running _ -> attr.``class`` "spin-icon"
                                        | _ -> attr.empty ()
                                    }

                                    span {
                                        attr.style
                                            "font-weight: bold; font-size: 13px; font-family: var(--rz-font-family);"

                                        op.Name
                                    } ],
                                  orientation = Orientation.Horizontal,
                                  alignItems = AlignItems.Center,
                                  gap = "6px"
                              )

                              button {
                                  attr.style
                                      "background: transparent; border: none; cursor: pointer; color: var(--rz-text-secondary-color); padding: 4px;"

                                  attr.title "Dismiss"
                                  on.click (fun _ -> dispatch (DismissOperation op.Id))
                                  Rz.icon ("close", style = "font-size: 16px;")
                              }
                          }
                          match op.State with
                          | OperationState.Pending ->
                              div {
                                  Rz.progressBar (0.0, mode = ProgressBarMode.Indeterminate)

                                  span {
                                      attr.style
                                          "font-size: 11px; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"

                                      "Pending..."
                                  }
                              }
                          | OperationState.Running(progress, stage) ->
                              div {
                                  Rz.progressBar (double progress)

                                  span {
                                      attr.style
                                          "font-size: 11px; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"

                                      sprintf "%d%% - %s" progress stage
                                  }
                              }
                          | OperationState.Completed() ->
                              div {
                                  Rz.progressBar (100.0)

                                  span {
                                      attr.style
                                          "font-size: 11px; color: var(--rz-success-color); font-weight: 500; font-family: var(--rz-font-family);"

                                      "Completed successfully"
                                  }
                              }
                          | OperationState.Failed err ->
                              div {
                                  Rz.progressBar (100.0)

                                  span {
                                      attr.style
                                          "font-size: 11px; color: var(--rz-danger-color); font-weight: 500; font-family: var(--rz-font-family); word-break: break-all;"

                                      sprintf "Failed: %s" err
                                  }
                              }
                          | OperationState.Cancelled ->
                              div {
                                  Rz.progressBar (0.0)

                                  span {
                                      attr.style
                                          "font-size: 11px; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"

                                      "Cancelled"
                                  }
                              } ],
                        gap = "8px"
                    )
                }
        }

// Layout Chrome: Activity Log slide-out Drawer
let activityFeedDrawer (model: Model) (dispatch: Message -> unit) =
    if not model.ActivityFeedOpen then
        empty ()
    else
        div {
            attr.style
                "position: fixed; top: 50px; right: 0; bottom: 0; width: 350px; z-index: 999; background-color: var(--rz-header-background-color, #2b3a4a); border-left: 1px solid var(--rz-border-color); box-shadow: -4px 0 10px rgba(0,0,0,0.25); display: flex; flex-direction: column; overflow: hidden;"

            div {
                attr.style
                    "padding: 16px; border-bottom: 1px solid var(--rz-border-color); display: flex; justify-content: space-between; align-items: center;"

                span {
                    attr.style
                        "font-weight: bold; color: var(--rz-header-color, #ffffff); font-family: var(--rz-font-family);"

                    "System Activity Log"
                }

                button {
                    attr.style
                        "background: transparent; border: none; cursor: pointer; color: var(--rz-header-color, #ffffff); padding: 4px;"

                    on.click (fun _ -> dispatch ToggleActivityFeed)
                    Rz.icon ("close")
                }
            }

            div {
                attr.style "flex: 1; overflow-y: auto; padding: 12px;"

                if List.isEmpty model.ActivityFeed then
                    div {
                        attr.style
                            "padding: 30px; text-align: center; color: var(--rz-color-text-secondary); font-size: 13px; font-family: var(--rz-font-family);"

                        "No background events logged."
                    }
                else
                    ul {
                        attr.style
                            "list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; gap: 8px;"

                        for item in model.ActivityFeed do
                            li {
                                attr.style
                                    "padding: 10px; border-radius: 6px; border: 1px solid var(--rz-border-color); background-color: rgba(255, 255, 255, 0.02); display: flex; flex-direction: column; gap: 4px;"

                                div {
                                    attr.style "display: flex; justify-content: space-between; align-items: center;"

                                    span {
                                        attr.style
                                            "font-weight: 600; font-size: 12px; color: var(--rz-header-color, #ffffff); font-family: var(--rz-font-family);"

                                        item.EventType
                                    }

                                    span {
                                        attr.style
                                            "font-size: 9px; color: var(--rz-color-text-secondary); font-family: var(--rz-font-family);"

                                        item.Timestamp.ToString("HH:mm:ss")
                                    }
                                }

                                span {
                                    attr.style
                                        "font-size: 11px; color: var(--rz-color-text-secondary); word-break: break-all; font-family: var(--rz-font-family);"

                                    sprintf "Stream: %s" item.Stream
                                }
                            }
                    }
            }
        }

// Layout Chrome: Settings/Configuration Dialog
let settingsDialog (model: Model) (dispatch: Message -> unit) =
    if not model.SettingsDialogOpen then
        empty ()
    else
        div {
            attr.``class`` "rz-dialog-mask"

            attr.style
                "position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; z-index: 10000; background-color: rgba(0,0,0,0.6); display: flex; align-items: center; justify-content: center;"

            on.click (fun (e: MouseEventArgs) -> dispatch ToggleSettingsDialog)

            div {
                attr.style
                    "width: 500px; background-color: var(--rz-dialog-background-color, #202b38); border: 1px solid var(--rz-border-color); border-radius: 8px; box-shadow: 0 10px 25px rgba(0,0,0,0.5); padding: 20px; display: flex; flex-direction: column; gap: 16px;"

                on.stopPropagation "click" true

                div {
                    attr.style
                        "display: flex; align-items: center; justify-content: space-between; border-bottom: 1px solid var(--rz-border-color); padding-bottom: 12px;"

                    h3 {
                        attr.style
                            "margin: 0; font-size: 18px; font-weight: bold; font-family: var(--rz-font-family); color: var(--rz-header-color, #ffffff);"

                        "Configuration"
                    }

                    button {
                        attr.style
                            "background: transparent; border: none; cursor: pointer; color: var(--rz-text-secondary-color);"

                        on.click (fun (e: MouseEventArgs) -> dispatch ToggleSettingsDialog)
                        Rz.icon ("close")
                    }
                }

                // Tabs header
                div {
                    attr.style
                        "display: flex; gap: 16px; border-bottom: 1px solid var(--rz-border-color); margin-bottom: 12px;"

                    button {
                        attr.style (
                            if model.ActiveSettingsTab = 0 then
                                "background: transparent; border: none; border-bottom: 2px solid var(--rz-primary-light, #3498db); padding: 8px 12px; color: var(--rz-header-color, #ffffff); font-weight: bold; cursor: pointer;"
                            else
                                "background: transparent; border: none; padding: 8px 12px; color: var(--rz-text-secondary-color); cursor: pointer;"
                        )

                        on.click (fun (e: MouseEventArgs) -> dispatch (SetSettingsTab 0))
                        "Data Visualization"
                    }

                    button {
                        attr.style (
                            if model.ActiveSettingsTab = 1 then
                                "background: transparent; border: none; border-bottom: 2px solid var(--rz-primary-light, #3498db); padding: 8px 12px; color: var(--rz-header-color, #ffffff); font-weight: bold; cursor: pointer;"
                            else
                                "background: transparent; border: none; padding: 8px 12px; color: var(--rz-text-secondary-color); cursor: pointer;"
                        )

                        on.click (fun (e: MouseEventArgs) -> dispatch (SetSettingsTab 1))
                        "Planning Horizon"
                    }
                }

                // Tab content
                if model.ActiveSettingsTab = 0 then
                    div {
                        attr.style "display: flex; flex-direction: column; gap: 16px;"

                        // Plant selector
                        div {
                            attr.style "display: flex; flex-direction: column; gap: 6px;"

                            label {
                                attr.style
                                    "font-size: 13px; font-weight: 500; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"

                                "Plant Context"
                            }

                            select {
                                attr.``class`` "rz-dropdown"

                                attr.style
                                    "width: 100%; padding: 8px; background-color: var(--rz-header-background-color, #2b3a4a); border: 1px solid var(--rz-border-color); color: var(--rz-text-color); border-radius: 4px;"

                                on.change (fun (args: ChangeEventArgs) ->
                                    let selected =
                                        match args.Value with
                                        | null -> None
                                        | v ->
                                            let s = string v
                                            if System.String.IsNullOrEmpty(s) then None else Some s

                                    dispatch (SetScopePlant selected))

                                option {
                                    attr.value ""
                                    "All Plants"
                                }

                                for p in model.PlantsList do
                                    option {
                                        attr.value p.Id

                                        attr.selected (model.CurrentScope.PlantId = Some p.Id)

                                        sprintf "%s (%s)" p.Name p.Code
                                    }
                            }
                        }

                        // Stocking Point selector
                        div {
                            attr.style "display: flex; flex-direction: column; gap: 6px;"

                            label {
                                attr.style
                                    "font-size: 13px; font-weight: 500; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"

                                "Stocking Point"
                            }

                            select {
                                attr.``class`` "rz-dropdown"

                                attr.style
                                    "width: 100%; padding: 8px; background-color: var(--rz-header-background-color, #2b3a4a); border: 1px solid var(--rz-border-color); color: var(--rz-text-color); border-radius: 4px;"

                                on.change (fun (args: ChangeEventArgs) ->
                                    let selected =
                                        match args.Value with
                                        | null -> None
                                        | v ->
                                            let s = string v
                                            if System.String.IsNullOrEmpty(s) then None else Some s

                                    dispatch (SetScopeStockingPoint selected))

                                option {
                                    attr.value ""
                                    "All Stocking Points"
                                }
                                // Filter stocking points that belong to the selected plant, or show all if no plant is selected
                                let filteredSps =
                                    match model.CurrentScope.PlantId with
                                    | None -> model.StockingPointsList
                                    | Some pid ->
                                        model.StockingPointsList
                                        |> List.filter (fun sp ->
                                            sp.PlantId.Equals(pid, StringComparison.OrdinalIgnoreCase))

                                for sp in filteredSps do
                                    option {
                                        attr.value sp.Id

                                        attr.selected (model.CurrentScope.StockingPointId = Some sp.Id)

                                        sprintf "%s (%s)" sp.Name sp.Code
                                    }
                            }
                        }
                    }
                elif model.ActiveSettingsTab = 1 then
                    div {
                        attr.style "display: flex; flex-direction: column; gap: 16px;"

                        // Horizon Start Date
                        div {
                            attr.style "display: flex; flex-direction: column; gap: 6px;"

                            label {
                                attr.style
                                    "font-size: 13px; font-weight: 500; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"

                                "Horizon Start"
                            }

                            input {
                                attr.``type`` "date"
                                attr.``class`` "rz-textbox"

                                attr.style
                                    "width: 100%; padding: 8px; background-color: var(--rz-header-background-color, #2b3a4a); border: 1px solid var(--rz-border-color); color: var(--rz-text-color); border-radius: 4px;"

                                attr.value (model.CurrentScope.HorizonStart.ToString("yyyy-MM-dd"))

                                on.change (fun (args: ChangeEventArgs) ->
                                    match args.Value with
                                    | null -> ()
                                    | v ->
                                        let s = string v
                                        if not (System.String.IsNullOrEmpty(s)) then
                                            match DateTime.TryParse(s) with
                                            | true, dt -> dispatch (SetScopeHorizonStart dt)
                                            | false, _ -> ())
                            }
                        }

                        // Horizon End Date
                        div {
                            attr.style "display: flex; flex-direction: column; gap: 6px;"

                            label {
                                attr.style
                                    "font-size: 13px; font-weight: 500; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"

                                "Horizon End"
                            }

                            input {
                                attr.``type`` "date"
                                attr.``class`` "rz-textbox"

                                attr.style
                                    "width: 100%; padding: 8px; background-color: var(--rz-header-background-color, #2b3a4a); border: 1px solid var(--rz-border-color); color: var(--rz-text-color); border-radius: 4px;"

                                attr.value (model.CurrentScope.HorizonEnd.ToString("yyyy-MM-dd"))

                                on.change (fun (args: ChangeEventArgs) ->
                                    match args.Value with
                                    | null -> ()
                                    | v ->
                                        let s = string v
                                        if not (System.String.IsNullOrEmpty(s)) then
                                            match DateTime.TryParse(s) with
                                            | true, dt -> dispatch (SetScopeHorizonEnd dt)
                                            | false, _ -> ())
                            }
                        }
                    }
            }
        }

// Layout Chrome: Master Wrapper View coordination
let layoutView (model: Model) (dispatch: Message -> unit) (content: Node) =
    Rz.rzLayout (
        items =
            [ Rz.rzHeader ([ headerView model dispatch ])
              Rz.rzSidebar (
                  items = [ sidebarView model dispatch ],
                  expanded = model.SidebarExpanded,
                  expandedChanged = (fun expanded -> dispatch (SetSidebar expanded)),
                  fullHeight = true,
                  responsive = false,
                  style = "position: absolute; z-index: 3"
              )
              Rz.rzBody (
                  [ div {
                        attr.``class`` "rz-p-4"
                        breadcrumbView model dispatch

                        div {
                            attr.style "margin-top: 16px;"

                            comp<MedhaviErrorBoundary> {
                                "OnRetry"
                                => Action(fun () -> dispatch ReloadAllData)

                                content
                            }
                        }
                    } ]
              )
              operationsPanel model dispatch
              activityFeedDrawer model dispatch
              settingsDialog model dispatch
              if model.SidebarExpanded then
                  div {
                      attr.``class`` "rz-dialog-mask"
                      attr.style "position: absolute; top: 0; left: 0; width: 100%; height: 100%; z-index: 2;"
                      on.click (fun _ -> dispatch (SetSidebar false))
                  } ],
        style = "position: relative; height: 100vh;"
    )

let dashboardView model dispatch =
    let canImport =
        match model.CurrentUser with
        | Some u ->
            match u.Role with
            | Role.Supervisor
            | Role.Manager
            | Role.Administrator -> true
            | Role.Planner -> false
            | _ -> false
        | None -> false

    div {
        attr.``class`` "p-4"

        h1 {
            attr.``class`` "rz-text-h4"
            "Dashboard 📊"
        }

        p {
            attr.``class`` "rz-color-text-secondary"
            "Welcome to the Medhavi Planning Dashboard."
        }

        div {
            attr.``class`` "rz-mt-4"

            comp<RadzenCard> {
                "Style"
                => "max-width: 420px; padding: 20px; border-radius: 8px;"

                Rz.stack (
                    [ h4 {
                          attr.``class`` "rz-text-h6 rz-m-0"
                          "Data Operations"
                      }
                      span {
                          attr.``class`` "rz-color-text-secondary"
                          "Import latest master data from external CSV files."
                      }
                      comp<RadzenButton> {
                          "Text" => "Import Master Data"
                          "Icon" => "get_app"
                          "ButtonStyle" => ButtonStyle.Primary
                          "Disabled" => not canImport

                          if canImport then
                              attr.callback "Click" (fun (e: MouseEventArgs) -> dispatch TriggerImportData)
                          else
                              attr.empty ()
                      } ],
                    gap = "12px"
                )
            }
        }
    }

let view model dispatch =
    let pageContent =
        match model.ActivePage with
        | Page.Dashboard -> dashboardView model dispatch
        | Page.Demand -> DemandWorkbench.View.render model.DemandWorkbench (fun msg -> dispatch (DemandMsg msg))
        | Page.Supply -> SupplyWorkbench.View.render model.SupplyWorkbench (fun msg -> dispatch (SupplyMsg msg))
        | Page.Capacity -> CapacityWorkbench.View.render model.CapacityWorkbench (fun msg -> dispatch (CapacityMsg msg))
        | Page.Promise -> PromiseWorkbench.View.render model.PromiseWorkbench (fun msg -> dispatch (PromiseMsg msg))
        | Page.Scenarios ->
            let canRun =
                match model.CurrentUser with
                | Some u ->
                    match u.Role with
                    | Role.Supervisor
                    | Role.Manager
                    | Role.Administrator -> true
                    | Role.Planner -> false
                    | _ -> false
                | None -> false

            Pages.ScenarioWorkbench.view
                model.ScenarioWorkbench
                (fun msg -> dispatch (ScenarioMsg msg))
                (fun () -> dispatch TriggerRunMrp)
                canRun

    div {
        link {
            attr.rel "stylesheet"

            attr.href (
                match model.Theme with
                | UITheme.Standard -> "_content/Radzen.Blazor/css/standard.css"
                | UITheme.Dark -> "_content/Radzen.Blazor/css/dark.css"
                | UITheme.StandardDark -> "_content/Radzen.Blazor/css/standard-dark.css"
            )
        }

        layoutView model dispatch pageContent

        comp<Medhavi.Web.Shell.CommandPalette> {
            "IsOpen" => model.CommandPaletteOpen
            "SearchText" => model.CommandPaletteSearchText
            "Results" => model.CommandPaletteResults

            "OnSearch"
            => (fun txt -> dispatch (CommandPaletteSearchChanged txt))

            "OnSelect"
            => (fun res -> dispatch (ExecuteCommandResult res))

            "OnClose"
            => (fun () -> dispatch (SetCommandPaletteOpen false))
        }
    }

let router = Router.infer SetPage (fun m -> m.ActivePage)

type Stores =
    { Demand: DemandStore
      Supply: SupplyStore
      Capacity: CapacityStore
      Scenario: ScenarioStore
      Activity: ActivityStore
      Promise: PromiseStore
      Engine: MedhaviEngine }

let loadDataCmd (stores: Stores) : Cmd<Message> =
    let run dispatch =
        task {
            printfn "[AppShell] Starting concurrent loadData via Cmd..."

            let loadDemand () =
                task {
                    try
                        printfn "[AppShell] Loading Demands..."
                        do! stores.Demand.Refresh()
                        let snapshot = stores.Demand.GetSnapshot()
                        printfn "[AppShell] Demands loaded successfully, count: %d" snapshot.Length
                        dispatch (DemandMsg(DemandWorkbench.Msg.LoadedSummary snapshot))
                    with ex ->
                        printfn "[AppShell] Demands loading failed: %s" ex.Message
                        dispatch (DemandMsg(DemandWorkbench.Msg.LoadFailed ex.Message))
                }

            let loadSupply () =
                task {
                    try
                        printfn "[AppShell] Loading Supplies..."
                        do! stores.Supply.Refresh()
                        let snapshot = stores.Supply.GetSnapshot()
                        printfn "[AppShell] Supplies loaded successfully, count: %d" snapshot.Length
                        dispatch (SupplyMsg(SupplyWorkbench.Msg.LoadedSummary snapshot))
                    with ex ->
                        printfn "[AppShell] Supplies loading failed: %s" ex.Message
                        dispatch (SupplyMsg(SupplyWorkbench.Msg.LoadFailed ex.Message))
                }

            let loadCapacity () =
                task {
                    try
                        printfn "[AppShell] Loading Capacities..."
                        do! stores.Capacity.Refresh()
                        let snapshot = stores.Capacity.GetSnapshot()
                        printfn "[AppShell] Capacities loaded successfully, count: %d" snapshot.Length
                        dispatch (CapacityMsg(CapacityWorkbench.Msg.LoadedSummary snapshot))
                    with ex ->
                        printfn "[AppShell] Capacities loading failed: %s" ex.Message
                        dispatch (CapacityMsg(CapacityWorkbench.Msg.LoadFailed ex.Message))
                }

            let loadScenario () =
                task {
                    try
                        printfn "[AppShell] Loading Scenarios..."
                        do! stores.Scenario.Refresh()
                        let snapshot = stores.Scenario.GetSnapshot()
                        printfn "[AppShell] Scenarios loaded successfully, count: %d" snapshot.Length
                        dispatch (ScenarioMsg(Pages.ScenarioWorkbench.LoadScenarios snapshot))
                    with ex ->
                        printfn "[AppShell] Scenarios loading failed: %s" ex.Message
                        dispatch (ScenarioMsg(Pages.ScenarioWorkbench.ShowError ex.Message))
                }

            let loadActivity () =
                task {
                    try
                        printfn "[AppShell] Loading Activity Feed..."
                        do! stores.Activity.Refresh()
                        let snapshot = stores.Activity.GetSnapshot()
                        printfn "[AppShell] Activity feed loaded, count: %d" snapshot.Length
                        dispatch (LoadActivityFeed snapshot)
                    with ex ->
                        printfn "[AppShell] Activity feed loading failed: %s" ex.Message
                }

            let loadPlantsAndSps () =
                task {
                    try
                        printfn "[AppShell] Loading Plants and Stocking Points..."
                        let! plants = stores.Engine.GetPlants()
                        let! sps = stores.Engine.GetStockingPoints()
                        dispatch (LoadedPlantsAndSps(plants, sps))
                    with ex ->
                        printfn "[AppShell] Failed to load Plants and Stocking Points: %s" ex.Message
                }

            do!
                Task.WhenAll(
                    [| loadDemand ()
                       loadSupply ()
                       loadCapacity ()
                       loadScenario ()
                       loadActivity ()
                       loadPlantsAndSps () |]
                )
                :> Task

            printfn "[AppShell] loadData via Cmd finished."
        }
        |> Async.AwaitTask
        |> Async.Ignore
        |> Async.Start

    [ run ]

let subscribeToStoresCmd
    (setSubs: IDisposable list -> unit)
    (stores: Stores)
    (context: WorkspaceContextService)
    : Cmd<Message> =
    let run dispatch =
        let dSub =
            stores.Demand.Subscribe(fun () ->
                printfn "[AppShell] DemandStore cache updated, dispatching to Workbench"
                dispatch (DemandMsg(DemandWorkbench.Msg.LoadedSummary(stores.Demand.GetSnapshot()))))

        let sSub =
            stores.Supply.Subscribe(fun () ->
                printfn "[AppShell] SupplyStore cache updated, dispatching to Workbench"
                dispatch (SupplyMsg(SupplyWorkbench.Msg.LoadedSummary(stores.Supply.GetSnapshot()))))

        let cSub =
            stores.Capacity.Subscribe(fun () ->
                printfn "[AppShell] CapacityStore cache updated, dispatching to Workbench"
                dispatch (CapacityMsg(CapacityWorkbench.Msg.LoadedSummary(stores.Capacity.GetSnapshot()))))

        let scSub =
            stores.Scenario.Subscribe(fun () ->
                printfn "[AppShell] ScenarioStore cache updated, dispatching to Workbench"
                dispatch (ScenarioMsg(Pages.ScenarioWorkbench.LoadScenarios(stores.Scenario.GetSnapshot()))))

        let actSub =
            stores.Activity.Subscribe(fun () ->
                printfn "[AppShell] ActivityStore cache updated, dispatching to Shell"
                dispatch (LoadActivityFeed(stores.Activity.GetSnapshot())))

        // Workspace scope change listener
        let contextSub =
            context.OnScopeChanged.Subscribe(fun newScope ->
                printfn "[AppShell] Workspace scope changed, refreshing all stores"
                dispatch (ScopeChanged newScope)

                task {
                    do! stores.Demand.SetScope(newScope)
                    do! stores.Supply.SetScope(newScope)
                    do! stores.Capacity.SetScope(newScope)
                    dispatch ReloadAllData
                }
                |> ignore)

        // Event invalidations
        let busSub =
            DomainEventBus.Subscribe<obj>(fun ev ->
                let typeName = ev.GetType().Name
                printfn "[AppShell] Live event received from EventBus: %s -> %A" typeName ev

                if typeName.Contains("DemandLine") then
                    stores.Demand.Refresh() |> ignore
                    stores.Activity.Refresh() |> ignore

                    let notif =
                        { Id = Guid.NewGuid()
                          Title = "Demand Event"
                          Message = sprintf "Demand change received: %s" typeName
                          Timestamp = DateTime.Now
                          IsRead = false }

                    dispatch (ReceiveNotification notif)
                elif
                    typeName.Contains("SupplyOrder")
                    || typeName.Contains("Reservation")
                then
                    stores.Supply.Refresh() |> ignore
                    stores.Activity.Refresh() |> ignore

                    let notif =
                        { Id = Guid.NewGuid()
                          Title = "Supply Event"
                          Message = sprintf "Supply change received: %s" typeName
                          Timestamp = DateTime.Now
                          IsRead = false }

                    dispatch (ReceiveNotification notif)
                elif
                    typeName.Contains("Capacity")
                    || typeName.Contains("Operation")
                then
                    stores.Capacity.Refresh() |> ignore
                    stores.Activity.Refresh() |> ignore

                    let notif =
                        { Id = Guid.NewGuid()
                          Title = "Capacity Event"
                          Message = sprintf "Capacity/Operation change received: %s" typeName
                          Timestamp = DateTime.Now
                          IsRead = false }

                    dispatch (ReceiveNotification notif))

        setSubs ([ dSub; sSub; cSub; scSub; actSub; contextSub; busSub ])

    [ run ]

let runMrpCmd (service: PlanningCommandService) : Cmd<Message> =
    let run dispatch =
        task {
            let opId = Guid.NewGuid()
            dispatch (StartOperation(opId, "MRP Scheduling Run"))

            let onProgress pct stage = dispatch (UpdateOperationProgress(opId, pct, stage))

            let! res = service.RunMrp(onProgress)

            match res with
            | Ok _ ->
                dispatch (CompleteOperation opId)

                let notif =
                    { Id = Guid.NewGuid()
                      Title = "MRP Completed"
                      Message = "Baseline MRP scheduling run completed successfully."
                      Timestamp = DateTime.Now
                      IsRead = false }

                dispatch (ReceiveNotification notif)
            | Error err ->
                dispatch (FailOperation(opId, err))

                let notif =
                    { Id = Guid.NewGuid()
                      Title = "MRP Failed"
                      Message = sprintf "MRP baseline run failed: %s" err
                      Timestamp = DateTime.Now
                      IsRead = false }

                dispatch (ReceiveNotification notif)
        }
        |> Async.AwaitTask
        |> Async.Ignore
        |> Async.Start

    [ run ]

let importDataCmd (service: PlanningCommandService) : Cmd<Message> =
    let run dispatch =
        task {
            let opId = Guid.NewGuid()
            dispatch (StartOperation(opId, "Import Master Data"))

            let onProgress pct stage = dispatch (UpdateOperationProgress(opId, pct, stage))

            let! res = service.TriggerImport(onProgress)

            match res with
            | Ok _ ->
                dispatch (CompleteOperation opId)

                let notif =
                    { Id = Guid.NewGuid()
                      Title = "Import Completed"
                      Message = "Master data synchronized successfully."
                      Timestamp = DateTime.Now
                      IsRead = false }

                dispatch (ReceiveNotification notif)
            | Error err ->
                dispatch (FailOperation(opId, err))

                let notif =
                    { Id = Guid.NewGuid()
                      Title = "Import Failed"
                      Message = sprintf "Master data import failed: %s" err
                      Timestamp = DateTime.Now
                      IsRead = false }

                dispatch (ReceiveNotification notif)
        }
        |> Async.AwaitTask
        |> Async.Ignore
        |> Async.Start

    [ run ]

let update
    (stores: Stores)
    (planningService: PlanningCommandService)
    (workspaceContext: WorkspaceContextService)
    (searchService: Services.GlobalSearchService)
    (msg: Message)
    (model: Model)
    : Model * Cmd<Message> =
    match msg with
    | TriggerRunMrp -> model, runMrpCmd planningService
    | TriggerImportData -> model, importDataCmd planningService
    | ReloadAllData -> model, loadDataCmd stores
    | ToggleCommandPalette ->
        { model with
            CommandPaletteOpen = not model.CommandPaletteOpen
            CommandPaletteSearchText = ""
            CommandPaletteResults = [] },
        Cmd.none
    | SetCommandPaletteOpen open' ->
        { model with
            CommandPaletteOpen = open'
            CommandPaletteSearchText = ""
            CommandPaletteResults = [] },
        Cmd.none
    | CommandPaletteSearchChanged text ->
        let searchCmd =
            Cmd.OfAsync.either
                (fun () ->
                    async {
                        let query =
                            { SearchText = text
                              MaxResults = 10
                              Context = None }

                        return! searchService.SearchAsync(query)
                    })
                ()
                CommandPaletteResultsLoaded
                (fun _ -> CommandPaletteResultsLoaded [])

        { model with
            CommandPaletteSearchText = text },
        searchCmd
    | CommandPaletteResultsLoaded results ->
        { model with
            CommandPaletteResults = results },
        Cmd.none
    | ExecuteCommandResult result ->
        match result with
        | Services.WorkbenchResult(kind, _) ->
            let nextPage =
                match kind with
                | DemandWorkspace -> Page.Demand
                | SupplyWorkspace -> Page.Supply
                | CapacityWorkspace -> Page.Capacity
                | ScenarioWorkspace -> Page.Scenarios
                | _ -> Page.Dashboard

            { model with
                ActivePage = nextPage
                CommandPaletteOpen = false },
            Cmd.none
        | Services.EntityResult(EntityRef(entityType, entityId), _) ->
            match entityType.ToLower() with
            | "demandline"
            | "demand" ->
                let selectCmd =
                    match model.DemandWorkbench.SummaryData with
                    | Panels.Loaded items ->
                        items
                        |> List.tryFind (fun d ->
                            d.DemandLineId = entityId
                            || d.DemandOrderId = entityId)
                        |> Option.map (fun row -> Cmd.ofMsg (DemandMsg(DemandWorkbench.Msg.RowSelected row)))
                        |> Option.defaultValue Cmd.none
                    | _ -> Cmd.none

                { model with
                    ActivePage = Page.Demand
                    CommandPaletteOpen = false },
                selectCmd
            | "sku"
            | "product" ->
                let updatedDemandModel =
                    { model.DemandWorkbench with
                        SearchText = entityId
                        PendingSearchText = entityId }

                { model with
                    ActivePage = Page.Demand
                    DemandWorkbench = updatedDemandModel
                    CommandPaletteOpen = false },
                Cmd.none
            | "stockingpoint"
            | "location"
            | "plant" ->
                let updatedDemandModel =
                    { model.DemandWorkbench with
                        SearchText = entityId
                        PendingSearchText = entityId }

                { model with
                    ActivePage = Page.Demand
                    DemandWorkbench = updatedDemandModel
                    CommandPaletteOpen = false },
                Cmd.none
            | "supplyorder" ->
                { model with
                    ActivePage = Page.Supply
                    CommandPaletteOpen = false },
                Cmd.none
            | "resource" ->
                { model with
                    ActivePage = Page.Capacity
                    CommandPaletteOpen = false },
                Cmd.none
            | _ ->
                { model with
                    CommandPaletteOpen = false },
                Cmd.none
        | Services.CapabilityResult _ ->
            { model with
                CommandPaletteOpen = false },
            Cmd.none
    | ScenarioMsg(Pages.ScenarioWorkbench.Msg.SelectActiveScenario sIdOpt) ->
        let currentScope = workspaceContext.CurrentScope

        let newScope =
            { currentScope with
                ScenarioId = sIdOpt }

        workspaceContext.SetScope(newScope) |> ignore

        let subModel =
            Pages.ScenarioWorkbench.update (Pages.ScenarioWorkbench.SelectActiveScenario sIdOpt) model.ScenarioWorkbench

        { model with
            ScenarioWorkbench = subModel },
        Cmd.none
    | ScenarioMsg(Pages.ScenarioWorkbench.Msg.SelectCompareScenario sIdOpt) ->
        let subModel =
            Pages.ScenarioWorkbench.update
                (Pages.ScenarioWorkbench.SelectCompareScenario sIdOpt)
                model.ScenarioWorkbench

        { model with
            ScenarioWorkbench = subModel },
        Cmd.none
    | SetPage page ->
        { model with
            ActivePage = page
            SidebarExpanded = false },
        Cmd.none
    | ToggleSidebar ->
        { model with
            SidebarExpanded = not model.SidebarExpanded },
        Cmd.none
    | SetSidebar expanded ->
        { model with
            SidebarExpanded = expanded },
        Cmd.none
    | ToggleThemePopover ->
        { model with
            ThemePopoverOpen = not model.ThemePopoverOpen },
        Cmd.none
    | SetTheme theme ->
        { model with
            Theme = theme
            ThemePopoverOpen = false },
        Cmd.none
    | ToggleNotifications ->
        { model with
            NotificationsOpen = not model.NotificationsOpen },
        Cmd.none
    | MarkAllNotificationsRead ->
        let readList =
            model.Notifications
            |> List.map (fun n -> { n with IsRead = true })

        { model with Notifications = readList }, Cmd.none
    | ClearNotifications -> { model with Notifications = [] }, Cmd.none
    | SetConnectionStatus status -> { model with ConnectionStatus = status }, Cmd.none
    | ReceiveNotification n ->
        let currentList = n :: model.Notifications

        let trimmedList =
            if currentList.Length > 50 then
                List.take 50 currentList
            else
                currentList

        { model with
            Notifications = trimmedList },
        Cmd.none
    | CycleUserRole ->
        let nextRole =
            match model.CurrentUser with
            | Some u ->
                match u.Role with
                | Role.Planner -> Role.Supervisor
                | Role.Supervisor -> Role.Manager
                | Role.Manager -> Role.Administrator
                | Role.Administrator -> Role.Planner
                | _ -> Role.Planner
            | None -> Role.Planner

        let updatedUser =
            model.CurrentUser
            |> Option.map (fun u -> { u with Role = nextRole })

        { model with CurrentUser = updatedUser }, Cmd.none
    | TriggerLogout -> model, Cmd.none
    | ToggleActivityFeed ->
        { model with
            ActivityFeedOpen = not model.ActivityFeedOpen },
        Cmd.none
    | LoadActivityFeed feed -> { model with ActivityFeed = feed }, Cmd.none
    | StartOperation(id, name) ->
        let op =
            { Id = id
              Name = name
              State = OperationState.Running(0, "Initializing") }

        { model with
            ActiveOperations = op :: model.ActiveOperations },
        Cmd.none
    | UpdateOperationProgress(id, progress, stage) ->
        let ops =
            model.ActiveOperations
            |> List.map (fun op ->
                if op.Id = id then
                    { op with
                        State = OperationState.Running(progress, stage) }
                else
                    op)

        { model with ActiveOperations = ops }, Cmd.none
    | CompleteOperation id ->
        let ops =
            model.ActiveOperations
            |> List.map (fun op ->
                if op.Id = id then
                    { op with
                        State = OperationState.Completed() }
                else
                    op)

        { model with ActiveOperations = ops }, Cmd.none
    | FailOperation(id, err) ->
        let ops =
            model.ActiveOperations
            |> List.map (fun op ->
                if op.Id = id then
                    { op with
                        State = OperationState.Failed err }
                else
                    op)

        { model with ActiveOperations = ops }, Cmd.none
    | DismissOperation id ->
        let ops =
            model.ActiveOperations
            |> List.filter (fun op -> op.Id <> id)

        { model with ActiveOperations = ops }, Cmd.none
    | DemandMsg subMsg ->
        let subModel, subCmd =
            DemandWorkbench.Update.update stores.Demand stores.Scenario subMsg model.DemandWorkbench

        { model with
            DemandWorkbench = subModel },
        Cmd.map DemandMsg subCmd
    | SupplyMsg subMsg ->
        let subModel, subCmd =
            SupplyWorkbench.Update.update stores.Supply subMsg model.SupplyWorkbench

        { model with
            SupplyWorkbench = subModel },
        Cmd.map SupplyMsg subCmd
    | CapacityMsg subMsg ->
        let subModel, subCmd =
            CapacityWorkbench.Update.update stores.Capacity subMsg model.CapacityWorkbench

        { model with
            CapacityWorkbench = subModel },
        Cmd.map CapacityMsg subCmd
    | PromiseMsg subMsg ->
        let subModel, subCmd =
            PromiseWorkbench.Update.update stores.Promise subMsg model.PromiseWorkbench

        { model with
            PromiseWorkbench = subModel },
        Cmd.map PromiseMsg subCmd
    | ScopeChanged scope -> { model with CurrentScope = scope }, Cmd.none
    | LoadedPlantsAndSps(plants, sps) ->
        { model with
            PlantsList = plants
            StockingPointsList = sps },
        Cmd.none
    | ToggleSettingsDialog ->
        { model with
            SettingsDialogOpen = not model.SettingsDialogOpen },
        Cmd.none
    | SetSettingsDialogOpen open' ->
        { model with
            SettingsDialogOpen = open' },
        Cmd.none
    | SetSettingsTab tab -> { model with ActiveSettingsTab = tab }, Cmd.none
    | SetScopePlant plantIdOpt ->
        let currentScope = workspaceContext.CurrentScope

        let newScope =
            { currentScope with
                PlantId = plantIdOpt
                StockingPointId = None }

        workspaceContext.SetScope(newScope) |> ignore
        model, Cmd.none
    | SetScopeStockingPoint spIdOpt ->
        let currentScope = workspaceContext.CurrentScope

        let newScope =
            { currentScope with
                StockingPointId = spIdOpt }

        workspaceContext.SetScope(newScope) |> ignore
        model, Cmd.none
    | ToggleProfilePopover ->
        { model with
            ProfilePopoverOpen = not model.ProfilePopoverOpen },
        Cmd.none
    | SetProfilePopoverOpen open' ->
        { model with
            ProfilePopoverOpen = open' },
        Cmd.none
    | SetScopeHorizonStart dt ->
        let currentScope = workspaceContext.CurrentScope
        let newScope = { currentScope with HorizonStart = dt }
        workspaceContext.SetScope(newScope) |> ignore
        model, Cmd.none
    | SetScopeHorizonEnd dt ->
        let currentScope = workspaceContext.CurrentScope
        let newScope = { currentScope with HorizonEnd = dt }
        workspaceContext.SetScope(newScope) |> ignore
        model, Cmd.none
    | ScenarioMsg(Pages.ScenarioWorkbench.Msg.SubmitCreateScenario) ->
        let name = model.ScenarioWorkbench.NewScenarioName
        let scType = model.ScenarioWorkbench.NewScenarioType
        let parentId = model.ScenarioWorkbench.NewScenarioParentId

        let createCmd =
            Cmd.OfAsync.either
                (fun () ->
                    async {
                        let! res =
                            stores.Scenario.CreateScenario(name, scType, parentId)
                            |> Async.AwaitTask

                        return res
                    })
                ()
                (fun res ->
                    match res with
                    | Ok _ -> ScenarioMsg(Pages.ScenarioWorkbench.Msg.CloseCreateForm)
                    | Error err -> ScenarioMsg(Pages.ScenarioWorkbench.Msg.ShowError err))
                (fun ex -> ScenarioMsg(Pages.ScenarioWorkbench.Msg.ShowError ex.Message))

        let updatedSubModel =
            Pages.ScenarioWorkbench.update (Pages.ScenarioWorkbench.Msg.SetLoading true) model.ScenarioWorkbench

        { model with
            ScenarioWorkbench = updatedSubModel },
        createCmd


    | ScenarioMsg(Pages.ScenarioWorkbench.Msg.RemoveOverride ov) ->
        match model.ScenarioWorkbench.ActiveScenarioId with
        | None -> model, Cmd.none
        | Some scenId ->
            let removeCmd =
                Cmd.OfAsync.either
                    (fun () ->
                        async {
                            let! res = stores.Scenario.RemoveOverride(scenId, ov) |> Async.AwaitTask
                            return res
                        })
                    ()
                    (fun res ->
                        match res with
                        | Ok _ -> ReloadAllData
                        | Error err -> ScenarioMsg(Pages.ScenarioWorkbench.Msg.ShowError err))
                    (fun ex -> ScenarioMsg(Pages.ScenarioWorkbench.Msg.ShowError ex.Message))
            model, removeCmd

    | ScenarioMsg(Pages.ScenarioWorkbench.Msg.SubmitForApproval) ->
        match model.ScenarioWorkbench.ActiveScenarioId with
        | None -> model, Cmd.none
        | Some scenId ->
            let submitCmd =
                Cmd.OfAsync.either
                    (fun () ->
                        async {
                            let! res = stores.Scenario.SubmitForApproval(scenId) |> Async.AwaitTask
                            return res
                        })
                    ()
                    (fun res ->
                        match res with
                        | Ok _ -> ReloadAllData
                        | Error err -> ScenarioMsg(Pages.ScenarioWorkbench.Msg.ShowError err))
                    (fun ex -> ScenarioMsg(Pages.ScenarioWorkbench.Msg.ShowError ex.Message))
            let updatedSubModel = Pages.ScenarioWorkbench.update (Pages.ScenarioWorkbench.Msg.SetLoading true) model.ScenarioWorkbench
            { model with ScenarioWorkbench = updatedSubModel }, submitCmd

    | ScenarioMsg(Pages.ScenarioWorkbench.Msg.ApproveScenario) ->
        match model.ScenarioWorkbench.ActiveScenarioId with
        | None -> model, Cmd.none
        | Some scenId ->
            let approveCmd =
                Cmd.OfAsync.either
                    (fun () ->
                        async {
                            let! res = stores.Scenario.ApproveScenario(scenId) |> Async.AwaitTask
                            return res
                        })
                    ()
                    (fun res ->
                        match res with
                        | Ok _ -> ReloadAllData
                        | Error err -> ScenarioMsg(Pages.ScenarioWorkbench.Msg.ShowError err))
                    (fun ex -> ScenarioMsg(Pages.ScenarioWorkbench.Msg.ShowError ex.Message))
            let updatedSubModel = Pages.ScenarioWorkbench.update (Pages.ScenarioWorkbench.Msg.SetLoading true) model.ScenarioWorkbench
            { model with ScenarioWorkbench = updatedSubModel }, approveCmd

    | ScenarioMsg(Pages.ScenarioWorkbench.Msg.SubmitRejectScenario) ->
        match model.ScenarioWorkbench.RejectingScenarioId with
        | None -> model, Cmd.none
        | Some scenId ->
            let reason = model.ScenarioWorkbench.RejectReason
            let rejectCmd =
                Cmd.OfAsync.either
                    (fun () ->
                        async {
                            let! res = stores.Scenario.RejectScenario(scenId, reason) |> Async.AwaitTask
                            return res
                        })
                    ()
                    (fun res ->
                        match res with
                        | Ok _ -> ScenarioMsg(Pages.ScenarioWorkbench.Msg.CloseRejectForm)
                        | Error err -> ScenarioMsg(Pages.ScenarioWorkbench.Msg.ShowError err))
                    (fun ex -> ScenarioMsg(Pages.ScenarioWorkbench.Msg.ShowError ex.Message))
            let updatedSubModel = Pages.ScenarioWorkbench.update (Pages.ScenarioWorkbench.Msg.SetLoading true) model.ScenarioWorkbench
            { model with ScenarioWorkbench = updatedSubModel }, rejectCmd

    | ScenarioMsg(Pages.ScenarioWorkbench.Msg.SubmitPublishScenario) ->
        match model.ScenarioWorkbench.ActiveScenarioId with
        | None -> model, Cmd.none
        | Some scenId ->
            let reason = if String.IsNullOrWhiteSpace(model.ScenarioWorkbench.PublishReason) then None else Some model.ScenarioWorkbench.PublishReason
            let publishCmd =
                Cmd.OfAsync.either
                    (fun () ->
                        async {
                            let! res = stores.Scenario.PublishScenario(scenId, reason) |> Async.AwaitTask
                            return res
                        })
                    ()
                    (fun res ->
                        match res with
                        | Ok _ -> ScenarioMsg(Pages.ScenarioWorkbench.Msg.ClosePublishForm)
                        | Error err -> ScenarioMsg(Pages.ScenarioWorkbench.Msg.ShowError err))
                    (fun ex -> ScenarioMsg(Pages.ScenarioWorkbench.Msg.ShowError ex.Message))
            let updatedSubModel = Pages.ScenarioWorkbench.update (Pages.ScenarioWorkbench.Msg.SetLoading true) model.ScenarioWorkbench
            { model with ScenarioWorkbench = updatedSubModel }, publishCmd

    | ScenarioMsg(Pages.ScenarioWorkbench.Msg.RollbackScenario publishId) ->
        let rollbackCmd =
            Cmd.OfAsync.either
                (fun () ->
                    async {
                        let! res = stores.Scenario.RollbackScenario(publishId) |> Async.AwaitTask
                        return res
                    })
                ()
                (fun res ->
                    match res with
                    | Ok _ -> ReloadAllData
                    | Error err -> ScenarioMsg(Pages.ScenarioWorkbench.Msg.ShowError err))
                (fun ex -> ScenarioMsg(Pages.ScenarioWorkbench.Msg.ShowError ex.Message))
        let updatedSubModel = Pages.ScenarioWorkbench.update (Pages.ScenarioWorkbench.Msg.SetLoading true) model.ScenarioWorkbench
        { model with ScenarioWorkbench = updatedSubModel }, rollbackCmd

    | ScenarioMsg(Pages.ScenarioWorkbench.Msg.ClosePublishForm) ->
        let subModel = Pages.ScenarioWorkbench.update Pages.ScenarioWorkbench.Msg.ClosePublishForm model.ScenarioWorkbench
        { model with ScenarioWorkbench = subModel }, Cmd.ofMsg ReloadAllData

    | ScenarioMsg(Pages.ScenarioWorkbench.Msg.CloseRejectForm) ->
        let subModel = Pages.ScenarioWorkbench.update Pages.ScenarioWorkbench.Msg.CloseRejectForm model.ScenarioWorkbench
        { model with ScenarioWorkbench = subModel }, Cmd.ofMsg ReloadAllData

    | ScenarioMsg subMsg ->
        let subModel = Pages.ScenarioWorkbench.update subMsg model.ScenarioWorkbench

        { model with
            ScenarioWorkbench = subModel },
        Cmd.none

[<BoleroRenderModeAttribute(BoleroRenderMode.Server)>]
type AppShellComponent() =
    inherit ProgramComponent<Model, Message>()

    let mutable subs: IDisposable list = []
    let mutable dotNetRef: DotNetObjectReference<AppShellComponent> option = None

    member this.SetSubs(s: IDisposable list) =
        for sub in subs do
            sub.Dispose()

        subs <- s

    [<Parameter>]
    member val CurrentUser: User = Unchecked.defaultof<User> with get, set

    [<Parameter>]
    member val OnLogout: EventCallback = EventCallback.Empty with get, set

    [<Inject>]
    member val DemandStore = Unchecked.defaultof<DemandStore> with get, set

    [<Inject>]
    member val SupplyStore = Unchecked.defaultof<SupplyStore> with get, set

    [<Inject>]
    member val CapacityStore = Unchecked.defaultof<CapacityStore> with get, set

    [<Inject>]
    member val ScenarioStore = Unchecked.defaultof<ScenarioStore> with get, set

    [<Inject>]
    member val ActivityStore = Unchecked.defaultof<ActivityStore> with get, set

    [<Inject>]
    member val PromiseStore = Unchecked.defaultof<PromiseStore> with get, set

    [<Inject>]
    member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

    [<Inject>]
    member val PlanningService = Unchecked.defaultof<PlanningCommandService> with get, set

    [<Inject>]
    member val WorkspaceContext = Unchecked.defaultof<WorkspaceContextService> with get, set

    [<Inject>]
    member val SearchService = Unchecked.defaultof<Services.GlobalSearchService> with get, set

    [<Inject>]
    member val Engine = Unchecked.defaultof<MedhaviEngine> with get, set

    override this.Program =
        let stores =
            { Demand = this.DemandStore
              Supply = this.SupplyStore
              Capacity = this.CapacityStore
              Scenario = this.ScenarioStore
              Activity = this.ActivityStore
              Promise = this.PromiseStore
              Engine = this.Engine }

        let init () =
            let model = initModel

            let updatedModel =
                { model with
                    CurrentUser = Some this.CurrentUser }

            let loadCmd = loadDataCmd stores
            let subCmd = subscribeToStoresCmd this.SetSubs stores this.WorkspaceContext
            updatedModel, Cmd.batch [ loadCmd; subCmd ]

        let update msg model =
            match msg with
            | TriggerLogout ->
                this.OnLogout.InvokeAsync() |> ignore
                model, Cmd.none
            | _ -> update stores this.PlanningService this.WorkspaceContext this.SearchService msg model

        Program.mkProgram (fun _ -> init ()) update view
        |> Program.withRouter router

    override this.OnAfterRender(firstRender) =
        if firstRender then
            let reference = DotNetObjectReference.Create(this)
            dotNetRef <- Some reference

            this.JSRuntime
                .InvokeVoidAsync("setupConnectionListener", reference)
                .AsTask()
            |> ignore

            this.JSRuntime
                .InvokeVoidAsync("setupKeyboardListener", reference)
                .AsTask()
            |> ignore

    [<JSInvokable>]
    member this.ToggleCommandPalette() = this.Dispatch(ToggleCommandPalette)

    [<JSInvokable>]
    member this.UpdateConnectionStatus(status: string) =
        let connStatus =
            match status with
            | "Connected" -> Connected
            | "Disconnected" -> Disconnected
            | _ -> Disconnected

        printfn "[AppShell] JS network status event received: %s -> %A" status connStatus
        this.Dispatch(SetConnectionStatus connStatus)

    interface IDisposable with
        member _.Dispose() =
            for sub in subs do
                sub.Dispose()

            match dotNetRef with
            | Some r -> r.Dispose()
            | None -> ()
