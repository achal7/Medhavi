namespace Medhavi.Client.ViewComposition.Data

open Medhavi.Client.ViewComposition.Schema

type CellValue =
    | TextValue of string
    | NumberValue of decimal
    | BooleanValue of bool
    | DateTimeValue of System.DateTimeOffset
    | EnumValue of string
    | ReferenceValue of key: string * label: string
    | EmptyValue

type UniversalRow =
    { Key: string
      Cells: Map<string, CellValue> }

type ProjectionAdapter<'T when 'T: not null> = { ToUniversal: 'T -> UniversalRow }

type DataPayload =
    | NoData
    | Tabular of UniversalRow list

module RowNormalization =
    /// Completes a row by adding any schema columns that are missing.
    /// Should be applied during projection ('T -> UniversalRow), not at render time.
    let completeRow (schema: SchemaDefinition) (row: UniversalRow) : UniversalRow =
        let completedCells =
            schema.Columns
            |> List.fold
                (fun cells col ->
                    if Map.containsKey col.ColumnId cells then
                        cells
                    else
                        Map.add col.ColumnId EmptyValue cells)
                row.Cells

        { row with Cells = completedCells }

    /// Completes a list of rows against a schema.
    let completeRows (schema: SchemaDefinition) (rows: UniversalRow list) : UniversalRow list =
        rows |> List.map(completeRow schema)
