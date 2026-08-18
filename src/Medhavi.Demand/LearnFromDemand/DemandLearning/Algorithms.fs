/// Traces to: BA-D-013 Derive Demand Learning
module Medhavi.Demand.LearnFromDemand.DemandLearning.Algorithms

open System
open MathNet.Numerics
open MathNet.Numerics.Statistics
open Medhavi.SemanticModel
open Medhavi.Demand
open Model
open Policies

// ---------- Statistical Helper Functions ----------

/// Compute Pearson correlation between two arrays of equal length
let correlation (xs: float[]) (ys: float[]) : float =
    if xs.Length <> ys.Length || xs.Length < 3 then
        0.0
    else
        let meanX = xs |> Array.average
        let meanY = ys |> Array.average
        let numerator = Array.zip xs ys |> Array.sumBy(fun (x, y) -> (x - meanX) * (y - meanY))
        let denomX = xs |> Array.sumBy(fun x -> (x - meanX) * (x - meanX)) |> sqrt
        let denomY = ys |> Array.sumBy(fun y -> (y - meanY) * (y - meanY)) |> sqrt
        if denomX = 0.0 || denomY = 0.0 then 0.0 else numerator / (denomX * denomY)

/// Compute least-squares linear regression slope for a sequence of (x, y) points
let computeLinearRegressionSlope (points: (float * float) list) : float =
    if points.Length < 2 then
        0.0
    else
        let n = float points.Length
        let sumX = points |> List.sumBy fst
        let sumY = points |> List.sumBy snd
        let sumXY = points |> List.sumBy(fun (x, y) -> x * y)
        let sumXX = points |> List.sumBy(fun (x, _) -> x * x)
        let denominator = n * sumXX - sumX * sumX
        if denominator = 0.0 then 0.0 else (n * sumXY - sumX * sumY) / denominator

/// Two-sample Welch's t-test p-value using MathNet Student-T distribution CDF
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
        let se = sqrt(var1 / n1 + var2 / n2)

        if se = 0.0 then
            1.0
        else
            let t = (mean1 - mean2) / se
            let dfNumerator = (var1 / n1 + var2 / n2) * (var1 / n1 + var2 / n2)
            let dfDenominator = ((var1 / n1) * (var1 / n1) / (n1 - 1.0)) + ((var2 / n2) * (var2 / n2) / (n2 - 1.0))

            if dfDenominator = 0.0 then
                1.0
            else
                let df = dfNumerator / dfDenominator
                let cdf = MathNet.Numerics.Distributions.StudentT.CDF(0.0, 1.0, double df, double(abs t))
                let p = 2.0 * (1.0 - cdf)
                p |> min 1.0 |> max 0.0

// ---------- Analytical Discovery Engines (BA-D-013) ----------

