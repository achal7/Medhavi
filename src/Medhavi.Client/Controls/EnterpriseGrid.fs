namespace Medhavi.Client.Controls

open System
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Microsoft.AspNetCore.Components.Web
open Medhavi.Client.ViewComposition

/// Declarative column specification for the unified enterprise data grid.
type ColumnSpec<'Row> =
    { PropertyName: string
      Title: string
      Width: string
      IsFrozen: bool
      Sortable: bool
      Filterable: bool
      FormatString: string option
      Template: ('Row -> Node) option }

module ColumnSpec =
    let create propertyName title width =
        { PropertyName = propertyName
          Title = title
          Width = width
          IsFrozen = false
          Sortable = true
          Filterable = true
          FormatString = None
          Template = None }

    let frozen propertyName title width =
        { PropertyName = propertyName
          Title = title
          Width = width
          IsFrozen = true
          Sortable = true
          Filterable = true
          FormatString = None
          Template = None }

    let withFormat format spec = { spec with FormatString = Some format }

    let withTemplate template spec = { spec with Template = Some template }

/// Configuration record for the pure EnterpriseGrid presentation control.
type EnterpriseGridConfig<'Row> =
    { Title: string
      Icon: string
      Data: 'Row list
      TotalRecordCount: int option
      Columns: ColumnSpec<'Row> list
      SelectedKey: string option
      ActiveFilters: FilterCriterion list
      ActiveSort: SortCriterion option
      RowKey: 'Row -> string
      ToolbarActions: Node list
      Banner: Node option
      OnRowSelected: string option -> unit
      OnSortChanged: SortCriterion option -> unit
      OnFiltersChanged: FilterCriterion list -> unit
      OnRefresh: unit -> unit }

module EnterpriseGrid =

    let render (config: EnterpriseGridConfig<'Row>) : Node =
        let selectedKey = config.SelectedKey |> Option.defaultValue ""
        let totalCount = config.TotalRecordCount |> Option.defaultValue(List.length config.Data)
        let currentCount = List.length config.Data
        let isFiltered = not(List.isEmpty config.ActiveFilters) || (currentCount < totalCount)

        let badgeText =
            if isFiltered then
                $"{currentCount} / {totalCount} records (Filtered)"
            else
                $"{currentCount} records"

        div {
            attr.style
                "width: 100%; height: 100%; display: flex; flex-direction: column; min-height: 0; gap: 8px; padding: 8px; box-sizing: border-box;"

            // Action Toolbar
            div {
                attr.style
                    "display: flex; align-items: center; gap: 8px; flex-shrink: 0; flex-wrap: wrap; padding: 4px 0;"

                Rz.icon(config.Icon, style = "font-size: 1.2rem; color: var(--rz-primary);")

                span {
                    attr.style "font-weight: 600; font-size: 14px;"
                    config.Title
                }

                span {
                    attr.style(
                        if isFiltered then
                            "padding: 2px 8px; border-radius: 10px; background: var(--rz-warning-lighter); color: var(--rz-warning-dark); border: 1px solid var(--rz-warning); font-size: 12px; font-weight: 600;"
                        else
                            "padding: 2px 8px; border-radius: 10px; background: var(--rz-secondary-lighter); font-size: 12px;"
                    )

                    badgeText
                }

                if not(List.isEmpty config.ToolbarActions) then
                    div { attr.style "width: 1px; height: 20px; background: var(--rz-border-color); margin: 0 4px;" }
                    forEach config.ToolbarActions id

                div { attr.style "margin-left: auto; display: flex; align-items: center; gap: 8px;" }

                if selectedKey <> "" then
                    span {
                        attr.style "color: var(--rz-text-secondary-color); font-size: 12px;"
                        $"Selected: {selectedKey}"
                    }

                Rz.iconButton(
                    "refresh",
                    onClick = (fun (_: MouseEventArgs) -> config.OnRefresh()),
                    tooltip = "Refresh data"
                )
            }

            // Optional Pegging / Scenario / Filter Banner
            match config.Banner with
            | Some bannerNode -> bannerNode
            | None -> empty()

            // Virtualized DataGrid with Two-Way Sort & Filter Synchronization
            div {
                attr.style "flex: 1; min-height: 0; width: 100%; height: 100%; overflow: hidden;"

                comp<RadzenDataGrid<'Row>> {
                    "Data" => config.Data
                    "AllowVirtualization" => true
                    "AllowPaging" => false
                    "AllowSorting" => true
                    "AllowFiltering" => true
                    "FilterCaseSensitivity" => FilterCaseSensitivity.CaseInsensitive
                    "AllowRowSelectOnRowClick" => true
                    "SelectionMode" => DataGridSelectionMode.Single
                    "Density" => Density.Compact
                    "Responsive" => true
                    "Style" => "width: 100%; height: 100%; min-height: 0;"

                    attr.callback "RowSelect" (fun (row: 'Row) -> config.OnRowSelected(Some(config.RowKey row)))

                    attr.callback "Sort" (fun (args: DataGridColumnSortEventArgs<'Row>) ->
                        let prop = args.Column.Property

                        let sortCriterion =
                            if args.SortOrder.HasValue then
                                let dir =
                                    if args.SortOrder.Value = SortOrder.Ascending then
                                        SortDirection.Ascending
                                    else
                                        SortDirection.Descending

                                Some
                                    { Field = prop
                                      Direction = dir
                                      Priority = 1 }
                            else
                                None

                        config.OnSortChanged sortCriterion)

                    attr.callback "Filter" (fun (args: DataGridColumnFilterEventArgs<'Row>) ->
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

                        let otherFilters = config.ActiveFilters |> List.filter(fun f -> f.Field <> prop)

                        let newFilters =
                            match filterValStr with
                            | Some v ->
                                { Field = prop
                                  Operator = op
                                  Value = Some v }
                                :: otherFilters
                            | None -> otherFilters

                        config.OnFiltersChanged newFilters)

                    attr.fragment
                        "Columns"
                        (concat {
                            for col in config.Columns do
                                let colFilter =
                                    config.ActiveFilters |> List.tryFind(fun f -> f.Field = col.PropertyName)

                                let isSorted =
                                    match config.ActiveSort with
                                    | Some s when s.Field = col.PropertyName ->
                                        if s.Direction = SortDirection.Ascending then
                                            Nullable SortOrder.Ascending
                                        else
                                            Nullable SortOrder.Descending
                                    | _ -> Nullable()

                                comp<RadzenDataGridColumn<'Row>> {
                                    "Property" => col.PropertyName
                                    "Title" => col.Title
                                    "Width" => col.Width
                                    "Frozen" => col.IsFrozen
                                    "Sortable" => col.Sortable
                                    "Filterable" => col.Filterable
                                    "SortOrder" => isSorted

                                    if Option.isSome colFilter && Option.isSome colFilter.Value.Value then
                                        "FilterValue" => (box colFilter.Value.Value.Value)

                                        "FilterOperator"
                                        => match colFilter.Value.Operator with
                                           | FilterOperator.Equals -> Radzen.FilterOperator.Equals
                                           | FilterOperator.NotEquals -> Radzen.FilterOperator.NotEquals
                                           | FilterOperator.GreaterThan -> Radzen.FilterOperator.GreaterThan
                                           | FilterOperator.GreaterThanOrEqual ->
                                               Radzen.FilterOperator.GreaterThanOrEquals
                                           | FilterOperator.LessThan -> Radzen.FilterOperator.LessThan
                                           | FilterOperator.LessThanOrEqual -> Radzen.FilterOperator.LessThanOrEquals
                                           | FilterOperator.Contains -> Radzen.FilterOperator.Contains
                                           | FilterOperator.IsEmpty -> Radzen.FilterOperator.IsEmpty
                                           | FilterOperator.IsNotEmpty -> Radzen.FilterOperator.IsNotEmpty
                                    else
                                        attr.empty()

                                    if Option.isSome col.FormatString then
                                        "FormatString" => col.FormatString.Value
                                    else
                                        attr.empty()

                                    if Option.isSome col.Template then
                                        attr.fragmentWith "Template" (fun (row: 'Row) -> col.Template.Value row)
                                    else
                                        attr.empty()
                                }
                        })
                }
            }
        }
