/// Traceability: BA-S-100 Derive Supply Learning (Stage: Discovery, Output: Candidate Learnings)
module Medhavi.Supply.LearnFromSupply.SupplyLearning.Algorithms

open System
open System.Globalization
open MathNet.Numerics
open MathNet.Numerics.Statistics
open Medhavi.SemanticModel
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Model
open Policies

// =========================================================================
// Multi-Period Historical Telemetry Fact Models
// =========================================================================

type SupplyQualitySummary =
    {
        Scope: PlanningScopeId
        PeriodStart: Timestamp
        PeriodEnd: Timestamp
        OverallScore: decimal
        QualityClassification: string
        DimensionalGaps: Map<string, decimal>
    }

type SupplyExceptionSummary =
    {
        ExceptionType: string
        EntityType: string
        EntityId: string
        Scope: PlanningScopeId
        Severity: string
        MetricValue: decimal
        ThresholdValue: decimal
        DetectedAt: Timestamp
    }

type SupplyPlanStabilitySummary =
    {
        Scope: PlanningScopeId
        PeriodStart: Timestamp
        PeriodEnd: Timestamp
        AdherenceRate: decimal
        FrozenZoneViolations: int
        StabilityScore: decimal
    }

type CapacityUtilizationSummary =
    {
        ResourceId: string
        PeriodStart: Timestamp
        PeriodEnd: Timestamp
        UtilizationRate: decimal
        PeakLoadRatio: decimal
        BottleneckHours: decimal
    }

type SupplierCommitmentSummary =
    {
        SupplierId: string
        ItemId: string
        PeriodStart: Timestamp
        PeriodEnd: Timestamp
        OtifRate: decimal
        AcknowledgmentLatencyHours: decimal
        SlippageDays: decimal
    }

type InventoryPositionSummary =
    {
        ItemLocationId: string
        PeriodStart: Timestamp
        PeriodEnd: Timestamp
        StockoutDays: int
        ExcessStockRatio: decimal
        ChurnRate: decimal
    }

/// Canonical historical evidence bundle consumed by BA-S-100
type HistoricalSupplyEvidenceCorpus =
    {
        Scope: PlanningScopeId
        Window: TemporalWindow
        Qualities: SupplyQualitySummary list
        Exceptions: SupplyExceptionSummary list
        PlanStabilities: SupplyPlanStabilitySummary list
        Capacities: CapacityUtilizationSummary list
        Suppliers: SupplierCommitmentSummary list
        Inventories: InventoryPositionSummary list
    }

/// Candidate supply learning packaged by BA-S-100
type CandidateSupplyLearning =
    {
        PatternReference: string
        SubjectType: string
        SubjectId: string
        LearningType: SupplyLearningType
        Scope: LearningScope
        TimeHorizon: TemporalWindow
        LearningStatement: string
        PatternConfidence: ConfidenceLevel
        PatternConfidenceScore: decimal
        RecurrenceCount: int
        RecurrenceFrequency: decimal
        StatisticalSignificance: decimal
        SupportingEvidence: HistoricalEvidenceReference list
        ImprovementOpportunities: ImprovementOpportunity list
        DerivedTimestamp: Timestamp
    }

// =========================================================================
// Statistical & Mathematical Helper Functions (BA-S-100 §7)
// =========================================================================

/// Compute linear regression slope over (x, y) coordinates
let computeLinearRegressionSlope (points: (float * float) list) : float =
    if points.Length < 2 then
        0.0
    else
        let n = float points.Length
        let sumX = points |> List.sumBy fst
        let sumY = points |> List.sumBy snd
        let sumXY = points |> List.sumBy(fun (x, y) -> x * y)
        let sumXX = points |> List.sumBy(fun (x, _) -> x * x)
        let denominator = n * sumXX - (sumX * sumX)

        if abs denominator < 1e-9 then
            0.0
        else
            (n * sumXY - (sumX * sumY)) / denominator

