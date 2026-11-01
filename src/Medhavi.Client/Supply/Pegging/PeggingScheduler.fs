module Medhavi.Client.Supply.Pegging.PeggingScheduler

open System
open System.Collections.Generic
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Medhavi.Client
open Medhavi.Client.Registries
open Medhavi.Client.Rendering
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ViewComposition.Context

// ----------------------------------------------------------------------------
// Backward-compatible core types and descriptors
// ----------------------------------------------------------------------------

type PeggingItemType =
    | Demand
    | PlannedSupply

type PeggingAppointment =
    {
        Id: string
        Text: string
        Start: DateTimeOffset
        End: DateTimeOffset
        ItemType: PeggingItemType
        Quantity: decimal
        EntityRef: string
        Status: string
    }

module PeggingSchedulerActions =

    let realignPegging: ActionDescriptor =
        {
            Id = ActionDefId.literal "pegging.realign"
            DisplayName = "Realign Pegging Horizon"
            Icon = "sync"
            Scope = GlobalScope
            ExecutionMode = Direct
            RequiredRoles = [ "Planner" ]
            IsPrimary = true
        }

    let all: ActionDescriptor list = [ realignPegging ]

let realignPegging = PeggingSchedulerActions.realignPegging
let allActions = PeggingSchedulerActions.all

let storeId = DataStoreId.literal "pegging-scheduler-store"
let schemaId = SchemaDefId.literal "schema-pegging-scheduler"
let definitionId = ComponentDefId.literal "comp-pegging-scheduler"

let schema: SchemaDefinition =
    {
        Id = schemaId
        DisplayName = "Demand-Supply Pegging Horizon & Matrix"
        Capabilities = [ "TimeSeries"; "Tabular"; "Hierarchical" ]
        SupportedActions = PeggingSchedulerActions.all |> List.map (fun a -> a.Id)
        Actions = PeggingSchedulerActions.all
        Columns =
            [
                {
                    ColumnId = "Id"
                    PropertyName = Some "Id"
                    Title = "Order ID"
                    DefaultOrder = 0
                    DefaultWidthInPixels = Some 140
                    DefaultVisible = true
                    DefaultPinned = true
                    Sortable = true
                    FilterValueKind = Some Text
                    Filterable = true
                    Groupable = false
                    RenderHint = None
                    Alignment = Left
                    CellType = TextType
                    IsKey = true
                    FormatString = None
                    UserConfigurable = false
                }
                {
                    ColumnId = "EntityRef"
                    PropertyName = Some "EntityRef"
                    Title = "Product / SKU"
                    DefaultOrder = 1
                    DefaultWidthInPixels = Some 150
                    DefaultVisible = true
                    DefaultPinned = false
                    Sortable = true
                    FilterValueKind = Some Text
                    Filterable = true
                    Groupable = true
                    RenderHint = None
                    Alignment = Left
                    CellType = TextType
                    IsKey = false
                    FormatString = None
                    UserConfigurable = true
                }
                {
                    ColumnId = "Location"
                    PropertyName = Some "Location"
                    Title = "Plant / Site"
                    DefaultOrder = 2
                    DefaultWidthInPixels = Some 140
                    DefaultVisible = true
                    DefaultPinned = false
                    Sortable = true
                    FilterValueKind = Some Text
                    Filterable = true
                    Groupable = true
                    RenderHint = None
                    Alignment = Left
                    CellType = TextType
                    IsKey = false
                    FormatString = None
                    UserConfigurable = true
                }
                {
                    ColumnId = "ProductGroup"
                    PropertyName = Some "ProductGroup"
                    Title = "Product Group"
                    DefaultOrder = 3
                    DefaultWidthInPixels = Some 160
                    DefaultVisible = true
                    DefaultPinned = false
                    Sortable = true
                    FilterValueKind = Some Text
                    Filterable = true
                    Groupable = true
                    RenderHint = None
                    Alignment = Left
                    CellType = TextType
                    IsKey = false
                    FormatString = None
                    UserConfigurable = true
                }
                {
                    ColumnId = "ItemType"
                    PropertyName = Some "ItemType"
                    Title = "Stream Type"
                    DefaultOrder = 4
                    DefaultWidthInPixels = Some 120
                    DefaultVisible = true
                    DefaultPinned = false
                    Sortable = true
                    FilterValueKind = Some Text
                    Filterable = true
                    Groupable = true
                    RenderHint = Some "StatusBadge"
                    Alignment = Left
                    CellType = TextType
                    IsKey = false
                    FormatString = None
                    UserConfigurable = true
                }
                {
                    ColumnId = "Start"
                    PropertyName = Some "Start"
                    Title = "Date"
                    DefaultOrder = 5
                    DefaultWidthInPixels = Some 130
                    DefaultVisible = true
                    DefaultPinned = false
                    Sortable = true
                    FilterValueKind = Some FilterValueKind.DateTimeOffset
                    Filterable = true
                    Groupable = false
                    RenderHint = None
                    Alignment = Left
                    CellType = DateTimeType
                    IsKey = false
                    FormatString = Some "yyyy-MM-dd"
                    UserConfigurable = true
                }
                {
                    ColumnId = "Quantity"
                    PropertyName = Some "Quantity"
                    Title = "Quantity"
                    DefaultOrder = 6
                    DefaultWidthInPixels = Some 110
                    DefaultVisible = true
                    DefaultPinned = false
                    Sortable = true
                    FilterValueKind = Some Number
                    Filterable = true
                    Groupable = false
                    RenderHint = None
                    Alignment = Right
                    CellType = NumberType
                    IsKey = false
                    FormatString = Some "N0"
                    UserConfigurable = true
                }
                {
                    ColumnId = "Status"
                    PropertyName = Some "Status"
                    Title = "Status"
                    DefaultOrder = 7
                    DefaultWidthInPixels = Some 110
                    DefaultVisible = true
                    DefaultPinned = false
                    Sortable = true
                    FilterValueKind = Some Text
                    Filterable = true
                    Groupable = true
                    RenderHint = Some "StatusBadge"
                    Alignment = Left
                    CellType = TextType
                    IsKey = false
                    FormatString = None
                    UserConfigurable = true
                }
            ]
        DefaultSort = None
        DefaultFilters = []
    }

let definition: SchemaDefinition = schema

let createRowEnriched
    (id: string)
    (text: string)
    (startDt: DateTimeOffset)
    (endDt: DateTimeOffset)
    (itemType: string)
    (quantity: decimal)
    (entityRef: string)
    (status: string)
    (locationOpt: string option)
    (productGroupOpt: string option)
    (supplyTypeOpt: string option)
    : UniversalRow =
    let baseCells =
        [
            "Id", TextValue id
            "Text", TextValue text
            "Start", DateTimeValue startDt
            "End", DateTimeValue endDt
            "ItemType", TextValue itemType
            "Quantity", NumberValue quantity
            "EntityRef", TextValue entityRef
            "Status", TextValue status
        ]

    let locationCell =
        locationOpt |> Option.map (fun loc -> "Location", TextValue loc) |> Option.toList

    let groupCell =
        productGroupOpt |> Option.map (fun grp -> "ProductGroup", TextValue grp) |> Option.toList

    let supplyTypeCell =
        supplyTypeOpt |> Option.map (fun st -> "SupplyType", TextValue st) |> Option.toList

    {
        Key = id
        Cells = Map.ofList (baseCells @ locationCell @ groupCell @ supplyTypeCell)
    }

