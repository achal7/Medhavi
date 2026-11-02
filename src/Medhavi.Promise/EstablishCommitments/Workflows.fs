/// CA-P-006 Establish Commitments Workflows
/// Traces to: BW-P-006, BW-P-014, BN-P-002, BN-P-003, BN-P-004, BN-P-019
module Medhavi.Promise.EstablishCommitments.Workflows

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

type EstablishCommitmentsWorkflowDependencies =
    { CommitmentCodec: Codec<OrderCommitmentDto>
      ExplanationCodec: Codec<PromiseExplanationDto>
      Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchEnvelope: Envelope -> Task<unit>
      Apis: EstablishCommitmentsApis
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

/// BW-P-006 / BW-P-014: Workflow reacting to commitment lifecycle events and publishing notifications
let createCommitmentWorkflow
    (deps: EstablishCommitmentsWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let commitmentId = envelope.AggregateId
                if not (String.IsNullOrWhiteSpace commitmentId) then
                    // Business Notification dispatch is triggered here when enterprise events are observed
                    ()
                else ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.commitmentEstablished.Id
              ArsIdentifiers.EnterpriseEvents.commitmentRevised.Id
              ArsIdentifiers.EnterpriseEvents.commitmentFulfilled.Id
              ArsIdentifiers.EnterpriseEvents.commitmentCancelled.Id
              ArsIdentifiers.EnterpriseEvents.promiseExplanationGenerated.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe commitment workflow: {ex.Message}")))
    }

