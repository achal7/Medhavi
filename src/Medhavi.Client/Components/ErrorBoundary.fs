namespace Medhavi.Web.Components

open System
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Microsoft.AspNetCore.Components.Rendering
open Radzen
open Radzen.Blazor

type MedhaviErrorBoundary() =
    inherit ErrorBoundary()

    [<Parameter>]
    member val OnRetry : Action = null with get, set

    override this.OnParametersSet() =
        this.Recover()

    override this.BuildRenderTree(builder: RenderTreeBuilder) =
        if this.CurrentException = null then
            builder.AddContent(0, this.ChildContent)
        else
            let ex = this.CurrentException
            builder.OpenComponent<RadzenCard>(1)
            builder.AddAttribute(2, "Style", "margin: 20px; padding: 24px; border: 1px solid var(--rz-danger-color); background-color: rgba(233, 30, 99, 0.05); text-align: center; border-radius: 8px;")
            builder.AddAttribute(3, "ChildContent", RenderFragment(fun b ->
                b.OpenComponent<RadzenIcon>(4)
                b.AddAttribute(5, "Icon", "error_outline")
                b.AddAttribute(6, "Style", "font-size: 48px; color: var(--rz-danger-color);")
                b.CloseComponent()

                b.OpenElement(7, "h4")
                b.AddAttribute(8, "style", "margin: 16px 0; color: var(--rz-danger-color); font-weight: bold; font-family: var(--rz-font-family);")
                b.AddContent(9, "A critical workbench error occurred")
                b.CloseElement()

                b.OpenElement(10, "p")
                b.AddAttribute(11, "style", "font-size: 14px; margin-bottom: 20px; color: var(--rz-text-secondary-color); max-width: 600px; margin-left: auto; margin-right: auto; word-break: break-all; font-family: var(--rz-font-family);")
                b.AddContent(12, ex.Message)
                b.CloseElement()

                b.OpenComponent<RadzenButton>(13)
                b.AddAttribute(14, "Text", "Retry & Refresh")
                b.AddAttribute(15, "ButtonStyle", ButtonStyle.Danger)
                let onClick = EventCallback.Factory.Create<MouseEventArgs>(this, Action<MouseEventArgs>(fun _ ->
                    this.Recover()
                    if this.OnRetry <> null then this.OnRetry.Invoke()
                ))
                b.AddAttribute(16, "Click", onClick)
                b.CloseComponent()
            ))
            builder.CloseComponent()
