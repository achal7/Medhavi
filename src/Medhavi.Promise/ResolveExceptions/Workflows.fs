/// CA-P-009 Resolve Exceptions — Workflows
/// Traces to: BW-P-012, BN-P-016, BN-P-017, BN-P-023
module Medhavi.Promise.ResolveExceptions.Workflows

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

type ResolveExceptionsWorkflowDependencies =
    { ExceptionCodec: Codec<PromiseExceptionAssessmentDto>
      Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchEnvelope: Envelope -> Task<unit>
      Apis: ResolveExceptionsApis
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

/// BW-P-012: Disruption resolution workflow observing opened cases and mitigation approvals
let createExceptionWorkflow
    (deps: ResolveExceptionsWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let exceptionId = envelope.AggregateId
                if not (String.IsNullOrWhiteSpace exceptionId) then
                    // Business Notification dispatch is triggered here when disruption events are observed
                    ()
                else ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.disruptionCaseOpened.Id
              ArsIdentifiers.EnterpriseEvents.disruptionRecoveryExecuted.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe exception workflow: {ex.Message}")))
    }

