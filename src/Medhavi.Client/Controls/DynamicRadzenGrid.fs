namespace Medhavi.Client.Controls

open System
open Radzen

module DynamicGridColumn =

    type Definition =
        { Key: string
          Title: string
          ValueType: Type
          FormatString: string option
          Width: string option
          TextAlign: TextAlign
          Sortable: bool
          Filterable: bool
          Groupable: bool
          Frozen: bool
          Visible: bool
          RenderHint: string option }

    let create (key: string) (title: string) (valueType: Type) : Definition =
        { Key = key
          Title = title
          ValueType = valueType
          FormatString = None
          Width = None
          TextAlign = TextAlign.Left
          Sortable = true
          Filterable = true
          Groupable = true
          Frozen = false
          Visible = true
          RenderHint = None }

    let withFormat (formatString: string) (definition: Definition) : Definition =
        { definition with
            FormatString = Some formatString }

    let withWidth (width: string) (definition: Definition) : Definition = { definition with Width = Some width }

    let withTextAlign (textAlign: TextAlign) (definition: Definition) : Definition =
        { definition with
            TextAlign = textAlign }

    let frozen (definition: Definition) : Definition = { definition with Frozen = true }

    let withSortable (sortable: bool) (definition: Definition) : Definition = { definition with Sortable = sortable }

    let withFilterable (filterable: bool) (definition: Definition) : Definition =
        { definition with
            Filterable = filterable }

    let withGroupable (groupable: bool) (definition: Definition) : Definition =
        { definition with
            Groupable = groupable }

    let withVisible (visible: bool) (definition: Definition) : Definition =
        { definition with
            Visible = visible }

open System.Collections.Generic

module DynamicGridRow =

    type Value = IDictionary<string, obj>

    let create (values: (string * obj) list) : Value =
        let row = Dictionary<string, obj>()
        values |> List.iter(fun (key: string, value: obj) -> row.Add(key, value))
        row :> IDictionary<string, obj>

open System.Globalization
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Bolero
open Bolero.Html
open Radzen.Blazor
open DynamicGridColumn
open DynamicGridRow

