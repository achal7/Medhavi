namespace Medhavi.Client

open System.Threading.Tasks
open Microsoft.AspNetCore.Components
open Bolero
open Elmish
open Radzen
open Medhavi.Contracts.Client
open Medhavi.Client.Shells
open Bolero.Html
open Medhavi.Client.Components
open Medhavi.Client.Primitives
open System

type Model =
    | Unauthenticated
    | Authenticated of UserShell.Model

type Message =
    | LoginSucceeded
    | LogoutRequested
    | ShowNotification of Services.NotificationRequest
    | ApplyTheme of VisualTheme
    | ApplyCulture of CulturalPreferences
    | UserShellMsg of UserShell.Msg

type Application() =
    inherit ProgramComponent<Model, Message>()

    let mapToSeverity =
        fun (notificationType: Services.NotificationType) ->
            match notificationType with
            | Services.NotificationType.Error' -> NotificationSeverity.Error
            | Services.NotificationType.Info -> NotificationSeverity.Info
            | Services.NotificationType.Success -> NotificationSeverity.Success
            | Services.NotificationType.Warning -> NotificationSeverity.Warning
            | _ -> NotificationSeverity.Info

    let view ctx model dispatch =
        div {
            comp<RadzenOutlets> { }

            match model with
            | Unauthenticated ->
                div {
                    attr.style "display: flex; align-items: center; justify-content: center; height: 100vh;"
                    "Login screen (not implemented in V1)"
                }
            | Authenticated shellModel ->
                ecomp<UserShell.Component, _, _> shellModel (UserShellMsg >> dispatch) { "Context" => ctx }
        }

    let initModel = Unauthenticated, Cmd.ofMsg LoginSucceeded

    let init () =
        let model, cmd = initModel
        model, cmd

    let handleUserShellOutput (output: UserShell.Output) (model: Model) : Model * Cmd<Message> =
        match output with
        | UserShell.Output.Logout -> model, Cmd.ofMsg LogoutRequested
        | UserShell.Output.Notify req -> model, Cmd.ofMsg(ShowNotification req)
        | UserShell.Output.ChangeTheme theme -> model, Cmd.ofMsg(ApplyTheme theme)
        | UserShell.Output.ChangeCulture culture -> model, Cmd.ofMsg(ApplyCulture culture)

    let update ctx (themeService: ThemeService) (nservice: Radzen.NotificationService) msg model =
        match msg with
        | LoginSucceeded ->
            let usershell, cmd = UserShell.init ctx
            Authenticated usershell, Cmd.map UserShellMsg cmd

        | UserShellMsg msg ->
            match model with
            | Authenticated shellModel ->
                let updatedUserShellModel, shellCmd, output = UserShell.update ctx msg shellModel
                let model' = Authenticated updatedUserShellModel
                let mappedCommand = Cmd.map UserShellMsg shellCmd

                let outputCmd =
                    match output with
                    | Some output ->
                        let _, cmd = handleUserShellOutput output model'
                        cmd
                    | None -> Cmd.none

                model', Cmd.batch [ mappedCommand; outputCmd ]

            | _ -> model, Cmd.none

        | ApplyTheme theme ->
            let cssName = VisualTheme.cssName theme

            if not(isNull(box themeService)) then
                themeService.SetTheme(cssName, true)

            model, Cmd.none

        | ApplyCulture culture ->
            let ci = Formatting.toCultureInfo culture
            System.Globalization.CultureInfo.CurrentCulture <- ci
            System.Globalization.CultureInfo.CurrentUICulture <- ci
            model, Cmd.none

        | LogoutRequested -> Unauthenticated, Cmd.none

        | ShowNotification req ->
            let notificationMsg = NotificationMessage()
            notificationMsg.Detail <- req.Details |> Option.defaultValue ""
            notificationMsg.Summary <- req.Message
            notificationMsg.Severity <- mapToSeverity req.Type

            notificationMsg.Duration <-
                req.AutoDismissAfter |> Option.map(fun x -> x.TotalMilliseconds) |> Option.defaultValue 3000.0

            nservice.Notify(notificationMsg)
            model, Cmd.none

    [<Inject>]
    member val ThemeService: ThemeService = Unchecked.defaultof<ThemeService> with get, set

    [<Inject>]
    member val NotificationService: NotificationService = Unchecked.defaultof<NotificationService> with get, set

    [<Inject>]
    member val DialogService: DialogService = Unchecked.defaultof<DialogService> with get, set

    [<Inject>]
    member val DomainServices: DomainServices = Unchecked.defaultof<DomainServices> with get, set

    [<Inject>]
    member val Services: IServiceProvider = Unchecked.defaultof<IServiceProvider> with get, set

    override this.Program =
        //Medhavi.Client.SemanticModel.Tests.report()
        //Medhavi.Client.Rendering.Tests.ViewSurfaceTests.report()
        let componentRegistry = ComponentCatalog.components
        let schemaRegistry = ComponentCatalog.schemas

        let baselinePlanningContext =
            { Scenario = ScenarioId.baseline
              Horizon =
                { Start = DateTimeOffset.Now
                  End = DateTimeOffset.Now.AddDays(30) }
              PlantId = None }

        let pctx = Services.PlanningContextService.create baselinePlanningContext

        let ctx =
            { PlanningContextService = pctx
              ComponentRegistry = componentRegistry
              SchemaRegistry = schemaRegistry
              DataStoreRegistry = ComponentCatalog.dataStores
              ActionRegistry = ComponentCatalog.actionRegistry
              Services = this.Services
              DomainServices = if isNull (box this.DomainServices) then None else Some this.DomainServices }
            : ApplicationContext

        let view model dispatch = view ctx model dispatch

        Program.mkProgram (fun _ -> init()) (update ctx this.ThemeService this.NotificationService) view
