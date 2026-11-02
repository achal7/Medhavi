module Medhavi.Client.Promise.Allocations.AllocationFeature

open Medhavi.Client
open Medhavi.Contracts.Promise

let store: DataStore =
    DataStore.fromQuery
        AllocationGrid.storeId
        (fun ds -> ds.Promise.AllocateSupply.AllocationQueries)
        AllocationGrid.adapter

let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = AllocationActions.all
        Components = [ AllocationGrid.registration ]
    }

