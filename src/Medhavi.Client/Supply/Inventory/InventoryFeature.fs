module Medhavi.Client.Supply.Inventory.InventoryFeature

open Medhavi.Client.Registries
open Medhavi.Client.ActionExecution
open Medhavi.Client

let manifest =
    {| Schemas = [ InventoryGridSchema.definition ]
       Stores = [ (InventoryStore.store.Id, InventoryStore.store) ]
       Actions = InventoryActions.all
       Components = [ InventoryGrid.registration ] |}
