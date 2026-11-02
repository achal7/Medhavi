/// CA-P-004 Evaluate Multi-Option ATP Workflows
/// Traces to: BW-P-010, BN-P-010
module Medhavi.Promise.EvaluateATP.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers

type EvaluateATPWorkflowDependencies =
    { AssessmentCodec: Codec<ATPFeasibilityAssessmentDto>
      Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchEnvelope: Envelope -> Task<unit>
      DefaultScopeId: string }

let createATPWorkflow
    (deps: EvaluateATPWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (_: Envelope) : Task<unit> =
            Task.FromResult ()

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.promiseOptionsGenerated.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe ATP workflow: {ex.Message}")))
    }

