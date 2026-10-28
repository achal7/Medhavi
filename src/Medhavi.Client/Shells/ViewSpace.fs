module Medhavi.Client.Shells.ViewSpace

open System
open System.Globalization
open Elmish
open Bolero
open Bolero.Html
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Radzen
open Radzen.Blazor
open Medhavi.Common.Validation
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Context
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Layout
open Medhavi.Client.ViewComposition.View
open Medhavi.Client.ViewComposition.Session
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.Registries
open Medhavi.Client.Rendering
open Medhavi.Client
open Medhavi.Client.Shells // for ViewSurfaceMsg, LayoutAdapterError

// ============================================================================
// ViewSpace semantic integration
// ============================================================================

// Presentation identity (workspace tab)
[<Struct>]
type WorkspaceTabId = private WorkspaceTabId of string

module WorkspaceTabId =
    let create value =
        if String.IsNullOrWhiteSpace value then
            invalidArg (nameof value) "Workspace tab id cannot be empty."

        WorkspaceTabId value

    let value (WorkspaceTabId value) = value

// External request protocol
type Request =
    | OpenViewDefinition of ViewDefinition
    | OpenViewSession of ViewSession
    | AddComponent of ViewComponentId * ComponentBinding

// Host outputs
[<RequireQualifiedAccess>]
type Output =
    | ActionRequested of ViewComponentId * actionId: ActionDefId * targetKey: string option * payload: ActionPayload
    | ParameterizedActionRequested of ViewComponentId * actionId: ActionDefId * targetKey: string option
    | RefreshRequested of ViewComponentId
    | ViewOperationRejected of ViewError list
    | LayoutAdapterError of LayoutAdapterError
    | SelectionChanged of ViewComponentId * selection: string option

// Workspace tab
type WorkspaceTab =
    { Id: WorkspaceTabId
      Session: ViewSession
      IsEditMode: bool }

// Presentation dependencies
type ViewSpaceDependencies =
    { Components: ComponentRegistry
      Schemas: SchemaRegistry
      Culture: CultureInfo
      DashboardHints: DashboardRenderHints
      ContextMenu: ContextMenuService option }

// Model
type Model =
    { Tabs: WorkspaceTab list
      ActiveTabId: WorkspaceTabId option
      NextTabNumber: int
      NextSessionNumber: int
      DataStores: Map<DataStoreId, DataPayload> }

// Messages
type Msg =
    | Request of Request
    | SelectTab of WorkspaceTabId
    | CloseTab of WorkspaceTabId
    | CloseOtherTabs of WorkspaceTabId
    | CloseTabsToRight of WorkspaceTabId
    | CloseTabsToLeft of WorkspaceTabId
    | CloseAllTabs
    | DuplicateTab of WorkspaceTabId
    | RefreshTab of WorkspaceTabId
    | ToggleEditMode
    | SurfaceMessage of ViewSurfaceMsg
    | SurfaceAdapterError of LayoutAdapterError
    | DataStoreLoaded of DataStoreId * DataPayload
    | DataStoreLoadFailed of DataStoreId * string
    | RefreshStores of DataStoreId list

// Deterministic identity generation
let private createWorkspaceTabId number = sprintf "workspace-tab-%03d" number |> WorkspaceTabId.create
let private createViewSessionId number = sprintf "workspace-session-%03d" number |> ViewSessionId.literal

// Initial state
let init () : Model * Cmd<Msg> =
    { Tabs = []
      ActiveTabId = None
      NextTabNumber = 1
      NextSessionNumber = 1
      DataStores = Map.empty },
    Cmd.none

// Pure model helpers
let tryFindTab tabId tabs = tabs |> List.tryFind(fun tab -> tab.Id = tabId)
let activeTab model = model.ActiveTabId |> Option.bind(fun tabId -> tryFindTab tabId model.Tabs)
let updateTab tabId transform tabs = tabs |> List.map(fun tab -> if tab.Id = tabId then transform tab else tab)
let private removeTab tabId tabs = tabs |> List.filter(fun tab -> tab.Id <> tabId)

