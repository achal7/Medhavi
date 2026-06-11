namespace Medhavi.Scheduler.Planning.Domain

open System
open System.Text.Json.Serialization
open Medhavi.SharedKernel
open Medhavi.SharedKernel.ScenarioContracts

[<JsonFSharpConverter>]
type RunStatus =
    | Created
    | Running
    | Completed
    | Failed
    | Cancelled

type PlanningRun =
    {
        RunId: PlanningRunId
        SnapshotId: SnapshotId
        ScenarioId: ScenarioId
        /// Unified PlanningMode from SharedKernel — carries scope data for ReactiveRepair.
        Mode: PlanningMode
        Status: RunStatus
        /// Storage key in PostgreSQL pointing to PlanVersionData bulk output.
        ResultStorageKey: string option
        StartedAt: DateTimeOffset option
        CompletedAt: DateTimeOffset option
        /// Which phase the run is currently executing — for telemetry and resume.
        CurrentPhase: PlanningRunPhase option
        /// Recorded duration per phase for performance dashboards.
        PhaseTimings: Map<string, TimeSpan>
        /// Which phase failed (if any) — for targeted resume.
        FailedPhase: PlanningRunPhase option
    }

type PlanningRunCommand =
    | CreateRun of scenarioId: ScenarioId * snapshotId: SnapshotId * mode: PlanningMode
    | StartRun
    | AdvancePhase of phase: PlanningRunPhase
    | CompletePhase of phase: PlanningRunPhase * duration: TimeSpan
    | MarkRunCompleted of resultStorageKey: string
    | FailRun of reason: string
    | FailInPhase of phase: PlanningRunPhase * reason: string
    | CancelRun

type PlanningRunEvent =
    | RunCreated of PlanningRunId * ScenarioId * SnapshotId * PlanningMode
    | RunStarted of PlanningRunId * startedAt: DateTimeOffset
    | PhaseStarted of PlanningRunId * PlanningRunPhase
    | PhaseCompleted of PlanningRunId * PlanningRunPhase * duration: TimeSpan
    | PhaseFailed of PlanningRunId * PlanningRunPhase * reason: string
    | RunCompleted of PlanningRunId * resultStorageKey: string * completedAt: DateTimeOffset
    | RunFailed of PlanningRunId * reason: string * at: DateTimeOffset
    | RunCancelled of PlanningRunId * at: DateTimeOffset

