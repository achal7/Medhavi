module Medhavi.Web.Workspaces.MaterialReservation

open System
open Elmish
open Bolero
open Bolero.Html
open Medhavi.Contracts.Scenario
open Medhavi.Contracts.Demand
open Medhavi.Contracts.Supply
open Medhavi.Web.Panels

type Action =
    | Refresh
    | SelectReservation of string
    | ReleaseReservation of string

type ReservationEnv = { DemandLineQueries: DemandLineQueries }

type Model =
    { Context: PlanningContext
      DemandPanel: DemandPanel.Model }

type Msg =
    | Initialize
    | DemandsLoaded of Result<DemandLine list, string>
    | DemandPanelMsg of DemandPanel.Msg
    | SearchDemandAction of string
    | SelectDemandAction of string
    | WorkspaceAction of Action

type Output = | DemandLoaded

let init (ctx: PlanningContext) =
    { Context = ctx
      DemandPanel = DemandPanel.init() },
    Cmd.ofMsg Initialize

let executeAction (action: Action) (model: Model) : Model * Cmd<Msg> = model, Cmd.none

let update (env: ReservationEnv) (msg: Msg) (model: Model) : Model * Cmd<Msg> * Output option =
    match msg with
    | Initialize ->
        let loadCmd =
            Cmd.OfTask.either (fun () -> env.DemandLineQueries.GetAll()) () (Ok >> DemandsLoaded) (fun ex ->
                DemandsLoaded(Error ex.Message))

        match model.DemandPanel.Demands with
        | RemoteData.Loaded _ ->
            model, loadCmd, None
        | _ ->
            let childModel, childCmd = DemandPanel.update (DemandPanel.Msg.SetDemands RemoteData.Loading) model.DemandPanel
            { model with DemandPanel = childModel }, Cmd.batch [ loadCmd; Cmd.map DemandPanelMsg childCmd ], None

    | WorkspaceAction action ->
        match action with
        | Refresh -> model, Cmd.none, None
        | SelectReservation reservationId -> model, Cmd.none, None
        | ReleaseReservation reservationId -> model, Cmd.none, None

    | DemandsLoaded result ->
        let panelState =
            match result with
            | Ok list -> RemoteData.Loaded list
            | Error err -> RemoteData.Failed err

        let childModel, childCmd = DemandPanel.update (DemandPanel.Msg.SetDemands panelState) model.DemandPanel

        { model with DemandPanel = childModel }, Cmd.map DemandPanelMsg childCmd, None

    | DemandPanelMsg childMsg ->
        let childModel, childCmd = DemandPanel.update childMsg model.DemandPanel

        let nextModel = { model with DemandPanel = childModel }

        match childMsg with
        | DemandPanel.Msg.RefreshRequested -> nextModel, Cmd.ofMsg Initialize, None
        | _ -> nextModel, Cmd.map DemandPanelMsg childCmd, None

    | SearchDemandAction query ->
        let childModel, childCmd = DemandPanel.update (DemandPanel.Msg.SearchTextChanged query) model.DemandPanel
        { model with DemandPanel = childModel }, Cmd.map DemandPanelMsg childCmd, None

    | SelectDemandAction demandLineId ->
        let foundDemand =
            match model.DemandPanel.Demands with
            | RemoteData.Loaded list -> list |> List.tryFind(fun d -> d.DemandLineId = demandLineId)
            | _ -> None

        let childModel, childCmd = DemandPanel.update (DemandPanel.Msg.SelectDemand foundDemand) model.DemandPanel
        { model with DemandPanel = childModel }, Cmd.map DemandPanelMsg childCmd, None

let view (model: Model) (dispatch: Msg -> unit) : Node =
    div {
        attr.style "height: 100%; display: flex; flex-direction: column;"

        div {
            attr.``class`` "rz-p-4 rz-border-bottom"
            attr.style "border-color: var(--rz-border-color);"

            h2 {
                attr.``class`` "rz-m-0 rz-text-h5 rz-font-weight-bold"
                "Material Reservation Workbench"
            }
        }

        div {
            attr.style "flex: 1; min-height: 0;"
            DemandPanel.view model.DemandPanel (DemandPanelMsg >> dispatch)
        }
    }
