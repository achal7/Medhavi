module Medhavi.Client.Supply.ChangeSensing.SupplyChangeSensingFeature

open Medhavi.Client

let manifest =
    {|
        Schemas = [ SupplyChangeSensingGridSchema.definition ]
        Stores = [ (SupplyChangeSensingStore.store.Id, SupplyChangeSensingStore.store) ]
        Actions = SupplyChangeSensingActions.all
        Components = [ SupplyChangeSensingGrid.registration ]
    |}
