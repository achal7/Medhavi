module Medhavi.Client.Core.Traceability.TraceabilityFeature

open Medhavi.Client.Registries
open Medhavi.Client.ActionExecution
open Medhavi.Client
open Medhavi.Client.Core.Traceability

/// Feature composition manifest for the Traceability bounded context slice.
let manifest =
    {| Schemas = [ TraceabilityGridSchema.definition ]
       Stores = [ (TraceabilityStore.store.Id, TraceabilityStore.store) ]
       Actions = TraceabilityActions.all
       Components = [ TraceabilityWorkbench.registration ] |}