let createRow
    (id: string)
    (text: string)
    (startDt: DateTimeOffset)
    (endDt: DateTimeOffset)
    (itemType: string)
    (quantity: decimal)
    (entityRef: string)
    (status: string)
    : UniversalRow =
    createRowEnriched id text startDt endDt itemType quantity entityRef status None None None

let adapter: ProjectionAdapter<PeggingAppointment> =
    {
        ToUniversal =
            fun (appt: PeggingAppointment) ->
                let itemTypeStr =
                    match appt.ItemType with
                    | Demand -> "Demand"
                    | PlannedSupply -> "PlannedSupply"

                createRow appt.Id appt.Text appt.Start appt.End itemTypeStr appt.Quantity appt.EntityRef appt.Status
    }

let componentDefinition: ComponentDefinition =
    {
        Id = definitionId
        DisplayName = "Demand-Supply Pegging & Time-Phased Matrix"
        Icon = "grid_view"
        Category = ComponentCategory.Visualization
        RequiredSchemaCapabilities = [ "TimeSeries" ]
        DefaultGridSpan = GridSpan.literal 12 7
        DefaultSchemaId = Some schemaId
        DefaultStoreId = Some storeId
        IsResizable = true
        IsDraggable = true
        SelectionMode = SelectionMode.SingleSelection
    }

// ----------------------------------------------------------------------------
// Domain & ViewModel Architecture (Pure Functional DDD & Elmish)
// ----------------------------------------------------------------------------

