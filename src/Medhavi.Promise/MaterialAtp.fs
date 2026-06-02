module Medhavi.Promise.MaterialAtp

open System
open Medhavi.SharedKernel
open Medhavi.Promise.PromiseTypes

let createInMemoryMaterialProvider () : MaterialProvider =
    let availability = Map.empty<(SkuId * StockingPointId), decimal>

    { GetSnapshot =
        fun (skuId, stockingPointId, _asOf) ->
            async {
                match Map.tryFind (skuId, stockingPointId) availability with
                | Some onHand ->
                    let snap: MaterialSnapshot =
                        { SkuId = skuId
                          StockingPointId = stockingPointId
                          OnHand = onHand
                          Inbound = []
                          Reservations = 0m
                          Safety = 0m }
                    return Ok snap
                | None ->
                    return Error(ProviderError.Unavailable)
      }
      GetSupplierOptions =
        fun (_skuId, _stockingPointId, _qty, _asOf) ->
            async { return Ok [] } }