/// Engine 1: Analyse planner override effectiveness (FVA post-mortems and habitual bias)
let analyseOverrideEffectiveness
    (scope: PlanningScopeId)
    (windowStart: Timestamp)
    (windowEnd: Timestamp)
    (overrides: PlannerOverrideSummary list)
    (policy: LearningAnalysisPolicy)
    (timestamp: Timestamp)
    : DemandLearning list =

    let plannerGroups = overrides |> List.groupBy(fun o -> o.PlannerId)

    plannerGroups
    |> List.choose(fun (plannerId, ovs) ->
        let evaluatedOverrides =
            ovs
            |> List.choose(fun o ->
                match o.ActualValue with
                | Some actual ->
                    let baseError = abs(o.OriginalValue - actual)
                    let overrideError = abs(o.OverrideValue - actual)
                    let fva = baseError - overrideError
                    Some(o, fva, overrideError - baseError)
                | None -> None)

        let totalEvaluated = evaluatedOverrides.Length

        if totalEvaluated < policy.MinSampleSize then
            None
        else
            let negativeFvaOverrides = evaluatedOverrides |> List.filter(fun (_, fva, _) -> fva < 0.0m)
            let negativeRatio = decimal negativeFvaOverrides.Length / decimal totalEvaluated

            let avgAccuracyLoss =
                if negativeFvaOverrides.IsEmpty then
                    0.0m
                else
                    negativeFvaOverrides |> List.averageBy(fun (_, _, loss) -> loss)

            if negativeRatio >= policy.MaxOverrideNegativeFvaRatio then
                let confidence =
                    if totalEvaluated >= 20 && negativeRatio >= 0.75m then
                        policy.PatternConfidenceHigh
                    elif totalEvaluated >= policy.MinSampleSize then
                        policy.PatternConfidenceMedium
                    else
                        policy.PatternConfidenceMedium

                let learningId =
                    DemandLearningId.ofComponents
                        scope
                        ("override-" + plannerId.ToLowerInvariant())
                        windowStart
                        windowEnd

                let percentageStr = (negativeRatio * 100.0m).ToString("N1") + "%"

                let statement =
                    "Planner '"
                    + plannerId
                    + "' overrides in scope '"
                    + (PlanningScopeId.value scope)
                    + "' systematically degraded forecast accuracy in "
                    + (string negativeFvaOverrides.Length)
                    + " of "
                    + (string totalEvaluated)
                    + " instances ("
                    + percentageStr
                    + "), causing an average accuracy degradation of "
                    + avgAccuracyLoss.ToString("N2")
                    + " units."

                let opportunity: ImprovementOpportunity =
                    { OpportunityId = "opp-override-limit-" + plannerId.ToLowerInvariant()
                      TargetCapability = "ForecastDemand"
                      TargetPolicyId = "PO-D-022"
                      ProposedParameterChange =
                        "Reduce manual override deviation limits from +-50% to +-25% for Planner '" + plannerId + "'"
                      ExpectedBenefit =
                        "Expected accuracy recovery of " + avgAccuracyLoss.ToString("N2") + " units per cycle"
                      InterventionConfidence = policy.InterventionConfidenceHigh }

                let evidenceRefs =
                    ovs
                    |> List.map(fun o ->
                        { ArtifactType = "PlannerOverride"
                          ArtifactId = o.PublicationId
                          PeriodStart = o.OverriddenAt
                          PeriodEnd = o.OverriddenAt
                          SummaryStatistics =
                            [ "OriginalValue", o.OriginalValue
                              "OverrideValue", o.OverrideValue
                              match o.ActualValue with
                              | Some act -> "ActualValue", act
                              | None -> () ]
                            |> Map.ofList })

                Some
                    { Id = learningId
                      Scope = scope
                      LearningType = LearningType.OverrideEffectiveness
                      LearningStatement = statement
                      PatternConfidence = confidence
                      InterventionConfidence = policy.InterventionConfidenceHigh
                      SupportingEvidence = evidenceRefs
                      ImprovementOpportunities = [ opportunity ]
                      PolicyVersion = "PO-D-048:v" + (string policy.Version)
                      CreatedAt = timestamp }
            else
                None)

