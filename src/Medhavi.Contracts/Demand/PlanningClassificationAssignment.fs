module Medhavi.Contracts.Demand.PlanningClassificationAssignment

open System
open System.Threading.Tasks
open Medhavi.Contracts

type UpdatePlanningClassificationReq =
    { EntityType: string
      EntityId: string
      ClassificationType: string }

type OverridePlanningClassificationReq =
    { EntityType: string
      EntityId: string
      ClassificationType: string
      NewClassification: string
      Justification: string }

type PlanningClassificationAssignment =
    { EntityType: string
      EntityId: string
      ClassificationType: string
      CurrentClassification: string
      ClassificationConfidence: decimal
      LastClassified: DateTimeOffset
      BusinessTime: DateTimeOffset
      TransactionTime: DateTimeOffset }

type PlanningClassificationApi =
    { UpdateClassification: UpdatePlanningClassificationReq -> Task<Result<string, ApiError>>
      OverrideClassification: OverridePlanningClassificationReq -> Task<Result<string, ApiError>> }

type PlanningClassificationQueries = QueryService<PlanningClassificationAssignment, string>

type PlanningClassificationChangedNotification =
    { EntityType: string
      EntityId: string
      ClassificationType: string
      PreviousClassification: string
      NewClassification: string
      Reason: string
      Confidence: decimal }
