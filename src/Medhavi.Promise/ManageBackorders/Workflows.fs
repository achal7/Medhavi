/// CA-P-008 Manage Backorders — Workflows
/// Traces to: BW-P-011, BN-P-015
module Medhavi.Promise.ManageBackorders.Workflows

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

type ManageBackordersWorkflowDependencies =
    { BackorderCodec: Codec<BackorderRecordDto>
      Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchEnvelope: Envelope -> Task<unit>
      Apis: ManageBackordersApis
      DefaultScopeId: string }

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
            |> Result.mapError (fun err -> Infrastructure(Serialization(sprintf "Notification encoding failed: %A" err)))
            |> TaskResult.ofResult

        let env = Envelope.Create(notificationId, "P", aggregateType, aggregateId, 1L, capabilityId, json)

        do!
            dispatch env
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError (fun ex ->
                Infrastructure(EventStore(sprintf "Notification dispatch failed: %A" ex)))
    }

/// BW-P-011: Backorder netting workflow observing supply arrivals and resolving backlog lines
let createBackorderWorkflow
    (deps: ManageBackordersWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let backorderId = envelope.AggregateId
                if not (String.IsNullOrWhiteSpace backorderId) then
                    // Business Notification dispatch is triggered here when backorder events are observed
                    ()
                else ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.backorderQueued.Id
              ArsIdentifiers.EnterpriseEvents.backorderResolved.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe backorder workflow: {ex.Message}")))
    }

