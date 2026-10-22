namespace Medhavi.Contracts.Supply

open System
open System.Threading.Tasks
open Medhavi.Contracts

[<RequireQualifiedAccess>]
type DistributionRecommendationLifecycleState =
    | Active
    | Superseded
    | Withdrawn

[<RequireQualifiedAccess>]
type DistributionPlanLifecycleState =
    | Draft
    | Published
    | Superseded

[<RequireQualifiedAccess>]
type DistributionFeasibilityStatus =
    | FullyFeasible
    | ConditionallyFeasible
    | Infeasible

[<RequireQualifiedAccess>]
type DistributionConfidenceLevel =
    | High
    | Medium
    | Low

[<RequireQualifiedAccess>]
type DistributionStrategyType =
    | PreferredSource
    | AlternateSource
    | CrossDock
    | Deferral

[<RequireQualifiedAccess>]
type AllocationPolicyType =
    | FairShare
    | PriorityBased
    | ServiceLevelPriority

[<RequireQualifiedAccess>]
type DeploymentPriorityType =
    | ServiceProtection
    | PromotionSupport
    | NewProductLaunch
    | SeasonalPositioning
    | InventoryBalancing

[<RequireQualifiedAccess>]
type PublicationReasonType =
    | ScheduledCadence
    | MaterialChange
    | GovernanceOverride
    | EmergencyReplan

type SourceLocationAllocationDto =
    { LocationId: string
      Quantity: decimal
      DepartureTiming: DateTimeOffset }

type LaneUsageDto =
    { LaneId: string
      TransitTimeHours: decimal
      LaneCapacityUtilization: decimal }

type FeasibilityConstraintDto =
    { ConstraintType: string
      Description: string
      AffectedDimension: string }

type RiskSignalDto =
    { RiskType: string
      Severity: string
      Description: string
      AffectedMovementCount: int }

type RecoveryOpportunityDto =
    { MovementReference: string
      CurrentLaneId: string
      AlternativeLaneId: string
      EstimatedImprovement: string
      RecoveryStrategy: string }

type RecommendationSignalDto =
    { SignalType: string
      Description: string
      AffectedMovementCount: int }

type LaneUtilizationSummaryDto =
    { LaneId: string
      TotalMovementsOnLane: int
      TotalQuantityOnLane: decimal
      UtilizationPercentage: decimal }

type NetworkBalanceSummaryDto =
    { TotalInboundQuantity: decimal
      TotalOutboundQuantity: decimal
      NetBalance: decimal }

type FeasibilitySummaryDto =
    { FeasibleCount: int
      ConditionallyFeasibleCount: int
      InfeasibleCount: int
      OverallFeasibilityPercentage: decimal }

type StabilitySummaryDto =
    { AverageStabilityScore: decimal
      TotalChangeCount: int
      HighVolatilityMovementCount: int }

type ConfidenceDistributionDto =
    { HighConfidenceCount: int
      MediumConfidenceCount: int
      LowConfidenceCount: int
      OverallPlanConfidence: DistributionConfidenceLevel }

type DistributionRecommendationDto =
    { RecommendationId: string
      RequirementReference: string
      VersionNumber: int
      LifecycleState: DistributionRecommendationLifecycleState
      ItemId: string
      RequiredQuantity: decimal
      DestinationLocationId: string
      RequiredDeliveryDate: DateTimeOffset
      SourceLocations: SourceLocationAllocationDto list
      LanesUsed: LaneUsageDto list
      DepartureTiming: DateTimeOffset
      ExpectedArrivalTiming: DateTimeOffset
      Strategy: DistributionStrategyType
      AllocationPolicyUsed: AllocationPolicyType option
      AllocatedQuantity: decimal option
      DeferredQuantity: decimal option
      DeferralReason: string option
      DeploymentPriority: DeploymentPriorityType
      FeasibilityStatus: DistributionFeasibilityStatus
      FeasibilityConstraints: FeasibilityConstraintDto list
      ConfidenceLevel: DistributionConfidenceLevel
      StabilityScore: decimal
      ChangeCount: int
      RiskSignals: RiskSignalDto list
      ReasonCodes: string list
      DecisionRationale: string
      DecisionEvidenceReferences: string list
      CreatedTimestamp: DateTimeOffset
      SupersededVersionId: string option }

type DistributionPlanDto =
    { PlanId: string
      PlanningScopeId: string
      PlanningCycle: string
      VersionNumber: int
      LifecycleState: DistributionPlanLifecycleState
      PublicationReason: PublicationReasonType
      TotalMovementCount: int
      TotalQuantityMoved: decimal
      LaneUtilizationSummary: LaneUtilizationSummaryDto list
      NetworkBalanceSummary: NetworkBalanceSummaryDto
      FeasibilitySummary: FeasibilitySummaryDto
      StabilitySummary: StabilitySummaryDto
      RiskSignals: RiskSignalDto list
      RecoveryOpportunities: RecoveryOpportunityDto list
      RecommendationSignals: RecommendationSignalDto list
      ConfidenceDistribution: ConfidenceDistributionDto
      SourceRecommendationIds: string list
      PublishedTimestamp: DateTimeOffset option
      SupersededVersionId: string option }

/// Query service aliases
type DistributionRecommendationQueries = QueryService<DistributionRecommendationDto, string>
type DistributionPlanQueries = QueryService<DistributionPlanDto, string>

// API Request Records
type CreateDistributionRecommendationReq =
    { RequirementReference: string
      Timestamp: DateTimeOffset }

type PublishDistributionPlanReq =
    { PlanningScopeId: string
      PlanningCycle: string
      PublicationReason: PublicationReasonType
      Timestamp: DateTimeOffset }

// Business Notifications (Dispatched post-persistence by Workflow Notification Nodes)
type DistributionRecommendationCreatedNotification =
    { NotificationId: string
      ReferencedEventId: string
      RequirementReference: string
      VersionNumber: int
      ItemId: string
      RequiredQuantity: decimal
      DestinationLocationId: string
      DepartureTiming: DateTimeOffset
      ExpectedArrivalTiming: DateTimeOffset
      FeasibilityStatus: DistributionFeasibilityStatus
      ConfidenceLevel: DistributionConfidenceLevel
      Timestamp: DateTimeOffset }

type DistributionPlanPublishedNotification =
    { NotificationId: string
      ReferencedEventId: string
      PlanningScopeId: string
      PlanningCycle: string
      VersionNumber: int
      TotalMovementCount: int
      TotalQuantityMoved: decimal
      FeasibilityPercentage: decimal
      AverageStabilityScore: decimal
      Timestamp: DateTimeOffset }

// Capability & Aggregate Command API Contracts
type DistributionRecommendationApi =
    { Create: CreateDistributionRecommendationReq -> Task<Result<DistributionRecommendationDto, ApiError>> }

type DistributionPlanApi =
    { Publish: PublishDistributionPlanReq -> Task<Result<DistributionPlanDto, ApiError>> }

type ManageDistributionApis =
    { Recommendation: DistributionRecommendationApi
      Plan: DistributionPlanApi }
