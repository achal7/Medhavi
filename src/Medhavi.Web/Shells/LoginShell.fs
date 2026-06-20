module Medhavi.Web.LoginShell

open Elmish
open Bolero.Html
open Medhavi.Web.Components
open Medhavi.Web.Controls
open Radzen
open Radzen.Blazor
open Medhavi.Contracts

type LoginEnv =
    { Authenticate: string * string -> Async<Result<User, string>> }

type Model =
    { ErrorMessage: string option
      IsBusy: bool }

type Msg =
    | SubmitLogin of username: string * password: string
    | LoginFailed of string
    | LoginSuccess of User

type Output = LoggedIn of User

let init () = { ErrorMessage = None; IsBusy = false }, Cmd.none

let update env msg model =
    match msg with
    | SubmitLogin(username, pwd) ->
        let cmd =
            Cmd.OfAsync.either
                env.Authenticate
                (username, pwd)
                (function
                | Ok user -> LoginSuccess user
                | Error error -> LoginFailed error)
                (fun ex -> LoginFailed ex.Message)

        { model with
            IsBusy = true
            ErrorMessage = None },
        cmd,
        None
    | LoginFailed err ->
        { model with
            ErrorMessage = Some err
            IsBusy = false },
        Cmd.none,
        None
    | LoginSuccess user -> { model with IsBusy = false }, Cmd.none, Some(LoggedIn user)

let view (model: Model) (dispatch: Msg -> unit) =
    let handleLogin (username, password) = dispatch(SubmitLogin(username, password))

    div {
        attr.``class`` "login-page"

        Rz.row
            [
              // Left column – welcome panel (visible on medium+ screens)
              6,
              [ Rz.card(
                    [ Rz.stack(
                          [ Rz.icon("bubble_chart", class' = "icon-large")
                            Rz.text(
                                "Welcome!",
                                textStyle = TextStyle.DisplayH3,
                                tagName = TagName.H2,
                                class' = "rz-color-white rz-m-0"
                            )
                            Rz.text(
                                "Medhavi APS Client Portal",
                                textStyle = TextStyle.H6,
                                tagName = TagName.H3,
                                class' = "rz-color-white rz-m-0"
                            ) ],
                          orientation = Orientation.Vertical
                      ) ],
                    class' = "welcome-panel"
                ) ]
              // Right column – login form
              6,
              [ Rz.card(
                    [ Rz.text("Login", textStyle = TextStyle.H5, tagName = TagName.H2, class' = "rz-mb-6 rz-mt-0")
                      match model.ErrorMessage with
                      | Some err ->
                          Rz.alert(err, alertStyle = AlertStyle.Danger, variant = Variant.Flat, shade = Shade.Dark)
                      | None -> empty()
                      Rz.login(handleLogin) ],
                    class' = "login-form-card"
                ) ] ]
    }
