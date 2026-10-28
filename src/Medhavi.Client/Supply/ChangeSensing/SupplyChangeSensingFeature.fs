module Medhavi.Client.Supply.ChangeSensing.SupplyChangeSensingFeature

open Medhavi.Client

let store =
    DataStore.fromQuery
        SupplyChangeSensingGrid.storeId
        (fun ds -> ds.Supply.SenseSupplyChanges.SupplyChangeQueries)
        SupplyChangeSensingGrid.adapter

let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = SupplyChangeSensingActions.all
        Components = [ SupplyChangeSensingGrid.registration ]
    }
