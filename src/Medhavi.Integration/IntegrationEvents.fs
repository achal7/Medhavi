namespace Medhavi.Integration

open System
open Medhavi.Infrastructure
open Medhavi.Contracts
open Medhavi.Common.Serialization

/// Discriminated Union representing all Integration Events.
type IntegrationEvent =
    | MasterDataImported of MasterDataImportedPayload
    | DemandSignalsImported of DemandSignalsPayload
    | InventoryPositionsImported of InventoryPositionPayload list
    | SupplyOrdersImported of SupplyOrderStatusPayload list
    | ResourceCalendarsImported of ResourceCalendarPayload list
    | WorkOrdersCompleted of WorkOrderCompletedPayload list
    | MaterialsReceived of MaterialReceivedPayload list
    | ResourceDowntimes of ResourceDowntimePayload list
    | TransportDelays of TransportDelayPayload list

[<RequireQualifiedAccess>]
module IntegrationEventEnvelope =

    /// Helper to create a generic Envelope containing a serialized IntegrationEvent.
    let create
        (tenantId: string)
        (correlationId: Guid)
        (payload: IntegrationEvent)
        : Result<Envelope, SerializationError> =
        serialize payload
        |> Result.map (fun dataJson ->
            Envelope.createEnvelope "IntegrationEvent" dataJson 1
            |> Envelope.withTenantId tenantId
            |> Envelope.withCorrelationId (correlationId.ToString()))

    /// Helper to parse the typed IntegrationEvent out of a generic Envelope.
    let tryGetPayload (env: Envelope) : Result<IntegrationEvent, SerializationError> =
        Envelope.deserialize<IntegrationEvent> env
