module Medhavi.Client.Demand.Segmentation.SegmentationFeature

open Medhavi.Client

let store =
    DataStore.fromQuery SegmentationGrid.storeId (fun ds -> ds.Demand.SegmentDemand.Queries) SegmentationGrid.adapter

/// Feature composition manifest for the Demand Segmentation slice.
let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = SegmentationActions.all
        Components = [ SegmentationGrid.registration ]
    }
