namespace Medhavi.Client.ViewComposition.Session

open System
open Medhavi.Common.Validation
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.View
open Medhavi.Client.ViewComposition.Layout

// ============================================================================
// Runtime View session/lifecycle
// ============================================================================

type DataState =
    | Idle
    | Loading
    | Loaded of rowCount: int * updatedAt: DateTimeOffset
    | Failed of message: string

/// The Runtime Aggregate. IsDirty is removed; it is computed purely via ViewLifecycle.isDirty.
type ViewSession =
    { SessionId: ViewSessionId
      SourceViewDefId: ViewDefId option
      SourceVersion: int option
      State: ViewState
      DataStates: Map<ViewComponentId, DataState>
      Selections: Map<ViewComponentId, string option> }

/// Minimum immutable projection of ViewSession required for rendering.
/// ABSENCE SEMANTICS (explicit invariant):
///   DataStates:  missing ViewComponentId → equivalent to Idle
///   Selections:  missing ViewComponentId → equivalent to None
type ViewRenderState =
    { State: ViewState
      DataStates: Map<ViewComponentId, DataState>
      Selections: Map<ViewComponentId, string option> }

module ViewRenderState =
    let ofSession (session: ViewSession) : ViewRenderState =
        { State = session.State
          DataStates = session.DataStates
          Selections = session.Selections }

module ViewLifecycle =
    let private pruneRuntime (session: ViewSession) : ViewSession =
        let liveIds = session.State.Components |> Map.keys |> Set.ofSeq

        { session with
            DataStates = session.DataStates |> Map.filter(fun id _ -> Set.contains id liveIds)
            Selections = session.Selections |> Map.filter(fun id _ -> Set.contains id liveIds) }

    let openSession (sessionId: ViewSessionId) (definition: ViewDefinition) : ViewSession =
        { SessionId = sessionId
          SourceViewDefId = Some definition.Id
          SourceVersion = Some definition.Version
          State = definition.State
          DataStates = Map.empty
          Selections = Map.empty }

    let createBlankSession (sessionId: ViewSessionId) (title: string) : ViewSession =
        { SessionId = sessionId
          SourceViewDefId = None
          SourceVersion = None
          State =
            { Title = title
              Components = Map.empty
              Layout = Empty
              SharedContext = None }
          DataStates = Map.empty
          Selections = Map.empty }

    /// Applies a ViewAlgebra operation. Aligned to the Validation applicative.
    let applyOperation
        (operation: ViewState -> Validation<ViewState, ViewError>)
        (session: ViewSession)
        : Validation<ViewSession, ViewError> =
        operation session.State |> map(fun newState -> { session with State = newState } |> pruneRuntime)

    /// Pure computation of dirty state. Compares current state against a baseline.
    let isDirty (session: ViewSession) (baseline: ViewState option) : bool =
        match baseline with
        | None -> true // Transient sessions (AI generated) are inherently unsaved
        | Some baseState -> session.State <> baseState

module TransientViewPolicy =
    open Medhavi.Client.ViewComposition.Geometry

    let createComponentWorkspace (sessionId: ViewSessionId) (title: string) (dimensions: GridDimensions) : ViewSession =
        { SessionId = sessionId
          SourceViewDefId = None
          SourceVersion = None
          State =
            { Title = title
              Components = Map.empty
              Layout = Dashboard(dimensions, Map.empty)
              SharedContext = None }
          DataStates = Map.empty
          Selections = Map.empty }
