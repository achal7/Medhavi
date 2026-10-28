module Medhavi.Client.Supply.Learning.SupplyLearningFeature

open Medhavi.Client

let store =
    DataStore.fromQuery
        SupplyLearningGrid.storeId
        (fun ds -> ds.Supply.LearnFromSupply.SupplyLearningQueries)
        SupplyLearningGrid.adapter

let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = SupplyLearningActions.all
        Components = [ SupplyLearningGrid.registration ]
    }