/// Engine 2: Analyse forecast bias drift across time (least-squares trend slope & Welch's t-test)
let analyseBiasPatterns
    (scope: PlanningScopeId)
    (windowStart: Timestamp)
    (windowEnd: Timestamp)
    (qualityAssessments: ForecastQualityAssessmentSummary list)
    (policy: LearningAnalysisPolicy)
    (timestamp: Timestamp)
    : DemandLearning list =

    if qualityAssessments.Length < policy.MinRecurrencePeriods then
        []
    else
        let sortedAssessments = qualityAssessments |> List.sortBy(fun a -> a.PeriodStart)
        let biasSeries = sortedAssessments |> List.mapi(fun idx a -> (float idx, float a.ForecastBias))

        let slope = computeLinearRegressionSlope biasSeries
        let biasValues = sortedAssessments |> List.map(fun a -> float a.ForecastBias) |> List.toArray
        let zeroBaseline = Array.create biasValues.Length 0.0
        let pValue = welchTTestPValue biasValues zeroBaseline
        let avgBias = biasValues |> Array.average

        let isTrending = abs slope >= float policy.TrendMinSlopeForDegradation
        let isStatisticallySignificant = pValue <= float policy.StatisticalSignificanceThreshold && abs avgBias >= 5.0

        if isTrending || isStatisticallySignificant then
            let trendDir =
                if slope > 0.0 then
                    "upward (under-forecasting)"
                else
                    "downward (over-forecasting)"

            let confidence =
                if sortedAssessments.Length >= 6 && pValue <= 0.01 then
                    policy.PatternConfidenceHigh
                else
                    policy.PatternConfidenceMedium

            let learningId = DemandLearningId.ofComponents scope "bias-drift" windowStart windowEnd

            let statement =
                "Forecast bias in scope '"
                + (PlanningScopeId.value scope)
                + "' exhibits a statistically significant "
                + trendDir
                + " drift (slope = "
                + slope.ToString("F4")
                + ", mean bias = "
                + avgBias.ToString("F2")
                + ", p = "
                + pValue.ToString("F4")
                + ") over "
                + (string sortedAssessments.Length)
                + " consecutive periods."

            let opportunity: ImprovementOpportunity =
                { OpportunityId = "opp-bias-recalibration"
                  TargetCapability = "ForecastDemand"
                  TargetPolicyId = "PO-D-017"
                  ProposedParameterChange = "Recalibrate trend smoothing parameter beta or apply damped trend model"
                  ExpectedBenefit = "Expected bias reduction towards 0 (from " + avgBias.ToString("F2") + ")"
                  InterventionConfidence = policy.InterventionConfidenceMedium }

            let evidenceRefs =
                sortedAssessments
                |> List.map(fun a ->
                    { ArtifactType = "ForecastQualityAssessment"
                      ArtifactId = PlanningScopeId.value a.Scope
                      PeriodStart = a.PeriodStart
                      PeriodEnd = a.PeriodEnd
                      SummaryStatistics =
                        [ "Wape", a.Wape
                          "ForecastBias", a.ForecastBias
                          "TrackingSignal", a.TrackingSignal ]
                        |> Map.ofList })

            [ { Id = learningId
                Scope = scope
                LearningType = LearningType.ForecastBiasPattern
                LearningStatement = statement
                PatternConfidence = confidence
                InterventionConfidence = policy.InterventionConfidenceMedium
                SupportingEvidence = evidenceRefs
                ImprovementOpportunities = [ opportunity ]
                PolicyVersion = "PO-D-048:v" + (string policy.Version)
                CreatedAt = timestamp } ]
        else
            []

/// Engine 3: Analyse model performance degradation (monotonic WAPE increase)
let analyseModelDegradation
    (scope: PlanningScopeId)
    (windowStart: Timestamp)
    (windowEnd: Timestamp)
    (qualityAssessments: ForecastQualityAssessmentSummary list)
    (policy: LearningAnalysisPolicy)
    (timestamp: Timestamp)
    : DemandLearning list =

    if qualityAssessments.Length < policy.MinRecurrencePeriods then
        []
    else
        let sortedAssessments = qualityAssessments |> List.sortBy(fun a -> a.PeriodStart)
        let wapePairs = sortedAssessments |> List.pairwise
        let consecutiveIncreases = wapePairs |> List.filter(fun (prev, curr) -> curr.Wape > prev.Wape)

        if
            consecutiveIncreases.Length >= policy.MinRecurrencePeriods - 1
            && consecutiveIncreases.Length = wapePairs.Length
        then
            let firstWape = sortedAssessments |> List.head |> (fun a -> a.Wape)
            let lastWape = sortedAssessments |> List.last |> (fun a -> a.Wape)
            let totalDegradation = lastWape - firstWape

            let learningId = DemandLearningId.ofComponents scope "model-degradation" windowStart windowEnd

            let statement =
                "Forecast accuracy in scope '"
                + (PlanningScopeId.value scope)
                + "' has degraded monotonically for "
                + (string sortedAssessments.Length)
                + " consecutive periods (WAPE increased from "
                + firstWape.ToString("N1")
                + "% to "
                + lastWape.ToString("N1")
                + "%, net degradation: +"
                + totalDegradation.ToString("N1")
                + "%)."

            let opportunity: ImprovementOpportunity =
                { OpportunityId = "opp-model-recompetition"
                  TargetCapability = "ForecastDemand"
                  TargetPolicyId = "PO-D-017"
                  ProposedParameterChange = "Trigger champion model re-competition and feature re-selection"
                  ExpectedBenefit = "Expected WAPE recovery of at least " + totalDegradation.ToString("N1") + "%"
                  InterventionConfidence = policy.InterventionConfidenceHigh }

            let evidenceRefs =
                sortedAssessments
                |> List.map(fun a ->
                    { ArtifactType = "ForecastQualityAssessment"
                      ArtifactId = PlanningScopeId.value a.Scope
                      PeriodStart = a.PeriodStart
                      PeriodEnd = a.PeriodEnd
                      SummaryStatistics = [ "Wape", a.Wape; "ForecastAccuracy", a.ForecastAccuracy ] |> Map.ofList })

            [ { Id = learningId
                Scope = scope
                LearningType = LearningType.ModelPerformanceDegradation
                LearningStatement = statement
                PatternConfidence = policy.PatternConfidenceHigh
                InterventionConfidence = policy.InterventionConfidenceHigh
                SupportingEvidence = evidenceRefs
                ImprovementOpportunities = [ opportunity ]
                PolicyVersion = "PO-D-048:v" + (string policy.Version)
                CreatedAt = timestamp } ]
        else
            []

