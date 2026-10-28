module Medhavi.Client.Core.Demand.DemandFeature

open Medhavi.Client
open Medhavi.Client.Core.Demand

let store =
    DataStore.fromQuery DemandOrdersGrid.storeId (fun ds -> ds.Core.DemandManagement.Queries) DemandOrdersGrid.adapter

/// Feature composition manifest for the Demand Orders bounded context slice.
let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = DemandActions.all
        Components = [ DemandOrdersGrid.registration ]
    }
