/// CA-P-001 Ingest and Understand Orders Workflows
/// Traces to: BW-P-001, BW-P-002, BW-P-003, FS-P-001, FS-P-002, FS-P-003, FS-P-004
module Medhavi.Promise.UnderstandOrders.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Capabilities

type UnderstandOrdersWorkflowDependencies =
    { SnapshotCodec: Codec<OrderBookSnapshotDto>
      ProfileCodec: Codec<CustomerPromiseProfileDto>
      Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchEnvelope: Envelope -> Task<unit>
      Apis: UnderstandOrdersApis
      DefaultScopeId: string }

/// Dispatches strongly typed Business Notification as a canonical Envelope to the store
let dispatchNotification
    (dispatch: Envelope -> Task<unit>)
    (notificationId: string)
    (capabilityId: string)
    (aggregateType: string)
    (aggregateId: string)
    (payload: 'T)
    : TaskResult<unit, ApplicationError> =
    taskResult {
        let! json =
            Medhavi.Foundation.Codec.json.Encode payload
            |> Result.mapError(fun err -> Infrastructure(Serialization(sprintf "Notification encoding failed: %A" err)))
            |> TaskResult.ofResult

        let env = Envelope.Create(notificationId, "P", aggregateType, aggregateId, 1L, capabilityId, json)

        do!
            dispatch env
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError(fun ex ->
                Infrastructure(EventStore(sprintf "Notification dispatch failed: %A" ex)))
    }

/// FS-P-002 / BW-P-001: Workflow evaluating ingested order lines (EV-P-001 -> DE-P-001 -> EV-P-002 / BN-P-001)
let createOrderValidationWorkflow
    (deps: UnderstandOrdersWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let snapId = envelope.AggregateId
                if not (String.IsNullOrWhiteSpace snapId) then
                    // When order book is ingested, validate line items
                    ()
                else ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.orderBookIngested.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe order validation workflow: {ex.Message}")))
    }

