module Medhavi.Web.Workspaces.MasterData

open System
open Elmish
open Bolero
open Bolero.Html
open Medhavi.Contracts.Scenario
open Medhavi.Contracts.MasterData
open Medhavi.Web.Panels
open Radzen.Blazor
open Microsoft.AspNetCore.Components
open Medhavi.Web.Controls

type Action = | Refresh

type MasterDataEnv =
    { MasterDataQueries: Medhavi.Web.Stores.MasterDataService }

type Model =
    { Context: PlanningContext
      Uoms: RemoteData<Uom.UnitOfMeasure list>
      Skus: RemoteData<Sku.Sku list>
      Plants: RemoteData<Network.Plant list>
      StockingPoints: RemoteData<Network.StockingPoint list>
      Boms: RemoteData<Bom.Bom list>
      BomSearchText: string
      // track which section is expanded if needed
      ExpandedSection: string option }

type TreeNode =
    { SkuId: string
      SkuName: string
      SkuCode: string
      Quantity: decimal option
      Children: TreeNode list
      IsExpanded: bool }

type Msg =
    | Initialize
    | LoadUoms
    | UomsLoaded of Uom.UnitOfMeasure list
    | LoadSkus
    | SkusLoaded of Result<Sku.Sku list, string>
    | LoadPlants
    | PlantsLoaded of Result<Network.Plant list, string>
    | LoadStockingPoints
    | StockingPointsLoaded of Result<Network.StockingPoint list, string>
    | LoadBoms
    | BomsLoaded of Result<Bom.Bom list, string>
    | UpdateBomSearchText of string
    | LoadNetwork
    | WorkspaceAction of Action

type Output = | Nothing

let init (ctx: PlanningContext) =
    { Context = ctx
      Uoms = RemoteData.NotRequested
      Skus = RemoteData.NotRequested
      Plants = RemoteData.NotRequested
      StockingPoints = RemoteData.NotRequested
      Boms = RemoteData.NotRequested
      BomSearchText = ""
      ExpandedSection = None },
    Cmd.none

let executeAction (action: Action) (model: Model) : Model * Cmd<Msg> = model, Cmd.none

let update (env: MasterDataEnv) (msg: Msg) (model: Model) : Model * Cmd<Msg> * Output option =
    match msg with
    | Initialize ->
        { model with ExpandedSection = None }, Cmd.none, None
    | LoadUoms ->
        let m = { model with ExpandedSection = Some "Uoms" }
        if model.Uoms = RemoteData.NotRequested then
            { m with Uoms = RemoteData.Loading },
            Cmd.OfAsync.either
                (fun () -> async { return! env.MasterDataQueries.UomQueryService.GetAll() |> Async.AwaitTask })
                ()
                UomsLoaded
                (fun ex -> UomsLoaded([])),
            None
        else
            m, Cmd.none, None
    | UomsLoaded items ->
        { model with
            Uoms = RemoteData.Loaded items },
        Cmd.none,
        None
    | LoadSkus ->
        let m = { model with ExpandedSection = Some "Skus" }
        if model.Skus = RemoteData.NotRequested then
            { m with Skus = RemoteData.Loading },
            Cmd.OfAsync.either
                (fun () -> async { return! env.MasterDataQueries.SkuQueryService.GetAll() |> Async.AwaitTask })
                ()
                (fun items -> SkusLoaded(Ok items))
                (fun ex -> SkusLoaded(Error ex.Message)),
            None
        else
            m, Cmd.none, None
    | SkusLoaded result ->
        let updatedData =
            match result with
            | Ok items -> RemoteData.Loaded items
            | Error err -> RemoteData.Failed err
        { model with Skus = updatedData }, Cmd.none, None
    | LoadPlants ->
        let m = { model with ExpandedSection = Some "Network" }
        if model.Plants = RemoteData.NotRequested then
            { m with Plants = RemoteData.Loading },
            Cmd.OfAsync.either
                (fun () -> async { return! env.MasterDataQueries.PlantQueryService.GetAll() |> Async.AwaitTask })
                ()
                (fun items -> PlantsLoaded(Ok items))
                (fun ex -> PlantsLoaded(Error ex.Message)),
            None
        else
            m, Cmd.none, None
    | PlantsLoaded result ->
        let updatedData =
            match result with
            | Ok items -> RemoteData.Loaded items
            | Error err -> RemoteData.Failed err
        { model with Plants = updatedData }, Cmd.none, None
    | LoadStockingPoints ->
        let m = { model with ExpandedSection = Some "Network" }
        if model.StockingPoints = RemoteData.NotRequested then
            { m with StockingPoints = RemoteData.Loading },
            Cmd.OfAsync.either
                (fun () -> async { return! env.MasterDataQueries.StockingPointQueryService.GetAll() |> Async.AwaitTask })
                ()
                (fun items -> StockingPointsLoaded(Ok items))
                (fun ex -> StockingPointsLoaded(Error ex.Message)),
            None
        else
            m, Cmd.none, None
    | StockingPointsLoaded result ->
        let updatedData =
            match result with
            | Ok items -> RemoteData.Loaded items
            | Error err -> RemoteData.Failed err
        { model with StockingPoints = updatedData }, Cmd.none, None
    | LoadBoms ->
        let m = { model with ExpandedSection = Some "Boms" }
        let skusCmd =
            if model.Skus = RemoteData.NotRequested then
                Cmd.ofMsg LoadSkus
            else
                Cmd.none
        let bomsCmd =
            Cmd.OfAsync.either
                (fun () -> async { return! env.MasterDataQueries.BomQueryService.GetAll() |> Async.AwaitTask })
                ()
                (fun items -> BomsLoaded(Ok items))
                (fun ex -> BomsLoaded(Error ex.Message))
        { m with Boms = RemoteData.Loading }, Cmd.batch [ skusCmd; bomsCmd ], None
    | BomsLoaded result ->
        let updatedData =
            match result with
            | Ok items -> RemoteData.Loaded items
            | Error err -> RemoteData.Failed err
        { model with Boms = updatedData }, Cmd.none, None
    | UpdateBomSearchText text ->
        { model with BomSearchText = text }, Cmd.none, None
    | LoadNetwork ->
        let m = { model with ExpandedSection = Some "Network" }
        m, Cmd.batch [ Cmd.ofMsg LoadPlants; Cmd.ofMsg LoadStockingPoints ], None
    | _ -> model, Cmd.none, None

