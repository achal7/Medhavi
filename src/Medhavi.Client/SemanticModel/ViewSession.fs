namespace Medhavi.Client.SemanticModel

open System

// ============================================================================
// I4 — Runtime View session/lifecycle
// ============================================================================

type DataState =
    | Idle
    | Loading
    | Loaded of rowCount: int * updatedAt: DateTimeOffset
    | Failed of message: string

type ViewSession =
    { SessionId: ViewSessionId
      SourceViewDefId: ViewDefId option
      SourceVersion: int option
      State: ViewState
      IsDirty: bool
      DataStates: Map<ViewComponentId, DataState>
      Selections: Map<ViewComponentId, string option> }

/// Minimum immutable projection of ViewSession required for rendering (R2-R1).
/// ABSENCE SEMANTICS (explicit invariant):
///   DataStates:  missing ViewComponentId → equivalent to Idle
///   Selections:  missing ViewComponentId → equivalent to None
/// These are legitimate absence, not invariant violations.
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
          IsDirty = false
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
          IsDirty = false
          DataStates = Map.empty
          Selections = Map.empty }

    let applyOperation
        (operation: ViewState -> Result<ViewState, ViewError>)
        (session: ViewSession)
        : Result<ViewSession, ViewError> =
        match operation session.State with
        | Error error -> Error error
        | Ok newState ->
            let changed = newState <> session.State

            { session with
                State = newState
                IsDirty = session.IsDirty || changed }
            |> pruneRuntime
            |> Ok

module TransientViewPolicy =
    let createComponentWorkspace (sessionId: ViewSessionId) (title: string) (dimensions: GridDimensions) : ViewSession =
        { SessionId = sessionId
          SourceViewDefId = None
          SourceVersion = None
          State =
            { Title = title
              Components = Map.empty
              Layout = Dashboard(dimensions, Map.empty)
              SharedContext = None }
          IsDirty = false
          DataStates = Map.empty
          Selections = Map.empty }

