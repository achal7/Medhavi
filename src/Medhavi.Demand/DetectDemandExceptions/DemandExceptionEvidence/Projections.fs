/// SE-D-009 — Demand Exception Evidence Read Model Projections
/// Pure Functional Projection Fold (Layer E: Catamorphism)
module Medhavi.Demand.DetectDemandExceptions.DemandExceptionEvidence.Projections

open Medhavi.SemanticModel
open Medhavi.Demand
open Medhavi.Contracts.Demand
open Model

let mapRecordToDto (record: DemandExceptionEvidenceRecord) : DemandExceptionEvidenceDto =
    { EvidenceId = DemandExceptionEvidenceId.value record.EvidenceId
      ExceptionType = record.ExceptionType.AsString
      EntityType = record.PlanningEntityType
      EntityId = record.PlanningEntityId
      ScopeId = PlanningScopeId.value record.Scope
      Severity = record.Severity |> Option.map (fun s -> s.AsString) |> Option.defaultValue "None"
      TriggeringMetric = record.TriggeringMetric
      MetricValue = record.MetricValue
      ThresholdValue = record.ThresholdValue
      Rationale = record.Rationale
      IsResolution = record.IsResolution
      Timestamp = Timestamp.value record.Timestamp }

let mapToDto (aggregate: DemandExceptionEvidenceAggregate) : DemandExceptionEvidenceDto option =
    aggregate.History
    |> List.tryHead
    |> Option.map mapRecordToDto

/// Projection state: Map of DemandExceptionEvidenceId to latest DTO
type State = Map<DemandExceptionEvidenceId, DemandExceptionEvidenceDto>

let initial: State = Map.empty

/// Pure projection fold (Layer E: Catamorphism)
let apply (state: State) (event: DemandExceptionEvent) : State =
    match event with
    | DemandExceptionDetected(agg, record)
    | DemandExceptionResolved(agg, record) ->
        let dto = mapRecordToDto record
        Map.add agg.Id dto state
    | DemandExceptionNoEvidence _ ->
        state

/// Seed projection from existing aggregates
let seedFromAggregates (aggregates: DemandExceptionEvidenceAggregate list) : State =
    aggregates
    |> List.fold
        (fun state agg ->
            match mapToDto agg with
            | Some dto -> Map.add agg.Id dto state
            | None -> state)
        initial
