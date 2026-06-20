module Medhavi.Web.Stores.StoreComposition

open Medhavi.Contracts.Scenario
open Medhavi.Contracts.Demand

let createRegistry (demandApi: DemandLineApi) (demandQueries: DemandLineQueries) (initialContext: PlanningContext) =
    // 1. Create the central context store (shared by all workspaces)
    let contextStore = PlanningContextStore.create initialContext

    // 2. Create the registry
    let registry = WorkspaceStoreRegistry.create contextStore

    // 3. Create and register individual workspace stores
    let demandStore, demandHandlers = DemandStore.create demandApi demandQueries initialContext
    registry.Register(WorkspaceKind.DemandWorkspace, demandStore)

    let supplyStore = SupplyStore.create initialContext
    registry.Register(WorkspaceKind.SupplyWorkspace, supplyStore)

    let capacityStore = CapacityStore.create initialContext
    registry.Register(WorkspaceKind.CapacityWorkspace, capacityStore)

    let materialReservationStore = MaterialReservationStore.create demandQueries initialContext
    registry.Register(WorkspaceKind.MaterialReservationWorkspace, materialReservationStore)

    // 4. Start the projection subscription layer, wiring notifications to store handlers
    let subscriptionLayer = ProjectionSubscription.create demandHandlers

    // Return both registry and subscription for cleanup if needed
    registry, subscriptionLayer
