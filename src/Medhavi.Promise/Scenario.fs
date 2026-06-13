module Medhavi.Promise.Scenario

open Medhavi.SharedKernel
open Medhavi.Contracts.Promise

type ScenarioStatus =
    | Active
    | Expired
    | Finalized

type ScenarioVersion = private ScenarioVersion of int

module ScenarioVersion =
    let initial = ScenarioVersion 1
    let increment (ScenarioVersion v) = ScenarioVersion(v + 1)
    let value (ScenarioVersion v) = v

type Scenario =
    { Id: string
      Version: ScenarioVersion
      OrderId: OrderId
      Status: ScenarioStatus
      PlanSnapshot: PromiseDateRange option
      CreatedAt: Timestamp
      ModifiedAt: Timestamp
      DirtySteps: string list }

type CreateScenarioCmd =
    { Id: string
      OrderId: OrderId }

type SetSnapshotCmd =
    { Id: string
      Snapshot: PromiseDateRange }

type MarkDirtyCmd =
    { Id: string
      StepId: string }

type MarkCleanCmd =
    { Id: string
      StepIds: string list }

type FinalizeScenarioCmd =
    { Id: string }

type ExpireScenarioCmd =
    { Id: string }

type ScenarioCommand =
    | Create of CreateScenarioCmd
    | SetSnapshot of SetSnapshotCmd
    | MarkDirty of MarkDirtyCmd
    | MarkClean of MarkCleanCmd
    | Finalize of FinalizeScenarioCmd
    | Expire of ExpireScenarioCmd

type ScenarioCreatedEvt = Scenario

type SnapshotAddedEvt =
    { ScenarioId: string
      Snapshot: PromiseDateRange option }

type StepMarkedDirtyEvt =
    { ScenarioId: string
      StepId: string }

type StepsMarkedCleanEvt =
    { ScenarioId: string
      StepIds: string list }

type ScenarioFinalizedEvt =
    { ScenarioId: string }

type ScenarioExpiredEvt =
    { ScenarioId: string }

type ScenarioEvent =
    | ScenarioCreated of ScenarioCreatedEvt
    | SnapshotAdded of SnapshotAddedEvt
    | StepMarkedDirty of StepMarkedDirtyEvt
    | StepsMarkedClean of StepsMarkedCleanEvt
    | ScenarioFinalized of ScenarioFinalizedEvt
    | ScenarioExpired of ScenarioExpiredEvt

type DecideScenario = Decide<Scenario, ScenarioCommand, ScenarioEvent>
type EvolveScenario = Evolve<Scenario, ScenarioEvent>

let private applySnapshot (snapshot: PromiseDateRange) (state: Scenario) : Scenario =
    { state with PlanSnapshot = Some snapshot; ModifiedAt = Timestamp.now }

let private markDirty (stepId: string) (state: Scenario) : Scenario =
    let dirtySteps =
        if state.DirtySteps |> List.contains stepId then state.DirtySteps
        else stepId :: state.DirtySteps
    { state with DirtySteps = dirtySteps; ModifiedAt = Timestamp.now }

let private markCleanLocal (stepIds: string list) (state: Scenario) : Scenario =
    let dirtySteps = state.DirtySteps |> List.filter (fun s -> not (List.contains s stepIds))
    { state with DirtySteps = dirtySteps; ModifiedAt = Timestamp.now }

let private incrementVersion (state: Scenario) : Scenario =
    { state with Version = ScenarioVersion.increment state.Version; ModifiedAt = Timestamp.now }

let decide: DecideScenario =
    fun command stateOpt ->
        match command, stateOpt with
        | Create cmd, None ->
            if System.String.IsNullOrWhiteSpace(cmd.Id) then
                Error(DomainError.validation "Scenario Id cannot be empty")
            else
                let scenario =
                    { Id = cmd.Id
                      Version = ScenarioVersion.initial
                      OrderId = cmd.OrderId
                      Status = ScenarioStatus.Active
                      PlanSnapshot = None
                      CreatedAt = Timestamp.now
                      ModifiedAt = Timestamp.minValue
                      DirtySteps = [] }

                Ok { NewState = scenario
                     Events = [ ScenarioCreated scenario ] }

        | SetSnapshot cmd, Some state ->
            if state.Status = ScenarioStatus.Finalized then
                Error(DomainError.validation "Cannot modify a finalized scenario")
            else
                let updated = state |> applySnapshot cmd.Snapshot |> incrementVersion
                Ok { NewState = updated
                     Events = [ SnapshotAdded { ScenarioId = cmd.Id; Snapshot = Some cmd.Snapshot } ] }

        | MarkDirty cmd, Some state ->
            let updated = state |> markDirty cmd.StepId
            Ok { NewState = updated
                 Events = [ StepMarkedDirty { ScenarioId = cmd.Id; StepId = cmd.StepId } ] }

        | MarkClean cmd, Some state ->
            let updated = state |> markCleanLocal cmd.StepIds
            Ok { NewState = updated
                 Events = [ StepsMarkedClean { ScenarioId = cmd.Id; StepIds = cmd.StepIds } ] }

        | Finalize cmd, Some state ->
            if state.Status = ScenarioStatus.Finalized then
                Error(DomainError.validation "Scenario already finalized")
            else
                let updated = { state with Status = ScenarioStatus.Finalized; ModifiedAt = Timestamp.now }
                Ok { NewState = updated
                     Events = [ ScenarioFinalized { ScenarioId = cmd.Id } ] }

        | Expire cmd, Some state ->
            if state.Status = ScenarioStatus.Expired then
                Error(DomainError.validation "Scenario already expired")
            else
                let updated = { state with Status = ScenarioStatus.Expired; ModifiedAt = Timestamp.now }
                Ok { NewState = updated
                     Events = [ ScenarioExpired { ScenarioId = cmd.Id } ] }

        | Create _, Some _ ->
            Error(DomainError.validation "Scenario already exists")

        | SetSnapshot _, None
        | MarkDirty _, None
        | MarkClean _, None
        | Finalize _, None
        | Expire _, None ->
            Error(DomainError.validation "Scenario not found")

let evolve: EvolveScenario =
    fun event stateOpt ->
        match event, stateOpt with
        | ScenarioCreated e, None -> Some e
        | SnapshotAdded _, Some state -> Some(incrementVersion state)
        | StepMarkedDirty e, Some state -> Some(markDirty e.StepId state)
        | StepsMarkedClean e, Some state -> Some(markCleanLocal e.StepIds state)
        | ScenarioFinalized _, Some state -> Some { state with Status = ScenarioStatus.Finalized; ModifiedAt = Timestamp.now }
        | ScenarioExpired _, Some state -> Some { state with Status = ScenarioStatus.Expired; ModifiedAt = Timestamp.now }
        | _, _ -> stateOpt
