module Medhavi.Demand.UnderstandDemand.DemandObservation.Model

open Medhavi.SemanticModel
open Medhavi.Demand

type ObservationLifecycleState =
    | Received
    | Accepted
    | Quarantined
    | Rejected

/// SE-D-001 Demand Observation
type DemandObservation =
    { ObservationId: DemandObservationId
      Item: ItemId
      Location: LocationId
      Quantity: Quantity
      ObservationType: VocabularyEntryId
      BusinessTime: Timestamp
      ObservationTime: Timestamp
      SourceSystemProvenance: string
      LifecycleState: ObservationLifecycleState
      DecisionTraceability: string option }

/// AB-D-001 Input
type ReceiveObservationCmd =
    { ObservationId: DemandObservationId
      Item: ItemId
      Location: LocationId
      Quantity: Quantity
      ObservationType: VocabularyEntryId
      BusinessTime: Timestamp
      ObservationTime: Timestamp
      SourceSystemProvenance: string }

/// AB-D-002 Input
type EvaluateObservationCmd =
    { ObservationId: DemandObservationId
      EvaluationTime: Timestamp }

type ObservationCmd =
    | Receive of ReceiveObservationCmd
    | Evaluate of EvaluateObservationCmd

    static member GetId(cmd: ObservationCmd) =
        match cmd with
        | Receive c -> c.ObservationId
        | Evaluate c -> c.ObservationId

type ObservationEvent =
    | ObservationReceived of DemandObservation
    | ObservationAccepted of DemandObservationId * EvaluationTime: Timestamp * TraceId: string
    | ObservationQuarantined of DemandObservationId * EvaluationTime: Timestamp * TraceId: string
    | ObservationRejected of DemandObservationId * EvaluationTime: Timestamp * TraceId: string

/// Pure evolution (Layer E: Catamorphism). No validation.
let evolve: Medhavi.Foundation.Contracts.Evolve<DemandObservation, ObservationEvent> =
    fun (state: DemandObservation option) (event: ObservationEvent) ->
        match event with
        | ObservationReceived obs -> Some obs
        | ObservationAccepted(obsId, _, traceId) ->
            state
            |> Option.map(fun o ->
                if o.ObservationId = obsId then
                    { o with
                        LifecycleState = Accepted
                        DecisionTraceability = Some traceId }
                else
                    o)
        | ObservationQuarantined(obsId, _, traceId) ->
            state
            |> Option.map(fun o ->
                if o.ObservationId = obsId then
                    { o with
                        LifecycleState = Quarantined
                        DecisionTraceability = Some traceId }
                else
                    o)
        | ObservationRejected(obsId, _, traceId) ->
            state
            |> Option.map(fun o ->
                if o.ObservationId = obsId then
                    { o with
                        LifecycleState = Rejected
                        DecisionTraceability = Some traceId }
                else
                    o)

let replay (events: ObservationEvent seq) : DemandObservation option = Seq.fold evolve None events
