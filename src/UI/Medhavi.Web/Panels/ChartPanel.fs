namespace Medhavi.Web.Panels

open Microsoft.AspNetCore.Components
open Bolero
open Bolero.Html
open Radzen.Blazor

type ChartConfig<'T> =
    { Data: 'T list
      Title: string
      CategoryProperty: string
      ValueProperty: string }

type ChartPanel<'T>() =
    inherit Component()

    [<Parameter>]
    member val Config : ChartConfig<'T> = Unchecked.defaultof<ChartConfig<'T>> with get, set

    override this.Render() =
        comp<RadzenChart> {
            comp<RadzenColumnSeries<'T>> {
                "Data" => this.Config.Data
                "CategoryProperty" => this.Config.CategoryProperty
                "ValueProperty" => this.Config.ValueProperty
                "Title" => this.Config.Title
            }
        }
