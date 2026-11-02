/// CA-P-011 Learn & Calibrate Promising Models — Workflows
module Medhavi.Promise.LearnFromPromises.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Failure
open Medhavi.Contracts.Promise
open Medhavi.Promise

type LearningWorkflowDependencies =
    { Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchNotification: string -> obj -> CancellationToken -> Task<unit> }

type LearningWorkflows =
    { StartCalibration: CancellationToken -> TaskResult<IDisposable, ApplicationError> }

let create (deps: LearningWorkflowDependencies) : LearningWorkflows =
    let startCalibration (ct: CancellationToken) : TaskResult<IDisposable, ApplicationError> =
        task {
            let processEnvelope (env: Envelope) : Task<unit> =
                task {
                    let modelId =
                        match env.Metadata.TryGetValue("ModelId") with
                        | true, id -> id
                        | false, _ -> ""

                    if not (String.IsNullOrWhiteSpace modelId) then
                        // Dispatch BN-P-020 / BN-P-022 notifications when calibration events occur
                        ()
                    else ()
                }

            let filter =
                [ ArsIdentifiers.EnterpriseEvents.promiseCalibrationProposed.Id
                  ArsIdentifiers.EnterpriseEvents.promiseCalibrationApplied.Id ]
                |> EnvelopeFilter.EventTypes

            try
                let! sub = deps.Subscribe filter processEnvelope ct
                return Ok sub
            with ex ->
                return Error (Infrastructure(EventStore($"Failed to subscribe calibration workflow: {ex.Message}")))
        }

    { StartCalibration = startCalibration }
