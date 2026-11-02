/// CA-P-006 Establish Commitments — Order Commitment Model
/// Traces to: SE-P-010, CR-P-019, CR-P-020, CR-P-021, CR-P-022
module Medhavi.Promise.EstablishCommitments.OrderCommitment.Model

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Medhavi.Promise.EvaluateATP.Model

type CommitmentLifecycleState =
    | Draft
    | Committed
    | Revised
    | Fulfilled
    | Cancelled

type CommitmentRevision =
    { RevisionIndex: int
      CommittedDeliveryDate: Timestamp
      PromisedQuantity: Quantity
      ProjectedLandedCost: decimal
      GrossMarginPct: decimal
      DecisionConfidence: decimal
      SourcingLegs: SourcingLeg list
      RevisionReason: string
      RevisionTimestamp: Timestamp }

type OrderCommitment =
    { CommitmentId: OrderCommitmentId
      CustomerOrderIdentifier: string
      OrderLineId: OrderLineId
      CustomerId: CustomerId
      Item: ItemId
      DestinationLocation: LocationId
      CurrentRevisionIndex: int
      Revisions: CommitmentRevision list
      LifecycleState: CommitmentLifecycleState
      DecisionTraceability: string option }
    member this.Revision0 =
        this.Revisions |> List.tryFind (fun r -> r.RevisionIndex = 0)
    member this.LatestRevision =
        this.Revisions |> List.maxBy (fun r -> r.RevisionIndex)

/// AB-P-007 Commit Order Promise Command
type EstablishCommitmentCmd =
    { CommitmentId: OrderCommitmentId
      CustomerOrderIdentifier: string
      OrderLineId: OrderLineId
      CustomerId: CustomerId
      Item: ItemId
      DestinationLocation: LocationId
      CommittedDeliveryDate: Timestamp
      PromisedQuantity: Quantity
      ProjectedLandedCost: decimal
      GrossMarginPct: decimal
      DecisionConfidence: decimal
      SourcingLegs: SourcingLeg list
      EstablishmentTime: Timestamp }

/// AB-P-008 Revise Promise Schedule Command
type ReviseCommitmentCmd =
    { CommitmentId: OrderCommitmentId
      NewDeliveryDate: Timestamp
      PromisedQuantity: Quantity
      ProjectedLandedCost: decimal
      GrossMarginPct: decimal
      DecisionConfidence: decimal
      SourcingLegs: SourcingLeg list
      RevisionReason: string
      RevisionTime: Timestamp }

/// AB-P-009 Confirm Fulfillment Command
type ConfirmFulfillmentCmd =
    { CommitmentId: OrderCommitmentId
      ProofOfDeliveryTime: Timestamp
      DeliveredQuantity: Quantity }

/// AB-P-010 Cancel Commitment Command
type CancelCommitmentCmd =
    { CommitmentId: OrderCommitmentId
      CancellationReason: string
      CancellationTime: Timestamp }

type OrderCommitmentCmd =
    | Establish of EstablishCommitmentCmd
    | Revise of ReviseCommitmentCmd
    | ConfirmFulfillment of ConfirmFulfillmentCmd
    | Cancel of CancelCommitmentCmd

    static member GetId(cmd: OrderCommitmentCmd) =
        match cmd with
        | Establish c -> c.CommitmentId
        | Revise c -> c.CommitmentId
        | ConfirmFulfillment c -> c.CommitmentId
        | Cancel c -> c.CommitmentId

/// Domain Events emitted by Order Commitment
type OrderCommitmentEvent =
    | CommitmentEstablished of OrderCommitment
    | CommitmentRevised of OrderCommitment
    | CommitmentFulfilled of CommitmentId: OrderCommitmentId * PodTime: Timestamp * TraceId: string
    | CommitmentCancelled of CommitmentId: OrderCommitmentId * Reason: string * CancelTime: Timestamp * TraceId: string

/// Pure state evolution (catamorphism)
let evolve: Evolve<OrderCommitment, OrderCommitmentEvent> =
    fun (state: OrderCommitment option) (event: OrderCommitmentEvent) ->
        match event with
        | CommitmentEstablished commitment -> Some commitment

        | CommitmentRevised updatedCommitment -> Some updatedCommitment

        | CommitmentFulfilled(id, _, traceId) ->
            state
            |> Option.map (fun c ->
                if c.CommitmentId = id then
                    { c with
                        LifecycleState = Fulfilled
                        DecisionTraceability = Some traceId }
                else c)

        | CommitmentCancelled(id, _, _, traceId) ->
            state
            |> Option.map (fun c ->
                if c.CommitmentId = id then
                    { c with
                        LifecycleState = Cancelled
                        DecisionTraceability = Some traceId }
                else c)

