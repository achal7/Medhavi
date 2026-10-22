/// SE-S-032 — Inventory Health Algorithms
/// Traces to: BA-S-004 (Evaluate Inventory Risk), BA-S-005 (Determine Inventory Health)
module Medhavi.Supply.ManageInventory.InventoryHealth.Algorithms

open System
open Medhavi.Supply.ManageInventory
open Policies
open Model

/// BA-S-004: Evaluate Inventory Risk across 5 orthogonal dimensions
let evaluateInventoryRisk (input: RiskEvaluationInput) : MultiDimensionalRiskAssessment =
    let available = input.OnHand + input.InTransit - input.Allocated

    // 1. Stockout Exposure
    let stockoutExposure =
        if available < input.SafetyStock then
            let deficit = max 0m (input.SafetyStock - available)
            let impact = if available <= 0m then Severe else Major

            {
                Likelihood = VeryHigh
                Impact = impact
                MagnitudeUnits = deficit
                ValueCurrency = deficit * input.UnitCost
                ReasonCodes = [ "BelowSafetyStock"; "BelowReorderPoint" ]
            }
        elif available < input.ReorderPoint then
            let deficit = max 0m (input.ReorderPoint - available)

            {
                Likelihood = Likelihood.Medium
                Impact = Impact.Minor
                MagnitudeUnits = deficit
                ValueCurrency = deficit * input.UnitCost
                ReasonCodes = [ "BelowReorderPoint" ]
            }
        else
            {
                Likelihood = Likelihood.VeryLow
                Impact = Impact.Negligible
                MagnitudeUnits = 0m
                ValueCurrency = 0m
                ReasonCodes = []
            }

    // 2. Excess Inventory Exposure
    let tsl =
        if input.TargetStockLevel > 0m then
            input.TargetStockLevel
        else
            input.ReorderPoint * 1.5m

    let excessExposure =
        if available > tsl then
            let surplus = available - tsl
            let impact = if surplus > tsl then Impact.Major else Impact.Moderate

            {
                Likelihood = Likelihood.High
                Impact = impact
                MagnitudeUnits = surplus
                ValueCurrency = surplus * input.UnitCost
                ReasonCodes = [ "ExcessAboveTargetStock"; "HoldingCapitalBurden" ]
            }
        else
            {
                Likelihood = Likelihood.VeryLow
                Impact = Impact.Negligible
                MagnitudeUnits = 0m
                ValueCurrency = 0m
                ReasonCodes = []
            }

    // 3. Obsolescence Exposure
    let obsolescenceExposure =
        match input.RemainingForecastDemand with
        | Some remDem when input.OnHand > remDem ->
            let excessLife = input.OnHand - remDem

            {
                Likelihood = Likelihood.High
                Impact = Impact.Major
                MagnitudeUnits = excessLife
                ValueCurrency = excessLife * input.UnitCost
                ReasonCodes = [ "DecliningLifecycleDemand" ]
            }
        | _ ->
            match input.ShelfLifeRemainingDays with
            | Some shelf when shelf < input.LeadTimeDays ->
                {
                    Likelihood = Likelihood.VeryHigh
                    Impact = Impact.Major
                    MagnitudeUnits = input.OnHand
                    ValueCurrency = input.OnHand * input.UnitCost
                    ReasonCodes = [ "NearShelfLifeExpiry" ]
                }
            | _ ->
                {
                    Likelihood = Likelihood.Low
                    Impact = Impact.Negligible
                    MagnitudeUnits = 0m
                    ValueCurrency = 0m
                    ReasonCodes = []
                }

    // 4. Policy Conformance
    let delta = if tsl > 0m then Math.Abs(available - tsl) / tsl else 0m

    let policyConformance =
        if available < input.SafetyStock then
            {
                Likelihood = Likelihood.VeryHigh
                Impact = Impact.Major
                MagnitudeUnits = max 0m (input.SafetyStock - available)
                ValueCurrency = 0m
                ReasonCodes = [ "PolicyBandBreach"; "BelowSafetyStock" ]
            }
        elif delta > 0.20m then
            {
                Likelihood = Likelihood.Medium
                Impact = Impact.Moderate
                MagnitudeUnits = Math.Abs(available - tsl)
                ValueCurrency = 0m
                ReasonCodes = [ "PolicyBandBreach" ]
            }
        else
            {
                Likelihood = Likelihood.VeryLow
                Impact = Impact.Negligible
                MagnitudeUnits = 0m
                ValueCurrency = 0m
                ReasonCodes = []
            }

    // 5. Supply Dependency Exposure
    let supplyDependency =
        if input.IsSingleSource && input.SupplierReliability < 0.90m then
            {
                Likelihood = Likelihood.High
                Impact = Impact.Severe
                MagnitudeUnits = available
                ValueCurrency = available * input.UnitCost
                ReasonCodes = [ "SingleSourceDependency"; "UnreliableSupplierCommitment" ]
            }
        elif input.IsSingleSource then
            {
                Likelihood = Likelihood.Medium
                Impact = Impact.Minor
                MagnitudeUnits = available
                ValueCurrency = available * input.UnitCost
                ReasonCodes = [ "SingleSourceDependency" ]
            }
        else
            {
                Likelihood = Likelihood.Low
                Impact = Impact.Negligible
                MagnitudeUnits = 0m
                ValueCurrency = 0m
                ReasonCodes = []
            }

    let confidence =
        if input.RemainingForecastDemand.IsNone || input.ShelfLifeRemainingDays.IsNone then
            0.80m
        else
            0.95m

    {
        StockoutExposure = stockoutExposure
        ExcessExposure = excessExposure
        ObsolescenceExposure = obsolescenceExposure
        PolicyConformance = policyConformance
        SupplyDependencyExposure = supplyDependency
        OverallConfidence = confidence
    }

