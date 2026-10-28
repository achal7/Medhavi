module Medhavi.Client.Core.ItemTransition.ItemTransitionFeature

open Medhavi.Client
open Medhavi.Client.Core.ItemTransition

let retire = ItemTransitionGrid.retire

let store =
    DataStore.fromQuery
        ItemTransitionGrid.storeId
        (fun ds -> ds.Core.ItemTransitionManagement.Queries)
        ItemTransitionGrid.adapter

/// Feature composition manifest for the Item Transition bounded context slice.
let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = ItemTransitionActions.all
        Components = [ ItemTransitionGrid.registration ]
    }