module ViewModel =

    /// Supply stream classification
    type SupplyStream =
        | PurchaseOrder of Vendor: string
        | ProductionOrder of WorkCenter: string
        | InventoryOnHand
        | TransferOrder of SourcePlant: string

    /// Concrete supply record available on a specific date
    type SupplyRecord =
        {
            SupplyId: string
            Stream: SupplyStream
            Quantity: decimal
            ItemRef: string
            Plant: string
            ProductGroup: string
            AvailableDate: DateTimeOffset
            Status: string
        }

    /// Concrete demand requirement needed on a specific date
    type DemandRecord =
        {
            DemandId: string
            Customer: string option
            Quantity: decimal
            ItemRef: string
            Plant: string
            ProductGroup: string
            RequiredDate: DateTimeOffset
            Status: string
        }

    /// Daily Gantt-cell containing demands, supplies, and net coverage metrics
    type DailyGanttCell =
        {
            Date: DateTime
            TotalDemand: decimal
            TotalSupply: decimal
            Supplies: SupplyRecord list
            Demands: DemandRecord list
            ProjectedOnHand: decimal
        }
        with
            member this.NetBalance = this.TotalSupply - this.TotalDemand
            member this.HasShortage = this.NetBalance < 0m
            member this.CoverageRatio =
                if this.TotalDemand <= 0m then 1.0m
                else Math.Min(1.0m, Math.Max(0.0m, this.TotalSupply / this.TotalDemand))

            member this.PoQuantity =
                this.Supplies
                |> List.sumBy (fun s -> match s.Stream with PurchaseOrder _ -> s.Quantity | _ -> 0m)

            member this.WoQuantity =
                this.Supplies
                |> List.sumBy (fun s -> match s.Stream with ProductionOrder _ -> s.Quantity | _ -> 0m)

            member this.StockQuantity =
                this.Supplies
                |> List.sumBy (fun s -> match s.Stream with InventoryOnHand -> s.Quantity | _ -> 0m)

            member this.TransferQuantity =
                this.Supplies
                |> List.sumBy (fun s -> match s.Stream with TransferOrder _ -> s.Quantity | _ -> 0m)

    /// Supply chain hierarchy nodes: Plant -> Product Group -> Product / SKU
    type PlantInfo = { Id: string; Name: string }
    type ProductGroupInfo = { Id: string; Name: string; PlantId: string }
    type ProductSkuInfo = { SkuId: string; Name: string; GroupId: string; PlantId: string; Uom: string; SafetyStock: decimal }

    type HierarchyNode =
        | PlantNode of PlantInfo
        | ProductGroupNode of ProductGroupInfo
        | ProductSkuNode of ProductSkuInfo

    /// Hierarchical row for the matrix grid
    type PeggingMatrixRow =
        {
            RowId: string
            Node: HierarchyNode
            ParentId: string option
            Depth: int
            IsExpanded: bool
            HasChildren: bool
            TotalDemand: decimal
            TotalSupply: decimal
            NetBalance: decimal
            DailyCells: Map<DateTime, DailyGanttCell>
        }

    /// Cell selection state for detailed pegging inspection
    type SelectedCellDetail =
        {
            RowId: string
            NodeTitle: string
            Date: DateTime
            Cell: DailyGanttCell
        }

    /// Elmish State Model
    type Model =
        {
            HorizonDays: int
            StartDate: DateTime
            ExpandedRowIds: Set<string>
            SelectedCell: SelectedCellDetail option
            SelectedPlantFilter: string option
            SearchQuery: string
            ShowOnlyShortages: bool
        }

    /// Elmish Messages
    type Msg =
        | ToggleRowExpand of rowId: string
        | ExpandAll
        | CollapseAll
        | SelectCell of detail: SelectedCellDetail
        | ClearCellSelection
        | SetHorizonDays of int
        | SetPlantFilter of string option
        | SetSearchQuery of string
        | ToggleShowOnlyShortages
        | RealignPeggingRequested
        | RefreshRequested

    let init (startDate: DateTime) : Model =
        {
            HorizonDays = 14
            StartDate = startDate.Date
            ExpandedRowIds = Set.empty
            SelectedCell = None
            SelectedPlantFilter = None
            SearchQuery = ""
            ShowOnlyShortages = false
        }

    let update (msg: Msg) (model: Model) : Model =
        match msg with
        | ToggleRowExpand rowId ->
            let expanded =
                if Set.contains rowId model.ExpandedRowIds then
                    Set.remove rowId model.ExpandedRowIds
                else
                    Set.add rowId model.ExpandedRowIds
            { model with ExpandedRowIds = expanded }

        | ExpandAll ->
            model

        | CollapseAll ->
            { model with ExpandedRowIds = Set.empty }

        | SelectCell detail ->
            { model with SelectedCell = Some detail }

        | ClearCellSelection ->
            { model with SelectedCell = None }

        | SetHorizonDays days ->
            { model with HorizonDays = Math.Max(7, Math.Min(60, days)) }

        | SetPlantFilter plantOpt ->
            { model with SelectedPlantFilter = plantOpt }

        | SetSearchQuery query ->
            { model with SearchQuery = query }

        | ToggleShowOnlyShortages ->
            { model with ShowOnlyShortages = not model.ShowOnlyShortages }

        | RealignPeggingRequested
        | RefreshRequested ->
            model

    // ------------------------------------------------------------------------
    // Pure Data Projections & Aggregations
    // ------------------------------------------------------------------------

    let private inferPlant (cells: Map<string, CellValue>) (entityRef: string) : string =
        match Map.tryFind "Location" cells with
        | Some (TextValue loc) when not (String.IsNullOrWhiteSpace loc) -> loc
        | _ ->
            if entityRef.EndsWith("01") || entityRef.EndsWith("03") then "Site-Austin-01"
            else "Site-Dresden-02"

    let private inferProductGroup (cells: Map<string, CellValue>) (entityRef: string) : string =
        match Map.tryFind "ProductGroup" cells with
        | Some (TextValue grp) when not (String.IsNullOrWhiteSpace grp) -> grp
        | _ ->
            if entityRef.Contains("CHIP") || entityRef.Contains("MCU") then "Industrial Microcontrollers"
            elif entityRef.Contains("TRANS") || entityRef.Contains("PWR") then "Power Management ICs"
            elif entityRef.Contains("SNS") || entityRef.Contains("RADAR") then "Automotive Sensors"
            else "Standard Electronics"

    let extractRawAppointments (data: DataPayload) : (DemandRecord list * SupplyRecord list) =
        match data with
        | Tabular rows ->
            let demands = ResizeArray<DemandRecord>()
            let supplies = ResizeArray<SupplyRecord>()

            for row in rows do
                match Map.tryFind "Id" row.Cells, Map.tryFind "Start" row.Cells, Map.tryFind "Quantity" row.Cells with
                | Some (TextValue id), Some (DateTimeValue startDto), Some (NumberValue qty) ->
                    let itemRef =
                        match Map.tryFind "EntityRef" row.Cells with
                        | Some (TextValue r) -> r
                        | _ -> id

                    let itemType =
                        match Map.tryFind "ItemType" row.Cells with
                        | Some (TextValue t) -> t
                        | _ -> "PlannedSupply"

                    let status =
                        match Map.tryFind "Status" row.Cells with
                        | Some (TextValue s) -> s
                        | _ -> "Active"

                    let plant = inferPlant row.Cells itemRef
                    let group = inferProductGroup row.Cells itemRef

                    if itemType = "Demand" then
                        let customerOpt =
                            match Map.tryFind "Customer" row.Cells with
                            | Some (TextValue c) -> Some c
                            | _ -> None

                        demands.Add
                            {
                                DemandId = id
                                Customer = customerOpt
                                Quantity = qty
                                ItemRef = itemRef
                                Plant = plant
                                ProductGroup = group
                                RequiredDate = startDto
                                Status = status
                            }
                    else
                        let streamType =
                            match Map.tryFind "SupplyType" row.Cells with
                            | Some (TextValue "Procurement") -> PurchaseOrder "Supplier Alpha"
                            | Some (TextValue "Production") -> ProductionOrder "Line 1"
                            | Some (TextValue "Transfer") -> TransferOrder "Central DC"
                            | _ ->
                                if id.StartsWith("PO") || id.Contains("Purch") then PurchaseOrder "Supplier Alpha"
                                elif id.StartsWith("INV") || id.Contains("Stock") then InventoryOnHand
                                else ProductionOrder "Assembly Line"

                        supplies.Add
                            {
                                SupplyId = id
                                Stream = streamType
                                Quantity = qty
                                ItemRef = itemRef
                                Plant = plant
                                ProductGroup = group
                                AvailableDate = startDto
                                Status = status
                            }
                | _ -> ()

            (List.ofSeq demands, List.ofSeq supplies)
        | _ -> ([], [])

    let extractInitialDate (data: DataPayload) : DateTime =
        match data with
        | Tabular rows ->
            rows
            |> List.choose (fun r ->
                match Map.tryFind "Start" r.Cells with
                | Some (DateTimeValue dto) -> Some dto.DateTime.Date
                | _ -> None)
            |> List.sort
            |> List.tryHead
            |> Option.defaultValue DateTime.Today
        | _ -> DateTime.Today

    let buildMatrixRows (data: DataPayload) (model: Model) : PeggingMatrixRow list =
        let demands, supplies = extractRawAppointments data

        let horizonDates =
            [ 0 .. model.HorizonDays - 1 ]
            |> List.map (fun offset -> model.StartDate.AddDays(float offset).Date)

        // Group demands & supplies by (Plant, ProductGroup, SKU)
        let allKeys =
            let demandKeys = demands |> List.map (fun d -> d.Plant, d.ProductGroup, d.ItemRef)
            let supplyKeys = supplies |> List.map (fun s -> s.Plant, s.ProductGroup, s.ItemRef)
            Set.ofList (demandKeys @ supplyKeys) |> Set.toList

        let demandsByDateAndKey =
            demands
            |> List.groupBy (fun d -> d.Plant, d.ProductGroup, d.ItemRef, d.RequiredDate.Date)
            |> Map.ofList

        let suppliesByDateAndKey =
            supplies
            |> List.groupBy (fun s -> s.Plant, s.ProductGroup, s.ItemRef, s.AvailableDate.Date)
            |> Map.ofList

        // Build SKU Rows (Depth = 2)
        let skuRows =
            allKeys
            |> List.map (fun (plant, group, sku) ->
                let dailyCells =
                    horizonDates
                    |> List.map (fun dt ->
                        let cellDemands = Map.tryFind (plant, group, sku, dt) demandsByDateAndKey |> Option.defaultValue []
                        let cellSupplies = Map.tryFind (plant, group, sku, dt) suppliesByDateAndKey |> Option.defaultValue []
                        let totDem = cellDemands |> List.sumBy (fun d -> d.Quantity)
                        let totSup = cellSupplies |> List.sumBy (fun s -> s.Quantity)

                        dt,
                        {
                            Date = dt
                            TotalDemand = totDem
                            TotalSupply = totSup
                            Supplies = cellSupplies
                            Demands = cellDemands
                            ProjectedOnHand = 0m
                        })
                    |> Map.ofList

                let totD = dailyCells |> Map.values |> Seq.sumBy (fun c -> c.TotalDemand)
                let totS = dailyCells |> Map.values |> Seq.sumBy (fun c -> c.TotalSupply)

                let skuInfo: ProductSkuInfo =
                    {
                        SkuId = sku
                        Name = sku
                        GroupId = group
                        PlantId = plant
                        Uom = "EA"
                        SafetyStock = 50m
                    }

                {
                    RowId = $"{plant}/{group}/{sku}"
                    Node = ProductSkuNode skuInfo
                    ParentId = Some $"{plant}/{group}"
                    Depth = 2
                    IsExpanded = false
                    HasChildren = false
                    TotalDemand = totD
                    TotalSupply = totS
                    NetBalance = totS - totD
                    DailyCells = dailyCells
                })

        // Build Product Group Rows (Depth = 1, rollup of SKUs)
        let groupKeys = skuRows |> List.map (fun r -> match r.Node with ProductSkuNode s -> s.PlantId, s.GroupId | _ -> "", "") |> List.distinct
        let groupRows =
            groupKeys
            |> List.map (fun (plant, group) ->
                let childSkus =
                    skuRows
                    |> List.filter (fun r -> match r.Node with ProductSkuNode s -> s.PlantId = plant && s.GroupId = group | _ -> false)

                let dailyCells =
                    horizonDates
                    |> List.map (fun dt ->
                        let cellsForDate = childSkus |> List.choose (fun s -> Map.tryFind dt s.DailyCells)
                        let totDem = cellsForDate |> List.sumBy (fun c -> c.TotalDemand)
                        let totSup = cellsForDate |> List.sumBy (fun c -> c.TotalSupply)
                        let allSupplies = cellsForDate |> List.collect (fun c -> c.Supplies)
                        let allDemands = cellsForDate |> List.collect (fun c -> c.Demands)

                        dt,
                        {
                            Date = dt
                            TotalDemand = totDem
                            TotalSupply = totSup
                            Supplies = allSupplies
                            Demands = allDemands
                            ProjectedOnHand = 0m
                        })
                    |> Map.ofList

                let totD = dailyCells |> Map.values |> Seq.sumBy (fun c -> c.TotalDemand)
                let totS = dailyCells |> Map.values |> Seq.sumBy (fun c -> c.TotalSupply)

                let groupInfo: ProductGroupInfo = { Id = group; Name = group; PlantId = plant }
                let rowId = $"{plant}/{group}"

                {
                    RowId = rowId
                    Node = ProductGroupNode groupInfo
                    ParentId = Some plant
                    Depth = 1
                    IsExpanded = Set.contains rowId model.ExpandedRowIds
                    HasChildren = not (List.isEmpty childSkus)
                    TotalDemand = totD
                    TotalSupply = totS
                    NetBalance = totS - totD
                    DailyCells = dailyCells
                })

        // Build Plant Rows (Depth = 0, rollup of Product Groups)
        let plantKeys = groupRows |> List.map (fun r -> match r.Node with ProductGroupNode g -> g.PlantId | _ -> "") |> List.distinct
        let plantRows =
            plantKeys
            |> List.map (fun plant ->
                let childGroups =
                    groupRows
                    |> List.filter (fun r -> match r.Node with ProductGroupNode g -> g.PlantId = plant | _ -> false)

                let dailyCells =
                    horizonDates
                    |> List.map (fun dt ->
                        let cellsForDate = childGroups |> List.choose (fun g -> Map.tryFind dt g.DailyCells)
                        let totDem = cellsForDate |> List.sumBy (fun c -> c.TotalDemand)
                        let totSup = cellsForDate |> List.sumBy (fun c -> c.TotalSupply)
                        let allSupplies = cellsForDate |> List.collect (fun c -> c.Supplies)
                        let allDemands = cellsForDate |> List.collect (fun c -> c.Demands)

                        dt,
                        {
                            Date = dt
                            TotalDemand = totDem
                            TotalSupply = totSup
                            Supplies = allSupplies
                            Demands = allDemands
                            ProjectedOnHand = 0m
                        })
                    |> Map.ofList

                let totD = dailyCells |> Map.values |> Seq.sumBy (fun c -> c.TotalDemand)
                let totS = dailyCells |> Map.values |> Seq.sumBy (fun c -> c.TotalSupply)

                let plantInfo: PlantInfo = { Id = plant; Name = plant }
                let rowId = plant

                {
                    RowId = rowId
                    Node = PlantNode plantInfo
                    ParentId = None
                    Depth = 0
                    IsExpanded = Set.contains rowId model.ExpandedRowIds
                    HasChildren = not (List.isEmpty childGroups)
                    TotalDemand = totD
                    TotalSupply = totS
                    NetBalance = totS - totD
                    DailyCells = dailyCells
                })

        // Hierarchy Assembly & Filter Pipeline
        let matchesSearch (node: HierarchyNode) =
            if String.IsNullOrWhiteSpace model.SearchQuery then true
            else
                let term = model.SearchQuery.Trim().ToLowerInvariant()
                match node with
                | PlantNode p -> p.Name.ToLowerInvariant().Contains(term)
                | ProductGroupNode g -> g.Name.ToLowerInvariant().Contains(term)
                | ProductSkuNode s -> s.SkuId.ToLowerInvariant().Contains(term) || s.Name.ToLowerInvariant().Contains(term)

        let matchesPlantFilter (node: HierarchyNode) =
            match model.SelectedPlantFilter with
            | Some selected when not (String.IsNullOrWhiteSpace selected) ->
                match node with
                | PlantNode p -> p.Id = selected
                | ProductGroupNode g -> g.PlantId = selected
                | ProductSkuNode s -> s.PlantId = selected
            | _ -> true

        let hasShortageCondition (row: PeggingMatrixRow) =
            if not model.ShowOnlyShortages then true
            else row.DailyCells |> Map.values |> Seq.exists (fun c -> c.HasShortage)

        // Flatten visible tree according to expansion state
        let flattenedVisible =
            plantRows
            |> List.filter (fun p -> matchesPlantFilter p.Node && hasShortageCondition p)
            |> List.collect (fun pRow ->
                let isPlantExpanded = Set.contains pRow.RowId model.ExpandedRowIds

                let matchingGroups =
                    if not isPlantExpanded then []
                    else
                        groupRows
                        |> List.filter (fun g -> g.ParentId = Some pRow.RowId && hasShortageCondition g)
                        |> List.collect (fun gRow ->
                            let isGroupExpanded = Set.contains gRow.RowId model.ExpandedRowIds

                            let matchingSkus =
                                if not isGroupExpanded then []
                                else
                                    skuRows
                                    |> List.filter (fun s ->
                                        s.ParentId = Some gRow.RowId &&
                                        matchesSearch s.Node &&
                                        hasShortageCondition s)

                            gRow :: matchingSkus)

                pRow :: matchingGroups)

        flattenedVisible

    // ------------------------------------------------------------------------
    // Pure Bolero View Renderers (Gantt-Cells, Drawer, Workbench)
    // ------------------------------------------------------------------------

    let renderGanttCell
        (cell: DailyGanttCell)
        (onSelect: SelectedCellDetail -> unit)
        (rowId: string)
        (nodeTitle: string)
        : Node =

        let isShortage = cell.TotalDemand > 0m && cell.TotalSupply < cell.TotalDemand
        let isBalanced = cell.TotalDemand > 0m && cell.TotalSupply = cell.TotalDemand
        let isExcess = cell.TotalSupply > cell.TotalDemand && cell.TotalDemand > 0m
        let isUncommittedSupply = cell.TotalDemand = 0m && cell.TotalSupply > 0m
        let hasActivity = cell.TotalDemand > 0m || cell.TotalSupply > 0m
        let coveragePercent = int (cell.CoverageRatio * 100m)

        let cellDetail: SelectedCellDetail =
            {
                RowId = rowId
                NodeTitle = nodeTitle
                Date = cell.Date
                Cell = cell
            }

        let microBarStyle =
            sprintf "width: %d%%; height: 100%%; transition: width 0.3s ease; %s"
                coveragePercent
                (if isShortage then "background: var(--rz-danger);"
                 elif isBalanced then "background: var(--rz-success);"
                 elif isExcess then "background: var(--rz-warning);"
                 else "background: var(--rz-info);")

        let cellBgStyle =
            if isShortage then "background: var(--rz-danger-lighter); border: 1px solid var(--rz-danger-light);"
            elif isBalanced then "background: var(--rz-success-lighter); border: 1px solid var(--rz-success-light);"
            elif isExcess then "background: var(--rz-warning-lighter); border: 1px solid var(--rz-warning-light);"
            elif isUncommittedSupply then "background: rgba(var(--rz-info-rgb), 0.08); border: 1px solid var(--rz-info-light);"
            elif hasActivity then "background: var(--rz-base-100); border: 1px solid var(--rz-border-color);"
            else "opacity: 0.35; border: 1px dashed var(--rz-border-color); justify-content: center; align-items: center;"

        div {
            attr.``class`` "pegging-gantt-cell"
            attr.style (
                "display: flex; flex-direction: column; gap: 2px; padding: 4px 6px; " +
                "border-radius: 4px; font-size: 11px; cursor: pointer; transition: all 0.15s ease; box-sizing: border-box; width: 100%; min-height: 52px; " +
                cellBgStyle
            )
            on.click (fun _ -> if hasActivity then onSelect cellDetail)

            if not hasActivity then
                span {
                    attr.style "color: var(--rz-text-secondary-color); font-size: 10px;"
                    text "-"
                }
            else
                // Row 1: Demands (Required Date) & Status Icon Badge
                div {
                    attr.style "display: flex; justify-content: space-between; align-items: center;"
                    span {
                        attr.style (
                            if cell.TotalDemand > 0m then "color: var(--rz-danger-dark); font-weight: 700;"
                            else "color: var(--rz-text-secondary-color);"
                        )
                        $"▼ D: {cell.TotalDemand:N0}"
                    }
                    if isShortage then
                        span {
                            attr.style "color: var(--rz-danger); font-weight: 800; font-size: 11px; line-height: 1;"
                            attr.title $"Danger / Deficit: Demand is {cell.TotalDemand:N0} but Supply is {cell.TotalSupply:N0} (Net: {cell.NetBalance:N0})"
                            text "⚠"
                        }
                    elif isBalanced then
                        span {
                            attr.style "color: var(--rz-success); font-weight: 800; font-size: 11px; line-height: 1;"
                            attr.title $"Success / Balanced: Supply matches Demand ({cell.TotalSupply:N0})"
                            text "✓"
                        }
                    elif isExcess then
                        span {
                            attr.style "color: var(--rz-warning-dark); font-weight: 800; font-size: 10px; line-height: 1;"
                            attr.title $"Warning / Excess: Over-producing by +{cell.NetBalance:N0} (Supply: {cell.TotalSupply:N0} > Demand: {cell.TotalDemand:N0})"
                            text "▲+"
                        }
                }

                // Row 2: Visual Gantt Micro-Bar (Coverage Fill)
                div {
                    attr.style "width: 100%; height: 5px; background: var(--rz-base-300); border-radius: 3px; overflow: hidden; display: flex; margin: 1px 0;"
                    if cell.TotalDemand > 0m then
                        div {
                            attr.style microBarStyle
                        }
                }

                // Row 3: Total Supply & Net Balance
                div {
                    attr.style "display: flex; justify-content: space-between; align-items: center; font-size: 10.5px;"
                    span {
                        attr.style (
                            if cell.TotalSupply > 0m then "color: var(--rz-success-dark); font-weight: 700;"
                            else "color: var(--rz-text-secondary-color);"
                        )
                        $"▲ S: {cell.TotalSupply:N0}"
                    }
                    span {
                        attr.style (
                            if isExcess then "color: var(--rz-warning-dark); font-weight: 700;"
                            elif isBalanced then "color: var(--rz-success-dark); font-weight: 700;"
                            elif isShortage then "color: var(--rz-danger-dark); font-weight: 800;"
                            elif cell.NetBalance > 0m then "color: var(--rz-success-dark); font-weight: 700;"
                            else "color: var(--rz-text-secondary-color);"
                        )
                        let balText = if cell.NetBalance > 0m then $"+{cell.NetBalance:N0}" else $"{cell.NetBalance:N0}"
                        text balText
                    }
                }

                // Row 4: Supply Streams Breakdown (PO / WO / Inv)
                if cell.TotalSupply > 0m then
                    div {
                        attr.style "display: flex; gap: 3px; margin-top: 2px; flex-wrap: wrap;"
                        if cell.PoQuantity > 0m then
                            span {
                                attr.style "background: #0284c7; color: white; padding: 0 3px; border-radius: 2px; font-size: 9px; font-weight: 600;"
                                attr.title $"Purchase Orders: {cell.PoQuantity:N0}"
                                text $"PO:{cell.PoQuantity:N0}"
                            }
                        if cell.WoQuantity > 0m then
                            span {
                                attr.style "background: #16a34a; color: white; padding: 0 3px; border-radius: 2px; font-size: 9px; font-weight: 600;"
                                attr.title $"Production Orders: {cell.WoQuantity:N0}"
                                text $"WO:{cell.WoQuantity:N0}"
                            }
                        if cell.StockQuantity > 0m then
                            span {
                                attr.style "background: #64748b; color: white; padding: 0 3px; border-radius: 2px; font-size: 9px; font-weight: 600;"
                                attr.title $"On-Hand Stock: {cell.StockQuantity:N0}"
                                text $"Inv:{cell.StockQuantity:N0}"
                            }
                    }
        }

    let renderCellDetailDrawer (selected: SelectedCellDetail) (dispatch: Msg -> unit) : Node =
        let dateStr = selected.Date.ToString("yyyy-MM-dd (ddd)")
        div {
            attr.style "display: flex; flex-direction: column; gap: 8px; padding: 12px; background: var(--rz-base-100); border-top: 2px solid var(--rz-primary); height: 100%; box-sizing: border-box; overflow: auto;"

            // Drawer Header
            div {
                attr.style "display: flex; justify-content: space-between; align-items: center;"
                div {
                    attr.style "display: flex; align-items: center; gap: 8px;"
                    span {
                        attr.``class`` "rzi"
                        attr.style "color: var(--rz-primary); font-size: 18px;"
                        text "fact_check"
                    }
                    span {
                        attr.style "font-weight: 700; font-size: 13px;"
                        text $"Pegging Trace: {selected.NodeTitle} · Date: {dateStr}"
                    }
                    span {
                        attr.style (
                            "padding: 2px 8px; border-radius: 10px; font-size: 11px; font-weight: 700; " +
                            (if selected.Cell.HasShortage then "background: var(--rz-danger-lighter); color: var(--rz-danger); border: 1px solid var(--rz-danger);"
                             else "background: var(--rz-success-lighter); color: var(--rz-success); border: 1px solid var(--rz-success);")
                        )
                        let badgeText = if selected.Cell.HasShortage then $"Deficit: {selected.Cell.NetBalance:N0} EA" else $"Surplus: +{selected.Cell.NetBalance:N0} EA"
                        text badgeText
                    }
                }

                button {
                    attr.``class`` "rz-button rz-button-xs rz-light"
                    on.click (fun _ -> dispatch ClearCellSelection)
                    span { attr.``class`` "rzi"; text "close" }
                }
            }

            // Dual Cards: Demands vs Supplies
            div {
                attr.style "display: flex; gap: 12px; flex: 1; min-height: 0;"

                // Left: Demands Table
                div {
                    attr.style "flex: 1; display: flex; flex-direction: column; border: 1px solid var(--rz-border-color); border-radius: 4px; overflow: hidden;"
                    div {
                        attr.style "padding: 6px 10px; background: var(--rz-danger-lighter); font-weight: 700; font-size: 12px; color: var(--rz-danger-dark); border-bottom: 1px solid var(--rz-border-color);"
                        text $"Required Demands ({selected.Cell.Demands.Length} orders · Total: {selected.Cell.TotalDemand:N0} EA)"
                    }
                    div {
                        attr.style "flex: 1; overflow-y: auto; padding: 4px;"
                        if selected.Cell.Demands.IsEmpty then
                            div { attr.style "padding: 8px; color: var(--rz-text-secondary-color); font-size: 11px;"; text "No demand orders on this date." }
                        else
                            forEach selected.Cell.Demands (fun d ->
                                div {
                                    attr.style "display: flex; justify-content: space-between; align-items: center; padding: 4px 6px; border-bottom: 1px dashed var(--rz-border-color); font-size: 11.5px;"
                                    div {
                                        span { attr.style "font-weight: 600;"; text d.DemandId }
                                        match d.Customer with
                                        | Some c -> span { attr.style "margin-left: 6px; color: var(--rz-text-secondary-color);"; text $"({c})" }
                                        | None -> ()
                                    }
                                    div {
                                        span { attr.style "font-weight: 700; color: var(--rz-danger);"; text $"{d.Quantity:N0} EA" }
                                        span { attr.style "margin-left: 8px; font-size: 10px; padding: 1px 5px; border-radius: 3px; background: var(--rz-base-200);"; text d.Status }
                                    }
                                })
                    }
                }

                // Right: Supplies Table
                div {
                    attr.style "flex: 1; display: flex; flex-direction: column; border: 1px solid var(--rz-border-color); border-radius: 4px; overflow: hidden;"
                    div {
                        attr.style "padding: 6px 10px; background: var(--rz-success-lighter); font-weight: 700; font-size: 12px; color: var(--rz-success-dark); border-bottom: 1px solid var(--rz-border-color);"
                        text $"Available Supplies ({selected.Cell.Supplies.Length} orders · Total: {selected.Cell.TotalSupply:N0} EA)"
                    }
                    div {
                        attr.style "flex: 1; overflow-y: auto; padding: 4px;"
                        if selected.Cell.Supplies.IsEmpty then
                            div { attr.style "padding: 8px; color: var(--rz-text-secondary-color); font-size: 11px;"; text "No supply receipts scheduled for this date." }
                        else
                            forEach selected.Cell.Supplies (fun s ->
                                let streamColor, streamLabel =
                                    match s.Stream with
                                    | PurchaseOrder v -> "#0284c7", $"PO ({v})"
                                    | ProductionOrder w -> "#16a34a", $"WO ({w})"
                                    | InventoryOnHand -> "#64748b", "On-Hand Stock"
                                    | TransferOrder t -> "#9333ea", $"Xfer ({t})"

                                div {
                                    attr.style "display: flex; justify-content: space-between; align-items: center; padding: 4px 6px; border-bottom: 1px dashed var(--rz-border-color); font-size: 11.5px;"
                                    div {
                                        span { attr.style "font-weight: 600;"; text s.SupplyId }
                                        span {
                                            attr.style $"background: {streamColor}; color: white; padding: 1px 5px; border-radius: 3px; font-size: 9.5px; margin-left: 6px;"
                                            text streamLabel
                                        }
                                    }
                                    div {
                                        span { attr.style "font-weight: 700; color: var(--rz-success-dark);"; text $"{s.Quantity:N0} EA" }
                                        span { attr.style "margin-left: 8px; font-size: 10px; padding: 1px 5px; border-radius: 3px; background: var(--rz-base-200);"; text s.Status }
                                    }
                                })
                    }
                }
            }
        }

    let renderWorkbench (model: Model) (data: DataPayload) (dispatch: Msg -> unit) : Node =
        let matrixRows = buildMatrixRows data model

        let horizonDates =
            [ 0 .. model.HorizonDays - 1 ]
            |> List.map (fun offset -> model.StartDate.AddDays(float offset).Date)

        let totalDemandsAcross = matrixRows |> List.filter (fun r -> r.Depth = 0) |> List.sumBy (fun r -> r.TotalDemand)
        let totalSuppliesAcross = matrixRows |> List.filter (fun r -> r.Depth = 0) |> List.sumBy (fun r -> r.TotalSupply)
        let totalShortageCells =
            matrixRows
            |> List.filter (fun r -> r.Depth = 2)
            |> List.sumBy (fun r -> r.DailyCells |> Map.values |> Seq.filter (fun c -> c.HasShortage) |> Seq.length)

        let distinctPlants =
            matrixRows
            |> List.choose (fun r -> match r.Node with PlantNode p -> Some p.Id | _ -> None)
            |> List.distinct

        div {
            attr.style "display: flex; flex-direction: column; width: 100%; height: 100%; overflow: hidden; background: var(--rz-base-background-color);"

            // ----------------------------------------------------------------
            // 1. Top Enterprise Control Bar (Kinaxis Scope & Actions)
            // ----------------------------------------------------------------
            div {
                attr.style "display: flex; align-items: center; justify-content: space-between; padding: 6px 12px; border-bottom: 1px solid var(--rz-border-color); background: var(--rz-base-200); flex-wrap: wrap; gap: 8px;"

                // Scope Filters (Plant, Search, Horizon, Shortages)
                div {
                    attr.style "display: flex; align-items: center; gap: 8px; flex-wrap: wrap;"

                    // Scope: Plant Selector
                    select {
                        attr.``class`` "rz-dropdown"
                        attr.style "height: 30px; font-size: 12px; padding: 0 8px; border-radius: 4px; border: 1px solid var(--rz-border-color);"
                        attr.value (model.SelectedPlantFilter |> Option.defaultValue "ALL")
                        on.change (fun (args: ChangeEventArgs) ->
                            let v = string args.Value
                            let filterOpt = if String.IsNullOrWhiteSpace v || v = "ALL" then None else Some v
                            dispatch (SetPlantFilter filterOpt))

                        option {
                            attr.value "ALL"
                            text "🏭 All Sites / Plants"
                        }
                        forEach distinctPlants (fun p ->
                            option {
                                attr.value p
                                text $"🏭 {p}"
                            })
                    }

                    // Scope: Horizon Buttons
                    div {
                        attr.style "display: flex; border: 1px solid var(--rz-border-color); border-radius: 4px; overflow: hidden;"

                        button {
                            attr.``class`` (if model.HorizonDays = 7 then "rz-button rz-button-xs rz-primary" else "rz-button rz-button-xs rz-light")
                            attr.style "border-radius: 0; padding: 3px 8px; font-size: 11px;"
                            on.click (fun _ -> dispatch (SetHorizonDays 7))
                            text "7D"
                        }
                        button {
                            attr.``class`` (if model.HorizonDays = 14 then "rz-button rz-button-xs rz-primary" else "rz-button rz-button-xs rz-light")
                            attr.style "border-radius: 0; padding: 3px 8px; font-size: 11px;"
                            on.click (fun _ -> dispatch (SetHorizonDays 14))
                            text "14D"
                        }
                        button {
                            attr.``class`` (if model.HorizonDays = 30 then "rz-button rz-button-xs rz-primary" else "rz-button rz-button-xs rz-light")
                            attr.style "border-radius: 0; padding: 3px 8px; font-size: 11px;"
                            on.click (fun _ -> dispatch (SetHorizonDays 30))
                            text "30D"
                        }
                    }

                    // Scope: Filter Shortages Toggle
                    button {
                        attr.``class`` (
                            if model.ShowOnlyShortages then "rz-button rz-button-xs rz-danger"
                            else "rz-button rz-button-xs rz-light"
                        )
                        attr.style "padding: 3px 8px; font-size: 11px; display: flex; align-items: center; gap: 4px;"
                        on.click (fun _ -> dispatch ToggleShowOnlyShortages)
                        span { attr.``class`` "rzi"; text "warning" }
                        text (if model.ShowOnlyShortages then "Shortages Only [ON]" else "Filter Shortages")
                    }

                    // Search input
                    input {
                        attr.``class`` "rz-textbox"
                        attr.style "height: 30px; font-size: 12px; padding: 0 8px; width: 170px;"
                        attr.placeholder "Filter SKU / Product..."
                        attr.value model.SearchQuery
                        on.input (fun (args: ChangeEventArgs) ->
                            dispatch (SetSearchQuery (string args.Value)))
                    }
                }

                // Summary KPIs & Action Buttons
                div {
                    attr.style "display: flex; align-items: center; gap: 10px;"

                    // Horizon KPIs
                    div {
                        attr.style "display: flex; align-items: center; gap: 8px; font-size: 11.5px;"
                        span {
                            attr.style "color: var(--rz-danger-dark); font-weight: 700;"
                            text $"Demands: {totalDemandsAcross:N0}"
                        }
                        span { attr.style "color: var(--rz-border-color);"; text "|" }
                        span {
                            attr.style "color: var(--rz-success-dark); font-weight: 700;"
                            text $"Supplies: {totalSuppliesAcross:N0}"
                        }
                        span { attr.style "color: var(--rz-border-color);"; text "|" }
                        span {
                            attr.style (
                                if totalShortageCells > 0 then "color: var(--rz-danger); font-weight: 800;"
                                else "color: var(--rz-text-secondary-color);"
                            )
                            text $"Shortage Alerts: {totalShortageCells}"
                        }
                    }

                    // Workbench Actions
                    button {
                        attr.``class`` "rz-button rz-button-sm rz-secondary rz-shade-default"
                        attr.style "display: flex; align-items: center; gap: 4px;"
                        on.click (fun _ -> dispatch RealignPeggingRequested)

                        span { attr.``class`` "rzi"; text "sync" }
                        span { text "Realign Pegging" }
                    }

                    button {
                        attr.``class`` "rz-button rz-button-sm rz-base-100 rz-shade-default"
                        attr.style "display: flex; align-items: center;"
                        on.click (fun _ -> dispatch RefreshRequested)

                        span { attr.``class`` "rzi"; text "refresh" }
                    }
                }
            }

            // ----------------------------------------------------------------
            // 1b. Pegging Status & Supply Stream Legend Bar (Kinaxis/Quintiq Pattern)
            // ----------------------------------------------------------------
            div {
                attr.style "display: flex; align-items: center; justify-content: space-between; padding: 4px 12px; background: rgba(0,0,0,0.02); border-bottom: 1px solid var(--rz-border-color); font-size: 11px; flex-wrap: wrap; gap: 8px;"

                // Status Indicators
                div {
                    attr.style "display: flex; align-items: center; gap: 14px;"
                    span {
                        attr.style "font-weight: 700; color: var(--rz-text-secondary-color); text-transform: uppercase; font-size: 10px; letter-spacing: 0.5px;"
                        text "Status Indicators:"
                    }
                    div {
                        attr.style "display: flex; align-items: center; gap: 5px;"
                        span {
                            attr.style "width: 15px; height: 15px; border-radius: 3px; background: var(--rz-danger-lighter); border: 1px solid var(--rz-danger); color: var(--rz-danger); font-size: 10px; font-weight: 800; display: inline-flex; align-items: center; justify-content: center;"
                            text "⚠"
                        }
                        span {
                            attr.style "font-weight: 600; color: var(--rz-danger-dark);"
                            text "Danger / Deficit"
                        }
                        span {
                            attr.style "color: var(--rz-text-secondary-color); font-size: 10px;"
                            text "(Supply < Demand)"
                        }
                    }
                    div {
                        attr.style "display: flex; align-items: center; gap: 5px;"
                        span {
                            attr.style "width: 15px; height: 15px; border-radius: 3px; background: var(--rz-success-lighter); border: 1px solid var(--rz-success); color: var(--rz-success); font-size: 10px; font-weight: 800; display: inline-flex; align-items: center; justify-content: center;"
                            text "✓"
                        }
                        span {
                            attr.style "font-weight: 600; color: var(--rz-success-dark);"
                            text "Success / Balanced"
                        }
                        span {
                            attr.style "color: var(--rz-text-secondary-color); font-size: 10px;"
                            text "(Supply = Demand)"
                        }
                    }
                    div {
                        attr.style "display: flex; align-items: center; gap: 5px;"
                        span {
                            attr.style "width: 15px; height: 15px; border-radius: 3px; background: var(--rz-warning-lighter); border: 1px solid var(--rz-warning); color: var(--rz-warning-dark); font-size: 9.5px; font-weight: 800; display: inline-flex; align-items: center; justify-content: center;"
                            text "▲+"
                        }
                        span {
                            attr.style "font-weight: 600; color: var(--rz-warning-dark);"
                            text "Warning / Excess"
                        }
                        span {
                            attr.style "color: var(--rz-text-secondary-color); font-size: 10px;"
                            text "(Supply > Demand · Over-producing Risk)"
                        }
                    }
                }

                // Supply Streams Indicators
                div {
                    attr.style "display: flex; align-items: center; gap: 8px;"
                    span {
                        attr.style "font-weight: 700; color: var(--rz-text-secondary-color); text-transform: uppercase; font-size: 10px; letter-spacing: 0.5px;"
                        text "Streams:"
                    }
                    span {
                        attr.style "background: #0284c7; color: white; padding: 1px 5px; border-radius: 2px; font-size: 9.5px; font-weight: 600;"
                        text "PO: Purchase"
                    }
                    span {
                        attr.style "background: #16a34a; color: white; padding: 1px 5px; border-radius: 2px; font-size: 9.5px; font-weight: 600;"
                        text "WO: Work Order"
                    }
                    span {
                        attr.style "background: #64748b; color: white; padding: 1px 5px; border-radius: 2px; font-size: 9.5px; font-weight: 600;"
                        text "Inv: On-Hand"
                    }
                    span {
                        attr.style "background: #9333ea; color: white; padding: 1px 5px; border-radius: 2px; font-size: 9.5px; font-weight: 600;"
                        text "Xfer: Transfer"
                    }
                }
            }

            // ----------------------------------------------------------------
            // 2. Main Workbench Body (Grid & Collapsible Detail Drawer)
            // ----------------------------------------------------------------
            div {
                attr.style "display: flex; flex-direction: column; flex: 1; min-height: 0; width: 100%; overflow: hidden;"

                // Grid Area
                div {
                    attr.style (
                        "flex: 1; min-height: 0; width: 100%; overflow: hidden; " +
                        (if model.SelectedCell.IsSome then "height: 65%;" else "height: 100%;")
                    )

                    comp<RadzenDataGrid<PeggingMatrixRow>> {
                        "Data" => (matrixRows :> seq<PeggingMatrixRow>)
                        "AllowSorting" => false
                        "AllowFiltering" => false
                        "AllowColumnResize" => true
                        "Density" => Density.Compact
                        "Style" => "width: 100%; height: 100%; min-height: 0;"

                        attr.fragment "Columns" (concat {
                            // Pinned Left: Supply Chain Hierarchy Node
                            comp<RadzenDataGridColumn<PeggingMatrixRow>> {
                                "Title" => "Plant -> Product Group -> Product SKU"
                                "Width" => "280px"
                                "Frozen" => true
                                attr.fragmentWith "Template" (fun (row: PeggingMatrixRow) ->
                                    let indent = row.Depth * 18
                                    div {
                                        attr.style $"display: flex; align-items: center; gap: 6px; padding-left: {indent}px;"

                                        if row.HasChildren then
                                            button {
                                                attr.``class`` "rz-button rz-button-xs rz-light"
                                                attr.style "padding: 1px 5px; font-size: 10px; line-height: 1; min-width: 18px;"
                                                on.click (fun _ -> dispatch (ToggleRowExpand row.RowId))
                                                text (if row.IsExpanded then "▼" else "▶")
                                            }
                                        else
                                            span { attr.style "display: inline-block; width: 18px;" }

                                        match row.Node with
                                        | PlantNode p ->
                                            span {
                                                attr.style "font-weight: 700; font-size: 12.5px; color: var(--rz-primary-dark);"
                                                text $"🏭 {p.Name}"
                                            }
                                        | ProductGroupNode g ->
                                            span {
                                                attr.style "font-weight: 600; font-size: 12px; color: var(--rz-text-color);"
                                                text $"📦 {g.Name}"
                                            }
                                        | ProductSkuNode s ->
                                            div {
                                                attr.style "display: flex; flex-direction: column;"
                                                span {
                                                    attr.style "font-weight: 600; font-size: 11.5px; color: var(--rz-text-color);"
                                                    text s.SkuId
                                                }
                                                span {
                                                    attr.style "font-size: 10px; color: var(--rz-text-secondary-color);"
                                                    text $"SS: {s.SafetyStock:N0} {s.Uom}"
                                                }
                                            }
                                    }
                                )
                            }

                            // Summary Columns: Total Demand & Supply Across Horizon
                            comp<RadzenDataGridColumn<PeggingMatrixRow>> {
                                "Title" => "Tot. Demand"
                                "Width" => "95px"
                                "TextAlign" => TextAlign.Right
                                "Frozen" => true
                                attr.fragmentWith "Template" (fun (row: PeggingMatrixRow) ->
                                    span {
                                        attr.style "font-weight: 600; color: var(--rz-danger-dark); font-size: 11.5px;"
                                        text $"{row.TotalDemand:N0}"
                                    }
                                )
                            }

                            comp<RadzenDataGridColumn<PeggingMatrixRow>> {
                                "Title" => "Tot. Supply"
                                "Width" => "95px"
                                "TextAlign" => TextAlign.Right
                                "Frozen" => true
                                attr.fragmentWith "Template" (fun (row: PeggingMatrixRow) ->
                                    span {
                                        attr.style "font-weight: 600; color: var(--rz-success-dark); font-size: 11.5px;"
                                        text $"{row.TotalSupply:N0}"
                                    }
                                )
                            }

                            comp<RadzenDataGridColumn<PeggingMatrixRow>> {
                                "Title" => "Net Balance"
                                "Width" => "95px"
                                "TextAlign" => TextAlign.Right
                                "Frozen" => true
                                attr.fragmentWith "Template" (fun (row: PeggingMatrixRow) ->
                                    span {
                                        attr.style (
                                            "font-weight: 700; font-size: 11.5px; " +
                                            (if row.NetBalance > 0m then "color: var(--rz-success-dark);"
                                             elif row.NetBalance < 0m then "color: var(--rz-danger); font-weight: 800;"
                                             else "color: var(--rz-text-secondary-color);")
                                        )
                                        let netStr = if row.NetBalance > 0m then $"+{row.NetBalance:N0}" else $"{row.NetBalance:N0}"
                                        text netStr
                                    }
                                )
                            }

                            // Dynamic Daily Date Columns (Gantt-Cells)
                            forEach horizonDates (fun colDate ->
                                let targetDate = colDate.Date
                                let isToday = targetDate = DateTime.Today

                                comp<RadzenDataGridColumn<PeggingMatrixRow>> {
                                    "Title" => targetDate.ToString("ddd, dd MMM")
                                    "Width" => "160px"
                                    attr.fragment "HeaderTemplate" (
                                        div {
                                            attr.style (
                                                "text-align: center; " +
                                                (if isToday then "color: var(--rz-primary); font-weight: bold;" else "")
                                            )
                                            div { attr.style "font-size: 10px; text-transform: uppercase;"; text (targetDate.ToString("ddd")) }
                                            div { attr.style "font-size: 12.5px; font-weight: 700;"; text (targetDate.ToString("dd MMM")) }
                                        }
                                    )
                                    attr.fragmentWith "Template" (fun (row: PeggingMatrixRow) ->
                                        let nodeTitle =
                                            match row.Node with
                                            | PlantNode p -> p.Name
                                            | ProductGroupNode g -> g.Name
                                            | ProductSkuNode s -> s.SkuId

                                        match Map.tryFind targetDate row.DailyCells with
                                        | Some cell ->
                                            renderGanttCell
                                                cell
                                                (fun detail -> dispatch (SelectCell detail))
                                                row.RowId
                                                nodeTitle
                                        | None ->
                                            div {
                                                attr.style "text-align: center; color: var(--rz-text-secondary-color); font-size: 10px;"
                                                text "-"
                                            }
                                    )
                                })
                        })
                    }
                }

                // Collapsible Pegging Trace Detail Drawer
                match model.SelectedCell with
                | Some detail ->
                    div {
                        attr.style "height: 35%; min-height: 180px; width: 100%; border-top: 1px solid var(--rz-border-color);"
                        renderCellDetailDrawer detail dispatch
                    }
                | None -> ()
            }
        }

