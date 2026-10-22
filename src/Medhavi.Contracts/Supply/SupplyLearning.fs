// Medhavi.Contracts.Supply.SupplyLearning
// Traceability: SE-S-130 Supply Learning, CR-S-025, CR-S-026, EV-S-120, EV-S-121, BN-S-120, BN-S-121
namespace Medhavi.Contracts.Supply

open System
open System.Threading.Tasks
open Medhavi.Contracts

// =============================================================================
// CA-S-013 & SE-S-130 — Supply Learning Public Contracts
// =============================================================================

/// Preserved historical evidence reference DTO across planning cycles
type HistoricalEvidenceReferenceDto =
    { CapabilityId: string
      ArtifactType: string
      ArtifactId: string
      PeriodId: string
      PeriodStart: DateTimeOffset
      PeriodEnd: DateTimeOffset
      SummaryMetrics: Map<string, decimal> }

/// Actionable improvement opportunity DTO for cycle t+1
type ImprovementOpportunityDto =
    { OpportunityId: string
      TargetCapability: string
      TargetPolicyId: string
      TargetParameter: string
      CurrentBaselineValue: string
      RecommendedValue: string
      ExpectedBenefit: string
      InterventionConfidence: string
      AnalogueSuccessRate: decimal
      OperationalFeasibility: decimal }

/// Multi-period Supply Quality summary DTO
type SupplyQualitySummaryDto =
    { Scope: string
      PeriodStart: DateTimeOffset
      PeriodEnd: DateTimeOffset
      OverallScore: decimal
      QualityClassification: string
      DimensionalGaps: Map<string, decimal> }

/// Multi-period Supply Exception summary DTO
type SupplyExceptionSummaryDto =
    { ExceptionType: string
      EntityType: string
      EntityId: string
      Scope: string
      Severity: string
      MetricValue: decimal
      ThresholdValue: decimal
      DetectedAt: DateTimeOffset }

/// Multi-period Supply Plan stability summary DTO
type SupplyPlanStabilitySummaryDto =
    { Scope: string
      PeriodStart: DateTimeOffset
      PeriodEnd: DateTimeOffset
      AdherenceRate: decimal
      FrozenZoneViolations: int
      StabilityScore: decimal }

/// Multi-period Capacity utilization summary DTO
type CapacityUtilizationSummaryDto =
    { ResourceId: string
      PeriodStart: DateTimeOffset
      PeriodEnd: DateTimeOffset
      UtilizationRate: decimal
      PeakLoadRatio: decimal
      BottleneckHours: decimal }

/// Multi-period Supplier commitment summary DTO
type SupplierCommitmentSummaryDto =
    { SupplierId: string
      ItemId: string
      PeriodStart: DateTimeOffset
      PeriodEnd: DateTimeOffset
      OtifRate: decimal
      AcknowledgmentLatencyHours: decimal
      SlippageDays: decimal }

/// Multi-period Inventory position summary DTO
type InventoryPositionSummaryDto =
    { ItemLocationId: string
      PeriodStart: DateTimeOffset
      PeriodEnd: DateTimeOffset
      StockoutDays: int
      ExcessStockRatio: decimal
      ChurnRate: decimal }

/// Canonical historical evidence bundle DTO for BA-S-100 discovery
type HistoricalSupplyEvidenceBundleDto =
    { Scope: string
      WindowStart: DateTimeOffset
      WindowEnd: DateTimeOffset
      QualitySummaries: SupplyQualitySummaryDto list
      ExceptionSummaries: SupplyExceptionSummaryDto list
      PlanStabilitySummaries: SupplyPlanStabilitySummaryDto list
      CapacitySummaries: CapacityUtilizationSummaryDto list
      SupplierSummaries: SupplierCommitmentSummaryDto list
      InventorySummaries: InventoryPositionSummaryDto list }

