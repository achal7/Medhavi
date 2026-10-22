/// SE-S-070 — Production Schedule Projections & DTO Mappings
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-070)
module Medhavi.Supply.ScheduleProduction.ProductionSchedule.Projections

open System
open Medhavi.Contracts.Supply
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

let mapScheduleType (t: Medhavi.Supply.ScheduleType) : Medhavi.Contracts.Supply.ScheduleType =
    match t with
    | Medhavi.Supply.ScheduleType.Primary -> Medhavi.Contracts.Supply.ScheduleType.Primary
    | Medhavi.Supply.ScheduleType.Recovery -> Medhavi.Contracts.Supply.ScheduleType.Recovery
    | Medhavi.Supply.ScheduleType.Frozen -> Medhavi.Contracts.Supply.ScheduleType.Frozen
    | Medhavi.Supply.ScheduleType.Scenario -> Medhavi.Contracts.Supply.ScheduleType.Scenario

let mapLifecycleState (s: ProductionScheduleLifecycleState) : ScheduleLifecycleState =
    match s with
    | ProductionScheduleLifecycleState.Draft -> ScheduleLifecycleState.Draft
    | ProductionScheduleLifecycleState.Active -> ScheduleLifecycleState.Active
    | ProductionScheduleLifecycleState.Superseded -> ScheduleLifecycleState.Superseded
    | ProductionScheduleLifecycleState.Archived -> ScheduleLifecycleState.Archived

let mapCriticality (c: ScheduleCriticality) : ScheduleCriticalityLevel =
    match c with
    | ScheduleCriticality.CriticalPath -> ScheduleCriticalityLevel.CriticalPath
    | ScheduleCriticality.BottleneckConstrained -> ScheduleCriticalityLevel.BottleneckConstrained
    | ScheduleCriticality.Flexible -> ScheduleCriticalityLevel.Flexible

let mapConfidence (c: Medhavi.Supply.ScheduleConfidence) : ScheduleConfidenceLevel =
    match c with
    | Medhavi.Supply.ScheduleConfidence.High -> ScheduleConfidenceLevel.High
    | Medhavi.Supply.ScheduleConfidence.Medium -> ScheduleConfidenceLevel.Medium
    | Medhavi.Supply.ScheduleConfidence.Low -> ScheduleConfidenceLevel.Low

let mapChangeoverType (ct: ChangeoverType) : ChangeoverClassification =
    match ct with
    | ChangeoverType.ProductFamilyChange -> ChangeoverClassification.ProductFamilyChange
    | ChangeoverType.DeepWashdown -> ChangeoverClassification.DeepWashdown
    | ChangeoverType.StandardWashdown -> ChangeoverClassification.StandardWashdown
    | ChangeoverType.ToolChange -> ChangeoverClassification.ToolChange
    | ChangeoverType.FormatChange -> ChangeoverClassification.FormatChange

let mapActivityToDto (a: ProductionActivity) : ProductionActivityDto =
    let (ActivityId actIdStr) = a.ActivityId
    { ActivityId = actIdStr
      SequenceNumber = a.SequenceNumber
      ItemId = ItemId.value a.Item
      Quantity = Quantity.value a.Quantity
      StartTime = Timestamp.value a.StartTime
      EndTime = Timestamp.value a.EndTime
      DurationHours = a.DurationHours
      PrecedingActivityId = a.PrecedingActivityId |> Option.map (fun (ActivityId id) -> id)
      SucceedingActivityId = a.SucceedingActivityId |> Option.map (fun (ActivityId id) -> id)
      CampaignId = a.CampaignId
      Criticality = mapCriticality a.Criticality
      IsFrozen = a.IsFrozen }

let mapChangeoverToDto (c: ChangeoverDetail) : ChangeoverDetailDto =
    let (ChangeoverId chgIdStr) = c.ChangeoverId
    let (ActivityId fromIdStr) = c.FromActivityId
    let (ActivityId toIdStr) = c.ToActivityId
    { ChangeoverId = chgIdStr
      FromActivityId = fromIdStr
      ToActivityId = toIdStr
      ChangeoverType = mapChangeoverType c.ChangeoverType
      SetupHours = c.SetupHours
      CleaningHours = c.CleaningHours
      ToolingChangeHours = c.ToolingChangeHours }

