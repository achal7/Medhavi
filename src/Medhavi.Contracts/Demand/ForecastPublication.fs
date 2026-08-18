namespace Medhavi.Contracts.Demand

open System
open System.Threading.Tasks
open Medhavi.Contracts

// =============================================================================
// SE-D-003 — Forecast Publication Public Contracts (DTOs, Requests, API)
// =============================================================================

/// Prediction interval (80%, 90%, 95% confidence).
type PredictionIntervalDto =
    { Lower: decimal
      Upper: decimal
      ConfidenceLevel: decimal }

/// A single forecast line for one item-location-bucket.
type ForecastLineDto =
    { ItemId: string
      LocationId: string
      BucketStart: DateTimeOffset
      BucketEnd: DateTimeOffset
      Mean: decimal
      Interval: PredictionIntervalDto option
      ConfidenceScore: decimal
      ModelProvenance: string
      IsOverride: bool
      OriginalMean: decimal option
      UnforecastableFlag: bool
      UnforecastableReason: string option
      FallbackMethod: string option }

/// A declared assumption influencing the forecast publication.
type ForecastAssumptionDto =
    { AssumptionId: string
      Category: string
      Statement: string
      SignoffStatus: string
      SignoffAuthority: string option
      SignoffTimestamp: DateTimeOffset option }

/// A planner override record preserving original system forecast.
type ForecastOverrideDto =
    { OverrideId: string
      ItemId: string
      LocationId: string
      BucketStart: DateTimeOffset
      OriginalValue: decimal
      OverrideValue: decimal
      PlannerId: string
      Justification: string
      Timestamp: DateTimeOffset }

/// Forecast Publication Read Model DTO (SE-D-003).
type ForecastPublicationDto =
    { PublicationId: string
      PlanningScopeId: string
      HorizonStart: DateTimeOffset
      HorizonEnd: DateTimeOffset
      VersionNumber: int
      Lines: ForecastLineDto list
      Assumptions: ForecastAssumptionDto list
      Overrides: ForecastOverrideDto list
      ConfidenceIndex: decimal
      CompletenessScore: decimal
      ChampionModelId: string
      GenerationStatus: string
      PublicationTime: DateTimeOffset option
      LifecycleState: string }

// ---------- Commands / Requests ----------

type InitiateForecastCycleReq =
    { PlanningScopeId: string
      HorizonStart: DateTimeOffset
      HorizonEnd: DateTimeOffset
      InitiationReason: string }

type SelectChampionModelReq =
    { PublicationId: string
      ChampionModelId: string }

type ProduceForecastProjectionReq =
    { PublicationId: string }

type ApplyPlannerOverrideReq =
    { PublicationId: string
      ItemId: string
      LocationId: string
      BucketStart: DateTimeOffset
      NewValue: decimal
      Justification: string
      PlannerId: string }

type PublishForecastPublicationReq =
    { PublicationId: string }

// ---------- API Record ----------

type ForecastPublicationApi =
    { InitiateCycle: InitiateForecastCycleReq -> Task<Result<ForecastPublicationDto, ApiError>>
      SelectChampionModel: SelectChampionModelReq -> Task<Result<ForecastPublicationDto, ApiError>>
      ProduceProjection: ProduceForecastProjectionReq -> Task<Result<ForecastPublicationDto, ApiError>>
      ApplyOverride: ApplyPlannerOverrideReq -> Task<Result<ForecastPublicationDto, ApiError>>
      Publish: PublishForecastPublicationReq -> Task<Result<ForecastPublicationDto, ApiError>> }

/// Query service alias
type ForecastPublicationQueries = QueryService<ForecastPublicationDto, string>
