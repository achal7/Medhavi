module Medhavi.Client.Supply.Procurement.ProcurementFeature

open Medhavi.Client

let store =
    DataStore.fromQuery
        ProcurementGrid.storeId
        (fun ds -> ds.Supply.ProcureMaterials.ProcurementRecommendationQueries)
        ProcurementGrid.adapter

let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = ProcurementActions.all
        Components = [ ProcurementGrid.registration ]
    }
