/// CA-P-008 Manage Backorders — Behaviors
/// Traces to: AB-P-021 Queue Backorder, AB-P-022 Resolve Backorder
module Medhavi.Promise.ManageBackorders.BackorderRecord.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Rules
open Decisions
open Policies

let buildTrace policy decisionId decisionOutcome state events summary =
    let policyId = policy |> Option.map (fun (p: BacklogGovernancePolicy) -> p.PolicyId)
    let policyVersion = policy |> Option.map (fun (p: BacklogGovernancePolicy) -> p.Version)

    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        Capabilities.rePromiseBacklogAndManageBackorders.Id
        decisionOutcome
        policyId
        policyVersion
        [ SemanticObjects.backorderRecord.Id ]
        (Some summary)

/// AB-P-021: Queue Backorder
let enqueue
    (policy: BacklogGovernancePolicy)
    (customerExists: bool)
    (itemExists: bool)
    (locationExists: bool)
    : Decide<BackorderRecord, EnqueueBackorderCmd, BackorderEvent> =
    fun (cmd: EnqueueBackorderCmd) (state: BackorderRecord option) ->
        result {
            match state with
            | Some _ ->
                return! Error(DomainError.conflict ($"Backorder already exists: {BackorderId.value cmd.BackorderId}", Capabilities.rePromiseBacklogAndManageBackorders.Id))
            | None ->
                let input: EnqueueBackorderInput =
                    { Cmd = cmd
                      CustomerExists = customerExists
                      ItemExists = itemExists
                      LocationExists = locationExists
                      Policy = policy }

                let! decision = evaluateEnqueue enqueueRules input
                let traceId = Guid.NewGuid().ToString()

                match decision.Outcome with
                | EnqueueAuthorized ->
                    let record: BackorderRecord =
                        { BackorderId = cmd.BackorderId
                          OrderLineId = cmd.OrderLineId
                          CustomerId = cmd.CustomerId
                          Item = cmd.Item
                          Location = cmd.Location
                          BackorderedQuantity = cmd.Quantity
                          EnqueuedTime = cmd.EnqueuedTime
                          EstimatedReplenishmentDate = None
                          LifecycleState = Queued
                          Attempts = []
                          DecisionTraceability = Some traceId }

                    let events = [ BackorderQueued record ]
                    return buildTrace (Some policy) traceId decision state events $"Backorder enqueued for line {OrderLineId.value cmd.OrderLineId}"

                | EnqueueRejected reason ->
                    return! Error(DomainError.rule (reason, rule = Rules.backorderQueueIdentity.Id))
        }

/// AB-P-022: Resolve Backorder
let resolve
    (policy: BacklogGovernancePolicy)
    : Decide<BackorderRecord, ResolveBackorderCmd, BackorderEvent> =
    fun (cmd: ResolveBackorderCmd) (state: BackorderRecord option) ->
        result {
            match state with
            | None ->
                return! Error(DomainError.notFound (SemanticObjects.backorderRecord.Id, BackorderId.value cmd.BackorderId))
            | Some record ->
                let input: ResolveBackorderInput =
                    { Cmd = cmd
                      CurrentRecord = state
                      Policy = policy }

                let! decision = evaluateResolve resolveRules input
                let traceId = Guid.NewGuid().ToString()

                match decision.Outcome with
                | ResolveAuthorized ->
                    let events =
                        [ BackorderResolved(
                            cmd.BackorderId,
                            record.OrderLineId,
                            cmd.MatchedSupplyIdentifier,
                            cmd.ResolvedDeliveryDate,
                            cmd.ResolutionTime,
                            traceId) ]
                    return buildTrace (Some policy) traceId decision state events $"Backorder resolved with supply {cmd.MatchedSupplyIdentifier}"

                | ResolveRejected reason ->
                    return! Error(DomainError.rule (reason, rule = Rules.backorderQueueFairShareInvariant.Id))
        }

