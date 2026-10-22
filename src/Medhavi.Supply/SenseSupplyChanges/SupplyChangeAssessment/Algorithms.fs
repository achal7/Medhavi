/// BA-S-060 — Assess Supply Change Business Algorithm
/// Traces to: Supply Intelligence Specification §10 (BA-S-060)
module Medhavi.Supply.SenseSupplyChanges.SupplyChangeAssessment.Algorithms

open System
open Medhavi.SemanticModel
open Model
open Policies

type SupplyChangeAssessmentResult =
    {
        AssessmentId: string
        EntityReference: string
        ChangeDescription: string
        Materiality: PlanningMateriality
        Confidence: AssessmentConfidence
        DaysVariance: decimal
        QuantityVariance: decimal
        QuantityVariancePercent: decimal
        ThreatenedObjectives: string list
        ReasonCodes: string list
        AssessmentTimestamp: Timestamp
        SuggestedEpistemicState: ChangeEpistemicState
    }

/// BA-S-060: Pure 5-stage algorithm assessing supply change
let assessSupplyChange
    (signal: IncomingSupplySignal)
    (baseline: BaselineEntityFact)
    (policy: SupplyChangePolicyConfiguration)
    (corroboratingSignalsCount: int)
    (evaluationTimestamp: Timestamp)
    : SupplyChangeAssessmentResult =

    // Stage 1: Ingestion & Baseline Alignment (Enforce Preconditions)
    let boundedReliability = Math.Max(0.0m, Math.Min(1.0m, signal.SourceReliabilityPrior))

    // Stage 2: Variance & Shift Delta Quantification
    // Inbound timing difference in days: positive indicates delay, negative indicates early arrival
    let rawDays =
        (Timestamp.value signal.ObservedDate - Timestamp.value baseline.CommittedDeliveryDate).TotalDays |> decimal

    let daysVariance = Math.Round(rawDays, 2)

    // Volume delta: positive indicates shortfall, negative indicates surplus
    let qtyDelta = baseline.CommittedQuantity - signal.ObservedQuantity
    let quantityVariance = Math.Round(qtyDelta, 2)

    let quantityVariancePercent =
        if baseline.CommittedQuantity > 0.0m then
            Math.Round((Math.Abs(qtyDelta) / baseline.CommittedQuantity) * 100.0m, 2)
        else
            100.0m

    // Stage 3: Governed Materiality Testing & Gating (PO-S-100)
    let delayPred =
        daysVariance > policy.DelayThresholdDays
        || (baseline.IsCriticalPath && daysVariance > Math.Max(0.5m, baseline.BufferDays))

    let earlyPred = daysVariance < -policy.EarlyThresholdDays

    let qtyDeficitPred = qtyDelta > 0.0m && quantityVariancePercent > policy.QuantityVarianceThresholdPercent

    let qtySurplusPred = qtyDelta < 0.0m && quantityVariancePercent > (2.0m * policy.QuantityVarianceThresholdPercent)

    let isMaterial = delayPred || earlyPred || qtyDeficitPred || qtySurplusPred

    let materiality =
        if isMaterial then
            PlanningMateriality.MaterialToPlanning
        else
            PlanningMateriality.NonMaterial

    // Stage 4: Bayesian Evidence Confidence Derivation
    let kCorrob = Math.Max(0, corroboratingSignalsCount)
    let credibilityFactor = 1.0 - Math.Exp(-(float(kCorrob + 1)) / 2.0)
    let lambda = (float boundedReliability) * credibilityFactor |> decimal

    let confidence =
        if lambda >= 0.75m && kCorrob >= 1 && boundedReliability >= 0.85m then
            AssessmentConfidence.High
        elif (lambda >= 0.40m && lambda < 0.75m) || (kCorrob = 0 && boundedReliability >= 0.90m) then
            AssessmentConfidence.Medium
        else
            AssessmentConfidence.Low

    let suggestedEpistemicState =
        if kCorrob >= policy.CorroborationMinimumRequired then
            ChangeEpistemicState.Corroborated
        else
            ChangeEpistemicState.Observed

    // Stage 5: Objective Threat Mapping & Packaging
    let threatenedObjectives =
        [
            if delayPred then
                yield "On-Time Delivery (OTIF)"
                yield "Schedule Adherence"
            if qtyDeficitPred then
                yield "Material Availability"
                yield "Customer Order Fulfillment"
            if earlyPred || qtySurplusPred then
                yield "Warehouse Yard Capacity"
                yield "Working Capital Ceiling"
        ]
        |> List.distinct

    let reasonCodes =
        [
            if delayPred then
                yield "DeliveryDelayExceedsThreshold"
            if baseline.IsCriticalPath && daysVariance > 0.5m then
                yield "CriticalPathMaterialDelay"
            if earlyPred then
                yield "EarlyDeliveryViolatesWindow"
            if qtyDeficitPred then
                yield "QuantityReductionMaterial"
            if qtySurplusPred then
                yield "QuantitySurplusMaterial"
            if kCorrob = 0 then
                yield "SingleSourceEvidence"
            else
                yield "CorroboratedByMultipleSources"
            if not isMaterial then
                yield "NegligibleOperationalVariance"
        ]
        |> List.distinct

    let desc =
        sprintf
            "Entity: %s | DaysVariance: %.2f days | QtyVariance: %.2f (%.2f%%) | Materiality: %A | Confidence: %A"
            signal.EntityReference
            daysVariance
            quantityVariance
            quantityVariancePercent
            materiality
            confidence

    {
        AssessmentId = sprintf "SCA-%s" signal.SignalId
        EntityReference = signal.EntityReference
        ChangeDescription = desc
        Materiality = materiality
        Confidence = confidence
        DaysVariance = daysVariance
        QuantityVariance = quantityVariance
        QuantityVariancePercent = quantityVariancePercent
        ThreatenedObjectives = threatenedObjectives
        ReasonCodes = reasonCodes
        AssessmentTimestamp = evaluationTimestamp
        SuggestedEpistemicState = suggestedEpistemicState
    }