let masterDataFieldset<'Msg>
    (title: string)
    (icon: string)
    (collapsed: bool)
    (summaryText: string)
    (loadMsg: 'Msg)
    (collapseMsg: 'Msg)
    (dispatch: 'Msg -> unit)
    (childContent: Node)
    : Node =
    comp<RadzenFieldset> {
        "AllowCollapse" => true
        "Collapsed" => collapsed
        "Style" => "width: 100%; margin: 0.5rem 0;"

        "Expand" => EventCallback.Factory.Create(obj(), System.Action(fun () -> dispatch loadMsg))
        "Collapse" => EventCallback.Factory.Create(obj(), System.Action(fun () -> dispatch collapseMsg))

        attr.fragment
            "HeaderTemplate"
            (comp<RadzenStack> {
                "Orientation" => Radzen.Orientation.Horizontal
                "Gap" => "0.25rem"
                comp<RadzenIcon> { "Icon" => icon }
                b { text title }
            })

        attr.fragment "ChildContent" childContent

        attr.fragment
            "SummaryTemplate"
            (comp<RadzenCard> {
                "Class" => "rz-mt-4"
                b { text summaryText }
            })
    }

let rec renderTreeNode (node: TreeNode) : Node =
    comp<RadzenTreeItem> {
        "Expanded" => node.IsExpanded
        attr.fragmentWith "Template" (fun (item: RadzenTreeItem) ->
            div {
                attr.style "display: flex; align-items: center; gap: 8px; padding: 4px 0;"
                comp<RadzenIcon> {
                    "Icon" => if node.Children.IsEmpty then "settings" else "account_tree"
                    "Style" => "font-size: 18px; color: var(--rz-text-secondary-color);"
                }
                span {
                    attr.style "font-weight: 500;"
                    text (sprintf "%s (%s)" node.SkuName node.SkuCode)
                }
                match node.Quantity with
                | Some qty ->
                    comp<RadzenBadge> {
                        "BadgeStyle" => Radzen.BadgeStyle.Info
                        "IsPill" => true
                        "Text" => sprintf "Qty: %M" qty
                    }
                | None -> empty()
            }
        )
        for child in node.Children do
            renderTreeNode child
    }

let view (model: Model) (dispatch: Msg -> unit) : Node =
    div {
        attr.style "max-width: 1200px; margin: 0 auto; padding: 1rem;"

        let uomsCollapsed = model.ExpandedSection <> Some "Uoms"
        let uomsSummary =
            match model.Uoms with
            | RemoteData.Loaded items -> sprintf "%d Units of Measure" items.Length
            | RemoteData.Loading -> "Loading..."
            | RemoteData.NotRequested -> "Click to expand and load Units of Measure..."
            | RemoteData.Failed err -> sprintf "Failed to load: %s" err

        let skusCollapsed = model.ExpandedSection <> Some "Skus"
        let skusSummary =
            match model.Skus with
            | RemoteData.Loaded items -> sprintf "%d SKUs" items.Length
            | RemoteData.Loading -> "Loading..."
            | RemoteData.NotRequested -> "Click to expand and load SKUs..."
            | RemoteData.Failed err -> sprintf "Failed to load: %s" err

        let networkCollapsed = model.ExpandedSection <> Some "Network"
        let networkSummary =
            match model.Plants, model.StockingPoints with
            | RemoteData.Loaded p, RemoteData.Loaded s -> sprintf "%d Plants, %d Stocking Points" p.Length s.Length
            | RemoteData.Loading, _
            | _, RemoteData.Loading -> "Loading..."
            | RemoteData.NotRequested, _
            | _, RemoteData.NotRequested -> "Click to expand and load Network..."
            | RemoteData.Failed err, _
            | _, RemoteData.Failed err -> sprintf "Failed to load: %s" err

        let bomsCollapsed = model.ExpandedSection <> Some "Boms"
        let bomsSummary =
            match model.Boms, model.Skus with
            | RemoteData.Loaded boms, RemoteData.Loaded skus -> sprintf "%d Bills of Materials" boms.Length
            | RemoteData.Loading, _
            | _, RemoteData.Loading -> "Loading..."
            | RemoteData.NotRequested, _
            | _, RemoteData.NotRequested -> "Click to expand and load Bills of Materials..."
            | RemoteData.Failed err, _
            | _, RemoteData.Failed err -> sprintf "Failed to load: %s" err

        Rz.stack(
            [ masterDataFieldset
                  "Units of Measure"
                  "scale"
                  uomsCollapsed
                  uomsSummary
                  LoadUoms
                  Initialize
                  dispatch
                  (match model.Uoms with
                   | RemoteData.NotRequested -> empty()
                   | RemoteData.Loading -> empty()
                   | RemoteData.Loaded items ->
                       Rz.dataGrid(
                           items,
                           columns =
                               [ Rz.dataGridColumn<Uom.UnitOfMeasure>(
                                     "IsBase",
                                     "Base?",
                                     width = "40px",
                                     sortable = false,
                                     filterable = false,
                                     template =
                                         fun d ->
                                             if d.IsBase then
                                                 comp<RadzenIcon> { "Icon" => "bookmark_check" }
                                             else
                                                 empty()
                                 )
                                 Rz.dataGridColumn<Uom.UnitOfMeasure>("Code", "Code")
                                 Rz.dataGridColumn<Uom.UnitOfMeasure>("Name", "Name")
                                 Rz.dataGridColumn<Uom.UnitOfMeasure>("Status", "Status")
                                 Rz.dataGridColumn<Uom.UnitOfMeasure>("ConversionFactor", "Factor") ],
                           allowPaging = false,
                           allowSorting = true,
                           allowFiltering = true
                       )
                   | RemoteData.Failed _ -> empty())

              masterDataFieldset
                  "SKUs"
                  "inventory_2"
                  skusCollapsed
                  skusSummary
                  LoadSkus
                  Initialize
                  dispatch
                  (match model.Skus with
                   | RemoteData.NotRequested -> empty()
                   | RemoteData.Loading -> empty()
                   | RemoteData.Loaded items ->
                       Rz.dataGrid(
                           items,
                           columns =
                               [ Rz.dataGridColumn<Sku.Sku>("Code", "Code")
                                 Rz.dataGridColumn<Sku.Sku>("Name", "Name")
                                 Rz.dataGridColumn<Sku.Sku>("Group", "Group")
                                 Rz.dataGridColumn<Sku.Sku>("Status", "Status") ],
                           allowPaging = false,
                           allowSorting = true,
                           allowFiltering = true
                       )
                   | RemoteData.Failed _ -> empty())

              masterDataFieldset
                  "Network"
                  "hub"
                  networkCollapsed
                  networkSummary
                  LoadNetwork
                  Initialize
                  dispatch
                  (Rz.tabs(
                      [ "Plants",
                        [ match model.Plants with
                          | RemoteData.NotRequested -> empty()
                          | RemoteData.Loading -> text "Loading Plants..."
                          | RemoteData.Failed err -> text(sprintf "Failed to load plants: %s" err)
                          | RemoteData.Loaded items ->
                              Rz.dataGrid(
                                  items,
                                  columns =
                                      [ Rz.dataGridColumn<Network.Plant>("Code", "Code")
                                        Rz.dataGridColumn<Network.Plant>("Name", "Name")
                                        Rz.dataGridColumn<Network.Plant>("Status", "Status") ],
                                  allowPaging = false,
                                  allowSorting = true,
                                  allowFiltering = true
                              ) ]
                        "Stocking Points",
                        [ match model.StockingPoints with
                          | RemoteData.NotRequested -> empty()
                          | RemoteData.Loading -> text "Loading Stocking Points..."
                          | RemoteData.Failed err -> text(sprintf "Failed to load stocking points: %s" err)
                          | RemoteData.Loaded items ->
                              Rz.dataGrid(
                                  items,
                                  columns =
                                      [ Rz.dataGridColumn<Network.StockingPoint>("Code", "Code")
                                        Rz.dataGridColumn<Network.StockingPoint>("Name", "Name")
                                        Rz.dataGridColumn<Network.StockingPoint>("Type", "Type")
                                        Rz.dataGridColumn<Network.StockingPoint>("Status", "Status") ],
                                  allowPaging = false,
                                  allowSorting = true,
                                  allowFiltering = true
                              ) ] ]
                  ))

              masterDataFieldset
                  "Bills of Materials"
                  "account_tree"
                  bomsCollapsed
                  bomsSummary
                  LoadBoms
                  Initialize
                  dispatch
                  (match model.Boms, model.Skus with
                   | RemoteData.NotRequested, _ | _, RemoteData.NotRequested -> empty()
                   | RemoteData.Loading, _ | _, RemoteData.Loading -> text "Loading BOM data..."
                   | RemoteData.Failed err, _ -> text(sprintf "Failed to load BOMs: %s" err)
                   | _, RemoteData.Failed err -> text(sprintf "Failed to load SKUs for BOM: %s" err)
                   | RemoteData.Loaded boms, RemoteData.Loaded skus ->
                       let skuMap = skus |> List.map (fun s -> s.Id, s) |> readOnlyDict
                       let bomMap = boms |> List.map (fun b -> b.SkuId, b) |> readOnlyDict

                       let allComponents =
                           boms
                           |> List.collect (fun b -> b.Items |> List.map (fun item -> item.ComponentSkuId))
                           |> Set.ofList

                       let rootSkuIds =
                           boms
                           |> List.map (fun b -> b.SkuId)
                           |> List.filter (fun parentId -> not(allComponents.Contains parentId))

                       let rec buildTree (skuId: string) (qty: decimal option) (visited: Set<string>) : TreeNode option =
                           if visited.Contains skuId then
                               None
                           else
                               let skuOpt = if skuMap.ContainsKey skuId then Some skuMap.[skuId] else None
                               skuOpt
                               |> Option.map (fun sku ->
                                   let children =
                                       if bomMap.ContainsKey skuId then
                                           let bom = bomMap.[skuId]
                                           bom.Items
                                           |> List.choose (fun item ->
                                               buildTree item.ComponentSkuId (Some item.Quantity) (visited.Add skuId))
                                       else
                                           []

                                   { SkuId = sku.Id
                                     SkuName = sku.Name
                                     SkuCode = sku.Code
                                     Quantity = qty
                                     Children = children
                                     IsExpanded = true })

                       let rec filterTree (searchText: string) (node: TreeNode) : TreeNode option =
                           if String.IsNullOrWhiteSpace searchText then
                               Some node
                           else
                               let matchesSelf =
                                   node.SkuName.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                                   || node.SkuCode.Contains(searchText, StringComparison.OrdinalIgnoreCase)

                               let filteredChildren = node.Children |> List.choose(filterTree searchText)

                               if matchesSelf || not filteredChildren.IsEmpty then
                                   Some { node with Children = filteredChildren; IsExpanded = true }
                               else
                                   None

                       let treeNodes =
                           rootSkuIds
                           |> List.choose (fun id -> buildTree id None Set.empty)
                           |> List.choose (filterTree model.BomSearchText)

                       div {
                           Rz.textBox(
                               value = model.BomSearchText,
                               placeholder = "Search SKU name or code...",
                               style = "width: 100%; margin-bottom: 1rem;",
                               valueChanged = (fun s -> dispatch (UpdateBomSearchText s))
                           )

                           if treeNodes.IsEmpty then
                               div {
                                   attr.style "padding: 1rem; text-align: center; color: var(--rz-text-secondary-color);"
                                   text "No matching Bill of Materials found."
                               }
                           else
                               comp<RadzenTree> {
                                   "Style" => "max-height: 400px; width:100%; border: 1px solid var(--rz-border-color); padding: 0.5rem; overflow-y: auto;"
                                   for node in treeNodes do
                                       renderTreeNode node
                               }
                       }) ],
            gap = "1rem"
        )
    }