let private replacementActiveTab closedTabId tabs =
    let index = tabs |> List.tryFindIndex(fun tab -> tab.Id = closedTabId)

    match index with
    | None -> tabs |> List.tryLast |> Option.map(fun tab -> tab.Id)
    | Some closedIndex ->
        let remaining =
            tabs |> List.mapi(fun index tab -> index, tab) |> List.filter(fun (index, _) -> index <> closedIndex)

        match remaining |> List.tryItem(max 0 (closedIndex - 1)) with
        | Some(_, previous) -> Some previous.Id
        | None -> remaining |> List.tryHead |> Option.map(fun (_, tab) -> tab.Id)

// Semantic session helpers
let private validateSession (session: ViewSession) : Result<ViewSession, ViewError list> =
    ViewAlgebra.validate session.State
    |> function
        | Medhavi.Common.Validation.Valid _ -> Ok session
        | Medhavi.Common.Validation.Invalid errors -> Error errors

let private addBindingToSession
    (viewComponentId: ViewComponentId)
    (binding: ComponentBinding)
    (session: ViewSession)
    : Result<ViewSession, ViewError list> =
    let span = binding.Definition.DefaultGridSpan

    ViewAlgebra.addComponent PlacementPolicy.proposePosition span viewComponentId binding session.State
    |> function
        | Medhavi.Common.Validation.Valid state -> Ok { session with State = state }
        | Medhavi.Common.Validation.Invalid errors -> Error errors

// Data fetching helpers
let private fetchStoreCmd (ctx: ApplicationContext) (storeId: DataStoreId) : Cmd<Msg> =
    match ctx.DataStoreRegistry |> Map.tryFind storeId with
    | Some store ->
        Cmd.OfAsync.either
            store.Fetch
            ctx
            (fun payload -> DataStoreLoaded(storeId, payload))
            (fun ex -> DataStoreLoadFailed(storeId, ex.Message))
    | None -> Cmd.none

let private fetchStoresForSession (ctx: ApplicationContext) (session: ViewSession) : Cmd<Msg> =
    session.State.Components
    |> Map.toList
    |> List.map(fun (_, vc) -> vc.Binding.StoreId)
    |> List.distinct
    |> List.map(fetchStoreCmd ctx)
    |> Cmd.batch

// Request handling
let private openSession (tabId: WorkspaceTabId) (session: ViewSession) (model: Model) : Model =
    { model with
        Tabs =
            model.Tabs
            @ [ { Id = tabId
                  Session = session
                  IsEditMode = false } ]
        ActiveTabId = Some tabId }

let private handleOpenViewSession (ctx: ApplicationContext) (session: ViewSession) (model: Model) : Model * Cmd<Msg> * Output option =
    match validateSession session with
    | Error errors -> model, Cmd.none, Some(Output.ViewOperationRejected errors)
    | Ok validSession ->
        match model.Tabs |> List.tryFind(fun tab -> tab.Session.SessionId = validSession.SessionId) with
        | Some existing ->
            { model with
                ActiveTabId = Some existing.Id },
            fetchStoresForSession ctx validSession,
            None
        | None ->
            let tabId = createWorkspaceTabId model.NextTabNumber

            (openSession tabId validSession model
             |> fun updated ->
                 { updated with
                     NextTabNumber = model.NextTabNumber + 1 }),
            fetchStoresForSession ctx validSession,
            None

let private handleOpenViewDefinition (ctx: ApplicationContext) (definition: ViewDefinition) (model: Model) : Model * Cmd<Msg> * Output option =
    let sessionId = createViewSessionId model.NextSessionNumber
    let session = ViewLifecycle.openSession sessionId definition

    match validateSession session with
    | Error errors -> model, Cmd.none, Some(Output.ViewOperationRejected errors)
    | Ok validSession ->
        let tabId = createWorkspaceTabId model.NextTabNumber

        (openSession tabId validSession model
         |> fun updated ->
             { updated with
                 NextTabNumber = model.NextTabNumber + 1
                 NextSessionNumber = model.NextSessionNumber + 1 }),
        fetchStoresForSession ctx validSession,
        None

