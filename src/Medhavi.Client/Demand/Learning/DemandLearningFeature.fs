module Medhavi.Client.Demand.Learning.DemandLearningFeature

open Medhavi.Client

let store =
    DataStore.fromQuery
        DemandLearningGrid.storeId
        (fun ds -> ds.Demand.LearnFromDemand.Queries)
        DemandLearningGrid.adapter

/// Feature composition manifest for the Demand Learning slice.
let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = DemandLearningActions.all
        Components = [ DemandLearningGrid.registration ]
    }
