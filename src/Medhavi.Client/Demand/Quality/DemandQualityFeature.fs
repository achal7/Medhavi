module Medhavi.Client.Demand.Quality.DemandQualityFeature

open Medhavi.Client

let store =
    DataStore.fromQuery
        DemandQualityGrid.storeId
        (fun ds -> ds.Demand.EvaluateDemandQuality.Queries)
        DemandQualityGrid.adapter

/// Feature composition manifest for the Forecast Quality Assessment slice.
let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = DemandQualityActions.all
        Components = [ DemandQualityGrid.registration ]
    }
