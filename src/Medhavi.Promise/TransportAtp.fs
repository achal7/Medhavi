module Medhavi.Promise.TransportAtp

open System
open Medhavi.SharedKernel
open Medhavi.Promise.PromiseTypes
open Medhavi.Transport

type TransportAvailabilityProvider =
    { GetLegs: string -> string -> Async<Result<Itinerary list, ProviderError>> }

let createInMemoryTransportProvider () : TransportProvider =
    { GetOptions =
        fun (origin, dest, _asOf) ->
            async {
                // Stub implementation returning a simple itinerary
                let itinerary =
                    { Id = ItineraryId.generate()
                      SkuId = None
                      FromNode = origin
                      ToNode = dest
                      Hops = []
                      TotalLeadTimeMinutes = 1440.0m // 1 day default
                      TotalFixedCost = 100m
                      TotalVariableCostPerUnit = None
                      TotalCO2 = None
                      TotalReliability = 0.9m
                      HopCount = 0 }

                return Ok [ itinerary ]
      } }

let calculateArrival (itinerary: Itinerary) (departure: DateTimeOffset) : DateTimeOffset =
    departure.AddMinutes(float itinerary.TotalLeadTimeMinutes)