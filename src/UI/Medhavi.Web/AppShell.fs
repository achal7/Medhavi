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
open Medhavi.Demand.Domain.DemandLineAgg
open Medhavi.Supply.Domain.SupplyOrderAgg
open Radzen
open Radzen.Blazor


type Model = {
    ActivePage: Page
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
    
    // Sub-workbench models
    DemandWorkbench: Pages.DemandWorkbench.Model
    SupplyWorkbench: Pages.SupplyWorkbench.Model
    CapacityWorkbench: Pages.CapacityWorkbench.Model
    ScenarioWorkbench: Pages.ScenarioWorkbench.Model
}

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
    
    // Operations
    | StartOperation of id: Guid * name: string
    | UpdateOperationProgress of id: Guid * progressPercentage: int * currentStage: string
    | CompleteOperation of id: Guid
    | FailOperation of id: Guid * error: string
    | DismissOperation of id: Guid
    | TriggerRunMrp
    | TriggerImportData
    
    // Workbench Sub-messages
    | DemandMsg of Pages.DemandWorkbench.Msg
    | SupplyMsg of Pages.SupplyWorkbench.Msg
    | CapacityMsg of Pages.CapacityWorkbench.Msg
    | ScenarioMsg of Pages.ScenarioWorkbench.Msg

let initModel = 
    let dModel = Pages.DemandWorkbench.init()
    let sModel = Pages.SupplyWorkbench.init()
    let cModel = Pages.CapacityWorkbench.init()
    let scModel = Pages.ScenarioWorkbench.init()
    {
        ActivePage = Page.Dashboard
        SidebarExpanded = false
        ThemePopoverOpen = false
        Theme = UITheme.Dark
        CurrentUser = Some { Username = "Planner1"; Email = "planner1@medhavi.com"; Role = Role.Planner }
        ConnectionStatus = Connected
        Notifications = []
        NotificationsOpen = false
        ActiveOperations = []
        ActivityFeedOpen = false
        ActivityFeed = []
        
        DemandWorkbench = dModel
        SupplyWorkbench = sModel
        CapacityWorkbench = cModel
        ScenarioWorkbench = scModel
    }

// Layout Chrome: Connection Status Badge
let connectionBadge status =
    let badgeText, badgeClass, icon =
        match status with
        | Connected -> "Online", "rz-background-color-success rz-color-white", "check_circle"
        | Reconnecting -> "Reconnecting", "rz-background-color-warning rz-color-black", "sync"
        | Disconnected -> "Offline", "rz-background-color-danger rz-color-white", "error"
    div {
        attr.``class`` (sprintf "rz-border-radius-4 rz-px-2 rz-py-1 rz-display-flex rz-align-items-center %s" badgeClass)
        attr.style "gap: 4px; font-size: 11px; font-weight: 600; font-family: var(--rz-font-family); line-height: 1;"
        Rz.icon(icon, style = "font-size: 14px;")
        span { badgeText }
    }