/// Engine 4: Analyse classification and policy change lift (before/after accuracy comparison)
let analysePolicyAndClassificationEffectiveness
    (scope: PlanningScopeId)
    (windowStart: Timestamp)
    (windowEnd: Timestamp)
    (qualityAssessments: ForecastQualityAssessmentSummary list)
    (classificationChanges: ClassificationChangeSummary list)
    (policy: LearningAnalysisPolicy)
    (timestamp: Timestamp)
    : DemandLearning list =

    classificationChanges
    |> List.choose(fun change ->
        let beforeQuality =
            qualityAssessments
            |> List.filter(fun q -> q.PeriodEnd <= change.ChangedAt)
            |> List.sortBy(fun q -> q.PeriodStart)

        let afterQuality =
            qualityAssessments
            |> List.filter(fun q -> q.PeriodStart >= change.ChangedAt)
            |> List.sortBy(fun q -> q.PeriodStart)

        if beforeQuality.IsEmpty || afterQuality.Length < 2 then
            None
        else
            let avgWapeBefore = beforeQuality |> List.averageBy(fun q -> q.Wape)
            let avgWapeAfter = afterQuality |> List.averageBy(fun q -> q.Wape)
            let lift = avgWapeBefore - avgWapeAfter

            if lift >= policy.MinAccuracyLiftForClassification then
                let learningId =
                    DemandLearningId.ofComponents
                        scope
                        ("classification-lift-" + change.EntityId.ToLowerInvariant())
                        windowStart
                        windowEnd

                let statement =
                    "Reclassifying "
                    + change.EntityType
                    + " '"
                    + change.EntityId
                    + "' under scheme '"
                    + change.Scheme
                    + "' from '"
                    + change.PreviousClassification
                    + "' to '"
                    + change.NewClassification
                    + "' yielded an average WAPE improvement of "
                    + lift.ToString("N1")
                    + "% (WAPE dropped from "
                    + avgWapeBefore.ToString("N1")
                    + "% to "
                    + avgWapeAfter.ToString("N1")
                    + "% across "
                    + (string afterQuality.Length)
                    + " post-change periods)."

                let opportunity: ImprovementOpportunity =
                    { OpportunityId = "opp-ratify-classification-" + change.EntityId.ToLowerInvariant()
                      TargetCapability = "ClassifyDemand"
                      TargetPolicyId = "PO-D-037"
                      ProposedParameterChange =
                        "Ratify '"
                        + change.NewClassification
                        + "' classification policy threshold for "
                        + change.EntityType
                        + " category"
                      ExpectedBenefit = "Maintains sustained +" + lift.ToString("N1") + "% WAPE accuracy lift"
                      InterventionConfidence = policy.InterventionConfidenceHigh }

                let evidenceRefs =
                    (beforeQuality @ afterQuality)
                    |> List.map(fun q ->
                        { ArtifactType = "ForecastQualityAssessment"
                          ArtifactId = PlanningScopeId.value q.Scope
                          PeriodStart = q.PeriodStart
                          PeriodEnd = q.PeriodEnd
                          SummaryStatistics = [ "Wape", q.Wape ] |> Map.ofList })

                Some
                    { Id = learningId
                      Scope = scope
                      LearningType = LearningType.ClassificationImpact
                      LearningStatement = statement
                      PatternConfidence = policy.PatternConfidenceHigh
                      InterventionConfidence = policy.InterventionConfidenceHigh
                      SupportingEvidence = evidenceRefs
                      ImprovementOpportunities = [ opportunity ]
                      PolicyVersion = "PO-D-048:v" + (string policy.Version)
                      CreatedAt = timestamp }
            else
                None)

