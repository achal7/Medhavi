module Medhavi.Client.Supply.Collaboration.SupplierCollaborationFeature

open Medhavi.Client

let manifest =
    {|
        Schemas = [ SupplierCollaborationGridSchema.definition ]
        Stores = [ (SupplierCollaborationStore.store.Id, SupplierCollaborationStore.store) ]
        Actions = SupplierCollaborationActions.all
        Components = [ SupplierCollaborationGrid.registration ]
    |}
