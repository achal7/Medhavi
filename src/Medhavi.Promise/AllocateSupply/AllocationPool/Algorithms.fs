/// CA-P-002 Allocate Supply and Manage Priority Algorithms
/// Traces to: BA-P-002 Dynamic Order Priority Scoring, BA-P-005 Allocation Quota Rationing
module Medhavi.Promise.AllocateSupply.AllocationPool.Algorithms

open System
open Medhavi.SemanticModel
open Medhavi.Promise
open Model
open Policies

/// BA-P-002: Dynamic Order Priority Scoring
let calculateCompositePriority
    (orderLineId: OrderLineId)
    (policy: DynamicOrderPriorityPolicy)
    (customerTier: string)
    (unitSellingPrice: decimal)
    (standardCogs: decimal)
    (dailyPenaltyRatePct: decimal)
    (orderSubmissionTime: Timestamp)
    (currentTime: Timestamp)
    : DynamicOrderPriority =

    // 1. Customer Tier Factor (0-100)
    let tierScore =
        match customerTier.Trim().ToLowerInvariant() with
        | "tier1" | "tier 1" | "strategic" | "platinum" -> 100.0m
        | "tier2" | "tier 2" | "preferred" | "gold" -> 75.0m
        | "tier3" | "tier 3" | "standard" | "silver" -> 50.0m
        | _ -> 25.0m

    // 2. Margin Contribution Factor (0-100)
    let marginPct =
        if unitSellingPrice > 0m && unitSellingPrice >= standardCogs then
            ((unitSellingPrice - standardCogs) / unitSellingPrice) * 100.0m
        else 0.0m
    let marginScore = Math.Clamp(marginPct, 0.0m, 100.0m)

    // 3. Contractual Penalty Factor (0-100)
    let penaltyScore = Math.Clamp(dailyPenaltyRatePct * 100.0m, 0.0m, 100.0m)

    // 4. Order Age Factor (0-100, max reached at 72 hours)
    let ageHours =
        let diff = (Timestamp.value currentTime) - (Timestamp.value orderSubmissionTime)
        Math.Max(0.0, diff.TotalHours) |> decimal
    let ageScore = Math.Clamp((ageHours / 72.0m) * 100.0m, 0.0m, 100.0m)

    // 5. Strategic Criticality Factor (0-100)
    let criticalityScore = if tierScore >= 75.0m then 90.0m else 40.0m

    // Weighted Composite Score
    let composite =
        (policy.WeightCustomerTier * tierScore)
        + (policy.WeightMarginContribution * marginScore)
        + (policy.WeightContractualPenalty * penaltyScore)
        + (policy.WeightOrderAge * ageScore)
        + (policy.WeightStrategicCriticality * criticalityScore)

    let normalizedScore = Math.Clamp(Math.Round(composite, 2), 0.0m, 100.0m)

    let band =
        if normalizedScore >= 80.0m then "Tier 1 - Strategic"
        elif normalizedScore >= 60.0m then "Tier 2 - Preferred"
        elif normalizedScore >= 40.0m then "Tier 3 - Standard"
        else "Tier 4 - Promotional"

    let rank = if normalizedScore >= 80.0m then 1 elif normalizedScore >= 60.0m then 2 elif normalizedScore >= 40.0m then 3 else 4

    { OrderLineId = orderLineId
      CustomerTierScore = tierScore
      MarginContributionScore = marginScore
      ContractualPenaltyScore = penaltyScore
      OrderAgeScore = ageScore
      StrategicCriticalityScore = criticalityScore
      CompositePriorityScore = normalizedScore
      DynamicRank = rank
      PriorityBand = band
      ScoredAt = currentTime }

/// BA-P-005: Constrained Resource Rationing & Quota Drawdown Check
let checkQuotaAvailability
    (govPolicy: AllocationGovernancePolicy)
    (bucket: AllocationBucket)
    (drawQuantity: Quantity)
    : bool * Quantity =

    let drawVal = Quantity.value drawQuantity
    let remainingVal = Quantity.value bucket.RemainingQuantity

    if remainingVal >= drawVal then
        true, drawQuantity
    elif not bucket.HardRingFence && govPolicy.AllowOverdraw then
        let maxOverdraw = Quantity.value bucket.AllocatedQuantity * (govPolicy.MaxOverdrawPercentage / 100.0m)
        let totalAllowed = remainingVal + maxOverdraw
        if totalAllowed >= drawVal then
            true, drawQuantity
        else
            false, Quantity.ofDecimal (Math.Max(0.0m, totalAllowed)) (Quantity.unitOfMeasure bucket.AllocatedQuantity)
    else
        false, bucket.RemainingQuantity
