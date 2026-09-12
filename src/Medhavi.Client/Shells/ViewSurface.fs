namespace Medhavi.Client.Shells

open System.Globalization
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Medhavi.Common.Validation
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Context
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Layout
open Medhavi.Client.ViewComposition.Session
open Medhavi.Client.Registries
open Medhavi.Client.Rendering

// ============================================================================
// ViewSurface boundary.
// Dependency direction: ViewSurface → Registries → Rendering → ViewComposition.
// ============================================================================

/// Presentation intents emitted by the surface adapter.
type ViewSurfaceMsg =
    | ComponentEvent of ViewComponentId * ComponentEvent
    | PlacementAdjustRequested of ViewComponentId * TilePosition

/// Adapter-level infrastructure failure. Routed through diagnostic/telemetry
/// boundary, never through the normal Elmish message alphabet.
type LayoutAdapterError =
    | InvalidIdentityTransport of raw: string
    | InvalidPositionTransport of id: ViewComponentId * col: int * row: int * colSpan: int * rowSpan: int

/// Presentation-only dashboard hints. Not semantic state.
type DashboardRenderHints = { RowHeight: int; Gap: int }

// ============================================================================
// Component resolution with compatibility validation.
// ============================================================================

module ComponentResolver =

    type ResolutionError =
        | ComponentNotRegistered of ComponentDefId
        | SchemaNotRegistered of SchemaDefId
        | IncompatibleBinding of CompatibilityError list

    /// Resolution = identity resolution + compatibility validation.
    /// Re-validates because definitions may have evolved since the
    /// ViewComponent was persisted.
    let resolve
        (components: ComponentRegistry)
        (schemas: SchemaRegistry)
        (vc: ViewComponent)
        : Result<ComponentRegistration * SchemaDefinition, ResolutionError> =
        match ComponentRegistry.find vc.ComponentDefId components with
        | Error _ -> Error(ComponentNotRegistered vc.ComponentDefId)
        | Ok registration ->
            match SchemaRegistry.find vc.Binding.SchemaDefId schemas with
            | Error _ -> Error(SchemaNotRegistered vc.Binding.SchemaDefId)
            | Ok schema ->
                match SchemaCompatibility.validate registration.Definition schema with
                | Valid() -> Ok(registration, schema)
                | Invalid errors -> Error(IncompatibleBinding errors)

    /// Context construction: resolved semantics + runtime state.
    /// ViewRenderState ABSENCE SEMANTICS (explicit invariant):
    ///   missing DataStates entry  → Idle
    ///   missing Selections entry  → None
    let buildContext
        (id: ViewComponentId)
        (vc: ViewComponent)
        (definition: ComponentDefinition)
        (schema: SchemaDefinition)
        (renderState: ViewRenderState)
        (dataStores: Map<DataStoreId, Medhavi.Client.ViewComposition.Data.DataPayload>)
        (culture: CultureInfo)
        : ComponentRenderContext =
        let data =
            dataStores
            |> Map.tryFind vc.Binding.StoreId
            |> Option.defaultValue Medhavi.Client.ViewComposition.Data.DataPayload.NoData

        { Definition = definition
          Schema = schema
          Configuration = vc.Configuration
          Data = data
          SharedContext = renderState.State.SharedContext
          DataState = renderState.DataStates |> Map.tryFind id |> Option.defaultValue DataState.Idle
          Selection = renderState.Selections |> Map.tryFind id |> Option.defaultValue None
          Culture = culture }

// ============================================================================
// Layout interpretation (recursive/catamorphic interpreter).
// ============================================================================

