module Medhavi.Web.SystemShell

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Server.Html
open Microsoft.AspNetCore.Components
open Radzen
open Radzen.Blazor
open Medhavi.Contracts
open Medhavi.Contracts.Demand
open Medhavi.Contracts.Scenario
open Medhavi.Web.AppShell
open Medhavi.Web.Stores
open Medhavi.SharedKernel.BoundedContexts

type IAuthApplicationService =
    abstract Authenticate: string -> string -> Async<Result<User, string>>

type Env =
    { Authentication: IAuthApplicationService
      DemandLineApi: DemandLineApi
      DemandLineQueries: DemandLineQueries
      StoreRegistry: WorkspaceStoreRegistry
      TooltipService: TooltipService
      MasterDataService: MasterDataService }

type Model =
    { User: User option
      LoginState: LoginShell.Model
      AppState: AppShell.AppShellModel }

type Message =
    | LoginMsg of LoginShell.Msg
    | AppShellMsg of AppShell.Message

let init () =
    let login, _ = LoginShell.init()
    let session, _ = Session.init()
    let app, _ = AppShell.AppShellEngine.init session

    let model =
        { User =
            Some
                { Name = "admin"
                  Role = Role.Administrator }
          LoginState = login
          AppState = app }

    let eventSubDispatch dispatch =
        DomainEventBus.Subscribe<DemandCreatedNotification>(fun n ->
            printfn $"[SystemShell] DemandCreatedNotification received: %s{n.DemandLineId}. Refreshing workspace."
            dispatch(AppShellMsg(ExecuteWorkspaceAction WorkspaceAction.RefreshActiveWorkspace))

            let notif: Medhavi.Web.Notification =
                { Id = Guid.NewGuid()
                  Category = "Demand"
                  Title = "Demand Created"
                  Message = sprintf "Demand line %s was created." n.DemandLineId
                  Timestamp = DateTime.Now
                  IsRead = false }

            dispatch(AppShellMsg(SessionMsg(Medhavi.Web.Session.Msg.ReceiveNotification notif))))
        |> ignore

        DomainEventBus.Subscribe<DemandUpdatedNotification>(fun n ->
            printfn $"[SystemShell] DemandUpdatedNotification received: %s{n.DemandLineId}. Refreshing workspace."
            dispatch(AppShellMsg(ExecuteWorkspaceAction WorkspaceAction.RefreshActiveWorkspace))

            let notif: Medhavi.Web.Notification =
                { Id = Guid.NewGuid()
                  Category = "Demand"
                  Title = "Demand Updated"
                  Message = sprintf "Demand line %s was updated." n.DemandLineId
                  Timestamp = DateTime.Now
                  IsRead = false }

            dispatch(AppShellMsg(SessionMsg(Medhavi.Web.Session.Msg.ReceiveNotification notif))))
        |> ignore

        DomainEventBus.Subscribe<DemandDeletedNotification>(fun n ->
            printfn $"[SystemShell] DemandDeletedNotification received: %s{n.DemandLineId}. Refreshing workspace."
            dispatch(AppShellMsg(ExecuteWorkspaceAction WorkspaceAction.RefreshActiveWorkspace))

            let notif: Medhavi.Web.Notification =
                { Id = Guid.NewGuid()
                  Category = "Demand"
                  Title = "Demand Deleted"
                  Message = sprintf "Demand line %s was deleted." n.DemandLineId
                  Timestamp = DateTime.Now
                  IsRead = false }

            dispatch(AppShellMsg(SessionMsg(Medhavi.Web.Session.Msg.ReceiveNotification notif))))
        |> ignore

    let subCmd = [ eventSubDispatch ]

    model, subCmd

let private handleLoginOutput env (output: LoginShell.Output) (model: Model) =
    match output with
    | LoginShell.LoggedIn user ->
        let updatedAppState =
            { model.AppState with
                Session =
                    { model.AppState.Session with
                        User = user } }
            |> AppShell.AppShellEngine.syncAppbarState

        { model with
            User = Some user
            AppState = updatedAppState },
        Cmd.none

