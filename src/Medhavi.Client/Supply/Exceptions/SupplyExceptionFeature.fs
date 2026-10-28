module Medhavi.Client.Supply.Exceptions.SupplyExceptionFeature

open Medhavi.Client

let store =
    DataStore.fromQuery
        SupplyExceptionGrid.storeId
        (fun ds -> ds.Supply.DetectSupplyExceptions.SupplyExceptionQueries)
        SupplyExceptionGrid.adapter

let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = SupplyExceptionActions.all
        Components = [ SupplyExceptionGrid.registration ]
    }
