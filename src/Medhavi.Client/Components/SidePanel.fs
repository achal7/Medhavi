namespace Medhavi.Client.Components

open Microsoft.AspNetCore.Components
open Bolero
open Bolero.Html
open Elmish
open Medhavi.Client.Controls
open Radzen

module SidePanel =

    type PanelTab =
        | Detail
        | Notifications
        | AI
        | Transactions

    type Model =
        { ActiveTab: PanelTab
          IsOpen: bool
          Width: int }

    type Msg =
        | TabChanged of PanelTab
        | ToggleOpen
        | ResizeStart of int
        | ResizeEnd

    [<RequireQualifiedAccess>]
    type Output =
        | Closed
        | TabSelected of PanelTab

    let init () : Model * Cmd<Msg> =
        { ActiveTab = Detail
          IsOpen = false
          Width = 320 },
        Cmd.none

    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> * Output option =
        match msg with
        | TabChanged tab -> { model with ActiveTab = tab }, Cmd.none, Some(Output.TabSelected tab)

        | ToggleOpen ->
            { model with IsOpen = not model.IsOpen }, Cmd.none, (if not model.IsOpen then None else Some Output.Closed)

        | ResizeStart _
        | ResizeEnd -> model, Cmd.none, None

    let view (model: Model) (dispatch: Msg -> unit) : Node = div { }
