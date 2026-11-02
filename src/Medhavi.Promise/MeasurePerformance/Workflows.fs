/// CA-P-010 Measure Promise Performance & Service Quality — Workflows
module Medhavi.Promise.MeasurePerformance.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Failure
open Medhavi.Contracts.Promise
open Medhavi.Promise

type QualityWorkflowDependencies =
    {
        Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
        DispatchNotification: string -> obj -> CancellationToken -> Task<unit>
    }

type QualityWorkflows =
    {
        StartAuditing: CancellationToken -> TaskResult<IDisposable, ApplicationError>
    }

let create (deps: QualityWorkflowDependencies) : QualityWorkflows =
    let startAuditing (ct: CancellationToken) : TaskResult<IDisposable, ApplicationError> =
        task {
            let processEnvelope (env: Envelope) : Task<unit> =
                task {
                    let assessmentId =
                        match env.Metadata.TryGetValue("AssessmentId") with
                        | true, id -> id
                        | false, _ -> ""

                    if not(String.IsNullOrWhiteSpace assessmentId) then
                        // Dispatch BN-P-018 notification when SLA penalty exists
                        ()
                    else
                        ()
                }

            let filter =
                [
                    ArsIdentifiers.EnterpriseEvents.commitmentQualityAudited.Id
                    ArsIdentifiers.EnterpriseEvents.commitmentDisputeRecorded.Id
                ]
                |> EnvelopeFilter.EventTypes

            try
                let! sub = deps.Subscribe filter processEnvelope ct
                return Ok sub
            with ex ->
                return Error(Infrastructure(EventStore($"Failed to subscribe quality audit workflow: {ex.Message}")))
        }

    { StartAuditing = startAuditing }
