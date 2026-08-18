/// SE-D-009 — Demand Exception Evidence Aggregate Model
/// Traces to: Demand Intelligence Specification (SE-D-009, CR-D-015, DE-D-012, FS-D-015, Chapter 4.3.1)
module Medhavi.Demand.DetectDemandExceptions.DemandExceptionEvidence.Model

open System
open Medhavi.SemanticModel
open Medhavi.Demand
open Medhavi.Foundation.Failure

// ---------- Governed Enums ----------

/// Governed Demand Exception Types per PO-D-044 (SE-D-009)
type DemandExceptionType =
    | ForecastBiasElevated
    | ForecastAccuracyDegraded
    | DataCompletenessGap
    | DemandBehaviorCritical
    | TrackingSignalDrift
    | NegativeForecastValueAdd

    member this.AsString =
        match this with
        | ForecastBiasElevated -> "ForecastBiasElevated"
        | ForecastAccuracyDegraded -> "ForecastAccuracyDegraded"
        | DataCompletenessGap -> "DataCompletenessGap"
        | DemandBehaviorCritical -> "DemandBehaviorCritical"
        | TrackingSignalDrift -> "TrackingSignalDrift"
        | NegativeForecastValueAdd -> "NegativeForecastValueAdd"

    static member FromString(s: string) : Result<DemandExceptionType, DomainError> =
        match s.Trim().ToLowerInvariant() with
        | "forecastbiaselevated" | "forecastbias" -> Ok ForecastBiasElevated
        | "forecastaccuracydegraded" | "forecastaccuracy" -> Ok ForecastAccuracyDegraded
        | "datacompletenessgap" | "completeness" -> Ok DataCompletenessGap
        | "demandbehaviorcritical" | "behaviorcritical" -> Ok DemandBehaviorCritical
        | "trackingsignaldrift" | "trackingsignal" -> Ok TrackingSignalDrift
        | "negativeforecastvalueadd" | "fva" -> Ok NegativeForecastValueAdd
        | other -> DomainError.validation $"Unsupported DemandExceptionType: '{other}'" |> Error

/// Governed Demand Exception Severity Levels per PO-D-044 & DE-D-012
type DemandExceptionSeverity =
    | Critical
    | High
    | Medium
    | Low

    member this.AsString =
        match this with
        | Critical -> "Critical"
        | High -> "High"
        | Medium -> "Medium"
        | Low -> "Low"

    static member FromString(s: string) : Result<DemandExceptionSeverity, DomainError> =
        match s.Trim().ToLowerInvariant() with
        | "critical" -> Ok Critical
        | "high" -> Ok High
        | "medium" -> Ok Medium
        | "low" -> Ok Low
        | other -> DomainError.validation $"Unsupported DemandExceptionSeverity: '{other}'" |> Error

// ---------- Lifecycle Determination ----------

/// DE-D-012 Decision Determination
type LifecycleDetermination =
    | DetectionEvidenceExists of
        Severity: DemandExceptionSeverity *
        TriggeringMetric: string *
        MetricValue: decimal *
        ThresholdValue: decimal *
        Rationale: string
    | ResolutionEvidenceExists of
        ResolutionMetric: string *
        MetricValue: decimal *
        ThresholdValue: decimal *
        Rationale: string
    | NoEvidence

// ---------- Immutable Evidence Record ----------

/// Single preserved evidence record produced for Core Exception Management
type DemandExceptionEvidenceRecord =
    { EvidenceId: DemandExceptionEvidenceId
      ExceptionType: DemandExceptionType
      PlanningEntityType: string
      PlanningEntityId: string
      Scope: PlanningScopeId
      Severity: DemandExceptionSeverity option
      TriggeringMetric: string
      MetricValue: decimal
      ThresholdValue: decimal
      Rationale: string
      IsResolution: bool
      Timestamp: Timestamp }

// ---------- Aggregate Root State ----------

/// SE-D-009 — Demand Exception Evidence Aggregate Root
/// Identity: ExceptionType + EntityType + EntityId + Scope (DemandExceptionEvidenceId)
type DemandExceptionEvidenceAggregate =
    { Id: DemandExceptionEvidenceId
      ExceptionType: DemandExceptionType
      PlanningEntityType: string
      PlanningEntityId: string
      Scope: PlanningScopeId
      ActiveSeverity: DemandExceptionSeverity option
      LastTriggeringMetric: string
      LastMetricValue: decimal
      LastThresholdValue: decimal
      LastRationale: string
      IsCurrentlyActive: bool
      History: DemandExceptionEvidenceRecord list
      LastUpdated: Timestamp }

// ---------- Commands ----------

/// AB-D-015 Command: Evaluate Demand Exception Evidence
type EvaluateDemandExceptionCmd =
    { EvidenceId: DemandExceptionEvidenceId
      Scope: PlanningScopeId
      PlanningEntityType: string
      PlanningEntityId: string
      ExceptionType: DemandExceptionType
      TriggeringMetric: string
      MetricValue: decimal
      HistoricalValues: decimal list
      EvaluationTime: Timestamp }

// ---------- Enterprise Events ----------

/// Enterprise Events emitted by Demand Exception Evidence aggregate
type DemandExceptionEvent =
    | DemandExceptionDetected of
        Aggregate: DemandExceptionEvidenceAggregate *
        Record: DemandExceptionEvidenceRecord
    | DemandExceptionResolved of
        Aggregate: DemandExceptionEvidenceAggregate *
        Record: DemandExceptionEvidenceRecord
    | DemandExceptionNoEvidence of
        Aggregate: DemandExceptionEvidenceAggregate

// ---------- Pure State Evolution (Layer E: Catamorphism) ----------

let evolve: Medhavi.Foundation.Contracts.Evolve<DemandExceptionEvidenceAggregate, DemandExceptionEvent> =
    fun (_: DemandExceptionEvidenceAggregate option) (event: DemandExceptionEvent) ->
        match event with
        | DemandExceptionDetected(agg, _) -> Some agg
        | DemandExceptionResolved(agg, _) -> Some agg
        | DemandExceptionNoEvidence(agg) -> Some agg

/// Replay event sequence to rehydrate aggregate state
let replay (events: DemandExceptionEvent seq) : DemandExceptionEvidenceAggregate option =
    Seq.fold evolve None events
