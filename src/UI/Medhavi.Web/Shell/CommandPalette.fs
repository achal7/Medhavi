namespace Medhavi.Web.Shell

open System
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Medhavi.Web
open Medhavi.Web.Services
open Medhavi.Web.WorkspaceEngine

type CommandPalette() =
    inherit Component()

    [<Parameter>]
    member val IsOpen : bool = false with get, set

    [<Parameter>]
    member val SearchText : string = "" with get, set

    [<Parameter>]
    member val Results : GlobalSearchResult list = [] with get, set

    [<Parameter>]
    member val OnSearch : string -> unit = (fun _ -> ()) with get, set

    [<Parameter>]
    member val OnSelect : GlobalSearchResult -> unit = (fun _ -> ()) with get, set

    [<Parameter>]
    member val OnClose : unit -> unit = (fun () -> ()) with get, set

    override this.Render() =
        if not this.IsOpen then
            empty()
        else
            let getResultText res =
                match res with
                | WorkbenchResult (_, title) -> sprintf "💻 Go to: %s" title
                | EntityResult (EntityRef (t, id), display) -> sprintf "🔍 View: %s (%s)" display t
                | CapabilityResult (_, name) -> sprintf "⚡ Run: %s" name

            div {
                attr.``class`` "rz-dialog-mask"
                attr.style "position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; z-index: 10000; background-color: rgba(0,0,0,0.6); display: flex; align-items: start; justify-content: center; padding-top: 10vh;"
                on.click (fun _ -> this.OnClose())
                
                div {
                    attr.style "width: 500px; background-color: var(--rz-header-background-color, #2b3a4a); border: 1px solid var(--rz-border-color); border-radius: 8px; box-shadow: 0 10px 25px rgba(0,0,0,0.5); padding: 16px; display: flex; flex-direction: column; gap: 12px;"
                    on.stopPropagation "click" true
                    
                    div {
                        attr.style "display: flex; align-items: center; justify-content: space-between;"
                        h3 { attr.style "margin: 0; font-size: 16px; font-weight: bold; font-family: var(--rz-font-family); color: var(--rz-header-color, #ffffff);"; "Command Palette" }
                        button {
                            attr.style "background: transparent; border: none; cursor: pointer; color: var(--rz-text-secondary-color);"
                            on.click (fun _ -> this.OnClose())
                            comp<RadzenIcon> { "Icon" => "close" }
                        }
                    }

                    input {
                        attr.``class`` "rz-textbox"
                        attr.placeholder "Type to search workbenches or entities..."
                        attr.value this.SearchText
                        attr.style "width: 100%; font-size: 14px; padding: 10px;"
                        on.input (fun (args: ChangeEventArgs) ->
                            let txt = match args.Value with null -> "" | v -> string v
                            this.OnSearch txt)
                    }

                    if not this.Results.IsEmpty then
                        ul {
                            attr.style "list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; gap: 4px; max-height: 250px; overflow-y: auto;"
                            for res in this.Results do
                                li {
                                    attr.style "padding: 8px 12px; border-radius: 4px; cursor: pointer; transition: background-color 0.2s; color: var(--rz-text-color); font-family: var(--rz-font-family); font-size: 13px;"
                                    on.click (fun _ -> 
                                        this.OnSelect res
                                        this.OnClose())
                                    attr.``class`` "theme-item"
                                    getResultText res
                                }
                        }
                    elif not (String.IsNullOrWhiteSpace(this.SearchText)) then
                        span { attr.style "font-size: 12px; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family); text-align: center; display: block; margin: 8px 0;"; "No results found." }
                }
            }
