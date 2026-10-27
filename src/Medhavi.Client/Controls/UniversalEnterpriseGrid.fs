module Medhavi.Client.Controls.UniversalEnterpriseGrid

open System
open Bolero
open Bolero.Html
open Radzen
open Microsoft.AspNetCore.Components.Web
open Medhavi.Client.ViewComposition.Context
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.Rendering
open DynamicGridColumn
open DynamicGridRow
open System.Collections.Generic

/// Render the universal enterprise grid
let render (ctx: ComponentRenderContext) (dispatch: ComponentEvent -> unit) : Node =

    let rawRows =
        match ctx.Data with
        | Tabular r -> RowNormalization.completeRows ctx.Schema r
        | NoData -> []

    // Decode sort from parameters or fall back to schema default
    let sortOpt =
        match
            ctx.Configuration.Parameters |> Map.tryFind "SortField",
            ctx.Configuration.Parameters |> Map.tryFind "SortDirection"
        with
        | Some(StringValue field), Some(StringValue dir) ->
            let direction = if dir = "Descending" then Descending else Ascending

            Some
                {
                    Field = field
                    Direction = direction
                    Priority = 1
                }
        | _ -> ctx.Schema.DefaultSort

    // Apply functional pipeline: local filters then sorting
    let filteredRows = UniversalGridPipeline.applyFilters ctx.Configuration.LocalFilters rawRows

    let sortedRows =
        match sortOpt with
        | Some s -> UniversalGridPipeline.applySort s filteredRows
        | None -> filteredRows

    let dynamicRows = UniversalGridAdapter.toDynamicRows sortedRows
    let dynamicColumns = UniversalGridAdapter.toDynamicColumns ctx.Schema

    let selectedKey = ctx.Selection |> Option.defaultValue ""

    let keyColumn = ctx.Schema.Columns |> List.tryFind(fun c -> c.IsKey)
    let keyColId = keyColumn |> Option.map(fun c -> c.ColumnId)

    // Build the selected values list for RadzenDataGrid to highlight the selected row
    let selectedRowValues: IList<Value> =
        match ctx.Selection, keyColumn with
        | Some key, Some kc when not(String.IsNullOrEmpty key) ->
            dynamicRows
            |> Array.filter(fun r ->
                let mutable v: obj = null
                r.TryGetValue(kc.ColumnId, &v) && string v = key)
            |> fun arr -> (ResizeArray<Value>(arr) :> IList<Value>)
        | _ -> (ResizeArray<Value>() :> IList<Value>)

    let isFiltered = not(List.isEmpty ctx.Configuration.LocalFilters) || (sortedRows.Length < List.length rawRows)

    let badgeText =
        if isFiltered then
            $"{sortedRows.Length} / {rawRows.Length} records (Filtered)"
        else
            $"{sortedRows.Length} records"

    let selectionMode =
        match ctx.Definition.SelectionMode with
        | SingleSelection -> DataGridSelectionMode.Single
        | MultiSelection -> DataGridSelectionMode.Multiple
        | NoSelection -> DataGridSelectionMode.Single

    let activeSortField = sortOpt |> Option.map(fun s -> s.Field)

    let activeSortDirection =
        sortOpt |> Option.map(fun s -> if s.Direction = Descending then "Descending" else "Ascending")

    div {
        attr.style
            "width: 100%; height: 100%; display: flex; flex-direction: column; min-height: 0; gap: 8px; padding: 8px; box-sizing: border-box;"

        // Compact Toolstrip (Filter badge + Selection + Actions + Refresh)
        div {
            attr.style "display: flex; align-items: center; gap: 8px; flex-shrink: 0; flex-wrap: wrap; padding: 2px 0;"

            span {
                attr.style(
                    if isFiltered then
                        "padding: 2px 8px; border-radius: 10px; background: var(--rz-warning-lighter); color: var(--rz-warning-dark); border: 1px solid var(--rz-warning); font-size: 11px; font-weight: 600;"
                    else
                        "padding: 2px 8px; border-radius: 10px; background: var(--rz-secondary-lighter); font-size: 11px;"
                )

                badgeText
            }

            div {
                attr.style "margin-left: auto; display: flex; align-items: center; gap: 6px; flex-wrap: wrap;"

                for action in ctx.Schema.Actions do
                    let btnStyle = if action.IsPrimary then ButtonStyle.Primary else ButtonStyle.Light

                    Rz.button(
                        action.DisplayName,
                        onClick =
                            (fun (_: MouseEventArgs) ->
                                let targetOpt = if selectedKey <> "" then Some selectedKey else None
                                dispatch(ActionRequested(action.Id, targetOpt, NoPayload))),
                        style = btnStyle,
                        icon = action.Icon
                    )

                if selectedKey <> "" then
                    span {
                        attr.style "color: var(--rz-text-secondary-color); font-size: 12px; margin-left: 4px;"
                        $"Selected: {selectedKey}"
                    }

                Rz.iconButton(
                    "refresh",
                    onClick = (fun (_: MouseEventArgs) -> dispatch RefreshRequested),
                    tooltip = "Refresh data"
                )
            }
        }

        // Grid
        div {
            attr.style "flex: 1; min-height: 0; width: 100%; height: 100%; overflow: hidden;"

            comp<DynamicRadzenGrid> {
                "Rows" => (dynamicRows :> IReadOnlyList<Value>)
                "Columns" => (dynamicColumns :> IReadOnlyList<Definition>)
                "AllowPaging" => false
                "AllowSorting" => true
                "AllowFiltering" => true
                "AllowGrouping" => true
                "AllowVirtualization" => false
                "AllowColumnResize" => true
                "AllowColumnPicking" => true
                "SelectedValues" => selectedRowValues
                "SelectedKey" => ctx.Selection
                "KeyColumnId" => keyColId
                "ActiveSortField" => activeSortField
                "ActiveSortDirection" => activeSortDirection
                "SelectionMode" => selectionMode
                "AllowRowSelectOnRowClick" => true
                "FormatProvider" => (ctx.Culture :> IFormatProvider)
                "CellTemplates" => CellRenderers.templates

                "OnRowSelected" => Action<Value>(fun (row: Value) ->
                    let mutable key: obj = null

                    match keyColumn with
                    | Some kc ->
                        if row.TryGetValue(kc.ColumnId, &key) then
                            dispatch(SelectionChanged(Some(string key)))
                    | None -> ())

                "OnSort" => Action<DataGridColumnSortEventArgs<Value>>(fun (args: DataGridColumnSortEventArgs<Value>) ->
                    let colId = args.Column.Property

                    let sortCriterion =
                        if args.SortOrder.HasValue then
                            let dir = if args.SortOrder.Value = SortOrder.Ascending then Ascending else Descending

                            Some
                                {
                                    Field = colId
                                    Direction = dir
                                    Priority = 1
                                }
                        else
                            None

                    dispatch(SortChanged sortCriterion))

                "OnFilter" => Action<DataGridColumnFilterEventArgs<Value>>(fun (args: DataGridColumnFilterEventArgs<Value>) ->
                    let colId = args.Column.Property

                    let filterValStr =
                        if isNull args.FilterValue then
                            None
                        else
                            let s = string args.FilterValue
                            if String.IsNullOrWhiteSpace s then None else Some s

                    let op =
                        match args.FilterOperator with
                        | Radzen.FilterOperator.Equals -> FilterOperator.Equals
                        | Radzen.FilterOperator.NotEquals -> FilterOperator.NotEquals
                        | Radzen.FilterOperator.GreaterThan -> FilterOperator.GreaterThan
                        | Radzen.FilterOperator.GreaterThanOrEquals -> FilterOperator.GreaterThanOrEqual
                        | Radzen.FilterOperator.LessThan -> FilterOperator.LessThan
                        | Radzen.FilterOperator.LessThanOrEquals -> FilterOperator.LessThanOrEqual
                        | Radzen.FilterOperator.Contains -> FilterOperator.Contains
                        | Radzen.FilterOperator.IsEmpty -> FilterOperator.IsEmpty
                        | Radzen.FilterOperator.IsNotEmpty -> FilterOperator.IsNotEmpty
                        | _ -> FilterOperator.Contains

                    let filterCriterion =
                        match filterValStr with
                        | Some v ->
                            Some
                                {
                                    Field = colId
                                    Operator = op
                                    Value = Some v
                                }
                        | None -> None

                    dispatch(FilterChanged(colId, filterCriterion)))
            }
        }
    }
