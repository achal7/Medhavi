module Medhavi.Web.AppShell.Appbar

open Microsoft.AspNetCore.Components
open Bolero.Html
open Elmish
open Medhavi.Contracts
open Medhavi.Web.Controls
open Radzen
open Radzen.Blazor
open Microsoft.AspNetCore.Components.Web
open Medhavi.Web

type Scenario =
    { ScenarioId: string
      Name: string
      Children: Scenario list }

type Scope = { ScenarioId: string option }

type Model =
    { SettingsDialogOpen: bool
      User: User
      ConnectionStatus: ConnectionStatus
      Scenarios: Scenario list
      CurrentScope: Scope
      Theme: UITheme
      Notifications: Notification list
      CurrentUser: User option }

type Msg =
    | ToggleSettingsDialog
    | SetSettingsDialogOpen of bool
    | ToggleNavigationbar
    | ToggleNotifications
    | ToggleActivityFeeds
    | SelectActiveScenario of string option
    | ToggleCommandPalette
    | SetTheme of UITheme
    | MarkAllNotificationsRead
    | ClearNotifications
    | CycleUserRole
    | TriggerLogout
    | ToggleScenarioDropdown

[<RequireQualifiedAccess>]
type Output =
    | SelectActiveScenario of string option
    | ToggleCommandPalette
    | SetTheme of UITheme
    | MarkAllNotificationsRead
    | ClearNotifications
    | CycleUserRole
    | TriggerLogout
    | ToggleNavigationbar
    | ToggleNotifications
    | ToggleActivityFeed
    | ToggleSettingsDialog
    | SetSettingsDialogOpen of bool

