module Medhavi.Client.Components.ViewCatalog

open System
open Medhavi.Client.Registries
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.View
open Medhavi.Client.ViewComposition.Layout
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Data

// Feature slices: Core
open Medhavi.Client.Core.Demand
open Medhavi.Client.Core.Exceptions
open Medhavi.Client.Core.ItemTransition
open Medhavi.Client.Core.EnterprisePicture
open Medhavi.Client.Core.Traceability

// Feature slices: Demand
open Medhavi.Client.Demand.Observations
open Medhavi.Client.Demand.Understanding

// Feature slices: Supply
open Medhavi.Client.Supply.Inventory
open Medhavi.Client.Supply.Capacity
open Medhavi.Client.Supply.SupplyOrders
open Medhavi.Client.Supply.Exceptions
open Medhavi.Client.Supply.ChangeSensing
open Medhavi.Client.Supply.Collaboration
open Medhavi.Client.Supply.Explanation
open Medhavi.Client.Supply.Pegging

// AI & Cross-cutting
open Medhavi.Client.Components

let private defaultMetadata name category icon =
    {
        Category = category
        Icon = icon
        Roles = [ "Planner"; "Buyer"; "PlantManager"; "Executive" ]
        IsDefault = true
        IsTemplate = false
        CreatedAt = DateTimeOffset.UtcNow
        CreatedBy = "System"
        ModifiedAt = DateTimeOffset.UtcNow
        ModifiedBy = "System"
        Permissions =
            {
                Owner = "System"
                Visibility = ViewVisibility.Public
                EditableBy = [ "Planner"; "PlantManager" ]
                Shareable = true
            }
    }

let private idMedhaviAssistant = ViewComponentId.literal "comp-medhavi-assistant"
let private idDemand = ViewComponentId.literal "comp-demand-grid"
let private idException = ViewComponentId.literal "comp-exception-grid"
let private idItemTransition = ViewComponentId.literal "comp-item-transition-grid"
let private idEnterprisePicture = ViewComponentId.literal "comp-enterprise-picture-grid"
let private idDemandObservation = ViewComponentId.literal "comp-demand-observation-grid"
let private idInventory = ViewComponentId.literal "comp-inventory-grid"
let private idCapacity = ViewComponentId.literal "comp-capacity-grid"
let private idCapacityStudio = ViewComponentId.literal "comp-capacity-studio"
let private idSupply = ViewComponentId.literal "comp-supply-grid"
let private idSupplyUnderstanding = ViewComponentId.literal "comp-supply-understanding-grid"
let private idSupplyExceptions = ViewComponentId.literal "comp-supply-exceptions-grid"
let private idSupplyChanges = ViewComponentId.literal "comp-supply-changes-grid"
let private idSupplierCollaboration = ViewComponentId.literal "comp-supplier-collaboration-grid"
let private idSupplyExplanationStudio = ViewComponentId.literal "comp-supply-explanation-studio"
let private idPeggingScheduler = ViewComponentId.literal "comp-pegging-scheduler"
let private idKpiSummary = ViewComponentId.literal "comp-kpi-summary"
let private idDomainRadar = ViewComponentId.literal "comp-domain-radar"
let private idTraceabilityWorkbench = ViewComponentId.literal "comp-traceability-workbench"

