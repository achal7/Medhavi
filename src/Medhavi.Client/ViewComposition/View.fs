namespace Medhavi.Client.ViewComposition.View

open Medhavi.Common.Validation
open Medhavi.Client.ViewComposition.Context
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Layout
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Layout.PlacementPolicy

type ViewVisibility =
    | Private
    | Team
    | Public
    | Restricted

type ViewPermissions =
    { Owner: string
      Visibility: ViewVisibility
      EditableBy: string list
      Shareable: bool }

type ViewMetadata =
    { Category: string
      Icon: string
      Roles: string list
      IsDefault: bool
      IsTemplate: bool
      CreatedAt: System.DateTimeOffset
      CreatedBy: string
      ModifiedAt: System.DateTimeOffset
      ModifiedBy: string
      Permissions: ViewPermissions }

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

    let validate (state: ViewState) : Validation<ViewState, ViewError> =
        let occurrences = LayoutAlgebra.occurrences state.Layout
        let componentIds = state.Components |> Map.keys |> Set.ofSeq
        let occurrenceIds = occurrences |> Map.keys |> Set.ofSeq
        let unresolved = Set.difference occurrenceIds componentIds

        if not(Set.isEmpty unresolved) then
            Invalid [ UnresolvedLayoutReference(Set.minElement unresolved) ]
        else
            let unreferenced = Set.difference componentIds occurrenceIds

            if not(Set.isEmpty unreferenced) then
                Invalid [ UnreferencedComponent(Set.minElement unreferenced) ]
            else
                match occurrences |> Map.tryPick(fun id count -> if count = 1 then None else Some id) with
                | Some duplicate -> Invalid [ DuplicateLayoutOccurrence duplicate ]
                | None ->
                    LayoutAlgebra.validateLayout state.Layout
                    |> function
                        | Ok() -> Valid state
                        | Error e -> Invalid [ InvalidLayout e ]

    let addComponent
        (propose: PlacementPolicy.PlacementContext -> Result<TilePosition, PlacementFailure>)
        (span: GridSpan)
        (newId: ViewComponentId)
        (binding: ComponentBinding)
        (state: ViewState)
        : Validation<ViewState, ViewError> =
        let viewComponent = ComponentBinding.toViewComponent binding

        if Map.containsKey newId state.Components then
            Invalid [ DuplicateComponentId newId ]
        else
            match state.Layout with
            | Empty ->
                { state with
                    Components = Map.add newId viewComponent state.Components
                    Layout = Single newId }
                |> validate
            | Dashboard(dims, tiles) ->
                let context: PlacementPolicy.PlacementContext =
                    { Dimensions = dims
                      Occupied = tiles |> Map.toList |> List.map snd
                      Span = span }

                match propose context with
                | Error(PlacementFailure.Policy pe) -> Invalid [ InvalidPlacement pe ]
                | Error(PlacementFailure.InternalBug b) -> Invalid [ InternalPlacementInvariant b ]
                | Ok position ->
                    LayoutAlgebra.placeInDashboard newId position state.Layout
                    |> function
                        | Ok newLayout ->
                            { state with
                                Components = Map.add newId viewComponent state.Components
                                Layout = newLayout }
                            |> validate
                        | Error e -> Invalid [ InvalidLayout e ]
            | other -> Invalid [ InvalidPlacement(PlacementError.UnsupportedTopology(LayoutAlgebra.topologyOf other)) ]

    let removeComponent (targetId: ViewComponentId) (state: ViewState) : Validation<ViewState, ViewError> =
        if not(Map.containsKey targetId state.Components) then
            Invalid [ ComponentNotFound targetId ]
        else
            match LayoutAlgebra.extract targetId state.Layout with
            | None -> Invalid [ ComponentNotFound targetId ]
            | Some newLayout ->
                { state with
                    Components = Map.remove targetId state.Components
                    Layout = newLayout }
                |> validate

    let updateConfiguration
        (targetId: ViewComponentId)
        (configuration: ComponentConfiguration)
        (state: ViewState)
        : Validation<ViewState, ViewError> =
        match Map.tryFind targetId state.Components with
        | None -> Invalid [ ComponentNotFound targetId ]
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
        : Validation<ViewState, ViewError> =
        if not(Map.containsKey targetId state.Components) then
            Invalid [ ComponentNotFound targetId ]
        else
            LayoutAlgebra.adjustDashboardPlacement targetId newPosition state.Layout
            |> function
                | Ok newLayout -> { state with Layout = newLayout } |> validate
                | Error e -> Invalid [ InvalidLayout e ]
