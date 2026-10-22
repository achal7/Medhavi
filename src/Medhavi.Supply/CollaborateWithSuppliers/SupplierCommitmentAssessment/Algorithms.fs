/// BA-S-020 — Evaluate Commitment Exposure Algorithm
/// Traces to: Supply Intelligence Specification §10 (BA-S-020)
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCommitmentAssessment.Algorithms

open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Policies

/// BA-S-020: Evaluates multi-dimensional commitment exposure across 7 orthogonal dimensions
let evaluateCommitmentExposure
    (supplierId: SupplierId)
    (openQty: decimal)
    (openValuation: decimal)
    (telemetry: SupplierTelemetryFact)
    (policy: SupplierCollaborationPolicy)
    (timestamp: Timestamp)
    : CommitmentExposureAssessment =

    // 1. Fulfilment Exposure
    let otif = telemetry.HistoricalOTIF |> Option.defaultValue 1.0m
    let hasOtif = telemetry.HistoricalOTIF.IsSome
    let qAtRisk = if hasOtif then openQty * (1.0m - otif) else 0.0m
    let vAtRisk = if hasOtif then openValuation * (1.0m - otif) else 0.0m

    let fulfilLevel, fulfilReasons, fulfilObjectives =
        if not hasOtif then
            ExposureLevel.Low, [], []
        elif otif < policy.CriticalOtifThreshold then
            ExposureLevel.Critical, [ "DegradedHistoricalOTIF" ], [ "Delivery Reliability"; "Customer Service" ]
        elif otif < policy.UnreliableOtifThreshold then
            ExposureLevel.High, [ "DegradedHistoricalOTIF" ], [ "Delivery Reliability"; "Customer Service" ]
        else
            ExposureLevel.Nominal, [], []

    let expFulfil =
        {
            DimensionName = "Fulfilment"
            Level = fulfilLevel
            QuantityAtRisk = qAtRisk
            ValuationAtRisk = vAtRisk
            ReasonCodes = fulfilReasons
            ThreatenedObjectives = fulfilObjectives
        }

    // 2. Predictability Exposure
    let ltStdDev = telemetry.HistoricalLeadTimeStdDevDays |> Option.defaultValue 0.0m
    let hasLtStdDev = telemetry.HistoricalLeadTimeStdDevDays.IsSome

    let predLevel, predReasons, predObjectives =
        if not hasLtStdDev then
            ExposureLevel.Low, [], []
        elif ltStdDev > policy.HighLeadTimeStdDevDaysThreshold then
            ExposureLevel.High, [ "HighLeadTimeVariance" ], [ "Schedule Stability" ]
        elif ltStdDev > policy.ModerateLeadTimeStdDevDaysThreshold then
            ExposureLevel.Medium, [ "ModerateLeadTimeVariance" ], [ "Schedule Stability" ]
        else
            ExposureLevel.Low, [], []

    let expPred =
        {
            DimensionName = "Predictability"
            Level = predLevel
            QuantityAtRisk = 0.0m
            ValuationAtRisk = 0.0m
            ReasonCodes = predReasons
            ThreatenedObjectives = predObjectives
        }

    // 3. Credibility Exposure
    let slippage = telemetry.MeanPromiseDateSlippageDays |> Option.defaultValue 0.0m
    let hasSlippage = telemetry.MeanPromiseDateSlippageDays.IsSome

    let credLevel, credReasons, credObjectives =
        if not hasSlippage then
            ExposureLevel.Nominal, [], []
        elif slippage > policy.HighPromiseSlippageDaysThreshold then
            ExposureLevel.High, [ "UnrealisticConfirmationDate" ], [ "Planning Accuracy" ]
        elif slippage > policy.ModeratePromiseSlippageDaysThreshold then
            ExposureLevel.Medium, [ "MinorConfirmationSlippage" ], [ "Planning Accuracy" ]
        else
            ExposureLevel.Nominal, [], []

    let expCred =
        {
            DimensionName = "Credibility"
            Level = credLevel
            QuantityAtRisk = 0.0m
            ValuationAtRisk = 0.0m
            ReasonCodes = credReasons
            ThreatenedObjectives = credObjectives
        }

    // 4. Dependency Exposure
    let catSpend =
        if telemetry.CategoryTotalSpend <= 0.0m then
            openValuation
        else
            telemetry.CategoryTotalSpend

    let wVal = if catSpend > 0.0m then openValuation / catSpend else 0.0m

    let omegaAlt =
        if telemetry.QualifiedAlternateSuppliersCount = 0 then 1.0m
        elif telemetry.QualifiedAlternateSuppliersCount = 1 then 0.5m
        else 0.2m

    let idxDep = wVal * omegaAlt

    let depLevel, depReasons, depObjectives =
        if telemetry.QualifiedAlternateSuppliersCount = 0 && wVal > policy.SingleSourceSpendThreshold then
            ExposureLevel.Critical, [ "SingleSourceSupplierDependency" ], [ "Supply Continuity"; "Resilience" ]
        elif idxDep > policy.HighDependencyIndexThreshold then
            ExposureLevel.High, [ "NarrowSupplyBaseDependency" ], [ "Supply Continuity" ]
        else
            ExposureLevel.Low, [], []

    let expDep =
        {
            DimensionName = "Dependency"
            Level = depLevel
            QuantityAtRisk = 0.0m
            ValuationAtRisk = openValuation * idxDep
            ReasonCodes = depReasons
            ThreatenedObjectives = depObjectives
        }

    // 5. Flexibility Exposure
    let flexLevel, flexReasons, flexObjectives =
        if telemetry.ContractedVolumeSurgeTolerance < policy.MinSurgeFlexibilityThreshold then
            ExposureLevel.Medium, [ "ZeroVolumeFlexibility" ], [ "Agility" ]
        else
            ExposureLevel.Low, [], []

    let expFlex =
        {
            DimensionName = "Flexibility"
            Level = flexLevel
            QuantityAtRisk = 0.0m
            ValuationAtRisk = 0.0m
            ReasonCodes = flexReasons
            ThreatenedObjectives = flexObjectives
        }

    // 6. Reliability Exposure
    let qar = telemetry.QualityAcceptanceRate |> Option.defaultValue 1.0m
    let hasQar = telemetry.QualityAcceptanceRate.IsSome

    let relLevel, relReasons, relObjectives =
        if not hasQar then
            ExposureLevel.Nominal, [], []
        elif qar < policy.CriticalQualityAcceptanceThreshold then
            ExposureLevel.Critical, [ "HighQualityDefectRate" ], [ "Product Quality"; "Operational Efficiency" ]
        elif qar < policy.HighQualityAcceptanceThreshold then
            ExposureLevel.High, [ "ModerateDefectRate" ], [ "Product Quality" ]
        else
            ExposureLevel.Nominal, [], []

    let expRel =
        {
            DimensionName = "Reliability"
            Level = relLevel
            QuantityAtRisk = if hasQar then openQty * (1.0m - qar) else 0.0m
            ValuationAtRisk = if hasQar then openValuation * (1.0m - qar) else 0.0m
            ReasonCodes = relReasons
            ThreatenedObjectives = relObjectives
        }

    // 7. Stability Exposure
    let stabLevel, stabReasons, stabObjectives =
        if telemetry.RevisionsInsideFrozenWindow > policy.HighOrderRevisionsThreshold then
            ExposureLevel.High, [ "ExcessiveCommitmentRevisions" ], [ "Schedule Stability" ]
        elif telemetry.RevisionsInsideFrozenWindow >= 1 then
            ExposureLevel.Medium, [ "MinorCommitmentRevisions" ], [ "Schedule Stability" ]
        else
            ExposureLevel.Nominal, [], []

    let expStab =
        {
            DimensionName = "Stability"
            Level = stabLevel
            QuantityAtRisk = 0.0m
            ValuationAtRisk = 0.0m
            ReasonCodes = stabReasons
            ThreatenedObjectives = stabObjectives
        }

    // Composite Confidence
    let confidence =
        if not hasOtif || not hasLtStdDev then "Low"
        elif not hasQar then "Medium"
        else "High"

    let allExposures = [ expFulfil; expPred; expCred; expDep; expFlex; expRel; expStab ]
    let allReasons = allExposures |> List.collect(fun e -> e.ReasonCodes) |> List.distinct

    {
        SupplierId = supplierId
        EvaluationTimestamp = timestamp
        OverallConfidence = confidence
        Exposures = allExposures
        ReasonCodes = allReasons
    }

