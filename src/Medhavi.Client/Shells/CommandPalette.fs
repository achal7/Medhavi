module Medhavi.Client.Shells.CommandPalette

open System
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Bolero
open Bolero.Html
open Elmish
open Medhavi.Client.Controls

type CommandItem =
    { Id: string
      Label: string
      Icon: string
      Category: string
      Action: string }

type Model =
    { IsOpen: bool
      SearchText: string
      AllCommands: CommandItem list
      FilteredCommands: CommandItem list
      SelectedIndex: int }

type Msg =
    | Open
    | Close
    | SearchTextChanged of string
    | NavigateUp
    | NavigateDown
    | ExecuteSelected
    | CommandClicked of CommandItem

[<RequireQualifiedAccess>]
type Output =
    | ExecuteCommand of CommandItem

let defaultCommands: CommandItem list =
    [ { Id = "cmd-view-kpi"
        Label = "Executive KPI & Planning Dashboard"
        Icon = "analytics"
        Category = "Views"
        Action = "view-executive-kpi-dashboard" }
      { Id = "cmd-view-pegging-dash"
        Label = "Demand & Supply Workspace (Tile Dashboard)"
        Icon = "dashboard"
        Category = "Views"
        Action = "view-demand-supply-pegging-dashboard" }
      { Id = "cmd-view-demand"
        Label = "Demand Orders Workbench"
        Icon = "view_list"
        Category = "Views"
        Action = "view-demand-workbench" }
      { Id = "cmd-view-supply"
        Label = "Supply Orders Workbench"
        Icon = "local_shipping"
        Category = "Views"
        Action = "view-supply-workbench" }
      { Id = "cmd-view-pegging-split"
        Label = "Demand-Supply Pegging (Split Pane)"
        Icon = "splitscreen"
        Category = "Views"
        Action = "view-demand-supply-pegging-split" }
      { Id = "cmd-toggle-edit"
        Label = "Toggle Layout Edit Mode (Drag & Resize Tiles)"
        Icon = "dashboard_customize"
        Category = "Layout"
        Action = "action:toggle-edit" }
      { Id = "cmd-scen-base"
        Label = "Switch Scenario: Baseline Plan"
        Icon = "science"
        Category = "Scenario"
        Action = "scenario:baseline" }
      { Id = "cmd-scen-whatif1"
        Label = "Switch Scenario: What-If: Supplier Delay"
        Icon = "science"
        Category = "Scenario"
        Action = "scenario:what-if-1" }
      { Id = "cmd-scen-whatif2"
        Label = "Switch Scenario: What-If: Outage"
        Icon = "science"
        Category = "Scenario"
        Action = "scenario:what-if-2" }
      { Id = "cmd-plant-all"
        Label = "Plant Scope: All Plants"
        Icon = "factory"
        Category = "Scope"
        Action = "plant:ALL" }
      { Id = "cmd-plant-ber"
        Label = "Plant Scope: Berlin DC"
        Icon = "factory"
        Category = "Scope"
        Action = "plant:PL-BER" }
      { Id = "cmd-plant-mun"
        Label = "Plant Scope: Munich DC"
        Icon = "factory"
        Category = "Scope"
        Action = "plant:PL-MUN" } ]

let private filterCommands (query: string) (commands: CommandItem list) =
    if String.IsNullOrWhiteSpace query then
        commands
    else
        let q = query.Trim().ToLowerInvariant()
        commands
        |> List.filter (fun c ->
            c.Label.ToLowerInvariant().Contains(q)
            || c.Category.ToLowerInvariant().Contains(q)
            || c.Action.ToLowerInvariant().Contains(q))

let init () : Model * Cmd<Msg> =
    { IsOpen = false
      SearchText = ""
      AllCommands = defaultCommands
      FilteredCommands = defaultCommands
      SelectedIndex = 0 },
    Cmd.none

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> * Output option =
    match msg with
    | Open ->
        { model with
            IsOpen = true
            SearchText = ""
            FilteredCommands = model.AllCommands
            SelectedIndex = 0 },
        Cmd.none,
        None

    | Close ->
        { model with IsOpen = false }, Cmd.none, None

    | SearchTextChanged text ->
        let filtered = filterCommands text model.AllCommands
        { model with
            SearchText = text
            FilteredCommands = filtered
            SelectedIndex = 0 },
        Cmd.none,
        None

    | NavigateUp ->
        { model with
            SelectedIndex = max 0 (model.SelectedIndex - 1) },
        Cmd.none,
        None

    | NavigateDown ->
        let maxIdx = max 0 (model.FilteredCommands.Length - 1)
        { model with
            SelectedIndex = min maxIdx (model.SelectedIndex + 1) },
        Cmd.none,
        None

    | ExecuteSelected ->
        if model.SelectedIndex >= 0 && model.SelectedIndex < model.FilteredCommands.Length then
            let cmd = model.FilteredCommands.[model.SelectedIndex]
            { model with IsOpen = false }, Cmd.none, Some(Output.ExecuteCommand cmd)
        else
            model, Cmd.none, None

    | CommandClicked cmd ->
        { model with IsOpen = false }, Cmd.none, Some(Output.ExecuteCommand cmd)

