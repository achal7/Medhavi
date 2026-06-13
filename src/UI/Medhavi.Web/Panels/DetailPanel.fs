namespace Medhavi.Web.Panels

open Microsoft.AspNetCore.Components
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor

type DetailConfig =
    { Title: string
      Items: (string * string) list
      OnClose: unit -> unit }

type DetailPanel() =
    inherit Component()

    [<Parameter>]
    member val Config : DetailConfig = Unchecked.defaultof<DetailConfig> with get, set

    override this.Render() =
        comp<RadzenCard> {
            "Style" => "padding: 16px; border-radius: 8px; border: 1px solid var(--rz-border-color);"
            comp<RadzenStack> {
                "Gap" => "12px"
                div {
                    attr.style "display: flex; justify-content: space-between; align-items: center;"
                    h4 { attr.style "margin: 0; font-weight: bold; font-family: var(--rz-font-family);"; this.Config.Title }
                    button {
                        attr.style "background: transparent; border: none; cursor: pointer; color: var(--rz-text-secondary-color);"
                        on.click (fun _ -> this.Config.OnClose())
                        comp<RadzenIcon> { "Icon" => "close" }
                    }
                }
                comp<RadzenStack> {
                    "Gap" => "8px"
                    for (label, value) in this.Config.Items do
                        div {
                            attr.style "display: flex; border-bottom: 1px solid var(--rz-border-color); padding-bottom: 6px;"
                            span { attr.style "width: 150px; font-weight: 500; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"; label }
                            span { attr.style "flex: 1; font-family: var(--rz-font-family);"; value }
                        }
                }
            }
        }
