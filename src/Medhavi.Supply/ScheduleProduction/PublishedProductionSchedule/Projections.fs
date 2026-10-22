/// SE-S-071 — Published Production Schedule Projections & DTO Mappings
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-071)
module Medhavi.Supply.ScheduleProduction.PublishedProductionSchedule.Projections

open System
open Medhavi.Contracts.Supply
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

let mapPublishedLifecycleState (s: PublishedScheduleLifecycleState) : Medhavi.Contracts.Supply.PublishedScheduleLifecycleState =
    match s with
    | PublishedScheduleLifecycleState.Draft -> Medhavi.Contracts.Supply.PublishedScheduleLifecycleState.Draft
    | PublishedScheduleLifecycleState.Published -> Medhavi.Contracts.Supply.PublishedScheduleLifecycleState.Published
    | PublishedScheduleLifecycleState.Superseded -> Medhavi.Contracts.Supply.PublishedScheduleLifecycleState.Superseded

let toDto (aggregate: PublishedProductionScheduleAggregate) : PublishedProductionScheduleDto =
    let scopeIdStr = PlanningScopeId.value aggregate.PlanningScopeId
    let periodIdStr = PlanningPeriodId.value aggregate.PlanningPeriodId
    let baselineIdStr = $"PUB-{scopeIdStr}-{periodIdStr}-v{aggregate.VersionNumber}"
    let supersededVersionIdStr =
        aggregate.SupersededVersionNumber
        |> Option.map (fun v -> $"PUB-{scopeIdStr}-{periodIdStr}-v{v}")

    { BaselineId = baselineIdStr
      PlanningScopeId = scopeIdStr
      PlanningPeriodId = periodIdStr
      VersionNumber = aggregate.VersionNumber
      LifecycleState = mapPublishedLifecycleState aggregate.LifecycleState
      ActiveScheduleIds = aggregate.ActiveScheduleIds
      TotalChangeoverHours = aggregate.Metrics.TotalChangeoverHours
      ResourceUtilization = aggregate.Metrics.ResourceUtilization
      OvertimeRequiredHours = aggregate.Metrics.OvertimeRequiredHours
      ScheduleAdherenceProjection = aggregate.Metrics.ScheduleAdherenceProjection
      FeasibleResourceCount = aggregate.Feasibility.FeasibleCount
      ConditionallyFeasibleResourceCount = aggregate.Feasibility.ConditionallyFeasibleCount
      InfeasibleResourceCount = aggregate.Feasibility.InfeasibleCount
      TopConstraints = aggregate.Feasibility.TopConstraints
      AverageStabilityScore = aggregate.Stability.AverageStabilityScore
      HighVolatilityResources = aggregate.Stability.HighVolatilityResources |> List.map PhysicalResourceId.value
      ResourcesAtRiskOfDelay = aggregate.Risks.ResourcesAtRiskOfDelay |> List.map PhysicalResourceId.value
      MaterialsAtRiskOfUnavailability = aggregate.Risks.MaterialsAtRiskOfUnavailability |> List.map PhysicalResourceId.value
      BottleneckResources = aggregate.Risks.BottleneckResources |> List.map PhysicalResourceId.value
      OvertimeDependentSchedules = aggregate.Risks.OvertimeDependentSchedules |> List.map PhysicalResourceId.value
      RecoverableResources = aggregate.Recovery.RecoverableResources |> List.map PhysicalResourceId.value
      EstimatedRecoverableHours = aggregate.Recovery.EstimatedRecoverableHours
      RecoveryStrategy = aggregate.Recovery.RecoveryStrategy
      PublishedTimestamp = aggregate.PublicationTimestamp |> Option.map Timestamp.value
      SupersededVersionId = supersededVersionIdStr }

/// Projection state: Map of baseline key to DTO
type State = Map<string, PublishedProductionScheduleDto>

let initial: State = Map.empty

/// Pure projection fold (catamorphism)
let apply (state: State) (event: PublishedProductionScheduleEvent) : State =
    match event with
    | ProductionSchedulePublished(baseline, _) ->
        let dto = toDto baseline
        state
        |> Map.add dto.BaselineId dto
        |> Map.add (PlanningScopeId.value baseline.PlanningScopeId) dto

/// Seed projection from existing aggregates
let seedFromAggregates (aggregates: PublishedProductionScheduleAggregate list) : State =
    aggregates
    |> List.fold (fun st agg ->
        let dto = toDto agg
        st
        |> Map.add dto.BaselineId dto
        |> Map.add (PlanningScopeId.value agg.PlanningScopeId) dto) initial
