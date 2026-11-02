/// CA-P-002 Allocate Supply and Manage Priority — Allocation Pool Model
/// Traces to: SE-P-020, SE-P-014, CR-P-005, CR-P-006, CR-P-007, CR-P-008
module Medhavi.Promise.AllocateSupply.AllocationPool.Model

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.ArsArtifact
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers

type AllocationPoolLifecycleState =
    | Draft
    | Active
    | Exhausted
    | Closed

type AllocationBucket =
    { BucketId: AllocationBucketId
      ChannelOrTier: string
      AllocatedQuantity: Quantity
      ConsumedQuantity: Quantity
      HardRingFence: bool }
    member this.RemainingQuantity =
        let rem = Quantity.value this.AllocatedQuantity - Quantity.value this.ConsumedQuantity
        let uom = Quantity.unitOfMeasure this.AllocatedQuantity
        if rem < 0m then Quantity.zeroWithUoM uom else Quantity.ofDecimal rem uom

type AllocationPool =
    { PoolId: AllocationPoolId
      PlanningScopeId: PlanningScopeId
      Item: ItemId
      Location: LocationId
      PlanningPeriod: string
      TotalPlannedSupply: Quantity
      Buckets: AllocationBucket list
      LifecycleState: AllocationPoolLifecycleState
      DecisionTraceability: string option }
    member this.TotalAllocatedQuantity =
        let sum = this.Buckets |> List.sumBy (fun b -> Quantity.value b.AllocatedQuantity)
        Quantity.ofDecimal sum (Quantity.unitOfMeasure this.TotalPlannedSupply)

type DynamicOrderPriority =
    { OrderLineId: OrderLineId
      CustomerTierScore: decimal
      MarginContributionScore: decimal
      ContractualPenaltyScore: decimal
      OrderAgeScore: decimal
      StrategicCriticalityScore: decimal
      CompositePriorityScore: decimal
      DynamicRank: int
      PriorityBand: string
      ScoredAt: Timestamp }

/// AB-P-004 Initialize Allocation Pool Command
type InitializeAllocationPoolCmd =
    { PoolId: AllocationPoolId
      PlanningScopeId: PlanningScopeId
      Item: ItemId
      Location: LocationId
      PlanningPeriod: string
      TotalPlannedSupply: Quantity
      Buckets: AllocationBucket list }

/// AB-P-012 Consume Allocation Quota Command
type DrawAllocationQuotaCmd =
    { PoolId: AllocationPoolId
      BucketId: AllocationBucketId
      OrderLineId: OrderLineId
      Quantity: Quantity
      DrawdownTime: Timestamp }

/// AB-P-013 Rebalance Allocation Buckets Command
type RebalanceAllocationBucketsCmd =
    { SourcePoolId: AllocationPoolId
      SourceBucketId: AllocationBucketId
      TargetPoolId: AllocationPoolId option
      TargetBucketId: AllocationBucketId option
      TransferQuantity: Quantity
      RebalanceReason: string
      RebalanceTime: Timestamp }

/// AB-P-011 Calculate Dynamic Order Priority Command
type CalculateDynamicPriorityCmd =
    { OrderLineId: OrderLineId
      CustomerId: CustomerId
      Item: ItemId
      Location: LocationId
      RequestedQuantity: Quantity
      UnitSellingPrice: decimal
      RequestedDeliveryDate: Timestamp
      OrderSubmissionTime: Timestamp }

type AllocationPoolCmd =
    | Initialize of InitializeAllocationPoolCmd
    | DrawQuota of DrawAllocationQuotaCmd
    | Rebalance of RebalanceAllocationBucketsCmd

    static member GetId(cmd: AllocationPoolCmd) =
        match cmd with
        | Initialize c -> c.PoolId
        | DrawQuota c -> c.PoolId
        | Rebalance c -> c.SourcePoolId

/// Domain Events emitted by Allocation Pool Aggregate
type AllocationPoolEvent =
    | AllocationPoolInitialized of AllocationPool
    | OrderPriorityCalculated of OrderLineId: OrderLineId * PriorityScore: decimal * Rank: int * Band: string * TraceId: string
    | AllocationQuotaConsumed of PoolId: AllocationPoolId * BucketId: AllocationBucketId * OrderLineId: OrderLineId * ConsumedQty: Quantity * RemainingQty: Quantity * TraceId: string
    | AllocationBucketsRebalanced of SourcePoolId: AllocationPoolId * SourceBucketId: AllocationBucketId * TargetPoolId: AllocationPoolId option * TargetBucketId: AllocationBucketId option * TransferQty: Quantity * Reason: string * TraceId: string

/// Pure state evolution (catamorphism)
let evolve: Evolve<AllocationPool, AllocationPoolEvent> =
    fun (state: AllocationPool option) (event: AllocationPoolEvent) ->
        match event with
        | AllocationPoolInitialized pool -> Some pool

        | OrderPriorityCalculated _ -> state

        | AllocationQuotaConsumed(poolId, bucketId, _, consumedQty, _, traceId) ->
            state
            |> Option.map (fun pool ->
                if pool.PoolId = poolId then
                    let updatedBuckets =
                        pool.Buckets
                        |> List.map (fun b ->
                            if b.BucketId = bucketId then
                                let newConsumed =
                                    let sum = Quantity.value b.ConsumedQuantity + Quantity.value consumedQty
                                    Quantity.ofDecimal sum (Quantity.unitOfMeasure b.ConsumedQuantity)
                                { b with ConsumedQuantity = newConsumed }
                            else b)
                    let allExhausted =
                        updatedBuckets
                        |> List.forall (fun b -> Quantity.value b.ConsumedQuantity >= Quantity.value b.AllocatedQuantity)
                    { pool with
                        Buckets = updatedBuckets
                        LifecycleState = if allExhausted then Exhausted else Active
                        DecisionTraceability = Some traceId }
                else pool)

        | AllocationBucketsRebalanced(sourcePoolId, sourceBucketId, targetPoolIdOpt, targetBucketIdOpt, transferQty, _, traceId) ->
            state
            |> Option.map (fun pool ->
                if pool.PoolId = sourcePoolId then
                    let isSamePoolTarget =
                        match targetPoolIdOpt with
                        | Some tPoolId -> tPoolId = sourcePoolId
                        | None -> true

                    let updatedBuckets =
                        pool.Buckets
                        |> List.map (fun b ->
                            if b.BucketId = sourceBucketId then
                                let rem = Quantity.value b.AllocatedQuantity - Quantity.value transferQty
                                let newAlloc = Quantity.ofDecimal (Math.Max(0m, rem)) (Quantity.unitOfMeasure b.AllocatedQuantity)
                                { b with AllocatedQuantity = newAlloc }
                            elif isSamePoolTarget && targetBucketIdOpt = Some b.BucketId then
                                let sum = Quantity.value b.AllocatedQuantity + Quantity.value transferQty
                                let newAlloc = Quantity.ofDecimal sum (Quantity.unitOfMeasure b.AllocatedQuantity)
                                { b with AllocatedQuantity = newAlloc }
                            else b)
                    { pool with
                        Buckets = updatedBuckets
                        DecisionTraceability = Some traceId }
                else pool)

