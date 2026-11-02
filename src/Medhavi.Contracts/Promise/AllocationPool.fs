namespace Medhavi.Contracts.Promise

open System
open System.Threading.Tasks
open Medhavi.Contracts

/// Granular allocation bucket within an allocation pool
type AllocationBucketDto =
    { BucketId: string
      ChannelOrTier: string
      AllocatedQuantity: decimal
      ConsumedQuantity: decimal
      RemainingQuantity: decimal
      HardRingFence: bool }

/// SE-P-020 Allocation Pool Data Transfer Object
type AllocationPoolDto =
    { PoolId: string
      PlanningScopeId: string
      Item: string
      Location: string
      PlanningPeriod: string
      TotalPlannedSupply: decimal
      TotalAllocatedQuantity: decimal
      Buckets: AllocationBucketDto list
      LifecycleState: string
      DecisionTraceability: string option }

/// SE-P-014 Dynamic Order Priority Data Transfer Object
type DynamicOrderPriorityDto =
    { OrderLineId: string
      CustomerTierScore: decimal
      MarginContributionScore: decimal
      ContractualPenaltyScore: decimal
      OrderAgeScore: decimal
      StrategicCriticalityScore: decimal
      CompositePriorityScore: decimal
      DynamicRank: int
      PriorityBand: string
      ScoredAt: DateTimeOffset }

/// External payload bucket configuration
type InitBucketReq =
    { BucketId: string
      ChannelOrTier: string
      AllocatedQuantity: decimal
      HardRingFence: bool }

/// External request to initialize an allocation pool
type InitializeAllocationPoolReq =
    { PoolId: string
      PlanningScopeId: string
      Item: string
      Location: string
      PlanningPeriod: string
      TotalPlannedSupply: decimal
      Buckets: InitBucketReq list }

/// External request to draw down allocation quota for an order
type DrawAllocationQuotaReq =
    { PoolId: string
      BucketId: string
      OrderLineId: string
      Quantity: decimal
      DrawdownTime: DateTimeOffset }

/// External request to transfer quota between channels or release surplus
type RebalancePoolsReq =
    { SourcePoolId: string
      SourceBucketId: string
      TargetPoolId: string option
      TargetBucketId: string option
      TransferQuantity: decimal
      RebalanceReason: string
      RebalanceTime: DateTimeOffset }

/// External request to calculate dynamic order priority
type CalculateDynamicPriorityReq =
    { OrderLineId: string
      CustomerId: string
      Item: string
      Location: string
      RequestedQuantity: decimal
      UnitSellingPrice: decimal
      RequestedDeliveryDate: DateTimeOffset
      OrderSubmissionTime: DateTimeOffset }

// ---------- Business Notifications ----------

/// BN-P-005: Allocation Bucket Established Notification
type AllocationBucketEstablishedNotification =
    { PoolId: string
      PlanningScopeId: string
      Item: string
      Location: string
      TotalAllocatedQuantity: decimal
      NotificationTime: DateTimeOffset }

/// BN-P-006: Allocation Quota Depleted Alert Notification
type AllocationQuotaDepletedNotification =
    { PoolId: string
      BucketId: string
      ChannelOrTier: string
      NotificationTime: DateTimeOffset }

/// BN-P-007: Allocation Rebalanced Notification
type AllocationRebalancedNotification =
    { SourcePoolId: string
      TargetPoolId: string option
      TransferQuantity: decimal
      RebalanceReason: string
      NotificationTime: DateTimeOffset }

/// Public API for Supply Allocation & Order Prioritization (CA-P-002)
type AllocationApi =
    { InitializePool: InitializeAllocationPoolReq -> Task<Result<AllocationPoolDto, ApiError>>
      DrawQuota: DrawAllocationQuotaReq -> Task<Result<AllocationPoolDto, ApiError>>
      Rebalance: RebalancePoolsReq -> Task<Result<AllocationPoolDto, ApiError>>
      CalculatePriority: CalculateDynamicPriorityReq -> Task<Result<DynamicOrderPriorityDto, ApiError>> }

/// Query service alias for Allocation Pools
type AllocationQueries = QueryService<AllocationPoolDto, string>