let private handleAppShellOutput (output: AppShell.Output) model =
    match output with
    | AppShell.Output.Logout -> { model with User = None }, Cmd.none

let update (env: Env) msg (model: Model) =
    match msg with
    | LoginMsg msg ->
        let loginEnv: LoginShell.LoginEnv =
            { Authenticate = fun (user, pwd) -> env.Authentication.Authenticate user pwd }

        updateChild
            (fun m -> m.LoginState)
            (fun child m -> { m with LoginState = child })
            LoginMsg
            (LoginShell.update loginEnv)
            (handleLoginOutput env)
            msg
            model
    | AppShellMsg msg ->
        let appShellEnv: AppShell.AppShellEnv =
            { DemandLineQueries = env.DemandLineQueries
              StoreRegistry = env.StoreRegistry
              TooltipService = env.TooltipService
              MasterDataService = env.MasterDataService }

        updateChild
            (fun m -> m.AppState)
            (fun child m -> { m with AppState = child })
            AppShellMsg
            (AppShell.AppShellEngine.update appShellEnv)
            handleAppShellOutput
            msg
            model

let view (env: Env) (model: Model) dispatch : Node =
    match model.User with
    | None -> LoginShell.view model.LoginState (LoginMsg >> dispatch)
    | Some _ ->
        let appShellEnv: AppShell.AppShellEnv =
            { DemandLineQueries = env.DemandLineQueries
              StoreRegistry = env.StoreRegistry
              TooltipService = env.TooltipService
              MasterDataService = env.MasterDataService }

        div {
            comp<RadzenTooltip> { attr.empty() }

            ecomp<AppShell.AppShellEngine.Component, AppShell.AppShellModel, AppShell.Message>
                model.AppState
                (AppShellMsg >> dispatch) {
                "Env" => appShellEnv
            }
        }

type App() =
    inherit ProgramComponent<Model, Message>()

    [<Inject>]
    member val AuthService: IAuthApplicationService = Unchecked.defaultof<_> with get, set

    [<Inject>]
    member val DemandLineApi: DemandLineApi = Unchecked.defaultof<_> with get, set

    [<Inject>]
    member val DemandLineQueries: DemandLineQueries = Unchecked.defaultof<_> with get, set

    [<Inject>]
    member val MasterDataService: MasterDataService = Unchecked.defaultof<_> with get, set

    [<Inject>]
    member val TooltipService: TooltipService = null with get, set

    override this.Program =
        let initialContext = PlanningContext.Default()
        let storeRegistry, _ = StoreComposition.createRegistry this.DemandLineApi this.DemandLineQueries initialContext

        let env =
            { Authentication = this.AuthService
              DemandLineApi = this.DemandLineApi
              DemandLineQueries = this.DemandLineQueries
              StoreRegistry = storeRegistry
              TooltipService = this.TooltipService
              MasterDataService = this.MasterDataService }

        Program.mkProgram (fun _ -> init()) (update env) (view env)

[<Route "/{*path}">]
type Root() =
    inherit Component()

    override _.Render() : Node =
        doctypeHtml {
            head {
                meta { attr.charset "UTF-8" }

                meta {
                    attr.name "viewport"
                    attr.content "width=device-width, initial-scale=1.0"
                }

                title { "Medhavi APS" }
                Html.``base`` { attr.href "/" }

                link {
                    attr.rel "stylesheet"
                    attr.href "_content/Radzen.Blazor/css/standard.css"
                }

                link {
                    attr.rel "stylesheet"
                    attr.href "css/index.css"
                }
            }

            body {
                comp<App> { attr.renderMode Web.RenderMode.InteractiveServer }
                script { attr.src "_content/Radzen.Blazor/Radzen.Blazor.js" }
                script { attr.src "js/utilities.js" }
                boleroScript
            }
        }