let private compMedhaviAssistant: ViewComponent =
    {
        ComponentDefId = MedhaviAssistant.definitionId
        Binding =
            {
                StoreId = MedhaviAssistant.storeId
                SchemaDefId = MedhaviAssistant.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Medhavi Assistant"
        Icon = Some "psychology"
    }

let private compDemand: ViewComponent =
    {
        ComponentDefId = DemandOrdersGrid.definitionId
        Binding =
            {
                StoreId = DemandOrdersGrid.storeId
                SchemaDefId = DemandGridSchema.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Demand Orders"
        Icon = Some "shopping_cart"
    }

let private compException: ViewComponent =
    {
        ComponentDefId = ExceptionGrid.definitionId
        Binding =
            {
                StoreId = ExceptionStore.storeId
                SchemaDefId = ExceptionGridSchema.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Demand Exceptions"
        Icon = Some "warning"
    }

let private compItemTransition: ViewComponent =
    {
        ComponentDefId = ItemTransitionGrid.definitionId
        Binding =
            {
                StoreId = ItemTransitionStore.storeId
                SchemaDefId = ItemTransitionGridSchema.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Item Transitions"
        Icon = Some "swap_horiz"
    }

let private compEnterprisePicture: ViewComponent =
    {
        ComponentDefId = EnterprisePictureGrid.definitionId
        Binding =
            {
                StoreId = EnterprisePictureStore.storeId
                SchemaDefId = EnterprisePictureGridSchema.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Enterprise Picture"
        Icon = Some "photo_library"
    }

let private compTraceabilityWorkbench: ViewComponent =
    {
        ComponentDefId = TraceabilityWorkbench.definitionId
        Binding =
            {
                StoreId = TraceabilityStore.storeId
                SchemaDefId = TraceabilityGridSchema.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Traceability & Investigation Studio"
        Icon = Some "account_tree"
    }

let private compDemandObservation: ViewComponent =
    {
        ComponentDefId = DemandObservationGrid.definitionId
        Binding =
            {
                StoreId = DemandObservationStore.storeId
                SchemaDefId = DemandObservationGridSchema.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Demand Observations"
        Icon = Some "visibility"
    }

let private compInventory: ViewComponent =
    {
        ComponentDefId = InventoryGrid.definitionId
        Binding =
            {
                StoreId = InventoryStore.storeId
                SchemaDefId = InventoryGridSchema.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Inventory Stock Status"
        Icon = Some "inventory_2"
    }

let private compCapacity: ViewComponent =
    {
        ComponentDefId = CapacityGrid.definitionId
        Binding =
            {
                StoreId = CapacityStore.storeId
                SchemaDefId = CapacityGridSchema.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Plant Capacity Workcenters"
        Icon = Some "precision_manufacturing"
    }

let private compCapacityStudio: ViewComponent =
    {
        ComponentDefId = CapacityStudio.definitionId
        Binding =
            {
                StoreId = CapacityStudio.storeId
                SchemaDefId = CapacityGridSchema.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Capacity Planning Studio"
        Icon = Some "insights"
    }

let private compSupply: ViewComponent =
    {
        ComponentDefId = SupplyGrid.definitionId
        Binding =
            {
                StoreId = SupplyOrderStore.storeId
                SchemaDefId = SupplyOrderGridSchema.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Supply Purchase Orders"
        Icon = Some "local_shipping"
    }

let private compKpiSummary: ViewComponent =
    {
        ComponentDefId = KpiSummaryGrid.definitionId
        Binding =
            {
                StoreId = KpiSummaryGrid.storeId
                SchemaDefId = KpiSchema.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Executive KPI Summary"
        Icon = Some "speed"
    }

let private compDomainRadar: ViewComponent =
    {
        ComponentDefId = DomainHealthRadar.definitionId
        Binding =
            {
                StoreId = DomainHealthStore.storeId
                SchemaDefId = DomainHealthSchema.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "APS Domain Intelligence Radar"
        Icon = Some "radar"
    }

let private compSupplyExceptions: ViewComponent =
    {
        ComponentDefId = SupplyExceptionGrid.definitionId
        Binding =
            {
                StoreId = SupplyExceptionStore.storeId
                SchemaDefId = SupplyExceptionGridSchema.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Supply Exceptions"
        Icon = Some "warning"
    }

let private compSupplyChanges: ViewComponent =
    {
        ComponentDefId = SupplyChangeSensingGrid.definitionId
        Binding =
            {
                StoreId = SupplyChangeSensingStore.storeId
                SchemaDefId = SupplyChangeSensingGridSchema.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Supply Change Signals"
        Icon = Some "sensors"
    }

let private compSupplierCollaboration: ViewComponent =
    {
        ComponentDefId = SupplierCollaborationGrid.definitionId
        Binding =
            {
                StoreId = SupplierCollaborationStore.storeId
                SchemaDefId = SupplierCollaborationGridSchema.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Supplier Commitments"
        Icon = Some "handshake"
    }

let private compSupplyExplanationStudio: ViewComponent =
    {
        ComponentDefId = SupplyExplanationStudio.definitionId
        Binding =
            {
                StoreId = SupplyExplanationStore.storeId
                SchemaDefId = SupplyExplanationSchema.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Supply Reasoning Graph"
        Icon = Some "psychology"
    }

let private compPeggingScheduler: ViewComponent =
    {
        ComponentDefId = PeggingScheduler.definitionId
        Binding =
            {
                StoreId = PeggingSchedulerStore.storeId
                SchemaDefId = PeggingSchedulerSchema.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Demand-Supply Pegging Horizon"
        Icon = Some "alt_route"
    }

let medhaviAssistantView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-medhavi-assistant"
        Name = "Medhavi Assistant"
        Version = 1
        State =
            {
                Title = "Medhavi Assistant — AI Strategy Studio"
                Components = Map.ofList [ (idMedhaviAssistant, compMedhaviAssistant) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idMedhaviAssistant, TilePosition.literal 1 1 12 12) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Medhavi Assistant" "AI Intelligence" "auto_awesome"
    }

let executiveKpiDashboardView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-executive-kpi-dashboard"
        Name = "Executive KPI & Planning Dashboard"
        Version = 1
        State =
            {
                Title = "Executive Supply Chain Overview"
                Components =
                    Map.ofList
                        [
                            (idKpiSummary, compKpiSummary)
                            (idDomainRadar, compDomainRadar)
                            (idDemand, compDemand)
                            (idCapacity, compCapacity)
                        ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList
                            [
                                (idKpiSummary, TilePosition.literal 1 1 12 3)
                                (idDomainRadar, TilePosition.literal 1 4 4 5)
                                (idDemand, TilePosition.literal 5 4 8 5)
                                (idCapacity, TilePosition.literal 1 9 12 4)
                            ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Executive Overview" "Executive Dashboard" "dashboard"
    }

let demandSupplyCapacityTripleView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-demand-supply-capacity-triple"
        Name = "3-Way Demand-Supply-Capacity Analysis"
        Version = 1
        State =
            {
                Title = "3-Way Supply Chain Bottleneck Analysis"
                Components = Map.ofList [ (idDemand, compDemand); (idSupply, compSupply); (idCapacity, compCapacity) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList
                            [
                                (idDemand, TilePosition.literal 1 1 12 4)
                                (idSupply, TilePosition.literal 1 5 12 4)
                                (idCapacity, TilePosition.literal 1 9 12 4)
                            ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Demand-Supply-Capacity Triple" "Supply Intelligence" "account_tree"
    }

let coreExceptionsWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-core-exceptions"
        Name = "Core Exceptions Workbench"
        Version = 1
        State =
            {
                Title = "Core Constraint Exceptions"
                Components = Map.ofList [ (idException, compException) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idException, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Core Exceptions Workbench" "Core Management" "warning"
    }

let itemTransitionsWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-item-transitions"
        Name = "Item Transitions Workbench"
        Version = 1
        State =
            {
                Title = "Item Transitions & Supersession Governance"
                Components = Map.ofList [ (idItemTransition, compItemTransition) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idItemTransition, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Item Transitions Workbench" "Core Management" "swap_calls"
    }

let enterprisePictureWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-enterprise-picture"
        Name = "Enterprise Picture Workbench"
        Version = 1
        State =
            {
                Title = "Enterprise Picture Versions & Truth Ledger"
                Components = Map.ofList [ (idEnterprisePicture, compEnterprisePicture) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idEnterprisePicture, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Enterprise Picture Workbench" "Core Management" "photo_library"
    }

let demandObservationsStreamView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-demand-observations"
        Name = "Demand Observations Stream"
        Version = 1
        State =
            {
                Title = "Demand Observations Sensory Stream"
                Components = Map.ofList [ (idDemandObservation, compDemandObservation) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idDemandObservation, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Demand Observations Stream" "Demand Intelligence" "sensors"
    }

let capacityWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-capacity-workbench"
        Name = "Work Center Capacity Workbench"
        Version = 1
        State =
            {
                Title = "Work Center Capacity & Load"
                Components = Map.ofList [ (idCapacity, compCapacity) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idCapacity, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Work Center Capacity Workbench" "Capacity Planning" "precision_manufacturing"
    }

let capacityStudioView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-capacity-studio"
        Name = "Capacity Planning Studio (Gantt)"
        Version = 1
        State =
            {
                Title = "Capacity Planning Studio — Gantt & Envelopes"
                Components = Map.ofList [ (idCapacityStudio, compCapacityStudio) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idCapacityStudio, TilePosition.literal 1 1 12 8) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Capacity Planning Studio" "Capacity Planning" "view_timeline"
    }

let peggingDashboardView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-pegging-dashboard"
        Name = "Demand-Supply Pegging Analysis"
        Version = 1
        State =
            {
                Title = "Demand & Pegged Supply Orders"
                Components = Map.ofList [ (idDemand, compDemand); (idSupply, compSupply) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList
                            [
                                (idDemand, TilePosition.literal 1 1 12 4)
                                (idSupply, TilePosition.literal 1 5 12 4)
                            ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Demand-Supply Pegging Analysis" "Comparative Analysis" "link"
    }

let supplyWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-supply-workbench"
        Name = "Supply Orders Workbench"
        Version = 1
        State =
            {
                Title = "Supply Orders"
                Components = Map.ofList [ (idSupply, compSupply) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idSupply, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Supply Orders Workbench" "Supply Planning" "local_shipping"
    }

let peggingSplitView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-demand-supply-pegging-split"
        Name = "Demand-Supply Pegging (Split Pane)"
        Version = 1
        State =
            {
                Title = "Demand-Supply Pegging Studio & Horizon Planner"
                Components =
                    Map.ofList
                        [
                            (idPeggingScheduler, compPeggingScheduler)
                            (idSupply, compSupply)
                            (idDemand, compDemand)
                        ]
                Layout =
                    LayoutDefinition.Split(
                        SplitDirection.Vertical,
                        LayoutDefinition.Single idPeggingScheduler,
                        LayoutDefinition.Split(
                            SplitDirection.Horizontal,
                            LayoutDefinition.Single idSupply,
                            LayoutDefinition.Single idDemand,
                            SplitRatio.literal 0.5m
                        ),
                        SplitRatio.literal 0.42m
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Demand-Supply Pegging Studio" "Comparative Analysis" "splitscreen"
    }

let supplyControlTowerView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-supply-control-tower"
        Name = "Supply Control Tower"
        Version = 1
        State =
            {
                Title = "Supply Control Tower"
                Components =
                    Map.ofList
                        [
                            (idSupplyExceptions, compSupplyExceptions)
                            (idSupplyChanges, compSupplyChanges)
                            (idSupplierCollaboration, compSupplierCollaboration)
                            (idInventory, compInventory)
                        ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList
                            [
                                (idSupplyExceptions, TilePosition.literal 1 1 6 6)
                                (idSupplyChanges, TilePosition.literal 7 1 6 6)
                                (idSupplierCollaboration, TilePosition.literal 1 7 6 6)
                                (idInventory, TilePosition.literal 7 7 6 6)
                            ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Supply Control Tower" "Control Tower" "cell_tower"
    }

let aiCausalStudioView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-ai-causal-studio"
        Name = "AI Causal Reasoning & Explanation Studio"
        Version = 1
        State =
            {
                Title = "AI Causal Reasoning & Root Cause Studio"
                Components =
                    Map.ofList
                        [
                            (idSupplyExceptions, compSupplyExceptions)
                            (idSupplyExplanationStudio, compSupplyExplanationStudio)
                        ]
                Layout =
                    LayoutDefinition.Split(
                        SplitDirection.Horizontal,
                        LayoutDefinition.Single idSupplyExceptions,
                        LayoutDefinition.Single idSupplyExplanationStudio,
                        SplitRatio.literal 0.35m
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "AI Causal Studio" "AI Intelligence" "psychology"
    }

let inventoryWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-inventory-workbench"
        Name = "Inventory Health Workbench"
        Version = 1
        State =
            {
                Title = "Inventory Position Health"
                Components = Map.ofList [ (idInventory, compInventory) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idInventory, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Inventory Health Workbench" "Inventory Planning" "inventory"
    }

let supplyUnderstandingView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-supply-understanding"
        Name = "Supply Sensory Stream"
        Version = 1
        State =
            {
                Title = "Supply Sensory Stream"
                Components = Map.ofList [ (idSupplyChanges, compSupplyChanges) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idSupplyChanges, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Supply Sensory Stream" "Supply Intelligence" "sensors"
    }

let supplyQuadWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-supply-quad-workbench"
        Name = "Quad Supply Triangulation Workbench"
        Version = 1
        State =
            {
                Title = "4-Way Supply Chain Triangulation (Inventory, Capacity, Orders, Sensory)"
                Components =
                    Map.ofList
                        [
                            (idInventory, compInventory)
                            (idCapacity, compCapacity)
                            (idSupply, compSupply)
                            (idSupplyChanges, compSupplyChanges)
                        ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList
                            [
                                (idInventory, TilePosition.literal 1 1 6 6)
                                (idCapacity, TilePosition.literal 7 1 6 6)
                                (idSupply, TilePosition.literal 1 7 6 6)
                                (idSupplyChanges, TilePosition.literal 7 7 6 6)
                            ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Quad Supply Triangulation" "Supply Intelligence" "dashboard"
    }

let traceabilityWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-traceability-workbench"
        Name = "Traceability & Investigation Studio"
        Version = 1
        State =
            {
                Title = "End-to-End Traceability & Causal Investigation"
                Components = Map.ofList [ (idTraceabilityWorkbench, compTraceabilityWorkbench) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idTraceabilityWorkbench, TilePosition.literal 1 1 12 8) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Traceability & Investigation Studio" "Auditing & Traceability" "account_tree"
    }

let defaultViews: ViewDefinition list =
    [
        medhaviAssistantView
        executiveKpiDashboardView
        demandSupplyCapacityTripleView
        coreExceptionsWorkbenchView
        itemTransitionsWorkbenchView
        enterprisePictureWorkbenchView
        demandObservationsStreamView
        inventoryWorkbenchView
        capacityWorkbenchView
        capacityStudioView
        supplyWorkbenchView
        supplyUnderstandingView
        supplyQuadWorkbenchView
        supplyControlTowerView
        aiCausalStudioView
        peggingDashboardView
        peggingSplitView
        traceabilityWorkbenchView
    ]

