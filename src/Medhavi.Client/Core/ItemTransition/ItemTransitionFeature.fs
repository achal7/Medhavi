module Medhavi.Client.Core.ItemTransition.ItemTransitionFeature

open Medhavi.Client.Registries
open Medhavi.Client.ActionExecution
open Medhavi.Client
open Medhavi.Client.Core.ItemTransition

/// Feature composition manifest for the Item Transition bounded context slice.
let manifest =
    {| Schemas = [ ItemTransitionGridSchema.definition ]
       Stores = [ (ItemTransitionStore.store.Id, ItemTransitionStore.store) ]
       Actions = ItemTransitionActions.all
       Components = [ ItemTransitionGrid.registration ] |}
