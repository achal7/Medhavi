namespace Medhavi.Demand

open System
open System.Text.Json.Serialization
open Medhavi.Contracts
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Failure

[<JsonFSharpConverter>]
type DemandObservationId = private DemandObservationId of string

module DemandObservationId =
    /// BR-D-001 — Observation identity must be unique and immutable
    let create = IdsFactory.createExplicitId DemandObservationId "DemandObservationId"
    let value (DemandObservationId id) = id

[<JsonFSharpConverter>]
type PlanningScopeId = private PlanningScopeId of string

module PlanningScopeId =
    let create (sku: string, sp: string, cust: string option, period: PlanningPeriod) =
        let custStr = cust |> Option.defaultValue "ALL"

        let periodKey =
            match period with
            | PlanningDay d -> $"D-{d:yyyyMMdd}"
            | PlanningWeek(y, w) -> $"W-{y}-{w}"
            | PlanningMonth(y, m) -> $"M-{y}-{m}"
            | PlanningQuarter(y, q) -> $"Q-{y}-{q}"

        let id = $"{sku}-{sp}-{custStr}-{periodKey}"
        Ok(PlanningScopeId id)

    let fromString (s: string) =
        if String.IsNullOrWhiteSpace s then
            Error(DomainError.validation "PlanningScopeId must not be empty")
        else
            Ok(PlanningScopeId s)

    let value (PlanningScopeId id) = id

/// Source integration metadata for replayability and audit traces
type Provenance =
    { SourceSystem: string
      ExternalRef: string
      MessageId: string
      Revision: Revision
      ScenarioId: ScenarioId option }

type DemandSignal =
    { SignalId: string
      Source: string
      SourceReliability: decimal
      Timestamp: DateTimeOffset
      Value: decimal
      StatisticalBound: decimal
      RecentBaseline: decimal }

[<JsonFSharpConverter>]
type ForecastPublicationId = ForecastPublicationId of string

module ForecastPublicationId =
    let create s =
        if String.IsNullOrWhiteSpace s then
            Error(DomainError.validation "ForecastPublicationId must not be empty")
        else
            Ok(ForecastPublicationId s)

    let value (ForecastPublicationId id) = id

[<JsonFSharpConverter>]
type ForecastQualityAssessmentId = ForecastQualityAssessmentId of string

module ForecastQualityAssessmentId =
    let create s =
        if String.IsNullOrWhiteSpace s then
            Error(DomainError.validation "Assessment ID required")
        else
            Ok(ForecastQualityAssessmentId s)

    let createFromScopeAndPeriod (scopeId: PlanningScopeId) (start: Timestamp) (end_: Timestamp) =
        let idStr = $"{PlanningScopeId.value scopeId}-{Timestamp.value start:yyyyMMdd}-{Timestamp.value end_:yyyyMMdd}"
        ForecastQualityAssessmentId idStr

    let value (ForecastQualityAssessmentId id) = id

[<JsonFSharpConverter>]
type DemandExplanationId = private DemandExplanationId of string

module DemandExplanationId =
    let create s =
        if String.IsNullOrWhiteSpace s then
            Error(DomainError.validation "Explanation ID required")
        else
            Ok(DemandExplanationId s)

    let value (DemandExplanationId id) = id


[<JsonFSharpConverter>]
type DemandPlanningConditionId = private DemandPlanningConditionId of string
module DemandPlanningConditionId =
    let create s =
        if String.IsNullOrWhiteSpace s then
            Error(DomainError.validation "DemandPlanningCondition ID required")
        else
            Ok(DemandPlanningConditionId s)

    let value (DemandPlanningConditionId id) = id

[<JsonFSharpConverter>]
type DemandLearningId = DemandLearningId of string
module DemandLearningId =
    let create (s: string) =
        if System.String.IsNullOrWhiteSpace s then Error(DomainError.validation "Learning ID required")
        else Ok (DemandLearningId s)
    let value (DemandLearningId id) = id