/// BA-S-005: Determine Inventory Health via strict policy precedence cascade
let determineInventoryHealth
    (_: InventoryHealthAssessmentPolicy)
    (risk: MultiDimensionalRiskAssessment)
    : InventoryHealthStatus * string =

    // Precedence 1: Obsolete
    if
        (risk.ObsolescenceExposure.Likelihood = Likelihood.High
         || risk.ObsolescenceExposure.Likelihood = Likelihood.VeryHigh)
        && (risk.ObsolescenceExposure.Impact = Impact.Moderate
            || risk.ObsolescenceExposure.Impact = Impact.Major
            || risk.ObsolescenceExposure.Impact = Impact.Severe)
    then
        InventoryHealthStatus.Obsolete, "ObsolescenceThresholdExceeded"

    // Precedence 2: At Risk
    elif
        (risk.StockoutExposure.Likelihood = Likelihood.High || risk.StockoutExposure.Likelihood = Likelihood.VeryHigh)
        && (risk.StockoutExposure.Impact = Major || risk.StockoutExposure.Impact = Severe)
    then
        InventoryHealthStatus.AtRisk, "SevereStockoutRisk"

    elif risk.SupplyDependencyExposure.Impact = Impact.Severe then
        InventoryHealthStatus.AtRisk, "CriticalSingleSourceDependency"

    // Precedence 3: Under-Stocked
    elif
        (risk.StockoutExposure.Likelihood = Likelihood.Medium
         || risk.StockoutExposure.Likelihood = Likelihood.High
         || risk.StockoutExposure.Likelihood = Likelihood.VeryHigh)
        && (risk.PolicyConformance.Impact = Impact.Major || risk.PolicyConformance.Impact = Impact.Severe)
    then
        InventoryHealthStatus.UnderStocked, "SafetyStockBreached"

    // Precedence 4: Over-Stocked
    elif
        (risk.ExcessExposure.Likelihood = Likelihood.Medium
         || risk.ExcessExposure.Likelihood = Likelihood.High
         || risk.ExcessExposure.Likelihood = Likelihood.VeryHigh)
        && (risk.ExcessExposure.Impact = Impact.Moderate
            || risk.ExcessExposure.Impact = Impact.Major
            || risk.ExcessExposure.Impact = Impact.Severe)
    then
        InventoryHealthStatus.OverStocked, "TargetStockLevelExceeded"

    // Precedence 5: Optimal
    else
        InventoryHealthStatus.Optimal, "OperatingWithinGovernedBounds"
