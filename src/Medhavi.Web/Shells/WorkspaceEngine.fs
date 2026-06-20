module Medhavi.Web.AppShell.WorkspaceEngine

open Elmish
open Medhavi.Web
open Medhavi.Web.Workspaces
open Medhavi.Web.Stores

let makeMaterialReservationEnv (env: AppShellEnv) : MaterialReservation.ReservationEnv =
    { DemandLineQueries = env.DemandLineQueries }

let private navigateToWorkspace
    (model: AppShellModel)
    (workspace: Workspace)
    : AppShellModel * Cmd<Message> =
    match workspace with
    | Workspace.MaterialReservation ->
        match model.MaterialReservationState with
        | Some _ ->
            { model with
                ActiveWorkspace = Some workspace },
            Cmd.none
        | None ->
            let rSubModel, rSubCmd = MaterialReservation.init model.Session.PlanningContext

            { model with
                MaterialReservationState = Some rSubModel
                ActiveWorkspace = Some workspace },
            Cmd.map ReservationWorkspaceMsg rSubCmd
    | _ ->
        { model with
            ActiveWorkspace = Some workspace },
        Cmd.none

let executeWorkspaceAction
    (appEnv: AppShellEnv)
    (model: AppShellModel)
    (action: WorkspaceAction)
    : AppShellModel * Cmd<Message> =
    match action with
    | WorkspaceAction.NavigateTo w -> navigateToWorkspace model w
    | WorkspaceAction.RefreshActiveWorkspace ->
        match model.ActiveWorkspace with
        | Some Workspace.MaterialReservation ->
            model, Cmd.ofMsg (ReservationWorkspaceMsg MaterialReservation.Msg.Initialize)
        | _ -> model, Cmd.none
    | _ -> model, Cmd.none
