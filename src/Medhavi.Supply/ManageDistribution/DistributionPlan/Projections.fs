/// SE-S-081 — Distribution Plan Projections
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-081)
module Medhavi.Supply.ManageDistribution.DistributionPlan.Projections

open System
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model

let private mapPlanLifecycleState = function
    | DistributionPlanLifecycleState.Draft -> Medhavi.Contracts.Supply.DistributionPlanLifecycleState.Draft
    | DistributionPlanLifecycleState.Published -> Medhavi.Contracts.Supply.DistributionPlanLifecycleState.Published
    | DistributionPlanLifecycleState.Superseded -> Medhavi.Contracts.Supply.DistributionPlanLifecycleState.Superseded

let private mapPublicationReason = function
    | PublicationReason.ScheduledCadence -> Medhavi.Contracts.Supply.PublicationReasonType.ScheduledCadence
    | PublicationReason.MaterialChange -> Medhavi.Contracts.Supply.PublicationReasonType.MaterialChange
    | PublicationReason.GovernanceOverride -> Medhavi.Contracts.Supply.PublicationReasonType.GovernanceOverride
    | PublicationReason.EmergencyReplan -> Medhavi.Contracts.Supply.PublicationReasonType.EmergencyReplan

let private mapConfidence = function
    | DistributionConfidence.High -> Medhavi.Contracts.Supply.DistributionConfidenceLevel.High
    | DistributionConfidence.Medium -> Medhavi.Contracts.Supply.DistributionConfidenceLevel.Medium
    | DistributionConfidence.Low -> Medhavi.Contracts.Supply.DistributionConfidenceLevel.Low

let private mapLaneUtilization (l: LaneUtilizationSummary) : LaneUtilizationSummaryDto =
    { LaneId = TransportationLaneId.value l.Lane
      TotalMovementsOnLane = l.TotalMovements
      TotalQuantityOnLane = Quantity.value l.TotalQuantity
      UtilizationPercentage = l.UtilizationPercentage }

let private mapNetworkBalance (n: NetworkBalanceSummary) : NetworkBalanceSummaryDto =
    { TotalInboundQuantity = Quantity.value n.TotalInbound
      TotalOutboundQuantity = Quantity.value n.TotalOutbound
      NetBalance = Quantity.value n.NetBalance }

let private mapFeasibility (f: FeasibilitySummary) : FeasibilitySummaryDto =
    { FeasibleCount = f.FeasibleCount
      ConditionallyFeasibleCount = f.ConditionallyFeasibleCount
      InfeasibleCount = f.InfeasibleCount
      OverallFeasibilityPercentage = f.OverallFeasibilityPercentage }

let private mapStability (s: StabilitySummary) : StabilitySummaryDto =
    { AverageStabilityScore = s.AverageStabilityScore
      TotalChangeCount = s.TotalChangeCount
      HighVolatilityMovementCount = s.HighVolatilityCount }

let private mapConfidenceDist (c: ConfidenceDistribution) : ConfidenceDistributionDto =
    { HighConfidenceCount = c.HighConfidenceCount
      MediumConfidenceCount = c.MediumConfidenceCount
      LowConfidenceCount = c.LowConfidenceCount
      OverallPlanConfidence = mapConfidence c.Overall }

let private mapRecovery (r: RecoveryOpportunity) : RecoveryOpportunityDto =
    { MovementReference = r.MovementReference
      CurrentLaneId = TransportationLaneId.value r.CurrentLane
      AlternativeLaneId = TransportationLaneId.value r.AlternativeLane
      EstimatedImprovement = r.EstimatedImprovement
      RecoveryStrategy = r.RecoveryStrategy }

let private mapSignal (s: RecommendationSignal) : RecommendationSignalDto =
    { SignalType = s.SignalType
      Description = s.Description
      AffectedMovementCount = s.AffectedMovementCount }

let private mapRisk (r: RiskSignal) : RiskSignalDto =
    { RiskType = r.RiskType
      Severity = sprintf "%A" r.Severity
      Description = r.Description
      AffectedMovementCount = r.AffectedMovementCount }

let toDto (plan: DistributionPlanAggregate) : DistributionPlanDto =
    let planIdStr = DistributionPlanId.value plan.PlanId
    { PlanId = planIdStr
      PlanningScopeId = PlanningScopeId.value plan.PlanningScopeId
      PlanningCycle = plan.PlanningCycle
      VersionNumber = plan.VersionNumber
      LifecycleState = mapPlanLifecycleState plan.LifecycleState
      PublicationReason = mapPublicationReason plan.PublicationReason
      TotalMovementCount = plan.TotalMovementCount
      TotalQuantityMoved = Quantity.value plan.TotalQuantityMoved
      LaneUtilizationSummary = plan.LaneUtilizationSummary |> List.map mapLaneUtilization
      NetworkBalanceSummary = mapNetworkBalance plan.NetworkBalanceSummary
      FeasibilitySummary = mapFeasibility plan.FeasibilitySummary
      StabilitySummary = mapStability plan.StabilitySummary
      RiskSignals = plan.RiskSignals |> List.map mapRisk
      RecoveryOpportunities = plan.RecoveryOpportunities |> List.map mapRecovery
      RecommendationSignals = plan.RecommendationSignals |> List.map mapSignal
      ConfidenceDistribution = mapConfidenceDist plan.ConfidenceDistribution
      SourceRecommendationIds = plan.SourceRecommendationIds |> List.map DistributionRequirementId.value
      PublishedTimestamp = plan.PublishedTimestamp |> Option.map Timestamp.value
      SupersededVersionId = plan.SupersededByVersion |> Option.map (fun v -> sprintf "%s#v%d" planIdStr v) }

type State = Map<DistributionPlanId, DistributionPlanDto>

let seedFromAggregates (aggregates: DistributionPlanAggregate list) : State =
    aggregates
    |> List.map (fun p -> p.PlanId, toDto p)
    |> Map.ofList

let apply (state: State) (event: DistributionPlanEvent) : State =
    match event with
    | DistributionPlanPublished(plan, _) ->
        state |> Map.add plan.PlanId (toDto plan)
