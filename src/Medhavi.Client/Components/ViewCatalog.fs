module Medhavi.Client.Components.ViewCatalog

open System
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.View
open Medhavi.Client.ViewComposition.Layout
open Medhavi.Client.ViewComposition.Geometry

// Feature slices: Core
open Medhavi.Client.Core.MasterData
open Medhavi.Client.Core.Demand
open Medhavi.Client.Core.Exceptions
open Medhavi.Client.Core.ItemTransition
open Medhavi.Client.Core.EnterprisePicture
open Medhavi.Client.Core.Traceability

// Feature slices: Demand
open Medhavi.Client.Demand.Observations
open Medhavi.Client.Demand.Understanding
open Medhavi.Client.Demand.Forecast
open Medhavi.Client.Demand.Sensing
open Medhavi.Client.Demand.Exceptions
open Medhavi.Client.Demand.Segmentation
open Medhavi.Client.Demand.Classification
open Medhavi.Client.Demand.Prioritization
open Medhavi.Client.Demand.Quality
open Medhavi.Client.Demand.Explanation
open Medhavi.Client.Demand.Learning
open Medhavi.Client.Demand.Interventions

// Feature slices: Supply
open Medhavi.Client.Supply.Inventory
open Medhavi.Client.Supply.Capacity
open Medhavi.Client.Supply.SupplyOrders
open Medhavi.Client.Supply.Exceptions
open Medhavi.Client.Supply.ChangeSensing
open Medhavi.Client.Supply.Collaboration
open Medhavi.Client.Supply.Explanation
open Medhavi.Client.Supply.Pegging
open Medhavi.Client.Supply.Procurement
open Medhavi.Client.Supply.Distribution
open Medhavi.Client.Supply.Quality
open Medhavi.Client.Supply.Learning

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
let private idProcurement = ViewComponentId.literal "comp-procurement-recommendations"
let private idDistribution = ViewComponentId.literal "comp-distribution-recommendations"
let private idSupplyQuality = ViewComponentId.literal "comp-supply-quality"
let private idSupplyLearning = ViewComponentId.literal "comp-supply-learning"
let private idMasterData = ViewComponentId.literal "comp-core-master-data"
let private idForecast = ViewComponentId.literal "comp-forecast-publication"
let private idDemandSensing = ViewComponentId.literal "comp-demand-sensing"
let private idDemandExceptions = ViewComponentId.literal "comp-demand-exceptions"
let private idSegmentation = ViewComponentId.literal "comp-demand-segmentation"
let private idClassification = ViewComponentId.literal "comp-demand-classification"
let private idPrioritization = ViewComponentId.literal "comp-demand-prioritization"
let private idDemandQuality = ViewComponentId.literal "comp-demand-quality"
let private idDemandExplanation = ViewComponentId.literal "comp-demand-explanation"
let private idDemandLearning = ViewComponentId.literal "comp-demand-learning"
let private idDemandInterventions = ViewComponentId.literal "comp-demand-interventions"

