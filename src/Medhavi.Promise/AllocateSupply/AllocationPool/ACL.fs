/// CA-P-002 Allocate Supply and Manage Priority ACL
module Medhavi.Promise.AllocateSupply.AllocationPool.ACL

open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let private toBucket (dto: InitBucketReq) : Validation<AllocationBucket, DomainError> =
    let create id qty =
        { BucketId = id
          ChannelOrTier = dto.ChannelOrTier
          AllocatedQuantity = qty
          ConsumedQuantity = Quantity.zero
          HardRingFence = dto.HardRingFence }

    create <!> validateAllocationBucketId dto.BucketId
           <*> validateQty dto.AllocatedQuantity

let toInitializeCmd (req: InitializeAllocationPoolReq) : Validation<InitializeAllocationPoolCmd, DomainError> =
    let validatedBuckets = req.Buckets |> List.map toBucket

    let create poolId scopeId item loc plannedSupply buckets =
        { PoolId = poolId
          PlanningScopeId = scopeId
          Item = item
          Location = loc
          PlanningPeriod = req.PlanningPeriod
          TotalPlannedSupply = plannedSupply
          Buckets = buckets }

    create <!> validateAllocationPoolId req.PoolId
           <*> validatePlanningScopeId req.PlanningScopeId
           <*> validateItemId req.Item
           <*> validateLocationId req.Location
           <*> validateQty req.TotalPlannedSupply
           <*> sequenceValidations validatedBuckets

let toDrawQuotaCmd (req: DrawAllocationQuotaReq) : Validation<DrawAllocationQuotaCmd, DomainError> =
    let create poolId bucketId lineId qty time =
        { PoolId = poolId
          BucketId = bucketId
          OrderLineId = lineId
          Quantity = qty
          DrawdownTime = time }

    create <!> validateAllocationPoolId req.PoolId
           <*> validateAllocationBucketId req.BucketId
           <*> validateOrderLineId req.OrderLineId
           <*> validateQty req.Quantity
           <*> validateTimestamp req.DrawdownTime

let toRebalanceCmd (req: RebalancePoolsReq) : Validation<RebalanceAllocationBucketsCmd, DomainError> =
    let validateOptionalPoolId (sOpt: string option) : Validation<AllocationPoolId option, DomainError> =
        match sOpt with
        | Some s -> Some <!> validateAllocationPoolId s
        | None -> Valid None

    let validateOptionalBucketId (sOpt: string option) : Validation<AllocationBucketId option, DomainError> =
        match sOpt with
        | Some s -> Some <!> validateAllocationBucketId s
        | None -> Valid None

    let create srcPool srcBucket tgtPool tgtBucket qty time =
        { SourcePoolId = srcPool
          SourceBucketId = srcBucket
          TargetPoolId = tgtPool
          TargetBucketId = tgtBucket
          TransferQuantity = qty
          RebalanceReason = req.RebalanceReason
          RebalanceTime = time }

    create <!> validateAllocationPoolId req.SourcePoolId
           <*> validateAllocationBucketId req.SourceBucketId
           <*> validateOptionalPoolId req.TargetPoolId
           <*> validateOptionalBucketId req.TargetBucketId
           <*> validateQty req.TransferQuantity
           <*> validateTimestamp req.RebalanceTime

let toCalculatePriorityCmd (req: CalculateDynamicPriorityReq) : Validation<CalculateDynamicPriorityCmd, DomainError> =
    let create lineId custId item loc qty reqDate subTime =
        { OrderLineId = lineId
          CustomerId = custId
          Item = item
          Location = loc
          RequestedQuantity = qty
          UnitSellingPrice = req.UnitSellingPrice
          RequestedDeliveryDate = reqDate
          OrderSubmissionTime = subTime }

    create <!> validateOrderLineId req.OrderLineId
           <*> validateCustomerId req.CustomerId
           <*> validateItemId req.Item
           <*> validateLocationId req.Location
           <*> validateQty req.RequestedQuantity
           <*> validateTimestamp req.RequestedDeliveryDate
           <*> validateTimestamp req.OrderSubmissionTime

let toArsEventId = function
    | AllocationPoolInitialized _ -> ArsIdentifiers.EnterpriseEvents.allocationBucketInitialized.Id
    | OrderPriorityCalculated _ -> ArsIdentifiers.EnterpriseEvents.orderPriorityCalculated.Id
    | AllocationQuotaConsumed _ -> ArsIdentifiers.EnterpriseEvents.allocationQuotaConsumed.Id
    | AllocationBucketsRebalanced _ -> ArsIdentifiers.EnterpriseEvents.allocationBucketsRebalanced.Id

