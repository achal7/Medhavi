/// BA-S-070 & BA-S-071 — Supply Quality Assessment Business Algorithms
/// Traces to: Supply Intelligence Specification §10 (BA-S-070, BA-S-071)
module Medhavi.Supply.EvaluateSupplyQuality.SupplyQualityAssessment.Algorithms

open System
open Model
open Policies

// =========================================================================
// BA-S-070 — Evaluate Supply Quality Dimensions
// =========================================================================

module EvaluateSupplyQualityDimensions =

    let private classifyHigherBetter (actual: decimal) (config: MetricThresholdConfig) =
        match config.Stretch, config.Minimum, config.Critical with
        | Some str, _, _ when actual >= str -> QualityClassification.Excellent
        | _, _, _ when actual >= config.Target -> QualityClassification.Good
        | _, Some min, _ when actual >= min -> QualityClassification.Adequate
        | _, _, Some crit when actual >= crit -> QualityClassification.Poor
        | _, _, Some _ -> QualityClassification.Critical
        | _ ->
            if actual >= config.Target then QualityClassification.Good
            else QualityClassification.Poor

    let private classifyLowerBetter (actual: decimal) (config: MetricThresholdConfig) =
        match config.Stretch, config.Minimum, config.Critical with
        | Some str, _, _ when actual <= str -> QualityClassification.Excellent
        | _, _, _ when actual <= config.Target -> QualityClassification.Good
        | _, Some min, _ when actual <= min -> QualityClassification.Adequate
        | _, _, Some crit when actual <= crit -> QualityClassification.Poor
        | _, _, Some _ -> QualityClassification.Critical
        | _ ->
            if actual <= config.Target then QualityClassification.Good
            else QualityClassification.Poor

    let evaluateMetric (config: MetricThresholdConfig) (actual: decimal) : MetricEvaluation =
        let deviation =
            if config.Target = 0.0m then 0.0m
            elif config.IsHigherBetter then
                Math.Round(((actual - config.Target) / Math.Abs(config.Target)) * 100.0m, 2)
            else
                Math.Round(((config.Target - actual) / Math.Abs(config.Target)) * 100.0m, 2)

        let classification =
            if config.IsHigherBetter then
                classifyHigherBetter actual config
            else
                classifyLowerBetter actual config

        { MetricName = config.MetricName
          ActualValue = actual
          ObjectiveTarget = config.Target
          StretchTarget = config.Stretch
          MinimumAcceptable = config.Minimum
          CriticalThreshold = config.Critical
          DeviationPercent = deviation
          Classification = classification
          IsEffectiveness = config.IsEffectiveness }

    let deriveTrend
        (currentClassification: QualityClassification)
        (priorClassification: QualityClassification option)
        (currentVal: decimal)
        (priorVal: decimal option)
        (deadband: decimal)
        (isHigherBetter: bool) : TrendTrajectory =
        match priorClassification, priorVal with
        | None, _ | _, None -> TrendTrajectory.Unassessed
        | Some priorClass, Some priorValNum ->
            let currentRank = QualityClassification.rank currentClassification
            let priorRank = QualityClassification.rank priorClass
            let delta = if isHigherBetter then currentVal - priorValNum else priorValNum - currentVal
            let rankDiff = Math.Abs(currentRank - priorRank)

            if rankDiff >= 2 then
                TrendTrajectory.Volatile
            elif (priorClass = QualityClassification.Poor || priorClass = QualityClassification.Critical) && QualityClassification.isAtLeast QualityClassification.Adequate currentClassification then
                TrendTrajectory.Recovering
            elif currentRank > priorRank || (currentRank = priorRank && delta > deadband) then
                TrendTrajectory.Improving
            elif currentRank < priorRank || (currentRank = priorRank && delta < -deadband) then
                TrendTrajectory.Deteriorating
            else
                TrendTrajectory.Stable

    let deriveConfidence (completeness: decimal) (timeliness: decimal) (consistency: decimal) : QualityConfidenceLevel =
        let gamma = completeness * timeliness * consistency
        if gamma >= 0.80m && completeness >= 1.0m then
            QualityConfidenceLevel.High
        elif gamma >= 0.50m && completeness >= 0.70m then
            QualityConfidenceLevel.Medium
        else
            QualityConfidenceLevel.Low

    let scoreOfClassification = function
        | QualityClassification.Excellent -> 98.0m
        | QualityClassification.Good -> 88.0m
        | QualityClassification.Adequate -> 72.0m
        | QualityClassification.Poor -> 50.0m
        | QualityClassification.Critical -> 25.0m
        | QualityClassification.Unassessed -> 0.0m

    let evaluateDimension
        (dimConfig: DimensionPolicyConfig)
        (actualMetrics: Map<string, decimal>)
        (priorMetrics: Map<string, decimal>)
        (priorClassifications: Map<QualityDimensionType, QualityClassification>)
        (evidenceCompleteness: decimal)
        (evidenceTimeliness: decimal)
        (sourceConsistency: decimal)
        (evidenceRefs: string list)
        (deadband: decimal) : DimensionalEvaluation =

        let evaluatedMetrics =
            dimConfig.Metrics
            |> List.choose (fun mConfig ->
                match Map.tryFind mConfig.MetricName actualMetrics with
                | Some actualVal -> Some (evaluateMetric mConfig actualVal)
                | None -> None)

        if evaluatedMetrics.IsEmpty then
            { Dimension = dimConfig.Dimension
              DimensionName = QualityDimensionType.name dimConfig.Dimension
              IsMandatory = dimConfig.IsMandatory
              EvaluatedMetrics = []
              EffectivenessClassification = QualityClassification.Unassessed
              EfficiencyClassification = QualityClassification.Unassessed
              RecommendedClassification = QualityClassification.Unassessed
              AuthoritativeClassification = QualityClassification.Unassessed
              Trend = TrendTrajectory.Unassessed
              Confidence = QualityConfidenceLevel.Low
              DataCompleteness = 0.0m
              Score = 0.0m
              IssueCount = 1
              EvidenceReferences = evidenceRefs
              ReasonCodes = [ "MandatoryEvidenceIncomplete" ]
              AffectedObjectives = [ dimConfig.ObjectiveDescription ] }
        else
            let effectivenessMetrics = evaluatedMetrics |> List.filter (fun m -> m.IsEffectiveness)
            let efficiencyMetrics = evaluatedMetrics |> List.filter (fun m -> not m.IsEffectiveness)

            let effClassification =
                if effectivenessMetrics.IsEmpty then QualityClassification.Good
                else effectivenessMetrics |> List.map (fun m -> m.Classification) |> List.reduce QualityClassification.min

            let effcClassification =
                if efficiencyMetrics.IsEmpty then QualityClassification.Good
                else efficiencyMetrics |> List.map (fun m -> m.Classification) |> List.reduce QualityClassification.min

            // Trade-off evaluation
            let recommendedClassification, tradeOffReason =
                if QualityClassification.isAtLeast QualityClassification.Good effClassification &&
                   (effcClassification = QualityClassification.Poor || effcClassification = QualityClassification.Critical) then
                    QualityClassification.Adequate, Some "EfficiencySubordinatedToEffectiveness"
                else
                    QualityClassification.min effClassification effcClassification, None

            let primaryMetric = evaluatedMetrics.Head
            let primaryConfig = dimConfig.Metrics.Head
            let priorValOpt = Map.tryFind primaryConfig.MetricName priorMetrics
            let priorClassOpt = Map.tryFind dimConfig.Dimension priorClassifications

            let trend =
                deriveTrend
                    recommendedClassification
                    priorClassOpt
                    primaryMetric.ActualValue
                    priorValOpt
                    deadband
                    primaryConfig.IsHigherBetter

            let confidence = deriveConfidence evidenceCompleteness evidenceTimeliness sourceConsistency

            let reasonCodes =
                [ match recommendedClassification with
                  | QualityClassification.Excellent -> "QualityTargetExceeded"
                  | QualityClassification.Good -> "QualityTargetMet"
                  | QualityClassification.Adequate -> "QualityWithinAcceptableTolerance"
                  | QualityClassification.Poor -> "QualityTargetMissed"
                  | QualityClassification.Critical -> "QualitySeverelyDegraded"
                  | QualityClassification.Unassessed -> "MandatoryEvidenceIncomplete"

                  match tradeOffReason with
                  | Some r -> r
                  | None -> ()

                  match trend with
                  | TrendTrajectory.Improving -> "TrendImprovingPositiveTrajectory"
                  | TrendTrajectory.Deteriorating -> "TrendDeterioratingNegativeShift"
                  | TrendTrajectory.Recovering -> "TrendRecoveringFromCritical"
                  | TrendTrajectory.Stable -> "TrendStableNominalVariance"
                  | TrendTrajectory.Volatile -> "PerformanceVolatilityDetected"
                  | TrendTrajectory.Unassessed -> () ]

            let score = scoreOfClassification recommendedClassification

            let issues =
                evaluatedMetrics
                |> List.filter (fun m -> m.Classification = QualityClassification.Poor || m.Classification = QualityClassification.Critical)
                |> List.length

            { Dimension = dimConfig.Dimension
              DimensionName = QualityDimensionType.name dimConfig.Dimension
              IsMandatory = dimConfig.IsMandatory
              EvaluatedMetrics = evaluatedMetrics
              EffectivenessClassification = effClassification
              EfficiencyClassification = effcClassification
              RecommendedClassification = recommendedClassification
              AuthoritativeClassification = recommendedClassification
              Trend = trend
              Confidence = confidence
              DataCompleteness = evidenceCompleteness
              Score = score
              IssueCount = issues
              EvidenceReferences = evidenceRefs
              ReasonCodes = reasonCodes
              AffectedObjectives = [ dimConfig.ObjectiveDescription ] }