let private compForecast: ViewComponent =
    {
        ComponentDefId = ForecastGrid.definitionId
        Binding =
            {
                StoreId = ForecastGrid.storeId
                SchemaDefId = ForecastGrid.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Forecast Publications"
        Icon = Some "query_stats"
    }

let private compDemandSensing: ViewComponent =
    {
        ComponentDefId = DemandSensingGrid.definitionId
        Binding =
            {
                StoreId = DemandSensingGrid.storeId
                SchemaDefId = DemandSensingGrid.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Demand Sensing"
        Icon = Some "sensors"
    }

let private compDemandExceptions: ViewComponent =
    {
        ComponentDefId = DemandExceptionGrid.definitionId
        Binding =
            {
                StoreId = DemandExceptionGrid.storeId
                SchemaDefId = DemandExceptionGrid.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Demand Exceptions"
        Icon = Some "warning_amber"
    }

let private compSegmentation: ViewComponent =
    {
        ComponentDefId = SegmentationGrid.definitionId
        Binding =
            {
                StoreId = SegmentationGrid.storeId
                SchemaDefId = SegmentationGrid.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Demand Planning Segmentation (ABC/XYZ)"
        Icon = Some "category"
    }

let private compClassification: ViewComponent =
    {
        ComponentDefId = DemandBehaviorClassificationGrid.definitionId
        Binding =
            {
                StoreId = DemandBehaviorClassificationGrid.storeId
                SchemaDefId = DemandBehaviorClassificationGrid.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Demand Behavior Classification Studio"
        Icon = Some "auto_graph"
    }

let private compPrioritization: ViewComponent =
    {
        ComponentDefId = PrioritizationGrid.definitionId
        Binding =
            {
                StoreId = PrioritizationGrid.storeId
                SchemaDefId = PrioritizationGrid.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Demand Prioritization Workbench"
        Icon = Some "rule"
    }

let private compDemandQuality: ViewComponent =
    {
        ComponentDefId = DemandQualityGrid.definitionId
        Binding =
            {
                StoreId = DemandQualityGrid.storeId
                SchemaDefId = DemandQualityGrid.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Forecast Quality Studio"
        Icon = Some "verified"
    }

let private compDemandExplanation: ViewComponent =
    {
        ComponentDefId = DemandExplanationGrid.definitionId
        Binding =
            {
                StoreId = DemandExplanationGrid.storeId
                SchemaDefId = DemandExplanationGrid.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Demand Decision Explanations"
        Icon = Some "lightbulb"
    }

let private compDemandLearning: ViewComponent =
    {
        ComponentDefId = DemandLearningGrid.definitionId
        Binding =
            {
                StoreId = DemandLearningGrid.storeId
                SchemaDefId = DemandLearningGrid.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Demand Continuous Learning Studio"
        Icon = Some "school"
    }

let private compDemandInterventions: ViewComponent =
    {
        ComponentDefId = DemandInterventionGrid.definitionId
        Binding =
            {
                StoreId = DemandInterventionGrid.storeId
                SchemaDefId = DemandInterventionGrid.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Demand Intervention & Lift Studio"
        Icon = Some "transform"
    }

let private compMasterData: ViewComponent =
    {
        ComponentDefId = MasterDataGrid.definitionId
        Binding =
            {
                StoreId = MasterDataGrid.storeId
                SchemaDefId = MasterDataGrid.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Core Master Data"
        Icon = Some "inventory_2"
    }

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
                SchemaDefId = DemandOrdersGrid.schemaId
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
                StoreId = ExceptionGrid.storeId
                SchemaDefId = ExceptionGrid.schemaId
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
                StoreId = ItemTransitionGrid.storeId
                SchemaDefId = ItemTransitionGrid.schemaId
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
                StoreId = EnterprisePictureGrid.storeId
                SchemaDefId = EnterprisePictureGrid.schemaId
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
                StoreId = TraceabilityWorkbench.storeId
                SchemaDefId = TraceabilityWorkbench.schemaId
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
                StoreId = DemandObservationGrid.storeId
                SchemaDefId = DemandObservationGrid.schemaId
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
                StoreId = InventoryGrid.storeId
                SchemaDefId = InventoryGrid.schemaId
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
                StoreId = CapacityGrid.storeId
                SchemaDefId = CapacityGrid.schemaId
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
                StoreId = CapacityGrid.storeId
                SchemaDefId = CapacityGrid.schemaId
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
                StoreId = SupplyGrid.storeId
                SchemaDefId = SupplyGrid.schemaId
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
                StoreId = DomainHealthRadar.storeId
                SchemaDefId = DomainHealthRadar.schemaId
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
                StoreId = SupplyExceptionGrid.storeId
                SchemaDefId = SupplyExceptionGrid.schemaId
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
                StoreId = SupplyChangeSensingGrid.storeId
                SchemaDefId = SupplyChangeSensingGrid.schemaId
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
                StoreId = SupplierCollaborationGrid.storeId
                SchemaDefId = SupplierCollaborationGrid.schemaId
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
                StoreId = SupplyExplanationStudio.storeId
                SchemaDefId = SupplyExplanationStudio.schemaId
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
                StoreId = PeggingScheduler.storeId
                SchemaDefId = PeggingScheduler.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Demand-Supply Pegging Horizon"
        Icon = Some "alt_route"
    }

let private compProcurement: ViewComponent =
    {
        ComponentDefId = ProcurementGrid.definitionId
        Binding =
            {
                StoreId = ProcurementGrid.storeId
                SchemaDefId = ProcurementGrid.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Procurement Recommendations"
        Icon = Some "shopping_cart"
    }

let private compDistribution: ViewComponent =
    {
        ComponentDefId = DistributionGrid.definitionId
        Binding =
            {
                StoreId = DistributionGrid.storeId
                SchemaDefId = DistributionGrid.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Distribution Recommendations"
        Icon = Some "local_shipping"
    }

let private compSupplyQuality: ViewComponent =
    {
        ComponentDefId = SupplyQualityGrid.definitionId
        Binding =
            {
                StoreId = SupplyQualityGrid.storeId
                SchemaDefId = SupplyQualityGrid.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Supply Quality Assessments"
        Icon = Some "verified"
    }

let private compSupplyLearning: ViewComponent =
    {
        ComponentDefId = SupplyLearningGrid.definitionId
        Binding =
            {
                StoreId = SupplyLearningGrid.storeId
                SchemaDefId = SupplyLearningGrid.schemaId
            }
        Configuration =
            {
                LocalFilters = []
                Parameters = Map.empty
            }
        Title = Some "Supply Learning Insights"
        Icon = Some "school"
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

let procurementWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-procurement-workbench"
        Name = "Procurement Recommendations Workbench"
        Version = 1
        State =
            {
                Title = "Procurement Recommendations (CA-S-006)"
                Components = Map.ofList [ (idProcurement, compProcurement) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idProcurement, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Procurement Recommendations" "Procurement Planning" "shopping_cart"
    }

let distributionWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-distribution-workbench"
        Name = "Distribution Network Workbench"
        Version = 1
        State =
            {
                Title = "Distribution Network Recommendations (CA-S-008)"
                Components = Map.ofList [ (idDistribution, compDistribution) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idDistribution, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Distribution Network Workbench" "Distribution Planning" "local_shipping"
    }

let supplyQualityWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-supply-quality-workbench"
        Name = "Supply Quality Assessment Workbench"
        Version = 1
        State =
            {
                Title = "Supply Quality & Operational Alignment (CA-S-010)"
                Components = Map.ofList [ (idSupplyQuality, compSupplyQuality) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idSupplyQuality, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Supply Quality Workbench" "Quality & Governance" "verified"
    }

let supplyLearningWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-supply-learning-workbench"
        Name = "Supply Learning & Retrospectives"
        Version = 1
        State =
            {
                Title = "Supply Learning & Multi-Period Retrospectives (CA-S-013)"
                Components = Map.ofList [ (idSupplyLearning, compSupplyLearning) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idSupplyLearning, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Supply Learning Workbench" "Learning & Adaptation" "school"
    }

let coreMasterDataWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-core-master-data-workbench"
        Name = "Core Master Data Catalog"
        Version = 1
        State =
            {
                Title = "Core Items & Locations Master Catalog"
                Components = Map.ofList [ (idMasterData, compMasterData) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idMasterData, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Core Master Data Catalog" "Master Data" "inventory_2"
    }

let forecastWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-forecast-workbench"
        Name = "Forecast Publication Studio"
        Version = 1
        State =
            {
                Title = "Forecast Publication Studio (CA-D-002)"
                Components = Map.ofList [ (idForecast, compForecast) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idForecast, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Forecast Publication Studio" "Demand Forecasting" "query_stats"
    }

let demandSensingWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-demand-sensing-workbench"
        Name = "Demand Sensing Studio"
        Version = 1
        State =
            {
                Title = "Demand Sensing & Signal Intelligence (CA-D-003)"
                Components = Map.ofList [ (idDemandSensing, compDemandSensing) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idDemandSensing, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Demand Sensing Studio" "Demand Sensing" "sensors"
    }

let demandExceptionsWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-demand-exceptions-workbench"
        Name = "Demand Exceptions Workbench"
        Version = 1
        State =
            {
                Title = "Demand Exceptions & Pattern Detection (CA-D-008)"
                Components = Map.ofList [ (idDemandExceptions, compDemandExceptions) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idDemandExceptions, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Demand Exceptions Workbench" "Exception Management" "warning_amber"
    }

let segmentationWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-demand-segmentation"
        Name = "Demand Planning Segmentation Workbench (ABC/XYZ)"
        Version = 1
        State =
            {
                Title = "Demand Planning Segmentation Workbench (ABC/XYZ)"
                Components = Map.ofList [ (idSegmentation, compSegmentation) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idSegmentation, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Demand Segmentation Workbench" "Demand Planning" "category"
    }

let classificationWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-demand-classification"
        Name = "Demand Behavior Classification Studio"
        Version = 1
        State =
            {
                Title = "Demand Behavior Classification Studio"
                Components = Map.ofList [ (idClassification, compClassification) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idClassification, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Demand Behavior Classification" "Demand Planning" "auto_graph"
    }

let prioritizationWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-demand-prioritization"
        Name = "Demand Planning Priority Workbench"
        Version = 1
        State =
            {
                Title = "Demand Planning Priority Workbench"
                Components = Map.ofList [ (idPrioritization, compPrioritization) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idPrioritization, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Demand Prioritization Workbench" "Demand Planning" "rule"
    }

let demandQualityWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-demand-quality"
        Name = "Forecast Quality Studio"
        Version = 1
        State =
            {
                Title = "Forecast Quality & Accuracy Studio (CA-D-007)"
                Components = Map.ofList [ (idDemandQuality, compDemandQuality) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idDemandQuality, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Forecast Quality Studio" "Demand Forecasting" "verified"
    }

let demandExplanationWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-demand-explanation"
        Name = "Demand Decision Explanations Studio"
        Version = 1
        State =
            {
                Title = "Demand Decision Explanations & Factor Attribution (CA-D-009)"
                Components = Map.ofList [ (idDemandExplanation, compDemandExplanation) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idDemandExplanation, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Demand Explanations Studio" "Demand Planning" "lightbulb"
    }

let demandLearningWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-demand-learning"
        Name = "Demand Continuous Learning Workbench"
        Version = 1
        State =
            {
                Title = "Demand Continuous Learning & Bias Insights (CA-D-010)"
                Components = Map.ofList [ (idDemandLearning, compDemandLearning) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idDemandLearning, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Demand Continuous Learning" "Demand Planning" "school"
    }

let demandInterventionWorkbenchView: ViewDefinition =
    {
        Id = ViewDefId.literal "view-demand-interventions"
        Name = "Demand Intervention & Lift Studio"
        Version = 1
        State =
            {
                Title = "Demand Intervention & Scenario Lift Modeling (CA-D-011)"
                Components = Map.ofList [ (idDemandInterventions, compDemandInterventions) ]
                Layout =
                    LayoutDefinition.Dashboard(
                        GridDimensions.defaultDimensions,
                        Map.ofList [ (idDemandInterventions, TilePosition.literal 1 1 12 6) ]
                    )
                SharedContext = None
            }
        Metadata = defaultMetadata "Demand Intervention Studio" "Demand Planning" "transform"
    }

let defaultViews: ViewDefinition list =
    [
        medhaviAssistantView
        executiveKpiDashboardView
        coreMasterDataWorkbenchView
        demandSupplyCapacityTripleView
        coreExceptionsWorkbenchView
        itemTransitionsWorkbenchView
        enterprisePictureWorkbenchView
        demandObservationsStreamView
        forecastWorkbenchView
        demandSensingWorkbenchView
        demandExceptionsWorkbenchView
        segmentationWorkbenchView
        classificationWorkbenchView
        prioritizationWorkbenchView
        demandQualityWorkbenchView
        demandExplanationWorkbenchView
        demandLearningWorkbenchView
        demandInterventionWorkbenchView
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
        procurementWorkbenchView
        distributionWorkbenchView
        supplyQualityWorkbenchView
        supplyLearningWorkbenchView
    ]
