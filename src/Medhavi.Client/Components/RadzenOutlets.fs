namespace Medhavi.Client.Components

open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Rendering
open Radzen.Blazor

type RadzenOutlets() =
    inherit ComponentBase()

    /// Absorbs Bolero's automatic ChildContent injection.
    /// This parameter is intentionally ignored - it is never passed to RadzenComponents.
    [<Parameter>]
    member val ChildContent: RenderFragment = Unchecked.defaultof<RenderFragment> with get, set

    override _.BuildRenderTree(builder: RenderTreeBuilder) =
        // Render RadzenComponents with NO child content.
        // This provides the outlet for Radzen services (Dialog, Notification, etc.)
        builder.OpenComponent<RadzenComponents>(0)
        builder.CloseComponent()
