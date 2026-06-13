namespace Medhavi.Scenario.Domain

open System
open Medhavi.SharedKernel
open Medhavi.SharedKernel.ScenarioContracts
open Medhavi.Scenario.Domain

type Scenario =
    { Id: ScenarioId
      Name: string
      Type: ScenarioType
      Version: Version
      ParentScenarioId: ScenarioId option
      ConfigurationId: ScenarioConfigurationId option
      ActivePlanRef: PlanRef option
      PreviousPlanRef: PlanRef option
      IsDirty: bool
      DirtyReason: DirtyReason option
      Status: ScenarioStatus
      StructuralChange: StructuralChange
      ActiveRunId: System.Guid option
      LastKnownInputVersions: InputVersionVector option
      LastPlanningMode: PlanningMode option }

type ScenarioCommand =
    | Create of ScenarioId * string * ScenarioType * parentId: ScenarioId option
    | Rename of string
    | MarkDirty
    | MarkDirtyWith of DirtyReason
    | MarkStructuralChange
    | ClearStructuralChange
    | SetPlanRef of PlanRef * Version
    | Configure of ScenarioConfigurationId
    | Branch of newId: ScenarioId * newName: string
    | StartPlanning of runId: System.Guid
    | StopPlanning
    | RequestPlanning
    | CompletePlanning of runId: System.Guid * planRef: PlanRef * inputVersions: InputVersionVector
    | FailPlanning of runId: System.Guid * reason: string
    | SubmitForApproval
    | Approve
    | Reject of reason: string
    | Archive of publishId: string option * rollbackId: string option

type ScenarioEvent =
    | ScenarioCreated of ScenarioId * string * ScenarioType * parentId: ScenarioId option
    | ScenarioRenamed of ScenarioId * string
    | ScenarioMarkedDirty of ScenarioId
    | ScenarioDirtyReasonSet of ScenarioId * DirtyReason
    | ScenarioStructuralChangeMarked of ScenarioId
    | ScenarioStructuralChangeCleared of ScenarioId
    | ScenarioPlanRefSet of ScenarioId * PlanRef
    | ScenarioConfigured of ScenarioId * ScenarioConfigurationId
    | ScenarioBranched of parentId: ScenarioId * childId: ScenarioId * childName: string
    | ScenarioPlanningStarted of ScenarioId * runId: System.Guid
    | ScenarioPlanningStopped of ScenarioId
    | ScenarioPlanningRequested of ScenarioId * PlanningMode
    | ScenarioPlanningCompleted of ScenarioId * runId: System.Guid * PlanRef * InputVersionVector
    | ScenarioPlanningFailed of ScenarioId * runId: System.Guid * string
    | ScenarioSubmittedForApproval of ScenarioId
    | ScenarioApproved of ScenarioId
    | ScenarioRejected of ScenarioId * string
    | ScenarioArchived of ScenarioId * publishId: string option * rollbackId: string option

