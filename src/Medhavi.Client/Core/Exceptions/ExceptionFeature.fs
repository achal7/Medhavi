module Medhavi.Client.Core.Exceptions.ExceptionFeature

open Medhavi.Client.Registries
open Medhavi.Client.ActionExecution
open Medhavi.Client
open Medhavi.Client.Core.Exceptions

/// Feature composition manifest for the Core Exceptions bounded context slice.
let manifest =
    {| Schemas = [ ExceptionGridSchema.definition ]
       Stores = [ (ExceptionStore.store.Id, ExceptionStore.store) ]
       Actions = ExceptionActions.all
       Components = [ ExceptionGrid.registration ] |}
