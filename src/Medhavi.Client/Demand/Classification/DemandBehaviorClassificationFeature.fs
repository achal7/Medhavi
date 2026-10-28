module Medhavi.Client.Demand.Classification.DemandBehaviorClassificationFeature

open Medhavi.Client

let store =
    DataStore.fromQuery
        DemandBehaviorClassificationGrid.storeId
        (fun ds -> ds.Demand.ClassifyDemand.Queries)
        DemandBehaviorClassificationGrid.adapter

/// Feature composition manifest for the Demand Behavior Pattern Classification slice.
let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = DemandBehaviorClassificationActions.all
        Components = [ DemandBehaviorClassificationGrid.registration ]
    }
