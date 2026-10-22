/// SE-S-081 — Distribution Plan Domain Model
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-081, AB-S-071, EV-S-071)
module Medhavi.Supply.ManageDistribution.DistributionPlan.Model

open System
open Medhavi.SemanticModel
open Medhavi.Supply

[<RequireQualifiedAccess>]
type DistributionPlanLifecycleState =
    | Draft
    | Published
    | Superseded

[<RequireQualifiedAccess>]
type PublicationReason =
    | ScheduledCadence
    | MaterialChange
    | GovernanceOverride
    | EmergencyReplan

type RiskSignal =
    { RiskType: string
      Severity: RiskSeverity
      Description: string
      AffectedMovementCount: int }

type LaneUtilizationSummary =
    { Lane: TransportationLaneId
      TotalMovements: int
      TotalQuantity: Quantity
      UtilizationPercentage: decimal }

type NetworkBalanceSummary =
    { TotalInbound: Quantity
      TotalOutbound: Quantity
      NetBalance: Quantity }

type FeasibilitySummary =
    { FeasibleCount: int
      ConditionallyFeasibleCount: int
      InfeasibleCount: int
      OverallFeasibilityPercentage: decimal }

type StabilitySummary =
    { AverageStabilityScore: decimal
      TotalChangeCount: int
      HighVolatilityCount: int }

type ConfidenceDistribution =
    { HighConfidenceCount: int
      MediumConfidenceCount: int
      LowConfidenceCount: int
      Overall: DistributionConfidence }

type RecoveryOpportunity =
    { MovementReference: string
      CurrentLane: TransportationLaneId
      AlternativeLane: TransportationLaneId
      EstimatedImprovement: string
      RecoveryStrategy: string }

type RecommendationSignal =
    { SignalType: string
      Description: string
      AffectedMovementCount: int }

/// SE-S-081 Aggregate Root (Isolated from DistributionRecommendation internal state)
type DistributionPlanAggregate =
    { PlanId: DistributionPlanId
      PlanningScopeId: PlanningScopeId
      PlanningCycle: string
      VersionNumber: int
      LifecycleState: DistributionPlanLifecycleState
      PublicationReason: PublicationReason
      TotalMovementCount: int
      TotalQuantityMoved: Quantity
      LaneUtilizationSummary: LaneUtilizationSummary list
      NetworkBalanceSummary: NetworkBalanceSummary
      FeasibilitySummary: FeasibilitySummary
      StabilitySummary: StabilitySummary
      RiskSignals: RiskSignal list
      RecoveryOpportunities: RecoveryOpportunity list
      RecommendationSignals: RecommendationSignal list
      ConfidenceDistribution: ConfidenceDistribution
      SourceRecommendationIds: DistributionRequirementId list
      PublishedTimestamp: Timestamp option
      SupersededTimestamp: Timestamp option
      SupersededByVersion: int option }

/// Aggregate Command
type PublishDistributionPlanCmd =
    { PlanId: DistributionPlanId
      PlanningScopeId: PlanningScopeId
      PlanningCycle: string
      PublicationReason: PublicationReason
      Timestamp: Timestamp }

/// EV-S-071 — Distribution Plan Events
type DistributionPlanEvent =
    | DistributionPlanPublished of plan: DistributionPlanAggregate * supersededVersionOpt: int option

/// Pure catamorphic evolution
let evolve (stateOpt: DistributionPlanAggregate option) (event: DistributionPlanEvent) : DistributionPlanAggregate option =
    match event with
    | DistributionPlanPublished(plan, _) -> Some plan

let replay (events: DistributionPlanEvent seq) : DistributionPlanAggregate option =
    Seq.fold evolve None events
