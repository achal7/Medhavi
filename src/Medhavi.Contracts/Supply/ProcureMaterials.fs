namespace Medhavi.Contracts.Supply

open System
open System.Threading.Tasks
open Medhavi.Contracts

[<RequireQualifiedAccess>]
type ProcurementFeasibilityStatus =
    | Feasible
    | ConditionallyFeasible
    | Infeasible

[<RequireQualifiedAccess>]
type SourcingStrategyType =
    | SingleSource
    | DualSource
    | Competitive
    | StrategicPartnership

[<RequireQualifiedAccess>]
type RecommendationConfidenceLevel =
    | High
    | Medium
    | Low

[<RequireQualifiedAccess>]
type ProcurementRecommendationLifecycleState =
    | Active
    | Superseded
    | Archived

[<RequireQualifiedAccess>]
type ProcurementGovernanceStatus =
    | Authorized
    | Escalated
    | Deferred

[<RequireQualifiedAccess>]
type RequirementPriority =
    | Standard
    | Expedited
    | CriticalShortage

[<RequireQualifiedAccess>]
type SourcingNecessity =
    | MandatoryShortfall
    | StrategicReserve
    | DiscretionaryOptimization

[<RequireQualifiedAccess>]
type SourcingIntent =
    | StandardReplenishment
    | DualSourceRiskMitigation
    | ExpeditedIntervention
    | SoleSourcePreservation

[<RequireQualifiedAccess>]
type GovernancePosture =
    | AutonomousExecutionPermitted
    | PlannerApprovalMandatory
    | ExecutiveEscalationRequired

[<RequireQualifiedAccess>]
type SourcingDirective =
    | ExecuteProcurementRecommendation
    | HoldForConsolidation
    | HaltAndEscalate of Reason: string

[<RequireQualifiedAccess>]
type ProcurementPlanLifecycleState =
    | Draft
    | Published
    | Superseded

[<RequireQualifiedAccess>]
type ProcurementPlanPublicationOutcome =
    | Publish
    | PublishWithFlags of Flags: string list
    | Hold of Reason: string

type SupplierAllocationDto =
    { SupplierId: string
      Quantity: decimal
      OrderReleaseDate: DateTimeOffset
      ExpectedDeliveryDate: DateTimeOffset
      EstimatedUnitCost: decimal option }

type ProcurementRecommendationDto =
    { RequirementId: string
      VersionNumber: int
      LifecycleState: ProcurementRecommendationLifecycleState
      ItemId: string
      LocationId: string
      RequiredDate: DateTimeOffset
      RequiredQuantity: decimal
      FeasibilityStatus: ProcurementFeasibilityStatus
      FeasibilityConstraints: string list
      Allocations: SupplierAllocationDto list
      SourcingStrategy: SourcingStrategyType
      Rationale: string
      EvidenceReferences: string list
      OverallConfidence: RecommendationConfidenceLevel
      SupplierConfidence: RecommendationConfidenceLevel
      DemandConfidence: RecommendationConfidenceLevel
      CapacityConfidence: RecommendationConfidenceLevel
      InventoryRiskFactor: decimal
      SourcingComplexityFactor: decimal
      GovernanceStatus: ProcurementGovernanceStatus
      Assumptions: string list
      SourceSupplyPlanId: string
      LastUpdatedTimestamp: DateTimeOffset }

type ProcurementPlanLineItemDto =
    { RequirementId: string
      ItemId: string
      LocationId: string
      SupplierId: string
      DeliveryDate: DateTimeOffset
      OrderReleaseDate: DateTimeOffset
      OrderQuantity: decimal
      EstimatedCost: decimal
      Confidence: RecommendationConfidenceLevel }

type ProcurementAggregateMetricsDto =
    { TotalSpend: decimal
      SupplierAllocationPercentages: (string * decimal) list
      ConfidenceDistribution: (RecommendationConfidenceLevel * int) list
      FeasibilitySummary: (ProcurementFeasibilityStatus * int) list }

type ProcurementRiskSignalsDto =
    { SupplierConcentration: (string * decimal) list
      SingleSourceExposure: string list
      GeographicConcentration: (string * decimal) list
      ProcurementConfidenceDistribution: (RecommendationConfidenceLevel * int) list }

type ProcurementRecommendationSignalsDto =
    { ItemsRequiringPlannerReview: string list
      SupplierConsolidationOpportunities: string list
      RiskConcentrationAlerts: string list
      UnresolvedFeasibilityIssues: string list }

type ProcurementPlanDto =
    { PlanningScopeId: string
      PlanningCycleId: string
      VersionNumber: int
      LifecycleState: ProcurementPlanLifecycleState
      LineItems: ProcurementPlanLineItemDto list
      ActiveRecommendationIds: string list
      AggregateMetrics: ProcurementAggregateMetricsDto
      RiskSignals: ProcurementRiskSignalsDto
      RecommendationSignals: ProcurementRecommendationSignalsDto
      PublicationTimestamp: DateTimeOffset
      PublicationAuthority: string }

// API Request Records
type CreateProcurementRecommendationReq =
    { RequirementId: string
      SourceSupplyPlanId: string
      ItemId: string
      LocationId: string
      RequiredDate: DateTimeOffset
      RequiredQuantity: decimal
      EvaluationTimestamp: DateTimeOffset }

type PublishProcurementPlanReq =
    { PlanningScopeId: string
      PlanningCycleId: string
      PublicationAuthority: string
      PublicationTimestamp: DateTimeOffset }

// Business Notifications (Dispatched post-persistence)
type ProcurementRecommendationCreatedNotification =
    { RequirementId: string
      VersionNumber: int
      SourcingStrategy: SourcingStrategyType
      SupplierAllocations: SupplierAllocationDto list
      ProcurementFeasibility: ProcurementFeasibilityStatus
      OverallConfidence: RecommendationConfidenceLevel
      GovernanceStatus: ProcurementGovernanceStatus
      DeliveryGuarantee: string }

type ProcurementPlanPublishedNotification =
    { PlanningScopeId: string
      PlanningCycleId: string
      VersionNumber: int
      AggregateMetrics: ProcurementAggregateMetricsDto
      RiskSignals: ProcurementRiskSignalsDto
      RecommendationSignals: ProcurementRecommendationSignalsDto
      DeliveryGuarantee: string }

// Capability & Aggregate Command API Contracts
type ProcurementRecommendationApi =
    { Create: CreateProcurementRecommendationReq -> Task<Result<ProcurementRecommendationDto, ApiError>> }

type ProcurementPlanApi =
    { Publish: PublishProcurementPlanReq -> Task<Result<ProcurementPlanDto, ApiError>> }

type ProcureMaterialsApis =
    { Recommendation: ProcurementRecommendationApi
      Plan: ProcurementPlanApi }

/// Query service aliases
type ProcurementRecommendationQueries = QueryService<ProcurementRecommendationDto, string>
type ProcurementPlanQueries = QueryService<ProcurementPlanDto, string>
