module Medhavi.Web.AppShell.WorkspaceEngine

open Elmish
open Medhavi.Web
open Medhavi.Web.Workspaces
open Medhavi.Web.Stores

let makeMaterialReservationEnv (env: AppShellEnv) : MaterialReservation.ReservationEnv =
    { DemandLineQueries = env.DemandLineQueries }

let makeMasterDataEnv (env: AppShellEnv) : MasterData.MasterDataEnv = { MasterDataQueries = env.MasterDataService }

let private navigateToWorkspace (model: AppShellModel) (workspace: Workspace) : AppShellModel * Cmd<Message> =
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

    | Workspace.MasterData ->
        match model.MasterDataState with
        | Some _ ->
            { model with
                ActiveWorkspace = Some workspace },
            Cmd.none
        | None ->
            let mdSubModel, mdSubCmd = MasterData.init model.Session.PlanningContext

            { model with
                MasterDataState = Some mdSubModel
                ActiveWorkspace = Some workspace },
            Cmd.map MasterDataMsg mdSubCmd
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
            model, Cmd.ofMsg(ReservationWorkspaceMsg MaterialReservation.Msg.Initialize)
        | _ -> model, Cmd.none
    | _ -> model, Cmd.none
