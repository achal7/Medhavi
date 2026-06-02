/// MRP Run Aggregate Root — State machine for tracking individual MRP run lifecycles
/// DDD Pattern: Event-sourced aggregate root using Decide/Evolve signatures
module Medhavi.Planning.Mrp.Aggregate.MrpRunAggregate

open System
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate
open Medhavi.Planning.Mrp.Domain.Types
open Medhavi.Planning.Mrp.Domain.Policies

// ============================================================================
// AGGREGATE STATE
// ============================================================================

type MrpRunningData =
    { StartTime: Timestamp
      Progress: int }

type MrpCompletedData =
    { StartTime: Timestamp
      EndTime: Timestamp
      Result: MrpRunResult }

type MrpFailedData =
    { StartTime: Timestamp
      EndTime: Timestamp
      Error: string }

type MrpRunState =
    | Pending
    | Running of MrpRunningData
    | Completed of MrpCompletedData
    | Failed of MrpFailedData

// ============================================================================
// COMMANDS & EVENTS
// ============================================================================

type StartMrpRunCmd =
    { RunId: string
      StartDate: Timestamp
      EndDate: Timestamp
      StockingPointId: StockingPointId
      Policy: MrpPolicy
      StartedAt: Timestamp }

type CompleteMrpRunCmd =
    { RunId: string
      Result: MrpRunResult
      CompletedAt: Timestamp }

type FailMrpRunCmd =
    { RunId: string
      Error: string
      FailedAt: Timestamp }

type UpdateMrpRunProgressCmd =
    { RunId: string
      Progress: int
      UpdatedAt: Timestamp }

type MrpRunCommand =
    | StartMrpRun of StartMrpRunCmd
    | CompleteMrpRun of CompleteMrpRunCmd
    | FailMrpRun of FailMrpRunCmd
    | UpdateMrpRunProgress of UpdateMrpRunProgressCmd

type MrpRunStartedEvt =
    { RunId: string
      StartDate: Timestamp
      EndDate: Timestamp
      StockingPointId: StockingPointId
      Policy: MrpPolicy
      StartedAt: Timestamp }

type MrpRunCompletedEvt =
    { RunId: string
      Result: MrpRunResult
      CompletedAt: Timestamp }

type MrpRunFailedEvt =
    { RunId: string
      Error: string
      FailedAt: Timestamp }

type MrpRunProgressUpdatedEvt =
    { RunId: string
      Progress: int
      UpdatedAt: Timestamp }

type MrpRunEvent =
    | MrpRunStarted of MrpRunStartedEvt
    | MrpRunCompleted of MrpRunCompletedEvt
    | MrpRunFailed of MrpRunFailedEvt
    | MrpRunProgressUpdated of MrpRunProgressUpdatedEvt

// ============================================================================
// SIGNATURE ALIGNMENT
// ============================================================================

type DecideMrpRun = Decide<MrpRunState, MrpRunCommand, MrpRunEvent>
type EvolveMrpRun = Evolve<MrpRunState, MrpRunEvent>

// ============================================================================
// DECIDE & EVOLVE IMPLEMENTATION
// ============================================================================

let decide: DecideMrpRun =
    fun cmd stateOpt ->
        match cmd, stateOpt with
        | StartMrpRun c, None ->
            let evt =
                MrpRunStarted
                    { RunId = c.RunId
                      StartDate = c.StartDate
                      EndDate = c.EndDate
                      StockingPointId = c.StockingPointId
                      Policy = c.Policy
                      StartedAt = c.StartedAt }
            let newState =
                Running
                    { StartTime = c.StartedAt
                      Progress = 0 }
            Ok { NewState = newState; Events = [ evt ] }

        | StartMrpRun _, Some _ ->
            Error (DomainError.conflict "MRP run has already been started.")

        | CompleteMrpRun c, Some (Running r) ->
            let evt =
                MrpRunCompleted
                    { RunId = c.RunId
                      Result = c.Result
                      CompletedAt = c.CompletedAt }
            let newState =
                Completed
                    { StartTime = r.StartTime
                      EndTime = c.CompletedAt
                      Result = c.Result }
            Ok { NewState = newState; Events = [ evt ] }

        | CompleteMrpRun _, Some _ ->
            Error (DomainError.invariant "Cannot complete an MRP run that is not currently running.")

        | CompleteMrpRun _, None ->
            Error (DomainError.notFound "MRP run not found.")

        | FailMrpRun c, Some (Running r) ->
            let evt =
                MrpRunFailed
                    { RunId = c.RunId
                      Error = c.Error
                      FailedAt = c.FailedAt }
            let newState =
                Failed
                    { StartTime = r.StartTime
                      EndTime = c.FailedAt
                      Error = c.Error }
            Ok { NewState = newState; Events = [ evt ] }

        | FailMrpRun _, Some _ ->
            Error (DomainError.invariant "Cannot fail an MRP run that is not currently running.")

        | FailMrpRun _, None ->
            Error (DomainError.notFound "MRP run not found.")

        | UpdateMrpRunProgress c, Some (Running r) ->
            if c.Progress < 0 || c.Progress > 100 then
                Error (DomainError.validation "Progress percentage must be between 0 and 100.")
            else
                let evt =
                    MrpRunProgressUpdated
                        { RunId = c.RunId
                          Progress = c.Progress
                          UpdatedAt = c.UpdatedAt }
                let newState =
                    Running
                        { r with Progress = c.Progress }
                Ok { NewState = newState; Events = [ evt ] }

        | UpdateMrpRunProgress _, Some _ ->
            Error (DomainError.invariant "Cannot update progress for a run that is not running.")

        | UpdateMrpRunProgress _, None ->
            Error (DomainError.notFound "MRP run not found.")

let evolve: EvolveMrpRun =
    fun event stateOpt ->
        match event, stateOpt with
        | MrpRunStarted e, None ->
            Some (Running { StartTime = e.StartedAt; Progress = 0 })

        | MrpRunCompleted e, Some (Running r) ->
            Some (Completed { StartTime = r.StartTime; EndTime = e.CompletedAt; Result = e.Result })

        | MrpRunFailed e, Some (Running r) ->
            Some (Failed { StartTime = r.StartTime; EndTime = e.FailedAt; Error = e.Error })

        | MrpRunProgressUpdated e, Some (Running r) ->
            Some (Running { r with Progress = e.Progress })

        | MrpRunStarted _, Some state ->
            Some state

        | _, current ->
            current
