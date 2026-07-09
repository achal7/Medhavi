module Medhavi.Contracts.Demand.PlanningPriorityAssignment

open System
open System.Threading.Tasks
open Medhavi.Contracts

type UpdatePlanningPriorityReq =
    { EntityType: string; EntityId: string }

type OverridePlanningPriorityReq =
    { EntityType: string
      EntityId: string
      NewPriority: string
      Justification: string }

type PlanningPriorityAssignment =
    { EntityType: string
      EntityId: string
      CurrentPriority: string
      PriorityScore: decimal
      DecisionRationale: string
      BusinessValidity: string
      LastEvaluated: DateTimeOffset
      BusinessTime: DateTimeOffset
      TransactionTime: DateTimeOffset }

type PlanningPriorityAssignmentApi =
    { UpdatePriority: UpdatePlanningPriorityReq -> Task<Result<string, ApiError>>
      OverridePriority: OverridePlanningPriorityReq -> Task<Result<string, ApiError>> }

type PlanningPriorityQueries = QueryService<PlanningPriorityAssignment, string>

type PlanningPriorityChangedNotification =
    { EntityType: string
      EntityId: string
      PreviousPriority: string
      NewPriority: string
      PriorityScore: decimal
      DecisionRationale: string
      BusinessValidity: string }
