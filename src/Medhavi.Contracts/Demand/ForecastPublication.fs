module Medhavi.Contracts.Demand.ForecastPublication

open System
open System.Threading.Tasks
open Medhavi.Contracts

type CoverageItem =
    { SkuId: string
      StockingPointId: string }

type ForecastPublication =
    { PublicationId: string
      Version: int
      Status: string
      PlanningScopeIds: string list
      ForecastHorizon: string
      ChampionModelId: string option
      OverallConfidenceIndex: decimal option
      Coverage: CoverageItem list
      Forecasts: Forecast list
      Assumptions: Assumption list
      Overrides: Override list
      TransactionTime: DateTimeOffset
      PublicationTime: DateTimeOffset option }

and Forecast =
    { ForecastId: string
      SkuId: string
      StockingPointId: string
      PlanningPeriod: PlanningPeriod
      Mean: decimal
      LowerBound: decimal
      UpperBound: decimal
      Confidence: decimal
      ModelId: string
      OverrideReason: string option }

and Assumption =
    { AssumptionId: string
      Statement: string
      DeclaredBy: string
      LifecycleState: string
      LinkedDriverRef: string option }

and Override =
    { ForecastId: string
      OriginalValue: decimal
      OverrideValue: decimal
      Justification: string
      PlannerIdentity: string
      OverrideTimestamp: DateTimeOffset }

// Commands
type InitiateForecastCycleReq =
    { PublicationId: string
      PlanningScopeIds: string list
      ForecastHorizon: string
      TimeBucketConfig: string }

type PrepareForecastContextReq =
    { PublicationId: string
      Assumptions: Assumption list
      Coverage: CoverageItem list }

type SelectChampionModelReq =
    { PublicationId: string
      CandidateModelId: string
      EvaluationWindowStart: DateTimeOffset
      EvaluationWindowEnd: DateTimeOffset }

type GenerateBaselineForecastsReq =
    { PublicationId: string
      Forecasts: Forecast list option }

type RecordForecastOverrideReq =
    { PublicationId: string
      ForecastId: string
      NewValue: decimal
      Justification: string
      PlannerIdentity: string }

type ReconcileForecastHierarchyReq =
    { PublicationId: string
      TargetTotal: decimal option }

type PublishForecastPublicationReq = { PublicationId: string }

type ForecastPublishedNotification =
    { PublicationId: string
      Version: int
      PublicationTime: DateTimeOffset }

type ForecastPublicationFailedNotification =
    { PublicationId: string
      Reason: string }

type ForecastCycleInitialisedNotification =
    { PublicationId: string
      PlanningScopeIds: string list
      CycleTime: DateTimeOffset }

type ForecastOverrideRecordedNotification =
    { PublicationId: string
      ForecastId: string
      OverrideValue: decimal
      PlannerIdentity: string }

// API
type ForecastPublicationApi =
    { InitiateCycle: InitiateForecastCycleReq -> Task<Result<string, ApiError>>
      PrepareContext: PrepareForecastContextReq -> Task<Result<string, ApiError>>
      SelectChampion: SelectChampionModelReq -> Task<Result<string, ApiError>>
      GenerateBaseline: GenerateBaselineForecastsReq -> Task<Result<string, ApiError>>
      RecordOverride: RecordForecastOverrideReq -> Task<Result<string, ApiError>>
      Reconcile: ReconcileForecastHierarchyReq -> Task<Result<string, ApiError>>
      Publish: PublishForecastPublicationReq -> Task<Result<string, ApiError>> }

// Queries
type ForecastPublicationQueries = QueryService<ForecastPublication, string>
