module Medhavi.Client.Supply.Distribution.DistributionFeature

open Medhavi.Client

let store =
    DataStore.fromQuery
        DistributionGrid.storeId
        (fun ds -> ds.Supply.ManageDistribution.DistributionRecommendationQueries)
        DistributionGrid.adapter

let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = DistributionActions.all
        Components = [ DistributionGrid.registration ]
    }