/// Two-sample Welch's t-test p-value
let welchTTestPValue (sample1: float[]) (sample2: float[]) : float =
    if sample1.Length < 2 || sample2.Length < 2 then
        1.0
    else
        let mean1 = sample1 |> Array.average
        let mean2 = sample2 |> Array.average
        let var1 = sample1 |> Array.averageBy(fun x -> (x - mean1) * (x - mean1))
        let var2 = sample2 |> Array.averageBy(fun x -> (x - mean2) * (x - mean2))
        let n1 = float sample1.Length
        let n2 = float sample2.Length
        let se = sqrt((var1 / n1) + (var2 / n2))

        if se < 1e-9 then
            1.0
        else
            let t = (mean1 - mean2) / se
            let dfNumerator = ((var1 / n1) + (var2 / n2)) ** 2.0
            let dfDenominator = (((var1 / n1) ** 2.0) / (n1 - 1.0)) + (((var2 / n2) ** 2.0) / (n2 - 1.0))

            if dfDenominator < 1e-9 then
                1.0
            else
                let df = dfNumerator / dfDenominator
                let cdf = MathNet.Numerics.Distributions.StudentT.CDF(0.0, 1.0, double df, double(abs t))
                let p = 2.0 * (1.0 - cdf)
                p |> min 1.0 |> max 0.0

/// Pattern Confidence index formulation (BA-S-100 §7.2)
/// Lambda_pat = nu_rec * (1.0 - exp(-N_src / 1.5)) * StatSig
let computePatternConfidence
    (nuRec: decimal)
    (sourceCount: int)
    (statSig: decimal)
    (recurrenceCount: int)
    (policy: LearningPolicy)
    : ConfidenceLevel * decimal =

    let nSrcFloat = float sourceCount
    let multiSourceFactor = 1.0 - exp(-nSrcFloat / 1.5)
    let lambdaPat = (float nuRec) * multiSourceFactor * (float statSig)
    let lambdaDecimal = decimal(max 0.0 (min 1.0 lambdaPat))

    let level =
        if
            lambdaDecimal >= policy.PatternConfidenceHighThreshold
            && sourceCount >= policy.MinCrossCapabilitySources
            && recurrenceCount >= policy.MinRecurrencePeriods
        then
            ConfidenceLevel.High
        elif lambdaDecimal >= policy.PatternConfidenceMediumThreshold && recurrenceCount >= 2 then
            ConfidenceLevel.Medium
        else
            ConfidenceLevel.Low

    (level, lambdaDecimal)

/// Intervention Confidence index formulation (BA-S-100 §7.3)
/// Lambda_int = eta_analogue * phi_feasibility * (NumericScore(C_pat) / 1.0)
let computeInterventionConfidence
    (analogueSuccess: decimal)
    (feasibility: decimal)
    (patternConf: ConfidenceLevel)
    (policy: LearningPolicy)
    : ConfidenceLevel =

    let score = patternConf.NumericScore
    let lambdaInt = analogueSuccess * feasibility * score

    if lambdaInt >= policy.InterventionConfidenceHighThreshold && analogueSuccess >= policy.AnalogueHighThreshold then
        ConfidenceLevel.High
    elif lambdaInt >= policy.InterventionConfidenceMediumThreshold then
        ConfidenceLevel.Medium
    else
        ConfidenceLevel.Low

// =========================================================================
// Analytical Discovery Engines (BA-S-100 Stage 2 to 5)
// =========================================================================

