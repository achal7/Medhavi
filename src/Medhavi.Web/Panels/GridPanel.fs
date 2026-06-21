namespace Medhavi.Web.Panels

open System
open Microsoft.AspNetCore.Components
open Bolero
open Bolero.Html
open Medhavi.Web.Controls

type GridConfig<'T, 'Msg when 'T : not null> =
    { Columns: Node list
      Data: 'T list
      IsLoading: bool
      OnRowSelected: 'T -> 'Msg }

type GridPanel<'T, 'Msg when 'T : not null>() =
    inherit Component()

    let mutable allowGrouping = false

    let gridRef = Ref<Radzen.Blazor.RadzenDataGrid<'T>>()
    
    [<Parameter>]
    member val Config : GridConfig<'T, 'Msg> = Unchecked.defaultof<GridConfig<'T, 'Msg>> with get, set

    [<Parameter>]
    member val Dispatch : 'Msg -> unit = (fun _ -> ()) with get, set

    [<Parameter>]
    member val OnRefresh : (unit -> unit) option = None with get, set

    [<Parameter>]
    member val ShowGroupingToggle : bool = false with get, set

    [<Parameter>]
    member val SearchText : string = "" with get, set

    [<Parameter>]
    member val SearchPlaceholder : string = "Search..." with get, set

    [<Parameter>]
    member val OnSearchChanged : (string -> unit) option = None with get, set

    member private this.Refresh() =
        match this.OnRefresh with
        | Some refresh -> refresh()
        | None -> ()
        // Optionally reload the grid to apply external data changes
        match gridRef.Value with
        | None -> ()
        | Some grid -> grid.Reload().Wait()


    override this.Render() =
        let onRowSelected =
            EventCallback.Factory.Create<'T>(
                this,
                Action<'T>(fun item -> this.Dispatch (this.Config.OnRowSelected item))
            )
        let toolbar =
            div {
                attr.style "display: flex; gap: 0.5rem; margin-bottom: 0.5rem;"

                // Search box (if enabled)
                if this.OnSearchChanged.IsSome then
                    Rz.textBox(
                        value = this.SearchText,
                        placeholder = this.SearchPlaceholder,
                        style = "flex: 1; width: 250px;",
                        valueChanged = (fun s -> this.OnSearchChanged.Value(s))
                    )

                // Refresh button
                if this.OnRefresh.IsSome then
                    Rz.button(
                        "",
                        (fun _ -> this.Refresh()),
                        style = Radzen.ButtonStyle.Secondary,
                        icon = "refresh"
                    )
            }
        
        Rz.dataGrid(this.Config.Data, headerTemplate = toolbar, columns = this.Config.Columns, allowFiltering = true, allowSorting = true, allowResize = true, allowPaging = true, pageSize = 20, showFooter = true, allowVirtualization = true, allowGrouping = true, allowColumnPicking = true)
        
