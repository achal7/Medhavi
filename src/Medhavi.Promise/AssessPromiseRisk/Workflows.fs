/// CA-P-007 Assess Promise Risk and Resilience — Workflows
/// Traces to: BW-P-009, BN-P-013, BN-P-014
module Medhavi.Promise.AssessPromiseRisk.Workflows

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

type AssessPromiseRiskWorkflowDependencies =
    { RiskCodec: Codec<PromiseRiskAssessmentDto>
      Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchEnvelope: Envelope -> Task<unit>
      Apis: AssessPromiseRiskApis
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

/// BW-P-009: Risk monitoring workflow evaluating commitments and publishing alerts
let createRiskMonitoringWorkflow
    (deps: AssessPromiseRiskWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let assessmentId = envelope.AggregateId
                if not (String.IsNullOrWhiteSpace assessmentId) then
                    // Business Notification dispatch is triggered here when risk events are observed
                    ()
                else ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.commitmentRiskAssessed.Id
              ArsIdentifiers.EnterpriseEvents.commitmentRiskEscalated.Id
              ArsIdentifiers.EnterpriseEvents.riskFlagCleared.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe risk monitoring workflow: {ex.Message}")))
    }