/// BA-S-021: Determine authoritative Supplier Collaboration Health State via governed precedence cascade
let determineHealthState
    (exposure: CommitmentExposureAssessment)
    (openCommitmentsCount: int)
    (dormancyDays: decimal)
    : {|
          AuthoritativeState: SupplierCollaborationHealthState
          TriggeringRuleId: string
          SupersededStates: SupplierCollaborationHealthState list
      |}
    =

    let getDimLevel name =
        exposure.Exposures
        |> List.tryFind(fun e -> e.DimensionName = name)
        |> Option.map(fun e -> e.Level)
        |> Option.defaultValue ExposureLevel.Nominal

    let lFul = getDimLevel "Fulfilment"
    let lPred = getDimLevel "Predictability"
    let lCred = getDimLevel "Credibility"
    let lDep = getDimLevel "Dependency"
    let lFlex = getDimLevel "Flexibility"
    let lRel = getDimLevel "Reliability"
    let lStab = getDimLevel "Stability"

    let isHighOrCritical l = l = ExposureLevel.High || l = ExposureLevel.Critical
    let isMedOrHigher l = l = ExposureLevel.Medium || l = ExposureLevel.High || l = ExposureLevel.Critical

    // Rule 0 (k=0): Insufficient Evidence
    let isRule0 = exposure.OverallConfidence = "Insufficient"

    // Rule 1 (k=1): Critically Dependent
    let isRule1 =
        lDep = ExposureLevel.Critical && (isHighOrCritical lFul || isHighOrCritical lPred || isHighOrCritical lRel)

    // Rule 2 (k=2): Underperforming
    let isRule2 = isHighOrCritical lFul || isHighOrCritical lRel

    // Rule 3 (k=3): Unpredictable
    let isRule3 = isHighOrCritical lPred || isHighOrCritical lStab

    // Rule 4 (k=4): At Risk
    let isRule4 = isMedOrHigher lDep || isHighOrCritical lCred || isHighOrCritical lFlex || isMedOrHigher lFul

    // Rule 5 (k=5): Inactive
    let isRule5 = openCommitmentsCount = 0 && dormancyDays >= 30.0m

    // Rule 6 (k=6): Healthy (Default Fallback)
    let isRule6 = true

    let matchingRules =
        [
            if isRule1 then
                (1, SupplierCollaborationHealthState.CriticallyDependent, "PO-S-060-R1-CriticallyDependent")
            if isRule2 then
                (2, SupplierCollaborationHealthState.Underperforming, "PO-S-060-R2-Underperforming")
            if isRule3 then
                (3, SupplierCollaborationHealthState.Unpredictable, "PO-S-060-R3-Unpredictable")
            if isRule4 then
                (4, SupplierCollaborationHealthState.AtRisk, "PO-S-060-R4-AtRisk")
            if isRule5 then
                (5, SupplierCollaborationHealthState.Inactive, "PO-S-060-R5-Inactive")
            if isRule6 then
                (6, SupplierCollaborationHealthState.Healthy, "PO-S-060-R6-Healthy")
        ]

    if isRule0 then
        {|
            AuthoritativeState = SupplierCollaborationHealthState.Unassessed
            TriggeringRuleId = "PO-S-060-R0-UnassessedHistoryDeficit"
            SupersededStates = []
        |}
    else
        match matchingRules with
        | [] ->
            {|
                AuthoritativeState = SupplierCollaborationHealthState.Unassessed
                TriggeringRuleId = "PO-S-060-R0-NoMatchingRule"
                SupersededStates = []
            |}
        | (minRank, state, ruleId) :: rest ->
            let superseded = rest |> List.map(fun (_, s, _) -> s)

            {|
                AuthoritativeState = state
                TriggeringRuleId = ruleId
                SupersededStates = superseded
            |}
