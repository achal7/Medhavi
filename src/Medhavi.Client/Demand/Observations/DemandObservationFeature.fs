module Medhavi.Client.Demand.Observations.DemandObservationFeature

open Medhavi.Client

let store =
    DataStore.fromQuery
        DemandObservationGrid.storeId
        (fun ds -> ds.Demand.UnderstandDemand.Observation.Queries)
        DemandObservationGrid.adapter

/// Feature composition manifest for the Demand Observations slice.
let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = DemandObservationActions.all
        Components = [ DemandObservationGrid.registration ]
    }