let private handleAddComponent
    (ctx: ApplicationContext)
    (viewComponentId: ViewComponentId)
    (binding: ComponentBinding)
    (model: Model)
    : Model * Cmd<Msg> * Output option =
    match activeTab model with
    | Some tab ->
        match addBindingToSession viewComponentId binding tab.Session with
        | Error errors -> model, Cmd.none, Some(Output.ViewOperationRejected errors)
        | Ok session ->
            { model with
                Tabs = updateTab tab.Id (fun current -> { current with Session = session }) model.Tabs },
            fetchStoreCmd ctx binding.Store,
            None
    | None ->
        let sessionId = createViewSessionId model.NextSessionNumber
        let title = binding.Definition.DisplayName
        let session = TransientViewPolicy.createComponentWorkspace sessionId title GridDimensions.defaultDimensions

        match addBindingToSession viewComponentId binding session with
        | Error errors -> model, Cmd.none, Some(Output.ViewOperationRejected errors)
        | Ok updatedSession ->
            let tabId = createWorkspaceTabId model.NextTabNumber

            let updatedModel =
                { model with
                    Tabs =
                        model.Tabs
                        @ [ { Id = tabId
                              Session = updatedSession
                              IsEditMode = false } ]
                    ActiveTabId = Some tabId
                    NextTabNumber = model.NextTabNumber + 1
                    NextSessionNumber = model.NextSessionNumber + 1 }

            updatedModel, fetchStoreCmd ctx binding.Store, None

// Surface-event handling
let private updateSelection
    (viewComponentId: ViewComponentId)
    (selection: string option)
    (model: Model)
    : Model * Cmd<Msg> * Output option =
    match activeTab model with
    | None ->
        model, Cmd.none, None
    | Some tab ->
        let updatedSelections = tab.Session.Selections |> Map.add viewComponentId selection

        let session =
            { tab.Session with
                Selections = updatedSelections }

        { model with
            Tabs = updateTab tab.Id (fun current -> { current with Session = session }) model.Tabs },
        Cmd.none,
        Some(Output.SelectionChanged(viewComponentId, selection))

let private updateConfiguration
    (viewComponentId: ViewComponentId)
    (configuration: ComponentConfiguration)
    (model: Model)
    : Model * Cmd<Msg> * Output option =
    match activeTab model with
    | None -> model, Cmd.none, None
    | Some tab ->
        let operation = ViewAlgebra.updateConfiguration viewComponentId configuration

        ViewLifecycle.applyOperation operation tab.Session
        |> function
            | Valid session ->
                { model with
                    Tabs = updateTab tab.Id (fun current -> { current with Session = session }) model.Tabs },
                Cmd.none,
                None
            | Invalid errors -> model, Cmd.none, Some(Output.ViewOperationRejected errors)

let private adjustPlacement
    (viewComponentId: ViewComponentId)
    (position: TilePosition)
    (model: Model)
    : Model * Cmd<Msg> * Output option =
    match activeTab model with
    | None -> model, Cmd.none, None
    | Some tab ->
        let operation = ViewAlgebra.adjustDashboardPlacement viewComponentId position

        ViewLifecycle.applyOperation operation tab.Session
        |> function
            | Valid session ->
                { model with
                    Tabs = updateTab tab.Id (fun current -> { current with Session = session }) model.Tabs },
                Cmd.none,
                None
            | Invalid errors -> model, Cmd.none, Some(Output.ViewOperationRejected errors)

