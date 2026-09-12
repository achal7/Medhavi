namespace Medhavi.Client.SemanticModel

// ============================================================================
// I3 — View-local component composition and View algebra
// ============================================================================

type ViewState =
    { Title: string
      Components: Map<ViewComponentId, ViewComponent>
      Layout: LayoutDefinition
      SharedContext: SharedContext option }

type ViewDefinition =
    { Id: ViewDefId
      Name: string
      Version: int
      State: ViewState
      Metadata: ViewMetadata }

type ViewError =
    | DuplicateComponentId of ViewComponentId
    | ComponentNotFound of ViewComponentId
    | UnresolvedLayoutReference of ViewComponentId
    | UnreferencedComponent of ViewComponentId
    | DuplicateLayoutOccurrence of ViewComponentId
    | InvalidLayout of LayoutError
    | InvalidPlacement of PlacementError
    | InternalPlacementInvariant of PlacementInternalBug

module ViewAlgebra =
    let validate (state: ViewState) : Result<ViewState, ViewError> =
        let occurrences = LayoutAlgebra.occurrences state.Layout
        let componentIds = state.Components |> Map.keys |> Set.ofSeq
        let occurrenceIds = occurrences |> Map.keys |> Set.ofSeq
        let unresolved = Set.difference occurrenceIds componentIds

        if not(Set.isEmpty unresolved) then
            Error(UnresolvedLayoutReference(Set.minElement unresolved))
        else
            let unreferenced = Set.difference componentIds occurrenceIds

            if not(Set.isEmpty unreferenced) then
                Error(UnreferencedComponent(Set.minElement unreferenced))
            else
                match occurrences |> Map.tryPick(fun id count -> if count = 1 then None else Some id) with
                | Some duplicate -> Error(DuplicateLayoutOccurrence duplicate)
                | None ->
                    LayoutAlgebra.validateLayout state.Layout
                    |> Result.map(fun () -> state)
                    |> Result.mapError InvalidLayout

    /// Receives a validated ComponentBinding. Does not resolve registries.
    /// Does not validate compatibility (owned by ComponentBinding.create).
    let addComponent
        (propose: PlacementContext -> Result<TilePosition, PlacementFailure>)
        (span: GridSpan)
        (newId: ViewComponentId)
        (binding: ComponentBinding)
        (state: ViewState)
        : Result<ViewState, ViewError> =
        let viewComponent = ComponentBinding.toViewComponent binding

        if Map.containsKey newId state.Components then
            Error(DuplicateComponentId newId)
        else
            match state.Layout with
            | Empty ->
                { state with
                    Components = Map.add newId viewComponent state.Components
                    Layout = Single newId }
                |> validate
            | Dashboard(dims, tiles) ->
                let context =
                    { Dimensions = dims
                      Occupied = tiles |> Map.toList |> List.map snd
                      Span = span }

                match propose context with
                | Error(Policy pe) -> Error(InvalidPlacement pe)
                | Error(InternalBug b) -> Error(InternalPlacementInvariant b)
                | Ok position ->
                    LayoutAlgebra.placeInDashboard newId position state.Layout
                    |> Result.mapError InvalidLayout
                    |> Result.bind(fun newLayout ->
                        { state with
                            Components = Map.add newId viewComponent state.Components
                            Layout = newLayout }
                        |> validate)
            | other -> Error(InvalidPlacement(UnsupportedTopology(LayoutAlgebra.topologyOf other)))

    let removeComponent (targetId: ViewComponentId) (state: ViewState) : Result<ViewState, ViewError> =
        if not(Map.containsKey targetId state.Components) then
            Error(ComponentNotFound targetId)
        else
            match LayoutAlgebra.extract targetId state.Layout with
            | None -> Error(ComponentNotFound targetId)
            | Some newLayout ->
                { state with
                    Components = Map.remove targetId state.Components
                    Layout = newLayout }
                |> validate

    let updateConfiguration
        (targetId: ViewComponentId)
        (configuration: ComponentConfiguration)
        (state: ViewState)
        : Result<ViewState, ViewError> =
        match Map.tryFind targetId state.Components with
        | None -> Error(ComponentNotFound targetId)
        | Some existing ->
            { state with
                Components =
                    Map.add
                        targetId
                        { existing with
                            Configuration = configuration }
                        state.Components }
            |> validate

    let adjustDashboardPlacement
        (targetId: ViewComponentId)
        (newPosition: TilePosition)
        (state: ViewState)
        : Result<ViewState, ViewError> =
        if not(Map.containsKey targetId state.Components) then
            Error(ComponentNotFound targetId)
        else
            LayoutAlgebra.adjustDashboardPlacement targetId newPosition state.Layout
            |> Result.mapError InvalidLayout
            |> Result.bind(fun newLayout -> { state with Layout = newLayout } |> validate)
