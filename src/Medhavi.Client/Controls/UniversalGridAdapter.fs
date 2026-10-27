namespace Medhavi.Client.Controls

open System
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open DynamicGridColumn

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
            Groupable = true
            Frozen = col.DefaultPinned
            Visible = col.DefaultVisible
            RenderHint = col.RenderHint
        }

    /// Convert UniversalRow to IDictionary<string, obj>
    let universalRowToDynamicRow (row: UniversalRow) : DynamicGridRow.Value =
        row.Cells |> Map.toList |> List.map(fun (key, cell) -> key, cellToObj cell) |> DynamicGridRow.create

    /// Convert full dataset
    let toDynamicRows (rows: UniversalRow list) : DynamicGridRow.Value array =
        rows |> List.map universalRowToDynamicRow |> List.toArray

    /// Convert full schema with explicit column visibility
    let toDynamicColumnsWithVisibility
        (hiddenColumnIds: Set<string>)
        (schema: SchemaDefinition)
        : DynamicGridColumn.Definition array =
        schema.Columns
        |> List.sortBy(fun c -> c.DefaultOrder)
        |> List.map(fun c ->
            let isVisible = c.DefaultVisible && not(Set.contains c.ColumnId hiddenColumnIds)
            let dynamicCol = schemaColumnToDynamicColumn c
            { dynamicCol with Visible = isVisible })
        |> List.toArray

    /// Convert full schema using default column visibility
    let toDynamicColumns (schema: SchemaDefinition) : DynamicGridColumn.Definition array =
        toDynamicColumnsWithVisibility Set.empty schema

module UniversalGridPipeline =

    open Medhavi.Client.ViewComposition.Context

    let private cellToString (cell: CellValue) : string =
        match cell with
        | TextValue s -> s
        | NumberValue n -> string n
        | BooleanValue b -> string b
        | DateTimeValue d -> d.ToString("yyyy-MM-dd")
        | EnumValue s -> s
        | ReferenceValue(_, label) -> label
        | EmptyValue -> ""

    let private cellToDecimal (cell: CellValue) : decimal option =
        match cell with
        | NumberValue n -> Some n
        | TextValue s ->
            match Decimal.TryParse(s, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture) with
            | true, v -> Some v
            | false, _ -> None
        | _ -> None

    let private cellToDate (cell: CellValue) : DateTime option =
        match cell with
        | DateTimeValue d -> Some d.Date
        | _ -> None

    let private tryParseDate (s: string) : DateTime option =
        match DateTime.TryParse(s, Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None) with
        | true, dt -> Some dt.Date
        | false, _ ->
            match DateTime.TryParse(s, Globalization.CultureInfo.CurrentCulture, Globalization.DateTimeStyles.None) with
            | true, dt -> Some dt.Date
            | false, _ -> None

    let private matchesFilter (filter: FilterCriterion) (row: UniversalRow) : bool =
        let cell = row.Cells |> Map.tryFind filter.Field |> Option.defaultValue EmptyValue
        let filterStr = filter.Value |> Option.defaultValue ""

        match filter.Operator with
        | FilterOperator.IsEmpty ->
            match cell with
            | EmptyValue -> true
            | TextValue s -> String.IsNullOrWhiteSpace s
            | _ -> false
        | FilterOperator.IsNotEmpty ->
            match cell with
            | EmptyValue -> false
            | TextValue s -> not(String.IsNullOrWhiteSpace s)
            | _ -> true
        | FilterOperator.Equals ->
            match cellToDate cell, tryParseDate filterStr with
            | Some cd, Some fd -> cd = fd
            | _ ->
                let str = cellToString cell
                String.Equals(str, filterStr, StringComparison.OrdinalIgnoreCase)
        | FilterOperator.NotEquals ->
            match cellToDate cell, tryParseDate filterStr with
            | Some cd, Some fd -> cd <> fd
            | _ ->
                let str = cellToString cell
                not(String.Equals(str, filterStr, StringComparison.OrdinalIgnoreCase))
        | FilterOperator.Contains ->
            let str = cellToString cell
            str.IndexOf(filterStr, StringComparison.OrdinalIgnoreCase) >= 0
        | FilterOperator.GreaterThan ->
            match
                cellToDecimal cell,
                Decimal.TryParse(filterStr, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture)
            with
            | Some v, (true, target) -> v > target
            | _ ->
                match cellToDate cell, tryParseDate filterStr with
                | Some cd, Some fd -> cd > fd
                | _ -> false
        | FilterOperator.GreaterThanOrEqual ->
            match
                cellToDecimal cell,
                Decimal.TryParse(filterStr, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture)
            with
            | Some v, (true, target) -> v >= target
            | _ ->
                match cellToDate cell, tryParseDate filterStr with
                | Some cd, Some fd -> cd >= fd
                | _ -> false
        | FilterOperator.LessThan ->
            match
                cellToDecimal cell,
                Decimal.TryParse(filterStr, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture)
            with
            | Some v, (true, target) -> v < target
            | _ ->
                match cellToDate cell, tryParseDate filterStr with
                | Some cd, Some fd -> cd < fd
                | _ -> false
        | FilterOperator.LessThanOrEqual ->
            match
                cellToDecimal cell,
                Decimal.TryParse(filterStr, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture)
            with
            | Some v, (true, target) -> v <= target
            | _ ->
                match cellToDate cell, tryParseDate filterStr with
                | Some cd, Some fd -> cd <= fd
                | _ -> false

    let applyFilters (filters: FilterCriterion list) (rows: UniversalRow list) : UniversalRow list =
        if List.isEmpty filters then
            rows
        else
            rows |> List.filter(fun row -> filters |> List.forall(fun f -> matchesFilter f row))

    let private compareCells (c1: CellValue) (c2: CellValue) : int =
        match c1, c2 with
        | EmptyValue, EmptyValue -> 0
        | EmptyValue, _ -> -1
        | _, EmptyValue -> 1
        | NumberValue n1, NumberValue n2 -> compare n1 n2
        | DateTimeValue d1, DateTimeValue d2 -> compare d1 d2
        | BooleanValue b1, BooleanValue b2 -> compare b1 b2
        | TextValue s1, TextValue s2 -> String.Compare(s1, s2, StringComparison.OrdinalIgnoreCase)
        | EnumValue e1, EnumValue e2 -> String.Compare(e1, e2, StringComparison.OrdinalIgnoreCase)
        | ReferenceValue(_, l1), ReferenceValue(_, l2) -> String.Compare(l1, l2, StringComparison.OrdinalIgnoreCase)
        | _ -> String.Compare(cellToString c1, cellToString c2, StringComparison.OrdinalIgnoreCase)

    let applySort (sort: SortCriterion) (rows: UniversalRow list) : UniversalRow list =
        rows
        |> List.sortWith(fun r1 r2 ->
            let c1 = r1.Cells |> Map.tryFind sort.Field |> Option.defaultValue EmptyValue
            let c2 = r2.Cells |> Map.tryFind sort.Field |> Option.defaultValue EmptyValue
            let cmp = compareCells c1 c2
            if sort.Direction = Ascending then cmp else -cmp)