let private applySortChange
    (viewComponentId: ViewComponentId)
    (sortOpt: SortCriterion option)
    (model: Model)
    : Model * Cmd<Msg> * Output option =
    match activeTab model with
    | None -> model, Cmd.none, None
    | Some tab ->
        match tab.Session.State.Components |> Map.tryFind viewComponentId with
        | None -> model, Cmd.none, None
        | Some vc ->
            let baseParams =
                vc.Configuration.Parameters
                |> Map.remove "SortField"
                |> Map.remove "SortDirection"

            let newParams =
                match sortOpt with
                | Some s ->
                    baseParams
                    |> Map.add "SortField" (StringValue s.Field)
                    |> Map.add "SortDirection" (StringValue(if s.Direction = Ascending then "Ascending" else "Descending"))
                | None -> baseParams

            let newConfig =
                { vc.Configuration with
                    Parameters = newParams }

            let operation = ViewAlgebra.updateConfiguration viewComponentId newConfig

            match ViewLifecycle.applyOperation operation tab.Session with
            | Valid session ->
                { model with
                    Tabs = updateTab tab.Id (fun current -> { current with Session = session }) model.Tabs },
                Cmd.none,
                None
            | Invalid errors -> model, Cmd.none, Some(Output.ViewOperationRejected errors)

let private applyFilterChange
    (viewComponentId: ViewComponentId)
    (columnId: string)
    (filterOpt: FilterCriterion option)
    (model: Model)
    : Model * Cmd<Msg> * Output option =
    match activeTab model with
    | None -> model, Cmd.none, None
    | Some tab ->
        match tab.Session.State.Components |> Map.tryFind viewComponentId with
        | None -> model, Cmd.none, None
        | Some vc ->
            let otherFilters = vc.Configuration.LocalFilters |> List.filter(fun f -> f.Field <> columnId)

            let newFilters =
                match filterOpt with
                | Some f -> f :: otherFilters
                | None -> otherFilters

            let newConfig =
                { vc.Configuration with
                    LocalFilters = newFilters }

            let operation = ViewAlgebra.updateConfiguration viewComponentId newConfig

            match ViewLifecycle.applyOperation operation tab.Session with
            | Valid session ->
                { model with
                    Tabs = updateTab tab.Id (fun current -> { current with Session = session }) model.Tabs },
                Cmd.none,
                None
            | Invalid errors -> model, Cmd.none, Some(Output.ViewOperationRejected errors)

let private handleSurfaceMessage (ctx: ApplicationContext) (message: ViewSurfaceMsg) (model: Model) : Model * Cmd<Msg> * Output option =
    match message with
    | ComponentEvent(viewComponentId, SelectionChanged selection) -> updateSelection viewComponentId selection model
    | ComponentEvent(viewComponentId, ConfigurationChangeRequested configuration) ->
        updateConfiguration viewComponentId configuration model
    | ComponentEvent(viewComponentId, SortChanged sortOpt) -> applySortChange viewComponentId sortOpt model
    | ComponentEvent(viewComponentId, FilterChanged(columnId, filterOpt)) ->
        applyFilterChange viewComponentId columnId filterOpt model
    | ComponentEvent(viewComponentId, ActionRequested(actionId, targetKey, payload)) ->
        model, Cmd.none, Some(Output.ActionRequested(viewComponentId, actionId, targetKey, payload))
    | ComponentEvent(viewComponentId, ParameterizedActionRequested(actionId, targetKey)) ->
        model, Cmd.none, Some(Output.ActionRequested(viewComponentId, actionId, targetKey, NoPayload)) // Host handles dialog
    | ComponentEvent(viewComponentId, RefreshRequested) ->
        let cmd =
            match activeTab model with
            | Some tab ->
                match tab.Session.State.Components |> Map.tryFind viewComponentId with
                | Some vc -> fetchStoreCmd ctx vc.Binding.StoreId
                | None -> Cmd.none
            | None -> Cmd.none

        model, cmd, Some(Output.RefreshRequested viewComponentId)
    | PlacementAdjustRequested(viewComponentId, position) -> adjustPlacement viewComponentId position model
    | ComponentEvent(_, _) -> failwith "Not Implemented"

