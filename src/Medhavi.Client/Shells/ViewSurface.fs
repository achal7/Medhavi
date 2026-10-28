namespace Medhavi.Client.Shells

open System.Globalization
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Medhavi.Common.Validation
open Medhavi.Client.Controls
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
            let schema = registration.Schema
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

        {
            Definition = definition
            Schema = schema
            Configuration = vc.Configuration
            Data = data
            SharedContext = renderState.State.SharedContext
            DataState = renderState.DataStates |> Map.tryFind id |> Option.defaultValue DataState.Idle
            Selection = renderState.Selections |> Map.tryFind id |> Option.defaultValue None
            Culture = culture
        }

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
        printfn "[TILE-CHANGE] adaptTileChange fired! item.Title='%s', Col=%d, Row=%d" item.Title item.Col item.Row
        let raw = string item.Title

        match ViewComponentId.create raw with
        | Error _ -> onAdapterError(InvalidIdentityTransport raw)
        | Ok id ->
            match TilePosition.create item.Col item.Row item.ColSpan item.RowSpan with
            | Error _ -> onAdapterError(InvalidPositionTransport(id, item.Col, item.Row, item.ColSpan, item.RowSpan))
            | Ok pos -> dispatch(PlacementAdjustRequested(id, pos))

    /// Layout interpretation is referentially transparent with respect to
    /// semantic inputs. The supplied callbacks (leaf, header, dispatch, onAdapterError)
    /// are the explicit presentation effect boundary.
    let rec renderWithHeader
        (hints: DashboardRenderHints)
        (isEditMode: bool)
        (leaf: ViewComponentId -> Node)
        (header: ViewComponentId -> string * string)
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
                    renderWithHeader hints isEditMode leaf header dispatch onAdapterError left
                }

                comp<RadzenSplitterPane> {
                    "Size" => $"{100m - pct}%%"
                    renderWithHeader hints isEditMode leaf header dispatch onAdapterError right
                }
            }
        | Dashboard(dims, tiles) ->
            if not isEditMode then
                let cols = GridDimensions.columns dims
                let rowH = float hints.RowHeight
                let gap = float hints.Gap

                let gridStyle =
                    "display: grid; grid-template-columns: repeat("
                    + string cols
                    + ", minmax(0, 1fr)); "
                    + "grid-auto-rows: "
                    + string rowH
                    + "px; "
                    + "gap: "
                    + string gap
                    + "px; "
                    + "width: 100%; height: 100%; overflow: auto; box-sizing: border-box;"

                div {
                    attr.style gridStyle

                    forEach (Map.toList tiles) (fun (id, pos) ->
                        let title, icon = header id
                        let c = TilePosition.col pos
                        let r = TilePosition.row pos
                        let cs = TilePosition.colSpan pos
                        let rs = TilePosition.rowSpan pos

                        let itemStyle =
                            "grid-column: "
                            + string c
                            + " / span "
                            + string cs
                            + "; "
                            + "grid-row: "
                            + string r
                            + " / span "
                            + string rs
                            + "; "
                            + "display: flex; flex-direction: column; min-width: 0; min-height: 0; "
                            + "background: var(--rz-base-background-color, #ffffff); border: 1px solid var(--rz-border-color, #e0e0e0); "
                            + "border-radius: var(--rz-border-radius, 4px); overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.06);"

                        div {
                            attr.key(ViewComponentId.value id)
                            attr.style itemStyle

                            // Header
                            div {
                                attr.style
                                    "display: flex; align-items: center; gap: 8px; width: 100%; height: 36px; padding: 0 12px; background: var(--rz-base-100, #f8f9fa); border-bottom: 1px solid var(--rz-border-color, #e0e0e0); user-select: none; box-sizing: border-box; flex-shrink: 0;"

                                Rz.icon(icon, style = "font-size: 1.1rem; color: var(--rz-primary); flex-shrink: 0;")

                                span {
                                    attr.style
                                        "font-weight: 600; font-size: 13px; color: var(--rz-text-title-color); white-space: nowrap; overflow: hidden; text-overflow: ellipsis;"

                                    title
                                }
                            }

                            // Body
                            div {
                                attr.style
                                    "flex: 1; min-height: 0; min-width: 0; overflow: auto; display: flex; flex-direction: column;"

                                leaf id
                            }
                        })
                }
            else
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
                            let title, icon = header id

                            comp<RadzenTileLayoutItem> {
                                attr.key(ViewComponentId.value id)
                                "Title" => title
                                "Col" => TilePosition.col pos
                                "Row" => TilePosition.row pos
                                "ColSpan" => TilePosition.colSpan pos
                                "RowSpan" => TilePosition.rowSpan pos

                                attr.fragment
                                    "HeaderTemplate"
                                    (div {
                                        attr.style
                                            "display: flex; align-items: center; gap: 8px; width: 100%; user-select: none;"

                                        Rz.icon(
                                            icon,
                                            style = "font-size: 1.1rem; color: var(--rz-primary); flex-shrink: 0;"
                                        )

                                        span {
                                            attr.style
                                                "font-weight: 600; font-size: 13px; color: var(--rz-text-title-color); white-space: nowrap; overflow: hidden; text-overflow: ellipsis;"

                                            title
                                        }
                                    })

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
                            renderWithHeader hints isEditMode leaf header dispatch onAdapterError tab.Content
                        }))
            }

    /// Backwards-compatible overload using default identifier header.
    let render
        (hints: DashboardRenderHints)
        (isEditMode: bool)
        (leaf: ViewComponentId -> Node)
        (dispatch: ViewSurfaceMsg -> unit)
        (onAdapterError: LayoutAdapterError -> unit)
        (layout: LayoutDefinition)
        : Node =
        let defaultHeader id = (ViewComponentId.value id, "widgets")
        renderWithHeader hints isEditMode leaf defaultHeader dispatch onAdapterError layout

