/// SE-S-100 — Supply Quality Assessment Anti-Corruption Layer
/// Traces to: FS-S-090 (Evaluate Supply Performance)
module Medhavi.Supply.EvaluateSupplyQuality.SupplyQualityAssessment.ACL

open System
open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Medhavi.Contracts.Supply
open Model

let mapClassificationToDto (c: QualityClassification) : string =
    match c with
    | QualityClassification.Excellent -> "Excellent"
    | QualityClassification.Good -> "Good"
    | QualityClassification.Adequate -> "Adequate"
    | QualityClassification.Poor -> "Poor"
    | QualityClassification.Critical -> "Critical"
    | QualityClassification.Unassessed -> "Unassessed"

let mapTrendToDto (t: TrendTrajectory) : string =
    match t with
    | TrendTrajectory.Improving -> "Improving"
    | TrendTrajectory.Stable -> "Stable"
    | TrendTrajectory.Deteriorating -> "Deteriorating"
    | TrendTrajectory.Recovering -> "Recovering"
    | TrendTrajectory.Volatile -> "Volatile"
    | TrendTrajectory.Unassessed -> "Unassessed"

let mapConfidenceToDto (conf: QualityConfidenceLevel) : string =
    match conf with
    | QualityConfidenceLevel.High -> "High"
    | QualityConfidenceLevel.Medium -> "Medium"
    | QualityConfidenceLevel.Low -> "Low"

let mapLifecycleStateToDto (state: SupplyQualityLifecycleState) : string =
    match state with
    | SupplyQualityLifecycleState.Draft -> "Draft"
    | SupplyQualityLifecycleState.Published -> "Published"
    | SupplyQualityLifecycleState.Superseded -> "Superseded"

let mapMetricEvaluationToDto (m: MetricEvaluation) : MetricEvaluationDto =
    {
        MetricName = m.MetricName
        ActualValue = m.ActualValue
        ObjectiveTarget = m.ObjectiveTarget
        StretchTarget = m.StretchTarget
        MinimumAcceptable = m.MinimumAcceptable
        CriticalThreshold = m.CriticalThreshold
        DeviationPercent = m.DeviationPercent
        Classification = mapClassificationToDto m.Classification
        IsEffectiveness = m.IsEffectiveness
    }

let mapDimensionalEvaluationToDto (d: DimensionalEvaluation) : DimensionalEvaluationDto =
    {
        Dimension = d.Dimension.ToString()
        DimensionName = d.DimensionName
        IsMandatory = d.IsMandatory
        EvaluatedMetrics = d.EvaluatedMetrics |> List.map mapMetricEvaluationToDto
        EffectivenessClassification = mapClassificationToDto d.EffectivenessClassification
        EfficiencyClassification = mapClassificationToDto d.EfficiencyClassification
        RecommendedClassification = mapClassificationToDto d.RecommendedClassification
        AuthoritativeClassification = mapClassificationToDto d.AuthoritativeClassification
        Trend = mapTrendToDto d.Trend
        Confidence = mapConfidenceToDto d.Confidence
        DataCompleteness = d.DataCompleteness
        Score = d.Score
        IssueCount = d.IssueCount
        EvidenceReferences = d.EvidenceReferences
        ReasonCodes = d.ReasonCodes
        AffectedObjectives = d.AffectedObjectives
    }

let mapComparatorsToDto (c: AssessmentContextComparator) : AssessmentContextComparatorDto =
    {
        HistoricalComparator = c.HistoricalComparator
        CrossScopeComparator = c.CrossScopeComparator
        TargetVarianceSummary = c.TargetVarianceSummary
    }

let mapOverallQualityToDto (o: OverallQualityState) : OverallQualityStateDto =
    {
        OverallClassification = mapClassificationToDto o.OverallClassification
        OverallScore = o.OverallScore
        OverallConfidence = mapConfidenceToDto o.OverallConfidence
        SynthesisRationale = o.SynthesisRationale
        PrimaryDrivers = o.PrimaryDrivers |> List.map(fun d -> d.ToString())
        TradeOffs = o.TradeOffs
        TriggeredRules = o.TriggeredRules
        MandatoryDimensionStatus =
            o.MandatoryDimensionStatus
            |> Map.toList
            |> List.map(fun (k, v) -> k.ToString(), mapClassificationToDto v)
            |> Map.ofList
    }