/// Engine 1: Supply Plan Stability & Nervousness Discovery
let analysePlanStability
    (scope: PlanningScopeId)
    (window: TemporalWindow)
    (stabilities: SupplyPlanStabilitySummary list)
    (exceptions: SupplyExceptionSummary list)
    (policy: LearningPolicy)
    (timestamp: Timestamp)
    : CandidateSupplyLearning list =

    let totalCycles = stabilities.Length

    if totalCycles < policy.MinRecurrencePeriods then
        []
    else
        let unstableCycles =
            stabilities
            |> List.filter(fun s -> s.StabilityScore < policy.PlanStabilityThreshold || s.FrozenZoneViolations > 0)

        let recurrenceCount = unstableCycles.Length
        let nuRec = decimal recurrenceCount / decimal totalCycles

        if recurrenceCount >= policy.MinRecurrencePeriods && nuRec >= policy.MinRecurrenceFrequency then
            let correlatedExceptions =
                exceptions
                |> List.filter(fun e ->
                    e.ExceptionType.Contains("PlanNervousness") || e.ExceptionType.Contains("ScheduleChange"))

            let sourceCount = if correlatedExceptions.Length > 0 then 2 else 1
            let avgStability = stabilities |> List.averageBy(fun s -> s.StabilityScore)
            let avgViolations = stabilities |> List.averageBy(fun s -> decimal s.FrozenZoneViolations)

            // Statistical significance: variance against stable baseline (0.90)
            let sample = stabilities |> List.map(fun s -> float s.StabilityScore) |> List.toArray
            let baseline = Array.create sample.Length 0.90
            let pVal = welchTTestPValue sample baseline
            let statSig = decimal(max 0.01 (min 1.0 (1.0 - pVal)))

            let (patConf, patScore) = computePatternConfidence nuRec sourceCount statSig recurrenceCount policy

            let analogueSuccess = 0.85m
            let feasibility = 0.80m
            let intConf = computeInterventionConfidence analogueSuccess feasibility patConf policy

            let opp: ImprovementOpportunity =
                {
                    OpportunityId = $"opp-plan-stability-{PlanningScopeId.value scope}"
                    TargetCapability = "PlanSupply"
                    TargetPolicyId = "PO-S-020"
                    TargetParameter = "FrozenHorizonDays"
                    CurrentBaselineValue = "Current Horizon"
                    RecommendedValue = "Extend Frozen Horizon by +7 days and calibrate change tolerance band to 10%"
                    ExpectedBenefit =
                        $"Stabilizes production dispatch; reduces recurring frozen zone violations by up to {avgViolations:N1} per cycle"
                    InterventionConfidence = intConf
                    AnalogueSuccessRate = analogueSuccess
                    OperationalFeasibility = feasibility
                }

            let evidence =
                stabilities
                |> List.mapi(fun idx s ->
                    let pId =
                        EvaluationPeriodId.create $"period-{idx + 1}"
                        |> Result.defaultValue EvaluationPeriodId.defaultId

                    let tw = TemporalWindow.create s.PeriodStart s.PeriodEnd |> Result.defaultValue window

                    {
                        CapabilityId = "PlanSupply"
                        ArtifactType = "SupplyPlanStability"
                        ArtifactId = PlanningScopeId.value s.Scope
                        PeriodId = pId
                        PeriodWindow = tw
                        SummaryMetrics =
                            [
                                "StabilityScore", s.StabilityScore
                                "AdherenceRate", s.AdherenceRate
                                "FrozenZoneViolations", decimal s.FrozenZoneViolations
                            ]
                            |> Map.ofList
                    })

            let statement =
                $"Supply Plan in scope '{PlanningScopeId.value scope}' exhibits chronic operational nervousness across {recurrenceCount} of {totalCycles} planning cycles (recurrence ratio {nuRec:P1}, mean stability score {avgStability:N2}, averaging {avgViolations:N1} frozen-horizon violations per cycle)."

            [
                {
                    PatternReference = $"plan-nervousness-{PlanningScopeId.value scope}"
                    SubjectType = "PlanningScope"
                    SubjectId = PlanningScopeId.value scope
                    LearningType = SupplyLearningType.PlanStabilityAndNervousness
                    Scope = LearningScope.Enterprise
                    TimeHorizon = window
                    LearningStatement = statement
                    PatternConfidence = patConf
                    PatternConfidenceScore = patScore
                    RecurrenceCount = recurrenceCount
                    RecurrenceFrequency = nuRec
                    StatisticalSignificance = statSig
                    SupportingEvidence = evidence
                    ImprovementOpportunities = [ opp ]
                    DerivedTimestamp = timestamp
                }
            ]
        else
            []

