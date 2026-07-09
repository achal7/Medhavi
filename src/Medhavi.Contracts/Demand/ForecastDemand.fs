namespace Medhavi.Contracts.Demand

open System
open System.Threading.Tasks
open Medhavi.Contracts

type GenerateForecastReq =
    { CycleId: string
      HorizonStart: DateTimeOffset
      HorizonEnd: DateTimeOffset
      SkuIds: string list option
      StockingPointIds: string list option }

type SelectChampionModelReq =
    { CycleId: string
      CandidateModelId: string
      EvaluationWindowStart: DateTimeOffset
      EvaluationWindowEnd: DateTimeOffset }

type OverrideForecastReq =
    { ForecastId: string
      CycleId: string
      NewValue: decimal
      Justification: string }

type PublishForecastReq = { ForecastCycleId: string }

type Forecast =
    { ForecastId: string
      SkuId: string
      CycleId: string
      StockingPointId: string
      TimeBucket: DateTimeOffset
      Mean: decimal
      LowerBound: decimal
      UpperBound: decimal
      Confidence: decimal
      ModelId: string
      GeneratedAt: DateTimeOffset
      OverrideReason: string option
      PublishedAt: DateTimeOffset option }

type ForecastApi =
    { Generate: GenerateForecastReq -> Task<Result<int, ApiError>>
      SelectChampion: SelectChampionModelReq -> Task<Result<string, ApiError>>
      Override: OverrideForecastReq -> Task<Result<unit, ApiError>>
      Publish: PublishForecastReq -> Task<Result<unit, ApiError>> }

type ForecastCreatedNotification = { ForecastId: string }

type ForecastUpdatedNotification = { ForecastId: string }

type ForecastDeletedNotification = { ForecastId: string }

// type ForecastQueries = QueryService<Forecast, string>

type ForecastQueries =
    { GetById: string -> Task<Forecast option>
      GetAll: unit -> Task<Forecast list>
      GetBySku: string -> Task<Forecast list>
      GetByStockingPoint: string -> Task<Forecast list>
      GetByTimeBucket: DateTimeOffset -> Task<Forecast list>
      GetPublished: unit -> Task<Forecast list> }
