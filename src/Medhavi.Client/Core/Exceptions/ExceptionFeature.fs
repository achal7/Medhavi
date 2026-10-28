module Medhavi.Client.Core.Exceptions.ExceptionFeature

open Medhavi.Client
open Medhavi.Client.Core.Exceptions

let store =
    DataStore.fromQuery ExceptionGrid.storeId (fun ds -> ds.Core.ExceptionManagement.Queries) ExceptionGrid.adapter

/// Feature composition manifest for the Core Exceptions bounded context slice.
let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = ExceptionActions.all
        Components = [ ExceptionGrid.registration ]
    }
