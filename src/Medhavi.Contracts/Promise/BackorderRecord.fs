namespace Medhavi.Contracts.Promise

open System
open System.Threading.Tasks
open Medhavi.Contracts

/// SE-P-040 Backorder Record Data Transfer Object
type BackorderRecordDto =
    { BackorderId: string
      OrderLineId: string
      CustomerId: string
      Item: string
      Location: string
      BackorderedQuantity: decimal
      EnqueuedTime: DateTimeOffset
      EstimatedReplenishmentDate: DateTimeOffset option
      LifecycleState: string
      DecisionTraceability: string option }

/// External request to enqueue an unfulfilled order line into backorder recovery queue
type EnqueueBackorderReq =
    { BackorderId: string
      OrderLineId: string
      CustomerId: string
      Item: string
      Location: string
      Quantity: decimal
      EnqueuedTime: DateTimeOffset }

/// External request to resolve a backorder with newly arrived supply
type ResolveBackorderReq =
    { BackorderId: string
      MatchedSupplyIdentifier: string
      ResolvedDeliveryDate: DateTimeOffset
      ResolutionTime: DateTimeOffset }

// ---------- Business Notifications ----------

/// BN-P-015: Backlog Re-Promise Completed Notification
type BacklogRePromiseCompletedNotification =
    { BackorderId: string
      OrderLineId: string
      Item: string
      ResolvedDeliveryDate: DateTimeOffset
      NotificationTime: DateTimeOffset }

/// Public API for Backorder & Backlog Management (CA-P-008)
type BackorderApi =
    { Enqueue: EnqueueBackorderReq -> Task<Result<BackorderRecordDto, ApiError>>
      Resolve: ResolveBackorderReq -> Task<Result<BackorderRecordDto, ApiError>> }

/// Query service alias for Backorders
type BackorderQueries = QueryService<BackorderRecordDto, string>

