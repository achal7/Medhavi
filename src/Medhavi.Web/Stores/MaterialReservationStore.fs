namespace Medhavi.Web.Stores

open Medhavi.Contracts.Scenario
open Medhavi.Contracts.Supply
open Medhavi.Contracts.Demand
open Medhavi.Common.Patterns

type MaterialReservationData =
    { Demands: DemandLine list
      Reservations: MaterialReservation list
      Inventory: InventorySnapshot list }

module MaterialReservationStore =
    let create (demandQueries: DemandLineQueries) (initialContext: PlanningContext) =
        let loadFromBackend (context: PlanningContext) =
            taskResult {
                let! demands = demandQueries.GetAll()

                let data =
                    { Demands = demands
                      Reservations = []
                      Inventory = [] }

                return data
            }

        let store, _ = WorkspaceStore.create loadFromBackend initialContext None
        store
