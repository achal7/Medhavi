namespace Medhavi.Contracts.Demand

open System
open System.Threading.Tasks
open Medhavi.Contracts

// =============================================================================
// SE-D-007 — Planning Priority Assignment Public Contracts
// =============================================================================

/// Breakdown of multi-dimensional prioritization scores (SE-D-007)
type DimensionScoreBreakdownDto =
    { RevenueScore: decimal
      StrategyScore: decimal
      RiskScore: decimal
      ContractualScore: decimal }

/// Immutable audit record of a planning priority change event (SE-D-007)
type PriorityChangeEventDto =
    { PreviousPriority: string option
      NewPriority: string
      PriorityScore: decimal
      DimensionBreakdown: DimensionScoreBreakdownDto
      Rationale: string
      BusinessValidity: string
      PolicyVersion: string
      Timestamp: DateTimeOffset }

/// Planning Priority Assignment Read Model DTO (SE-D-007)
type PlanningPriorityDto =
    { AssignmentId: string
      EntityType: string
      EntityId: string
      CurrentPriority: string
      PriorityScore: decimal
      DimensionBreakdown: DimensionScoreBreakdownDto
      DecisionRationale: string
      BusinessValidity: string
      PolicyVersion: string
      ChangeEvents: PriorityChangeEventDto list
      LastUpdated: DateTimeOffset }

// ---------- Commands / Requests ----------

type PrioritizePlanningEntityReq =
    { EntityType: string
      EntityId: string
      RevenueContribution: decimal option
      StrategicImportance: decimal option
      RiskExposure: decimal option
      ContractualObligation: decimal option }

type OverridePlanningPriorityReq =
    { EntityType: string
      EntityId: string
      NewPriority: string
      Justification: string
      PlannerId: string }

// ---------- API Record ----------

type PlanningPriorityApi =
    { PrioritizeEntity: PrioritizePlanningEntityReq -> Task<Result<PlanningPriorityDto, ApiError>>
      OverridePriority: OverridePlanningPriorityReq -> Task<Result<PlanningPriorityDto, ApiError>> }

/// Query service alias
type PlanningPriorityQueries = QueryService<PlanningPriorityDto, string>