// ----------------------------------------------------------------------------
// Bolero Stateful Elmish Component Wrapper
// ----------------------------------------------------------------------------

type PeggingWorkbenchComponent() =
    inherit Component()

    let mutable model: ViewModel.Model = ViewModel.init DateTime.Today
    let mutable lastData: DataPayload = NoData
    let mutable externalDispatch: (ComponentEvent -> unit) = ignore

    [<Parameter>]
    member val Context: ComponentRenderContext = Unchecked.defaultof<ComponentRenderContext> with get, set

    [<Parameter>]
    member val Dispatch: ComponentEvent -> unit = ignore with get, set

    override this.OnParametersSet() =
        externalDispatch <- this.Dispatch
        if not (obj.ReferenceEquals(this.Context.Data, lastData)) then
            lastData <- this.Context.Data
            let initialDate = ViewModel.extractInitialDate this.Context.Data
            if model.ExpandedRowIds.IsEmpty then
                let rows = ViewModel.buildMatrixRows this.Context.Data model
                // Pre-expand top-level plants and product groups so structure is visible immediately
                let preExpanded =
                    rows
                    |> List.filter (fun r -> r.Depth <= 1)
                    |> List.map (fun r -> r.RowId)
                    |> Set.ofList
                model <- { model with StartDate = initialDate; ExpandedRowIds = preExpanded }

    member private this.DispatchLocal(msg: ViewModel.Msg) =
        match msg with
        | ViewModel.Msg.RealignPeggingRequested ->
            externalDispatch (ActionRequested(PeggingSchedulerActions.realignPegging.Id, None, NoPayload))
        | ViewModel.Msg.RefreshRequested ->
            externalDispatch RefreshRequested
        | _ -> ()

        model <- ViewModel.update msg model
        this.StateHasChanged()

    override this.Render() =
        ViewModel.renderWorkbench model this.Context.Data this.DispatchLocal

// ----------------------------------------------------------------------------
// Component Runtime Registration
// ----------------------------------------------------------------------------

let render (context: ComponentRenderContext) (dispatch: ComponentEvent -> unit) : Node =
    comp<PeggingWorkbenchComponent> {
        "Context" => context
        "Dispatch" => dispatch
    }

let runtime: ComponentRuntime = { Render = render }

let registration: ComponentRegistration =
    {
        Definition = componentDefinition
        Schema = schema
        Runtime = runtime
    }
