module Medhavi.Client.Demand.Understanding.DemandUnderstandingFeature

open Medhavi.Client.Registries
open Medhavi.Client.ActionExecution
open Medhavi.Client

let store =
    DataStore.fromQuery
        DomainHealthRadar.storeId
        (fun ds -> ds.Demand.UnderstandDemand.Understanding.Queries)
        DomainHealthRadar.adapter

/// Feature composition manifest for the Demand Understanding & Domain Health slice.
let manifest: FeatureManifest =
    { Stores = [ (store.Id, store) ]
      Actions = DomainHealthActions.all
      Components = [ DomainHealthRadar.registration ] }