let update (ctx: ApplicationContext) (msg: Msg) (model: Model) : Model * Cmd<Msg> * Output option =
    match msg with
    | Request(OpenViewDefinition definition) -> handleOpenViewDefinition ctx definition model
    | Request(OpenViewSession session) -> handleOpenViewSession ctx session model
    | Request(AddComponent(viewComponentId, binding)) -> handleAddComponent ctx viewComponentId binding model
    | SelectTab tabId ->
        if model.Tabs |> List.exists(fun tab -> tab.Id = tabId) then
            let cmd =
                match tryFindTab tabId model.Tabs with
                | Some tab -> fetchStoresForSession ctx tab.Session
                | None -> Cmd.none

            { model with ActiveTabId = Some tabId }, cmd, None
        else
            model, Cmd.none, None
    | CloseTab tabId ->
        let remaining = removeTab tabId model.Tabs

        let newActive =
            match model.ActiveTabId with
            | Some active when active <> tabId -> Some active
            | _ ->
                replacementActiveTab tabId model.Tabs
                |> Option.bind(fun candidate ->
                    remaining |> List.tryFind(fun tab -> tab.Id = candidate) |> Option.map(fun tab -> tab.Id))

        let cmd =
            match newActive with
            | Some activeId when model.ActiveTabId <> Some activeId ->
                match tryFindTab activeId remaining with
                | Some tab -> fetchStoresForSession ctx tab.Session
                | None -> Cmd.none
            | _ -> Cmd.none

        { model with
            Tabs = remaining
            ActiveTabId = newActive },
        cmd,
        None
    | CloseOtherTabs tabId ->
        let remaining = model.Tabs |> List.filter(fun tab -> tab.Id = tabId)
        let newActive = remaining |> List.tryHead |> Option.map(fun tab -> tab.Id)
        let cmd =
            match newActive with
            | Some activeId when model.ActiveTabId <> Some activeId ->
                match tryFindTab activeId remaining with
                | Some tab -> fetchStoresForSession ctx tab.Session
                | None -> Cmd.none
            | _ -> Cmd.none

        { model with
            Tabs = remaining
            ActiveTabId = newActive },
        cmd,
        None
    | CloseTabsToRight tabId ->
        let remaining =
            match model.Tabs |> List.tryFindIndex(fun tab -> tab.Id = tabId) with
            | Some idx -> model.Tabs |> List.take(idx + 1)
            | None -> model.Tabs

        let newActive =
            match model.ActiveTabId with
            | Some active when remaining |> List.exists(fun tab -> tab.Id = active) -> Some active
            | _ -> Some tabId

        let cmd =
            match newActive with
            | Some activeId when model.ActiveTabId <> Some activeId ->
                match tryFindTab activeId remaining with
                | Some tab -> fetchStoresForSession ctx tab.Session
                | None -> Cmd.none
            | _ -> Cmd.none

        { model with
            Tabs = remaining
            ActiveTabId = newActive },
        cmd,
        None
    | CloseTabsToLeft tabId ->
        let remaining =
            match model.Tabs |> List.tryFindIndex(fun tab -> tab.Id = tabId) with
            | Some idx -> model.Tabs |> List.skip idx
            | None -> model.Tabs

        let newActive =
            match model.ActiveTabId with
            | Some active when remaining |> List.exists(fun tab -> tab.Id = active) -> Some active
            | _ -> Some tabId

        let cmd =
            match newActive with
            | Some activeId when model.ActiveTabId <> Some activeId ->
                match tryFindTab activeId remaining with
                | Some tab -> fetchStoresForSession ctx tab.Session
                | None -> Cmd.none
            | _ -> Cmd.none

        { model with
            Tabs = remaining
            ActiveTabId = newActive },
        cmd,
        None
    | CloseAllTabs ->
        { model with
            Tabs = []
            ActiveTabId = None },
        Cmd.none,
        None
    | DuplicateTab tabId ->
        match tryFindTab tabId model.Tabs with
        | None -> model, Cmd.none, None
        | Some tab ->
            let newTabId = createWorkspaceTabId model.NextTabNumber
            let newSessionId = createViewSessionId model.NextSessionNumber
            let newTitle = $"{tab.Session.State.Title} (Copy)"
            let duplicatedState = { tab.Session.State with Title = newTitle }
            let duplicatedSession =
                { tab.Session with
                    SessionId = newSessionId
                    State = duplicatedState }
            let newTab =
                { Id = newTabId
                  Session = duplicatedSession
                  IsEditMode = tab.IsEditMode }

            let updatedTabs =
                let rec insertAfter acc = function
                    | [] -> List.rev (newTab :: acc)
                    | (head: WorkspaceTab) :: tail when head.Id = tabId ->
                        List.rev (head :: acc) @ (newTab :: tail)
                    | head :: tail -> insertAfter (head :: acc) tail
                insertAfter [] model.Tabs

            let updatedModel =
                { model with
                    Tabs = updatedTabs
                    ActiveTabId = Some newTabId
                    NextTabNumber = model.NextTabNumber + 1
                    NextSessionNumber = model.NextSessionNumber + 1 }

            updatedModel, fetchStoresForSession ctx duplicatedSession, None
    | RefreshTab tabId ->
        let cmd =
            match tryFindTab tabId model.Tabs with
            | Some tab -> fetchStoresForSession ctx tab.Session
            | None -> Cmd.none

        model, cmd, None
    | ToggleEditMode ->
        match activeTab model with
        | None -> model, Cmd.none, None
        | Some tab ->
            if LayoutAlgebra.canEdit tab.Session.State.Layout then
                { model with
                    Tabs =
                        updateTab
                            tab.Id
                            (fun current ->
                                { current with
                                    IsEditMode = not current.IsEditMode })
                            model.Tabs },
                Cmd.none,
                None
            else
                model, Cmd.none, None
    | SurfaceMessage message -> handleSurfaceMessage ctx message model
    | SurfaceAdapterError error -> model, Cmd.none, Some(Output.LayoutAdapterError error)
    | DataStoreLoaded(storeId, payload) ->
        { model with DataStores = Map.add storeId payload model.DataStores }, Cmd.none, None
    | DataStoreLoadFailed(_storeId, _error) ->
        model, Cmd.none, None
    | RefreshStores storeIds ->
        let cmds =
            storeIds
            |> List.distinct
            |> List.map (fetchStoreCmd ctx)
            |> Cmd.batch

        model, cmds, None

