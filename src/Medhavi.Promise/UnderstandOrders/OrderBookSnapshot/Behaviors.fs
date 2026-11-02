/// CA-P-001 Ingest and Understand Orders Aggregate Behaviors
module Medhavi.Promise.UnderstandOrders.OrderBookSnapshot.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Rules
open Decisions
open Policies

let buildTrace policy decisionId decisionOutcome state events summary =
    let policyId = policy |> Option.map (fun _ -> Policies.orderValidationPolicy.Id)
    let policyVersion = policy |> Option.map (fun _ -> 1)

    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        Capabilities.captureAndValidateOrderBook.Id
        decisionOutcome
        policyId
        policyVersion
        [ SemanticObjects.orderBookSnapshot.Id ]
        (Some summary)

/// AB-P-001: Ingest Order Request
let ingest: Decide<OrderBookSnapshot, IngestOrderRequestCmd, OrderBookEvent> =
    fun (cmd: IngestOrderRequestCmd) (state: OrderBookSnapshot option) ->
        result {
            match state with
            | Some _ ->
                return! Error(DomainError.conflict ($"OrderBookSnapshot already exists: {OrderBookSnapshotId.value cmd.SnapshotId}", Capabilities.captureAndValidateOrderBook.Id))
            | None ->
                let snapshot: OrderBookSnapshot =
                    { SnapshotId = cmd.SnapshotId
                      PlanningScopeId = cmd.PlanningScopeId
                      CustomerOrderIdentifier = cmd.CustomerOrderIdentifier
                      CustomerId = cmd.CustomerId
                      OrderChannel = cmd.OrderChannel
                      OrderPriorityClass = cmd.OrderPriorityClass
                      ObservationTimestamp = cmd.ObservationTimestamp
                      Lines = cmd.Lines
                      LifecycleState = Received
                      DecisionTraceability = None }

                let events = [ OrderBookIngested snapshot ]
                let decision: DecisionOutcome<OrderIngestionOutcome> = { Outcome = IngestionAccepted; Evaluations = [] }

                return buildTrace None Capabilities.captureAndValidateOrderBook.Id decision None events "Order Book Snapshot ingested"
        }

/// AB-P-002: Validate Order Terms
let validate
    (policy: OrderValidationPolicy)
    (itemExists: bool)
    (locationExists: bool)
    (customerProfile: CustomerProfileFact option)
    : Decide<OrderBookSnapshot, ValidateOrderTermsCmd, OrderBookEvent> =
    fun (cmd: ValidateOrderTermsCmd) (state: OrderBookSnapshot option) ->
        result {
            let targetLine =
                state
                |> Option.bind (fun s -> s.Lines |> List.tryFind (fun l -> l.OrderLineId = cmd.OrderLineId))

            let input: ValidateOrderLineInput =
                { Cmd = cmd
                  CurrentSnapshot = state
                  TargetLine = targetLine
                  ItemExists = itemExists
                  DestinationLocationExists = locationExists
                  CustomerProfile = customerProfile
                  Policy = policy }

            let! decision = evaluateOrderLine validationRules input
            let traceId = Guid.NewGuid().ToString()

            match decision.Outcome with
            | Validated ->
                let events = [ OrderValidationPassed(cmd.SnapshotId, cmd.OrderLineId, cmd.EvaluationTime, traceId) ]
                return buildTrace (Some policy) traceId decision state events "Order line validation passed"

            | ValidationFailed reasons ->
                let events = [ OrderValidationFailed(cmd.SnapshotId, cmd.OrderLineId, cmd.EvaluationTime, reasons, traceId) ]
                return buildTrace (Some policy) traceId decision state events (String.concat "; " reasons)
        }

/// AB-P-003: Cancel Order Line
let cancel
    (policy: OrderLifecyclePolicy)
    : Decide<OrderBookSnapshot, CancelOrderLineCmd, OrderBookEvent> =
    fun (cmd: CancelOrderLineCmd) (state: OrderBookSnapshot option) ->
        result {
            let targetLine =
                state
                |> Option.bind (fun s -> s.Lines |> List.tryFind (fun l -> l.OrderLineId = cmd.OrderLineId))

            let input: CancelOrderLineInput =
                { Cmd = cmd
                  CurrentSnapshot = state
                  TargetLine = targetLine
                  Policy = policy }

            let! decision = evaluateOrderCancellation cancellationRules input
            let traceId = Guid.NewGuid().ToString()

            match decision.Outcome with
            | CancellationAuthorized ->
                let events = [ OrderLineCancelled(cmd.SnapshotId, cmd.OrderLineId, cmd.CancellationReason, cmd.CancellationTime) ]
                return buildTrace None traceId decision state events $"Order line cancellation authorized: {cmd.CancellationReason}"

            | CancellationRejected reason ->
                return! Error(DomainError.rule (reason, rule = Rules.orderCancellationTimeFenceEligibility.Id))
        }
