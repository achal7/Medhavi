namespace Medhavi.Web.AppShell

open Radzen
open Medhavi.Contracts.Demand
open Medhavi.Web
open Medhavi.Web.Workspaces
open Medhavi.Web.Stores

type AppShellEnv =
    { DemandLineQueries: DemandLineQueries
      StoreRegistry: WorkspaceStoreRegistry
      TooltipService: TooltipService }

type AppShellModel =
    { Session: Session.Model
      ActiveWorkspace: Workspace option
      NavigationbarExpanded: bool
      RightSidebarExpanded: bool
      RightSidebarActiveTab: int
      SettingsDialogOpen: bool
      ProfilePopoverOpen: bool
      CommandPaletteOpen: bool
      CommandPaletteSearchText: string
      AppbarState: Appbar.Model
      NavigationState: Navigation.Model
      MaterialReservationState: MaterialReservation.Model option }

type Message =
    // AI & Workspace Messages
    | ToggleCommandPalette
    | SetCommandPaletteOpen of bool
    | ExecuteWorkspaceAction of WorkspaceAction
    | ReservationWorkspaceMsg of MaterialReservation.Msg

    // Component messages
    | ToggleSidebar
    | SetSidebar of bool
    | AppbarMsg of Appbar.Msg
    | NavigationMsg of Navigation.Msg
    | SessionMsg of Session.Msg
    | SetRightSidebar of expanded: bool * tab: int option
    | SetRightSidebarActiveTab of int
    | ToggleSettingsDialog
    | SetSettingsDialogOpen of bool
    | ToggleProfilePopover
    | SetProfilePopoverOpen of bool

[<RequireQualifiedAccess>]
type Output = | Logout