// Layout Chrome: Header Panel
let headerView (model: Model) (dispatch: Message -> unit) =
    Rz.stack(
        items = [
            Rz.stack(
                items = [
                    Rz.sidebarToggle(click = fun _ -> dispatch ToggleSidebar)
                    Rz.label("APS Planning", class' = "rz-text-weight-bold rz-pl-2")
                ],
                orientation = Orientation.Horizontal,
                alignItems = AlignItems.Center,
                gap = "0"
            )
            
            Rz.stack(
                items = [
                    connectionBadge model.ConnectionStatus
                    
                    // Theme popover selector
                    div {
                        attr.``class`` "theme-container"
                        button {
                            attr.``class`` "theme-trigger-btn"
                            on.click (fun _ -> dispatch ToggleThemePopover)
                            Rz.icon("palette")
                        }
                        if model.ThemePopoverOpen then
                            div {
                                attr.``class`` "theme-popover"
                                ul {
                                    attr.``class`` "theme-list"
                                    li {
                                        attr.``class`` (if model.Theme = UITheme.Standard then "theme-item active" else "theme-item")
                                        on.click (fun _ -> dispatch (SetTheme UITheme.Standard))
                                        Rz.icon("light_mode")
                                        span { "Standard" }
                                        if model.Theme = UITheme.Standard then Rz.icon("check")
                                    }
                                    li {
                                        attr.``class`` (if model.Theme = UITheme.Dark then "theme-item active" else "theme-item")
                                        on.click (fun _ -> dispatch (SetTheme UITheme.Dark))
                                        Rz.icon("dark_mode")
                                        span { "Dark" }
                                        if model.Theme = UITheme.Dark then Rz.icon("check")
                                    }
                                    li {
                                        attr.``class`` (if model.Theme = UITheme.StandardDark then "theme-item active" else "theme-item")
                                        on.click (fun _ -> dispatch (SetTheme UITheme.StandardDark))
                                        Rz.icon("nights_stay")
                                        span { "Standard Dark" }
                                        if model.Theme = UITheme.StandardDark then Rz.icon("check")
                                    }
                                }
                            }
                    }
                    
                    // Notifications center badge
                    div {
                        attr.``class`` "notification-container"
                        let unreadCount = model.Notifications |> List.filter (fun n -> not n.IsRead) |> List.length
                        button {
                            attr.``class`` "notification-trigger-btn"
                            on.click (fun _ -> dispatch ToggleNotifications)
                            Rz.icon("notifications")
                            if unreadCount > 0 then
                                span { attr.``class`` "notification-badge"; string unreadCount }
                        }
                        if model.NotificationsOpen then
                            div {
                                attr.``class`` "notification-popover"
                                div {
                                    attr.``class`` "notification-popover-header"
                                    span { attr.``class`` "popover-title"; "Notifications" }
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
                                        li { attr.``class`` "notification-item empty"; "No notifications" }
                                    else
                                        for n in model.Notifications do
                                            li {
                                                attr.``class`` (if n.IsRead then "notification-item read" else "notification-item unread")
                                                span { attr.``class`` "item-title"; n.Title }
                                                span { attr.``class`` "item-message"; n.Message }
                                                span { attr.``class`` "item-time"; n.Timestamp.ToString("HH:mm") }
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
                        Rz.icon("history")
                    }

                    // User role toggler
                    Rz.stack([
                        Rz.icon("account_circle", style = "font-size: 20px;")
                        span {
                            attr.style "font-size: 13px; font-weight: 500; font-family: var(--rz-font-family);"
                            match model.CurrentUser with
                            | Some u -> sprintf "%s (%A)" u.Username u.Role
                            | None -> "Guest"
                        }
                        button {
                            attr.``class`` "theme-trigger-btn"
                            attr.title "Cycle Role"
                            on.click (fun _ -> dispatch CycleUserRole)
                            Rz.icon("swap_horiz", style = "font-size: 18px;")
                        }
                        button {
                            attr.``class`` "theme-trigger-btn"
                            attr.title "Sign Out"
                            on.click (fun _ -> dispatch TriggerLogout)
                            Rz.icon("logout", style = "font-size: 18px;")
                        }
                    ], orientation = Orientation.Horizontal, alignItems = AlignItems.Center, gap = "6px")

                    Rz.icon("settings", style = "cursor: pointer;")
                ],
                orientation = Orientation.Horizontal,
                alignItems = AlignItems.Center,
                gap = "16px",
                style = "margin-left: auto;"
            )
        ],
        orientation = Orientation.Horizontal,
        alignItems = AlignItems.Center,
        style = "width: 100%; padding: 0 16px; height: 50px;"
    )

// Layout Chrome: Sidebar Panel
let sidebarView (model: Model) (dispatch: Message -> unit) =
    div {
        attr.style "height: 100%; display: flex; flex-direction: column;"
        Rz.stack(
            items = [
                Rz.button("", style = ButtonStyle.Secondary, icon = "west", onClick = fun _ -> dispatch (SetSidebar false))
            ],
            orientation = Orientation.Horizontal,
            justifyContent = JustifyContent.End,
            class' = "rz-p-2"
        )
        Rz.panelMenu([
            Rz.panelMenuItem("Dashboard", icon = "home", path = "/")
            Rz.panelMenuItem("Demand Workbench", icon = "trending_up", path = "/demand")
            Rz.panelMenuItem("Supply Workbench", icon = "local_shipping", path = "/supply")
            Rz.panelMenuItem("Capacity Workbench", icon = "schedule", path = "/capacity")
            Rz.panelMenuItem("Scenario Workbench", icon = "schema", path = "/scenarios")
        ], style = "flex: 1;")
    }

// Layout Chrome: Breadcrumb Indicator
let breadcrumbView (model: Model) (dispatch: Message -> unit) =
    Rz.breadCrumb(
        items = [
            Rz.breadCrumbItem("Home", icon = "home")
            match model.ActivePage with
            | Page.Dashboard -> Rz.breadCrumbItem("Dashboard", icon = "home")
            | Page.Demand -> Rz.breadCrumbItem("Demand", icon = "trending_up")
            | Page.Supply -> Rz.breadCrumbItem("Supply", icon = "local_shipping")
            | Page.Capacity -> Rz.breadCrumbItem("Capacity", icon = "schedule")
            | Page.Scenarios -> Rz.breadCrumbItem("Scenarios", icon = "schema")
        ]
    )

// Layout Chrome: Active Operations Overlay Card
let operationsPanel (model: Model) (dispatch: Message -> unit) =
    if List.isEmpty model.ActiveOperations then
        empty()
    else
        div {
            attr.style "position: fixed; bottom: 20px; right: 20px; z-index: 1000; width: 350px; max-height: 500px; overflow-y: auto; display: flex; flex-direction: column; gap: 10px;"
            for op in model.ActiveOperations do
                comp<RadzenCard> {
                    "Style" => "padding: 16px; box-shadow: 0 10px 15px -3px rgba(0,0,0,0.3), 0 4px 6px -2px rgba(0,0,0,0.05); border-radius: 8px; border: 1px solid var(--rz-border-color);"
                    Rz.stack([
                        div {
                            attr.style "display: flex; justify-content: space-between; align-items: center;"
                            Rz.stack([
                                let icon, iconStyle =
                                    match op.State with
                                    | OperationState.Pending -> "hourglass_empty", "color: var(--rz-text-secondary-color); font-size: 16px;"
                                    | OperationState.Running _ -> "sync", "color: var(--rz-info-color); font-size: 16px;"
                                    | OperationState.Completed () -> "check_circle", "color: var(--rz-success-color); font-size: 16px;"
                                    | OperationState.Failed _ -> "error", "color: var(--rz-danger-color); font-size: 16px;"
                                    | OperationState.Cancelled -> "cancel", "color: var(--rz-text-secondary-color); font-size: 16px;"
                                
                                comp<RadzenIcon> {
                                    "Icon" => icon
                                    "Style" => iconStyle
                                    match op.State with
                                    | Running _ -> attr.``class`` "spin-icon"
                                    | _ -> attr.empty()
                                }
                                span {
                                    attr.style "font-weight: bold; font-size: 13px; font-family: var(--rz-font-family);"
                                    op.Name
                                  }
                            ], orientation = Orientation.Horizontal, alignItems = AlignItems.Center, gap = "6px")
                            
                            button {
                                attr.style "background: transparent; border: none; cursor: pointer; color: var(--rz-text-secondary-color); padding: 4px;"
                                attr.title "Dismiss"
                                on.click (fun _ -> dispatch (DismissOperation op.Id))
                                Rz.icon("close", style = "font-size: 16px;")
                            }
                        }
                        match op.State with
                        | OperationState.Pending ->
                            div {
                                Rz.progressBar(0.0, mode = ProgressBarMode.Indeterminate)
                                span { attr.style "font-size: 11px; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"; "Pending..." }
                            }
                        | OperationState.Running (progress, stage) ->
                            div {
                                Rz.progressBar(double progress)
                                span { attr.style "font-size: 11px; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"; sprintf "%d%% - %s" progress stage }
                            }
                        | OperationState.Completed () ->
                            div {
                                Rz.progressBar(100.0)
                                span { attr.style "font-size: 11px; color: var(--rz-success-color); font-weight: 500; font-family: var(--rz-font-family);"; "Completed successfully" }
                            }
                        | OperationState.Failed err ->
                            div {
                                Rz.progressBar(100.0)
                                span { attr.style "font-size: 11px; color: var(--rz-danger-color); font-weight: 500; font-family: var(--rz-font-family); word-break: break-all;"; sprintf "Failed: %s" err }
                            }
                        | OperationState.Cancelled ->
                            div {
                                Rz.progressBar(0.0)
                                span { attr.style "font-size: 11px; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"; "Cancelled" }
                            }
                    ], gap = "8px")
                }
        }

// Layout Chrome: Activity Log slide-out Drawer
let activityFeedDrawer (model: Model) (dispatch: Message -> unit) =
    if not model.ActivityFeedOpen then
        empty()
    else
        div {
            attr.style "position: fixed; top: 50px; right: 0; bottom: 0; width: 350px; z-index: 999; background-color: var(--rz-header-background-color, #2b3a4a); border-left: 1px solid var(--rz-border-color); box-shadow: -4px 0 10px rgba(0,0,0,0.25); display: flex; flex-direction: column; overflow: hidden;"
            
            div {
                attr.style "padding: 16px; border-bottom: 1px solid var(--rz-border-color); display: flex; justify-content: space-between; align-items: center;"
                span { attr.style "font-weight: bold; color: var(--rz-header-color, #ffffff); font-family: var(--rz-font-family);"; "System Activity Log" }
                button {
                    attr.style "background: transparent; border: none; cursor: pointer; color: var(--rz-header-color, #ffffff); padding: 4px;"
                    on.click (fun _ -> dispatch ToggleActivityFeed)
                    Rz.icon("close")
                }
            }
            
            div {
                attr.style "flex: 1; overflow-y: auto; padding: 12px;"
                if List.isEmpty model.ActivityFeed then
                    div {
                        attr.style "padding: 30px; text-align: center; color: var(--rz-color-text-secondary); font-size: 13px; font-family: var(--rz-font-family);"
                        "No background events logged."
                    }
                else
                    ul {
                        attr.style "list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; gap: 8px;"
                        for item in model.ActivityFeed do
                            li {
                                attr.style "padding: 10px; border-radius: 6px; border: 1px solid var(--rz-border-color); background-color: rgba(255, 255, 255, 0.02); display: flex; flex-direction: column; gap: 4px;"
                                div {
                                    attr.style "display: flex; justify-content: space-between; align-items: center;"
                                    span {
                                        attr.style "font-weight: 600; font-size: 12px; color: var(--rz-header-color, #ffffff); font-family: var(--rz-font-family);"
                                        item.EventType
                                    }
                                    span {
                                        attr.style "font-size: 9px; color: var(--rz-color-text-secondary); font-family: var(--rz-font-family);"
                                        item.Timestamp.ToString("HH:mm:ss")
                                    }
                                }
                                span {
                                    attr.style "font-size: 11px; color: var(--rz-color-text-secondary); word-break: break-all; font-family: var(--rz-font-family);"
                                    sprintf "Stream: %s" item.Stream
                                }
                            }
                    }
            }
        }

// Layout Chrome: Master Wrapper View coordination
let layoutView (model: Model) (dispatch: Message -> unit) (content: Node) =
    Rz.rzLayout(
        items = [
            Rz.rzHeader([
                headerView model dispatch
            ])
            Rz.rzSidebar(
                items = [
                    sidebarView model dispatch
                ],
                expanded = model.SidebarExpanded,
                expandedChanged = (fun expanded -> dispatch (SetSidebar expanded)),
                fullHeight = true,
                responsive = false,
                style = "position: absolute; z-index: 3"
            )
            Rz.rzBody([
                div {
                    attr.``class`` "rz-p-4"
                    breadcrumbView model dispatch
                    div {
                        attr.style "margin-top: 16px;"
                        comp<MedhaviErrorBoundary> {
                            "OnRetry" => Action(fun () -> dispatch ReloadAllData)
                            content
                        }
                    }
                }
            ])
            operationsPanel model dispatch
            activityFeedDrawer model dispatch
            if model.SidebarExpanded then
                div {
                    attr.``class`` "rz-dialog-mask"
                    attr.style "position: absolute; top: 0; left: 0; width: 100%; height: 100%; z-index: 2;"
                    on.click (fun _ -> dispatch (SetSidebar false))
                }
        ],
        style = "position: relative; height: 100vh;"
    )

let dashboardView model dispatch =
    let canImport =
        match model.CurrentUser with
        | Some u ->
            match u.Role with
            | Role.Supervisor | Role.Manager | Role.Administrator -> true
            | Role.Planner -> false
            | _ -> false
        | None -> false

    div {
        attr.``class`` "p-4"
        h1 { attr.``class`` "rz-text-h4"; "Dashboard 📊" }
        p { attr.``class`` "rz-color-text-secondary"; "Welcome to the Medhavi Planning Dashboard." }
        
        div {
            attr.``class`` "rz-mt-4"
            comp<RadzenCard> {
                "Style" => "max-width: 420px; padding: 20px; border-radius: 8px;"
                Rz.stack([
                    h4 { attr.``class`` "rz-text-h6 rz-m-0"; "Data Operations" }
                    span { attr.``class`` "rz-color-text-secondary"; "Import latest master data from external CSV files." }
                    comp<RadzenButton> {
                        "Text" => "Import Master Data"
                        "Icon" => "get_app"
                        "ButtonStyle" => ButtonStyle.Primary
                        "Disabled" => not canImport
                        if canImport then attr.callback "Click" (fun (e: MouseEventArgs) -> dispatch TriggerImportData) else attr.empty()
                    }
                ], gap = "12px")
            }
        }
    }

let view model dispatch =
    let pageContent =
        match model.ActivePage with
        | Page.Dashboard -> dashboardView model dispatch
        | Page.Demand -> Pages.DemandWorkbench.view model.DemandWorkbench (fun msg -> dispatch (DemandMsg msg))
        | Page.Supply -> Pages.SupplyWorkbench.view model.SupplyWorkbench (fun msg -> dispatch (SupplyMsg msg))
        | Page.Capacity -> Pages.CapacityWorkbench.view model.CapacityWorkbench (fun msg -> dispatch (CapacityMsg msg))
        | Page.Scenarios -> 
            let canRun =
                match model.CurrentUser with
                | Some u ->
                    match u.Role with
                    | Role.Supervisor | Role.Manager | Role.Administrator -> true
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
    }

let router = Router.infer SetPage (fun m -> m.ActivePage)

type Stores = {
    Demand: DemandStore
    Supply: SupplyStore
    Capacity: CapacityStore
    Scenario: ScenarioStore
    Activity: ActivityStore
}

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
                        dispatch (DemandMsg (Pages.DemandWorkbench.LoadDemands snapshot))
                    with ex ->
                        printfn "[AppShell] Demands loading failed: %s" ex.Message
                        dispatch (DemandMsg (Pages.DemandWorkbench.ShowError ex.Message))
                }
            let loadSupply () =
                task {
                    try
                        printfn "[AppShell] Loading Supplies..."
                        do! stores.Supply.Refresh()
                        let snapshot = stores.Supply.GetSnapshot()
                        printfn "[AppShell] Supplies loaded successfully, count: %d" snapshot.Length
                        dispatch (SupplyMsg (Pages.SupplyWorkbench.LoadSupplies snapshot))
                    with ex ->
                        printfn "[AppShell] Supplies loading failed: %s" ex.Message
                        dispatch (SupplyMsg (Pages.SupplyWorkbench.ShowError ex.Message))
                }
            let loadCapacity () =
                task {
                    try
                        printfn "[AppShell] Loading Capacities..."
                        do! stores.Capacity.Refresh()
                        let snapshot = stores.Capacity.GetSnapshot()
                        printfn "[AppShell] Capacities loaded successfully, count: %d" snapshot.Length
                        dispatch (CapacityMsg (Pages.CapacityWorkbench.LoadOperations snapshot))
                    with ex ->
                        printfn "[AppShell] Capacities loading failed: %s" ex.Message
                        dispatch (CapacityMsg (Pages.CapacityWorkbench.ShowError ex.Message))
                }
            let loadScenario () =
                task {
                    try
                        printfn "[AppShell] Loading Scenarios..."
                        do! stores.Scenario.Refresh()
                        let snapshot = stores.Scenario.GetSnapshot()
                        printfn "[AppShell] Scenarios loaded successfully, count: %d" snapshot.Length
                        dispatch (ScenarioMsg (Pages.ScenarioWorkbench.LoadScenarios snapshot))
                    with ex ->
                        printfn "[AppShell] Scenarios loading failed: %s" ex.Message
                        dispatch (ScenarioMsg (Pages.ScenarioWorkbench.ShowError ex.Message))
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

            do! Task.WhenAll([| loadDemand(); loadSupply(); loadCapacity(); loadScenario(); loadActivity() |]) :> Task
            printfn "[AppShell] loadData via Cmd finished."
        }
        |> Async.AwaitTask
        |> Async.Ignore
        |> Async.Start
    [run]

let subscribeToStoresCmd (setSubs: IDisposable list -> unit) (stores: Stores) (context: WorkspaceContextService) : Cmd<Message> =
    let run dispatch =
        let dSub = stores.Demand.Subscribe(fun () ->
            printfn "[AppShell] DemandStore cache updated, dispatching to Workbench"
            dispatch (DemandMsg (Pages.DemandWorkbench.LoadDemands (stores.Demand.GetSnapshot())))
        )
        let sSub = stores.Supply.Subscribe(fun () ->
            printfn "[AppShell] SupplyStore cache updated, dispatching to Workbench"
            dispatch (SupplyMsg (Pages.SupplyWorkbench.LoadSupplies (stores.Supply.GetSnapshot())))
        )
        let cSub = stores.Capacity.Subscribe(fun () ->
            printfn "[AppShell] CapacityStore cache updated, dispatching to Workbench"
            dispatch (CapacityMsg (Pages.CapacityWorkbench.LoadOperations (stores.Capacity.GetSnapshot())))
        )
        let scSub = stores.Scenario.Subscribe(fun () ->
            printfn "[AppShell] ScenarioStore cache updated, dispatching to Workbench"
            dispatch (ScenarioMsg (Pages.ScenarioWorkbench.LoadScenarios (stores.Scenario.GetSnapshot())))
        )
        let actSub = stores.Activity.Subscribe(fun () ->
            printfn "[AppShell] ActivityStore cache updated, dispatching to Shell"
            dispatch (LoadActivityFeed (stores.Activity.GetSnapshot()))
        )

        // Workspace scope change listener
        let contextSub = context.OnScopeChanged.Subscribe(fun newScope ->
            printfn "[AppShell] Workspace scope changed, refreshing all stores"
            task {
                do! stores.Demand.SetScope(newScope)
                do! stores.Supply.SetScope(newScope)
                do! stores.Capacity.SetScope(newScope)
                dispatch ReloadAllData
            } |> ignore
        )

        // Event invalidations
        let demandBusSub = DomainEventBus.Subscribe<DemandLineEvent>(fun ev ->
            printfn "[AppShell] Live event received from EventBus: %A" ev
            stores.Demand.Refresh() |> ignore
            stores.Activity.Refresh() |> ignore
            let notif = { Id = Guid.NewGuid(); Title = "Demand Event"; Message = sprintf "Demand change received: %A" ev; Timestamp = DateTime.Now; IsRead = false }
            dispatch (ReceiveNotification notif)
        )

        let supplyBusSub = DomainEventBus.Subscribe<SupplyOrderEvent>(fun ev ->
            printfn "[AppShell] Live event received from EventBus: %A" ev
            stores.Supply.Refresh() |> ignore
            stores.Activity.Refresh() |> ignore
            let notif = { Id = Guid.NewGuid(); Title = "Supply Event"; Message = sprintf "Supply change received: %A" ev; Timestamp = DateTime.Now; IsRead = false }
            dispatch (ReceiveNotification notif)
        )

        let capacityBusSub = DomainEventBus.Subscribe<Medhavi.Capacity.Domain.CapacityAgg.CapacityEvent>(fun ev ->
            printfn "[AppShell] Live event received from EventBus: %A" ev
            stores.Capacity.Refresh() |> ignore
            stores.Activity.Refresh() |> ignore
            let notif = { Id = Guid.NewGuid(); Title = "Capacity Event"; Message = sprintf "Capacity change received: %A" ev; Timestamp = DateTime.Now; IsRead = false }
            dispatch (ReceiveNotification notif)
        )

        setSubs([ dSub; sSub; cSub; scSub; actSub; contextSub; demandBusSub; supplyBusSub; capacityBusSub ])
    [run]

let runMrpCmd (service: PlanningCommandService) : Cmd<Message> =
    let run dispatch =
        task {
            let opId = Guid.NewGuid()
            dispatch (StartOperation (opId, "MRP Scheduling Run"))
            
            let onProgress pct stage =
                dispatch (UpdateOperationProgress (opId, pct, stage))
                
            let! res = service.RunMrp(onProgress)
            match res with
            | Ok _ ->
                dispatch (CompleteOperation opId)
                let notif = { Id = Guid.NewGuid(); Title = "MRP Completed"; Message = "Baseline MRP scheduling run completed successfully."; Timestamp = DateTime.Now; IsRead = false }
                dispatch (ReceiveNotification notif)
            | Error err ->
                dispatch (FailOperation (opId, err))
                let notif = { Id = Guid.NewGuid(); Title = "MRP Failed"; Message = sprintf "MRP baseline run failed: %s" err; Timestamp = DateTime.Now; IsRead = false }
                dispatch (ReceiveNotification notif)
        }
        |> Async.AwaitTask
        |> Async.Ignore
        |> Async.Start
    [run]

let importDataCmd (service: PlanningCommandService) : Cmd<Message> =
    let run dispatch =
        task {
            let opId = Guid.NewGuid()
            dispatch (StartOperation (opId, "Import Master Data"))
            
            let onProgress pct stage =
                dispatch (UpdateOperationProgress (opId, pct, stage))
                
            let! res = service.TriggerImport(onProgress)
            match res with
            | Ok _ ->
                dispatch (CompleteOperation opId)
                let notif = { Id = Guid.NewGuid(); Title = "Import Completed"; Message = "Master data synchronized successfully."; Timestamp = DateTime.Now; IsRead = false }
                dispatch (ReceiveNotification notif)
            | Error err ->
                dispatch (FailOperation (opId, err))
                let notif = { Id = Guid.NewGuid(); Title = "Import Failed"; Message = sprintf "Master data import failed: %s" err; Timestamp = DateTime.Now; IsRead = false }
                dispatch (ReceiveNotification notif)
        }
        |> Async.AwaitTask
        |> Async.Ignore
        |> Async.Start
    [run]

let update (stores: Stores) (planningService: PlanningCommandService) (workspaceContext: WorkspaceContextService) (msg: Message) (model: Model) : Model * Cmd<Message> =
    match msg with
    | TriggerRunMrp ->
        model, runMrpCmd planningService
    | TriggerImportData ->
        model, importDataCmd planningService
    | ReloadAllData ->
        model, loadDataCmd stores
    | ScenarioMsg (Pages.ScenarioWorkbench.Msg.SelectActiveScenario sIdOpt) ->
        let currentScope = workspaceContext.CurrentScope
        let newScope = { currentScope with ScenarioId = sIdOpt }
        workspaceContext.SetScope(newScope) |> ignore
        
        let subModel = Pages.ScenarioWorkbench.update (Pages.ScenarioWorkbench.SelectActiveScenario sIdOpt) model.ScenarioWorkbench
        { model with ScenarioWorkbench = subModel }, Cmd.none
    | ScenarioMsg (Pages.ScenarioWorkbench.Msg.SelectCompareScenario sIdOpt) ->
        let subModel = Pages.ScenarioWorkbench.update (Pages.ScenarioWorkbench.SelectCompareScenario sIdOpt) model.ScenarioWorkbench
        { model with ScenarioWorkbench = subModel }, Cmd.none
    | SetPage page -> 
        { model with ActivePage = page; SidebarExpanded = false }, Cmd.none
    | ToggleSidebar -> 
        { model with SidebarExpanded = not model.SidebarExpanded }, Cmd.none
    | SetSidebar expanded -> 
        { model with SidebarExpanded = expanded }, Cmd.none
    | ToggleThemePopover -> 
        { model with ThemePopoverOpen = not model.ThemePopoverOpen }, Cmd.none
    | SetTheme theme -> 
        { model with Theme = theme; ThemePopoverOpen = false }, Cmd.none
    | ToggleNotifications ->
        { model with NotificationsOpen = not model.NotificationsOpen }, Cmd.none
    | MarkAllNotificationsRead ->
        let readList = model.Notifications |> List.map (fun n -> { n with IsRead = true })
        { model with Notifications = readList }, Cmd.none
    | ClearNotifications ->
        { model with Notifications = [] }, Cmd.none
    | SetConnectionStatus status ->
        { model with ConnectionStatus = status }, Cmd.none
    | ReceiveNotification n ->
        let currentList = n :: model.Notifications
        let trimmedList = if currentList.Length > 50 then List.take 50 currentList else currentList
        { model with Notifications = trimmedList }, Cmd.none
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
        let updatedUser = model.CurrentUser |> Option.map (fun u -> { u with Role = nextRole })
        { model with CurrentUser = updatedUser }, Cmd.none
    | TriggerLogout ->
        model, Cmd.none
    | ToggleActivityFeed ->
        { model with ActivityFeedOpen = not model.ActivityFeedOpen }, Cmd.none
    | LoadActivityFeed feed ->
        { model with ActivityFeed = feed }, Cmd.none
    | StartOperation (id, name) ->
        let op = { Id = id; Name = name; State = OperationState.Running (0, "Initializing") }
        { model with ActiveOperations = op :: model.ActiveOperations }, Cmd.none
    | UpdateOperationProgress (id, progress, stage) ->
        let ops = model.ActiveOperations |> List.map (fun op ->
            if op.Id = id then { op with State = OperationState.Running (progress, stage) } else op
        )
        { model with ActiveOperations = ops }, Cmd.none
    | CompleteOperation id ->
        let ops = model.ActiveOperations |> List.map (fun op ->
            if op.Id = id then { op with State = OperationState.Completed () } else op
        )
        { model with ActiveOperations = ops }, Cmd.none
    | FailOperation (id, err) ->
        let ops = model.ActiveOperations |> List.map (fun op ->
            if op.Id = id then { op with State = OperationState.Failed err } else op
        )
        { model with ActiveOperations = ops }, Cmd.none
    | DismissOperation id ->
        let ops = model.ActiveOperations |> List.filter (fun op -> op.Id <> id)
        { model with ActiveOperations = ops }, Cmd.none
    | DemandMsg subMsg ->
        let subModel, subCmd = Pages.DemandWorkbench.update subMsg model.DemandWorkbench
        { model with DemandWorkbench = subModel }, Cmd.map DemandMsg subCmd
    | SupplyMsg subMsg ->
        let subModel = Pages.SupplyWorkbench.update subMsg model.SupplyWorkbench
        { model with SupplyWorkbench = subModel }, Cmd.none
    | CapacityMsg subMsg ->
        let subModel = Pages.CapacityWorkbench.update subMsg model.CapacityWorkbench
        { model with CapacityWorkbench = subModel }, Cmd.none
    | ScenarioMsg subMsg ->
        let subModel = Pages.ScenarioWorkbench.update subMsg model.ScenarioWorkbench
        { model with ScenarioWorkbench = subModel }, Cmd.none

[<BoleroRenderModeAttribute(BoleroRenderMode.Server)>]
type AppShellComponent() =
    inherit ProgramComponent<Model, Message>()
    
    let mutable subs : IDisposable list = []
    let mutable dotNetRef : DotNetObjectReference<AppShellComponent> option = None

    member this.SetSubs(s: IDisposable list) =
        for sub in subs do sub.Dispose()
        subs <- s

    [<Parameter>]
    member val CurrentUser : User = Unchecked.defaultof<User> with get, set

    [<Parameter>]
    member val OnLogout : EventCallback = EventCallback.Empty with get, set

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
    member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

    [<Inject>]
    member val PlanningService = Unchecked.defaultof<PlanningCommandService> with get, set

    [<Inject>]
    member val WorkspaceContext = Unchecked.defaultof<WorkspaceContextService> with get, set
    
    override this.Program =
        let stores = {
            Demand = this.DemandStore
            Supply = this.SupplyStore
            Capacity = this.CapacityStore
            Scenario = this.ScenarioStore
            Activity = this.ActivityStore
        }
        
        let init () =
            let model = initModel
            let updatedModel = { model with CurrentUser = Some this.CurrentUser }
            let loadCmd = loadDataCmd stores
            let subCmd = subscribeToStoresCmd this.SetSubs stores this.WorkspaceContext
            updatedModel, Cmd.batch [ loadCmd; subCmd ]

        let update msg model =
            match msg with
            | TriggerLogout ->
                this.OnLogout.InvokeAsync() |> ignore
                model, Cmd.none
            | _ ->
                update stores this.PlanningService this.WorkspaceContext msg model

        Program.mkProgram (fun _ -> init ()) update view
        |> Program.withRouter router

    override this.OnAfterRender(firstRender) =
        if firstRender then
            let reference = DotNetObjectReference.Create(this)
            dotNetRef <- Some reference
            this.JSRuntime.InvokeVoidAsync("setupConnectionListener", reference).AsTask() |> ignore

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
            for sub in subs do sub.Dispose()
            match dotNetRef with
            | Some r -> r.Dispose()
            | None -> ()