/// Engine 5: Analyse recurring exception hotspots
let analyseRecurringExceptionPatterns
    (scope: PlanningScopeId)
    (windowStart: Timestamp)
    (windowEnd: Timestamp)
    (exceptions: DemandExceptionSummary list)
    (policy: LearningAnalysisPolicy)
    (timestamp: Timestamp)
    : DemandLearning list =

    let exceptionClusters = exceptions |> List.groupBy(fun e -> (e.EntityType, e.EntityId, e.ExceptionType))

    exceptionClusters
    |> List.choose(fun ((entityType, entityId, exceptionType), cluster) ->
        if cluster.Length >= policy.MaxRecurrentExceptionCount then
            let learningId =
                DemandLearningId.ofComponents
                    scope
                    ("exception-" + entityId.ToLowerInvariant() + "-" + exceptionType.ToLowerInvariant())
                    windowStart
                    windowEnd

            let statement =
                "Entity "
                + entityType
                + " '"
                + entityId
                + "' experienced "
                + (string cluster.Length)
                + " recurring '"
                + exceptionType
                + "' exception alerts within the evaluation horizon, indicating chronic demand instability."

            let opportunity: ImprovementOpportunity =
                { OpportunityId = "opp-buffer-recalibration-" + entityId.ToLowerInvariant()
                  TargetCapability = "PrioritizeDemand"
                  TargetPolicyId = "PO-D-044"
                  ProposedParameterChange =
                    "Adjust safety stock and exception tolerance buffer for " + entityType + " '" + entityId + "'"
                  ExpectedBenefit = "Reduces operational exception frequency and stockout risk"
                  InterventionConfidence = policy.InterventionConfidenceMedium }

            let evidenceRefs =
                cluster
                |> List.map(fun e ->
                    { ArtifactType = "DemandExceptionEvidence"
                      ArtifactId = entityType + ":" + entityId
                      PeriodStart = e.DetectedAt
                      PeriodEnd = e.DetectedAt
                      SummaryStatistics =
                        [ "MetricValue", e.MetricValue; "ThresholdValue", e.ThresholdValue ] |> Map.ofList })

            Some
                { Id = learningId
                  Scope = scope
                  LearningType = LearningType.RecurringExceptionPattern
                  LearningStatement = statement
                  PatternConfidence = policy.PatternConfidenceMedium
                  InterventionConfidence = policy.InterventionConfidenceMedium
                  SupportingEvidence = evidenceRefs
                  ImprovementOpportunities = [ opportunity ]
                  PolicyVersion = "PO-D-048:v" + (string policy.Version)
                  CreatedAt = timestamp }
        else
            None)

// ---------- Main Multi-Period Analytical Discovery Composer (BA-D-013) ----------

/// Derives candidate Demand Learnings from multi-period historical evidence bundle
let deriveDemandLearnings
    (bundle: HistoricalDemandEvidenceBundle)
    (policy: LearningAnalysisPolicy)
    (timestamp: Timestamp)
    : DemandLearning list =

    let overrideLearnings =
        analyseOverrideEffectiveness
            bundle.Scope
            bundle.WindowStart
            bundle.WindowEnd
            bundle.PlannerOverrides
            policy
            timestamp

    let biasLearnings =
        analyseBiasPatterns bundle.Scope bundle.WindowStart bundle.WindowEnd bundle.QualityAssessments policy timestamp

    let modelLearnings =
        analyseModelDegradation
            bundle.Scope
            bundle.WindowStart
            bundle.WindowEnd
            bundle.QualityAssessments
            policy
            timestamp

    let classificationLearnings =
        analysePolicyAndClassificationEffectiveness
            bundle.Scope
            bundle.WindowStart
            bundle.WindowEnd
            bundle.QualityAssessments
            bundle.ClassificationChanges
            policy
            timestamp

    let exceptionLearnings =
        analyseRecurringExceptionPatterns
            bundle.Scope
            bundle.WindowStart
            bundle.WindowEnd
            bundle.DemandExceptions
            policy
            timestamp

    overrideLearnings @ biasLearnings @ modelLearnings @ classificationLearnings @ exceptionLearnings
