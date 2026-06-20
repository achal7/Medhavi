namespace Medhavi.Web.Stores

open System
open Medhavi.Contracts
open Medhavi.Contracts.Capacity
open Medhavi.Contracts.Scenario
open Medhavi.Common.Patterns

type CapacityData = { GanttData: GanttGrid option }

module CapacityStore =
    let create (initialContext: PlanningContext) =
        let loadFromBackend (context: PlanningContext) =
            taskResult {
                let data = { GanttData = None }
                return data
            }

        let store, _ = WorkspaceStore.create loadFromBackend initialContext None
        store
