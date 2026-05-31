namespace Medhavi.Contracts

open System
open Medhavi.Contracts.Integration

type ResourceCalendarPayload =
    { ResourceId: string
      StartUtc: DateTimeOffset
      EndUtc: DateTimeOffset
      CapacityFactor: float // 0.0 (downtime) to 1.0 (available)
      Reason: string option }

type WorkOrderCompletedPayload =
    { WorkOrderId: string
      RoutingId: string
      QuantityCompleted: decimal
      CompletedAtUtc: DateTimeOffset }

type MaterialReceivedPayload =
    { ReceiptId: string
      ProductId: string
      StockingPointId: string
      QuantityReceived: decimal
      ReceivedAtUtc: DateTimeOffset }

type ResourceDowntimePayload =
    { ResourceId: string
      StartUtc: DateTimeOffset
      EndUtc: DateTimeOffset
      Reason: string }

type TransportDelayPayload =
    { TransportLegId: string
      EstimatedDelayMinutes: float
      NewArrivalUtc: DateTimeOffset
      Reason: string }
