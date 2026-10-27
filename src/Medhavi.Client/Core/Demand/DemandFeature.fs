module Medhavi.Client.Core.Demand.DemandFeature

open Medhavi.Client

/// Feature composition manifest for the Demand Orders bounded context slice.
let manifest =
    {|
        Schemas = [ DemandGridSchema.definition; DemandKpiSchema.definition ]
        Stores = [ (DemandStore.store.Id, DemandStore.store) ]
        Actions = DemandActions.all
        Components = [ DemandOrdersGrid.registration ]
    |}
