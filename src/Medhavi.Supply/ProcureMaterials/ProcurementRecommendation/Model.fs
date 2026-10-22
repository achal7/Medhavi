module Medhavi.Supply.ProcureMaterials.ProcurementRecommendation.Model

open Medhavi.SemanticModel
open Medhavi.Supply

[<RequireQualifiedAccess>]
type ProcurementRecommendationLifecycleState =
    | Active
    | Superseded
    | Archived

[<RequireQualifiedAccess>]
type ProcurementGovernanceStatus =
    | Authorized
    | Escalated of Reason: string
    | Deferred of Reason: string

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

type SupplierAllocation =
    {
        SupplierId: SupplierId
        Quantity: Quantity
        OrderReleaseDate: Timestamp
        ExpectedDeliveryDate: Timestamp
        EstimatedUnitCost: decimal option
    }

type EnterpriseSourcingDetermination =
    {
        DeterminationId: string
        Necessity: SourcingNecessity
        Intent: SourcingIntent
        Governance: GovernancePosture
        Directive: SourcingDirective
        TriggeringPolicyRuleId: string
        Rationale: string
        ReasonCodes: string list
        EffectiveTimestamp: Timestamp
    }

type ProcurementRecommendation =
    {
        RequirementId: ProcurementRequirementId
        VersionNumber: int
        LifecycleState: ProcurementRecommendationLifecycleState
        ItemId: ItemId
        LocationId: LocationId
        RequiredDate: Timestamp
        RequiredQuantity: Quantity
        Feasibility: ProcurementFeasibility
        Allocations: SupplierAllocation list
        Strategy: SourcingStrategy
        Determination: EnterpriseSourcingDetermination
        Rationale: string
        EvidenceReferences: string list
        OverallConfidence: RecommendationConfidence
        SupplierConfidence: RecommendationConfidence
        DemandConfidence: RecommendationConfidence
        CapacityConfidence: RecommendationConfidence
        InventoryRiskFactor: decimal
        SourcingComplexityFactor: decimal
        GovernanceStatus: ProcurementGovernanceStatus
        Assumptions: string list
        SourceSupplyPlanId: string
        LastUpdatedTimestamp: Timestamp
    }

[<RequireQualifiedAccess>]
type ProcurementRecommendationEvent =
    | RecommendationCreated of ProcurementRecommendation
    | RecommendationSuperseded of Version: int * Timestamp: Timestamp
    | RecommendationArchived of Version: int * Timestamp: Timestamp

type CreateProcurementRecommendationCmd =
    {
        RequirementId: ProcurementRequirementId
        SourceSupplyPlanId: string
        ItemId: ItemId
        LocationId: LocationId
        RequiredDate: Timestamp
        RequiredQuantity: Quantity
        EvaluationTimestamp: Timestamp
    }

type EvaluatedProcurementRequirement =
    {
        RequirementId: ProcurementRequirementId
        ItemId: ItemId
        LocationId: LocationId
        NetGapQuantity: decimal
        RecommendedOrderQuantity: decimal
        InducedExcessQuantity: decimal
        InducedExcessRatio: decimal
        RequiredDeliveryDate: Timestamp
        CalculatedReleaseDate: Timestamp
        LeadTimeDeficitDays: decimal
        SlackDays: decimal
        Priority: RequirementPriority
        ReasonCodes: string list
        EvaluationTimestamp: Timestamp
    }

type SupplyEcosystemSummary =
    {
        ApprovedSupplierCount: int
        AggregateCapacity: decimal
        HealthyRatio: decimal
        AtRiskRatio: decimal
        UnderperformingRatio: decimal
        CriticalRatio: decimal
        UnassessedRatio: decimal
        HerfindahlIndex: int
        MarketConcentration: string
    }

type SourcingSituationAssessment =
    {
        RequirementId: ProcurementRequirementId
        Ecosystem: SupplyEcosystemSummary
        Feasibility: ProcurementFeasibility
        CompositeRiskScore: decimal
        DisruptionRiskIntensity: decimal
        SingleSourceRisk: decimal
        VolumeBundlingAdvantage: bool
        HighBuyerLeverage: bool
        IdentifiedRisks: string list
        IdentifiedOpportunities: string list
        ReasonCodes: string list
        OverallConfidence: RecommendationConfidence
        EvaluationTimestamp: Timestamp
    }

let zero: ProcurementRecommendation option = None

let apply
    (state: ProcurementRecommendation option)
    (event: ProcurementRecommendationEvent)
    : ProcurementRecommendation option =
    match event with
    | ProcurementRecommendationEvent.RecommendationCreated rec' -> Some rec'
    | ProcurementRecommendationEvent.RecommendationSuperseded(_ver, ts) ->
        state
        |> Option.map(fun r ->
            { r with
                LifecycleState = ProcurementRecommendationLifecycleState.Superseded
                LastUpdatedTimestamp = ts
            })
    | ProcurementRecommendationEvent.RecommendationArchived(_ver, ts) ->
        state
        |> Option.map(fun r ->
            { r with
                LifecycleState = ProcurementRecommendationLifecycleState.Archived
                LastUpdatedTimestamp = ts
            })

let evolve
    (state: ProcurementRecommendation option)
    (event: ProcurementRecommendationEvent)
    : ProcurementRecommendation option =
    apply state event
