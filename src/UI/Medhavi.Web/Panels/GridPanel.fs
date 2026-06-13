namespace Medhavi.Web.Panels

open System
open System.Collections.Generic
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor

type GridConfig<'T, 'Msg> =
    { Columns: Node list
      Data: 'T list
      IsLoading: bool
      OnRowSelected: 'T -> 'Msg }

type GridPanel<'T, 'Msg>() =
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
            "RowSelect" => onRowSelected
            attr.fragment "Columns" (forEach this.Config.Columns id)
        }
