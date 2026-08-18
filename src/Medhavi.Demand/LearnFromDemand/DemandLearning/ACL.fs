module Medhavi.Demand.LearnFromDemand.DemandLearning.ACL

open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Validations
open Medhavi.SemanticModel
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Model

let evidenceRefFromDto (dto: EvidenceReferenceDto) : Validation<EvidenceReference, DomainError> =
    let create startTs endTs =
        { ArtifactType = dto.ArtifactType
          ArtifactId = dto.ArtifactId
          PeriodStart = startTs
          PeriodEnd = endTs
          SummaryStatistics = dto.SummaryStatistics }

    create <!> validateTimestamp dto.PeriodStart <*> validateTimestamp dto.PeriodEnd

let opportunityFromDto (dto: ImprovementOpportunityDto) : ImprovementOpportunity =
    { OpportunityId = dto.OpportunityId
      TargetCapability = dto.TargetCapability
      TargetPolicyId = dto.TargetPolicyId
      ProposedParameterChange = dto.ProposedParameterChange
      ExpectedBenefit = dto.ExpectedBenefit
      InterventionConfidence = dto.InterventionConfidence }

/// Translates RecordDemandLearningReq into EstablishLearningCmd using Applicative validation
let toRecordCmd (req: RecordDemandLearningReq) : Validation<EstablishLearningCmd, DomainError> =
    let create learningId scope =
        let evidenceList =
            req.SupportingEvidence
            |> List.choose(fun e ->
                match evidenceRefFromDto e with
                | Valid ref -> Some ref
                | Invalid _ -> None)

        { LearningId = learningId
          Scope = scope
          LearningType = LearningType.FromString req.LearningType
          LearningStatement = req.LearningStatement
          PatternConfidence = req.PatternConfidence
          InterventionConfidence = req.InterventionConfidence
          SupportingEvidence = evidenceList
          ImprovementOpportunities = req.ImprovementOpportunities |> List.map opportunityFromDto
          PolicyVersion = req.PolicyVersion
          Timestamp = Timestamp.now() }

    create <!> validateDemandLearningId req.LearningId <*> validatePlanningScopeId req.Scope

/// Translates DeriveDemandLearningsReq into HistoricalDemandEvidenceBundle using Applicative validation
let toEvidenceBundle (req: DeriveDemandLearningsReq) : Validation<HistoricalDemandEvidenceBundle, DomainError> =
    let create scope startTs endTs =
        let qualitySummaries: ForecastQualityAssessmentSummary list =
            req.EvidenceBundle.QualityAssessments
            |> List.choose(fun q ->
                let qScope = PlanningScopeId.create q.Scope |> Result.defaultValue scope

                match Timestamp.create q.PeriodStart, Timestamp.create q.PeriodEnd with
                | Ok pStart, Ok pEnd ->
                    Some
                        { Scope = qScope
                          PeriodStart = pStart
                          PeriodEnd = pEnd
                          Wape = q.Wape
                          ForecastBias = q.ForecastBias
                          TrackingSignal = q.TrackingSignal
                          ForecastAccuracy = q.ForecastAccuracy
                          CompletenessScore = q.CompletenessScore }
                | _ -> None)

        let overrideSummaries: PlannerOverrideSummary list =
            req.EvidenceBundle.PlannerOverrides
            |> List.choose(fun o ->
                let oScope = PlanningScopeId.create o.Scope |> Result.defaultValue scope

                match Timestamp.create o.OverriddenAt with
                | Ok oTs ->
                    Some
                        { PlannerId = o.PlannerId
                          PublicationId = o.PublicationId
                          Scope = oScope
                          OverriddenAt = oTs
                          OriginalValue = o.OriginalValue
                          OverrideValue = o.OverrideValue
                          ActualValue = o.ActualValue
                          ReasonCode = o.ReasonCode }
                | _ -> None)

        let exceptionSummaries: DemandExceptionSummary list =
            req.EvidenceBundle.DemandExceptions
            |> List.choose(fun e ->
                let eScope = PlanningScopeId.create e.Scope |> Result.defaultValue scope

                match Timestamp.create e.DetectedAt with
                | Ok dTs ->
                    Some
                        { ExceptionType = e.ExceptionType
                          EntityType = e.EntityType
                          EntityId = e.EntityId
                          Scope = eScope
                          Severity = e.Severity
                          TriggeringMetric = e.TriggeringMetric
                          MetricValue = e.MetricValue
                          ThresholdValue = e.ThresholdValue
                          DetectedAt = dTs }
                | _ -> None)

        let classificationSummaries: ClassificationChangeSummary list =
            req.EvidenceBundle.ClassificationChanges
            |> List.choose(fun c ->
                let cScope = PlanningScopeId.create c.Scope |> Result.defaultValue scope

                match Timestamp.create c.ChangedAt with
                | Ok cTs ->
                    Some
                        { EntityType = c.EntityType
                          EntityId = c.EntityId
                          Scope = cScope
                          Scheme = c.Scheme
                          PreviousClassification = c.PreviousClassification
                          NewClassification = c.NewClassification
                          ChangedAt = cTs }
                | _ -> None)

        { Scope = scope
          WindowStart = startTs
          WindowEnd = endTs
          QualityAssessments = qualitySummaries
          PlannerOverrides = overrideSummaries
          DemandExceptions = exceptionSummaries
          ClassificationChanges = classificationSummaries }

    create <!> validatePlanningScopeId req.Scope
    <*> validateTimestamp req.WindowStart
    <*> validateTimestamp req.WindowEnd
