/// SE-S-070 — Production Schedule Domain Model
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-070, AB-S-060, EV-S-060)
module Medhavi.Supply.ScheduleProduction.ProductionSchedule.Model

open System
open Medhavi.SemanticModel
open Medhavi.Supply

type ActivityId = ActivityId of string
type ChangeoverId = ChangeoverId of string

/// Activity criticality classification (Supply Intelligence Specification §4.3.1, SE-S-070)
[<RequireQualifiedAccess>]
type ScheduleCriticality =
    | CriticalPath
    | BottleneckConstrained
    | Flexible

/// Governed changeover classification (Supply Intelligence Specification §4.3.1, SE-S-070)
[<RequireQualifiedAccess>]
type ChangeoverType =
    | ProductFamilyChange
    | DeepWashdown
    | StandardWashdown
    | ToolChange
    | FormatChange

/// Governed scheduling reason codes (Supply Intelligence Specification §10, BA-S-040)
[<RequireQualifiedAccess>]
type SchedulingReasonCode =
    | EarliestDueDateSequenced
    | ChangeoverMinimized
    | MaterialArrivalConstrained
    | FrozenHorizonProtected
    | OvertimeCapacityUtilized
    | LateDeliveryRiskDetected
    | CapacityOverloadInfeasible
    | DefaultSetupApplied

[<RequireQualifiedAccess>]
type ProductionScheduleLifecycleState =
    | Draft
    | Active
    | Superseded
    | Archived

[<RequireQualifiedAccess>]
type DimensionFeasibility =
    | Satisfied
    | Conditional of Condition: string
    | Violated of Reason: string
    | Unassessed of Reason: string

type FeasibilityEvaluation =
    { FiniteCapacity: DimensionFeasibility
      MaterialAvailability: DimensionFeasibility
      ToolAvailability: DimensionFeasibility
      LaborAvailability: DimensionFeasibility
      MaintenanceAndCalendar: DimensionFeasibility
      FrozenHorizon: DimensionFeasibility
      Overall: ScheduleFeasibility }

type ConfidenceEvaluation =
    { MaterialCertainty: ScheduleConfidence
      ResourceReliability: ScheduleConfidence
      ChangeoverAccuracy: ScheduleConfidence
      DemandStability: ScheduleConfidence
      Overall: ScheduleConfidence }

type StabilityMetrics =
    { StabilityScore: decimal
      ChangeCount: int
      ChangeMagnitude: decimal }

type ProductionActivity =
    { ActivityId: ActivityId
      SequenceNumber: int
      Item: ItemId
      Quantity: Quantity
      StartTime: Timestamp
      EndTime: Timestamp
      DurationHours: decimal
      PrecedingActivityId: ActivityId option
      SucceedingActivityId: ActivityId option
      CampaignId: string option
      Criticality: ScheduleCriticality
      IsFrozen: bool }

type ChangeoverDetail =
    { ChangeoverId: ChangeoverId
      FromActivityId: ActivityId
      ToActivityId: ActivityId
      ChangeoverType: ChangeoverType
      SetupHours: decimal
      CleaningHours: decimal option
      ToolingChangeHours: decimal option }

/// SE-S-070 Aggregate Root
type ProductionScheduleAggregate =
    { ResourceId: PhysicalResourceId
      PlanningPeriodId: PlanningPeriodId
      ScheduleType: ScheduleType
      VersionNumber: int
      LifecycleState: ProductionScheduleLifecycleState
      Activities: ProductionActivity list
      Changeovers: ChangeoverDetail list
      TotalScheduledHours: decimal
      TotalChangeoverHours: decimal
      OvertimeHours: decimal
      Feasibility: FeasibilityEvaluation
      Confidence: ConfidenceEvaluation
      Stability: StabilityMetrics
      SourceSupplyPlanVersion: int
      SchedulingPolicyVersion: int
      DecisionRationale: string
      EvidenceReferences: string list
      CreatedTimestamp: Timestamp
      ActivatedTimestamp: Timestamp option
      SupersededTimestamp: Timestamp option
      SupersededByVersion: int option }

type CreateProductionScheduleCmd =
    { ResourceId: PhysicalResourceId
      PlanningPeriodId: PlanningPeriodId
      ScheduleType: ScheduleType
      SourceSupplyPlanVersion: int
      SchedulingPolicyVersion: int
      Timestamp: Timestamp }

type ProductionScheduleEvent =
    | ProductionScheduleActivated of schedule: ProductionScheduleAggregate * supersededVersionOpt: int option
    | ProductionScheduleArchived of schedule: ProductionScheduleAggregate

let evolve (stateOpt: ProductionScheduleAggregate option) (event: ProductionScheduleEvent) : ProductionScheduleAggregate option =
    match event with
    | ProductionScheduleActivated(schedule, _) -> Some schedule
    | ProductionScheduleArchived schedule -> Some schedule

let replay (events: ProductionScheduleEvent seq) : ProductionScheduleAggregate option =
    Seq.fold evolve None events
