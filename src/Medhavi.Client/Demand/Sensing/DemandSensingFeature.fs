module Medhavi.Client.Demand.Sensing.DemandSensingFeature

open Medhavi.Client

let store =
    DataStore.fromQuery DemandSensingGrid.storeId (fun ds -> ds.Demand.SenseDemand.Queries) DemandSensingGrid.adapter

/// Feature composition manifest for the Demand Sensing slice.
let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = DemandSensingActions.all
        Components = [ DemandSensingGrid.registration ]
    }
