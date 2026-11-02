/// CA-P-008 Manage Backorders — Model
/// Traces to: SE-P-040, CR-P-025, CR-P-026
module Medhavi.Promise.ManageBackorders.BackorderRecord.Model

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers

type BackorderState =
    | Queued
    | UnderReEvaluation
    | RePromisedResolved of ResolvedDate: Timestamp * SupplyId: string
    | AbandonedCancelled of Reason: string

type RePromiseAttempt =
    { AttemptIndex: int
      EvaluationEpoch: Timestamp
      TriggeringSupplyRef: string
      EvaluatedQuantity: Quantity
      BestFeasibleDate: Timestamp option
      OutcomeRationale: string }

/// SE-P-040 Backorder Record Aggregate Root
type BackorderRecord =
    { BackorderId: BackorderId
      OrderLineId: OrderLineId
      CustomerId: CustomerId
      Item: ItemId
      Location: LocationId
      BackorderedQuantity: Quantity
      EnqueuedTime: Timestamp
      EstimatedReplenishmentDate: Timestamp option
      LifecycleState: BackorderState
      Attempts: RePromiseAttempt list
      DecisionTraceability: string option }

type EnqueueBackorderCmd =
    { BackorderId: BackorderId
      OrderLineId: OrderLineId
      CustomerId: CustomerId
      Item: ItemId
      Location: LocationId
      Quantity: Quantity
      EnqueuedTime: Timestamp }

type ResolveBackorderCmd =
    { BackorderId: BackorderId
      MatchedSupplyIdentifier: string
      ResolvedDeliveryDate: Timestamp
      ResolutionTime: Timestamp }

type BackorderCmd =
    | Enqueue of EnqueueBackorderCmd
    | Resolve of ResolveBackorderCmd

    static member GetId(cmd: BackorderCmd) =
        match cmd with
        | Enqueue c -> c.BackorderId
        | Resolve c -> c.BackorderId

type BackorderEvent =
    | BackorderQueued of Record: BackorderRecord
    | BackorderResolved of BackorderId: BackorderId * LineId: OrderLineId * SupplyId: string * ResolvedDate: Timestamp * ResolveTime: Timestamp * TraceId: string

let evolve: Evolve<BackorderRecord, BackorderEvent> =
    fun (state: BackorderRecord option) (event: BackorderEvent) ->
        match event with
        | BackorderQueued record ->
            Some record

        | BackorderResolved(_, _, supplyId, resolvedDate, _, traceId) ->
            state
            |> Option.map (fun r ->
                { r with
                    LifecycleState = RePromisedResolved(resolvedDate, supplyId)
                    EstimatedReplenishmentDate = Some resolvedDate
                    DecisionTraceability = Some traceId })

