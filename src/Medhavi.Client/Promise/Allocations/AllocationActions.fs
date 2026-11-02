module Medhavi.Client.Promise.Allocations.AllocationActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Promise
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client

let initializePool: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Promise.AllocateSupply.Allocation
            let poolId = $"POOL-DRV-AUTO-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1000L}"
            let req: InitializeAllocationPoolReq =
                { PoolId = poolId
                  PlanningScopeId = "PS-GLOBAL-SUPPLY-CHAIN"
                  Item = "SKU-DRIVE-1TB"
                  Location = "LOC-CENTRAL-DC"
                  PlanningPeriod = "2026-W46"
                  TotalPlannedSupply = 2000.0m
                  Buckets =
                    [ { BucketId = $"{poolId}-TIER-PLAT"
                        ChannelOrTier = "Platinum"
                        AllocatedQuantity = 800.0m
                        HardRingFence = true }
                      { BucketId = $"{poolId}-TIER-GOLD"
                        ChannelOrTier = "Gold"
                        AllocatedQuantity = 700.0m
                        HardRingFence = false }
                      { BucketId = $"{poolId}-TIER-STD"
                        ChannelOrTier = "Standard"
                        AllocatedQuantity = 500.0m
                        HardRingFence = false } ] }

            match! api.InitializePool req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Allocation pool '{dto.PoolId}' initialized with {dto.Buckets.Length} buckets.",
                            [ AllocationGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let rebalance: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some poolId ->
                let api = ctx.Domain.Promise.AllocateSupply.Allocation
                let req: RebalancePoolsReq =
                    { SourcePoolId = poolId
                      SourceBucketId = "BUCKET-GOLD"
                      TargetPoolId = Some poolId
                      TargetBucketId = Some "BUCKET-PLATINUM"
                      TransferQuantity = 100.0m
                      RebalanceReason = "Shift quota to cover surge in high-tier SLA demand"
                      RebalanceTime = DateTimeOffset.UtcNow }

                match! api.Rebalance req with
                | Ok dto ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Allocation pool '{dto.PoolId}' rebalanced: transferred 100.0 units to Platinum.",
                                [ AllocationGrid.storeId ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No allocation pool selected."
        }

let calculatePriority: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some poolId ->
                let api = ctx.Domain.Promise.AllocateSupply.Allocation
                let req: CalculateDynamicPriorityReq =
                    { OrderLineId = "LINE-TITAN-7001-01"
                      CustomerId = "CUST-TITAN-CLOUD"
                      Item = "SKU-DRIVE-1TB"
                      Location = "LOC-CENTRAL-DC"
                      RequestedQuantity = 50.0m
                      UnitSellingPrice = 185.0m
                      RequestedDeliveryDate = DateTimeOffset.UtcNow.AddDays(7.0)
                      OrderSubmissionTime = DateTimeOffset.UtcNow }

                match! api.CalculatePriority req with
                | Ok priority ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Order priority scored: Band '{priority.PriorityBand}', Score {priority.CompositePriorityScore:F2}, Rank #{priority.DynamicRank}.",
                                [ AllocationGrid.storeId ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No allocation pool selected."
        }

let all: (ActionDefId * ActionHandler) list =
    [ AllocationGrid.AllocationActions.initializePool.Id, initializePool
      AllocationGrid.AllocationActions.rebalance.Id, rebalance
      AllocationGrid.AllocationActions.calculatePriority.Id, calculatePriority ]

