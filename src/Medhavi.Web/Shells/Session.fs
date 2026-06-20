module Medhavi.Web.Session

open System
open Elmish
open Medhavi.Web
open Medhavi.Contracts
open Medhavi.Contracts.Scenario

type Model =
    { User: User
      Theme: UITheme
      ConnectionStatus: ConnectionStatus
      Notifications: Notification list
      Operations: Operation list
      Activities: UIEventLogItem list
      PlanningContext: PlanningContext
      CommandHistory: CommandTrace list }

    static member Default() =
        { Theme = UITheme.Dark
          User =
            { Name = "admin"
              Role = Role.Administrator }
          ConnectionStatus = ConnectionStatus.Connected
          Notifications = []
          Operations = []
          Activities = []
          PlanningContext = PlanningContext.Default()
          CommandHistory = [] }

type Msg =
    | SetTheme of UITheme

    // User messages
    | SetUser of User
    | CycleUserRole
    | TriggerLogout

    // Planning context
    | SetPlanningContext of PlanningContext
    | ContextChanged of PlanningContext

    // Notifications & Activities
    | SetConnectionStatus of ConnectionStatus
    | SetNotifications of Notification list
    | MarkAllNotificationsRead
    | ClearNotifications
    | ReceiveNotification of Notification
    | SetActivities of UIEventLogItem list
    | ActivitiesLoaded of UIEventLogItem list

    // Operations
    | StartOperation of id: Guid * name: string
    | UpdateOperationProgress of id: Guid * progressPercentage: int * currentStage: string
    | CompleteOperation of id: Guid
    | FailOperation of id: Guid * error: string
    | DismissOperation of id: Guid
    | SetOperations of Operation list
    | LogAction of CommandTrace
    | ClearCommandHistory

let init () = (Model.Default(), Cmd.none)

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | SetTheme theme -> { model with Theme = theme }, Cmd.none

    // User messages
    | SetUser user -> { model with User = user }, Cmd.none
    | CycleUserRole ->
        let nextRole =
            match model.User.Role with
            | Role.Planner -> Role.Supervisor
            | Role.Supervisor -> Role.Manager
            | Role.Manager -> Role.Administrator
            | Role.Administrator -> Role.Planner

        let updatedUser = { model.User with Role = nextRole }

        { model with User = updatedUser }, Cmd.none
    | TriggerLogout -> model, Cmd.none

    // Planning context
    | ContextChanged scope -> { model with PlanningContext = scope }, Cmd.none
    | SetPlanningContext _ -> model, Cmd.none

    // Notifications & Activities

    | MarkAllNotificationsRead ->
        let readList = model.Notifications |> List.map(fun n -> { n with IsRead = true })

        { model with Notifications = readList }, Cmd.none
    | ClearNotifications -> { model with Notifications = [] }, Cmd.none
    | ReceiveNotification n ->
        let currentList = n :: model.Notifications
        let trimmedList = if currentList.Length > 50 then List.take 50 currentList else currentList

        { model with
            Notifications = trimmedList },
        Cmd.none
    | ActivitiesLoaded feed ->
        { model with
            Activities = feed @ model.Activities },
        Cmd.none
    | SetActivities activities -> { model with Activities = activities }, Cmd.none
    | SetNotifications notifications ->
        { model with
            Notifications = notifications },
        Cmd.none
    | SetConnectionStatus status -> { model with ConnectionStatus = status }, Cmd.none

    // Operations

    | StartOperation(id, name) ->
        let op =
            { Id = id
              Name = name
              State = OperationState.Running(0, "Initializing") }

        { model with
            Operations = op :: model.Operations },
        Cmd.none
    | UpdateOperationProgress(id, progress, stage) ->
        let ops =
            model.Operations
            |> List.map(fun op ->
                if op.Id = id then
                    { op with
                        State = OperationState.Running(progress, stage) }
                else
                    op)

        { model with Operations = ops }, Cmd.none
    | CompleteOperation id ->
        let ops =
            model.Operations
            |> List.map(fun op ->
                if op.Id = id then
                    { op with
                        State = OperationState.Completed() }
                else
                    op)

        { model with Operations = ops }, Cmd.none
    | FailOperation(id, err) ->
        let ops =
            model.Operations
            |> List.map(fun op ->
                if op.Id = id then
                    { op with
                        State = OperationState.Failed err }
                else
                    op)

        { model with Operations = ops }, Cmd.none
    | DismissOperation id ->
        let ops = model.Operations |> List.filter(fun op -> op.Id <> id)

        { model with Operations = ops }, Cmd.none

    | SetOperations ops -> { model with Operations = ops }, Cmd.none
    | LogAction trace ->
        let updatedHistory = (trace :: model.CommandHistory) |> List.truncate 20

        { model with
            CommandHistory = updatedHistory },
        Cmd.none
    | ClearCommandHistory -> { model with CommandHistory = [] }, Cmd.none
