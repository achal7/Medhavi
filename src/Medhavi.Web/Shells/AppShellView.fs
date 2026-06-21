module Medhavi.Web.AppShell.AppShellView

open Bolero
open Bolero.Html
open Radzen
open Microsoft.AspNetCore.Components
open Medhavi.Web
open Medhavi.Web.Controls
open Medhavi.Web.AppShell
open Medhavi.Web.Workspaces
open Medhavi.Web.Stores

let headerView (tooltip: TooltipService) (model: AppShellModel) (dispatch: Message -> unit) =
    Appbar.view tooltip model.AppbarState (AppbarMsg >> dispatch)

let sidebarView (model: AppShellModel) (dispatch: Message -> unit) =
    Navigation.view model.NavigationState (NavigationMsg >> dispatch)

let breadcrumbView (model: AppShellModel) (dispatch: Message -> unit) =
    Rz.breadCrumb(
        items =
            [ Rz.breadCrumbItem("Home", icon = "home")
              match model.ActiveWorkspace with
              | Some Workspace.MaterialReservation -> Rz.breadCrumbItem("Material Reservation", icon = "home")
              | _ -> Rz.breadCrumbItem("Dashboard", icon = "home") ]
    )

let layoutView (env: AppShellEnv) (model: AppShellModel) (dispatch: Message -> unit) (content: Node) =
    Rz.rzLayout(
        [ Rz.rzHeader([ headerView env.TooltipService model dispatch ])
          Rz.rzSidebar(
              items = [ sidebarView model dispatch ],
              expanded = model.NavigationbarExpanded,
              expandedChanged = (fun expanded -> dispatch(Message.SetSidebar expanded)),
              fullHeight = true,
              responsive = false,
              position = SidebarPosition.Left
          )
          Rz.rzSidebar(
              items =
                  [ Medhavi.Web.Panels.SidebarPanel.view(
                        model.RightSidebarActiveTab,
                        model.Session.Notifications,
                        model.Session.Operations,
                        model.Session.CommandHistory,
                        (fun () -> dispatch(SessionMsg Session.Msg.ClearNotifications)),
                        (fun () -> dispatch(SessionMsg Session.Msg.MarkAllNotificationsRead)),
                        (fun id -> dispatch(SessionMsg(Session.Msg.DismissOperation id))),
                        (fun () -> dispatch(SetRightSidebar(false, None)))
                    ) ],
              expanded = model.RightSidebarExpanded,
              expandedChanged = (fun expanded -> dispatch(SetRightSidebar(expanded, None))),
              position = SidebarPosition.Right,
              fullHeight = true,
              responsive = false
          )
          Rz.rzBody(
              [ div {
                    attr.``class`` "rz-p-4"
                    breadcrumbView model dispatch

                    div {
                        attr.style "margin-top: 16px;"

                        comp<Medhavi.Web.Components.MedhaviErrorBoundary> {
                            //"OnRetry" => Action(fun () -> dispatch ReloadAllData)
                            content
                        }
                    }
                } ]
          ) ],
        style = "position: relative; height: 100vh;"
    )

let pageContent (model: AppShellModel) (dispatch: Message -> unit) : Node =
    match model.ActiveWorkspace with
    | Some Workspace.MaterialReservation ->
        match model.MaterialReservationState with
        | Some rSubModel -> MaterialReservation.view rSubModel (ReservationWorkspaceMsg >> dispatch)
        | None -> div { "Reservation workspace loading..." }
    | Some Workspace.MasterData ->
        match model.MasterDataState with
        | Some rSubModel -> MasterData.view rSubModel (MasterDataMsg >> dispatch)
        | None -> div { "MasterData workspace loading..." }
    | _ -> div { text "SCENARIOS" }

let view (env: AppShellEnv) (model: AppShellModel) dispatch =
    div {
        link {
            attr.rel "stylesheet"

            attr.href(
                match model.Session.Theme with
                | Medhavi.Web.UITheme.Standard -> "_content/Radzen.Blazor/css/standard.css"
                | Medhavi.Web.UITheme.Dark -> "_content/Radzen.Blazor/css/dark.css"
                | Medhavi.Web.UITheme.StandardDark -> "_content/Radzen.Blazor/css/standard-dark.css"
            )
        }

        layoutView env model dispatch (pageContent model dispatch)
    }
