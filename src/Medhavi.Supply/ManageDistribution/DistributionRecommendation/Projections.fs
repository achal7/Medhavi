/// SE-S-080 — Distribution Recommendation Projections
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-080)
module Medhavi.Supply.ManageDistribution.DistributionRecommendation.Projections

open System
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model

let private mapLifecycleState = function
    | DistributionRecommendationLifecycleState.Active -> Medhavi.Contracts.Supply.DistributionRecommendationLifecycleState.Active
    | DistributionRecommendationLifecycleState.Superseded -> Medhavi.Contracts.Supply.DistributionRecommendationLifecycleState.Superseded
    | DistributionRecommendationLifecycleState.Withdrawn -> Medhavi.Contracts.Supply.DistributionRecommendationLifecycleState.Withdrawn

let private mapStrategy = function
    | DistributionStrategy.PreferredSource -> Medhavi.Contracts.Supply.DistributionStrategyType.PreferredSource
    | DistributionStrategy.AlternateSource -> Medhavi.Contracts.Supply.DistributionStrategyType.AlternateSource
    | DistributionStrategy.CrossDock -> Medhavi.Contracts.Supply.DistributionStrategyType.CrossDock
    | DistributionStrategy.Deferral -> Medhavi.Contracts.Supply.DistributionStrategyType.Deferral

let private mapAllocationPolicy = function
    | AllocationPolicy.FairShare -> Medhavi.Contracts.Supply.AllocationPolicyType.FairShare
    | AllocationPolicy.PriorityBased -> Medhavi.Contracts.Supply.AllocationPolicyType.PriorityBased
    | AllocationPolicy.ServiceLevelPriority -> Medhavi.Contracts.Supply.AllocationPolicyType.ServiceLevelPriority

let private mapDeploymentPriority = function
    | DeploymentPriority.ServiceProtection -> Medhavi.Contracts.Supply.DeploymentPriorityType.ServiceProtection
    | DeploymentPriority.PromotionSupport -> Medhavi.Contracts.Supply.DeploymentPriorityType.PromotionSupport
    | DeploymentPriority.NewProductLaunch -> Medhavi.Contracts.Supply.DeploymentPriorityType.NewProductLaunch
    | DeploymentPriority.SeasonalPositioning -> Medhavi.Contracts.Supply.DeploymentPriorityType.SeasonalPositioning
    | DeploymentPriority.InventoryBalancing -> Medhavi.Contracts.Supply.DeploymentPriorityType.InventoryBalancing

let private mapFeasibility = function
    | DistributionFeasibility.FullyFeasible -> Medhavi.Contracts.Supply.DistributionFeasibilityStatus.FullyFeasible
    | DistributionFeasibility.ConditionallyFeasible -> Medhavi.Contracts.Supply.DistributionFeasibilityStatus.ConditionallyFeasible
    | DistributionFeasibility.Infeasible -> Medhavi.Contracts.Supply.DistributionFeasibilityStatus.Infeasible

let private mapConfidence = function
    | DistributionConfidence.High -> Medhavi.Contracts.Supply.DistributionConfidenceLevel.High
    | DistributionConfidence.Medium -> Medhavi.Contracts.Supply.DistributionConfidenceLevel.Medium
    | DistributionConfidence.Low -> Medhavi.Contracts.Supply.DistributionConfidenceLevel.Low

let private mapSourceLocation (s: SourceLocationAllocation) : SourceLocationAllocationDto =
    { LocationId = LocationId.value s.Location
      Quantity = Quantity.value s.Quantity
      DepartureTiming = Timestamp.value s.DepartureTiming }

let private mapLaneUsage (l: LaneUsage) : LaneUsageDto =
    { LaneId = TransportationLaneId.value l.Lane
      TransitTimeHours = l.TransitTimeHours
      LaneCapacityUtilization = l.LaneCapacityUtilization }

let private mapConstraint (c: FeasibilityConstraint) : FeasibilityConstraintDto =
    { ConstraintType = c.ConstraintType
      Description = c.Description
      AffectedDimension = c.AffectedDimension }

let private mapRisk (r: RiskSignal) : RiskSignalDto =
    { RiskType = r.RiskType
      Severity = sprintf "%A" r.Severity
      Description = r.Description
      AffectedMovementCount = r.AffectedMovementCount }

let toDto (recAgg: DistributionRecommendationAggregate) : DistributionRecommendationDto =
    let reqId = DistributionRequirementId.value recAgg.RequirementReference
    { RecommendationId = sprintf "%s#v%d" reqId recAgg.VersionNumber
      RequirementReference = reqId
      VersionNumber = recAgg.VersionNumber
      LifecycleState = mapLifecycleState recAgg.LifecycleState
      ItemId = ItemId.value recAgg.Item
      RequiredQuantity = Quantity.value recAgg.RequiredQuantity
      DestinationLocationId = LocationId.value recAgg.DestinationLocation
      RequiredDeliveryDate = Timestamp.value recAgg.RequiredDeliveryDate
      SourceLocations = recAgg.SourceLocations |> List.map mapSourceLocation
      LanesUsed = recAgg.LanesUsed |> List.map mapLaneUsage
      DepartureTiming = Timestamp.value recAgg.DepartureTiming
      ExpectedArrivalTiming = Timestamp.value recAgg.ExpectedArrivalTiming
      Strategy = mapStrategy recAgg.Strategy
      AllocationPolicyUsed = recAgg.AllocationPolicyUsed |> Option.map mapAllocationPolicy
      AllocatedQuantity = recAgg.AllocationOutcome |> Option.map (fun a -> Quantity.value a.AllocatedQuantity)
      DeferredQuantity = recAgg.AllocationOutcome |> Option.map (fun a -> Quantity.value a.DeferredQuantity)
      DeferralReason = recAgg.AllocationOutcome |> Option.bind (fun a -> a.DeferralReason)
      DeploymentPriority = mapDeploymentPriority recAgg.DeploymentPriority
      FeasibilityStatus = mapFeasibility recAgg.Feasibility.Overall
      FeasibilityConstraints = recAgg.FeasibilityConstraints |> List.map mapConstraint
      ConfidenceLevel = mapConfidence recAgg.Confidence.Overall
      StabilityScore = recAgg.Stability.StabilityScore
      ChangeCount = recAgg.Stability.ChangeCount
      RiskSignals = recAgg.RiskSignals |> List.map mapRisk
      ReasonCodes = recAgg.ReasonCodes |> List.map (sprintf "%A")
      DecisionRationale = recAgg.DecisionRationale
      DecisionEvidenceReferences = recAgg.DecisionEvidenceReferences
      CreatedTimestamp = Timestamp.value recAgg.CreatedTimestamp
      SupersededVersionId = recAgg.SupersededByVersion |> Option.map (fun v -> sprintf "%s#v%d" reqId v) }

type State = Map<DistributionRequirementId, DistributionRecommendationDto>

let seedFromAggregates (aggregates: DistributionRecommendationAggregate list) : State =
    aggregates
    |> List.map (fun a -> a.RequirementReference, toDto a)
    |> Map.ofList

let apply (state: State) (event: DistributionRecommendationEvent) : State =
    match event with
    | DistributionRecommendationCreated(recAgg, _) ->
        state |> Map.add recAgg.RequirementReference (toDto recAgg)
    | DistributionRecommendationWithdrawn(recAgg, _) ->
        state |> Map.add recAgg.RequirementReference (toDto recAgg)
