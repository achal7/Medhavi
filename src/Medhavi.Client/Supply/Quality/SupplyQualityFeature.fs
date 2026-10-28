module Medhavi.Client.Supply.Quality.SupplyQualityFeature

open Medhavi.Client

let store =
    DataStore.fromQuery
        SupplyQualityGrid.storeId
        (fun ds -> ds.Supply.EvaluateSupplyQuality.SupplyQualityQueries)
        SupplyQualityGrid.adapter

let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = SupplyQualityActions.all
        Components = [ SupplyQualityGrid.registration ]
    }
