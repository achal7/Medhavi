/// Projections for Procurement Recommendation Aggregate
/// Maps pure domain models to Medhavi.Contracts.Supply DTOs and provides projection reducers
module Medhavi.Supply.ProcureMaterials.ProcurementRecommendation.Projections

open Medhavi.Contracts.Supply
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

let mapFeasibilityToDto (f: ProcurementFeasibility) : ProcurementFeasibilityStatus * string list =
        match f with
        | ProcurementFeasibility.Feasible -> ProcurementFeasibilityStatus.Feasible, []
        | ProcurementFeasibility.ConditionallyFeasible c -> ProcurementFeasibilityStatus.ConditionallyFeasible, c
        | ProcurementFeasibility.Infeasible r -> ProcurementFeasibilityStatus.Infeasible, [ r ]

let mapStrategyToDto (s: SourcingStrategy) : SourcingStrategyType =
        match s with
        | SourcingStrategy.SingleSource -> SourcingStrategyType.SingleSource
        | SourcingStrategy.DualSource -> SourcingStrategyType.DualSource
        | SourcingStrategy.Competitive -> SourcingStrategyType.Competitive
        | SourcingStrategy.StrategicPartnership -> SourcingStrategyType.StrategicPartnership

let mapConfidenceToDto (c: RecommendationConfidence) : RecommendationConfidenceLevel =
        match c with
        | RecommendationConfidence.High -> RecommendationConfidenceLevel.High
        | RecommendationConfidence.Medium -> RecommendationConfidenceLevel.Medium
        | RecommendationConfidence.Low -> RecommendationConfidenceLevel.Low

let mapGovernanceStatusToDto (g: ProcurementGovernanceStatus) : Medhavi.Contracts.Supply.ProcurementGovernanceStatus =
        match g with
        | ProcurementGovernanceStatus.Authorized -> Medhavi.Contracts.Supply.ProcurementGovernanceStatus.Authorized
        | ProcurementGovernanceStatus.Escalated _ -> Medhavi.Contracts.Supply.ProcurementGovernanceStatus.Escalated
        | ProcurementGovernanceStatus.Deferred _ -> Medhavi.Contracts.Supply.ProcurementGovernanceStatus.Deferred

let mapLifecycleStateToDto (s: ProcurementRecommendationLifecycleState) : Medhavi.Contracts.Supply.ProcurementRecommendationLifecycleState =
        match s with
        | ProcurementRecommendationLifecycleState.Active -> Medhavi.Contracts.Supply.ProcurementRecommendationLifecycleState.Active
        | ProcurementRecommendationLifecycleState.Superseded -> Medhavi.Contracts.Supply.ProcurementRecommendationLifecycleState.Superseded
        | ProcurementRecommendationLifecycleState.Archived -> Medhavi.Contracts.Supply.ProcurementRecommendationLifecycleState.Archived

let mapAllocationToDto (a: SupplierAllocation) : SupplierAllocationDto =
        { SupplierId = SupplierId.value a.SupplierId
          Quantity = Quantity.value a.Quantity
          OrderReleaseDate = Timestamp.value a.OrderReleaseDate
          ExpectedDeliveryDate = Timestamp.value a.ExpectedDeliveryDate
          EstimatedUnitCost = a.EstimatedUnitCost }

let mapRecommendationToDto (r: ProcurementRecommendation) : ProcurementRecommendationDto =
        let fStatus, fConstraints = mapFeasibilityToDto r.Feasibility
        { RequirementId = ProcurementRequirementId.value r.RequirementId
          VersionNumber = r.VersionNumber
          LifecycleState = mapLifecycleStateToDto r.LifecycleState
          ItemId = ItemId.value r.ItemId
          LocationId = LocationId.value r.LocationId
          RequiredDate = Timestamp.value r.RequiredDate
          RequiredQuantity = Quantity.value r.RequiredQuantity
          FeasibilityStatus = fStatus
          FeasibilityConstraints = fConstraints
          Allocations = r.Allocations |> List.map mapAllocationToDto
          SourcingStrategy = mapStrategyToDto r.Strategy
          Rationale = r.Rationale
          EvidenceReferences = r.EvidenceReferences
          OverallConfidence = mapConfidenceToDto r.OverallConfidence
          SupplierConfidence = mapConfidenceToDto r.SupplierConfidence
          DemandConfidence = mapConfidenceToDto r.DemandConfidence
          CapacityConfidence = mapConfidenceToDto r.CapacityConfidence
          InventoryRiskFactor = r.InventoryRiskFactor
          SourcingComplexityFactor = r.SourcingComplexityFactor
          GovernanceStatus = mapGovernanceStatusToDto r.GovernanceStatus
          Assumptions = r.Assumptions
          SourceSupplyPlanId = r.SourceSupplyPlanId
          LastUpdatedTimestamp = Timestamp.value r.LastUpdatedTimestamp }

let apply
        (state: Map<ProcurementRequirementId, ProcurementRecommendationDto>)
        (event: ProcurementRecommendationEvent)
        : Map<ProcurementRequirementId, ProcurementRecommendationDto> =

        match event with
        | ProcurementRecommendationEvent.RecommendationCreated r ->
            let dto = mapRecommendationToDto r
            Map.add r.RequirementId dto state
        | ProcurementRecommendationEvent.RecommendationSuperseded (_ver, _ts) ->
            state
        | ProcurementRecommendationEvent.RecommendationArchived (_ver, _ts) ->
            state

let seedFromAggregates (aggregates: ProcurementRecommendation list) : Map<ProcurementRequirementId, ProcurementRecommendationDto> =
        aggregates
        |> List.map (fun a -> a.RequirementId, mapRecommendationToDto a)
        |> Map.ofList