let view (model: Model) (dispatch: Msg -> unit) : Node =
    if not model.IsOpen then
        empty()
    else
        div {
            // Backdrop Overlay
            attr.style
                """
                position: fixed;
                top: 0;
                left: 0;
                width: 100vw;
                height: 100vh;
                background: rgba(0, 0, 0, 0.65);
                backdrop-filter: blur(4px);
                z-index: 99999;
                display: flex;
                align-items: flex-start;
                justify-content: center;
                padding-top: 10vh;
                box-sizing: border-box;
                """

            on.click (fun (_: MouseEventArgs) -> dispatch Close)

            // Palette Card (stops backdrop click)
            div {
                attr.style
                    """
                    background: var(--rz-base-background-color);
                    border: 1px solid var(--rz-border-color);
                    border-radius: 12px;
                    width: 90%;
                    max-width: 600px;
                    box-shadow: 0 16px 40px rgba(0, 0, 0, 0.35);
                    display: flex;
                    flex-direction: column;
                    overflow: hidden;
                    animation: fadeInDown 0.15s ease-out;
                    """

                on.click (fun (e: MouseEventArgs) -> ())

                // Search Header
                div {
                    attr.style
                        """
                    display: flex;
                    align-items: center;
                    gap: 0.75rem;
                    padding: 0.85rem 1.25rem;
                    border-bottom: 1px solid var(--rz-border-color);
                    background: var(--rz-base-background-color);
                    """

                    span {
                        attr.``class`` "rzi"
                        attr.style "font-size: 1.35rem; color: var(--rz-primary);"
                        "search"
                    }

                    input {
                        attr.``type`` "text"
                        attr.placeholder "Type a command, workbench, or scenario... (ESC to close)"
                        attr.value model.SearchText
                        attr.autofocus true
                        attr.style
                            """
                            flex: 1;
                            border: none;
                            outline: none;
                            font-size: 15px;
                            color: var(--rz-text-color);
                            background: transparent;
                            font-family: inherit;
                            """

                        on.input (fun (e: ChangeEventArgs) -> dispatch (SearchTextChanged (string e.Value)))

                        on.keydown (fun (e: KeyboardEventArgs) ->
                            match e.Key with
                            | "Escape" -> dispatch Close
                            | "ArrowUp" -> dispatch NavigateUp
                            | "ArrowDown" -> dispatch NavigateDown
                            | "Enter" -> dispatch ExecuteSelected
                            | _ -> ())
                    }

                    kbd {
                        attr.style
                            """
                            font-family: inherit;
                            font-size: 10px;
                            font-weight: 700;
                            background: var(--rz-border-color);
                            color: var(--rz-text-secondary-color);
                            padding: 2px 6px;
                            border-radius: 4px;
                            """
                        "ESC"
                    }
                }

                // Command Results List
                div {
                    attr.style
                        """
                        max-height: 380px;
                        overflow-y: auto;
                        padding: 0.5rem;
                        display: flex;
                        flex-direction: column;
                        gap: 2px;
                        """

                    if List.isEmpty model.FilteredCommands then
                        div {
                            attr.style
                                """
                                padding: 2rem;
                                text-align: center;
                                color: var(--rz-text-secondary-color);
                                font-size: 13px;
                                """
                            "No matching commands found."
                        }
                    else
                        forEach (List.indexed model.FilteredCommands) (fun (i, cmd) ->
                            let isSelected = i = model.SelectedIndex

                            div {
                                attr.style
                                    (sprintf
                                        """
                                        display: flex;
                                        align-items: center;
                                        justify-content: space-between;
                                        padding: 0.65rem 0.85rem;
                                        border-radius: 6px;
                                        cursor: pointer;
                                        background: %s;
                                        transition: background 0.1s ease;
                                        """
                                        (if isSelected then "rgba(var(--rz-primary-rgb, 67, 80, 222), 0.12)" else "transparent"))

                                on.click (fun (_: MouseEventArgs) -> dispatch (CommandClicked cmd))

                                div {
                                    attr.style "display: flex; align-items: center; gap: 0.75rem; min-width: 0;"

                                    span {
                                        attr.``class`` "rzi"
                                        attr.style
                                            (sprintf
                                                "font-size: 1.15rem; color: %s;"
                                                (if isSelected then "var(--rz-primary)" else "var(--rz-text-secondary-color)"))
                                        cmd.Icon
                                    }

                                    span {
                                        attr.style
                                            (sprintf
                                                "font-size: 13px; font-weight: %s; color: var(--rz-text-color); white-space: nowrap; overflow: hidden; text-overflow: ellipsis;"
                                                (if isSelected then "600" else "400"))
                                        cmd.Label
                                    }
                                }

                                span {
                                    attr.style
                                        """
                                        font-size: 11px;
                                        font-weight: 600;
                                        text-transform: uppercase;
                                        letter-spacing: 0.5px;
                                        color: var(--rz-text-secondary-color);
                                        background: var(--rz-border-color);
                                        padding: 2px 6px;
                                        border-radius: 4px;
                                        """
                                    cmd.Category
                                }
                            })
                }

                // Footer Hints
                div {
                    attr.style
                        """
                        display: flex;
                        align-items: center;
                        justify-content: space-between;
                        padding: 0.4rem 1rem;
                        border-top: 1px solid var(--rz-border-color);
                        background: rgba(0, 0, 0, 0.03);
                        font-size: 11px;
                        color: var(--rz-text-secondary-color);
                        """

                    div {
                        attr.style "display: flex; gap: 0.75rem;"
                        span { "↑↓ Navigate" }
                        span { "↵ Select" }
                        span { "ESC Close" }
                    }

                    span { "Medhavi Command Navigator" }
                }
            }
        }

type Component() =
    inherit ElmishComponent<Model, Msg>()

    override _.View model dispatch = view model dispatch
