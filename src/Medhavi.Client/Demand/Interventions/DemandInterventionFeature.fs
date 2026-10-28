module Medhavi.Client.Demand.Interventions.DemandInterventionFeature

open Medhavi.Client

let store =
    DataStore.fromQuery
        DemandInterventionGrid.storeId
        (fun ds -> ds.Demand.ModelDemandInterventions.Queries)
        DemandInterventionGrid.adapter

/// Feature composition manifest for the Demand Intervention & Lift Modeling slice.
let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = DemandInterventionActions.all
        Components = [ DemandInterventionGrid.registration ]
    }