module ScenarioAgg =
    let private errNotFound msg = Error(DomainError.notFound msg)
    let private errConflict msg = Error(DomainError.conflict msg)
    let private errInvariant msg = Error(DomainError.invariant msg)

    let private errVersionMismatch (expected: Version) (actual: Version) =
        Error(
            DomainError.invariant $"Version mismatch: expected {Version.value expected}, actual {Version.value actual}"
        )

    let private isImmutable status =
        match status with
        | Archived
        | Published _ -> true
        | _ -> false

    let private initialScenario id name scenarioType parentId : Scenario =
        { Id = id
          Name = name
          Type = scenarioType
          Version = Version.initial
          ParentScenarioId = parentId
          ConfigurationId = None
          ActivePlanRef = None
          PreviousPlanRef = None
          IsDirty = false
          DirtyReason = None
          Status = Draft
          StructuralChange = Unchanged
          ActiveRunId = None
          LastKnownInputVersions = None
          LastPlanningMode = None }

    let handle: Decide<Scenario, ScenarioCommand, ScenarioEvent> =
        fun command stateOpt ->
            match command, stateOpt with
            | Create(id, name, scenarioType, parentId), None ->
                let s = initialScenario id name scenarioType parentId

                Ok
                    { NewState = s
                      Events = [ ScenarioCreated(id, name, scenarioType, parentId) ] }

            | Create _, Some _ -> errConflict "Scenario already exists"

            | Rename newName, Some state when not (isImmutable state.Status) ->
                let updated = { state with Name = newName }

                Ok
                    { NewState = updated
                      Events = [ ScenarioRenamed(state.Id, newName) ] }

            | Rename _, Some _ -> errInvariant "Cannot rename an archived or published scenario"

            | MarkDirty, Some state when not (isImmutable state.Status) ->
                let updated =
                    { state with
                        Version = Version.increment state.Version
                        IsDirty = true }

                Ok
                    { NewState = updated
                      Events = [ ScenarioMarkedDirty state.Id ] }

            | MarkDirtyWith reason, Some state when not (isImmutable state.Status) ->
                let updated =
                    { state with
                        Version = Version.increment state.Version
                        IsDirty = true
                        DirtyReason = Some reason }

                Ok
                    { NewState = updated
                      Events = [ ScenarioDirtyReasonSet(state.Id, reason) ] }

            | (MarkDirty | MarkDirtyWith _), Some _ -> errInvariant "Cannot mark dirty an archived or published scenario"

            | MarkStructuralChange, Some state when not (isImmutable state.Status) ->
                let updated =
                    { state with
                        StructuralChange = FullReplanRequired
                        IsDirty = true
                        Version = Version.increment state.Version }

                Ok
                    { NewState = updated
                      Events = [ ScenarioStructuralChangeMarked state.Id ] }

            | MarkStructuralChange, Some _ -> errInvariant "Cannot mark structural change on an archived or published scenario"

            | ClearStructuralChange, Some state ->
                let updated =
                    { state with
                        StructuralChange = Unchanged }

                Ok
                    { NewState = updated
                      Events = [ ScenarioStructuralChangeCleared state.Id ] }

            | Configure configId, Some state when not (isImmutable state.Status) ->
                let updated =
                    { state with
                        ConfigurationId = Some configId
                        Version = Version.increment state.Version }

                Ok
                    { NewState = updated
                      Events = [ ScenarioConfigured(state.Id, configId) ] }

            | Configure _, Some _ -> errInvariant "Cannot configure an archived scenario"

            | SetPlanRef(planRef, expectedVersion), Some state ->
                if not (Version.equals state.Version expectedVersion) then
                    errVersionMismatch expectedVersion state.Version
                else
                    let updated =
                        { state with
                            PreviousPlanRef = state.ActivePlanRef
                            ActivePlanRef = Some planRef
                            IsDirty = false
                            StructuralChange = Unchanged }

                    Ok
                        { NewState = updated
                          Events = [ ScenarioPlanRefSet(state.Id, planRef) ] }

            | StartPlanning runId, Some state ->
                match state.Status with
                | PlanningRunning -> errConflict "Planning is already running"
                | Archived | Published _ -> errInvariant "Cannot start planning on an archived or published scenario"
                | _ ->
                    let mode =
                        ScenarioPolicy.determinePlanningMode state.StructuralChange state.DirtyReason

                    let updated =
                        { state with
                            Status = PlanningRunning
                            ActiveRunId = Some runId
                            LastPlanningMode = Some mode }

                    Ok
                        { NewState = updated
                          Events = [ ScenarioPlanningStarted(state.Id, runId) ] }

            | StopPlanning, Some state ->
                match state.Status with
                | PlanningRunning ->
                    let updated = { state with Status = PlanningPaused }

                    Ok
                        { NewState = updated
                          Events = [ ScenarioPlanningStopped state.Id ] }
                | _ -> errConflict "Planning is not currently running"

            | RequestPlanning, Some state ->
                match state.Status with
                | PlanningRunning -> errConflict "Planning is already running; stop it first"
                | Archived | Published _ -> errInvariant "Cannot request planning on an archived or published scenario"
                | _ ->
                    let mode =
                        ScenarioPolicy.determinePlanningMode state.StructuralChange state.DirtyReason

                    let updated =
                        { state with
                            Version = Version.increment state.Version
                            LastPlanningMode = Some mode }

                    Ok
                        { NewState = updated
                          Events = [ ScenarioPlanningRequested(state.Id, mode) ] }

            | CompletePlanning(runId, planRef, inputVersions), Some state ->
                match state.Status with
                | PlanningRunning ->
                    match state.ActiveRunId with
                    | Some activeId when activeId <> runId ->
                        errInvariant $"RunId mismatch: expected {activeId}, got {runId}"
                    | _ ->
                        let updated =
                            { state with
                                Status = PlanningComplete
                                PreviousPlanRef = state.ActivePlanRef
                                ActivePlanRef = Some planRef
                                ActiveRunId = None
                                LastKnownInputVersions = Some inputVersions
                                IsDirty = false
                                DirtyReason = None
                                StructuralChange = Unchanged }

                        Ok
                            { NewState = updated
                              Events = [ ScenarioPlanningCompleted(state.Id, runId, planRef, inputVersions) ] }
                | _ -> errConflict "Planning is not currently running"

            | FailPlanning(runId, reason), Some state ->
                match state.Status with
                | PlanningRunning ->
                    match state.ActiveRunId with
                    | Some activeId when activeId <> runId ->
                        errInvariant $"RunId mismatch: expected {activeId}, got {runId}"
                    | _ ->
                        let updated =
                            { state with
                                Status = PlanningFailed
                                ActiveRunId = None }

                        Ok
                            { NewState = updated
                              Events = [ ScenarioPlanningFailed(state.Id, runId, reason) ] }
                | _ -> errConflict "Planning is not currently running"

            | SubmitForApproval, Some state ->
                match state.Type, state.Status, state.ActivePlanRef with
                | Sandbox, _, _ -> errInvariant "Sandbox scenarios cannot be submitted for approval"
                | WhatIf, PlanningComplete, Some _ ->
                    let updated = { state with Status = UnderReview }

                    Ok
                        { NewState = updated
                          Events = [ ScenarioSubmittedForApproval state.Id ] }
                | WhatIf, PlanningComplete, None ->
                    errInvariant "WhatIf scenario must have a completed plan before submission"
                | Baseline, (Ready | PlanningComplete), _ ->
                    let updated = { state with Status = UnderReview }

                    Ok
                        { NewState = updated
                          Events = [ ScenarioSubmittedForApproval state.Id ] }
                | _, s, _ -> errInvariant (sprintf "Cannot submit for approval from status %A" s)

            | Approve, Some state ->
                match state.Status with
                | UnderReview ->
                    let updated = { state with Status = Approved }

                    Ok
                        { NewState = updated
                          Events = [ ScenarioApproved state.Id ] }
                | s -> errInvariant (sprintf "Cannot approve from status %A" s)

            | Reject reason, Some state ->
                match state.Status with
                | UnderReview ->
                    let updated = { state with Status = Ready }

                    Ok
                        { NewState = updated
                          Events = [ ScenarioRejected(state.Id, reason) ] }
                | s -> errInvariant (sprintf "Cannot reject from status %A" s)

            | Archive(publishId, rollbackId), Some state ->
                if isImmutable state.Status then
                    errInvariant "Scenario is already archived or published"
                elif state.Status = PlanningRunning then
                    errInvariant "Cannot archive a scenario while planning is running"
                else
                    let newStatus =
                        match publishId, rollbackId with
                        | Some pId, Some rId -> Published(DateTimeOffset.UtcNow, pId, rId)
                        | _ -> Archived
                    let updated =
                        { state with
                            Status = newStatus }

                    Ok
                        { NewState = updated
                          Events = [ ScenarioArchived(state.Id, publishId, rollbackId) ] }

            | _, None -> errNotFound "Scenario not found"
            | _, _ -> errInvariant "Invalid command / state combination"

    let evolve (event: ScenarioEvent) (state: Scenario option) : Scenario option =
        match event, state with
        | ScenarioCreated(id, name, scenarioType, parentId), None -> Some(initialScenario id name scenarioType parentId)

        | ScenarioRenamed(_, name), Some s ->
            Some
                { s with
                    Name = name
                    Version = Version.increment s.Version }

        | ScenarioMarkedDirty _, Some s ->
            Some
                { s with
                    IsDirty = true
                    Version = Version.increment s.Version }

        | ScenarioDirtyReasonSet(_, reason), Some s ->
            Some
                { s with
                    IsDirty = true
                    DirtyReason = Some reason
                    Version = Version.increment s.Version }

        | ScenarioStructuralChangeMarked _, Some s ->
            Some
                { s with
                    StructuralChange = FullReplanRequired
                    IsDirty = true
                    Version = Version.increment s.Version }

        | ScenarioStructuralChangeCleared _, Some s ->
            Some
                { s with
                    StructuralChange = Unchanged
                    Version = Version.increment s.Version }

        | ScenarioConfigured(_, configId), Some s ->
            Some
                { s with
                    ConfigurationId = Some configId
                    Version = Version.increment s.Version }

        | ScenarioPlanRefSet(_, planRef), Some s ->
            Some
                { s with
                    PreviousPlanRef = s.ActivePlanRef
                    ActivePlanRef = Some planRef
                    IsDirty = false
                    StructuralChange = Unchanged
                    Version = Version.increment s.Version }

        | ScenarioBranched _, Some s -> Some s

        | ScenarioPlanningStarted(_, runId), Some s ->
            Some
                { s with
                    Status = PlanningRunning
                    ActiveRunId = Some runId
                    Version = Version.increment s.Version }

        | ScenarioPlanningStopped _, Some s ->
            Some
                { s with
                    Status = PlanningPaused
                    Version = Version.increment s.Version }

        | ScenarioPlanningRequested(_, mode), Some s ->
            Some
                { s with
                    LastPlanningMode = Some mode
                    Version = Version.increment s.Version }

        | ScenarioPlanningCompleted(_, _, planRef, inputVersions), Some s ->
            Some
                { s with
                    Status = PlanningComplete
                    PreviousPlanRef = s.ActivePlanRef
                    ActivePlanRef = Some planRef
                    ActiveRunId = None
                    LastKnownInputVersions = Some inputVersions
                    IsDirty = false
                    DirtyReason = None
                    StructuralChange = Unchanged
                    Version = Version.increment s.Version }

        | ScenarioPlanningFailed(_, _, _), Some s ->
            Some
                { s with
                    Status = PlanningFailed
                    ActiveRunId = None
                    Version = Version.increment s.Version }

        | ScenarioSubmittedForApproval _, Some s ->
            Some
                { s with
                    Status = UnderReview
                    Version = Version.increment s.Version }

        | ScenarioApproved _, Some s ->
            Some
                { s with
                    Status = Approved
                    Version = Version.increment s.Version }

        | ScenarioRejected _, Some s ->
            Some
                { s with
                    Status = Ready
                    Version = Version.increment s.Version }

        | ScenarioArchived(_, publishId, rollbackId), Some s ->
            let newStatus =
                match publishId, rollbackId with
                | Some pId, Some rId -> Published(DateTimeOffset.UtcNow, pId, rId)
                | _ -> Archived
            Some
                { s with
                    Status = newStatus
                    Version = Version.increment s.Version }

        | _, _ -> state