let mapVersionToDto
    (assessmentId: SupplyQualityAssessmentId)
    (scopeId: PlanningScopeId)
    (horizon: PlanningHorizon)
    (periodId: EvaluationPeriodId)
    (supersededId: string option)
    (v: SupplyQualityAssessmentVersion)
    : SupplyQualityAssessmentDto =
    {
        AssessmentId = SupplyQualityAssessmentId.value assessmentId
        PlanningScopeId = PlanningScopeId.value scopeId
        PlanningHorizon =
            sprintf
                "%s/%s"
                ((Timestamp.value horizon.Start).ToString("O"))
                ((Timestamp.value horizon.End).ToString("O"))
        EvaluationPeriodId = EvaluationPeriodId.value periodId
        VersionNumber = v.VersionNumber
        LifecycleState = mapLifecycleStateToDto v.LifecycleState
        DimensionalEvaluations = v.DimensionalEvaluations |> List.map mapDimensionalEvaluationToDto
        OverallQuality = mapOverallQualityToDto v.OverallQuality
        Comparators = mapComparatorsToDto v.Comparators
        PublicationFlags = v.PublicationFlags
        PolicyVersion = v.PolicyVersion
        CreatedTimestamp = Timestamp.value v.CreatedTimestamp
        PublicationTimestamp = v.PublicationTimestamp |> Option.map Timestamp.value
        SupersededAssessmentId = supersededId
    }

let mapAggregateToDto (sqa: SupplyQualityAssessment) : SupplyQualityAssessmentDto option =
    match sqa.Versions |> List.tryFind(fun v -> v.LifecycleState = SupplyQualityLifecycleState.Published) with
    | Some published ->
        let supersededId =
            sqa.Versions
            |> List.tryFind(fun v ->
                v.LifecycleState = SupplyQualityLifecycleState.Superseded && v.VersionNumber < published.VersionNumber)
            |> Option.map(fun v ->
                SupplyQualityAssessmentId.ofScopeHorizonPeriodVersion
                    sqa.PlanningScopeId
                    sqa.PlanningHorizon
                    sqa.EvaluationPeriodId
                    v.VersionNumber
                |> SupplyQualityAssessmentId.value)

        Some(
            mapVersionToDto
                sqa.AssessmentId
                sqa.PlanningScopeId
                sqa.PlanningHorizon
                sqa.EvaluationPeriodId
                supersededId
                published
        )
    | None ->
        match sqa.Versions |> List.tryHead with
        | Some latest ->
            Some(
                mapVersionToDto
                    sqa.AssessmentId
                    sqa.PlanningScopeId
                    sqa.PlanningHorizon
                    sqa.EvaluationPeriodId
                    None
                    latest
            )
        | None -> None

let toEvaluateCmd
    (req: EvaluateSupplyQualityReq)
    (prereqs: ScopePrerequisitesFact)
    (outcomes: DimensionalOutcomeFact list)
    (evalContext: QualityEvaluationContextFact option)
    : Validation<EvaluateSupplyQualityCmd, DomainError> =
    (fun scopeId horizon periodId ->
        {
            PlanningScopeId = scopeId
            PlanningHorizon = horizon
            EvaluationPeriodId = periodId
            ForceMajeureDeclared = req.ForceMajeureDeclared
            StrategicInvestmentNotes = req.StrategicInvestmentNotes
            Prerequisites = prereqs
            DimensionalOutcomes = outcomes
            EvaluationContext = evalContext
            Timestamp = Timestamp.now()
        })
    <!> validatePlanningScopeId req.PlanningScopeId
    <*> (PlanningHorizon.createFromString req.PlanningHorizon
         |> Result.mapError mapSemanticValidationToDomainError
         |> fromResult)
    <*> validateEvaluationPeriodId req.EvaluationPeriodId

let toArsEventId =
    function
    | SupplyQualityAssessmentDraftCreated _ -> ArsIdentifiers.EnterpriseEvents.supplyQualityAssessmentPublished.Id
    | SupplyQualityAssessmentPublished _ -> ArsIdentifiers.EnterpriseEvents.supplyQualityAssessmentPublished.Id
