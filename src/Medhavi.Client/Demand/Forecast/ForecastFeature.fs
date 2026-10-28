module Medhavi.Client.Demand.Forecast.ForecastFeature

open Medhavi.Client

let store = DataStore.fromQuery ForecastGrid.storeId (fun ds -> ds.Demand.ForecastDemand.Queries) ForecastGrid.adapter

/// Feature composition manifest for the Forecast Publication slice.
let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = ForecastActions.all
        Components = [ ForecastGrid.registration ]
    }
