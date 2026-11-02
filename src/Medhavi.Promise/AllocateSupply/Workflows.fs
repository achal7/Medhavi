/// CA-P-002 Allocate Supply and Manage Priority Workflows
/// Traces to: BW-P-004, BW-P-005, BN-P-005, BN-P-006, BN-P-007
module Medhavi.Promise.AllocateSupply.Workflows

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

type AllocateSupplyWorkflowDependencies =
    { PoolCodec: Codec<AllocationPoolDto>
      Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchEnvelope: Envelope -> Task<unit>
      Apis: AllocateSupplyApis
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

let createAllocationWorkflow
    (deps: AllocateSupplyWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let poolId = envelope.AggregateId
                if not (String.IsNullOrWhiteSpace poolId) then
                    ()
                else ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.allocationBucketInitialized.Id
              ArsIdentifiers.EnterpriseEvents.allocationQuotaConsumed.Id
              ArsIdentifiers.EnterpriseEvents.allocationBucketsRebalanced.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe allocation workflow: {ex.Message}")))
    }

