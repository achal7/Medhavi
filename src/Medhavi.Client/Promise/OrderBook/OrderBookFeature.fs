module Medhavi.Client.Promise.OrderBook.OrderBookFeature

open Medhavi.Client
open Medhavi.Contracts.Promise

let store: DataStore =
    DataStore.fromQuery
        OrderBookGrid.storeId
        (fun ds -> ds.Promise.UnderstandOrders.OrderBookQueries)
        OrderBookGrid.adapter

let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = OrderBookActions.all
        Components = [ OrderBookGrid.registration ]
    }

