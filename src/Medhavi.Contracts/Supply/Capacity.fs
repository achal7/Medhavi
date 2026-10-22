namespace Medhavi.Contracts.Supply

open System
open System.Threading.Tasks
open Medhavi.Contracts

[<RequireQualifiedAccess>]
type CapacityUtilizationStatus =
    | Underutilized
    | Optimal
    | NearCapacity
    | Overloaded

[<RequireQualifiedAccess>]
type CapacityConstraintStatus =
    | NonConstraint
    | NearConstraint
    | ActiveConstraint

[<RequireQualifiedAccess>]
type BottleneckAssessmentStatus =
    | NotABottleneck
    | PotentialBottleneck
    | CurrentBottleneck

[<RequireQualifiedAccess>]
type CapacityRiskLevel =
    | Negligible
    | Low
    | Medium
    | High
    | Critical

[<RequireQualifiedAccess>]
type CapacityHealthState =
    | CriticallyConstrained
    | Constrained
    | AtRisk
    | Underutilized
    | Balanced
    | Unassessed

type CapacityOperatingEnvelopeDto =
    { TargetLowerHours: decimal
      TargetUpperHours: decimal
      OvertimeThresholdHours: decimal
      MaxPlannedHours: decimal
      AbsoluteCapacityHours: decimal
      MinEconomicHours: decimal }

type CapacityPeriodBucketDto =
    { BucketIndex: int
      PeriodStart: DateTimeOffset
      PeriodEnd: DateTimeOffset
      AvailableCapacityHours: decimal
      PlannedLoadHours: decimal
      UtilizationPercentage: decimal
      OperatingEnvelope: CapacityOperatingEnvelopeDto
      UtilizationStatus: CapacityUtilizationStatus
      ConstraintStatus: CapacityConstraintStatus
      BottleneckStatus: BottleneckAssessmentStatus
      HealthState: CapacityHealthState }

type CapacityAssessmentChangeEventDto =
    { SequenceNumber: int
      ChangeTimestamp: DateTimeOffset
      PreviousUtilizationStatus: CapacityUtilizationStatus
      NewUtilizationStatus: CapacityUtilizationStatus
      PreviousConstraintStatus: CapacityConstraintStatus
      NewConstraintStatus: CapacityConstraintStatus
      PreviousBottleneckAssessment: BottleneckAssessmentStatus
      NewBottleneckAssessment: BottleneckAssessmentStatus
      TriggeringEvidence: string list
      ReasonCode: string }

type CapacityPositionAssessmentDto =
    { PhysicalResourceId: string
      LifecycleState: string
      AvailableCapacityHours: decimal
      CurrentLoadHours: decimal
      CurrentUtilizationPercentage: decimal
      ProjectedLoadHours: decimal
      ProjectedUtilizationPercentage: decimal
      OperatingEnvelope: CapacityOperatingEnvelopeDto
      TimePhasedBuckets: CapacityPeriodBucketDto list
      UtilizationStatus: CapacityUtilizationStatus
      ConstraintStatus: CapacityConstraintStatus
      BottleneckAssessment: BottleneckAssessmentStatus
      OverloadRisk: CapacityRiskLevel
      UnderloadRisk: CapacityRiskLevel
      SinglePointOfFailureRisk: bool
      AssessmentConfidence: string
      SourceSupplyUnderstandingVersion: int option
      SourceSupplyPlanVersion: int option
      AssessmentTimestamp: DateTimeOffset
      ChangeHistory: CapacityAssessmentChangeEventDto list }

type CapacityHealthAssessmentVersionDto =
    { VersionNumber: int
      LifecycleState: string
      AssessmentScopeId: string
      EvaluationPeriodId: string
      OverallCapacityHealthState: CapacityHealthState
      ClassificationRationale: string
      AssessmentConfidence: string
      AverageUtilizationPercentage: decimal
      PeakUtilizationPercentage: decimal
      UnderutilizedResourceCount: int
      OverloadedResourceCount: int
      CapacityStability: string
      CapacityVolatility: decimal
      BindingConstraints: string list
      BottleneckResources: string list
      OverloadRiskExposureHours: decimal
      UnderloadCostExposureHours: decimal
      BottleneckImpactSummary: string
      ChronicOverloadResources: string list
      ChronicUnderutilizedResources: string list
      RecurringBottlenecks: string list
      LoadBalancingOpportunities: string list
      FlexibilityImprovementOpportunities: string list
      EvidenceReferences: string list
      TransactionTimestamp: DateTimeOffset
      PublicationTimestamp: DateTimeOffset option }

type CapacityHealthAssessmentDto =
    { AssessmentScopeId: string
      EvaluationPeriodId: string
      CurrentVersion: CapacityHealthAssessmentVersionDto option
      AllVersions: CapacityHealthAssessmentVersionDto list }

// ---------- API Requests ----------

type UpdateCapacityPositionReq =
    { PhysicalResourceId: string
      TriggeringEvidence: string list }

type PublishCapacityHealthReq =
    { AssessmentScopeId: string
      EvaluationPeriodId: string }

// ---------- Business Notifications ----------

/// BN-S-030: Capacity Position Assessment Changed Notification
type CapacityPositionChangedNotification =
    { PhysicalResourceId: string
      NewUtilizationStatus: CapacityUtilizationStatus
      NewConstraintStatus: CapacityConstraintStatus
      NewBottleneckAssessment: BottleneckAssessmentStatus
      ChangeSummary: string
      Timestamp: DateTimeOffset }

/// BN-S-031: Capacity Health Assessment Published Notification
type CapacityHealthPublishedNotification =
    { AssessmentScopeId: string
      EvaluationPeriodId: string
      VersionNumber: int
      OverallCapacityHealthState: CapacityHealthState
      ClassificationRationale: string
      BottleneckCount: int
      Timestamp: DateTimeOffset }

// ---------- Capability & Aggregate Command API Contracts ----------

type CapacityPositionApi =
    { Update: UpdateCapacityPositionReq -> Task<Result<CapacityPositionAssessmentDto, ApiError>> }

type CapacityHealthApi =
    { Publish: PublishCapacityHealthReq -> Task<Result<CapacityHealthAssessmentDto, ApiError>> }

type ManageCapacityApis =
    { Position: CapacityPositionApi
      Health: CapacityHealthApi }

/// Query service aliases
type CapacityPositionQueries = QueryService<CapacityPositionAssessmentDto, string>
type CapacityHealthQueries = QueryService<CapacityHealthAssessmentDto, string>