/// Engine 2: Chronic Bottleneck Persistence Discovery
let analyseBottleneckPersistence
    (scope: PlanningScopeId)
    (window: TemporalWindow)
    (capacities: CapacityUtilizationSummary list)
    (qualities: SupplyQualitySummary list)
    (policy: LearningPolicy)
    (timestamp: Timestamp)
    : CandidateSupplyLearning list =

    let resourceGroups = capacities |> List.groupBy(fun c -> c.ResourceId)

    resourceGroups
    |> List.choose(fun (resId, summaries) ->
        let totalPeriods = summaries.Length

        if totalPeriods < policy.MinRecurrencePeriods then
            None
        else
            let bottleneckPeriods =
                summaries
                |> List.filter(fun c ->
                    c.UtilizationRate >= policy.BottleneckPersistenceThreshold || c.BottleneckHours >= 20.0m)

            let recurrenceCount = bottleneckPeriods.Length
            let nuRec = decimal recurrenceCount / decimal totalPeriods

            if recurrenceCount >= policy.MinRecurrencePeriods && nuRec >= policy.MinRecurrenceFrequency then
                let correlatedQuality =
                    qualities
                    |> List.filter(fun q ->
                        q.DimensionalGaps.ContainsKey("CapacityAdequacy")
                        || q.DimensionalGaps.ContainsKey("OnTimeProduction"))

                let sourceCount = if correlatedQuality.Length > 0 then 2 else 1
                let avgUtil = summaries |> List.averageBy(fun c -> c.UtilizationRate)
                let avgBottleneckHours = summaries |> List.averageBy(fun c -> c.BottleneckHours)

                let utilValues = summaries |> List.map(fun c -> float c.UtilizationRate) |> List.toArray
                let targetUtil = Array.create utilValues.Length 0.85
                let pVal = welchTTestPValue utilValues targetUtil
                let statSig = decimal(max 0.01 (min 1.0 (1.0 - pVal)))

                let (patConf, patScore) = computePatternConfidence nuRec sourceCount statSig recurrenceCount policy

                let analogueSuccess = 0.80m
                let feasibility = 0.75m
                let intConf = computeInterventionConfidence analogueSuccess feasibility patConf policy

                let opp: ImprovementOpportunity =
                    {
                        OpportunityId = $"opp-bottleneck-{resId.ToLowerInvariant()}"
                        TargetCapability = "ManageCapacity"
                        TargetPolicyId = "PO-S-050"
                        TargetParameter = "OperatingEnvelopeBufferHours"
                        CurrentBaselineValue = $"{avgBottleneckHours:N1} bottleneck hrs"
                        RecommendedValue =
                            "Authorize dynamic secondary shift routing or increase lot-size smoothing coefficient by +15%"
                        ExpectedBenefit =
                            $"Relieves chronic capacity pressure on resource '{resId}', reducing expected bottleneck hours by 30%%"
                        InterventionConfidence = intConf
                        AnalogueSuccessRate = analogueSuccess
                        OperationalFeasibility = feasibility
                    }

                let evidence =
                    summaries
                    |> List.mapi(fun idx c ->
                        let pId =
                            EvaluationPeriodId.create $"period-{idx + 1}"
                            |> Result.defaultValue EvaluationPeriodId.defaultId

                        let tw = TemporalWindow.create c.PeriodStart c.PeriodEnd |> Result.defaultValue window

                        {
                            CapabilityId = "ManageCapacity"
                            ArtifactType = "CapacityUtilization"
                            ArtifactId = resId
                            PeriodId = pId
                            PeriodWindow = tw
                            SummaryMetrics =
                                [
                                    "UtilizationRate", c.UtilizationRate
                                    "PeakLoadRatio", c.PeakLoadRatio
                                    "BottleneckHours", c.BottleneckHours
                                ]
                                |> Map.ofList
                        })

                let statement =
                    $"Resource '{resId}' exhibits chronic bottleneck persistence across {recurrenceCount} of {totalPeriods} evaluated periods (recurrence ratio {nuRec:P1}, mean utilization {avgUtil:P1}, averaging {avgBottleneckHours:N1} bottleneck hours per period)."

                Some
                    {
                        PatternReference = $"bottleneck-persistence-{resId}"
                        SubjectType = "Resource"
                        SubjectId = resId
                        LearningType = SupplyLearningType.BottleneckPersistence
                        Scope = LearningScope.BusinessUnit(PlanningScopeId.value scope)
                        TimeHorizon = window
                        LearningStatement = statement
                        PatternConfidence = patConf
                        PatternConfidenceScore = patScore
                        RecurrenceCount = recurrenceCount
                        RecurrenceFrequency = nuRec
                        StatisticalSignificance = statSig
                        SupportingEvidence = evidence
                        ImprovementOpportunities = [ opp ]
                        DerivedTimestamp = timestamp
                    }
            else
                None)

