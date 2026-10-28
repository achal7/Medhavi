module Medhavi.Client.Supply.Explanation.SupplyExplanationFeature

open Medhavi.Client

let store =
    DataStore.fromQuery
        SupplyExplanationStudio.storeId
        (fun ds -> ds.Supply.ExplainSupplyDecisions.SupplyExplanationQueries)
        SupplyExplanationStudio.adapter

let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = []
        Components = [ SupplyExplanationStudio.registration ]
    }
