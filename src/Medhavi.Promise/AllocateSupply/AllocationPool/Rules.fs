/// CA-P-002 Allocate Supply and Manage Priority Rules
/// Traces to: BR-P-005, BR-P-024, BR-P-025, BR-P-052, BR-P-053, BR-P-080
module Medhavi.Promise.AllocateSupply.AllocationPool.Rules

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Policies

type InitializePoolInput =
    { Cmd: InitializeAllocationPoolCmd
      ScopeExists: bool
      ItemExists: bool
      LocationExists: bool
      Policy: AllocationGovernancePolicy }

type DrawQuotaInput =
    { Cmd: DrawAllocationQuotaCmd
      CurrentPool: AllocationPool option
      Policy: AllocationGovernancePolicy }

type RebalanceInput =
    { Cmd: RebalanceAllocationBucketsCmd
      CurrentPool: AllocationPool option
      GovPolicy: AllocationGovernancePolicy
      OverdrawPolicy: ChannelOverdrawPolicy }

type CalculatePriorityInput =
    { Cmd: CalculateDynamicPriorityCmd
      CustomerExists: bool
      ItemExists: bool
      Policy: DynamicOrderPriorityPolicy }

/// BR-P-005: Allocation pool prerequisites
let poolPrerequisitesRule: Rule<InitializePoolInput> =
    Rule.create
        Rules.allocationPoolIdentity.Id
        Rules.allocationPoolIdentity.Explanation
        (fun input -> input.ScopeExists && input.ItemExists && input.LocationExists)
        (fun input -> $"ScopeExists: %b{input.ScopeExists}, ItemExists: %b{input.ItemExists}, LocationExists: %b{input.LocationExists}")

/// BR-P-024: Sum of bucket allocations must not exceed planned supply
let supplyConservationRule: Rule<InitializePoolInput> =
    Rule.create
        Rules.allocationConservationInvariant.Id
        Rules.allocationConservationInvariant.Explanation
        (fun input ->
            let totalAllocated = input.Cmd.Buckets |> List.sumBy (fun b -> Quantity.value b.AllocatedQuantity)
            totalAllocated <= Quantity.value input.Cmd.TotalPlannedSupply)
        (fun input ->
            let totalAllocated = input.Cmd.Buckets |> List.sumBy (fun b -> Quantity.value b.AllocatedQuantity)
            $"TotalAllocated: %M{totalAllocated}, PlannedSupply: %M{Quantity.value input.Cmd.TotalPlannedSupply}")

let initPoolRules: Rule<InitializePoolInput> list =
    [ poolPrerequisitesRule; supplyConservationRule ]

/// BR-P-025: Quota drawdown availability
let quotaAvailabilityRule: Rule<DrawQuotaInput> =
    Rule.create
        Rules.allocationNonNegativityInvariant.Id
        Rules.allocationNonNegativityInvariant.Explanation
        (fun input ->
            match input.CurrentPool with
            | Some pool ->
                match pool.Buckets |> List.tryFind (fun b -> b.BucketId = input.Cmd.BucketId) with
                | Some bucket ->
                    let available, _ = Algorithms.checkQuotaAvailability input.Policy bucket input.Cmd.Quantity
                    available
                | None -> false
            | None -> false)
        (fun input ->
            match input.CurrentPool with
            | Some pool ->
                match pool.Buckets |> List.tryFind (fun b -> b.BucketId = input.Cmd.BucketId) with
                | Some b -> $"BucketId: %s{AllocationBucketId.value b.BucketId}, Remaining: %M{Quantity.value b.RemainingQuantity}, Requested: %M{Quantity.value input.Cmd.Quantity}"
                | None -> "BucketNotFound"
            | None -> "PoolNotFound")

let drawQuotaRules: Rule<DrawQuotaInput> list =
    [ quotaAvailabilityRule ]

/// BR-P-053: Hard ring fence protection against transfer
let hardRingFenceRule: Rule<RebalanceInput> =
    Rule.create
        Rules.quotaOverdrawProtectionInvariant.Id
        Rules.quotaOverdrawProtectionInvariant.Explanation
        (fun input ->
            match input.CurrentPool with
            | Some pool ->
                match pool.Buckets |> List.tryFind (fun b -> b.BucketId = input.Cmd.SourceBucketId) with
                | Some bucket -> not bucket.HardRingFence
                | None -> false
            | None -> false)
        (fun input -> $"SourceBucketId: %s{AllocationBucketId.value input.Cmd.SourceBucketId}")

/// BR-P-052: Inter-channel borrowing surplus reserve check
let surplusReserveRule: Rule<RebalanceInput> =
    Rule.create
        Rules.allocationRebalanceApproval.Id
        Rules.allocationRebalanceApproval.Explanation
        (fun input ->
            match input.CurrentPool with
            | Some pool ->
                match pool.Buckets |> List.tryFind (fun b -> b.BucketId = input.Cmd.SourceBucketId) with
                | Some bucket ->
                    let allocVal = Quantity.value bucket.AllocatedQuantity
                    let remVal = Quantity.value bucket.RemainingQuantity
                    let transferVal = Quantity.value input.Cmd.TransferQuantity
                    let minReserve = allocVal * (input.OverdrawPolicy.MinimumSurplusReservePercentage / 100.0m)
                    (remVal - transferVal) >= minReserve
                | None -> false
            | None -> false)
        (fun input -> $"TransferQty: %M{Quantity.value input.Cmd.TransferQuantity}")

let rebalanceRules: Rule<RebalanceInput> list =
    [ hardRingFenceRule; surplusReserveRule ]

/// BR-P-080: Dynamic order priority scoring prerequisites
let priorityPrerequisitesRule: Rule<CalculatePriorityInput> =
    Rule.create
        Rules.compositePriorityScoreDerivation.Id
        Rules.compositePriorityScoreDerivation.Explanation
        (fun input -> input.CustomerExists && input.ItemExists)
        (fun input -> $"CustomerExists: %b{input.CustomerExists}, ItemExists: %b{input.ItemExists}")

let calculatePriorityRules: Rule<CalculatePriorityInput> list =
    [ priorityPrerequisitesRule ]

