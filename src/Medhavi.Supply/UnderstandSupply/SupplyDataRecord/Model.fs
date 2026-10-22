module Medhavi.Supply.UnderstandSupply.SupplyDataRecord.Model

open Medhavi.SemanticModel
open Medhavi.Supply

type RecordLifecycleState =
    | Received
    | Accepted
    | Quarantined
    | Rejected

/// SE-S-010 Supply Data Record
type SupplyDataRecord =
    { RecordId: SupplyDataRecordId
      Item: ItemId
      Location: LocationId
      SupplyType: SupplyType
      Quantity: Quantity
      UnitOfMeasure: UnitOfMeasureId
      BatchIdentifier: string option
      OrderIdentifier: string option
      CommitmentIdentifier: string option
      SupplierIdentifier: SupplierId option
      ExpectedDeliveryDate: Timestamp option
      BusinessTime: Timestamp
      ObservationTime: Timestamp
      SourceSystemProvenance: string
      LifecycleState: RecordLifecycleState
      DecisionTraceability: string option }

/// AB-S-001 Input
type ReceiveRecordCmd =
    { RecordId: SupplyDataRecordId
      Item: ItemId
      Location: LocationId
      SupplyType: SupplyType
      Quantity: Quantity
      UnitOfMeasure: UnitOfMeasureId
      BatchIdentifier: string option
      OrderIdentifier: string option
      CommitmentIdentifier: string option
      SupplierIdentifier: SupplierId option
      ExpectedDeliveryDate: Timestamp option
      BusinessTime: Timestamp
      ObservationTime: Timestamp
      SourceSystemProvenance: string }

/// AB-S-002 Input
type EvaluateRecordCmd =
    { RecordId: SupplyDataRecordId
      EvaluationTime: Timestamp }

type RecordCmd =
    | Receive of ReceiveRecordCmd
    | Evaluate of EvaluateRecordCmd

    static member GetId(cmd: RecordCmd) =
        match cmd with
        | Receive c -> c.RecordId
        | Evaluate c -> c.RecordId

type RecordEvent =
    | RecordReceived of SupplyDataRecord
    | RecordAccepted of SupplyDataRecordId * EvaluationTime: Timestamp * TraceId: string
    | RecordQuarantined of SupplyDataRecordId * EvaluationTime: Timestamp * TraceId: string
    | RecordRejected of SupplyDataRecordId * EvaluationTime: Timestamp * TraceId: string

/// Pure evolution (catamorphism). No validation.
let evolve: Medhavi.Foundation.Contracts.Evolve<SupplyDataRecord, RecordEvent> =
    fun (state: SupplyDataRecord option) (event: RecordEvent) ->
        match event with
        | RecordReceived recrd -> Some recrd
        | RecordAccepted(recId, _, traceId) ->
            state
            |> Option.map(fun r ->
                if r.RecordId = recId then
                    { r with
                        LifecycleState = Accepted
                        DecisionTraceability = Some traceId }
                else
                    r)
        | RecordQuarantined(recId, _, traceId) ->
            state
            |> Option.map(fun r ->
                if r.RecordId = recId then
                    { r with
                        LifecycleState = Quarantined
                        DecisionTraceability = Some traceId }
                else
                    r)
        | RecordRejected(recId, _, traceId) ->
            state
            |> Option.map(fun r ->
                if r.RecordId = recId then
                    { r with
                        LifecycleState = Rejected
                        DecisionTraceability = Some traceId }
                else
                    r)

let replay (events: RecordEvent seq) : SupplyDataRecord option = Seq.fold evolve None events
