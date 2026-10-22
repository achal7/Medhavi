namespace Medhavi.Contracts.Supply

open System
open System.Threading.Tasks
open Medhavi.Contracts

// ---------- Governed Enums ----------

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

// ---------- Structured Components ----------

type PlannedOrderDto =
    { OrderId: string
      Item: string
      Location: string
      SupplyType: PlannedSupplyType
      Quantity: decimal
      UnitOfMeasure: string
      ReleaseDate: DateTimeOffset
      DueDate: DateTimeOffset
      AssignedResource: string option
      AssignedSupplier: string option
      SourceLocation: string option }

type ProjectedShortfallDto =
    { Item: string
      Location: string
      PeriodBucket: int
      ShortfallQuantity: decimal
      UnitOfMeasure: string
      Reason: string }

type ConstraintViolationDto =
    { Resource: string
      PeriodBucket: int
      RequiredLoad: decimal
      MaxPlannedCapacity: decimal
      OverloadQuantity: decimal
      UnitOfMeasure: string
      IsRelaxationApproved: bool
      RelaxationApprovalReference: string option }

// ---------- DTOs ----------

type SupplyPlanVersionDto =
    { VersionId: string
      VersionNumber: int
      LifecycleState: string // Draft / Published / Superseded
      PlannedOrders: PlannedOrderDto list
      ProjectedShortfalls: ProjectedShortfallDto list
      ConstraintViolations: ConstraintViolationDto list
      FeasibilityStatus: PlanFeasibilityStatus
      ServiceLevelAchieved: decimal
      MaxCapacityUtilization: decimal
      PlanStabilityScore: decimal
      PlanConfidenceScore: decimal
      SourceSupplyUnderstandingVersion: int
      SourceForecastPublicationVersion: int
      SourceEnterprisePictureVersion: int64
      GenerationTime: DateTimeOffset
      EvaluationOutcome: PlanEvaluationOutcome option
      EvaluationTime: DateTimeOffset option
      PublicationTime: DateTimeOffset option }

type SupplyPlanDto =
    { PlanningScopeId: string
      ForecastHorizonId: string
      CurrentVersion: SupplyPlanVersionDto option
      AllVersions: SupplyPlanVersionDto list }

// ---------- Commands ----------

type GenerateSupplyPlanReq =
    { PlanningScopeId: string
      ForecastHorizonId: string
      SupplyUnderstandingVersion: int
      ForecastPublicationVersion: int
      EnterprisePictureVersion: int64 }

type EvaluateSupplyPlanReq =
    { PlanningScopeId: string
      ForecastHorizonId: string
      VersionNumber: int }

type PublishSupplyPlanReq =
    { PlanningScopeId: string
      ForecastHorizonId: string
      VersionNumber: int
      IsOutCycle: bool }

// ---------- Notifications ----------

/// BN-S-010: Supply Plan Published
type SupplyPlanPublishedNotification =
    { PlanningScopeId: string
      ForecastHorizonId: string
      PublishedVersionNumber: int
      SupersededVersionIdentifier: string option
      PublicationTimestamp: DateTimeOffset
      PlanConfidenceScore: decimal
      ConstraintSummary: string
      PlanningAssumptionsSummary: string
      MaterialPlanningChanges: string }

// ---------- API Records ----------

type SupplyPlanApi =
    { Generate: GenerateSupplyPlanReq -> Task<Result<SupplyPlanDto, ApiError>>
      Evaluate: EvaluateSupplyPlanReq -> Task<Result<SupplyPlanDto, ApiError>>
      Publish: PublishSupplyPlanReq -> Task<Result<SupplyPlanDto, ApiError>> }

type SupplyPlanQueries = QueryService<SupplyPlanDto, string>
