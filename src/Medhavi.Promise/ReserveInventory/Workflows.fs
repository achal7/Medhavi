/// CA-P-003 Reserve Inventory and Capacity Temporarily Workflows
/// Traces to: BW-P-007, BW-P-008, BN-P-008, BN-P-009
module Medhavi.Promise.ReserveInventory.Workflows

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

type ReserveInventoryWorkflowDependencies =
    { ReservationCodec: Codec<TemporaryReservationDto>
      Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchEnvelope: Envelope -> Task<unit>
      Apis: ReserveInventoryApis
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

let createReservationWorkflow
    (deps: ReserveInventoryWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let token = envelope.AggregateId
                if not (String.IsNullOrWhiteSpace token) then
                    ()
                else ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.temporaryHoldCreated.Id
              ArsIdentifiers.EnterpriseEvents.temporaryHoldConfirmed.Id
              ArsIdentifiers.EnterpriseEvents.temporaryHoldReleased.Id
              ArsIdentifiers.EnterpriseEvents.temporaryHoldExpired.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe reservation workflow: {ex.Message}")))
    }

