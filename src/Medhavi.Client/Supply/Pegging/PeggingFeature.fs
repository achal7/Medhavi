module Medhavi.Client.Supply.Pegging.PeggingFeature

open Medhavi.Client.Registries
open Medhavi.Client.ActionExecution
open Medhavi.Client

let manifest =
    {| Schemas = [ PeggingSchedulerSchema.definition ]
       Stores = [ (PeggingSchedulerStore.store.Id, PeggingSchedulerStore.store) ]
       Actions = PeggingSchedulerActions.all
       Components = [ PeggingScheduler.registration ] |}
