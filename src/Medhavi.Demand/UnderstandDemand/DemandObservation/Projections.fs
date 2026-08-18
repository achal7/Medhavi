module Medhavi.Demand.UnderstandDemand.DemandObservation.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Model

/// Map aggregate state to DTO.
let mapToDto (obs: DemandObservation) : DemandObservationDto =
    { ObservationId = DemandObservationId.value obs.ObservationId
      Item = ItemId.value obs.Item
      Location = LocationId.value obs.Location
      Quantity = Quantity.value obs.Quantity
      ObservationType = VocabularyEntryId.value obs.ObservationType
      BusinessTime = Timestamp.value obs.BusinessTime
      ObservationTime = Timestamp.value obs.ObservationTime
      SourceSystemProvenance = obs.SourceSystemProvenance
      UnitOfMeasure = obs.Quantity |> (Quantity.unitOfMeasure >> UnitOfMeasureId.value)
      LifecycleState =
        match obs.LifecycleState with
        | Received -> "Received"
        | Accepted -> "Accepted"
        | Quarantined -> "Quarantined"
        | Rejected -> "Rejected"
      DecisionTraceability = obs.DecisionTraceability }

/// Projection state: Map of ObservationId to DTO.
type State = Map<DemandObservationId, DemandObservationDto>

let initial: State = Map.empty

/// Pure projection fold (Layer E: Catamorphism).
let apply (state: State) (event: ObservationEvent) : State =
    match event with
    | ObservationReceived obs -> state |> Map.add obs.ObservationId (mapToDto obs)
    | ObservationAccepted(obsId, _, traceId) ->
        state
        |> Map.change
            obsId
            (Option.map(fun dto ->
                { dto with
                    LifecycleState = "Accepted"
                    DecisionTraceability = Some traceId }))
    | ObservationQuarantined(obsId, _, traceId) ->
        state
        |> Map.change
            obsId
            (Option.map(fun dto ->
                { dto with
                    LifecycleState = "Quarantined"
                    DecisionTraceability = Some traceId }))
    | ObservationRejected(obsId, _, traceId) ->
        state
        |> Map.change
            obsId
            (Option.map(fun dto ->
                { dto with
                    LifecycleState = "Rejected"
                    DecisionTraceability = Some traceId }))

/// Seed projection from existing aggregates.
let seedFromAggregates (aggregates: DemandObservation list) : State =
    aggregates
    |> List.fold
        (fun state agg ->
            let dto = mapToDto agg
            Map.add agg.ObservationId dto state)
        initial
