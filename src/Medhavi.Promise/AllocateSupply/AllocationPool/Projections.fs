/// CA-P-002 Allocate Supply and Manage Priority Projections
module Medhavi.Promise.AllocateSupply.AllocationPool.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let mapBucketToDto (b: AllocationBucket) : AllocationBucketDto =
    { BucketId = AllocationBucketId.value b.BucketId
      ChannelOrTier = b.ChannelOrTier
      AllocatedQuantity = Quantity.value b.AllocatedQuantity
      ConsumedQuantity = Quantity.value b.ConsumedQuantity
      RemainingQuantity = Quantity.value b.RemainingQuantity
      HardRingFence = b.HardRingFence }

let mapToDto (pool: AllocationPool) : AllocationPoolDto =
    { PoolId = AllocationPoolId.value pool.PoolId
      PlanningScopeId = PlanningScopeId.value pool.PlanningScopeId
      Item = ItemId.value pool.Item
      Location = LocationId.value pool.Location
      PlanningPeriod = pool.PlanningPeriod
      TotalPlannedSupply = Quantity.value pool.TotalPlannedSupply
      TotalAllocatedQuantity = Quantity.value pool.TotalAllocatedQuantity
      Buckets = pool.Buckets |> List.map mapBucketToDto
      LifecycleState =
        match pool.LifecycleState with
        | Draft -> "Draft"
        | Active -> "Active"
        | Exhausted -> "Exhausted"
        | Closed -> "Closed"
      DecisionTraceability = pool.DecisionTraceability }

let mapPriorityToDto (p: DynamicOrderPriority) : DynamicOrderPriorityDto =
    { OrderLineId = OrderLineId.value p.OrderLineId
      CustomerTierScore = p.CustomerTierScore
      MarginContributionScore = p.MarginContributionScore
      ContractualPenaltyScore = p.ContractualPenaltyScore
      OrderAgeScore = p.OrderAgeScore
      StrategicCriticalityScore = p.StrategicCriticalityScore
      CompositePriorityScore = p.CompositePriorityScore
      DynamicRank = p.DynamicRank
      PriorityBand = p.PriorityBand
      ScoredAt = Timestamp.value p.ScoredAt }

type State = Map<AllocationPoolId, AllocationPoolDto>

let initial: State = Map.empty

let apply (state: State) (event: AllocationPoolEvent) : State =
    match event with
    | AllocationPoolInitialized pool ->
        state |> Map.add pool.PoolId (mapToDto pool)

    | OrderPriorityCalculated _ ->
        state

    | AllocationQuotaConsumed(poolId, bucketId, _, consumedQty, remainingQty, traceId) ->
        state
        |> Map.change poolId (Option.map (fun dto ->
            let updatedBuckets =
                dto.Buckets
                |> List.map (fun b ->
                    if b.BucketId = AllocationBucketId.value bucketId then
                        { b with
                            ConsumedQuantity = b.ConsumedQuantity + Quantity.value consumedQty
                            RemainingQuantity = Quantity.value remainingQty }
                    else b)
            let allDepleted = updatedBuckets |> List.forall (fun b -> b.RemainingQuantity <= 0m)
            { dto with
                Buckets = updatedBuckets
                LifecycleState = if allDepleted then "Exhausted" else dto.LifecycleState
                DecisionTraceability = Some traceId }))

    | AllocationBucketsRebalanced(sourcePoolId, sourceBucketId, targetPoolIdOpt, targetBucketIdOpt, transferQty, _, traceId) ->
        state
        |> Map.change sourcePoolId (Option.map (fun dto ->
            let isSamePoolTarget =
                match targetPoolIdOpt with
                | Some tPoolId -> tPoolId = sourcePoolId
                | None -> true

            let updatedBuckets =
                dto.Buckets
                |> List.map (fun b ->
                    if b.BucketId = AllocationBucketId.value sourceBucketId then
                        let newAlloc = b.AllocatedQuantity - Quantity.value transferQty
                        let newRem = b.RemainingQuantity - Quantity.value transferQty
                        { b with AllocatedQuantity = newAlloc; RemainingQuantity = newRem }
                    elif isSamePoolTarget && targetBucketIdOpt |> Option.map AllocationBucketId.value = Some b.BucketId then
                        let newAlloc = b.AllocatedQuantity + Quantity.value transferQty
                        let newRem = b.RemainingQuantity + Quantity.value transferQty
                        { b with AllocatedQuantity = newAlloc; RemainingQuantity = newRem }
                    else b)
            { dto with
                Buckets = updatedBuckets
                DecisionTraceability = Some traceId }))

let seedFromAggregates (aggregates: AllocationPool list) : State =
    aggregates
    |> List.fold (fun state pool -> Map.add pool.PoolId (mapToDto pool) state) initial