// Context menu helper leveraging Radzen ContextMenuService
let private openTabContextMenu
    (contextMenuService: ContextMenuService)
    (tab: WorkspaceTab)
    (model: Model)
    (dispatch: Msg -> unit)
    (args: MouseEventArgs)
    : unit =
    let tabs = model.Tabs
    let targetIndex = tabs |> List.tryFindIndex(fun t -> t.Id = tab.Id)
    let totalTabs = tabs.Length

    let canCloseOthers = totalTabs > 1
    let canCloseRight =
        match targetIndex with
        | Some idx -> idx < totalTabs - 1
        | None -> false
    let canCloseLeft =
        match targetIndex with
        | Some idx -> idx > 0
        | None -> false
    let canCloseAll = totalTabs > 0

    let items = ResizeArray<ContextMenuItem>()

    let addItem text icon disabled (msg: Msg) =
        let item = ContextMenuItem()
        item.Text <- text
        item.Icon <- icon
        item.Disabled <- disabled
        item.Value <- box msg
        items.Add item

    addItem "Close" "close" false (CloseTab tab.Id)
    addItem "Close Other Tabs" "tab_unselected" (not canCloseOthers) (CloseOtherTabs tab.Id)
    addItem "Close Tabs to the Right" "arrow_forward" (not canCloseRight) (CloseTabsToRight tab.Id)
    addItem "Close Tabs to the Left" "arrow_back" (not canCloseLeft) (CloseTabsToLeft tab.Id)
    addItem "Close All Tabs" "delete_sweep" (not canCloseAll) CloseAllTabs
    addItem "Duplicate View" "content_copy" false (DuplicateTab tab.Id)
    addItem "Refresh View" "refresh" false (RefreshTab tab.Id)

    contextMenuService.Open(
        args,
        items,
        Action<MenuItemEventArgs>(fun e ->
            contextMenuService.Close()
            match box e with | null -> () | _ -> match e.Value with | :? Msg as msg -> dispatch msg | _ -> ())
    )