// =========================================================================
// BA-S-071 — Derive Overall Supply Quality Assessment
// =========================================================================

module DeriveOverallSupplyQualityAssessment =

    let synthesizeOverall
        (policy: SupplyQualityPolicy)
        (dimensionalEvaluations: DimensionalEvaluation list)
        (forceMajeure: bool)
        (contextNotes: string option) : OverallQualityState =

        let mandatoryEvaluations = dimensionalEvaluations |> List.filter (fun d -> d.IsMandatory)
        let optionalEvaluations = dimensionalEvaluations |> List.filter (fun d -> not d.IsMandatory)

        let mandatoryStatus =
            mandatoryEvaluations
            |> List.map (fun d -> d.Dimension, d.AuthoritativeClassification)
            |> Map.ofList

        let unassessedMandatory =
            mandatoryEvaluations
            |> List.filter (fun d -> d.AuthoritativeClassification = QualityClassification.Unassessed)

        let criticalMandatory =
            mandatoryEvaluations
            |> List.filter (fun d -> d.AuthoritativeClassification = QualityClassification.Critical)

        let poorMandatory =
            mandatoryEvaluations
            |> List.filter (fun d -> d.AuthoritativeClassification = QualityClassification.Poor)

        let overallConfidence =
            if mandatoryEvaluations.IsEmpty then QualityConfidenceLevel.Low
            else mandatoryEvaluations |> List.map (fun d -> d.Confidence) |> List.reduce QualityConfidenceLevel.min

        // Synthesis cascade according to PO-S-110 & BA-S-071
        let overallClass, triggeredRules, rationale =
            if not unassessedMandatory.IsEmpty then
                let missingNames = unassessedMandatory |> List.map (fun d -> d.DimensionName) |> String.concat ", "
                QualityClassification.Unassessed,
                [ "BR-S-285 Mandatory Dimension Completeness Guard" ],
                $"Synthesis halted: Mandatory dimensions lacking conclusive assessment: {missingNames}."

            elif criticalMandatory.Length >= 2 then
                let names = criticalMandatory |> List.map (fun d -> d.DimensionName) |> String.concat ", "
                QualityClassification.Critical,
                [ "BR-S-281 Mandatory Dimension Veto Rule" ],
                $"Overall quality is Critical: multiple mandatory dimensions breached critical failure thresholds ({names})."

            elif criticalMandatory.Length = 1 then
                let name = criticalMandatory.Head.DimensionName
                QualityClassification.Poor,
                [ "BR-S-281 Mandatory Dimension Veto Rule"; "PO-S-110 Mandatory Dimension Constraint" ],
                $"Overall quality capped at Poor due to Critical status in mandatory dimension '{name}'."

            elif not poorMandatory.IsEmpty then
                let names = poorMandatory |> List.map (fun d -> d.DimensionName) |> String.concat ", "
                QualityClassification.Adequate,
                [ "BR-S-282 Mandatory Dimension Capping Cascade" ],
                $"Overall quality capped at Adequate due to Poor status in mandatory dimensions: {names}."

            else
                // All mandatory dimensions are at least Adequate
                let minMandatory =
                    mandatoryEvaluations
                    |> List.map (fun d -> d.AuthoritativeClassification)
                    |> List.reduce QualityClassification.min

                let allMandatoryGoodOrBetter =
                    mandatoryEvaluations
                    |> List.forall (fun d -> QualityClassification.isAtLeast QualityClassification.Good d.AuthoritativeClassification)

                let optionalBonus =
                    allMandatoryGoodOrBetter &&
                    optionalEvaluations |> List.exists (fun d -> d.AuthoritativeClassification = QualityClassification.Excellent)

                let finalClass =
                    if optionalBonus && minMandatory = QualityClassification.Good then
                        QualityClassification.Excellent
                    else
                        minMandatory

                let bonusNote = if optionalBonus then " Boosted by top-tier performance in optional dimensions." else ""
                finalClass,
                [ "BR-S-283 Pure Qualitative Synthesis Governance" ],
                $"Overall quality synthesized as {finalClass} from mandatory dimensions.{bonusNote}"

        // Adjust for force majeure if applicable per PO-S-110
        let adjustedClass, finalRationale =
            if forceMajeure && overallClass <> QualityClassification.Unassessed && overallClass <> QualityClassification.Critical then
                let downgraded =
                    match overallClass with
                    | QualityClassification.Excellent -> QualityClassification.Good
                    | QualityClassification.Good -> QualityClassification.Adequate
                    | QualityClassification.Adequate -> QualityClassification.Poor
                    | other -> other
                let noteText = contextNotes |> Option.defaultValue ""
                downgraded, $"{rationale} Adjusted down 1 level due to declared Force Majeure context ({noteText})."
            else
                overallClass, rationale

        // Calculate weighted composite score
        let weightedScore =
            let totalWeight = policy.Dimensions |> List.sumBy (fun d -> d.Weight)
            if totalWeight = 0.0m then 0.0m
            else
                let sum =
                    policy.Dimensions
                    |> List.sumBy (fun dimCfg ->
                        match dimensionalEvaluations |> List.tryFind (fun d -> d.Dimension = dimCfg.Dimension) with
                        | Some eval -> eval.Score * dimCfg.Weight
                        | None -> 0.0m)
                Math.Round(sum / totalWeight, 2)

        let primaryDrivers =
            dimensionalEvaluations
            |> List.filter (fun d ->
                d.AuthoritativeClassification = QualityClassification.Critical ||
                d.AuthoritativeClassification = QualityClassification.Poor ||
                d.AuthoritativeClassification = QualityClassification.Excellent)
            |> List.map (fun d -> d.Dimension)

        let tradeOffs =
            dimensionalEvaluations
            |> List.tryFind (fun d -> d.ReasonCodes |> List.contains "EfficiencySubordinatedToEffectiveness")
            |> Option.map (fun d -> $"{d.DimensionName}: High effectiveness was achieved at the expense of operational efficiency.")

        { OverallClassification = adjustedClass
          OverallScore = weightedScore
          OverallConfidence = overallConfidence
          SynthesisRationale = finalRationale
          PrimaryDrivers = primaryDrivers
          TradeOffs = tradeOffs
          TriggeredRules = triggeredRules
          MandatoryDimensionStatus = mandatoryStatus }
