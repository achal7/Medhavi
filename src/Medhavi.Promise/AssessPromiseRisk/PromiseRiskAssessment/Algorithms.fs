/// CA-P-007 Assess Promise Risk and Resilience — Algorithms
/// Traces to: BA-P-004 Bayesian Breach Probability and Buffer Consumption Calculator
module Medhavi.Promise.AssessPromiseRisk.PromiseRiskAssessment.Algorithms

open System
open Medhavi.SemanticModel
open Medhavi.Promise
open Model
open Policies

let evaluateRiskMetrics
    (policy: PromiseRiskAssessmentPolicy)
    (committedDeliveryDate: Timestamp)
    (evaluationTime: Timestamp)
    (disruptionSignals: SupplyDisruptionSignalFact list)
    : decimal * decimal * decimal * PromiseRiskTier * string list * RiskCausalFactorRecord list =

    let totalDisruptionDays =
        if disruptionSignals.IsEmpty then 0.0m
        else disruptionSignals |> List.sumBy (fun s -> s.EstimatedDelayDays)

    let daysToDelivery =
        let span = (Timestamp.value committedDeliveryDate) - (Timestamp.value evaluationTime)
        Math.Max(0.1m, decimal span.TotalDays)

    // Projected late days is disruption that exceeds time remaining
    let projectedLateDays = Math.Max(0.0m, totalDisruptionDays - (daysToDelivery * 0.2m))
    let effectiveBufferDays = Math.Max(1.0m, daysToDelivery * 0.25m)
    let bufferConsumptionRatio = Math.Round(totalDisruptionDays / effectiveBufferDays, 2)

    // Bayesian breach probability calculation
    let rawProb =
        if totalDisruptionDays <= 0.0m then 5.0m
        elif totalDisruptionDays > effectiveBufferDays then
            let excess = totalDisruptionDays - effectiveBufferDays
            Math.Min(98.0m, 60.0m + (excess * 15.0m))
        else
            Math.Min(60.0m, 10.0m + ((totalDisruptionDays / effectiveBufferDays) * 50.0m))

    let breachProbabilityPct = Math.Round(Math.Clamp(rawProb, 0.0m, 100.0m), 2)

    let severityTier =
        if breachProbabilityPct >= policy.BreachProbabilityEscalationThresholdPct || bufferConsumptionRatio >= policy.BufferConsumptionCriticalRatio then
            PromiseRiskTier.Critical
        elif breachProbabilityPct >= policy.BreachProbabilityWarningThresholdPct || bufferConsumptionRatio >= policy.BufferConsumptionWarningRatio then
            PromiseRiskTier.High
        elif breachProbabilityPct >= 30.0m then
            PromiseRiskTier.Medium
        elif breachProbabilityPct >= 10.0m then
            PromiseRiskTier.Low
        else
            PromiseRiskTier.Negligible

    let riskDrivers =
        if disruptionSignals.IsEmpty then
            [ "Standard operational variability within baseline buffer" ]
        else
            disruptionSignals
            |> List.map (fun s -> $"Signal %s{s.SignalId}: %A{s.DisruptionType} (+%M{s.EstimatedDelayDays} days)")

    let causalFactors =
        if disruptionSignals.IsEmpty then
            [ { FactorRecordId = Guid.NewGuid().ToString("N").[0..7]
                FactorType = "BaselineOperationalTolerance"
                ContributionWeightPct = 100.0m
                EvidenceDescription = "Normal lead time variance absorbed by commitment safety buffer"
                DetectedTimestamp = evaluationTime } ]
        else
            let count = decimal disruptionSignals.Length
            let equalWeight = Math.Round(100.0m / count, 2)
            disruptionSignals
            |> List.mapi (fun idx s ->
                let weight = if idx = disruptionSignals.Length - 1 then 100.0m - (equalWeight * decimal (disruptionSignals.Length - 1)) else equalWeight
                { FactorRecordId = Guid.NewGuid().ToString("N").[0..7]
                  FactorType = sprintf "%A" s.DisruptionType
                  ContributionWeightPct = weight
                  EvidenceDescription = sprintf "Disruption signal %s with %M delay days" s.SignalId s.EstimatedDelayDays
                  DetectedTimestamp = s.DetectedAt })

    breachProbabilityPct, projectedLateDays, bufferConsumptionRatio, severityTier, riskDrivers, causalFactors

