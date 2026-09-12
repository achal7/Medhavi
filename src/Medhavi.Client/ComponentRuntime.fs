namespace Medhavi.Client.Rendering

open System.Globalization
open Bolero
open Medhavi.Client.SemanticModel

// ============================================================================
// R1.2 — Presentation runtime capability contract.
// DEPENDENCY RULE: depends on SemanticModel only. Never references Registries.
// ============================================================================

type ComponentEvent =
    | SelectionChanged of rowKey: string option
    | ConfigurationChangeRequested of ComponentConfiguration
    | ActionRequested of actionId: ActionDefId * targetKey: string option * payload: ActionPayload
    | RefreshRequested

type ComponentRenderContext =
    { Definition: ComponentDefinition
      Schema: SchemaDefinition
      Configuration: ComponentConfiguration
      SharedContext: SharedContext option
      DataState: DataState
      Selection: string option
      Culture: CultureInfo }

/// PURITY CONTRACT: Render performs no immediate application effects. It
/// constructs a presentation description embedding the explicitly supplied
/// event sink. The rendering decision is pure; the sink is an effect
/// capability owned by ViewSurface.
type ComponentRuntime =
    { Render: ComponentRenderContext -> (ComponentEvent -> unit) -> Node }
