module Medhavi.Client.Shells.ApplicationBar

open Elmish
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Microsoft.AspNetCore.Components.Web
open Medhavi.Client
open Medhavi.Client.Controls
open Medhavi.Client.Primitives

type Model = { Planning: PlanningContext }

type Msg =
    | ToggleNavigationRequested
    | SetScenario of ScenarioId
    | SetHorizon of DateRange
    | SetPlant of string option
    | OpenCommandPaletteRequested
    | SettingsRequested
    | AboutRequested
    | LogoutRequested
    | ToggleSidePanelRequested

[<RequireQualifiedAccess>]
type Output =
    | ToggleNavigation
    | ScenarioChanged of ScenarioId
    | HorizonChanged of DateRange
    | PlantChanged of string option
    | OpenCommandPalette
    | SettingsRequested
    | AboutRequested
    | LogoutRequested
    | ToggleSidePanel

let init (planning: PlanningContext) : Model * Cmd<Msg> = { Planning = planning }, Cmd.none

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> * Output option =
    match msg with
    | ToggleNavigationRequested -> model, Cmd.none, Some Output.ToggleNavigation
    | SetScenario scenario ->
        { model with
            Planning =
                { model.Planning with
                    Scenario = scenario } },
        Cmd.none,
        Some(Output.ScenarioChanged scenario)
    | SetHorizon horizon ->
        { model with
            Planning =
                { model.Planning with
                    Horizon = horizon } },
        Cmd.none,
        Some(Output.HorizonChanged horizon)
    | SetPlant plantOpt ->
        { model with
            Planning =
                { model.Planning with
                    PlantId = plantOpt } },
        Cmd.none,
        Some(Output.PlantChanged plantOpt)
    | OpenCommandPaletteRequested -> model, Cmd.none, Some Output.OpenCommandPalette
    | SettingsRequested -> model, Cmd.none, Some Output.SettingsRequested
    | AboutRequested -> model, Cmd.none, Some Output.AboutRequested
    | LogoutRequested -> model, Cmd.none, Some Output.LogoutRequested
    | ToggleSidePanelRequested -> model, Cmd.none, Some Output.ToggleSidePanel