let init (user: User) : Model * Cmd<Msg> =
    { SettingsDialogOpen = false
      User = user
      ConnectionStatus = ConnectionStatus.Connected
      Scenarios =
        [ { ScenarioId = "BASELINE"
            Name = "Baseline Scenario"
            Children =
              [ { ScenarioId = "SIM_A"
                  Name = "Simulation A"
                  Children = [] }
                { ScenarioId = "SIM_B"
                  Name = "Simulation B"
                  Children =
                    [ { ScenarioId = "SIM_B_QA"
                        Name = "QA Scenario"
                        Children = [] } ] } ] }
          { ScenarioId = "OPTIMIZATION"
            Name = "Optimization Scenario"
            Children =
              [ { ScenarioId = "HIGH_DEMAND"
                  Name = "High Demand"
                  Children = [] }
                { ScenarioId = "SUPPLY_DISRUPT"
                  Name = "Supply Disruption"
                  Children = [] } ] } ]
      CurrentScope = { ScenarioId = Some "BASELINE" }
      Theme = UITheme.Dark
      Notifications = []
      CurrentUser = Some user },
    Cmd.none

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> * Output option =
    match msg with
    | ToggleNotifications -> model, Cmd.none, Some Output.ToggleNotifications
    | ToggleActivityFeeds -> model, Cmd.none, Some Output.ToggleActivityFeed
    | ToggleNavigationbar -> model, Cmd.none, Some Output.ToggleNavigationbar
    | ToggleSettingsDialog ->
        let open' = not model.SettingsDialogOpen

        { model with
            SettingsDialogOpen = open' },
        Cmd.none,
        Some Output.ToggleSettingsDialog
    | SetSettingsDialogOpen open' ->
        { model with
            SettingsDialogOpen = open' },
        Cmd.none,
        Some(Output.SetSettingsDialogOpen open')

    | SelectActiveScenario selected -> model, Cmd.none, Some(Output.SelectActiveScenario selected)
    | ToggleCommandPalette -> model, Cmd.none, Some Output.ToggleCommandPalette
    | SetTheme theme -> { model with Theme = theme }, Cmd.none, Some(Output.SetTheme theme)
    | MarkAllNotificationsRead -> model, Cmd.none, Some Output.MarkAllNotificationsRead
    | ClearNotifications -> model, Cmd.none, Some Output.ClearNotifications
    | CycleUserRole -> model, Cmd.none, Some Output.CycleUserRole
    | TriggerLogout -> model, Cmd.none, Some Output.TriggerLogout
    | ToggleScenarioDropdown -> model, Cmd.none, None

let connectionBadge (tooltipService: TooltipService) status =
    let titleText, icon, style =
        match status with
        | Connected -> "Online", "check_circle", ButtonStyle.Success
        | Reconnecting -> "Reconnecting", "refresh", ButtonStyle.Warning
        | Disconnected -> "Offline", "report", ButtonStyle.Danger

    comp<RadzenButton> {
        "Icon" => icon
        "ButtonStyle" => style
        "Variant" => Radzen.Variant.Text
        "Size" => ButtonSize.Small
        attr.callback "MouseEnter" (fun (elRef: ElementReference) -> tooltipService.Open(elRef, titleText))
        attr.``class`` "filled-icon"
    }

let scenarioSelection (tooltipService: TooltipService) (model: Model) dispatch =
    let flatScenarios =
        let rec flatten (level: int) (scs: Scenario list) =
            scs
            |> List.collect(fun sc ->
                let prefix = System.String('\u00A0', level * 4)
                let indentedScenario = { sc with Name = prefix + sc.Name }
                indentedScenario :: flatten (level + 1) sc.Children)

        flatten 0 model.Scenarios

    comp<RadzenDropDown<string>> {
        "Data" => flatScenarios
        "ValueProperty" => "ScenarioId"
        "TextProperty" => "Name"
        "Value" => (defaultArg model.CurrentScope.ScenarioId "")

        attr.callback "Change" (fun (args: obj) ->
            let id = args :?> string
            dispatch(SelectActiveScenario(if System.String.IsNullOrEmpty id then None else Some id)))

        attr.style "width: 180px;"
    }

let themeSelection (model: Model) dispatch =
    comp<RadzenSplitButton> {
        "Icon" => "palette"
        "Size" => ButtonSize.Small
        "AlwaysOpenPopup" => true
        "ButtonStyle" => ButtonStyle.Light

        attr.callback "Click" (fun (args: RadzenSplitButtonItem) ->
            match args.Value with
            | "Standard" -> dispatch(SetTheme UITheme.Standard)
            | "Dark" -> dispatch(SetTheme UITheme.Dark)
            | "StandardDark" -> dispatch(SetTheme UITheme.StandardDark)
            | _ -> ())

        comp<RadzenSplitButtonItem> {
            "Text" => UITheme.Standard.ToString()
            "Value" => UITheme.Standard.ToString()
            "Icon" => (if model.Theme = UITheme.Standard then "check" else "light_mode")
        }

        comp<RadzenSplitButtonItem> {
            "Text" => UITheme.Dark.ToString()
            "Value" => UITheme.Dark.ToString()
            "Icon" => (if model.Theme = UITheme.Dark then "check" else "dark_mode")
        }

        comp<RadzenSplitButtonItem> {
            "Text" => "Standard Dark"
            "Value" => UITheme.StandardDark.ToString()
            "Icon" => (if model.Theme = UITheme.StandardDark then "check" else "nights_stay")
        }
    }

let view (tooltipService: TooltipService) (model: Model) dispatch =
    Rz.stack(
        [ Rz.stack(
              [ Rz.sidebarToggle(click = fun _ -> dispatch ToggleNavigationbar)
                Rz.label("APS Planning", class' = "rz-text-weight-bold rz-pl-2") ],
              orientation = Orientation.Horizontal,
              alignItems = AlignItems.Center,
              gap = "0"
          )

          Rz.stack(
              [

                scenarioSelection tooltipService model dispatch
                comp<RadzenButton> {
                    "Icon" => "search"
                    "Variant" => Radzen.Variant.Text
                    "ButtonStyle" => ButtonStyle.Light
                    "Size" => ButtonSize.Small
                    attr.callback "Click" (fun (args: MouseEventArgs) -> dispatch(ToggleCommandPalette))

                    attr.callback "MouseEnter" (fun (elRef: ElementReference) ->
                        tooltipService.Open(elRef, "Search workbenches and entities (Ctrl+K)"))
                }

                let unreadCount = model.Notifications |> List.filter(fun n -> not n.IsRead) |> List.length
                let hasUnreadCount = unreadCount > 0

                comp<RadzenButton> {
                    "Variant" => Radzen.Variant.Text
                    "Size" => ButtonSize.Small
                    "ButtonStyle" => if hasUnreadCount then ButtonStyle.Info else ButtonStyle.Light
                    attr.callback "Click" (fun (_: MouseEventArgs) -> dispatch(ToggleNotifications))

                    attr.callback "MouseEnter" (fun (elRef: ElementReference) ->
                        tooltipService.Open(elRef, "Notifications History"))

                    attr.fragment
                        "ChildContent"
                        (concat {
                            comp<RadzenIcon> {
                                "Icon" => if hasUnreadCount then "notifications" else "notifications_none"
                                attr.``class`` "filled-icon"
                            }

                            comp<RadzenBadge> {
                                "BadgeStyle" => if hasUnreadCount then BadgeStyle.Secondary else BadgeStyle.Light
                                "IsPill" => true
                                "Text" => string unreadCount
                                "class" => "rz-ms-2"
                            }
                        })
                }

                comp<RadzenButton> {
                    "Icon" => "history"
                    "Variant" => Radzen.Variant.Text
                    attr.callback "Click" (fun (args: MouseEventArgs) -> dispatch(ToggleActivityFeeds))
                    "Size" => ButtonSize.Small
                    "ButtonStyle" => ButtonStyle.Light

                    attr.callback "MouseEnter" (fun (elRef: ElementReference) ->
                        tooltipService.Open(elRef, "System Activity Log"))
                }

                comp<RadzenButton> {
                    "Icon" => "settings"
                    "Variant" => Radzen.Variant.Text
                    attr.callback "Click" (fun (args: MouseEventArgs) -> dispatch(ToggleSettingsDialog))
                    "Size" => ButtonSize.Small
                    "ButtonStyle" => ButtonStyle.Light

                    attr.callback "MouseEnter" (fun (elRef: ElementReference) ->
                        tooltipService.Open(elRef, "Configuration settings"))
                }

                connectionBadge tooltipService model.ConnectionStatus

                themeSelection model dispatch

                comp<RadzenProfileMenu> {
                    attr.callback "Click" (fun (item: RadzenProfileMenuItem) ->
                        match item.Text with
                        | "Cycle User Role" -> dispatch CycleUserRole
                        | "Sign Out" -> dispatch TriggerLogout
                        | _ -> ())

                    attr.fragment
                        "Template"
                        (div {
                            attr.style
                                "display: flex; align-items: center; gap: 8px; font-size: 13px; font-weight: 500; font-family: var(--rz-font-family); color: var(--rz-text-color); cursor: pointer;"

                            Rz.icon("account_circle", style = "font-size: 20px;")
                            span { model.User.Name }
                        })

                    attr.fragment
                        "ChildContent"
                        (forEach
                            [ comp<RadzenProfileMenuItem> {
                                  "Text" => "Cycle User Role"
                                  "Icon" => "swap_horiz"
                              }
                              comp<RadzenProfileMenuItem> {
                                  "Text" => "Sign Out"
                                  "Icon" => "logout"
                              } ]
                            id)
                } ],

              orientation = Orientation.Horizontal,
              alignItems = AlignItems.Center,
              gap = "6px",
              style = "margin-left: auto;"
          ) ],
        orientation = Orientation.Horizontal,
        alignItems = AlignItems.Center,
        style = "width: 100%; padding: 0 16px; height: 50px;"
    )
