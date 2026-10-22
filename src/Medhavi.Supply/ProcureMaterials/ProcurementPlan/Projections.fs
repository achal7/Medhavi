/// Projections for Procurement Plan Aggregate
/// Maps pure domain models to Medhavi.Contracts.Supply DTOs and provides projection reducers
module Medhavi.Supply.ProcureMaterials.ProcurementPlan.Projections

open Medhavi.Contracts.Supply
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

let mapLineItemToDto (l: ProcurementPlanLineItem) : ProcurementPlanLineItemDto =
        { RequirementId = ProcurementRequirementId.value l.RequirementId
          ItemId = ItemId.value l.ItemId
          LocationId = LocationId.value l.LocationId
          SupplierId = SupplierId.value l.SupplierId
          DeliveryDate = Timestamp.value l.DeliveryDate
          OrderReleaseDate = Timestamp.value l.OrderReleaseDate
          OrderQuantity = Quantity.value l.OrderQuantity
          EstimatedCost = l.EstimatedCost
          Confidence =
            match l.Confidence with
            | RecommendationConfidence.High -> RecommendationConfidenceLevel.High
            | RecommendationConfidence.Medium -> RecommendationConfidenceLevel.Medium
            | RecommendationConfidence.Low -> RecommendationConfidenceLevel.Low }

let mapMetricsToDto (m: ProcurementAggregateMetrics) : ProcurementAggregateMetricsDto =
        { TotalSpend = m.TotalSpend
          SupplierAllocationPercentages =
            m.SupplierAllocationPercentages
            |> List.map (fun (s, pct) -> SupplierId.value s, pct)
          ConfidenceDistribution =
            m.ConfidenceDistribution
            |> List.map (fun (c, count) ->
                let cDto =
                    match c with
                    | RecommendationConfidence.High -> RecommendationConfidenceLevel.High
                    | RecommendationConfidence.Medium -> RecommendationConfidenceLevel.Medium
                    | RecommendationConfidence.Low -> RecommendationConfidenceLevel.Low
                cDto, count)
          FeasibilitySummary =
            m.FeasibilitySummary
            |> List.map (fun (f, count) ->
                let fDto =
                    match f with
                    | ProcurementFeasibility.Feasible -> ProcurementFeasibilityStatus.Feasible
                    | ProcurementFeasibility.ConditionallyFeasible _ -> ProcurementFeasibilityStatus.ConditionallyFeasible
                    | ProcurementFeasibility.Infeasible _ -> ProcurementFeasibilityStatus.Infeasible
                fDto, count) }

let mapRiskSignalsToDto (r: ProcurementRiskSignals) : ProcurementRiskSignalsDto =
        { SupplierConcentration =
            r.SupplierConcentration
            |> List.map (fun (s, pct) -> SupplierId.value s, pct)
          SingleSourceExposure =
            r.SingleSourceExposure
            |> List.map ItemId.value
          GeographicConcentration =
            r.GeographicConcentration
            |> List.map (fun (l, pct) -> LocationId.value l, pct)
          ProcurementConfidenceDistribution =
            r.ProcurementConfidenceDistribution
            |> List.map (fun (c, count) ->
                let cDto =
                    match c with
                    | RecommendationConfidence.High -> RecommendationConfidenceLevel.High
                    | RecommendationConfidence.Medium -> RecommendationConfidenceLevel.Medium
                    | RecommendationConfidence.Low -> RecommendationConfidenceLevel.Low
                cDto, count) }

let mapSignalsToDto (s: ProcurementRecommendationSignals) : ProcurementRecommendationSignalsDto =
        { ItemsRequiringPlannerReview = s.ItemsRequiringPlannerReview |> List.map ItemId.value
          SupplierConsolidationOpportunities = s.SupplierConsolidationOpportunities
          RiskConcentrationAlerts = s.RiskConcentrationAlerts
          UnresolvedFeasibilityIssues = s.UnresolvedFeasibilityIssues }

let mapPlanToDto (p: ProcurementPlan) : ProcurementPlanDto =
        { PlanningScopeId = PlanningScopeId.value p.PlanningScopeId
          PlanningCycleId = p.PlanningCycleId
          VersionNumber = p.VersionNumber
          LifecycleState =
            match p.LifecycleState with
            | ProcurementPlanLifecycleState.Draft -> Medhavi.Contracts.Supply.ProcurementPlanLifecycleState.Draft
            | ProcurementPlanLifecycleState.Published -> Medhavi.Contracts.Supply.ProcurementPlanLifecycleState.Published
            | ProcurementPlanLifecycleState.Superseded -> Medhavi.Contracts.Supply.ProcurementPlanLifecycleState.Superseded
          LineItems = p.LineItems |> List.map mapLineItemToDto
          ActiveRecommendationIds = p.ActiveRecommendationIds |> List.map ProcurementRequirementId.value
          AggregateMetrics = mapMetricsToDto p.AggregateMetrics
          RiskSignals = mapRiskSignalsToDto p.RiskSignals
          RecommendationSignals = mapSignalsToDto p.RecommendationSignals
          PublicationTimestamp = Timestamp.value p.PublicationTimestamp
          PublicationAuthority = p.PublicationAuthority }

let apply
        (state: Map<ProcurementPlanId, ProcurementPlanDto>)
        (event: ProcurementPlanEvent)
        : Map<ProcurementPlanId, ProcurementPlanDto> =

        match event with
        | ProcurementPlanEvent.PlanDraftComposed p ->
            let dto = mapPlanToDto p
            Map.add p.PlanId dto state
        | ProcurementPlanEvent.PlanPublished (_ver, _ts, _auth, p) ->
            let dto = mapPlanToDto p
            Map.add p.PlanId dto state
        | ProcurementPlanEvent.PlanSuperseded (_ver, _ts) ->
            state

let seedFromAggregates (aggregates: ProcurementPlan list) : Map<ProcurementPlanId, ProcurementPlanDto> =
        aggregates
        |> List.map (fun a -> a.PlanId, mapPlanToDto a)
        |> Map.ofList
