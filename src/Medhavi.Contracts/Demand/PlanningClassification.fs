namespace Medhavi.Contracts.Demand

open System
open System.Threading.Tasks
open Medhavi.Contracts

// =============================================================================
// SE-D-005 — Planning Classification Assignment Public Contracts
// =============================================================================

/// Immutable audit record of a classification change event (SE-D-005)
type AssignmentChangeEventDto =
    { PreviousClassification: string option
      NewClassification: string
      ClassificationScore: decimal
      ClassificationConfidence: string
      Rationale: string
      PolicyVersion: string
      Timestamp: DateTimeOffset }

/// Planning Classification Assignment Read Model DTO (SE-D-005)
type PlanningClassificationDto =
    { AssignmentId: string
      EntityType: string
      EntityId: string
      ClassificationType: string
      CurrentClassification: string
      AnalogItemReference: string option
      ClassificationScore: decimal
      ClassificationConfidence: string
      AssignmentRationale: string
      PolicyVersion: string
      ChangeEvents: AssignmentChangeEventDto list
      LastUpdated: DateTimeOffset }

// ---------- Commands / Requests ----------

type ClassifyPlanningEntityReq =
    { EntityType: string
      EntityId: string
      ClassificationType: string
      VolumeOrRevenuePercentage: decimal option
      HistoricalDemandValues: decimal list option
      AnalogItemId: string option }

type OverridePlanningClassificationReq =
    { EntityType: string
      EntityId: string
      ClassificationType: string
      NewClassification: string
      Justification: string
      PlannerId: string }

// ---------- API Record ----------

type PlanningClassificationApi =
    { ClassifyEntity: ClassifyPlanningEntityReq -> Task<Result<PlanningClassificationDto, ApiError>>
      OverrideClassification: OverridePlanningClassificationReq -> Task<Result<PlanningClassificationDto, ApiError>> }

/// Query service alias
type PlanningClassificationQueries = QueryService<PlanningClassificationDto, string>
