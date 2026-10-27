module Medhavi.Client.Supply.Capacity.CapacityFeature

open Medhavi.Client.Registries
open Medhavi.Client.ActionExecution
open Medhavi.Client

let manifest =
    {| Schemas = [ CapacityGridSchema.definition ]
       Stores = [ (CapacityStore.store.Id, CapacityStore.store) ]
       Actions = CapacityActions.all
       Components = [ CapacityGrid.registration; CapacityStudio.registration ] |}
