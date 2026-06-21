namespace Medhavi.Web.Workspaces

open Medhavi.Contracts
open System

type Workspace =
    | MaterialReservation
    | ResourceScheduling
    | ScenarioManagement
    | MasterData

type WorkspaceAction =
    | NavigateTo of Workspace
    | RefreshActiveWorkspace
    | RefreshAllWorkspaces
    | ApplyContext of PlanningContext
    | OpenCopilot
    | ShowWorkspaceEvents
    | Help
