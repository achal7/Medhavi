/// BA-D-007 — Compute Planning Priority Score & Level
/// Pure mathematical multi-criteria scoring engine
module Medhavi.Demand.PrioritizeDemand.PlanningPriorityAssignment.Algorithms

open Policies
open Model

/// Normalize a raw value to a 0–100 scale
let private normalize (value: decimal) (minVal: decimal) (maxVal: decimal) : decimal =
    if maxVal <= minVal then
        50.0m
    elif value <= minVal then
        0.0m
    elif value >= maxVal then
        100.0m
    else
        ((value - minVal) / (maxVal - minVal)) * 100.0m

/// Compute priority score, level, dimension breakdown, and rationale (BA-D-007)
let computePriority
    (revenueContribution: decimal option)
    (strategicImportance: decimal option)
    (riskExposure: decimal option)
    (contractualObligation: decimal option)
    (policy: PrioritizationPolicy)
    : PriorityLevel * decimal * DimensionScoreBreakdown * string * string =

    let revScore =
        revenueContribution
        |> Option.map (fun v -> normalize v 0.0m policy.MaxRevenueBaseline)
        |> Option.defaultValue 0.0m

    let stratScore =
        strategicImportance
        |> Option.map (fun v -> (min (max v 0.0m) 10.0m) * 10.0m)
        |> Option.defaultValue 0.0m

    let riskScore =
        riskExposure
        |> Option.map (fun v -> (min (max v 0.0m) 10.0m) * 10.0m)
        |> Option.defaultValue 0.0m

    let contractualScore =
        contractualObligation
        |> Option.map (fun v -> (min (max v 0.0m) 10.0m) * 10.0m)
        |> Option.defaultValue 0.0m

    let breakdown: DimensionScoreBreakdown =
        { RevenueScore = revScore
          StrategyScore = stratScore
          RiskScore = riskScore
          ContractualScore = contractualScore }

    let total =
        (revScore * policy.RevenueWeight)
        + (stratScore * policy.StrategyWeight)
        + (riskScore * policy.RiskWeight)
        + (contractualScore * policy.ContractualWeight)

    let hasAnyEvidence =
        revenueContribution.IsSome
        || strategicImportance.IsSome
        || riskExposure.IsSome
        || contractualObligation.IsSome

    let level =
        if not hasAnyEvidence then
            Unclassified
        elif total >= policy.CriticalThreshold then
            Critical
        elif total >= policy.HighThreshold then
            High
        elif total >= policy.MediumThreshold then
            Medium
        else
            Low

    let rationale =
        if not hasAnyEvidence then
            "Priority Unclassified: No business dimension evidence provided"
        else
            sprintf
                "Priority %s assigned (Score: %.2f/100). Breakdown: Revenue=%.1f (wt %.2f), Strategy=%.1f (wt %.2f), Risk=%.1f (wt %.2f), Contract=%.1f (wt %.2f)"
                level.AsString
                total
                revScore
                policy.RevenueWeight
                stratScore
                policy.StrategyWeight
                riskScore
                policy.RiskWeight
                contractualScore
                policy.ContractualWeight

    let validity =
        match level with
        | Critical -> "Valid across all planning horizons; overrides normal operational buffer constraints"
        | High -> "Valid for standard weekly and monthly planning cycles; prioritized allocation"
        | Medium -> "Valid for standard replenishment and forecast review"
        | Low -> "Standard operational replenishment; lowest exception escalation precedence"
        | Unclassified -> "Validity pending business evidence submission"

    level, total, breakdown, rationale, validity
