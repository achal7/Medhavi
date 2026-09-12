namespace Medhavi.Client.Rendering

open System.Globalization
open Bolero
open Medhavi.Common.Validation
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Context
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Session

type ComponentEvent =
    | SelectionChanged of rowKey: string option
    | MultiSelectionChanged of rowKeys: string list
    | ConfigurationChangeRequested of ComponentConfiguration
    | SortChanged of SortCriterion option
    | FilterChanged of columnId: string * FilterCriterion option
    | ActionRequested of actionId: ActionDefId * targetKey: string option * payload: ActionPayload
    | ParameterizedActionRequested of actionId: ActionDefId * targetKey: string option
    | RefreshRequested

type ComponentRenderContext =
    { Definition: ComponentDefinition
      Schema: SchemaDefinition
      Configuration: ComponentConfiguration
      SharedContext: SharedContext option
      DataState: DataState
      Data: DataPayload
      Selection: string option
      Culture: CultureInfo }

type CellRenderer = UniversalRow -> CellValue -> Node

type CellRendererRegistry = Map<string, CellRenderer>

module CellRendererRegistry =
    let empty: CellRendererRegistry = Map.empty

    let register (hint: string) (renderer: CellRenderer) (registry: CellRendererRegistry) : CellRendererRegistry =
        Map.add hint renderer registry

    let tryResolve (hint: string) (registry: CellRendererRegistry) : CellRenderer option = Map.tryFind hint registry

    /// Validates that every RenderHint in a schema resolves in the registry.
    /// Called at registration time in the composition root.
    let validateSchemaHints (schema: SchemaDefinition) (registry: CellRendererRegistry) : Validation<unit, string> =
        let unresolvedHints =
            schema.Columns
            |> List.choose(fun col -> col.RenderHint)
            |> List.distinct
            |> List.filter(fun hint -> not(Map.containsKey hint registry))

        match unresolvedHints with
        | [] -> Valid()
        | hints -> Invalid(hints |> List.map(fun h -> $"Unresolved RenderHint: {h}"))

/// PURITY CONTRACT: Render performs no immediate application effects.
/// It constructs a presentation description embedding the explicitly supplied event sink.
/// The rendering decision is pure; the sink is an effect capability owned by ViewSurface.
type ComponentRuntime =
    { Render: ComponentRenderContext -> (ComponentEvent -> unit) -> Node }
