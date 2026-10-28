module Medhavi.Client.Demand.Exceptions.DemandExceptionFeature

open Medhavi.Client

let store =
    DataStore.fromQuery
        DemandExceptionGrid.storeId
        (fun ds -> ds.Demand.DetectDemandExceptions.Queries)
        DemandExceptionGrid.adapter

/// Feature composition manifest for the Demand Exceptions slice.
let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = DemandExceptionActions.all
        Components = [ DemandExceptionGrid.registration ]
    }
