namespace Medhavi.Contracts.Supply

open System
open System.Threading.Tasks
open Medhavi.Contracts

[<RequireQualifiedAccess>]
type QualityClassification =
    | Excellent
    | Good
    | Adequate
    | Poor
    | Critical
    | Unassessed

[<RequireQualifiedAccess>]
type TrendTrajectory =
    | Improving
    | Stable
    | Deteriorating
    | Recovering
    | Volatile
    | Unassessed

[<RequireQualifiedAccess>]
type QualityConfidenceLevel =
    | High
    | Medium
    | Low

[<RequireQualifiedAccess>]
type QualityDimensionType =
    | Service
    | Inventory
    | Capacity
    | SupplierCollaboration
    | Procurement
    | Distribution
    | PlanningStability
    | Sustainability

[<RequireQualifiedAccess>]
type SupplyQualityLifecycleState =
    | Draft
    | Published
    | Superseded

[<RequireQualifiedAccess>]
type PublicationDecisionOutcome =
    | Publish
    | PublishWithFlags
    | Hold

type MetricEvaluationDto =
    { MetricName: string
      ActualValue: decimal
      ObjectiveTarget: decimal
      StretchTarget: decimal option
      MinimumAcceptable: decimal option
      CriticalThreshold: decimal option
      DeviationPercent: decimal
      Classification: string
      IsEffectiveness: bool }

type DimensionalEvaluationDto =
    { Dimension: string
      DimensionName: string
      IsMandatory: bool
      EvaluatedMetrics: MetricEvaluationDto list
      EffectivenessClassification: string
      EfficiencyClassification: string
      RecommendedClassification: string
      AuthoritativeClassification: string
      Trend: string
      Confidence: string
      DataCompleteness: decimal
      Score: decimal
      IssueCount: int
      EvidenceReferences: string list
      ReasonCodes: string list
      AffectedObjectives: string list }

type AssessmentContextComparatorDto =
    { HistoricalComparator: string option
      CrossScopeComparator: string option
      TargetVarianceSummary: string }

type OverallQualityStateDto =
    { OverallClassification: string
      OverallScore: decimal
      OverallConfidence: string
      SynthesisRationale: string
      PrimaryDrivers: string list
      TradeOffs: string option
      TriggeredRules: string list
      MandatoryDimensionStatus: Map<string, string> }

type SupplyQualityAssessmentDto =
    { AssessmentId: string
      PlanningScopeId: string
      PlanningHorizon: string
      EvaluationPeriodId: string
      VersionNumber: int
      LifecycleState: string
      DimensionalEvaluations: DimensionalEvaluationDto list
      OverallQuality: OverallQualityStateDto
      Comparators: AssessmentContextComparatorDto
      PublicationFlags: string list
      PolicyVersion: string
      CreatedTimestamp: DateTimeOffset
      PublicationTimestamp: DateTimeOffset option
      SupersededAssessmentId: string option }

type EvaluateSupplyQualityReq =
    { PlanningScopeId: string
      PlanningHorizon: string
      EvaluationPeriodId: string
      ForceMajeureDeclared: bool
      StrategicInvestmentNotes: string option }

type EvaluateSupplyQualityResp =
    { Assessment: SupplyQualityAssessmentDto
      PublicationOutcome: string
      PublicationFlags: string list
      NonPublicationReason: string option }

type SupplyQualityAssessmentPublishedEventDto =
    { EventId: string
      AssessmentId: string
      PlanningScopeId: string
      PlanningHorizon: string
      EvaluationPeriodId: string
      VersionNumber: int
      OverallClassification: string
      OverallScore: decimal
      PublicationTimestamp: DateTimeOffset }

type SupplyQualityAssessmentPublishedNotificationDto =
    { NotificationId: string
      EventId: string
      PlanningScopeId: string
      PlanningHorizon: string
      EvaluationPeriodId: string
      VersionNumber: int
      OverallClassification: string
      OverallScore: decimal
      OverallConfidence: string
      DimensionScores: Map<string, decimal>
      ConfidencePerDimension: Map<string, string>
      IssueCountsPerDimension: Map<string, int>
      PublicationFlags: string list
      Timestamp: DateTimeOffset }

// Capability & Aggregate Command API Contracts
type SupplyQualityApi =
    { Evaluate: EvaluateSupplyQualityReq -> Task<Result<SupplyQualityAssessmentDto, ApiError>> }

/// Query service alias
type SupplyQualityQueries = QueryService<SupplyQualityAssessmentDto, string>
