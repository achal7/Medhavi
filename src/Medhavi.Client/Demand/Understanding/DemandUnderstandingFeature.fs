module Medhavi.Client.Demand.Understanding.DemandUnderstandingFeature

open Medhavi.Client.Registries
open Medhavi.Client.ActionExecution
open Medhavi.Client

/// Feature composition manifest for the Demand Understanding & Domain Health slice.
let manifest =
    {| Schemas = [ DomainHealthSchema.definition ]
       Stores = [ (DomainHealthStore.store.Id, DomainHealthStore.store) ]
       Actions = DomainHealthActions.all
       Components = [ DomainHealthRadar.registration ] |}
