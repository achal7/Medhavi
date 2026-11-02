module Medhavi.Client.Promise.Backorders.BackorderFeature

open Medhavi.Client
open Medhavi.Contracts.Promise

let store: DataStore =
    DataStore.fromQuery
        BackorderGrid.storeId
        (fun ds -> ds.Promise.ManageBackorders.BackorderQueries)
        BackorderGrid.adapter

let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = BackorderActions.all
        Components = [ BackorderGrid.registration ]
    }

