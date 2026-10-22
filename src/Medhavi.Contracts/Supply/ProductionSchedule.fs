namespace Medhavi.Contracts.Supply

open System
open System.Threading.Tasks
open Medhavi.Contracts

[<RequireQualifiedAccess>]
type ScheduleType =
    | Primary
    | Recovery
    | Frozen
    | Scenario

[<RequireQualifiedAccess>]
type ScheduleLifecycleState =
    | Draft
    | Active
    | Superseded
    | Archived

[<RequireQualifiedAccess>]
type PublishedScheduleLifecycleState =
    | Draft
    | Published
    | Superseded

[<RequireQualifiedAccess>]
type ScheduleFeasibilityStatus =
    | FullyFeasible
    | ConditionallyFeasible
    | Infeasible

[<RequireQualifiedAccess>]
type ScheduleCriticalityLevel =
    | CriticalPath
    | BottleneckConstrained
    | Flexible

[<RequireQualifiedAccess>]
type ScheduleConfidenceLevel =
    | High
    | Medium
    | Low

[<RequireQualifiedAccess>]
type ChangeoverClassification =
    | ProductFamilyChange
    | DeepWashdown
    | StandardWashdown
    | ToolChange
    | FormatChange

type ProductionActivityDto =
    { ActivityId: string
      SequenceNumber: int
      ItemId: string
      Quantity: decimal
      StartTime: DateTimeOffset
      EndTime: DateTimeOffset
      DurationHours: decimal
      PrecedingActivityId: string option
      SucceedingActivityId: string option
      CampaignId: string option
      Criticality: ScheduleCriticalityLevel
      IsFrozen: bool }

type ChangeoverDetailDto =
    { ChangeoverId: string
      FromActivityId: string
      ToActivityId: string
      ChangeoverType: ChangeoverClassification
      SetupHours: decimal
      CleaningHours: decimal option
      ToolingChangeHours: decimal option }

type ProductionScheduleDto =
    { ScheduleId: string
      ResourceId: string
      PlanningPeriodId: string
      ScheduleType: ScheduleType
      VersionNumber: int
      LifecycleState: ScheduleLifecycleState
      Activities: ProductionActivityDto list
      Changeovers: ChangeoverDetailDto list
      TotalScheduledHours: decimal
      TotalChangeoverHours: decimal
      FeasibilityStatus: ScheduleFeasibilityStatus
      FeasibilityConditions: string list
      ConfidenceLevel: ScheduleConfidenceLevel
      StabilityScore: decimal
      SourceSupplyPlanVersion: string
      SchedulingPolicyVersion: string
      Rationale: string
      CreatedTimestamp: DateTimeOffset
      ActivatedTimestamp: DateTimeOffset option }

type PublishedProductionScheduleDto =
    { BaselineId: string
      PlanningScopeId: string
      PlanningPeriodId: string
      VersionNumber: int
      LifecycleState: PublishedScheduleLifecycleState
      ActiveScheduleIds: string list
      TotalChangeoverHours: decimal
      ResourceUtilization: decimal
      OvertimeRequiredHours: decimal
      ScheduleAdherenceProjection: decimal
      FeasibleResourceCount: int
      ConditionallyFeasibleResourceCount: int
      InfeasibleResourceCount: int
      TopConstraints: string list
      AverageStabilityScore: decimal
      HighVolatilityResources: string list
      ResourcesAtRiskOfDelay: string list
      MaterialsAtRiskOfUnavailability: string list
      BottleneckResources: string list
      OvertimeDependentSchedules: string list
      RecoverableResources: string list
      EstimatedRecoverableHours: decimal
      RecoveryStrategy: string
      PublishedTimestamp: DateTimeOffset option
      SupersededVersionId: string option }

/// Query service aliases
type ProductionScheduleQueries = QueryService<ProductionScheduleDto, string>
type PublishedProductionScheduleQueries = QueryService<PublishedProductionScheduleDto, string>

// API Request Records
type CreateProductionScheduleReq =
    { ResourceId: string
      PlanningPeriodId: string
      ScheduleType: ScheduleType
      Timestamp: DateTimeOffset }

type PublishProductionScheduleReq =
    { PlanningScopeId: string
      PlanningPeriodId: string
      TriggerType: string
      Timestamp: DateTimeOffset }

// Business Notifications (Dispatched post-persistence by Workflow Notification Nodes)
type ProductionScheduleActivatedNotification =
    { ResourceId: string
      PlanningPeriodId: string
      ScheduleId: string
      VersionNumber: int
      ScheduleType: ScheduleType
      OverallFeasibility: ScheduleFeasibilityStatus
      OverallConfidence: ScheduleConfidenceLevel
      StabilityScore: decimal
      TotalScheduledHours: decimal
      TotalChangeoverHours: decimal
      DeliveryGuarantee: string }

type ProductionSchedulePublishedNotification =
    { BaselineId: string
      PlanningScopeId: string
      PlanningPeriodId: string
      PublishedVersionNumber: int
      FeasibleResourceCount: int
      ConditionallyFeasibleResourceCount: int
      InfeasibleResourceCount: int
      AverageStabilityScore: decimal
      HighVolatilityResources: string list
      ResourcesAtRiskOfDelay: string list
      MaterialsAtRiskOfUnavailability: string list
      BottleneckResources: string list
      OvertimeDependentSchedules: string list
      DeliveryGuarantee: string }

// Capability & Aggregate Command API Contracts
type ProductionScheduleApi =
    { Create: CreateProductionScheduleReq -> Task<Result<ProductionScheduleDto, ApiError>> }

type PublishedProductionScheduleApi =
    { Publish: PublishProductionScheduleReq -> Task<Result<PublishedProductionScheduleDto, ApiError>> }

type ScheduleProductionApis =
    { Schedule: ProductionScheduleApi
      PublishedBaseline: PublishedProductionScheduleApi }

