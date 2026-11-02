namespace Medhavi.Contracts.Promise

open System
open System.Threading.Tasks
open Medhavi.Contracts

/// Sourcing leg specification within a commitment revision
type SourcingLegDto =
    { LegId: string
      OriginLocation: string
      Item: string
      SourcedQuantity: decimal
      FulfillmentMode: string
      EstimatedShipDate: DateTimeOffset
      EstimatedDeliveryDate: DateTimeOffset
      FreightCost: decimal
      CarbonEmissionsKg: decimal }

/// Immutable revision snapshot within an Order Commitment ledger
type CommitmentRevisionDto =
    { RevisionIndex: int
      CommittedDeliveryDate: DateTimeOffset
      PromisedQuantity: decimal
      ProjectedLandedCost: decimal
      GrossMarginPct: decimal
      DecisionConfidence: decimal
      SourcingLegs: SourcingLegDto list
      RevisionReason: string
      RevisionTimestamp: DateTimeOffset }

/// SE-P-010 Order Commitment Data Transfer Object
type OrderCommitmentDto =
    { CommitmentId: string
      CustomerOrderIdentifier: string
      OrderLineId: string
      CustomerId: string
      Item: string
      DestinationLocation: string
      CurrentRevisionIndex: int
      Revisions: CommitmentRevisionDto list
      LifecycleState: string
      DecisionTraceability: string option }

/// Payload sourcing leg input
type SourcingLegReq =
    { LegId: string
      OriginLocation: string
      Item: string
      SourcedQuantity: decimal
      FulfillmentMode: string
      EstimatedShipDate: DateTimeOffset
      EstimatedDeliveryDate: DateTimeOffset
      FreightCost: decimal
      CarbonEmissionsKg: decimal }

/// External request to establish the initial binding commitment (Revision 0 baseline)
type EstablishCommitmentReq =
    { CommitmentId: string
      CustomerOrderIdentifier: string
      OrderLineId: string
      CustomerId: string
      Item: string
      DestinationLocation: string
      CommittedDeliveryDate: DateTimeOffset
      PromisedQuantity: decimal
      ProjectedLandedCost: decimal
      GrossMarginPct: decimal
      DecisionConfidence: decimal
      SourcingLegs: SourcingLegReq list
      EstablishmentTime: DateTimeOffset }

/// External request to revise a commitment schedule
type ReviseCommitmentReq =
    { CommitmentId: string
      NewDeliveryDate: DateTimeOffset
      PromisedQuantity: decimal
      ProjectedLandedCost: decimal
      GrossMarginPct: decimal
      DecisionConfidence: decimal
      SourcingLegs: SourcingLegReq list
      RevisionReason: string
      RevisionTime: DateTimeOffset }

/// External request to confirm delivery receipt from carrier POD
type ConfirmFulfillmentReq =
    { CommitmentId: string
      ProofOfDeliveryTime: DateTimeOffset
      DeliveredQuantity: decimal }

/// External request to cancel an order commitment
type CancelCommitmentReq =
    { CommitmentId: string
      CancellationReason: string
      CancellationTime: DateTimeOffset }

// ---------- Business Notifications ----------

/// BN-P-002: Order Promise Firmly Committed Notification
type OrderPromiseFirmlyCommittedNotification =
    { CommitmentId: string
      CustomerOrderIdentifier: string
      OrderLineId: string
      CustomerId: string
      Item: string
      CommittedDeliveryDate: DateTimeOffset
      PromisedQuantity: decimal
      DecisionConfidence: decimal
      NotificationTime: DateTimeOffset }

/// BN-P-003: Order Promise Revised Notification
type OrderPromiseRevisedNotification =
    { CommitmentId: string
      OrderLineId: string
      PreviousDeliveryDate: DateTimeOffset
      NewDeliveryDate: DateTimeOffset
      RevisionIndex: int
      RevisionReason: string
      NotificationTime: DateTimeOffset }

/// BN-P-004: Order De-Commitment & Cancellation Alert Notification
type OrderDeCommitmentNotification =
    { CommitmentId: string
      OrderLineId: string
      CancellationReason: string
      NotificationTime: DateTimeOffset }

/// Public API for Order Commitments (CA-P-006)
type CommitmentApi =
    { Establish: EstablishCommitmentReq -> Task<Result<OrderCommitmentDto, ApiError>>
      Revise: ReviseCommitmentReq -> Task<Result<OrderCommitmentDto, ApiError>>
      ConfirmFulfillment: ConfirmFulfillmentReq -> Task<Result<OrderCommitmentDto, ApiError>>
      Cancel: CancelCommitmentReq -> Task<Result<OrderCommitmentDto, ApiError>> }

/// Query service alias for Order Commitments
type CommitmentQueries = QueryService<OrderCommitmentDto, string>

