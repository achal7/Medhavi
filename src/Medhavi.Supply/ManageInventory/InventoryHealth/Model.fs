/// SE-S-032 — Inventory Health Assessment Domain Model
module Medhavi.Supply.ManageInventory.InventoryHealth.Model

open Medhavi.SemanticModel
open Medhavi.Supply
open Medhavi.Supply.ManageInventory

type HealthVersionLifecycleState =
    | Draft
    | Published
    | Superseded

type ItemHealthSummary =
    {
        Item: ItemId
        Location: LocationId
        HealthClassification: InventoryHealthStatus
        DaysOfSupply: decimal
        PolicyComplianceStatus: PolicyComplianceStatus
        StockoutRiskExposure: decimal
        ExcessValue: decimal
    }

type InventoryHealthAssessmentVersion =
    {
        VersionNumber: int
        State: HealthVersionLifecycleState
        ItemHealthSummaries: ItemHealthSummary list
        TotalExcessValue: decimal
        TotalStockoutRiskExposure: decimal
        AssessmentConfidence: decimal
        AssessmentRationale: string
        PublicationTimestamp: Timestamp option
    }

/// SE-S-032 Aggregate Root (Published Knowledge pattern per AssessmentScope and EvaluationPeriod)
type InventoryHealthAssessment =
    {
        AssessmentScopeId: PlanningScopeId
        EvaluationPeriodId: EvaluationPeriodId
        Versions: InventoryHealthAssessmentVersion list
    }

type PublishInventoryHealthCmd =
    {
        AssessmentScopeId: PlanningScopeId
        EvaluationPeriodId: EvaluationPeriodId
        Timestamp: Timestamp
    }

/// EV-S-022: Inventory Health Assessment Published
type InventoryHealthEvent =
    | InventoryHealthAssessmentPublished of
        assessment: InventoryHealthAssessment *
        versionNumber: int *
        supersededVersionOpt: int option

type Likelihood =
    | VeryLow
    | Low
    | Medium
    | High
    | VeryHigh

type Impact =
    | Negligible
    | Minor
    | Moderate
    | Major
    | Severe

type DimensionAssessment =
    {
        Likelihood: Likelihood
        Impact: Impact
        MagnitudeUnits: decimal
        ValueCurrency: decimal
        ReasonCodes: string list
    }

type MultiDimensionalRiskAssessment =
    {
        StockoutExposure: DimensionAssessment
        ExcessExposure: DimensionAssessment
        ObsolescenceExposure: DimensionAssessment
        PolicyConformance: DimensionAssessment
        SupplyDependencyExposure: DimensionAssessment
        OverallConfidence: decimal
    }

type RiskEvaluationInput =
    {
        OnHand: decimal
        Allocated: decimal
        InTransit: decimal
        SafetyStock: decimal
        ReorderPoint: decimal
        TargetStockLevel: decimal
        LeadTimeDays: int
        UnitCost: decimal
        RemainingForecastDemand: decimal option
        ShelfLifeRemainingDays: int option
        SupplierReliability: decimal
        IsSingleSource: bool
    }

let evolve
    (stateOpt: InventoryHealthAssessment option)
    (event: InventoryHealthEvent)
    : InventoryHealthAssessment option =
    match event with
    | InventoryHealthAssessmentPublished(assessment, _, _) -> Some assessment

let replay (events: InventoryHealthEvent seq) : InventoryHealthAssessment option = Seq.fold evolve None events
