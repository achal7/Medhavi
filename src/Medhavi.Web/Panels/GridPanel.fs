namespace Medhavi.Web.Panels

open System
open Microsoft.AspNetCore.Components
open Bolero
open Bolero.Html
open Radzen.Blazor

type GridConfig<'T, 'Msg when 'T : not null> =
    { Columns: Node list
      Data: 'T list
      IsLoading: bool
      OnRowSelected: 'T -> 'Msg }

type GridPanel<'T, 'Msg when 'T : not null>() =
    inherit Component()

    [<Parameter>]
    member val Config : GridConfig<'T, 'Msg> = Unchecked.defaultof<GridConfig<'T, 'Msg>> with get, set

    [<Parameter>]
    member val Dispatch : 'Msg -> unit = (fun _ -> ()) with get, set

    override this.Render() =
        let onRowSelected =
            EventCallback.Factory.Create<'T>(
                this,
                Action<'T>(fun item -> this.Dispatch (this.Config.OnRowSelected item))
            )

        comp<RadzenDataGrid<'T>> {
            "Data" => this.Config.Data
            "AllowFiltering" => true
            "AllowSorting" => true
            "AllowPaging" => true
            "PageSize" => 10
            "AllowVirtualization" => false
            //"RowSelect" => onRowSelected
            attr.fragment "Columns" (forEach this.Config.Columns id)
        }