module LayoutInterpreter =

    /// Adapter: Radzen tile change → semantic placement value.
    let private adaptTileChange
        (dispatch: ViewSurfaceMsg -> unit)
        (onAdapterError: LayoutAdapterError -> unit)
        (item: RadzenTileLayoutItem)
        : unit =
        let raw = string item.Title

        match ViewComponentId.create raw with
        | Error _ -> onAdapterError(InvalidIdentityTransport raw)
        | Ok id ->
            match TilePosition.create item.Col item.Row item.ColSpan item.RowSpan with
            | Error _ -> onAdapterError(InvalidPositionTransport(id, item.Col, item.Row, item.ColSpan, item.RowSpan))
            | Ok pos -> dispatch(PlacementAdjustRequested(id, pos))

    /// Layout interpretation is referentially transparent with respect to
    /// semantic inputs. The supplied callbacks (leaf, dispatch, onAdapterError)
    /// are the explicit presentation effect boundary.
    let rec render
        (hints: DashboardRenderHints)
        (isEditMode: bool)
        (leaf: ViewComponentId -> Node)
        (dispatch: ViewSurfaceMsg -> unit)
        (onAdapterError: LayoutAdapterError -> unit)
        (layout: LayoutDefinition)
        : Node =
        match layout with
        | Empty ->
            div {
                attr.``class`` "view-region--empty"
                text "Empty region"
            }
        | Single id -> leaf id
        | Split(direction, left, right, ratio) ->
            let pct = SplitRatio.value ratio * 100m

            comp<RadzenSplitter> {
                "Orientation"
                => (if direction = SplitDirection.Horizontal then
                        Orientation.Horizontal
                    else
                        Orientation.Vertical)

                comp<RadzenSplitterPane> {
                    "Size" => $"{pct}%%"
                    render hints isEditMode leaf dispatch onAdapterError left
                }

                comp<RadzenSplitterPane> {
                    "Size" => $"{100m - pct}%%"
                    render hints isEditMode leaf dispatch onAdapterError right
                }
            }
        | Dashboard(dims, tiles) ->
            comp<RadzenTileLayout> {
                "Columns" => GridDimensions.columns dims
                "Rows" => GridDimensions.rows dims
                "RowHeight" => float hints.RowHeight
                "Gap" => float hints.Gap
                "EditMode" => isEditMode
                "ShowGrid" => isEditMode
                attr.callback "Change" (adaptTileChange dispatch onAdapterError)

                attr.fragment
                    "ChildContent"
                    (forEach (Map.toList tiles) (fun (id, pos) ->
                        comp<RadzenTileLayoutItem> {
                            attr.key(ViewComponentId.value id)
                            "Title" => ViewComponentId.value id
                            "Col" => TilePosition.col pos
                            "Row" => TilePosition.row pos
                            "ColSpan" => TilePosition.colSpan pos
                            "RowSpan" => TilePosition.rowSpan pos
                            attr.fragment "ChildContent" (leaf id)
                        }))
            }
        | Tabbed tabs ->
            comp<RadzenTabs> {
                attr.fragment
                    "Tabs"
                    (forEach tabs (fun tab ->
                        comp<RadzenTabsItem> {
                            "Text" => tab.Label
                            render hints isEditMode leaf dispatch onAdapterError tab.Content
                        }))
            }

// ============================================================================
// Top-level ViewSurface interpreter.
// ============================================================================

module ViewSurface =

    /// Leaf rendering: resolve, build context, invoke renderer.
    let private renderLeaf
        (components: ComponentRegistry)
        (schemas: SchemaRegistry)
        (dataStores: Map<DataStoreId, Medhavi.Client.ViewComposition.Data.DataPayload>)
        (renderState: ViewRenderState)
        (culture: CultureInfo)
        (dispatch: ViewSurfaceMsg -> unit)
        (id: ViewComponentId)
        : Node =
        match renderState.State.Components |> Map.tryFind id with
        | None ->
            div {
                attr.``class`` "view-unresolved"
                text $"Unresolved occurrence: {ViewComponentId.value id}"
            }
        | Some vc ->
            match ComponentResolver.resolve components schemas vc with
            | Error(ComponentResolver.ComponentNotRegistered defId) ->
                div {
                    attr.``class`` "view-resolution-error"
                    text $"Component not registered: {ComponentDefId.value defId}"
                }
            | Error(ComponentResolver.SchemaNotRegistered schemaId) ->
                div {
                    attr.``class`` "view-resolution-error"
                    text $"Schema not registered: {SchemaDefId.value schemaId}"
                }
            | Error(ComponentResolver.IncompatibleBinding _) ->
                div {
                    attr.``class`` "view-incompatible"
                    text $"Incompatible binding: {ViewComponentId.value id}"
                }
            | Ok(registration, schema) ->
                let ctx = ComponentResolver.buildContext id vc registration.Definition schema renderState dataStores culture
                let sink evt = dispatch(ComponentEvent(id, evt))
                registration.Runtime.Render ctx sink

    /// Top-level interpreter: registries + render state → Node.
    let render
        (components: ComponentRegistry)
        (schemas: SchemaRegistry)
        (dataStores: Map<DataStoreId, Medhavi.Client.ViewComposition.Data.DataPayload>)
        (renderState: ViewRenderState)
        (culture: CultureInfo)
        (hints: DashboardRenderHints)
        (isEditMode: bool)
        (dispatch: ViewSurfaceMsg -> unit)
        (onAdapterError: LayoutAdapterError -> unit)
        : Node =
        let leaf = renderLeaf components schemas dataStores renderState culture dispatch
        LayoutInterpreter.render hints isEditMode leaf dispatch onAdapterError renderState.State.Layout