let view (model: Model) (dispatch: Msg -> unit) : Node =
    let scenarios =
        [ "baseline", "Baseline Plan"
          "what-if-1", "What-If: Supplier Delay"
          "what-if-2", "What-If: Outage" ]

    let currentScenarioKey = ScenarioId.value model.Planning.Scenario

    let horizonPresets = Services.Horizon.presets()
    let currentHorizonName = Services.Horizon.nameOf model.Planning.Horizon

    let plants =
        [ "ALL", "All Plants"
          "PL-BER", "Berlin DC"
          "PL-MUN", "Munich DC"
          "PL-HAM", "Hamburg DC"
          "PL-FRA", "Frankfurt DC" ]

    let currentPlant = model.Planning.PlantId |> Option.defaultValue "ALL"

    comp<RadzenHeader> {
        comp<RadzenStack> {
            "Orientation" => Orientation.Horizontal
            "AlignItems" => AlignItems.Center
            "Gap" => "0.75rem"
            "Style" => "padding: 0 1rem; width: 100%;"

            // Radzen Sidebar Toggle (Top-Left Hamburger)
            comp<RadzenSidebarToggle> {
                attr.callback "Click" (fun (_: System.EventArgs) -> dispatch ToggleNavigationRequested)
            }

            // Brand Logo & Name
            Rz.icon("inventory_2", style = "font-size: 1.5rem; color: var(--rz-primary);")
            Rz.text("Medhavi APS", textStyle = TextStyle.H6)

            div { attr.style "width: 1px; height: 1.5rem; background: var(--rz-border-color); margin: 0 0.25rem;" }

            // Global Planning Scope Controls (Scenario, Horizon, Plant)
            comp<RadzenStack> {
                "Orientation" => Orientation.Horizontal
                "AlignItems" => AlignItems.Center
                "Gap" => "0.5rem"

                // Scenario Selector
                Rz.icon("science", style = "font-size: 1rem; color: var(--rz-secondary);")

                span {
                    attr.style "font-size: 12px; font-weight: 600; color: var(--rz-text-secondary-color);"
                    "Scenario:"
                }

                Rz.stringDropDown(
                    (scenarios |> List.map snd),
                    (scenarios
                     |> List.tryFind(fun (k, _) -> k = currentScenarioKey)
                     |> Option.map snd
                     |> Option.defaultValue "Baseline Plan"),
                    (fun selectedTitle ->
                        let matchedKey =
                            scenarios
                            |> List.tryFind(fun (_, t) -> t = selectedTitle)
                            |> Option.map fst
                            |> Option.defaultValue "baseline"

                        dispatch(SetScenario(ScenarioId.literal matchedKey)))
                )

                // Horizon Selector
                Rz.icon("date_range", style = "font-size: 1rem; color: var(--rz-secondary);")

                span {
                    attr.style "font-size: 12px; font-weight: 600; color: var(--rz-text-secondary-color);"
                    "Horizon:"
                }

                Rz.stringDropDown(
                    (horizonPresets |> List.map fst),
                    currentHorizonName,
                    (fun selectedName ->
                        let matchedHorizon =
                            horizonPresets
                            |> List.tryFind(fun (n, _) -> n = selectedName)
                            |> Option.map snd
                            |> Option.defaultValue(snd(List.head horizonPresets))

                        dispatch(SetHorizon matchedHorizon))
                )

                // Plant Scope Selector
                Rz.icon("factory", style = "font-size: 1rem; color: var(--rz-secondary);")

                span {
                    attr.style "font-size: 12px; font-weight: 600; color: var(--rz-text-secondary-color);"
                    "Plant:"
                }

                Rz.stringDropDown(
                    (plants |> List.map snd),
                    (plants
                     |> List.tryFind(fun (k, _) -> k = currentPlant)
                     |> Option.map snd
                     |> Option.defaultValue "All Plants"),
                    (fun selectedTitle ->
                        let matchedKey =
                            plants
                            |> List.tryFind(fun (_, t) -> t = selectedTitle)
                            |> Option.map fst
                            |> Option.defaultValue "ALL"

                        let plantOpt = if matchedKey = "ALL" then None else Some matchedKey
                        dispatch(SetPlant plantOpt))
                )
            }

            // Flexible Spacer
            div { attr.style "flex: 1;" }

            // Simple Command Palette Icon Button (Ctrl+K)
            button {
                attr.id "btn-command-palette"
                attr.``type`` "button"
                attr.title "Command Palette (Ctrl+K)"

                attr.style
                    """
                    display: inline-flex;
                    align-items: center;
                    justify-content: center;
                    width: 36px;
                    height: 36px;
                    background: transparent;
                    border: 1px solid var(--rz-border-color);
                    border-radius: 6px;
                    color: var(--rz-text-color);
                    cursor: pointer;
                    transition: background 0.15s ease, border-color 0.15s ease;
                    """

                attr.callback "onclick" (fun (_: MouseEventArgs) -> dispatch OpenCommandPaletteRequested)

                span {
                    attr.``class`` "rzi"
                    attr.style "font-size: 1.25rem; color: var(--rz-text-color);"
                    "terminal"
                }
            }

            // Notifications / SidePanel Trigger
            Rz.iconButton("notifications", onClick = (fun (_: MouseEventArgs) -> dispatch ToggleSidePanelRequested))

            // User Profile Menu
            comp<RadzenProfileMenu> {
                attr.callback "Click" (fun (args: RadzenProfileMenuItem) ->
                    match string args.Value with
                    | "settings" -> dispatch SettingsRequested
                    | "about" -> dispatch AboutRequested
                    | "logout" -> dispatch LogoutRequested
                    | _ -> ())

                comp<RadzenProfileMenuItem> {
                    "Text" => "Settings"
                    "Icon" => "settings"
                    "Value" => "settings"
                }

                comp<RadzenProfileMenuItem> {
                    "Text" => "About"
                    "Icon" => "info"
                    "Value" => "about"
                }

                comp<RadzenProfileMenuItem> {
                    "Text" => "Logout"
                    "Icon" => "logout"
                    "Value" => "logout"
                }
            }
        }
    }

type Component() =
    inherit ElmishComponent<Model, Msg>()

    override _.View model dispatch = view model dispatch
