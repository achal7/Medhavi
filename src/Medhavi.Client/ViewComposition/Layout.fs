namespace Medhavi.Client.ViewComposition.Layout

open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry

type SplitDirection =
    | Horizontal
    | Vertical

type LayoutTopology =
    | EmptyTopology
    | SingleTopology
    | SplitTopology
    | DashboardTopology
    | TabbedTopology

type LayoutError =
    | InvalidSplitRatio of attempted: decimal
    | InvalidGridDimensions of GridDimensions
    | InvalidTileBounds of ViewComponentId * TilePosition * GridDimensions
    | OverlappingTiles of ViewComponentId * ViewComponentId
    | EmptySplitBranch of SplitDirection
    | EmptyTabList
    | EmptyTabContent of TabId
    | DuplicateTabId of TabId
    | DuplicateLayoutOccurrence of ViewComponentId
    | ComponentNotFoundInLayout of ViewComponentId
    | TopologyMismatch of expected: LayoutTopology * actual: LayoutTopology

type Tab =
    { Id: TabId
      Label: string
      Content: LayoutDefinition }

and LayoutDefinition =
    | Empty
    | Single of ViewComponentId
    | Split of SplitDirection * LayoutDefinition * LayoutDefinition * SplitRatio
    | Dashboard of GridDimensions * Map<ViewComponentId, TilePosition>
    | Tabbed of Tab list