/// Engine 3: Supplier Lead Time Drift & Reliability Discovery
let analyseSupplierReliabilityDrift
    (scope: PlanningScopeId)
    (window: TemporalWindow)
    (supplierSummaries: SupplierCommitmentSummary list)
    (qualities: SupplyQualitySummary list)
    (policy: LearningPolicy)
    (timestamp: Timestamp)
    : CandidateSupplyLearning list =

    let supplierGroups = supplierSummaries |> List.groupBy(fun s -> (s.SupplierId, s.ItemId))

    supplierGroups
    |> List.choose(fun ((suppId, itemId), summaries) ->
        let totalPeriods = summaries.Length

        if totalPeriods < policy.MinRecurrencePeriods then
            None
        else
            let degradedPeriods = summaries |> List.filter(fun s -> s.OtifRate < 0.85m || s.SlippageDays >= 3.0m)

            let recurrenceCount = degradedPeriods.Length
            let nuRec = decimal recurrenceCount / decimal totalPeriods

            if recurrenceCount >= policy.MinRecurrencePeriods && nuRec >= policy.MinRecurrenceFrequency then
                let correlatedQuality =
                    qualities
                    |> List.filter(fun q ->
                        q.DimensionalGaps.ContainsKey("SupplierPerformance")
                        || q.DimensionalGaps.ContainsKey("InboundTimeliness"))

                let sourceCount = if correlatedQuality.Length > 0 then 2 else 1
                let avgOtif = summaries |> List.averageBy(fun s -> s.OtifRate)
                let avgSlippage = summaries |> List.averageBy(fun s -> s.SlippageDays)

                let otifValues = summaries |> List.map(fun s -> float s.OtifRate) |> List.toArray
                let otifTarget = Array.create otifValues.Length 0.95
                let pVal = welchTTestPValue otifValues otifTarget
                let statSig = decimal(max 0.01 (min 1.0 (1.0 - pVal)))

                let (patConf, patScore) = computePatternConfidence nuRec sourceCount statSig recurrenceCount policy

                let analogueSuccess = 0.85m
                let feasibility = 0.90m
                let intConf = computeInterventionConfidence analogueSuccess feasibility patConf policy

                let opp: ImprovementOpportunity =
                    {
                        OpportunityId = $"opp-supplier-buffer-{suppId.ToLowerInvariant()}-{itemId.ToLowerInvariant()}"
                        TargetCapability = "ProcureMaterials"
                        TargetPolicyId = "PO-S-070"
                        TargetParameter = "PlannedLeadTimeDays"
                        CurrentBaselineValue = $"{avgSlippage:N1} days slippage"
                        RecommendedValue =
                            $"Calibrate planned supplier lead time by +{ceil avgSlippage} days and diversify allocation quota to backup supplier"
                        ExpectedBenefit =
                            "Eliminates unexpected production delays; recovers planned schedule adherence"
                        InterventionConfidence = intConf
                        AnalogueSuccessRate = analogueSuccess
                        OperationalFeasibility = feasibility
                    }

                let evidence =
                    summaries
                    |> List.mapi(fun idx s ->
                        let pId =
                            EvaluationPeriodId.create $"period-{idx + 1}"
                            |> Result.defaultValue EvaluationPeriodId.defaultId

                        let tw = TemporalWindow.create s.PeriodStart s.PeriodEnd |> Result.defaultValue window

                        {
                            CapabilityId = "CollaborateWithSuppliers"
                            ArtifactType = "SupplierCommitment"
                            ArtifactId = $"{suppId}#{itemId}"
                            PeriodId = pId
                            PeriodWindow = tw
                            SummaryMetrics =
                                [
                                    "OtifRate", s.OtifRate
                                    "SlippageDays", s.SlippageDays
                                    "AcknowledgmentLatencyHours", s.AcknowledgmentLatencyHours
                                ]
                                |> Map.ofList
                        })

                let statement =
                    $"Supplier '{suppId}' for item '{itemId}' shows persistent lead-time slippage and delivery unreliability across {recurrenceCount} of {totalPeriods} periods (recurrence ratio {nuRec:P1}, mean OTIF {avgOtif:P1}, mean lead-time slippage {avgSlippage:N1} days)."

                Some
                    {
                        PatternReference = $"supplier-drift-{suppId}-{itemId}"
                        SubjectType = "SupplierItem"
                        SubjectId = $"{suppId}#{itemId}"
                        LearningType = SupplyLearningType.SupplierReliabilityDrift
                        Scope = LearningScope.SupplierNetwork(suppId)
                        TimeHorizon = window
                        LearningStatement = statement
                        PatternConfidence = patConf
                        PatternConfidenceScore = patScore
                        RecurrenceCount = recurrenceCount
                        RecurrenceFrequency = nuRec
                        StatisticalSignificance = statSig
                        SupportingEvidence = evidence
                        ImprovementOpportunities = [ opp ]
                        DerivedTimestamp = timestamp
                    }
            else
                None)

