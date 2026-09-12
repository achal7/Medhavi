namespace Medhavi.Client.Components

open System.Threading.Tasks
open Microsoft.AspNetCore.Components
open Bolero
open Bolero.Html
open Radzen
open Medhavi.Contracts.Client

type ThemeSelector() =
    inherit ComponentBase()

    [<Inject>]
    member val RadzenTheme: Radzen.ThemeService = Unchecked.defaultof<_> with get, set

    let themes: (VisualTheme * string) list =
        [ Material, "Material"
          MaterialDark, "Material Dark"
          Standard, "Standard"
          StandardDark, "Standard Dark"
          Default, "Default"
          DefaultDark, "Default Dark"
          Humanistic, "Humanistic"
          Software, "Software" ]

    let mutable selected: string = VisualTheme.cssName Material

    let onThemeSelected (cssName: string) =
        task {
            selected <- cssName
            do! this.RadzenTheme.ChangeThemeAsync(cssName)
            this.StateHasChanged()
        }

    override this.BuildRenderTree(builder) =
        comp<RadzenDropDown<string>> {
            "Data" => (themes |> List.map snd)
            "Value" => selected
            "Change" => EventCallback.Factory.Create<string>(this, System.Action<string>(onThemeSelected >> ignore))
            "Placeholder" => "Select theme"
            "Style" => "width: 200px;"
        }
