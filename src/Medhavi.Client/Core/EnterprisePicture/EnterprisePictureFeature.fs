module Medhavi.Client.Core.EnterprisePicture.EnterprisePictureFeature

open Medhavi.Client.Registries
open Medhavi.Client.ActionExecution
open Medhavi.Client
open Medhavi.Client.Core.EnterprisePicture

/// Feature composition manifest for the Enterprise Picture bounded context slice.
let manifest =
    {| Schemas = [ EnterprisePictureGridSchema.definition ]
       Stores = [ (EnterprisePictureStore.store.Id, EnterprisePictureStore.store) ]
       Actions = EnterprisePictureActions.all
       Components = [ EnterprisePictureGrid.registration ] |}