module LayoutAlgebra =
    type private Outcome<'a> =
        | Absent
        | Present of 'a

    let topologyOf (layout: LayoutDefinition) : LayoutTopology =
        match layout with
        | Empty -> EmptyTopology
        | Single _ -> SingleTopology
        | Split _ -> SplitTopology
        | Dashboard _ -> DashboardTopology
        | Tabbed _ -> TabbedTopology

    let canEdit (layout: LayoutDefinition) : bool =
        match topologyOf layout with
        | DashboardTopology -> true
        | _ -> false

    let editDisabledReason (layout: LayoutDefinition) : string option =
        match topologyOf layout with
        | DashboardTopology -> None
        | SplitTopology -> Some "Layout editing for Split views is not yet supported."
        | TabbedTopology -> Some "Layout editing for Tabbed views is not yet supported."
        | SingleTopology -> Some "Single component views cannot be rearranged."
        | EmptyTopology -> Some "Workspace is empty."

    let rec occurrences (layout: LayoutDefinition) : Map<ViewComponentId, int> =
        let addOne (counts: Map<ViewComponentId, int>) id =
            counts |> Map.change id (fun count -> Some(defaultArg count 0 + 1))

        let union (left: Map<ViewComponentId, int>) (right: Map<ViewComponentId, int>) =
            Map.fold
                (fun counts id count -> counts |> Map.change id (fun existing -> Some(defaultArg existing 0 + count)))
                left
                right

        match layout with
        | Empty -> Map.empty
        | Single id -> addOne Map.empty id
        | Split(_, left, right, _) -> union (occurrences left) (occurrences right)
        | Dashboard(_, tiles) -> tiles |> Map.keys |> Seq.fold addOne Map.empty
        | Tabbed tabs -> tabs |> List.fold (fun counts tab -> union counts (occurrences tab.Content)) Map.empty

    let rec private collectTabIds (layout: LayoutDefinition) : TabId list =
        match layout with
        | Empty -> []
        | Single _ -> []
        | Split(_, left, right, _) -> collectTabIds left @ collectTabIds right
        | Dashboard _ -> []
        | Tabbed tabs -> tabs |> List.collect (fun tab -> tab.Id :: collectTabIds tab.Content)

    let private firstError (results: Result<unit, LayoutError> list) : Result<unit, LayoutError> =
        results
        |> List.tryPick (function
            | Error error -> Some(Error error)
            | Ok () -> None)
        |> Option.defaultValue (Ok ())

    let rec private validateNode (layout: LayoutDefinition) : Result<unit, LayoutError> =
        match layout with
        | Empty -> Ok ()
        | Single _ -> Ok ()
        | Split(direction, left, right, _) ->
            match left, right with
            | Empty, _ -> Error(EmptySplitBranch direction)
            | _, Empty -> Error(EmptySplitBranch direction)
            | _ -> firstError [ validateNode left; validateNode right ]
        | Dashboard(dimensions, tiles) ->
            let columns = GridDimensions.columns dimensions
            let rows = GridDimensions.rows dimensions

            let boundsError =
                tiles
                |> Map.tryPick (fun id position ->
                    let col = TilePosition.col position
                    let row = TilePosition.row position
                    let colSpan = TilePosition.colSpan position
                    let rowSpan = TilePosition.rowSpan position

                    if col + colSpan - 1 > columns || row + rowSpan - 1 > rows then
                        Some(InvalidTileBounds(id, position, dimensions))
                    else
                        None)

            match boundsError with
            | Some error -> Error error
            | None ->
                let cells =
                    tiles
                    |> Map.toList
                    |> List.collect (fun (id, position) ->
                        [ for col in TilePosition.col position .. TilePosition.col position + TilePosition.colSpan position - 1 do
                              for row in TilePosition.row position .. TilePosition.row position + TilePosition.rowSpan position - 1 do
                                  col, row, id ])

                cells
                |> List.groupBy (fun (col, row, _) -> col, row)
                |> List.tryPick (fun (_, group) ->
                    match group with
                    | (_, _, firstId) :: (_, _, secondId) :: _ -> Some(Error(OverlappingTiles(firstId, secondId)))
                    | _ -> None)
                |> Option.defaultValue (Ok ())
        | Tabbed tabs ->
            if List.isEmpty tabs then
                Error EmptyTabList
            else
                tabs
                |> List.map (fun tab ->
                    if tab.Content = Empty then Error(EmptyTabContent tab.Id)
                    else validateNode tab.Content)
                |> firstError

    let validateLayout (layout: LayoutDefinition) : Result<unit, LayoutError> =
        match collectTabIds layout |> List.countBy id |> List.tryFind (fun (_, count) -> count > 1) with
        | Some(duplicate, _) -> Error(DuplicateTabId duplicate)
        | None -> validateNode layout

    let private collapse direction left right ratio =
        match left, right with
        | Empty, Empty -> Empty
        | Empty, content -> content
        | content, Empty -> content
        | _, _ -> Split(direction, left, right, ratio)

    let rec extract (targetId: ViewComponentId) (layout: LayoutDefinition) : LayoutDefinition option =
        match layout with
        | Empty -> None
        | Single id -> if id = targetId then Some Empty else None
        | Split(direction, left, right, ratio) ->
            match extract targetId left with
            | Some newLeft -> Some(collapse direction newLeft right ratio)
            | None ->
                match extract targetId right with
                | Some newRight -> Some(collapse direction left newRight ratio)
                | None -> None
        | Dashboard(dimensions, tiles) ->
            if Map.containsKey targetId tiles then
                Some(Dashboard(dimensions, Map.remove targetId tiles))
            else
                None
        | Tabbed tabs ->
            let rec loop prefix remaining =
                match remaining with
                | [] -> None
                | tab :: tail ->
                    match extract targetId tab.Content with
                    | None -> loop (prefix @ [ tab ]) tail
                    | Some content ->
                        let updated =
                            if content = Empty then prefix @ tail
                            else prefix @ [ { tab with Content = content } ] @ tail

                        Some(if List.isEmpty updated then Empty else Tabbed updated)

            loop [] tabs

    let adjustDashboardPlacement
        (targetId: ViewComponentId)
        (newPosition: TilePosition)
        (layout: LayoutDefinition)
        : Result<LayoutDefinition, LayoutError> =
        let rec go (current: LayoutDefinition) : Outcome<Result<LayoutDefinition, LayoutError>> =
            match current with
            | Dashboard(dimensions, tiles) when Map.containsKey targetId tiles ->
                let candidate = Dashboard(dimensions, Map.add targetId newPosition tiles)
                Present(validateLayout candidate |> Result.map (fun () -> candidate))
            | Split(direction, left, right, ratio) ->
                match go left with
                | Present result ->
                    Present(result |> Result.map (fun updatedLeft -> Split(direction, updatedLeft, right, ratio)))
                | Absent ->
                    match go right with
                    | Present result ->
                        Present(result |> Result.map (fun updatedRight -> Split(direction, left, updatedRight, ratio)))
                    | Absent -> Absent
            | Tabbed tabs ->
                let rec loop prefix remaining =
                    match remaining with
                    | [] -> Absent
                    | tab :: tail ->
                        match go tab.Content with
                        | Present result ->
                            Present(
                                result
                                |> Result.map (fun updatedContent ->
                                    Tabbed(List.rev prefix @ [ { tab with Content = updatedContent } ] @ tail))
                            )
                        | Absent -> loop (tab :: prefix) tail

                loop [] tabs
            | _ -> Absent

        match go layout with
        | Absent -> Error(ComponentNotFoundInLayout targetId)
        | Present result -> result

    let placeInDashboard
        (targetId: ViewComponentId)
        (position: TilePosition)
        (layout: LayoutDefinition)
        : Result<LayoutDefinition, LayoutError> =
        match layout with
        | Dashboard(dimensions, tiles) ->
            if Map.containsKey targetId tiles then
                Error(DuplicateLayoutOccurrence targetId)
            else
                let candidate = Dashboard(dimensions, Map.add targetId position tiles)
                validateLayout candidate |> Result.map (fun () -> candidate)
        | other -> Error(TopologyMismatch(DashboardTopology, topologyOf other))

    let wrapInSplit
        (direction: SplitDirection)
        (ratio: SplitRatio)
        (existing: LayoutDefinition)
        (newId: ViewComponentId)
        : Result<LayoutDefinition, LayoutError> =
        if occurrences existing |> Map.containsKey newId then
            Error(DuplicateLayoutOccurrence newId)
        else
            let candidate = Split(direction, existing, Single newId, ratio)
            validateLayout candidate |> Result.map (fun () -> candidate)

