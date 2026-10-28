module Medhavi.Client.Demand.Explanation.DemandExplanationFeature

open Medhavi.Client

let store =
    DataStore.fromQuery
        DemandExplanationGrid.storeId
        (fun ds -> ds.Demand.ExplainDemand.Queries)
        DemandExplanationGrid.adapter

/// Feature composition manifest for the Demand Decision Explanation slice.
let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = DemandExplanationActions.all
        Components = [ DemandExplanationGrid.registration ]
    }