let toDto (schedule: ProductionScheduleAggregate) : ProductionScheduleDto =
    let scheduleIdStr =
        $"SCHED-{PhysicalResourceId.value schedule.ResourceId}-{PlanningPeriodId.value schedule.PlanningPeriodId}-{schedule.ScheduleType}"

    let feasibilityStatus, conditions =
        match schedule.Feasibility.Overall with
        | Medhavi.Supply.ScheduleFeasibility.FullyFeasible -> ScheduleFeasibilityStatus.FullyFeasible, []
        | Medhavi.Supply.ScheduleFeasibility.ConditionallyFeasible conds -> ScheduleFeasibilityStatus.ConditionallyFeasible, conds
        | Medhavi.Supply.ScheduleFeasibility.Infeasible reasons -> ScheduleFeasibilityStatus.Infeasible, reasons

    { ScheduleId = scheduleIdStr
      ResourceId = PhysicalResourceId.value schedule.ResourceId
      PlanningPeriodId = PlanningPeriodId.value schedule.PlanningPeriodId
      ScheduleType = mapScheduleType schedule.ScheduleType
      VersionNumber = schedule.VersionNumber
      LifecycleState = mapLifecycleState schedule.LifecycleState
      Activities = schedule.Activities |> List.map mapActivityToDto
      Changeovers = schedule.Changeovers |> List.map mapChangeoverToDto
      TotalScheduledHours = schedule.TotalScheduledHours
      TotalChangeoverHours = schedule.TotalChangeoverHours
      FeasibilityStatus = feasibilityStatus
      FeasibilityConditions = conditions
      ConfidenceLevel = mapConfidence schedule.Confidence.Overall
      StabilityScore = schedule.Stability.StabilityScore
      SourceSupplyPlanVersion = $"v{schedule.SourceSupplyPlanVersion}"
      SchedulingPolicyVersion = $"v{schedule.SchedulingPolicyVersion}"
      Rationale = schedule.DecisionRationale
      CreatedTimestamp = Timestamp.value schedule.CreatedTimestamp
      ActivatedTimestamp = schedule.ActivatedTimestamp |> Option.map Timestamp.value }

/// Projects active schedule to read-only ActiveScheduleFact for baseline composition
let toActiveScheduleFact (schedule: ProductionScheduleAggregate) : ActiveScheduleFact =
    let scheduleIdStr =
        $"SCHED-{PhysicalResourceId.value schedule.ResourceId}-{PlanningPeriodId.value schedule.PlanningPeriodId}-{schedule.ScheduleType}"

    let utilization =
        if schedule.TotalScheduledHours > 0.0M then
            Math.Round(schedule.TotalScheduledHours / Math.Max(1.0M, schedule.TotalScheduledHours + schedule.TotalChangeoverHours), 4)
        else
            0.0M

    { ScheduleId = scheduleIdStr
      ResourceId = schedule.ResourceId
      PlanningPeriodId = schedule.PlanningPeriodId
      VersionNumber = schedule.VersionNumber
      Feasibility = schedule.Feasibility.Overall
      StabilityScore = schedule.Stability.StabilityScore
      TotalScheduledHours = schedule.TotalScheduledHours
      TotalChangeoverHours = schedule.TotalChangeoverHours
      ResourceUtilization = utilization
      OvertimeHours = schedule.OvertimeHours
      IsOvertimeDependent = schedule.OvertimeHours > 0.0M
      IsBottleneck = schedule.Activities |> List.exists (fun a -> a.Criticality = ScheduleCriticality.BottleneckConstrained)
      HasMaterialRisk =
        match schedule.Feasibility.MaterialAvailability with
        | DimensionFeasibility.Satisfied -> false
        | _ -> true
      HasDelayRisk =
        match schedule.Feasibility.Overall with
        | ScheduleFeasibility.ConditionallyFeasible _ -> true
        | _ -> false
      ActivityCount = schedule.Activities.Length }

/// Projection state: Map of schedule key to DTO
type State = Map<string, ProductionScheduleDto>

let initial: State = Map.empty

/// Pure projection fold (catamorphism)
let apply (state: State) (event: ProductionScheduleEvent) : State =
    match event with
    | ProductionScheduleActivated(schedule, _) ->
        let dto = toDto schedule
        state
        |> Map.add dto.ScheduleId dto
        |> Map.add (PhysicalResourceId.value schedule.ResourceId) dto
    | ProductionScheduleArchived schedule ->
        let dto = toDto schedule
        state
        |> Map.add dto.ScheduleId dto
        |> Map.add (PhysicalResourceId.value schedule.ResourceId) dto

/// Seed projection from existing aggregates
let seedFromAggregates (aggregates: ProductionScheduleAggregate list) : State =
    aggregates
    |> List.fold (fun st agg ->
        let dto = toDto agg
        st
        |> Map.add dto.ScheduleId dto
        |> Map.add (PhysicalResourceId.value agg.ResourceId) dto) initial
