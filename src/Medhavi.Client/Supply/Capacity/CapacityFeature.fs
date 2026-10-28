module Medhavi.Client.Supply.Capacity.CapacityFeature

open Medhavi.Client

let store =
    DataStore.fromQuery
        CapacityGrid.storeId
        (fun ds -> ds.Supply.ManageCapacity.CapacityPositionQueries)
        CapacityGrid.adapter

let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = CapacityActions.all
        Components = [ CapacityGrid.registration; CapacityStudio.registration ]
    }
