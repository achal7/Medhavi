module Medhavi.Client.Supply.SupplyOrders.SupplyOrderFeature

open Medhavi.Client.Registries
open Medhavi.Client.ActionExecution
open Medhavi.Client

let manifest =
    {| Schemas = [ SupplyOrderGridSchema.definition ]
       Stores = [ (SupplyOrderStore.store.Id, SupplyOrderStore.store) ]
       Actions = SupplyOrderActions.all
       Components = [ SupplyGrid.registration ] |}
