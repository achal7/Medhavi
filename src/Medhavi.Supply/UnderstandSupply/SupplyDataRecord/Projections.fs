module Medhavi.Supply.UnderstandSupply.SupplyDataRecord.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model

/// Map aggregate state to DTO.
let mapToDto (recrd: SupplyDataRecord) : SupplyDataRecordDto =
    { RecordId = SupplyDataRecordId.value recrd.RecordId
      Item = ItemId.value recrd.Item
      Location = LocationId.value recrd.Location
      SupplyType = sprintf "%A" recrd.SupplyType
      Quantity = Quantity.value recrd.Quantity
      UnitOfMeasure = UnitOfMeasureId.value recrd.UnitOfMeasure
      BatchIdentifier = recrd.BatchIdentifier
      OrderIdentifier = recrd.OrderIdentifier
      CommitmentIdentifier = recrd.CommitmentIdentifier
      SupplierIdentifier = recrd.SupplierIdentifier |> Option.map SupplierId.value
      ExpectedDeliveryDate = recrd.ExpectedDeliveryDate |> Option.map Timestamp.value
      BusinessTime = Timestamp.value recrd.BusinessTime
      ObservationTime = Timestamp.value recrd.ObservationTime
      SourceSystemProvenance = recrd.SourceSystemProvenance
      LifecycleState =
        match recrd.LifecycleState with
        | Received -> "Received"
        | Accepted -> "Accepted"
        | Quarantined -> "Quarantined"
        | Rejected -> "Rejected"
      DecisionTraceability = recrd.DecisionTraceability }

/// Projection state: Map of SupplyDataRecordId to DTO.
type State = Map<SupplyDataRecordId, SupplyDataRecordDto>

let initial: State = Map.empty

/// Pure projection fold (catamorphism).
let apply (state: State) (event: RecordEvent) : State =
    match event with
    | RecordReceived recrd -> state |> Map.add recrd.RecordId (mapToDto recrd)
    | RecordAccepted(recId, _, traceId) ->
        state
        |> Map.change
            recId
            (Option.map(fun dto ->
                { dto with
                    LifecycleState = "Accepted"
                    DecisionTraceability = Some traceId }))
    | RecordQuarantined(recId, _, traceId) ->
        state
        |> Map.change
            recId
            (Option.map(fun dto ->
                { dto with
                    LifecycleState = "Quarantined"
                    DecisionTraceability = Some traceId }))
    | RecordRejected(recId, _, traceId) ->
        state
        |> Map.change
            recId
            (Option.map(fun dto ->
                { dto with
                    LifecycleState = "Rejected"
                    DecisionTraceability = Some traceId }))

/// Seed projection from existing aggregates.
let seedFromAggregates (aggregates: SupplyDataRecord list) : State =
    aggregates
    |> List.fold
        (fun state agg ->
            let dto = mapToDto agg
            Map.add agg.RecordId dto state)
        initial
