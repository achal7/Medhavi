namespace Medhavi.Web

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Server.Html
open Microsoft.AspNetCore.Components

module SystemOrchestrator =

    type Model = {
        CurrentUser: User option
        LoginState: LoginShell.Model
    }

    type Message =
        | LoginMsg of LoginShell.Msg
        | Logout

    let init () =
        {
            CurrentUser = Some { Username = "admin"; Email = "admin@medhavi.com"; Role = Role.Supervisor }
            LoginState = LoginShell.init()
        }, Cmd.none

    let update (authService: Services.AuthService) msg model =
        match msg with
        | LoginMsg loginMsg ->
            match loginMsg with
            | LoginShell.LoginSuccess user ->
                { model with CurrentUser = Some user }, Cmd.none
            | LoginShell.SubmitLogin (username, password) ->
                let loginModel, loginCmd = LoginShell.update loginMsg model.LoginState
                let authCmd = 
                    Cmd.OfAsync.either
                        (fun () -> authService.Authenticate(username, password) |> Async.AwaitTask)
                        ()
                        (function 
                            | Ok user -> LoginMsg (LoginShell.LoginSuccess user)
                            | Error err -> LoginMsg (LoginShell.LoginFailed err))
                        (fun ex -> LoginMsg (LoginShell.LoginFailed ex.Message))
                { model with LoginState = loginModel }, Cmd.batch [ loginCmd; authCmd ]
            | _ ->
                let loginModel, loginCmd = LoginShell.update loginMsg model.LoginState
                { model with LoginState = loginModel }, Cmd.map LoginMsg loginCmd
        | Logout ->
            { model with CurrentUser = None; LoginState = LoginShell.init() }, Cmd.none

type App() =
    inherit ProgramComponent<SystemOrchestrator.Model, SystemOrchestrator.Message>()
    
    [<Inject>]
    member val AuthService = Unchecked.defaultof<Services.AuthService> with get, set
    
    override this.Program =
        let init () = SystemOrchestrator.init()
        let update msg model = SystemOrchestrator.update this.AuthService msg model
        
        let view (model: SystemOrchestrator.Model) dispatch =
            match model.CurrentUser with
            | None ->
                LoginShell.view model.LoginState (fun msg -> dispatch (SystemOrchestrator.LoginMsg msg))
            | Some user ->
                comp<AppShell.AppShellComponent> {
                    "CurrentUser" => user
                    "OnLogout" => EventCallback.Factory.Create(this, Action(fun () -> dispatch SystemOrchestrator.Logout))
                }

        Program.mkProgram (fun _ -> init ()) update view

[<Route "/{*path}">]
type SystemPage() =
    inherit Component()
    
    override this.Render() =
        doctypeHtml {
            head {
                meta { attr.charset "UTF-8" }
                meta { attr.name "viewport"; attr.content "width=device-width, initial-scale=1.0" }
                title { "Medhavi APS Workbench" }
                Bolero.Html.``base`` { attr.href "/" }
                link { attr.rel "stylesheet"; attr.href "_content/Radzen.Blazor/css/standard.css" }
                link { attr.rel "stylesheet"; attr.href "css/index.css" }
            }
            body {
                comp<App> { attr.empty() }
                script { attr.src "_content/Radzen.Blazor/Radzen.Blazor.js" }
                script { attr.src "js/reconnect.js" }
                boleroScript
            }
        }