/// Engine 4: Inventory Position Churn & Over/Under-Buffering Discovery
let analyseInventoryPositionChurn
    (scope: PlanningScopeId)
    (window: TemporalWindow)
    (inventories: InventoryPositionSummary list)
    (qualities: SupplyQualitySummary list)
    (policy: LearningPolicy)
    (timestamp: Timestamp)
    : CandidateSupplyLearning list =

    let locationGroups = inventories |> List.groupBy(fun i -> i.ItemLocationId)

    locationGroups
    |> List.choose(fun (itemLocId, summaries) ->
        let totalPeriods = summaries.Length

        if totalPeriods < policy.MinRecurrencePeriods then
            None
        else
            let churnPeriods =
                summaries
                |> List.filter(fun i ->
                    i.StockoutDays >= policy.InventoryStockoutChurnThreshold || i.ExcessStockRatio >= 0.30m)

            let recurrenceCount = churnPeriods.Length
            let nuRec = decimal recurrenceCount / decimal totalPeriods

            if recurrenceCount >= policy.MinRecurrencePeriods && nuRec >= policy.MinRecurrenceFrequency then
                let correlatedQuality =
                    qualities
                    |> List.filter(fun q ->
                        q.DimensionalGaps.ContainsKey("InventoryHealth")
                        || q.DimensionalGaps.ContainsKey("WorkingCapitalEfficiency"))

                let sourceCount = if correlatedQuality.Length > 0 then 2 else 1
                let avgStockoutDays = summaries |> List.averageBy(fun i -> decimal i.StockoutDays)
                let avgExcessRatio = summaries |> List.averageBy(fun i -> i.ExcessStockRatio)

                let stockoutValues = summaries |> List.map(fun i -> float i.StockoutDays) |> List.toArray
                let zeroStockout = Array.create stockoutValues.Length 0.0
                let pVal = welchTTestPValue stockoutValues zeroStockout
                let statSig = decimal(max 0.01 (min 1.0 (1.0 - pVal)))

                let (patConf, patScore) = computePatternConfidence nuRec sourceCount statSig recurrenceCount policy

                let analogueSuccess = 0.90m
                let feasibility = 0.85m
                let intConf = computeInterventionConfidence analogueSuccess feasibility patConf policy

                let opp: ImprovementOpportunity =
                    {
                        OpportunityId = $"opp-inv-buffer-{itemLocId.ToLowerInvariant()}"
                        TargetCapability = "ManageInventory"
                        TargetPolicyId = "PO-S-030"
                        TargetParameter = "SafetyStockMultiplier"
                        CurrentBaselineValue = $"{avgStockoutDays:N1} stockout days, {avgExcessRatio:P1} excess"
                        RecommendedValue =
                            "Re-segment inventory review frequency from Monthly to Weekly and adjust safety stock buffer by +12%"
                        ExpectedBenefit =
                            "Dampens severe inventory churn; eliminates chronic stockout exposure while controlling holding cost"
                        InterventionConfidence = intConf
                        AnalogueSuccessRate = analogueSuccess
                        OperationalFeasibility = feasibility
                    }

                let evidence =
                    summaries
                    |> List.mapi(fun idx i ->
                        let pId =
                            EvaluationPeriodId.create $"period-{idx + 1}"
                            |> Result.defaultValue EvaluationPeriodId.defaultId

                        let tw = TemporalWindow.create i.PeriodStart i.PeriodEnd |> Result.defaultValue window

                        {
                            CapabilityId = "ManageInventory"
                            ArtifactType = "InventoryPosition"
                            ArtifactId = itemLocId
                            PeriodId = pId
                            PeriodWindow = tw
                            SummaryMetrics =
                                [
                                    "StockoutDays", decimal i.StockoutDays
                                    "ExcessStockRatio", i.ExcessStockRatio
                                    "ChurnRate", i.ChurnRate
                                ]
                                |> Map.ofList
                        })

                let statement =
                    $"Item-Location '{itemLocId}' experiences severe inventory position churn across {recurrenceCount} of {totalPeriods} periods (recurrence ratio {nuRec:P1}, mean stockout days {avgStockoutDays:N1}, excess stock ratio {avgExcessRatio:P1})."

                Some
                    {
                        PatternReference = $"inventory-churn-{itemLocId}"
                        SubjectType = "ItemLocation"
                        SubjectId = itemLocId
                        LearningType = SupplyLearningType.InventoryPositionChurn
                        Scope = LearningScope.BusinessUnit(PlanningScopeId.value scope)
                        TimeHorizon = window
                        LearningStatement = statement
                        PatternConfidence = patConf
                        PatternConfidenceScore = patScore
                        RecurrenceCount = recurrenceCount
                        RecurrenceFrequency = nuRec
                        StatisticalSignificance = statSig
                        SupportingEvidence = evidence
                        ImprovementOpportunities = [ opp ]
                        DerivedTimestamp = timestamp
                    }
            else
                None)

