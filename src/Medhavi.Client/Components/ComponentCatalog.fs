module Medhavi.Client.Components.ComponentCatalog

open System
open Medhavi.Client
open Medhavi.Client.Registries
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.View
open Medhavi.Client.ViewComposition.Layout
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.Components.Core.Demand
open Medhavi.Client.Components.Core.Exceptions
open Medhavi.Client.Components.Core.ItemTransition
open Medhavi.Client.Components.Demand.Observations
open Medhavi.Client.Components.Demand.Understanding
open Medhavi.Client.Components.Supply
open Medhavi.Client.Components

    let allActions: ActionDescriptor list =
        DemandOrdersActions.all
        @ ExceptionActions.all
        @ ItemTransitionActions.all
        @ DemandObservationActions.all
        @ SupplyActions.all
        @ CapacityActions.all
        @ KpiSchema.all

    let tryFindAction (id: ActionDefId) : ActionDescriptor option =
        allActions |> List.tryFind(fun a -> a.Id = id)

    let components: ComponentRegistry =
        match ComponentRegistry.ofList
            [ MedhaviAssistant.registration
              DemandOrdersGrid.registration
              ExceptionGrid.registration
              ItemTransitionGrid.registration
              DemandObservationGrid.registration
              SupplyGrid.registration
              CapacityGrid.registration
              KpiSummaryGrid.registration
              DomainHealthRadar.registration ] with
        | Ok registry -> registry
        | Error error -> invalidArg "components" (sprintf "Invalid component catalog: %A" error)

    let schemas: SchemaRegistry =
        match SchemaRegistry.ofList
            [ MedhaviAssistant.schemaDefinition
              DemandOrdersSchema.definition
              ExceptionSchema.definition
              ItemTransitionSchema.definition
              DemandObservationSchema.definition
              SupplySchema.definition
              CapacitySchema.definition
              KpiSchema.definition ] with
        | Ok registry -> registry
        | Error error -> invalidArg "schemas" (sprintf "Invalid schema catalog: %A" error)

    let dataStores: DataStoreRegistry =
        Map.ofList
            [ DemandOrdersGrid.store.Id, DemandOrdersGrid.store
              ExceptionGrid.store.Id, ExceptionGrid.store
              ItemTransitionGrid.store.Id, ItemTransitionGrid.store
              DemandObservationGrid.store.Id, DemandObservationGrid.store
              SupplyGrid.store.Id, SupplyGrid.store
              CapacityGrid.store.Id, CapacityGrid.store ]

    let private defaultMetadata name category icon =
        { Category = category
          Icon = icon
          Roles = [ "Planner"; "Buyer"; "PlantManager"; "Executive" ]
          IsDefault = true
          IsTemplate = false
          CreatedAt = DateTimeOffset.UtcNow
          CreatedBy = "System"
          ModifiedAt = DateTimeOffset.UtcNow
          ModifiedBy = "System"
          Permissions =
            { Owner = "System"
              Visibility = ViewVisibility.Public
              EditableBy = [ "Planner"; "PlantManager" ]
              Shareable = true } }

    let private idMedhaviAssistant = ViewComponentId.literal "comp-medhavi-assistant"
    let private idDemand = ViewComponentId.literal "comp-demand-grid"
    let private idException = ViewComponentId.literal "comp-exception-grid"
    let private idItemTransition = ViewComponentId.literal "comp-item-transition-grid"
    let private idDemandObservation = ViewComponentId.literal "comp-demand-observation-grid"
    let private idSupply = ViewComponentId.literal "comp-supply-grid"
    let private idCapacity = ViewComponentId.literal "comp-capacity-grid"
    let private idKpiSummary = ViewComponentId.literal "comp-kpi-summary"
    let private idDomainRadar = ViewComponentId.literal "comp-domain-radar"

    let private compMedhaviAssistant: ViewComponent =
        { ComponentDefId = MedhaviAssistant.definitionId
          Binding =
            { StoreId = MedhaviAssistant.storeId
              SchemaDefId = MedhaviAssistant.schemaId }
          Configuration =
            { LocalFilters = []
              Parameters = Map.empty } }

    let private compDemand: ViewComponent =
        { ComponentDefId = DemandOrdersGrid.definitionId
          Binding =
            { StoreId = DemandOrdersGrid.storeId
              SchemaDefId = DemandOrdersSchema.schemaId }
          Configuration =
            { LocalFilters = []
              Parameters = Map.empty } }

    let private compException: ViewComponent =
        { ComponentDefId = ExceptionGrid.definitionId
          Binding =
            { StoreId = ExceptionGrid.storeId
              SchemaDefId = ExceptionSchema.schemaId }
          Configuration =
            { LocalFilters = []
              Parameters = Map.empty } }

    let private compItemTransition: ViewComponent =
        { ComponentDefId = ItemTransitionGrid.definitionId
          Binding =
            { StoreId = ItemTransitionGrid.storeId
              SchemaDefId = ItemTransitionSchema.schemaId }
          Configuration =
            { LocalFilters = []
              Parameters = Map.empty } }

    let private compDemandObservation: ViewComponent =
        { ComponentDefId = DemandObservationGrid.definitionId
          Binding =
            { StoreId = DemandObservationGrid.storeId
              SchemaDefId = DemandObservationSchema.schemaId }
          Configuration =
            { LocalFilters = []
              Parameters = Map.empty } }

    let private compSupply: ViewComponent =
        { ComponentDefId = SupplyGrid.definitionId
          Binding =
            { StoreId = SupplyGrid.storeId
              SchemaDefId = SupplySchema.schemaId }
          Configuration =
            { LocalFilters = []
              Parameters = Map.empty } }

    let private compCapacity: ViewComponent =
        { ComponentDefId = CapacityGrid.definitionId
          Binding =
            { StoreId = CapacityGrid.storeId
              SchemaDefId = CapacitySchema.schemaId }
          Configuration =
            { LocalFilters = []
              Parameters = Map.empty } }

    let private compKpiSummary: ViewComponent =
        { ComponentDefId = KpiSummaryGrid.definitionId
          Binding =
            { StoreId = KpiSummaryGrid.storeId
              SchemaDefId = KpiSchema.schemaId }
          Configuration =
            { LocalFilters = []
              Parameters = Map.empty } }

    let private compDomainRadar: ViewComponent =
        { ComponentDefId = DomainHealthRadar.definitionId
          Binding =
            { StoreId = DomainHealthRadar.storeId
              SchemaDefId = KpiSchema.schemaId }
          Configuration =
            { LocalFilters = []
              Parameters = Map.empty } }

    let medhaviAssistantView: ViewDefinition =
        { Id = ViewDefId.literal "view-medhavi-assistant"
          Name = "Medhavi Assistant"
          Version = 1
          State =
            { Title = "Medhavi Assistant — AI Strategy Studio"
              Components = Map.ofList [ (idMedhaviAssistant, compMedhaviAssistant) ]
              Layout =
                LayoutDefinition.Dashboard(
                    GridDimensions.defaultDimensions,
                    Map.ofList [ (idMedhaviAssistant, TilePosition.literal 1 1 12 12) ]
                )
              SharedContext = None }
          Metadata = defaultMetadata "Medhavi Assistant" "AI Intelligence" "auto_awesome" }

    let executiveKpiDashboardView: ViewDefinition =
        { Id = ViewDefId.literal "view-executive-kpi-dashboard"
          Name = "Executive KPI & Planning Dashboard"
          Version = 1
          State =
            { Title = "Executive Supply Chain Overview"
              Components =
                Map.ofList
                    [ (idKpiSummary, compKpiSummary)
                      (idDomainRadar, compDomainRadar)
                      (idDemand, compDemand)
                      (idCapacity, compCapacity) ]
              Layout =
                LayoutDefinition.Dashboard(
                    GridDimensions.defaultDimensions,
                    Map.ofList
                        [ (idKpiSummary, TilePosition.literal 1 1 12 3)
                          (idDomainRadar, TilePosition.literal 1 4 4 5)
                          (idDemand, TilePosition.literal 5 4 8 5)
                          (idCapacity, TilePosition.literal 1 9 12 4) ]
                )
              SharedContext = None }
          Metadata = defaultMetadata "Executive Overview" "Executive Dashboard" "dashboard" }

    let demandSupplyCapacityTripleView: ViewDefinition =
        { Id = ViewDefId.literal "view-demand-supply-capacity-triple"
          Name = "3-Way Demand-Supply-Capacity Analysis"
          Version = 1
          State =
            { Title = "3-Way Supply Chain Bottleneck Analysis"
              Components =
                Map.ofList
                    [ (idDemand, compDemand)
                      (idSupply, compSupply)
                      (idCapacity, compCapacity) ]
              Layout =
                LayoutDefinition.Dashboard(
                    GridDimensions.defaultDimensions,
                    Map.ofList
                        [ (idDemand, TilePosition.literal 1 1 12 4)
                          (idSupply, TilePosition.literal 1 5 12 4)
                          (idCapacity, TilePosition.literal 1 9 12 4) ]
                )
              SharedContext = None }
          Metadata = defaultMetadata "Demand-Supply-Capacity Triple" "Supply Intelligence" "account_tree" }

    let coreExceptionsWorkbenchView: ViewDefinition =
        { Id = ViewDefId.literal "view-core-exceptions"
          Name = "Core Exceptions Workbench"
          Version = 1
          State =
            { Title = "Core Constraint Exceptions"
              Components = Map.ofList [ (idException, compException) ]
              Layout =
                LayoutDefinition.Dashboard(
                    GridDimensions.defaultDimensions,
                    Map.ofList [ (idException, TilePosition.literal 1 1 12 6) ]
                )
              SharedContext = None }
          Metadata = defaultMetadata "Core Exceptions Workbench" "Core Management" "warning" }

    let demandObservationsStreamView: ViewDefinition =
        { Id = ViewDefId.literal "view-demand-observations"
          Name = "Demand Observations Stream"
          Version = 1
          State =
            { Title = "Demand Observations Sensory Stream"
              Components = Map.ofList [ (idDemandObservation, compDemandObservation) ]
              Layout =
                LayoutDefinition.Dashboard(
                    GridDimensions.defaultDimensions,
                    Map.ofList [ (idDemandObservation, TilePosition.literal 1 1 12 6) ]
                )
              SharedContext = None }
          Metadata = defaultMetadata "Demand Observations Stream" "Demand Intelligence" "sensors" }

    let capacityWorkbenchView: ViewDefinition =
        { Id = ViewDefId.literal "view-capacity-workbench"
          Name = "Work Center Capacity Workbench"
          Version = 1
          State =
            { Title = "Work Center Capacity & Load"
              Components = Map.ofList [ (idCapacity, compCapacity) ]
              Layout =
                LayoutDefinition.Dashboard(
                    GridDimensions.defaultDimensions,
                    Map.ofList [ (idCapacity, TilePosition.literal 1 1 12 6) ]
                )
              SharedContext = None }
          Metadata = defaultMetadata "Work Center Capacity Workbench" "Capacity Planning" "precision_manufacturing" }

    let peggingDashboardView: ViewDefinition =
        { Id = ViewDefId.literal "view-pegging-dashboard"
          Name = "Demand-Supply Pegging Analysis"
          Version = 1
          State =
            { Title = "Demand & Pegged Supply Orders"
              Components = Map.ofList [ (idDemand, compDemand); (idSupply, compSupply) ]
              Layout =
                LayoutDefinition.Dashboard(
                    GridDimensions.defaultDimensions,
                    Map.ofList
                        [ (idDemand, TilePosition.literal 1 1 12 4)
                          (idSupply, TilePosition.literal 1 5 12 4) ]
                )
              SharedContext = None }
          Metadata = defaultMetadata "Demand-Supply Pegging Analysis" "Comparative Analysis" "link" }

    let demandWorkbenchView: ViewDefinition =
        { Id = ViewDefId.literal "view-demand-workbench"
          Name = "Demand Orders Workbench"
          Version = 1
          State =
            { Title = "Demand Orders"
              Components = Map.ofList [ (idDemand, compDemand) ]
              Layout =
                LayoutDefinition.Dashboard(
                    GridDimensions.defaultDimensions,
                    Map.ofList [ (idDemand, TilePosition.literal 1 1 12 6) ]
                )
              SharedContext = None }
          Metadata = defaultMetadata "Demand Orders Workbench" "Demand Planning" "view_list" }

    let supplyWorkbenchView: ViewDefinition =
        { Id = ViewDefId.literal "view-supply-workbench"
          Name = "Supply Orders Workbench"
          Version = 1
          State =
            { Title = "Supply Orders"
              Components = Map.ofList [ (idSupply, compSupply) ]
              Layout =
                LayoutDefinition.Dashboard(
                    GridDimensions.defaultDimensions,
                    Map.ofList [ (idSupply, TilePosition.literal 1 1 12 6) ]
                )
              SharedContext = None }
          Metadata = defaultMetadata "Supply Orders Workbench" "Supply Planning" "local_shipping" }

    let peggingSplitView: ViewDefinition =
        { Id = ViewDefId.literal "view-demand-supply-pegging-split"
          Name = "Demand-Supply Pegging (Split Pane)"
          Version = 1
          State =
            { Title = "Demand & Pegged Supply"
              Components = Map.ofList [ (idDemand, compDemand); (idSupply, compSupply) ]
              Layout =
                LayoutDefinition.Split(
                    SplitDirection.Vertical,
                    LayoutDefinition.Single idDemand,
                    LayoutDefinition.Single idSupply,
                    SplitRatio.literal 0.5m
                )
              SharedContext = None }
          Metadata = defaultMetadata "Demand-Supply Pegging Analysis" "Comparative Analysis" "splitscreen" }

    let defaultViews: ViewDefinition list =
        [ medhaviAssistantView
          executiveKpiDashboardView
          demandSupplyCapacityTripleView
          coreExceptionsWorkbenchView
          demandObservationsStreamView
          capacityWorkbenchView
          peggingDashboardView
          demandWorkbenchView
          supplyWorkbenchView
          peggingSplitView ]