module PlacementPolicy =
    let private overlaps (occ: TilePosition) (col: int) (row: int) (cs: int) (rs: int) : bool =
        let oc = TilePosition.col occ
        let orw = TilePosition.row occ
        let ocs = TilePosition.colSpan occ
        let ors = TilePosition.rowSpan occ
        col < oc + ocs && oc < col + cs && row < orw + ors && orw < row + rs

    type PlacementContext =
        { Dimensions: GridDimensions
          Occupied: TilePosition list
          Span: GridSpan }

    type PlacementError =
        | SpanExceedsGridDimensions of span: GridSpan * dimensions: GridDimensions
        | NoAvailablePosition of span: GridSpan * dimensions: GridDimensions
        | UnsupportedTopology of LayoutTopology

    type PlacementInternalBug = CandidateViolatesPositionInvariant of col: int * row: int * colSpan: int * rowSpan: int

    type PlacementFailure =
        | Policy of PlacementError
        | InternalBug of PlacementInternalBug

    let proposePosition (context: PlacementContext) : Result<TilePosition, PlacementFailure> =
        let cols = GridDimensions.columns context.Dimensions
        let rows = GridDimensions.rows context.Dimensions
        let cs = GridSpan.colSpan context.Span
        let rs = GridSpan.rowSpan context.Span

        if cs > cols || rs > rows then
            Error(Policy(SpanExceedsGridDimensions(context.Span, context.Dimensions)))
        else
            let isFree col row = context.Occupied |> List.forall (fun occ -> not (overlaps occ col row cs rs))

            let rec scanRow row =
                if row > rows - rs + 1 then
                    Error(Policy(NoAvailablePosition(context.Span, context.Dimensions)))
                else
                    let rec scanCol col =
                        if col > cols - cs + 1 then
                            scanRow (row + 1)
                        elif isFree col row then
                            match TilePosition.create col row cs rs with
                            | Ok pos -> Ok pos
                            | Error _ -> Error(InternalBug(CandidateViolatesPositionInvariant(col, row, cs, rs)))
                        else
                            scanCol (col + 1)

                    scanCol 1

            scanRow 1
