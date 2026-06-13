namespace Medhavi.Web.Panels

open Microsoft.AspNetCore.Components
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor

type RemoteData<'T> =
    | NotRequested
    | Loading
    | Loaded of 'T
    | Failed of string

type RemoteState<'T>() =
    inherit Component()

    [<Parameter>]
    member val Data : RemoteData<'T> = NotRequested with get, set

    [<Parameter>]
    member val Template : 'T -> Node = (fun _ -> empty()) with get, set

    [<Parameter>]
    member val EmptyMessage : string = "No data available." with get, set

    override this.Render() =
        match this.Data with
        | NotRequested -> empty()
        | Loading ->
            div {
                attr.``class`` "rz-p-4 rz-display-flex rz-flex-column rz-align-items-center rz-justify-content-center"
                attr.style "min-height: 150px; gap: 12px;"
                comp<RadzenProgressBar> {
                    "Value" => 100.0
                    "Mode" => ProgressBarMode.Indeterminate
                    "Style" => "width: 200px;"
                }
                p { attr.``class`` "rz-color-text-secondary rz-m-0"; "Loading data..." }
            }
        | Failed err ->
            comp<RadzenAlert> {
                "AlertStyle" => AlertStyle.Danger
                "Title" => "Error"
                "Text" => err
                "AllowClose" => false
            }
        | Loaded payload ->
            this.Template payload
