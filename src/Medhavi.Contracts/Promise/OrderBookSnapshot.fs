namespace Medhavi.Contracts.Promise

open System
open System.Threading.Tasks
open Medhavi.Contracts

/// SE-P-010 Order Line entity within Order Book Snapshot
type OrderLineDto =
    { OrderLineId: string
      Item: string
      RequestedQuantity: decimal
      UnitOfMeasure: string
      RequestedDeliveryDate: DateTimeOffset
      EarliestAcceptableDate: DateTimeOffset option
      LatestAcceptableDate: DateTimeOffset option
      DestinationLocation: string
      ExternalLineReference: string
      ValidationStatus: string
      RejectionReason: string option }

/// SE-P-001 Order Book Snapshot Data Transfer Object
type OrderBookSnapshotDto =
    { SnapshotId: string
      PlanningScopeId: string
      CustomerOrderIdentifier: string
      CustomerId: string
      OrderChannel: string
      OrderPriorityClass: string
      ObservationTimestamp: DateTimeOffset
      Lines: OrderLineDto list
      LifecycleState: string
      DecisionTraceability: string option }

/// External payload line item for order ingestion
type IngestOrderLineReq =
    { OrderLineId: string
      Item: string
      RequestedQuantity: decimal
      UnitOfMeasure: string
      RequestedDeliveryDate: DateTimeOffset
      EarliestAcceptableDate: DateTimeOffset option
      LatestAcceptableDate: DateTimeOffset option
      DestinationLocation: string
      ExternalLineReference: string }

/// External request to ingest an incoming customer order payload
type IngestOrderRequest =
    { SnapshotId: string
      PlanningScopeId: string
      CustomerOrderIdentifier: string
      CustomerId: string
      OrderChannel: string
      OrderPriorityClass: string
      ObservationTimestamp: DateTimeOffset
      Lines: IngestOrderLineReq list }

/// External request to validate an ingested order line
type ValidateOrderLineReq =
    { SnapshotId: string
      OrderLineId: string
      EvaluationTime: DateTimeOffset }

/// External request to cancel an unfulfilled order line
type CancelOrderLineReq =
    { SnapshotId: string
      OrderLineId: string
      CancellationReason: string
      CancellationTime: DateTimeOffset }

// ---------- Business Notifications ----------

/// BN-P-001: Order Ingestion & Validation Notification (Dispatched post-persistence)
type OrderReceivedAndValidatedNotification =
    { SnapshotId: string
      PlanningScopeId: string
      CustomerOrderIdentifier: string
      CustomerId: string
      OrderLineCount: int
      ValidatedLineCount: int
      ValidationPassed: bool
      NotificationTime: DateTimeOffset }

/// Public API for Order Book Management (CA-P-001)
type OrderBookApi =
    { Ingest: IngestOrderRequest -> Task<Result<OrderBookSnapshotDto, ApiError>>
      Validate: ValidateOrderLineReq -> Task<Result<OrderBookSnapshotDto, ApiError>>
      Cancel: CancelOrderLineReq -> Task<Result<OrderBookSnapshotDto, ApiError>> }

/// Query service alias for Order Book Snapshots
type OrderBookQueries = QueryService<OrderBookSnapshotDto, string>

