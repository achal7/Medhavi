namespace Medhavi.Client.Controls

open System
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open DynamicGridColumn
open DynamicGridRow

module UniversalGridAdapter =

    /// Convert CellValue to obj for Radzen dynamic binding
    let cellToObj (cell: CellValue) : obj =
        match cell with
        | TextValue s -> box s
        | NumberValue n -> box n
        | BooleanValue b -> box b
        | DateTimeValue d -> box(d.DateTime) // Convert to DateTime for Radzen
        | EnumValue s -> box s
        | ReferenceValue(_, label) -> box label
        | EmptyValue -> null

    /// Convert CellType to CLR Type for Radzen
    let cellTypeToClrType (cellType: CellType) : Type =
        match cellType with
        | TextType -> typeof<string>
        | NumberType -> typeof<decimal>
        | BooleanType -> typeof<bool>
        | DateTimeType -> typeof<DateTime>
        | EnumType _ -> typeof<string>
        | ReferenceType -> typeof<string>

    /// Convert TextAlignment to Radzen TextAlign
    let alignmentToRadzen (alignment: TextAlignment) : Radzen.TextAlign =
        match alignment with
        | Left -> Radzen.TextAlign.Left
        | Center -> Radzen.TextAlign.Center
        | Right -> Radzen.TextAlign.Right

    /// Convert SchemaColumnDefinition to DynamicGridColumn.Definition
    let schemaColumnToDynamicColumn (col: SchemaColumnDefinition) : DynamicGridColumn.Definition =
        let def = DynamicGridColumn.create col.ColumnId col.Title (cellTypeToClrType col.CellType)

        let def =
            match col.DefaultWidthInPixels with
            | Some w -> DynamicGridColumn.withWidth $"{w}px" def
            | None -> def

        let def =
            match col.FormatString with
            | Some fmt -> DynamicGridColumn.withFormat fmt def
            | None -> def

        { def with
            TextAlign = alignmentToRadzen col.Alignment
            Sortable = col.Sortable
            Filterable = col.Filterable
            Groupable = col.Groupable
            Frozen = col.DefaultPinned
            RenderHint = col.RenderHint }

    /// Convert UniversalRow to IDictionary<string, obj>
    let universalRowToDynamicRow (row: UniversalRow) : DynamicGridRow.Value =
        row.Cells |> Map.toList |> List.map(fun (key, cell) -> key, cellToObj cell) |> DynamicGridRow.create

    /// Convert full dataset
    let toDynamicRows (rows: UniversalRow list) : DynamicGridRow.Value array =
        rows |> List.map universalRowToDynamicRow |> List.toArray

    /// Convert full schema
    let toDynamicColumns (schema: SchemaDefinition) : DynamicGridColumn.Definition array =
        schema.Columns
        |> List.filter(fun c -> c.DefaultVisible)
        |> List.sortBy(fun c -> c.DefaultOrder)
        |> List.map schemaColumnToDynamicColumn
        |> List.toArray
