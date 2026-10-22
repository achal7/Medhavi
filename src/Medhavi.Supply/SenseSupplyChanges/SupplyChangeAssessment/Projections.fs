module Medhavi.Supply.SenseSupplyChanges.SupplyChangeAssessment.Projections

open System
open Medhavi.Contracts.Supply
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

let private mapChangeEvent (e: AssessmentChangeEvent) : AssessmentChangeEventDto =
    { EventId = e.EventId
      PreviousEpistemicState = sprintf "%A" e.PreviousEpistemicState
      NewEpistemicState = sprintf "%A" e.NewEpistemicState
      TriggeringEvidence = e.TriggeringEvidence
      ReasonCode = e.ReasonCode
      Timestamp = Timestamp.value e.Timestamp }

let private mapSignalSummary (s: SignalSummary) : SignalSummaryDto =
    { SignalId = s.SignalId
      SourceId = s.SourceId
      SourceType = s.SourceType
      SourceReliabilityPrior = s.SourceReliabilityPrior
      ObservedQuantity = s.ObservedQuantity
      ObservedDate = Timestamp.value s.ObservedDate
      SignalGenerationTime = Timestamp.value s.SignalGenerationTime }

/// Pure mapping from domain aggregate to DTO
let mapToDto (agg: SupplyChangeAssessmentAggregate) : SupplyChangeAssessmentDto =
    { AssessmentId = SupplyChangeAssessmentId.value agg.AssessmentId
      MonitoredEntityType = sprintf "%A" agg.MonitoredEntityType
      MonitoredEntityId = agg.MonitoredEntityId
      ChangeType = sprintf "%A" agg.ChangeType
      CurrentEpistemicState = sprintf "%A" agg.CurrentEpistemicState
      ChangeClassification = sprintf "%A" agg.ChangeClassification
      ChangeDescription = agg.ChangeDescription
      Materiality = sprintf "%A" agg.Materiality
      OccurrenceTime = Timestamp.value agg.OccurrenceTime
      DetectionTime = Timestamp.value agg.DetectionTime
      ExpectedDuration = agg.ExpectedDuration
      SignalSummary = mapSignalSummary agg.SignalSummary
      AssessmentConfidence = sprintf "%A" agg.AssessmentConfidence
      DaysVariance = agg.DaysVariance
      QuantityVariance = agg.QuantityVariance
      QuantityVariancePercent = agg.QuantityVariancePercent
      ThreatenedObjectives = agg.ThreatenedObjectives
      CorrelatedChangeIds = agg.CorrelatedChangeIds |> List.map SupplyChangeAssessmentId.value
      History = agg.History |> List.map mapChangeEvent
      LifecycleState = sprintf "%A" agg.LifecycleState
      CreatedTimestamp = Timestamp.value agg.CreatedTimestamp
      UpdatedTimestamp = Timestamp.value agg.UpdatedTimestamp }

type State = Map<SupplyChangeAssessmentId, SupplyChangeAssessmentDto>

let initial: State = Map.empty

/// Pure projection fold (catamorphism)
let apply (state: State) (event: SupplyChangeEvent) : State =
    match event with
    | SupplyChangeAssessmentCreated agg
    | SupplyChangeAssessmentUpdated(agg, _)
    | SupplyChangeAssessmentArchived(agg, _) ->
        state |> Map.add agg.AssessmentId (mapToDto agg)

/// Seed projection from existing aggregates
let seedFromAggregates (aggregates: SupplyChangeAssessmentAggregate list) : State =
    aggregates |> List.fold (fun st agg -> Map.add agg.AssessmentId (mapToDto agg) st) initial
