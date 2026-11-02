/// CA-P-002 Allocate Supply and Manage Priority Policies
/// Traces to: PO-P-004, PO-P-005, PO-P-006
module Medhavi.Promise.AllocateSupply.AllocationPool.Policies

open Medhavi.Foundation.Contracts

type AllocationGovernancePolicy =
    { AllowOverdraw: bool
      MaxOverdrawPercentage: decimal
      AllowInterChannelBorrowing: bool
      RequireHardRingFenceForTier1: bool
      SurplusReclamationWindowDays: int }

type ChannelOverdrawPolicy =
    { MaxBorrowingLimitPercentage: decimal
      MinimumSurplusReservePercentage: decimal }

type DynamicOrderPriorityPolicy =
    { WeightCustomerTier: decimal
      WeightMarginContribution: decimal
      WeightContractualPenalty: decimal
      WeightOrderAge: decimal
      WeightStrategicCriticality: decimal }

let defaultAllocationGovernancePolicy: AllocationGovernancePolicy =
    { AllowOverdraw = false
      MaxOverdrawPercentage = 10.0m
      AllowInterChannelBorrowing = true
      RequireHardRingFenceForTier1 = true
      SurplusReclamationWindowDays = 3 }

let defaultChannelOverdrawPolicy: ChannelOverdrawPolicy =
    { MaxBorrowingLimitPercentage = 25.0m
      MinimumSurplusReservePercentage = 15.0m }

let defaultDynamicPriorityPolicy: DynamicOrderPriorityPolicy =
    { WeightCustomerTier = 0.30m
      WeightMarginContribution = 0.25m
      WeightContractualPenalty = 0.20m
      WeightOrderAge = 0.15m
      WeightStrategicCriticality = 0.10m }

