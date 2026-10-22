module Medhavi.Supply.ProcureMaterials.ProcurementPlan.Model

open Medhavi.SemanticModel
open Medhavi.Supply

type RecommendationAllocationFact =
    {
        SupplierId: SupplierId
        Quantity: Quantity
        OrderReleaseDate: Timestamp
        ExpectedDeliveryDate: Timestamp
        EstimatedUnitCost: decimal option
    }

type RecommendationSummaryFact =
    {
        RequirementId: ProcurementRequirementId
        ItemId: ItemId
        LocationId: LocationId
        Allocations: RecommendationAllocationFact list
        OverallConfidence: RecommendationConfidence
        Feasibility: ProcurementFeasibility
        Strategy: SourcingStrategy
        IsEscalated: bool
        IsHeldForConsolidation: bool
    }

[<RequireQualifiedAccess>]
type ProcurementPlanLifecycleState =
    | Draft
    | Published
    | Superseded

type ProcurementPlanLineItem =
    {
        RequirementId: ProcurementRequirementId
        ItemId: ItemId
        LocationId: LocationId
        SupplierId: SupplierId
        DeliveryDate: Timestamp
        OrderReleaseDate: Timestamp
        OrderQuantity: Quantity
        EstimatedCost: decimal
        Confidence: RecommendationConfidence
    }

type ProcurementAggregateMetrics =
    {
        TotalSpend: decimal
        SupplierAllocationPercentages: (SupplierId * decimal) list
        ConfidenceDistribution: (RecommendationConfidence * int) list
        FeasibilitySummary: (ProcurementFeasibility * int) list
    }

type ProcurementRiskSignals =
    {
        SupplierConcentration: (SupplierId * decimal) list
        SingleSourceExposure: ItemId list
        GeographicConcentration: (LocationId * decimal) list
        ProcurementConfidenceDistribution: (RecommendationConfidence * int) list
    }

type ProcurementRecommendationSignals =
    {
        ItemsRequiringPlannerReview: ItemId list
        SupplierConsolidationOpportunities: string list
        RiskConcentrationAlerts: string list
        UnresolvedFeasibilityIssues: string list
    }

type ProcurementPlan =
    {
        PlanId: ProcurementPlanId
        PlanningScopeId: PlanningScopeId
        PlanningCycleId: string
        VersionNumber: int
        LifecycleState: ProcurementPlanLifecycleState
        LineItems: ProcurementPlanLineItem list
        ActiveRecommendationIds: ProcurementRequirementId list
        AggregateMetrics: ProcurementAggregateMetrics
        RiskSignals: ProcurementRiskSignals
        RecommendationSignals: ProcurementRecommendationSignals
        PublicationTimestamp: Timestamp
        PublicationAuthority: string
    }

[<RequireQualifiedAccess>]
type ProcurementPlanEvent =
    | PlanDraftComposed of ProcurementPlan
    | PlanPublished of Version: int * Timestamp: Timestamp * Authority: string * Plan: ProcurementPlan
    | PlanSuperseded of Version: int * Timestamp: Timestamp

type PublishProcurementPlanCmd =
    {
        PlanningScopeId: PlanningScopeId
        PlanningCycleId: string
        PublicationAuthority: string
        PublicationTimestamp: Timestamp
    }

let zero: ProcurementPlan option = None

let apply (state: ProcurementPlan option) (event: ProcurementPlanEvent) : ProcurementPlan option =
    match event with
    | ProcurementPlanEvent.PlanDraftComposed p -> Some p
    | ProcurementPlanEvent.PlanPublished(_ver, ts, auth, p) ->
        Some
            { p with
                LifecycleState = ProcurementPlanLifecycleState.Published
                PublicationTimestamp = ts
                PublicationAuthority = auth
            }
    | ProcurementPlanEvent.PlanSuperseded(_ver, _ts) ->
        state
        |> Option.map(fun p ->
            { p with
                LifecycleState = ProcurementPlanLifecycleState.Superseded
            })

let evolve (state: ProcurementPlan option) (event: ProcurementPlanEvent) : ProcurementPlan option = apply state event
