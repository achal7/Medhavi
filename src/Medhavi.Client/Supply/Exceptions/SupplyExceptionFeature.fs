module Medhavi.Client.Supply.Exceptions.SupplyExceptionFeature

open Medhavi.Client.Registries
open Medhavi.Client.ActionExecution
open Medhavi.Client

let manifest =
    {| Schemas = [ SupplyExceptionGridSchema.definition ]
       Stores = [ (SupplyExceptionStore.store.Id, SupplyExceptionStore.store) ]
       Actions = SupplyExceptionActions.all
       Components = [ SupplyExceptionGrid.registration ] |}
