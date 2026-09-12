module Medhavi.Client.Controls.UniversalEnterpriseGrid

open System
open System.Globalization
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Microsoft.AspNetCore.Components.Web
open Medhavi.Client.ViewComposition.Identity
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

    let rows =
        match ctx.Data with
        | Tabular rows -> RowNormalization.completeRows ctx.Schema rows
        | NoData -> []

    let dynamicRows = UniversalGridAdapter.toDynamicRows rows
    let dynamicColumns = UniversalGridAdapter.toDynamicColumns ctx.Schema

    // Decode sort from parameters
    let sortOpt =
        match
            ctx.Configuration.Parameters |> Map.tryFind "SortField",
            ctx.Configuration.Parameters |> Map.tryFind "SortDirection"
        with
        | Some(StringValue field), Some(StringValue dir) ->
            let direction = if dir = "Descending" then Descending else Ascending

            Some
                { Field = field
                  Direction = direction
                  Priority = 1 }
        | _ -> ctx.Schema.DefaultSort

    let selectedKey = ctx.Selection |> Option.defaultValue ""

    let isFiltered = not(List.isEmpty ctx.Configuration.LocalFilters) || (dynamicRows.Length < List.length rows)

    let badgeText =
        if isFiltered then
            $"{dynamicRows.Length} / {rows.Length} records (Filtered)"
        else
            $"{dynamicRows.Length} records"

    let selectionMode =
        match ctx.Definition.SelectionMode with
        | SingleSelection -> DataGridSelectionMode.Single
        | MultiSelection -> DataGridSelectionMode.Multiple
        | NoSelection -> DataGridSelectionMode.Single

    div {
        attr.style
            "width: 100%; height: 100%; display: flex; flex-direction: column; min-height: 0; gap: 8px; padding: 8px; box-sizing: border-box;"

        // Toolbar
        div {
            attr.style "display: flex; align-items: center; gap: 8px; flex-shrink: 0; flex-wrap: wrap; padding: 4px 0;"

            Rz.icon(ctx.Definition.Icon, style = "font-size: 1.2rem; color: var(--rz-primary);")

            span {
                attr.style "font-weight: 600; font-size: 14px;"
                ctx.Schema.DisplayName
            }

            span {
                attr.style(
                    if isFiltered then
                        "padding: 2px 8px; border-radius: 10px; background: rgba(255, 193, 7, 0.2); color: #b78103; border: 1px solid #b78103; font-size: 12px; font-weight: 600;"
                    else
                        "padding: 2px 8px; border-radius: 10px; background: var(--rz-secondary-lighter); font-size: 12px;"
                )

                badgeText
            }

            div { attr.style "margin-left: auto; display: flex; align-items: center; gap: 8px;" }

            if selectedKey <> "" then
                span {
                    attr.style "color: var(--rz-text-secondary-color); font-size: 12px;"
                    $"Selected: {selectedKey}"
                }

            Rz.iconButton(
                "refresh",
                onClick = (fun (_: MouseEventArgs) -> dispatch RefreshRequested),
                tooltip = "Refresh data"
            )
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
                "AllowGrouping" => false
                "AllowVirtualization" => true
                "SelectionMode" => selectionMode
                "AllowRowSelectOnRowClick" => true
                "FormatProvider" => (ctx.Culture :> IFormatProvider)
                "CellTemplates" => CellRenderers.templates

                attr.callback "OnRowSelected" (fun (row: Value) ->
                    let mutable key: obj = null
                    let keyColumn = ctx.Schema.Columns |> List.tryFind(fun c -> c.IsKey)

                    match keyColumn with
                    | Some kc ->
                        if row.TryGetValue(kc.ColumnId, &key) then
                            dispatch(SelectionChanged(Some(string key)))
                    | None -> ())

                attr.callback "OnSort" (fun (args: DataGridColumnSortEventArgs<Value>) ->
                    let prop = args.Column.Property

                    let sortCriterion =
                        if args.SortOrder.HasValue then
                            let dir = if args.SortOrder.Value = SortOrder.Ascending then Ascending else Descending

                            Some
                                { Field = prop
                                  Direction = dir
                                  Priority = 1 }
                        else
                            None

                    dispatch(SortChanged sortCriterion))

                attr.callback "OnFilter" (fun (args: DataGridColumnFilterEventArgs<Value>) ->
                    let prop = args.Column.Property

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
                                { Field = prop
                                  Operator = op
                                  Value = Some v }
                        | None -> None

                    dispatch(FilterChanged(prop, filterCriterion)))
            }
        }
    }