/// Engine 5: Recurring Supply Exception Clusters Discovery
let analyseRecurringSupplyExceptions
    (scope: PlanningScopeId)
    (window: TemporalWindow)
    (exceptions: SupplyExceptionSummary list)
    (qualities: SupplyQualitySummary list)
    (policy: LearningPolicy)
    (timestamp: Timestamp)
    : CandidateSupplyLearning list =

    let exceptionClusters = exceptions |> List.groupBy(fun e -> (e.EntityType, e.EntityId, e.ExceptionType))

    exceptionClusters
    |> List.choose(fun ((entityType, entityId, exceptionType), cluster) ->
        let recurrenceCount = cluster.Length

        if recurrenceCount >= policy.ExceptionRecurrenceThreshold then
            let correlatedQuality =
                qualities
                |> List.filter(fun q ->
                    q.DimensionalGaps.ContainsKey(exceptionType)
                    || q.DimensionalGaps.ContainsKey("OperationalResilience"))

            let sourceCount = if correlatedQuality.Length > 0 then 2 else 1
            let nuRec = decimal(min 1.0 ((float recurrenceCount) / (float(policy.ExceptionRecurrenceThreshold * 2))))
            let statSig = 0.90m

            let (patConf, patScore) = computePatternConfidence nuRec sourceCount statSig recurrenceCount policy

            let analogueSuccess = 0.80m
            let feasibility = 0.80m
            let intConf = computeInterventionConfidence analogueSuccess feasibility patConf policy

            let opp: ImprovementOpportunity =
                {
                    OpportunityId =
                        $"opp-exception-mitigation-{entityId.ToLowerInvariant()}-{exceptionType.ToLowerInvariant()}"
                    TargetCapability = "DetectSupplyExceptions"
                    TargetPolicyId = "PO-S-100"
                    TargetParameter = "ExceptionThresholdTolerance"
                    CurrentBaselineValue = $"{recurrenceCount} occurrences"
                    RecommendedValue =
                        $"Establish automated proactive early-warning threshold for '{exceptionType}' on {entityType} '{entityId}'"
                    ExpectedBenefit = "Suppresses cascading operational disruptions and mitigates root cause"
                    InterventionConfidence = intConf
                    AnalogueSuccessRate = analogueSuccess
                    OperationalFeasibility = feasibility
                }

            let evidence =
                cluster
                |> List.mapi(fun idx e ->
                    let pId =
                        EvaluationPeriodId.create $"period-{idx + 1}"
                        |> Result.defaultValue EvaluationPeriodId.defaultId

                    let tw = TemporalWindow.create e.DetectedAt e.DetectedAt |> Result.defaultValue window

                    {
                        CapabilityId = "DetectSupplyExceptions"
                        ArtifactType = "SupplyExceptionEvidence"
                        ArtifactId = $"{entityType}:{entityId}"
                        PeriodId = pId
                        PeriodWindow = tw
                        SummaryMetrics =
                            [ "MetricValue", e.MetricValue; "ThresholdValue", e.ThresholdValue ] |> Map.ofList
                    })

            let statement =
                $"Entity {entityType} '{entityId}' experienced {recurrenceCount} recurring '{exceptionType}' exceptions during the evaluation horizon, indicating chronic operational vulnerability."

            Some
                {
                    PatternReference = $"exception-cluster-{entityId}-{exceptionType}"
                    SubjectType = entityType
                    SubjectId = entityId
                    LearningType = SupplyLearningType.RecurringExceptionPattern
                    Scope = LearningScope.Enterprise
                    TimeHorizon = window
                    LearningStatement = statement
                    PatternConfidence = patConf
                    PatternConfidenceScore = patScore
                    RecurrenceCount = recurrenceCount
                    RecurrenceFrequency = nuRec
                    StatisticalSignificance = statSig
                    SupportingEvidence = evidence
                    ImprovementOpportunities = [ opp ]
                    DerivedTimestamp = timestamp
                }
        else
            None)

