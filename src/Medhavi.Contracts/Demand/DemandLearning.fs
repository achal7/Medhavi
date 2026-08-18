namespace Medhavi.Contracts.Demand

open System
open System.Threading.Tasks
open Medhavi.Contracts

// =============================================================================
// CA-D-010 & SE-D-011 — Demand Learning Public Contracts
// =============================================================================

/// Supporting historical evidence reference DTO
type EvidenceReferenceDto =
    { ArtifactType: string
      ArtifactId: string
      PeriodStart: DateTimeOffset
      PeriodEnd: DateTimeOffset
      SummaryStatistics: Map<string, decimal> }

/// Concrete actionable improvement opportunity DTO for cycle N+1
type ImprovementOpportunityDto =
    { OpportunityId: string
      TargetCapability: string
      TargetPolicyId: string
      ProposedParameterChange: string
      ExpectedBenefit: string
      InterventionConfidence: decimal }

/// Multi-period Forecast Quality summary DTO
type ForecastQualityAssessmentSummaryDto =
    { Scope: string
      PeriodStart: DateTimeOffset
      PeriodEnd: DateTimeOffset
      Wape: decimal
      ForecastBias: decimal
      TrackingSignal: decimal
      ForecastAccuracy: decimal
      CompletenessScore: decimal }

/// Historical planner override summary DTO
type PlannerOverrideSummaryDto =
    { PlannerId: string
      PublicationId: string
      Scope: string
      OverriddenAt: DateTimeOffset
      OriginalValue: decimal
      OverrideValue: decimal
      ActualValue: decimal option
      ReasonCode: string }

/// Historical demand exception summary DTO
type DemandExceptionSummaryDto =
    { ExceptionType: string
      EntityType: string
      EntityId: string
      Scope: string
      Severity: string
      TriggeringMetric: string
      MetricValue: decimal
      ThresholdValue: decimal
      DetectedAt: DateTimeOffset }

/// Historical classification change summary DTO
type ClassificationChangeSummaryDto =
    { EntityType: string
      EntityId: string
      Scope: string
      Scheme: string
      PreviousClassification: string
      NewClassification: string
      ChangedAt: DateTimeOffset }

/// Canonical historical evidence bundle DTO
type HistoricalDemandEvidenceBundleDto =
    { Scope: string
      WindowStart: DateTimeOffset
      WindowEnd: DateTimeOffset
      QualityAssessments: ForecastQualityAssessmentSummaryDto list
      PlannerOverrides: PlannerOverrideSummaryDto list
      DemandExceptions: DemandExceptionSummaryDto list
      ClassificationChanges: ClassificationChangeSummaryDto list }

/// Canonical SE-D-011 Demand Learning DTO
type DemandLearningDto =
    { LearningId: string
      Scope: string
      LearningType: string
      LearningStatement: string
      PatternConfidence: decimal
      InterventionConfidence: decimal
      SupportingEvidence: EvidenceReferenceDto list
      ImprovementOpportunities: ImprovementOpportunityDto list
      PolicyVersion: string
      Timestamp: DateTimeOffset }

// ---------- Commands / Requests ----------

/// Request payload to derive candidate learnings from historical evidence
type DeriveDemandLearningsReq =
    { Scope: string
      WindowStart: DateTimeOffset
      WindowEnd: DateTimeOffset
      EvidenceBundle: HistoricalDemandEvidenceBundleDto }

/// Request payload to record an immutable Demand Learning
type RecordDemandLearningReq =
    { LearningId: string
      Scope: string
      LearningType: string
      LearningStatement: string
      PatternConfidence: decimal
      InterventionConfidence: decimal
      SupportingEvidence: EvidenceReferenceDto list
      ImprovementOpportunities: ImprovementOpportunityDto list
      PolicyVersion: string }

// ---------- API Interface ----------

type DemandLearningApi =
    { DeriveLearnings: DeriveDemandLearningsReq -> Task<Result<DemandLearningDto list, ApiError>>
      RecordLearning: RecordDemandLearningReq -> Task<Result<DemandLearningDto, ApiError>> }

/// Query service alias
type DemandLearningQueries = QueryService<DemandLearningDto, string>
