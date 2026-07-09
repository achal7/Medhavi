module Medhavi.Demand.ForecastPublication.Model

open System
open Medhavi.SharedKernel
open Medhavi.Contracts
open Medhavi.Demand

// =============================================================================
// SE‑D‑029 — Forecast Publication
// =============================================================================

// ---------- Reused types (from your existing code) ----------
type PredictionInterval =
    { LowerBound: PositiveDecimal
      UpperBound: PositiveDecimal
      ConfidenceLevel: PositiveDecimal }

type ForecastModelType =
    | Statistical
    | MachineLearning
    | Judgemental
    | Hybrid
    | Naive

type ForecastModel =
    { ModelId: string
      ModelType: ForecastModelType
      Hyperparameters: Map<string, string>
      TrainedAt: Timestamp }

type Forecast =
    { ForecastId: ForecastId
      SkuId: SkuId
      StockingPointId: StockingPointId
      PlanningPeriod: PlanningPeriod
      Mean: decimal
      PredictionInterval: PredictionInterval
      Confidence: PositiveDecimal
      ModelId: string
      GeneratedAt: Timestamp
      OverrideReason: string option }

// ---------- New types for Forecast Publication ----------

type LifecycleState =
    | Declared
    | Validated
    | Approved
    | Withdrawn

type ForecastAssumption =
    { AssumptionId: string
      Statement: string
      DeclaredBy: string
      LifecycleState: LifecycleState
      LinkedDriverRef: string option
      Timestamp: Timestamp }

type ForecastOverride =
    { ForecastId: ForecastId
      OriginalValue: decimal
      OverrideValue: decimal
      Justification: string
      PlannerIdentity: string
      DecisionId: string
      OverrideTimestamp: Timestamp }

type ForecastPublicationStatus =
    | Draft
    | Published
    | Superseded

type ForecastPublication =
    { Id: ForecastPublicationId
      PlanningScopeIds: PlanningScopeId list
      ForecastHorizon: TimeSpan
      TimeBucketConfig: string
      Status: ForecastPublicationStatus
      Version: int
      ChampionModelId: string option
      OverallConfidenceIndex: decimal option
      Forecasts: Map<string, Forecast>
      Assumptions: Map<string, ForecastAssumption>
      Overrides: Map<string, ForecastOverride>
      Coverage: (SkuId * StockingPointId) list
      TransactionTime: Timestamp
      PublicationTime: Timestamp option
      SupersededPublicationId: string option }

// ---------- Commands ----------
type InitiateForecastCycleCmd =
    { PublicationId: ForecastPublicationId
      PlanningScopeIds: PlanningScopeId list
      ForecastHorizon: TimeSpan
      TimeBucketConfig: string }

type PrepareForecastContextCmd =
    { PublicationId: ForecastPublicationId
      Assumptions: ForecastAssumption list
      Coverage: (SkuId * StockingPointId) list }

type SelectChampionModelCmd =
    { PublicationId: ForecastPublicationId
      CandidateModelId: string
      EvaluationWindowStart: Timestamp
      EvaluationWindowEnd: Timestamp }

type GenerateBaselineForecastsCmd =
    { PublicationId: ForecastPublicationId
      Forecasts: Forecast list }

type RecordForecastOverrideCmd =
    { PublicationId: ForecastPublicationId
      ForecastId: ForecastId
      NewValue: PositiveDecimal
      Justification: string
      PlannerIdentity: string }

type ReconcileForecastHierarchyCmd =
    { PublicationId: ForecastPublicationId
      TargetTotal: decimal option }

type PublishForecastPublicationCmd =
    { PublicationId: ForecastPublicationId }

type ForecastPublicationCommand =
    | InitiateForecastCycle of InitiateForecastCycleCmd
    | PrepareForecastContext of PrepareForecastContextCmd
    | SelectChampionModel of SelectChampionModelCmd
    | GenerateBaselineForecasts of GenerateBaselineForecastsCmd
    | RecordForecastOverride of RecordForecastOverrideCmd
    | ReconcileForecastHierarchy of ReconcileForecastHierarchyCmd
    | PublishForecastPublication of PublishForecastPublicationCmd

    member this.PublicationId =
        match this with
        | InitiateForecastCycle c -> c.PublicationId
        | PrepareForecastContext c -> c.PublicationId
        | SelectChampionModel c -> c.PublicationId
        | GenerateBaselineForecasts c -> c.PublicationId
        | RecordForecastOverride c -> c.PublicationId
        | ReconcileForecastHierarchy c -> c.PublicationId
        | PublishForecastPublication c -> c.PublicationId
        |> ForecastPublicationId.value

// ---------- Events ----------
type ForecastPublicationEvent =
    | ForecastCycleInitiated of ForecastPublication * ForecastAssumption list * (SkuId * StockingPointId) list
    | ForecastContextPrepared of ForecastPublication
    | ChampionModelSelected of
        ForecastPublication *
        oldModelId: string *
        newModelId: string *
        metrics: Map<string, decimal>
    | BaselineForecastsGenerated of ForecastPublication * Forecast list
    | ForecastOverrideRecorded of ForecastPublication * ForecastOverride
    | ForecastHierarchyReconciled of ForecastPublication
    | ForecastPublicationPublished of ForecastPublication * previousVersion: int option

// ---------- Evolve ----------
let evolve (event: ForecastPublicationEvent) (state: ForecastPublication option) : ForecastPublication option =
    match event with
    | ForecastCycleInitiated(pub, _, _) -> Some pub
    | ForecastContextPrepared pub -> Some pub
    | ChampionModelSelected(pub, _, _, _) -> Some pub
    | BaselineForecastsGenerated(pub, _) -> Some pub
    | ForecastOverrideRecorded(pub, _) -> Some pub
    | ForecastHierarchyReconciled pub -> Some pub
    | ForecastPublicationPublished(pub, _) -> Some pub
