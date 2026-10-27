module Medhavi.Client.Demand.Observations.DemandObservationFeature

open Medhavi.Client.Registries
open Medhavi.Client.ActionExecution
open Medhavi.Client

/// Feature composition manifest for the Demand Observations slice.
let manifest =
    {| Schemas = [ DemandObservationGridSchema.definition ]
       Stores = [ (DemandObservationStore.store.Id, DemandObservationStore.store) ]
       Actions = DemandObservationActions.all
       Components = [ DemandObservationGrid.registration ] |}
