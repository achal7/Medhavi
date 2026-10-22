/// SE-S-020 — Supply Plan Aggregate Model
/// Traces to: Supply Intelligence Specification (SE-S-020, SE-S-021, AB-S-010, AB-S-011, AB-S-012)
module Medhavi.Supply.PlanSupply.SupplyPlan.Model

open Medhavi.SemanticModel
open Medhavi.Supply

type PlanFeasibilityStatus =
    | FullyFeasible
    | ConstrainedInfeasible

type PlanEvaluationOutcome =
    | Accepted
    | AcceptedWithWarnings
    | Rejected

type PlannedSupplyType =
    | Production
    | Procurement
    | Transfer

type PlannedSupplyOrder =
    { OrderId: PlannedSupplyOrderId
      Item: ItemId
      Location: LocationId
      SupplyType: PlannedSupplyType
      Quantity: Quantity
      UnitOfMeasure: UnitOfMeasureId
      ReleaseDate: Timestamp
      DueDate: Timestamp
      AssignedResource: PhysicalResourceId option
      AssignedSupplier: SupplierId option
      SourceLocation: LocationId option }

type ShortfallRecord =
    { Item: ItemId
      Location: LocationId
      PeriodBucket: int
      ShortfallQuantity: Quantity
      UnitOfMeasure: UnitOfMeasureId
      Reason: string }

type ConstraintViolationRecord =
    { Resource: PhysicalResourceId
      PeriodBucket: int
      RequiredLoad: decimal
      MaxPlannedCapacity: decimal
      OverloadQuantity: decimal
      UnitOfMeasure: UnitOfMeasureId
      IsRelaxationApproved: bool
      RelaxationApprovalReference: string option }

type VersionState =
    | Draft
    | Published
    | Superseded

/// SE-S-021 — Supply Plan Version entity
type SupplyPlanVersion =
    { VersionId: SupplyPlanVersionId
      VersionNumber: int
      State: VersionState
      PlannedOrders: PlannedSupplyOrder list
      ProjectedShortfalls: ShortfallRecord list
      ConstraintViolations: ConstraintViolationRecord list
      FeasibilityStatus: PlanFeasibilityStatus
      ServiceLevelAchieved: decimal
      MaxCapacityUtilization: decimal
      PlanStabilityScore: decimal
      PlanConfidenceScore: decimal
      SourceSupplyUnderstandingVersion: int
      SourceForecastPublicationVersion: int
      SourceEnterprisePictureVersion: int64
      GenerationTime: Timestamp
      EvaluationOutcome: PlanEvaluationOutcome option
      EvaluationTime: Timestamp option
      PublicationTime: Timestamp option }

/// SE-S-020 — Supply Plan Aggregate Root
type SupplyPlan =
    { PlanningScopeId: PlanningScopeId
      ForecastHorizonId: ForecastHorizonId
      Versions: SupplyPlanVersion list }

// ---------- Domain Commands ----------

type GeneratePlanCmd =
    { PlanningScopeId: PlanningScopeId
      ForecastHorizonId: ForecastHorizonId
      SupplyUnderstandingVersion: int
      ForecastPublicationVersion: int
      EnterprisePictureVersion: int64
      Timestamp: Timestamp }

type EvaluatePlanCmd =
    { PlanningScopeId: PlanningScopeId
      ForecastHorizonId: ForecastHorizonId
      VersionNumber: int
      Timestamp: Timestamp }

type PublishPlanCmd =
    { PlanningScopeId: PlanningScopeId
      ForecastHorizonId: ForecastHorizonId
      VersionNumber: int
      IsOutCycle: bool
      Timestamp: Timestamp }

// ---------- Domain Events (Enterprise Events EV-S-010..012) ----------

type SupplyPlanEvent =
    | SupplyPlanGenerated of plan: SupplyPlan * draftVersion: int
    | SupplyPlanEvaluated of plan: SupplyPlan * versionNumber: int * outcome: PlanEvaluationOutcome
    | SupplyPlanPublished of plan: SupplyPlan * publishedVersion: int * supersededVersion: int option