// =========================================================================
// Primary Multi-Period Analytical Discovery Composer (BA-S-100)
// =========================================================================

/// Derives candidate Supply Learnings from multi-period historical evidence corpus
let deriveSupplyLearnings
    (corpus: HistoricalSupplyEvidenceCorpus)
    (policy: LearningPolicy)
    (timestamp: Timestamp)
    : CandidateSupplyLearning list =

    let planLearnings =
        analysePlanStability corpus.Scope corpus.Window corpus.PlanStabilities corpus.Exceptions policy timestamp

    let bottleneckLearnings =
        analyseBottleneckPersistence corpus.Scope corpus.Window corpus.Capacities corpus.Qualities policy timestamp

    let supplierLearnings =
        analyseSupplierReliabilityDrift corpus.Scope corpus.Window corpus.Suppliers corpus.Qualities policy timestamp

    let inventoryLearnings =
        analyseInventoryPositionChurn corpus.Scope corpus.Window corpus.Inventories corpus.Qualities policy timestamp

    let exceptionLearnings =
        analyseRecurringSupplyExceptions corpus.Scope corpus.Window corpus.Exceptions corpus.Qualities policy timestamp

    planLearnings @ bottleneckLearnings @ supplierLearnings @ inventoryLearnings @ exceptionLearnings
