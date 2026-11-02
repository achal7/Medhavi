/// CA-P-008 Manage Backorders — Projections
module Medhavi.Promise.ManageBackorders.BackorderRecord.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let mapToDto (r: BackorderRecord) : BackorderRecordDto =
    { BackorderId = BackorderId.value r.BackorderId
      OrderLineId = OrderLineId.value r.OrderLineId
      CustomerId = CustomerId.value r.CustomerId
      Item = ItemId.value r.Item
      Location = LocationId.value r.Location
      BackorderedQuantity = Quantity.value r.BackorderedQuantity
      EnqueuedTime = Timestamp.value r.EnqueuedTime
      EstimatedReplenishmentDate = r.EstimatedReplenishmentDate |> Option.map Timestamp.value
      LifecycleState =
        match r.LifecycleState with
        | Queued -> "Queued"
        | UnderReEvaluation -> "UnderReEvaluation"
        | RePromisedResolved _ -> "RePromisedResolved"
        | AbandonedCancelled _ -> "AbandonedCancelled"
      DecisionTraceability = r.DecisionTraceability }

type State = Map<BackorderId, BackorderRecordDto>

let initial: State = Map.empty

let apply (state: State) (event: BackorderEvent) : State =
    match event with
    | BackorderQueued record ->
        state |> Map.add record.BackorderId (mapToDto record)

    | BackorderResolved(id, _, _, resolvedDate, _, traceId) ->
        state
        |> Map.change id (Option.map (fun dto ->
            { dto with
                LifecycleState = "RePromisedResolved"
                EstimatedReplenishmentDate = Some (Timestamp.value resolvedDate)
                DecisionTraceability = Some traceId }))

let seedFromAggregates (aggregates: BackorderRecord list) : State =
    aggregates
    |> List.fold (fun state r -> Map.add r.BackorderId (mapToDto r) state) initial

