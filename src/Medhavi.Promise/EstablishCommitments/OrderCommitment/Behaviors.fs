/// CA-P-006 Establish Commitments Behaviors
/// Traces to: AB-P-007 Commit Order Promise, AB-P-008 Revise Promise Schedule, AB-P-009 Confirm Fulfillment, AB-P-010 Cancel Commitment
module Medhavi.Promise.EstablishCommitments.OrderCommitment.Behaviors

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

let buildTrace policy decisionId (decisionOutcome: DecisionOutcome<'T>) state events summary =
    let policyId = policy |> Option.map (fun _ -> Policies.commitmentGovernancePolicy.Id)
    let policyVersion = policy |> Option.map (fun _ -> 1)

    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        Capabilities.explainOrderPromisesAndProvideProvenance.Id
        decisionOutcome
        policyId
        policyVersion
        [ SemanticObjects.orderCommitment.Id ]
        (Some summary)

/// AB-P-007: Commit Order Promise (Revision 0 baseline lock)
let establish
    (policy: CommitmentGovernancePolicy)
    (customerExists: bool)
    (itemExists: bool)
    (destinationExists: bool)
    : Decide<OrderCommitment, EstablishCommitmentCmd, OrderCommitmentEvent> =
    fun (cmd: EstablishCommitmentCmd) (state: OrderCommitment option) ->
        result {
            match state with
            | Some _ ->
                return! Error(DomainError.conflict ($"OrderCommitment already exists: {OrderCommitmentId.value cmd.CommitmentId}", Capabilities.explainOrderPromisesAndProvideProvenance.Id))
            | None ->
                let input: EstablishCommitmentInput =
                    { Cmd = cmd
                      CustomerExists = customerExists
                      ItemExists = itemExists
                      DestinationExists = destinationExists
                      Policy = policy }

                let! decision = evaluateEstablishCommitment establishRules input
                let traceId = Guid.NewGuid().ToString()

                match decision.Outcome with
                | CommitmentAuthorized ->
                    let rev0: CommitmentRevision =
                        { RevisionIndex = 0
                          CommittedDeliveryDate = cmd.CommittedDeliveryDate
                          PromisedQuantity = cmd.PromisedQuantity
                          ProjectedLandedCost = cmd.ProjectedLandedCost
                          GrossMarginPct = cmd.GrossMarginPct
                          DecisionConfidence = cmd.DecisionConfidence
                          SourcingLegs = cmd.SourcingLegs
                          RevisionReason = "Initial Binding Commitment (Revision 0)"
                          RevisionTimestamp = cmd.EstablishmentTime }

                    let commitment: OrderCommitment =
                        { CommitmentId = cmd.CommitmentId
                          CustomerOrderIdentifier = cmd.CustomerOrderIdentifier
                          OrderLineId = cmd.OrderLineId
                          CustomerId = cmd.CustomerId
                          Item = cmd.Item
                          DestinationLocation = cmd.DestinationLocation
                          CurrentRevisionIndex = 0
                          Revisions = [ rev0 ]
                          LifecycleState = Committed
                          DecisionTraceability = Some traceId }

                    let events = [ CommitmentEstablished commitment ]
                    return buildTrace (Some policy) traceId decision state events "Initial order commitment locked at Revision 0"

                | CommitmentRejected reasons ->
                    return! Error(DomainError.validation (String.concat "; " reasons))
        }

/// AB-P-008: Revise Promise Schedule (Appends Revision N+1)
let revise
    (policy: CommitmentGovernancePolicy)
    : Decide<OrderCommitment, ReviseCommitmentCmd, OrderCommitmentEvent> =
    fun (cmd: ReviseCommitmentCmd) (state: OrderCommitment option) ->
        result {
            match state with
            | None ->
                return! Error(DomainError.notFound (SemanticObjects.orderCommitment.Id, OrderCommitmentId.value cmd.CommitmentId))
            | Some commitment ->
                let input: ReviseCommitmentInput =
                    { Cmd = cmd
                      CurrentCommitment = state
                      Policy = policy }

                let! decision = evaluateReviseCommitment reviseRules input
                let traceId = Guid.NewGuid().ToString()

                match decision.Outcome with
                | RevisionAuthorized ->
                    let newRevIndex = commitment.CurrentRevisionIndex + 1
                    let newRev: CommitmentRevision =
                        { RevisionIndex = newRevIndex
                          CommittedDeliveryDate = cmd.NewDeliveryDate
                          PromisedQuantity = cmd.PromisedQuantity
                          ProjectedLandedCost = cmd.ProjectedLandedCost
                          GrossMarginPct = cmd.GrossMarginPct
                          DecisionConfidence = cmd.DecisionConfidence
                          SourcingLegs = cmd.SourcingLegs
                          RevisionReason = cmd.RevisionReason
                          RevisionTimestamp = cmd.RevisionTime }

                    let updated: OrderCommitment =
                        { commitment with
                            CurrentRevisionIndex = newRevIndex
                            Revisions = newRev :: commitment.Revisions
                            LifecycleState = Revised
                            DecisionTraceability = Some traceId }

                    let events = [ CommitmentRevised updated ]
                    return buildTrace (Some policy) traceId decision state events $"Commitment revised to Revision {newRevIndex}: {cmd.RevisionReason}"

                | RevisionRejected reason ->
                    return! Error(DomainError.rule (reason, rule = Rules.firstPromisePermanenceInvariant.Id))
        }

/// AB-P-009: Confirm Fulfillment (Delivery POD confirmation)
let confirmFulfillment
    : Decide<OrderCommitment, ConfirmFulfillmentCmd, OrderCommitmentEvent> =
    fun (cmd: ConfirmFulfillmentCmd) (state: OrderCommitment option) ->
        result {
            match state with
            | None ->
                return! Error(DomainError.notFound (SemanticObjects.orderCommitment.Id, OrderCommitmentId.value cmd.CommitmentId))
            | Some _ ->
                let traceId = Guid.NewGuid().ToString()
                let events = [ CommitmentFulfilled(cmd.CommitmentId, cmd.ProofOfDeliveryTime, traceId) ]
                let decision: DecisionOutcome<string> = { Outcome = "Fulfilled"; Evaluations = [] }
                return buildTrace None traceId decision state events "Commitment delivery confirmed via proof of delivery"
        }

/// AB-P-010: Cancel Commitment (De-commitment)
let cancel
    (policy: CommitmentGovernancePolicy)
    : Decide<OrderCommitment, CancelCommitmentCmd, OrderCommitmentEvent> =
    fun (cmd: CancelCommitmentCmd) (state: OrderCommitment option) ->
        result {
            match state with
            | None ->
                return! Error(DomainError.notFound (SemanticObjects.orderCommitment.Id, OrderCommitmentId.value cmd.CommitmentId))
            | Some _ ->
                let input: CancelCommitmentInput =
                    { Cmd = cmd
                      CurrentCommitment = state
                      Policy = policy }

                let! decision = evaluateCancelCommitment cancelRules input
                let traceId = Guid.NewGuid().ToString()

                match decision.Outcome with
                | CancellationAuthorized ->
                    let events = [ CommitmentCancelled(cmd.CommitmentId, cmd.CancellationReason, cmd.CancellationTime, traceId) ]
                    return buildTrace (Some policy) traceId decision state events $"Commitment cancelled: {cmd.CancellationReason}"

                | CancellationRejected reason ->
                    return! Error(DomainError.rule (reason, rule = Rules.orderCancellationTimeFenceEligibility.Id))
        }

