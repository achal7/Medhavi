// Medhavi.Supply.LearnFromSupply.SupplyLearning.ACL
// Traceability: Anti-Corruption Layer for CA-S-013 & SE-S-130
namespace Medhavi.Supply.LearnFromSupply.SupplyLearning

open System
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.SharedKernel
open Medhavi.Contracts
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model
open Algorithms

module ACL =

    let private validateNonEmpty (fieldName: string) (value: string) : Validation<string, DomainError> =
        if String.IsNullOrWhiteSpace value then
            Invalid [ DomainError.validation $"{fieldName} cannot be empty or whitespace" ]
        else
            Valid(value.Trim())

    let toHistoricalEvidenceRef
        (dto: HistoricalEvidenceReferenceDto)
        : Validation<HistoricalEvidenceReference, DomainError> =
        let create capId artType artId pId window =
            {
                CapabilityId = capId
                ArtifactType = artType
                ArtifactId = artId
                PeriodId = pId
                PeriodWindow = window
                SummaryMetrics = dto.SummaryMetrics
            }

        create <!> validateNonEmpty "CapabilityId" dto.CapabilityId
        <*> validateNonEmpty "ArtifactType" dto.ArtifactType
        <*> validateNonEmpty "ArtifactId" dto.ArtifactId
        <*> validateEvaluationPeriodId dto.PeriodId
        <*> validateTemporalWindow
                (Timestamp.ofUtc dto.PeriodStart)
                (Timestamp.ofUtc dto.PeriodEnd)

    let toImprovementOpportunity (dto: ImprovementOpportunityDto) : ImprovementOpportunity =
        {
            OpportunityId = dto.OpportunityId
            TargetCapability = dto.TargetCapability
            TargetPolicyId = dto.TargetPolicyId
            TargetParameter = dto.TargetParameter
            CurrentBaselineValue = dto.CurrentBaselineValue
            RecommendedValue = dto.RecommendedValue
            ExpectedBenefit = dto.ExpectedBenefit
            InterventionConfidence = ConfidenceLevel.FromString dto.InterventionConfidence
            AnalogueSuccessRate = dto.AnalogueSuccessRate
            OperationalFeasibility = dto.OperationalFeasibility
        }

    let toRecordCmd (req: RecordSupplyLearningReq) : Validation<RecordSupplyLearningCmd, DomainError> =
        let create learningId subjType subjId scope window =
            let evidence =
                req.SupportingEvidence
                |> List.choose(fun e ->
                    match toHistoricalEvidenceRef e with
                    | Valid ref -> Some ref
                    | Invalid _ -> None)

            let opps = req.ImprovementOpportunities |> List.map toImprovementOpportunity

            {
                LearningId = learningId
                SubjectType = subjType
                SubjectId = subjId
                LearningType = SupplyLearningType.FromString req.LearningType
                Scope = scope
                TimeHorizon = window
                PatternDescription = req.PatternDescription
                RecurrenceCount = req.RecurrenceCount
                RecurrenceFrequency = req.RecurrenceFrequency
                StatisticalSignificance = req.StatisticalSignificance
                LearningStatement = req.LearningStatement
                PatternConfidence = ConfidenceLevel.FromString req.PatternConfidence
                PatternConfidenceScore = req.PatternConfidenceScore
                ImprovementOpportunities = opps
                SupportingEvidence = evidence
                SourceEvidenceVersionSet = req.SourceEvidenceVersionSet
                PolicyVersion = req.PolicyVersion
                Timestamp = Timestamp.now()
            }

        create <!> validateSupplyLearningId req.LearningId
        <*> validateNonEmpty "SubjectType" req.SubjectType
        <*> validateNonEmpty "SubjectId" req.SubjectId
        <*> Valid(LearningScope.FromString req.Scope)
        <*> validateTemporalWindow
                (Timestamp.ofUtc req.TimeHorizonStart)
                (Timestamp.ofUtc req.TimeHorizonEnd)

    let toOutcomeCmd (req: RecordLearningOutcomeReq) : Validation<RecordLearningOutcomeCmd, DomainError> =
        let create learningId authority : RecordLearningOutcomeCmd =
            let outcome = LearningOutcome.FromString req.Outcome

            {
                LearningId = learningId
                Outcome = outcome
                Authority = authority
                Rationale = req.Rationale
                Timestamp = Timestamp.now()
            }

        create <!> validateSupplyLearningId req.LearningId
        <*> validateNonEmpty "Authority" req.Authority

    let toEvidenceCorpus (req: DeriveSupplyLearningsReq) : Validation<HistoricalSupplyEvidenceCorpus, DomainError> =
        let create scope window =
            let qualities =
                req.EvidenceBundle.QualitySummaries
                |> List.choose(fun q ->
                    let qScope = PlanningScopeId.create q.Scope |> Result.defaultValue scope

                    match
                        Timestamp.create q.PeriodStart,
                        Timestamp.create q.PeriodEnd
                    with
                    | Ok sTs, Ok eTs ->
                        Some
                            {
                                Scope = qScope
                                PeriodStart = sTs
                                PeriodEnd = eTs
                                OverallScore = q.OverallScore
                                QualityClassification = q.QualityClassification
                                DimensionalGaps = q.DimensionalGaps
                            }
                    | _ -> None)

            let exceptions =
                req.EvidenceBundle.ExceptionSummaries
                |> List.choose(fun e ->
                    let eScope = PlanningScopeId.create e.Scope |> Result.defaultValue scope

                    match Timestamp.create e.DetectedAt with
                    | Ok dTs ->
                        Some
                            {
                                ExceptionType = e.ExceptionType
                                EntityType = e.EntityType
                                EntityId = e.EntityId
                                Scope = eScope
                                Severity = e.Severity
                                MetricValue = e.MetricValue
                                ThresholdValue = e.ThresholdValue
                                DetectedAt = dTs
                            }
                    | _ -> None)

            let planStabilities =
                req.EvidenceBundle.PlanStabilitySummaries
                |> List.choose(fun p ->
                    let pScope = PlanningScopeId.create p.Scope |> Result.defaultValue scope

                    match
                        Timestamp.create p.PeriodStart,
                        Timestamp.create p.PeriodEnd
                    with
                    | Ok sTs, Ok eTs ->
                        Some
                            {
                                Scope = pScope
                                PeriodStart = sTs
                                PeriodEnd = eTs
                                AdherenceRate = p.AdherenceRate
                                FrozenZoneViolations = p.FrozenZoneViolations
                                StabilityScore = p.StabilityScore
                            }
                    | _ -> None)

            let capacities =
                req.EvidenceBundle.CapacitySummaries
                |> List.choose(fun c ->
                    match
                        Timestamp.create c.PeriodStart,
                        Timestamp.create c.PeriodEnd
                    with
                    | Ok sTs, Ok eTs ->
                        Some
                            {
                                ResourceId = c.ResourceId
                                PeriodStart = sTs
                                PeriodEnd = eTs
                                UtilizationRate = c.UtilizationRate
                                PeakLoadRatio = c.PeakLoadRatio
                                BottleneckHours = c.BottleneckHours
                            }
                    | _ -> None)

            let suppliers =
                req.EvidenceBundle.SupplierSummaries
                |> List.choose(fun s ->
                    match
                        Timestamp.create s.PeriodStart,
                        Timestamp.create s.PeriodEnd
                    with
                    | Ok sTs, Ok eTs ->
                        Some
                            {
                                SupplierId = s.SupplierId
                                ItemId = s.ItemId
                                PeriodStart = sTs
                                PeriodEnd = eTs
                                OtifRate = s.OtifRate
                                AcknowledgmentLatencyHours = s.AcknowledgmentLatencyHours
                                SlippageDays = s.SlippageDays
                            }
                    | _ -> None)

            let inventories =
                req.EvidenceBundle.InventorySummaries
                |> List.choose(fun i ->
                    match
                        Timestamp.create i.PeriodStart,
                        Timestamp.create i.PeriodEnd
                    with
                    | Ok sTs, Ok eTs ->
                        Some
                            {
                                ItemLocationId = i.ItemLocationId
                                PeriodStart = sTs
                                PeriodEnd = eTs
                                StockoutDays = i.StockoutDays
                                ExcessStockRatio = i.ExcessStockRatio
                                ChurnRate = i.ChurnRate
                            }
                    | _ -> None)

            {
                Scope = scope
                Window = window
                Qualities = qualities
                Exceptions = exceptions
                PlanStabilities = planStabilities
                Capacities = capacities
                Suppliers = suppliers
                Inventories = inventories
            }

        create <!> validatePlanningScopeId req.Scope
        <*> validateTemporalWindow
                (Timestamp.ofUtc req.WindowStart)
                (Timestamp.ofUtc req.WindowEnd)