// View
let private renderTab (model: Model) (dependencies: ViewSpaceDependencies) (tab: WorkspaceTab) (dispatch: Msg -> unit) : Node =
    let tabId = WorkspaceTabId.value tab.Id
    let isActive = model.ActiveTabId = Some tab.Id

    div {
        attr.style(
            if isActive then
                """
                display: flex;
                align-items: center;
                gap: 6px;
                padding: 8px 12px;
                border-bottom: 2px solid var(--rz-primary);
                cursor: pointer;
                user-select: none;
                """
            else
                """
                display: flex;
                align-items: center;
                gap: 6px;
                padding: 8px 12px;
                border-bottom: 2px solid transparent;
                cursor: pointer;
                user-select: none;
                """
        )

        attr.callback "onclick" (fun (_: MouseEventArgs) -> dispatch(SelectTab tab.Id))
        on.preventDefault "contextmenu" true
        on.stopPropagation "contextmenu" true
        attr.callback "oncontextmenu" (fun (e: MouseEventArgs) ->
            match dependencies.ContextMenu with
            | Some cms -> openTabContextMenu cms tab model dispatch e
            | None -> ())

        span {
            attr.style "white-space: nowrap;"
            tab.Session.State.Title
        }

        comp<RadzenButton> {
            "Icon" => "close"
            "ButtonStyle" => ButtonStyle.Light
            "Size" => ButtonSize.Small
            "Variant" => Variant.Text
            attr.callback "Click" (fun (_: MouseEventArgs) -> dispatch(CloseTab tab.Id))
        }
    }

let private renderActiveWorkspace (model: Model) (dependencies: ViewSpaceDependencies) (dispatch: Msg -> unit) : Node =
    match activeTab model with
    | None ->
        div {
            attr.style
                """
                width: 100%;
                height: 100%;
                display: grid;
                place-items: center;
                color: var(--rz-text-secondary-color);
            """

            text "No view is open."
        }
    | Some tab ->
        let renderState = ViewRenderState.ofSession tab.Session

        ViewSurface.render
            dependencies.Components
            dependencies.Schemas
            model.DataStores
            renderState
            dependencies.Culture
            dependencies.DashboardHints
            tab.IsEditMode
            (SurfaceMessage >> dispatch)
            (SurfaceAdapterError >> dispatch)

let view (model: Model) (dependencies: ViewSpaceDependencies) (dispatch: Msg -> unit) : Node =
    div {
        attr.style
            """
            width: 100%;
            height: 100%;
            display: flex;
            flex-direction: column;
            min-height: 0;
        """

        div {
            attr.style
                """
                display: flex;
                align-items: stretch;
                overflow-x: auto;
                border-bottom: 1px solid var(--rz-border-color);
                flex-shrink: 0;
            """
            on.preventDefault "contextmenu" true
            on.stopPropagation "contextmenu" true

            forEach model.Tabs (fun tab -> renderTab model dependencies tab dispatch)
        }

        div {
            attr.style
                """
                flex: 1;
                min-width: 0;
                min-height: 0;
                padding: 8px;
                overflow: hidden;
            """

            renderActiveWorkspace model dependencies dispatch
        }
    }

type Component() =
    inherit ElmishComponent<Model, Msg>()

    [<Inject>]
    member val ContextMenuService: ContextMenuService = Unchecked.defaultof<ContextMenuService> with get, set

    [<Parameter>]
    member val Dependencies: ViewSpaceDependencies option = None with get, set

    override this.View model dispatch =
        match this.Dependencies with
        | Some dependencies ->
            let deps =
                match dependencies.ContextMenu with
                | Some _ -> dependencies
                | None ->
                    if isNull(box this.ContextMenuService) then dependencies
                    else { dependencies with ContextMenu = Some this.ContextMenuService }
            view model deps dispatch
        | None ->
            div {
                attr.``class`` "viewspace-configuration-error"
                text "ViewSpace presentation dependencies are not configured."
            }

