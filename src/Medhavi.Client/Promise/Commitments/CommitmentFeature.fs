module Medhavi.Client.Promise.Commitments.CommitmentFeature

open Medhavi.Client
open Medhavi.Contracts.Promise

let store: DataStore =
    DataStore.fromQuery
        CommitmentGrid.storeId
        (fun ds -> ds.Promise.EstablishCommitments.CommitmentQueries)
        CommitmentGrid.adapter

let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = CommitmentActions.all
        Components = [ CommitmentGrid.registration ]
    }