module PlanningRunAgg =
    let private errConflict msg = Error (DomainError.conflict msg)
    let private errNotFound msg = Error (DomainError.notFound msg)
    let private errInvariant msg = Error (DomainError.invariant msg)

    let handle: Decide<PlanningRun, PlanningRunCommand, PlanningRunEvent> =
        fun cmd stateOpt ->
            match cmd, stateOpt with

            | CreateRun(scenarioId, snapshotId, mode), None ->
                let runId = Guid.NewGuid()
                let state =
                    { RunId = runId
                      ScenarioId = scenarioId
                      SnapshotId = snapshotId
                      Mode = mode
                      Status = Created
                      ResultStorageKey = None
                      StartedAt = None
                      CompletedAt = None
                      CurrentPhase = None
                      PhaseTimings = Map.empty
                      FailedPhase = None }

                Ok { NewState = state; Events = [ RunCreated(runId, scenarioId, snapshotId, mode) ] }

            | CreateRun _, Some _ -> errConflict "PlanningRun already exists"

            | StartRun, Some state when state.Status = Created ->
                let now = DateTimeOffset.UtcNow
                let updated =
                    { state with
                        Status = Running
                        StartedAt = Some now }
                Ok { NewState = updated; Events = [ RunStarted(state.RunId, now) ] }

            | StartRun, Some _ -> errInvariant "Can only start a run in Created status"
            | StartRun, None -> errNotFound "PlanningRun not found"

            | AdvancePhase phase, Some state when state.Status = Running ->
                let updated = { state with CurrentPhase = Some phase }
                Ok { NewState = updated; Events = [ PhaseStarted(state.RunId, phase) ] }

            | AdvancePhase _, Some _ -> errInvariant "Can only advance phase while Running"
            | AdvancePhase _, None -> errNotFound "PlanningRun not found"

            | CompletePhase(phase, duration), Some state when state.Status = Running ->
                let phaseKey = sprintf "%A" phase
                let updated =
                    { state with
                        PhaseTimings = Map.add phaseKey duration state.PhaseTimings }
                Ok { NewState = updated; Events = [ PhaseCompleted(state.RunId, phase, duration) ] }

            | CompletePhase _, Some _ -> errInvariant "Can only complete phase while Running"
            | CompletePhase _, None -> errNotFound "PlanningRun not found"

            | MarkRunCompleted storageKey, Some state when state.Status = Running ->
                match String.IsNullOrEmpty storageKey with
                | true -> errInvariant "Storage key cannot be empty"
                | false ->
                    let now = DateTimeOffset.UtcNow
                    let updated =
                        { state with
                            Status = Completed
                            ResultStorageKey = Some storageKey
                            CompletedAt = Some now
                            CurrentPhase = None }
                    Ok { NewState = updated; Events = [ RunCompleted(state.RunId, storageKey, now) ] }

            | MarkRunCompleted _, Some _ -> errInvariant "Can only complete a running run"
            | MarkRunCompleted _, None -> errNotFound "PlanningRun not found"

            | FailRun reason, Some state when state.Status = Running ->
                let now = DateTimeOffset.UtcNow
                let updated =
                    { state with
                        Status = Failed
                        CompletedAt = Some now }
                Ok { NewState = updated; Events = [ RunFailed(state.RunId, reason, now) ] }

            | FailInPhase(phase, reason), Some state when state.Status = Running ->
                let now = DateTimeOffset.UtcNow
                let updated =
                    { state with
                        Status = Failed
                        FailedPhase = Some phase
                        CompletedAt = Some now }
                Ok { NewState = updated; Events = [ PhaseFailed(state.RunId, phase, reason); RunFailed(state.RunId, reason, now) ] }

            | (FailRun _ | FailInPhase _), Some _ -> errInvariant "Can only fail a running run"
            | (FailRun _ | FailInPhase _), None -> errNotFound "PlanningRun not found"

            | CancelRun, Some state when state.Status = Created || state.Status = Running ->
                let now = DateTimeOffset.UtcNow
                let updated =
                    { state with
                        Status = Cancelled
                        CompletedAt = Some now }
                Ok { NewState = updated; Events = [ RunCancelled(state.RunId, now) ] }

            | CancelRun, Some _ -> errInvariant "Cannot cancel a completed or already-cancelled run"
            | CancelRun, None -> errNotFound "PlanningRun not found"

    let evolve (event: PlanningRunEvent) (stateOpt: PlanningRun option) : PlanningRun option =
        match event, stateOpt with
        | RunCreated(runId, scenarioId, snapshotId, mode), None ->
            Some
                { RunId = runId
                  SnapshotId = snapshotId
                  ScenarioId = scenarioId
                  Mode = mode
                  Status = Created
                  ResultStorageKey = None
                  StartedAt = None
                  CompletedAt = None
                  CurrentPhase = None
                  PhaseTimings = Map.empty
                  FailedPhase = None }

        | RunStarted(_, startedAt), Some s ->
            Some
                { s with
                    Status = Running
                    StartedAt = Some startedAt }

        | PhaseStarted(_, phase), Some s -> Some { s with CurrentPhase = Some phase }

        | PhaseCompleted(_, phase, duration), Some s ->
            let key = sprintf "%A" phase
            Some
                { s with
                    PhaseTimings = Map.add key duration s.PhaseTimings }

        | PhaseFailed(_, phase, _), Some s -> Some { s with FailedPhase = Some phase }

        | RunCompleted(_, storageKey, completedAt), Some s ->
            Some
                { s with
                    Status = Completed
                    ResultStorageKey = Some storageKey
                    CompletedAt = Some completedAt
                    CurrentPhase = None }

        | RunFailed(_, _, at), Some s ->
            Some
                { s with
                    Status = Failed
                    CompletedAt = Some at }

        | RunCancelled(_, at), Some s ->
            Some
                { s with
                    Status = Cancelled
                    CompletedAt = Some at }

        | _, _ -> stateOpt
