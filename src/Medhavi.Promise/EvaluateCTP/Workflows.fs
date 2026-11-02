/// CA-P-005 Evaluate Capable-to-Promise Workflows
/// Traces to: BW-P-013, BN-P-011
module Medhavi.Promise.EvaluateCTP.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers

type EvaluateCTPWorkflowDependencies =
    { AssessmentCodec: Codec<CTPFeasibilityAssessmentDto>
      Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchEnvelope: Envelope -> Task<unit>
      DefaultScopeId: string }

let createCTPWorkflow
    (deps: EvaluateCTPWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (_: Envelope) : Task<unit> =
            Task.FromResult ()

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.ctpProductionSlotEvaluated.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe CTP workflow: {ex.Message}")))
    }

