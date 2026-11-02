module Medhavi.Client.Promise.CustomerProfiles.CustomerProfileFeature

open Medhavi.Client
open Medhavi.Contracts.Promise

let store: DataStore =
    DataStore.fromQuery
        CustomerProfileGrid.storeId
        (fun ds -> ds.Promise.UnderstandOrders.CustomerProfileQueries)
        CustomerProfileGrid.adapter

let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = CustomerProfileActions.all
        Components = [ CustomerProfileGrid.registration ]
    }

