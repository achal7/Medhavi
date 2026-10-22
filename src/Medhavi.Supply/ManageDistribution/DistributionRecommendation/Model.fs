/// SE-S-080 — Distribution Recommendation Domain Model
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-080, AB-S-070, EV-S-070)
module Medhavi.Supply.ManageDistribution.DistributionRecommendation.Model

open System
open Medhavi.SemanticModel
open Medhavi.Supply

type MovementId = MovementId of string

[<RequireQualifiedAccess>]
type DistributionRecommendationLifecycleState =
    | Active
    | Superseded
    | Withdrawn

[<RequireQualifiedAccess>]
type AllocationPolicy =
    | FairShare
    | PriorityBased
    | ServiceLevelPriority

[<RequireQualifiedAccess>]
type DistributionReasonCode =
    | OptimalLaneSelected
    | SourceStockAllocated
    | TransitTimeFeasible
    | DestinationDockThroughputVerified
    | InsufficientSourceInventory
    | LaneCapacityExceeded
    | DockCongestionBacklog
    | CircularRoutingProhibited
    | ExpeditedDeployment

[<RequireQualifiedAccess>]
type DimensionFeasibility =
    | Satisfied
    | Conditional of Condition: string
    | Violated of Reason: string
    | Unassessed of Reason: string

type FeasibilityEvaluation =
    { SourceInventoryAvailability: DimensionFeasibility
      LaneCapacityAvailability: DimensionFeasibility
      TransitTimeFeasibility: DimensionFeasibility
      DestinationReceivingCapacity: DimensionFeasibility
      TimeWindowAchievability: DimensionFeasibility
      Overall: DistributionFeasibility }

type ConfidenceEvaluation =
    { InventoryCertainty: DistributionConfidence
      LaneReliability: DistributionConfidence
      TransitTimeAccuracy: DistributionConfidence
      ReceivingCapacityConfidence: DistributionConfidence
      DemandVolatility: DistributionConfidence
      DisruptionExposure: DistributionConfidence
      Overall: DistributionConfidence }

type StabilityMetrics =
    { StabilityScore: decimal
      ChangeCount: int
      ChangeMagnitudeQuantity: decimal
      ChangeMagnitudeTimingHours: decimal
      LaneChangesCount: int }

type SourceLocationAllocation =
    { Location: LocationId
      Quantity: Quantity
      DepartureTiming: Timestamp }

type LaneUsage =
    { Lane: TransportationLaneId
      TransitTimeHours: decimal
      LaneCapacityUtilization: decimal }

type FeasibilityConstraint =
    { ConstraintType: string
      Description: string
      AffectedDimension: string }

type RiskSignal =
    { RiskType: string
      Severity: RiskSeverity
      Description: string
      AffectedMovementCount: int }

type AllocationOutcome =
    { RequirementReference: DistributionRequirementId
      AllocatedQuantity: Quantity
      DeferredQuantity: Quantity
      DeferralReason: string option }

type DistributionMovement =
    { MovementId: MovementId
      Item: ItemId
      SourceLocation: LocationId
      DestinationLocation: LocationId
      Quantity: Quantity
      DepartureTiming: Timestamp
      ExpectedArrivalTiming: Timestamp
      Lane: TransportationLaneId
      TransitDurationHours: decimal }

/// SE-S-080 Aggregate Root
type DistributionRecommendationAggregate =
    { RequirementReference: DistributionRequirementId
      VersionNumber: int
      LifecycleState: DistributionRecommendationLifecycleState
      Item: ItemId
      RequiredQuantity: Quantity
      DestinationLocation: LocationId
      RequiredDeliveryDate: Timestamp
      SourceLocations: SourceLocationAllocation list
      LanesUsed: LaneUsage list
      DepartureTiming: Timestamp
      ExpectedArrivalTiming: Timestamp
      DestinationReceivingWindow: (Timestamp * Timestamp) option
      Strategy: DistributionStrategy
      AlternativeStrategy: DistributionStrategy option
      AllocationPolicyUsed: AllocationPolicy option
      AllocationOutcome: AllocationOutcome option
      DeploymentPriority: DeploymentPriority
      Feasibility: FeasibilityEvaluation
      FeasibilityConstraints: FeasibilityConstraint list
      Confidence: ConfidenceEvaluation
      Stability: StabilityMetrics
      RiskSignals: RiskSignal list
      ReasonCodes: DistributionReasonCode list
      DecisionRationale: string
      DecisionEvidenceReferences: string list
      CreatedTimestamp: Timestamp
      SupersededTimestamp: Timestamp option
      SupersededByVersion: int option }

/// Aggregate Commands
type CreateRecommendationCmd =
    { RequirementId: DistributionRequirementId
      Timestamp: Timestamp }

type WithdrawRecommendationCmd =
    { RequirementId: DistributionRequirementId
      Reason: string
      Timestamp: Timestamp }

/// EV-S-070 — Distribution Recommendation Events
type DistributionRecommendationEvent =
    | DistributionRecommendationCreated of recommendation: DistributionRecommendationAggregate * supersededVersionOpt: int option
    | DistributionRecommendationWithdrawn of recommendation: DistributionRecommendationAggregate * reason: string

/// Pure catamorphic evolution (no side-effects, no validation)
let evolve (stateOpt: DistributionRecommendationAggregate option) (event: DistributionRecommendationEvent) : DistributionRecommendationAggregate option =
    match event with
    | DistributionRecommendationCreated(recAgg, _) -> Some recAgg
    | DistributionRecommendationWithdrawn(recAgg, _) -> Some recAgg

let replay (events: DistributionRecommendationEvent seq) : DistributionRecommendationAggregate option =
    Seq.fold evolve None events