// ============================================================================
// Top-level ViewSurface interpreter.
// ============================================================================

module ViewSurface =

    /// Resolves presentation title and icon for a component occurrence in a view.
    let resolveHeader
        (components: ComponentRegistry)
        (schemas: SchemaRegistry)
        (renderState: ViewRenderState)
        (id: ViewComponentId)
        : string * string =
        match renderState.State.Components |> Map.tryFind id with
        | None -> (ViewComponentId.value id, "widgets")
        | Some vc ->
            let defOpt =
                match ComponentRegistry.find vc.ComponentDefId components with
                | Ok reg -> Some reg.Definition
                | Error _ -> None

            let schemaOpt =
                match SchemaRegistry.find vc.Binding.SchemaDefId schemas with
                | Ok schema -> Some schema
                | Error _ -> None

            let title =
                match vc.Title with
                | Some t when not(System.String.IsNullOrWhiteSpace t) -> t
                | _ ->
                    match schemaOpt with
                    | Some s when not(System.String.IsNullOrWhiteSpace s.DisplayName) -> s.DisplayName
                    | _ ->
                        match defOpt with
                        | Some d when not(System.String.IsNullOrWhiteSpace d.DisplayName) -> d.DisplayName
                        | _ -> ViewComponentId.value id

            let icon =
                match vc.Icon with
                | Some i when not(System.String.IsNullOrWhiteSpace i) -> i
                | _ ->
                    match defOpt with
                    | Some d when not(System.String.IsNullOrWhiteSpace d.Icon) -> d.Icon
                    | _ -> "widgets"

            (title, icon)

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
                let ctx =
                    ComponentResolver.buildContext id vc registration.Definition schema renderState dataStores culture

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
        let header = resolveHeader components schemas renderState
        LayoutInterpreter.renderWithHeader hints isEditMode leaf header dispatch onAdapterError renderState.State.Layout
