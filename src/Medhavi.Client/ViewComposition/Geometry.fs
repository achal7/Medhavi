namespace Medhavi.Client.ViewComposition.Geometry

open System

[<Struct>]
type SplitRatio = private SplitRatio of decimal

module SplitRatio =
    let create value =
        if value <= 0.0m || value >= 1.0m then Error "Invalid split ratio"
        else Ok(SplitRatio value)

    let value (SplitRatio v) = v

    let literal v =
        match create v with
        | Ok r -> r
        | Error e -> raise(ArgumentException e)

[<Struct>]
type GridDimensions = private GridDimensions of int * int

module GridDimensions =
    let create cols rows =
        if cols < 1 || rows < 1 then Error "Invalid grid dimensions"
        else Ok(GridDimensions(cols, rows))

    let columns (GridDimensions(c, _)) = c
    let rows (GridDimensions(_, r)) = r

    let literal c r =
        match create c r with
        | Ok d -> d
        | Error e -> raise(ArgumentException e)

    let defaultDimensions = literal 12 12

[<Struct>]
type GridSpan = private GridSpan of int * int

module GridSpan =
    let create c r =
        if c < 1 || r < 1 then Error "Invalid grid span"
        else Ok(GridSpan(c, r))

    let colSpan (GridSpan(c, _)) = c
    let rowSpan (GridSpan(_, r)) = r

    let literal c r =
        match create c r with
        | Ok s -> s
        | Error e -> raise(ArgumentException e)

[<Struct>]
type TilePosition = private TilePosition of int * int * int * int

module TilePosition =
    let create col row colSpan rowSpan =
        if col < 1 || row < 1 || colSpan < 1 || rowSpan < 1 then Error "Invalid tile position"
        else Ok(TilePosition(col, row, colSpan, rowSpan))

    let col (TilePosition(c, _, _, _)) = c
    let row (TilePosition(_, r, _, _)) = r
    let colSpan (TilePosition(_, _, c, _)) = c
    let rowSpan (TilePosition(_, _, _, r)) = r

    let literal col row cSpan rSpan =
        match create col row cSpan rSpan with
        | Ok p -> p
        | Error e -> raise(ArgumentException e)
