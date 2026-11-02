module Medhavi.Client.Promise.Risk.PromiseRiskFeature

open Medhavi.Client
open Medhavi.Contracts.Promise

let store: DataStore =
    DataStore.fromQuery
        PromiseRiskGrid.storeId
        (fun ds -> ds.Promise.AssessPromiseRisk.RiskQueries)
        PromiseRiskGrid.adapter

let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = PromiseRiskActions.all
        Components = [ PromiseRiskGrid.registration ]
    }

