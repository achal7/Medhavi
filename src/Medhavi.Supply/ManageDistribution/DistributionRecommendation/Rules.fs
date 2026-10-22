/// SE-S-080 — Distribution Recommendation Rules
/// Traces to: Supply Intelligence Specification §4.3.1 (BR-S-080, BR-S-140..145, BR-S-230, BR-S-240)
module Medhavi.Supply.ManageDistribution.DistributionRecommendation.Rules

open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Model
open Algorithms

type SourcingInput =
    { Construction: ConstructionInput
      Result: ConstructionResult
      IsSupplyPlanPublished: bool
      IsPolicyCurrent: bool }

/// BR-S-230 — Distribution recommendation creation requires published supply plan and current policy
let creationEligibility: Rule<SourcingInput> =
    Rule.create
        Rules.distributionRecommendationCreationEligibility.Id
        Rules.distributionRecommendationCreationEligibility.Explanation
        (fun input ->
            input.IsSupplyPlanPublished
            && input.IsPolicyCurrent
            && Quantity.value input.Construction.Requirement.RequiredQuantity > 0.0m)
        (fun input ->
            $"SupplyPlanPublished: %b{input.IsSupplyPlanPublished}; PolicyCurrent: %b{input.IsPolicyCurrent}; Quantity: %M{Quantity.value input.Construction.Requirement.RequiredQuantity}")

/// BR-S-240 — Anti-circular routing invariant
let networkCoherence: Rule<SourcingInput> =
    Rule.create
        Rules.networkCoherenceGovernance.Id
        Rules.networkCoherenceGovernance.Explanation
        (fun input ->
            let dest = input.Construction.Requirement.DestinationLocation
            input.Result.SourceLocations
            |> List.forall (fun s -> s.Location <> dest))
        (fun input ->
            let dest = LocationId.value input.Construction.Requirement.DestinationLocation
            let sources = input.Result.SourceLocations |> List.map (fun s -> LocationId.value s.Location) |> String.concat ", "
            $"Destination: {dest}; Sources: [{sources}]")

/// BR-S-140 — Source inventory availability confirmed
let sourceInventoryAvailability: Rule<SourcingInput> =
    Rule.create
        Rules.sourceInventoryAvailabilityConfirmed.Id
        Rules.sourceInventoryAvailabilityConfirmed.Explanation
        (fun input ->
            match input.Result.Feasibility.SourceInventoryAvailability with
            | DimensionFeasibility.Violated _ -> false
            | _ -> true)
        (fun input -> $"Status: %A{input.Result.Feasibility.SourceInventoryAvailability}")

/// BR-S-141 — Lane capacity verified
let laneCapacity: Rule<SourcingInput> =
    Rule.create
        Rules.laneCapacityVerified.Id
        Rules.laneCapacityVerified.Explanation
        (fun input ->
            match input.Result.Feasibility.LaneCapacityAvailability with
            | DimensionFeasibility.Violated _ -> false
            | _ -> true)
        (fun input -> $"Status: %A{input.Result.Feasibility.LaneCapacityAvailability}")

/// BR-S-142 — Transit time feasibility confirmed
let transitTimeFeasibility: Rule<SourcingInput> =
    Rule.create
        Rules.transitTimeFeasibilityConfirmed.Id
        Rules.transitTimeFeasibilityConfirmed.Explanation
        (fun input ->
            match input.Result.Feasibility.TransitTimeFeasibility with
            | DimensionFeasibility.Violated _ -> false
            | _ -> true)
        (fun input -> $"Status: %A{input.Result.Feasibility.TransitTimeFeasibility}")

/// BR-S-143 — Destination receiving capacity confirmed
let destinationReceivingCapacity: Rule<SourcingInput> =
    Rule.create
        Rules.destinationReceivingCapacityConfirmed.Id
        Rules.destinationReceivingCapacityConfirmed.Explanation
        (fun input ->
            match input.Result.Feasibility.DestinationReceivingCapacity with
            | DimensionFeasibility.Violated _ -> false
            | _ -> true)
        (fun input -> $"Status: %A{input.Result.Feasibility.DestinationReceivingCapacity}")

/// BR-S-145 — Constrained supply allocation strategy governance
let constrainedSupplyAllocation: Rule<SourcingInput> =
    Rule.create
        Rules.constrainedSupplyAllocationStrategyApplied.Id
        Rules.constrainedSupplyAllocationStrategyApplied.Explanation
        (fun input ->
            match input.Result.Feasibility.Overall with
            | DistributionFeasibility.ConditionallyFeasible
            | DistributionFeasibility.Infeasible ->
                input.Result.AllocationPolicyUsed.IsSome && input.Result.AllocationOutcome.IsSome
            | DistributionFeasibility.FullyFeasible -> true)
        (fun input ->
            $"Feasibility: %A{input.Result.Feasibility.Overall}; PolicyUsed: %b{input.Result.AllocationPolicyUsed.IsSome}; OutcomeRecorded: %b{input.Result.AllocationOutcome.IsSome}")

/// Governance rule set for DE-S-071
let sourcingRules: Rule<SourcingInput> list =
    [ creationEligibility
      networkCoherence
      sourceInventoryAvailability
      laneCapacity
      transitTimeFeasibility
      destinationReceivingCapacity
      constrainedSupplyAllocation ]