/// Candidate supply learning produced by BA-S-100 discovery algorithm
type CandidateSupplyLearningDto =
    { PatternReference: string
      SubjectType: string
      SubjectId: string
      LearningType: string
      LearningScope: string
      TimeHorizonStart: DateTimeOffset
      TimeHorizonEnd: DateTimeOffset
      LearningStatement: string
      PatternConfidence: string
      PatternConfidenceScore: decimal
      RecurrenceCount: int
      RecurrenceFrequency: decimal
      StatisticalSignificance: decimal
      SupportingEvidence: HistoricalEvidenceReferenceDto list
      ImprovementOpportunities: ImprovementOpportunityDto list
      DerivedTimestamp: DateTimeOffset }

/// Authoritative, immutable SE-S-130 Supply Learning DTO
type SupplyLearningDto =
    { LearningId: string
      SubjectType: string
      SubjectId: string
      LearningType: string
      Version: int64
      LifecycleState: string
      PatternDescription: string
      RecurrenceCount: int
      RecurrenceFrequency: decimal
      StatisticalSignificance: decimal
      Scope: string
      TimeHorizonStart: DateTimeOffset
      TimeHorizonEnd: DateTimeOffset
      LearningStatement: string
      PatternConfidence: string
      PatternConfidenceScore: decimal
      ImprovementOpportunities: ImprovementOpportunityDto list
      Outcome: string
      OutcomeTimestamp: DateTimeOffset option
      OutcomeAuthority: string option
      OutcomeRationale: string option
      SupportingEvidence: HistoricalEvidenceReferenceDto list
      SourceEvidenceVersionSet: string list
      PolicyVersion: string
      GenerationTimestamp: DateTimeOffset
      SupersededLearningId: string option }

// =============================================================================
// Commands & Requests
// =============================================================================

/// Request payload to derive candidate supply learnings from multi-period historical evidence
type DeriveSupplyLearningsReq =
    { Scope: string
      WindowStart: DateTimeOffset
      WindowEnd: DateTimeOffset
      EvidenceBundle: HistoricalSupplyEvidenceBundleDto }

/// Request payload to record a Supply Learning in Pending Outcome state (AB-S-120)
type RecordSupplyLearningReq =
    { LearningId: string
      SubjectType: string
      SubjectId: string
      LearningType: string
      Scope: string
      TimeHorizonStart: DateTimeOffset
      TimeHorizonEnd: DateTimeOffset
      PatternDescription: string
      RecurrenceCount: int
      RecurrenceFrequency: decimal
      StatisticalSignificance: decimal
      LearningStatement: string
      PatternConfidence: string
      PatternConfidenceScore: decimal
      ImprovementOpportunities: ImprovementOpportunityDto list
      SupportingEvidence: HistoricalEvidenceReferenceDto list
      SourceEvidenceVersionSet: string list
      PolicyVersion: string }

/// Request payload to record the governance outcome on a candidate Supply Learning (AB-S-121)
type RecordLearningOutcomeReq =
    { LearningId: string
      Outcome: string
      Authority: string
      Rationale: string option }

// =============================================================================
// Business Notifications (BN-S-120 & BN-S-121)
// =============================================================================

/// BN-S-120 — Supply Learning Recorded notification payload
type SupplyLearningRecordedNotificationDto =
    { LearningId: string
      LearningStatementSummary: string
      LearningOutcome: string option
      PatternConfidence: string
      InterventionConfidenceSummary: string
      GenerationTimestamp: DateTimeOffset }

/// BN-S-121 — Supply Learning Outcome Recorded notification payload
type SupplyLearningOutcomeRecordedNotificationDto =
    { LearningId: string
      LearningOutcome: string
      RecordingTimestamp: DateTimeOffset
      Authority: string }

// =============================================================================
// Public Client API Interface
// =============================================================================

type SupplyLearningApi =
    { DeriveLearnings: DeriveSupplyLearningsReq -> Task<Result<CandidateSupplyLearningDto list, ApiError>>
      RecordLearning: RecordSupplyLearningReq -> Task<Result<SupplyLearningDto, ApiError>>
      RecordOutcome: RecordLearningOutcomeReq -> Task<Result<SupplyLearningDto, ApiError>>
      GetById: string -> Task<Result<SupplyLearningDto option, ApiError>> }

/// Query service alias for read models
type SupplyLearningQueries = QueryService<SupplyLearningDto, string>
