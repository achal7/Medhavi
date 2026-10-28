module Medhavi.Client.Demand.Prioritization.PrioritizationFeature

open Medhavi.Client

let store =
    DataStore.fromQuery
        PrioritizationGrid.storeId
        (fun ds -> ds.Demand.PrioritizeDemand.Queries)
        PrioritizationGrid.adapter

/// Feature composition manifest for the Demand Prioritization slice.
let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = PrioritizationActions.all
        Components = [ PrioritizationGrid.registration ]
    }
