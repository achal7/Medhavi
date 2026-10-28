module Medhavi.Client.Supply.Inventory.InventoryFeature

open Medhavi.Client

let store =
    DataStore.fromQuery
        InventoryGrid.storeId
        (fun ds -> ds.Supply.ManageInventory.InventoryPositionQueries)
        InventoryGrid.adapter

let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = InventoryActions.all
        Components = [ InventoryGrid.registration ]
    }
