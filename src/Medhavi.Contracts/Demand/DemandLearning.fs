namespace Medhavi.Contracts.Demand.DemandLearning

open System
open System.Threading.Tasks
open Medhavi.Contracts

// =============================================================================
// Demand Explanation (SE‑D‑041)
// =============================================================================
type RecordDemandExplanationReq =
    { ExplanationId: string
      ExplainedArtifactType: string
      ExplainedArtifactId: string
      Question: string
      BusinessTime: DateTimeOffset }

type DemandExplanation =
    { ExplanationId: string
      ExplainedArtifactType: string
      ExplainedArtifactId: string
      NaturalLanguageExplanation: string
      ExplanationGenerationTimestamp: DateTimeOffset
      TemplateVersionRef: string
      BusinessTime: DateTimeOffset
      TransactionTime: DateTimeOffset }

type DemandExplanationApi =
    { RecordExplanation: RecordDemandExplanationReq -> Task<Result<string, ApiError>> }

type DemandExplanationQueries = QueryService<DemandExplanation, string>

type DemandExplanationRecordedNotification =
    { ExplanationId: string
      ExplainedArtifactType: string
      ExplainedArtifactId: string
      ExplanationGenerationTimestamp: DateTimeOffset }

// =============================================================================
// Demand Planning Condition (SE‑D‑040)
// =============================================================================
type RecognizeDemandPlanningConditionReq =
    { ConditionId: string
      PlanningEntity: string
      ConditionType: string
      DetectionEvidence: string
      DetectionTimestamp: DateTimeOffset
      PolicyVersionRef: string
      BusinessTime: DateTimeOffset }

type ResolveDemandPlanningConditionReq =
    { ConditionId: string
      ResolutionEvidence: string
      ResolutionTimestamp: DateTimeOffset }

type DemandPlanningCondition =
    { ConditionId: string
      PlanningEntity: string
      ConditionType: string
      CurrentStatus: string
      Severity: string
      DetectionEvidence: string
      DetectionTimestamp: DateTimeOffset
      ResolutionTimestamp: DateTimeOffset option
      ResolutionEvidence: string option
      BusinessTime: DateTimeOffset
      TransactionTime: DateTimeOffset }

type DemandPlanningConditionApi =
    { Recognize: RecognizeDemandPlanningConditionReq -> Task<Result<string, ApiError>>
      Resolve: ResolveDemandPlanningConditionReq -> Task<Result<unit, ApiError>> }

type DemandPlanningConditionQueries = QueryService<DemandPlanningCondition, string>

type DemandPlanningConditionDetectedNotification =
    { ConditionId: string
      PlanningEntity: string
      ConditionType: string
      Severity: string
      DetectionEvidence: string
      DetectionTimestamp: DateTimeOffset }

type DemandPlanningConditionResolvedNotification =
    { ConditionId: string
      PlanningEntity: string
      ConditionType: string
      ResolutionEvidence: string
      ResolutionTimestamp: DateTimeOffset }

// =============================================================================
// Demand Learning (SE‑D‑042)
// =============================================================================
type RecordDemandLearningReq =
    { PlanningScopeId: string
      LearningId: string
      LearningType: string
      LearningStatement: string
      SupportingEvidence: string list
      EvidenceStrength: string
      SourceAnalysisRef: string
      BusinessTime: DateTimeOffset }

type DemandLearning =
    { LearningId: string
      LearningType: string
      LearningStatement: string
      SupportingEvidence: string list
      EvidenceStrength: string
      SourceAnalysisRef: string
      BusinessTime: DateTimeOffset
      TransactionTime: DateTimeOffset }

type DemandLearningApi =
    { RecordLearning: RecordDemandLearningReq -> Task<Result<string, ApiError>> }

type DemandLearningQueries = QueryService<DemandLearning, string>

type DemandLearningRecordedNotification =
    { LearningId: string
      LearningType: string
      LearningStatement: string
      EvidenceStrength: string }
