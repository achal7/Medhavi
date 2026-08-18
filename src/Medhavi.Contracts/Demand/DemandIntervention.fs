namespace Medhavi.Contracts.Demand

open System
open System.Threading.Tasks
open Medhavi.Contracts

/// DTO for Temporal Window interval
type TemporalWindowDto =
    { Start: DateTimeOffset
      End: DateTimeOffset }

/// DTO representing an external Scenario Adjustment intervention (SE-C-039)
type ScenarioAdjustmentDto =
    { AdjustmentId: string
      InterventionType: string
      Magnitude: decimal
      TemporalWindow: TemporalWindowDto
      IsActive: bool }

/// Canonical SE-D-018 Demand Intervention Impact DTO
type DemandInterventionImpactDto =
    { ImpactId: string
      InterventionReference: string
      Item: string
      Location: string
      AssessedDemandLift: decimal
      LiftConfidence: decimal
      TemporalValidityStart: DateTimeOffset
      TemporalValidityEnd: DateTimeOffset
      ModelProvenance: string
      LifecycleState: string
      Version: int
      CreatedAt: DateTimeOffset
      PublishedAt: DateTimeOffset option }

// ---------- Commands / Requests ----------

/// Request payload to assess the impact of a planned intervention (creates Draft)
type AssessInterventionImpactReq =
    { ImpactId: string
      InterventionReference: string
      Item: string
      Location: string
      InterventionType: string
      InterventionMagnitude: decimal
      TemporalValidityStart: DateTimeOffset
      TemporalValidityEnd: DateTimeOffset
      HistoricalPairs: (decimal * decimal) list
      BaselineDemand: decimal option }

/// Request payload to publish a Draft impact assessment (authoritative transition)
type PublishInterventionImpactReq =
    { ImpactId: string }

// ---------- API Interface ----------

type DemandInterventionApi =
    { AssessImpact: AssessInterventionImpactReq -> Task<Result<DemandInterventionImpactDto, ApiError>>
      PublishImpact: PublishInterventionImpactReq -> Task<Result<DemandInterventionImpactDto, ApiError>> }

/// Query service alias
type DemandInterventionQueries = QueryService<DemandInterventionImpactDto, string>
