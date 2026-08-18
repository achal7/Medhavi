namespace Medhavi.Contracts.Demand

open System
open System.Threading.Tasks
open Medhavi.Contracts

// =============================================================================
// SE-D-006 — Demand Behavior Assignment Public Contracts
// =============================================================================

/// Statistical evidence features calculated for demand behavior pattern analysis (SE-D-006)
type StatisticalFeaturesDto =
    { CoefficientOfVariation: decimal
      SquaredCoefficientOfVariation: decimal
      AverageDemandInterval: decimal
      AutocorrelationAtSeasonalLag: decimal option
      TrendPValue: decimal option
      ZeroDemandRatio: decimal
      SamplePeriodCount: int }

/// Immutable audit record of a behavior classification change event (SE-D-006)
type BehaviorChangeEventDto =
    { PreviousClassification: string option
      NewClassification: string
      StatisticalFeatures: StatisticalFeaturesDto option
      Confidence: string
      Rationale: string
      PolicyVersion: string
      Timestamp: DateTimeOffset }

/// Demand Behavior Assignment Read Model DTO (SE-D-006)
type DemandBehaviorAssignmentDto =
    { AssignmentId: string
      ItemId: string
      LocationId: string
      Dimension: string
      CurrentClassification: string
      StatisticalFeatures: StatisticalFeaturesDto option
      ClassificationConfidence: string
      AssignmentRationale: string
      PolicyVersion: string
      ChangeEvents: BehaviorChangeEventDto list
      LastUpdated: DateTimeOffset }

// ---------- Commands / Requests ----------

type ClassifyDemandBehaviorReq =
    { ItemId: string
      LocationId: string
      Dimension: string
      DemandQuantities: decimal list }

type OverrideDemandBehaviorReq =
    { ItemId: string
      LocationId: string
      Dimension: string
      NewClassification: string
      Justification: string
      PlannerId: string }

// ---------- API Record ----------

type DemandBehaviorClassificationApi =
    { ClassifyBehavior: ClassifyDemandBehaviorReq -> Task<Result<DemandBehaviorAssignmentDto, ApiError>>
      OverrideBehavior: OverrideDemandBehaviorReq -> Task<Result<DemandBehaviorAssignmentDto, ApiError>> }

/// Query service alias
type DemandBehaviorClassificationQueries = QueryService<DemandBehaviorAssignmentDto, string>
