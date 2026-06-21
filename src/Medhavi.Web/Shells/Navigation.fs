module Medhavi.Web.AppShell.Navigation

open Bolero
open Bolero.Html
open Elmish
open Radzen
open Medhavi.Web
open Medhavi.Web.Controls
open Medhavi.Web.Components

type WorkspaceNavigation =
    | [<EndPoint "/">] Dashboard
    | [<EndPoint "/mreservation">] MaterialReservation
    | [<EndPoint "/supply">] Supply
    | [<EndPoint "/capacity">] Capacity
    | [<EndPoint "/scenarios">] Scenarios
    | [<EndPoint "/promise">] Promise
    | [<EndPoint "/masterdata">] MasterData

type Model =
    { Menus: WorkspaceNavigation list
      SelectedMenu: WorkspaceNavigation
      SidebarOpen: bool }

type Msg =
    | ToggleSidebar
    | SetSidebar of bool
    | SelectMenu of WorkspaceNavigation

[<RequireQualifiedAccess>]
type Output =
    | SelectedMenu of WorkspaceNavigation
    | ToggleSidebar
    | SetSidebar of bool

let init (workspaceNavigation: WorkspaceNavigation list) : Model * Cmd<Msg> =
    { Menus = workspaceNavigation
      SelectedMenu = WorkspaceNavigation.Dashboard
      SidebarOpen = false },
    Cmd.none

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> * Output option =
    match msg with
    | ToggleSidebar ->
        let open' = not model.SidebarOpen
        { model with SidebarOpen = open' }, Cmd.none, Some Output.ToggleSidebar
    | SetSidebar expanded -> { model with SidebarOpen = expanded }, Cmd.none, Some(Output.SetSidebar expanded)
    | SelectMenu menu -> { model with SelectedMenu = menu }, Cmd.none, Some(Output.SelectedMenu menu)

let view (model: Model) dispatch =
    div {
        attr.style "height: 100%; display: flex; flex-direction: column;"

        Rz.stack(
            [ Rz.button(
                  "",
                  style = ButtonStyle.Secondary,
                  icon = "west",
                  onClick = fun _ -> dispatch(SetSidebar false)
              ) ],
            orientation = Orientation.Horizontal,
            justifyContent = JustifyContent.End,
            class' = "rz-p-2"
        )

        Rz.panelMenu(
            [ Rz.panelMenuItem(
                  "Dashboard",
                  icon = "home",
                  onClick = fun _ -> dispatch(SelectMenu WorkspaceNavigation.Dashboard)
              )
              Rz.panelMenuItem(
                  "Demand Workbench",
                  icon = "trending_up",
                  onClick = fun _ -> dispatch(SelectMenu WorkspaceNavigation.MaterialReservation)
              )
              Rz.panelMenuItem(
                  "Supply Workbench",
                  icon = "local_shipping",
                  onClick = fun _ -> dispatch(SelectMenu WorkspaceNavigation.Supply)
              )
              Rz.panelMenuItem(
                  "Capacity Workbench",
                  icon = "schedule",
                  onClick = fun _ -> dispatch(SelectMenu WorkspaceNavigation.Capacity)
              )
              Rz.panelMenuItem(
                  "Promise Workbench",
                  icon = "flash_on",
                  onClick = fun _ -> dispatch(SelectMenu WorkspaceNavigation.Promise)
              )
              Rz.panelMenuItem(
                  "Scenario Workbench",
                  icon = "schema",
                  onClick = fun _ -> dispatch(SelectMenu WorkspaceNavigation.Scenarios)
              )
              Rz.panelMenuItem(
                  "Master Data",
                  icon = "schema",
                  onClick = fun _ -> dispatch(SelectMenu WorkspaceNavigation.MasterData)
              )
              Rz.panelMenuItem("Command History", icon = "history") ],
            style = "flex: 1;"
        )
    }
