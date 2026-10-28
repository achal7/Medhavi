module Medhavi.Client.Supply.Collaboration.SupplierCollaborationFeature

open Medhavi.Client

let store =
    DataStore.fromQuery
        SupplierCollaborationGrid.storeId
        (fun ds -> ds.Supply.CollaborateWithSuppliers.SupplierAssessmentQueries)
        SupplierCollaborationGrid.adapter

let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = SupplierCollaborationActions.all
        Components = [ SupplierCollaborationGrid.registration ]
    }
