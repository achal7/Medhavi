module Medhavi.Client.Supply.Explanation.SupplyExplanationFeature

open Medhavi.Client
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution

let manifest =
    {|
        Schemas = [ SupplyExplanationSchema.definition ]
        Stores = [ (SupplyExplanationStore.store.Id, SupplyExplanationStore.store) ]
        Actions = ([]: (ActionDefId * ActionHandler) list)
        Components = [ SupplyExplanationStudio.registration ]
    |}
