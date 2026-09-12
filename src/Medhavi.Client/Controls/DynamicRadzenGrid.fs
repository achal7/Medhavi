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

open System.Collections.Generic

module DynamicGridRow =

    type Value = IDictionary<string, obj>

    let create (values: (string * obj) list) : Value =
        let row = Dictionary<string, obj>()
        values |> List.iter(fun (key: string, value: obj) -> row.Add(key, value))
        row :> IDictionary<string, obj>

open System.Globalization
open Microsoft.AspNetCore.Components
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
    member val AllowSorting: bool = true with get, set

    [<Parameter>]
    member val AllowFiltering: bool = true with get, set

    [<Parameter>]
    member val AllowGrouping: bool = false with get, set

    [<Parameter>]
    member val AllowVirtualization: bool = true with get, set

    [<Parameter>]
    member val SelectionMode: DataGridSelectionMode = DataGridSelectionMode.Single with get, set

    [<Parameter>]
    member val AllowRowSelectOnRowClick: bool = true with get, set

    [<Parameter>]
    member val FormatProvider: IFormatProvider = CultureInfo.CurrentCulture with get, set

    [<Parameter>]
    member val OnRowSelected: EventCallback<Value> = EventCallback<Value>.Empty with get, set

    [<Parameter>]
    member val OnSort: EventCallback<DataGridColumnSortEventArgs<Value>> = EventCallback<DataGridColumnSortEventArgs<Value>>.Empty with get, set

    [<Parameter>]
    member val OnFilter: EventCallback<DataGridColumnFilterEventArgs<Value>> = EventCallback<DataGridColumnFilterEventArgs<Value>>.Empty with get, set

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
        let propertyExpression: string =
            PropertyAccess.GetDynamicPropertyExpression(definition.Key, definition.ValueType)

        comp<RadzenDataGridColumn<Value>> {
            "Property" => propertyExpression
            "Title" => definition.Title
            "Type" => definition.ValueType
            "Sortable" => definition.Sortable
            "Filterable" => definition.Filterable
            "Groupable" => definition.Groupable
            "Frozen" => definition.Frozen
            "TextAlign" => definition.TextAlign

            match definition.Width with
            | Some width -> "Width" => width
            | None -> attr.empty()

            match definition.RenderHint with
            | Some hint when not (isNull this.CellTemplates) && this.CellTemplates.ContainsKey(hint) ->
                let templateFunc = this.CellTemplates.[hint]

                attr.fragmentWith "Template" (fun (row: Value) ->
                    let mutable value: obj = null

                    if row.TryGetValue(definition.Key, &value) then
                        templateFunc.Invoke(value)
                    else
                        Bolero.Html.empty())
            | _ ->
                attr.fragmentWith "Template" (fun (row: Value) ->
                    let mutable value: obj = null

                    if row.TryGetValue(definition.Key, &value) then
                        text(this.FormatValue definition.FormatString value)
                    else
                        text String.Empty)
        }

    override this.Render() =
        div {
            attr.style "width: 100%; height: 100%; min-height: 0;"

            comp<RadzenDataGrid<Value>> {
                "Data" => this.Rows
                "AllowPaging" => this.AllowPaging
                "AllowSorting" => this.AllowSorting
                "AllowFiltering" => this.AllowFiltering
                "AllowGrouping" => this.AllowGrouping
                "AllowVirtualization" => this.AllowVirtualization
                "AllowRowSelectOnRowClick" => this.AllowRowSelectOnRowClick
                "SelectionMode" => this.SelectionMode
                "FilterCaseSensitivity" => FilterCaseSensitivity.CaseInsensitive
                "Density" => Density.Compact
                "Responsive" => true
                "Style" => "width: 100%; height: 100%; min-height: 0;"

                attr.callback "RowSelect" (fun (row: Value) -> this.OnRowSelected.InvokeAsync(row) |> ignore)
                attr.callback "Sort" (fun (args: DataGridColumnSortEventArgs<Value>) -> this.OnSort.InvokeAsync(args) |> ignore)
                attr.callback "Filter" (fun (args: DataGridColumnFilterEventArgs<Value>) -> this.OnFilter.InvokeAsync(args) |> ignore)

                attr.fragment "Columns" (forEach this.Columns this.RenderColumn)
            }
        }
