namespace Medhavi.Web.Stores

open Medhavi.Contracts
open Medhavi.Contracts.Supply
open Medhavi.Contracts.Scenario
open Medhavi.Common.Patterns

type SupplyData =
    { SupplyOrders: SupplyElementView list
      Inventory: InventorySnapshot list
      MaterialReservations: MaterialReservation list
    // Add other derived data/indexes
    }

module SupplyStore =
    let create (initialContext: PlanningContext) =
        let loadFromBackend (context: PlanningContext) =
            taskResult {
                // In real implementation, load from supply service
                let data =
                    { SupplyOrders = []
                      Inventory = []
                      MaterialReservations = [] }

                return data
            }

        let store, _ = WorkspaceStore.create loadFromBackend initialContext None
        store
