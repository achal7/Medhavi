module Medhavi.Promise.CapacityAtp

open System
open Medhavi.SharedKernel
open Medhavi.Promise.PromiseTypes

let createInMemoryCapacityProvider () : CapacityProvider =
    let suggestedDate = DateTimeOffset.UtcNow.AddDays(7.0)
    let requiredLoads = Map.empty<string, decimal>
    { CheckCapacity =
        fun (_skuId, _qty, _asOf) ->
            async {
                let result: CapacityCheckResult =
                    { IsFeasible = true
                      SuggestedDate = suggestedDate
                      RequiredLoads = requiredLoads
                      BottleneckResourceId = None
                      LatenessReason = None
                      EarliestAvailable = DateTimeOffset.UtcNow }
                return Ok result
      } }

let wireCapacityProviderFromCheckCapacity
    (checkCapacity: SkuId * decimal * DateTimeOffset -> Async<Result<CapacityCheckResult, ProviderError>>)
    : CapacityProvider =
    { CheckCapacity = checkCapacity }