type DynamicRadzenGrid() =
    inherit Component()

    let mutable lastCacheKey: obj = null

    [<Parameter>]
    member val Rows: IReadOnlyList<Value> = Array.empty<Value> :> IReadOnlyList<Value> with get, set

    [<Parameter>]
    member val CacheKey: obj = null with get, set

    [<Parameter>]
    member val DataBuilder: Func<IReadOnlyList<Value>> = null with get, set

    override this.OnParametersSet() =
        base.OnParametersSet()
        if not (isNull this.DataBuilder) && not (Unchecked.equals this.CacheKey lastCacheKey) then
            this.Rows <- this.DataBuilder.Invoke()
            lastCacheKey <- this.CacheKey

    [<Parameter>]
    member val Columns: IReadOnlyList<Definition> = Array.empty<Definition> :> IReadOnlyList<Definition> with get, set

    [<Parameter>]
    member val AllowPaging: bool = false with get, set

    [<Parameter>]
    member val PageSize: int = 50 with get, set

    [<Parameter>]
    member val AllowSorting: bool = true with get, set

    [<Parameter>]
    member val AllowFiltering: bool = true with get, set

    [<Parameter>]
    member val AllowGrouping: bool = true with get, set

    [<Parameter>]
    member val AllowVirtualization: bool = false with get, set

    [<Parameter>]
    member val AllowColumnResize: bool = true with get, set

    [<Parameter>]
    member val AllowColumnPicking: bool = true with get, set

    [<Parameter>]
    member val SelectedValues: IList<Value> = (ResizeArray<Value>() :> IList<Value>) with get, set

    [<Parameter>]
    member val ActiveSortField: string option = None with get, set

    [<Parameter>]
    member val ActiveSortDirection: string option = None with get, set

    [<Parameter>]
    member val SelectedKey: string option = None with get, set

    [<Parameter>]
    member val KeyColumnId: string option = None with get, set

    [<Parameter>]
    member val SelectionMode: DataGridSelectionMode = DataGridSelectionMode.Single with get, set

    [<Parameter>]
    member val AllowRowSelectOnRowClick: bool = true with get, set

    [<Parameter>]
    member val FormatProvider: IFormatProvider = CultureInfo.CurrentCulture with get, set

    [<Parameter>]
    member val OnRowSelected: Action<Value> = null with get, set

    [<Parameter>]
    member val OnSort: Action<DataGridColumnSortEventArgs<Value>> = null with get, set

    [<Parameter>]
    member val OnFilter: Action<DataGridColumnFilterEventArgs<Value>> = null with get, set

    [<Parameter>]
    member val CellTemplates: IDictionary<string, Func<obj, Node>> = null with get, set

    member private this.FormatValue (formatString: string option) (value: obj) : string =
        if isNull value then
            String.Empty
        else
            match formatString, value with
            | Some format, (:? IFormattable as formattable) -> formattable.ToString(format, this.FormatProvider)
            | _, (:? DateTime as dateTime) -> dateTime.ToString("yyyy-MM-dd", this.FormatProvider)
            | _, (:? DateTimeOffset as dateTimeOffset) -> dateTimeOffset.ToString("yyyy-MM-dd", this.FormatProvider)
            | _ -> Convert.ToString(value, this.FormatProvider)

    member private this.RenderColumn(definition: Definition) : Node =
        let isSorted =
            match this.ActiveSortField with
            | Some field when field = definition.Key ->
                if this.ActiveSortDirection = Some "Descending" then
                    Nullable SortOrder.Descending
                else
                    Nullable SortOrder.Ascending
            | _ -> Nullable()

        comp<RadzenDataGridColumn<Value>> {
            "Property" => definition.Key
            "GroupProperty" => $@"it[""{definition.Key}""]"
            "Title" => definition.Title
            "Type" => definition.ValueType
            "Sortable" => definition.Sortable
            "Filterable" => definition.Filterable
            "Groupable" => definition.Groupable
            "Frozen" => definition.Frozen
            "TextAlign" => definition.TextAlign
            "Visible" => definition.Visible
            "Pickable" => true
            "ColumnPickerTitle" => definition.Title
            "SortOrder" => isSorted

            match definition.Width with
            | Some width -> "Width" => width
            | None -> attr.empty()

            let renderContent (row: Value) =
                let mutable value: obj = null

                let contentNode =
                    match definition.RenderHint with
                    | Some hint when not (isNull this.CellTemplates) && this.CellTemplates.ContainsKey(hint) ->
                        let templateFunc = this.CellTemplates.[hint]

                        if row.TryGetValue(definition.Key, &value) then
                            templateFunc.Invoke(value)
                        else
                            Bolero.Html.empty()
                    | _ ->
                        if row.TryGetValue(definition.Key, &value) then
                            text(this.FormatValue definition.FormatString value)
                        else
                            text String.Empty

                div {
                    attr.style "width: 100%; height: 100%; min-height: 24px; display: flex; align-items: center;"




                    contentNode
                }

            attr.fragmentWith "Template" (fun (row: Value) -> renderContent row)
        }

    override this.Render() =
        div {
            attr.style "width: 100%; height: 100%; min-height: 0;"

            comp<RadzenDataGrid<Value>> {
                "Data" => this.Rows
                "Count" => this.Rows.Count
                "AllowPaging" => this.AllowPaging
                "PageSize" => this.PageSize
                "AllowSorting" => this.AllowSorting
                "AllowFiltering" => this.AllowFiltering
                "AllowGrouping" => this.AllowGrouping
                "AllGroupsExpanded" => Nullable true
                "GroupPanelText" => "Drag a column header here and drop it to group by that column"
                "AllowVirtualization" => (this.AllowVirtualization && not this.AllowGrouping)
                "AllowRowSelectOnRowClick" => this.AllowRowSelectOnRowClick
                "AllowColumnResize" => this.AllowColumnResize
                "AllowColumnPicking" => this.AllowColumnPicking
                "Value" => this.SelectedValues
                "SelectionMode" => this.SelectionMode
                "FilterCaseSensitivity" => FilterCaseSensitivity.CaseInsensitive
                "Density" => Density.Compact
                "Responsive" => true
                "Style" => "width: 100%; height: 100%; min-height: 0;"

                "RowRender" => Action<RowRenderEventArgs<Value>>(fun args ->
                    match this.SelectedKey, this.KeyColumnId with
                    | Some key, Some kc when not (String.IsNullOrEmpty key) ->
                        let mutable v: obj = null
                        if not (isNull (box args.Data)) && args.Data.TryGetValue(kc, &v) && string v = key then
                            let existingClass = if args.Attributes.ContainsKey("class") then string args.Attributes.["class"] + " " else ""
                            args.Attributes.["class"] <- existingClass + "rz-state-highlight"
                            let existingStyle = if args.Attributes.ContainsKey("style") then string args.Attributes.["style"] + "; " else ""
                            args.Attributes.["style"] <- existingStyle + "background-color: var(--rz-primary-lighter) !important; color: var(--rz-primary-dark) !important; font-weight: 600;"
                    | _ -> ())

                "CellRender" => Action<DataGridCellRenderEventArgs<Value>>(fun args ->
                    match this.SelectedKey, this.KeyColumnId with
                    | Some key, Some kc when not (String.IsNullOrEmpty key) ->
                        let mutable v: obj = null
                        if not (isNull (box args.Data)) && args.Data.TryGetValue(kc, &v) && string v = key then
                            let existingStyle = if args.Attributes.ContainsKey("style") then string args.Attributes.["style"] + "; " else ""
                            args.Attributes.["style"] <- existingStyle + "background-color: var(--rz-primary-lighter) !important; color: var(--rz-primary-dark) !important; font-weight: 600;"
                    | _ -> ())

                "LoadData" => EventCallback.Factory.Create<LoadDataArgs>(obj(), Action<LoadDataArgs>(fun _ -> ()))

                "RowSelect" => EventCallback.Factory.Create<Value>(this, Action<Value>(fun row ->
                    if not (isNull this.OnRowSelected) then
                        this.OnRowSelected.Invoke(row)))

                "RowClick" => EventCallback.Factory.Create<DataGridRowMouseEventArgs<Value>>(this, Action<DataGridRowMouseEventArgs<Value>>(fun _ -> ()))



                "CellClick" => EventCallback.Factory.Create<DataGridCellMouseEventArgs<Value>>(this, Action<DataGridCellMouseEventArgs<Value>>(fun _ -> ()))



                "RowDeselect" => EventCallback.Factory.Create<Value>(this, Action<Value>(fun _ -> ()))

                "ValueChanged" => EventCallback.Factory.Create<IList<Value>>(this, Action<IList<Value>>(fun _ -> ()))



                "Sort" => EventCallback.Factory.Create<DataGridColumnSortEventArgs<Value>>(this, Action<DataGridColumnSortEventArgs<Value>>(fun args ->
                    if not (isNull this.OnSort) then this.OnSort.Invoke(args)))

                "Filter" => EventCallback.Factory.Create<DataGridColumnFilterEventArgs<Value>>(this, Action<DataGridColumnFilterEventArgs<Value>>(fun args ->
                    if not (isNull this.OnFilter) then this.OnFilter.Invoke(args)))

                attr.fragment "Columns" (forEach this.Columns this.RenderColumn)
            }
        }
